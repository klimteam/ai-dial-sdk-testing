using System.ClientModel;
using System.Text;
using System.Text.Json;
using AiDialSdk.Api.Chat;
using AiDialSdk.Api.Data;
using AiDialSdk.Api.Files;
using AiDialSdk.Api.Files.Extensions;
using AiDialSdk.Api.Infrastructure;
using AiDialSdk.Api.OpenAi.Data;
using AiDialSdk.Testing.Core;
using AiDialSdk.Testing.Extensions;
using AiDialSdk.Testing.TestStrategies.Models;
using Azure.AI.OpenAI;
using Microsoft.Extensions.AI;

namespace AiDialSdk.Testing.TestStrategies.Implementations;

public class LlmAgentTestStrategy : BaseTestStrategy<LlmAgentTestStrategyExecutionContext>, ITestStrategy
{
    private readonly string _prompt;
    private readonly Uri _uri;
    private readonly string _modelName;
    private readonly string _apiKey;
    private readonly int _maxIterations;
    private readonly IReadOnlySet<string> _mimeTypesToPropagate;

    private readonly IReadOnlyList<ChatAction<LlmAgentTestStrategyExecutionContext>> _chatActions;
    
    public LlmAgentTestStrategy(
        string prompt,
        string? firstMessage,
        Uri uri, 
        string modelName, 
        string apiKey, 
        int maxIterations, 
        IReadOnlyList<string> mimeTypesToPropagate,
        IReadOnlyList<CompletionCondition> completionConditions,
        IReadOnlyList<ChatAction<LlmAgentTestStrategyExecutionContext>> chatActions)
        : base(completionConditions)
    {
        _prompt = prompt;
        _uri = uri;
        _modelName = modelName;
        _apiKey = apiKey;
        _maxIterations = maxIterations;
        _mimeTypesToPropagate = mimeTypesToPropagate.ToHashSet();

        _chatActions = chatActions;
    }
    
    public async Task<TestResult> RunAsync(IDialChatApiClient chatClient, IDialFileApiClient fileClient, CancellationToken token)
    {
        var testExecutionContext = new LlmAgentTestStrategyExecutionContext(_prompt, chatClient, fileClient);
        var testAgentChatClient = GetChatClient();

        var chatOptions = new ChatOptions
        {
            Tools = new List<AITool>
            {
                AIFunctionFactory.Create(
                    (string message) => true,
                    Constants.SendNextMessageToChatToolName, "Use this function to send a next message to the chat of test application."),
                AIFunctionFactory.Create(
                    () => true,
                    Constants.MarkTestAsSuccessfulToolName,
                    "Call this function to mark the test as finished an successful. Once this function is called, the testing agent will stop processing and the test will be marked as passed."),
                AIFunctionFactory.Create(
                    (string expectedBehavior, string actualBehavior) => true,
                    Constants.MarkTestAsFailedToolName,
                    """
                    Call this function to mark the test as finished and failed. Once this function is called, the testing agent will stop processing and the test will be marked as failed.
                    Please provide structured information about the expected behavior and the actual behavior that led to the failure to help with debugging and analysis of test results.
                    """)
            },
            ToolMode = new AutoChatToolMode()
        };

        for (var i = 0; i < _maxIterations; i++)
        {
            await InvokeTestingAgentAsync(testExecutionContext, testAgentChatClient, chatOptions, token);
            
            if (testExecutionContext.IsComplete)
                break;
             
            await InvokeTestAppAsync(chatClient, fileClient, testExecutionContext, token);

            if (testExecutionContext.IsComplete)
                break;
            
            testExecutionContext.Iteration = i + 1;
        }

        return new LlmAgentTestResult(
            testExecutionContext.Messages,
            testExecutionContext.CompleteReasons,
            testExecutionContext.DialUsage,
            testExecutionContext.AgentUsage,
            testExecutionContext.TestCasePassed,
            testExecutionContext.TestCaseExpectedBehavior,
            testExecutionContext.TestCaseActualBehavior);
    }

    private async Task InvokeTestingAgentAsync(
        LlmAgentTestStrategyExecutionContext testExecutionContext,
        IChatClient testingAgentChatClient,
        ChatOptions chatOptions,
        CancellationToken token)
    {
        var chatActionResults = new List<ChatActionResult>();
        foreach (var chatActionCondition in _chatActions)
        {
            if (!chatActionCondition.NeedToEvaluate(testExecutionContext)) continue;
        
            var chatActionResult = await chatActionCondition.EvaluateAsync(testExecutionContext, token);
            chatActionResults.Add(chatActionResult);
        }

        if (!testExecutionContext.IsFirstIteration)
        {
            var lastTestAppMessage = testExecutionContext.LastDialAssistantMessageOrDefault();
            var lastTestAppMessageContent = new StringBuilder(lastTestAppMessage?.Content ?? string.Empty);
            
            foreach (var chatActionResult in chatActionResults)
            {
                switch (chatActionResult)
                {
                    case AppendMessageContentChatActionResult appendMessageContentResult:
                        lastTestAppMessageContent.AppendLine();
                        lastTestAppMessageContent.AppendLine(appendMessageContentResult.ContentToAppend);
                        break;
                    case ReplaceMessageContentChatActionResult replaceMessageContentResult:
                        lastTestAppMessageContent.Clear();
                        lastTestAppMessageContent.AppendLine(replaceMessageContentResult.NewContent);
                        break;
                    case MockMessageChatActionResult:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        
            //testExecutionContext.AddAgentChatMessage(new ChatMessage(ChatRole.User, lastTestAppMessageContent.ToString()));
        }
        
        var mockMessageChatActions = chatActionResults
            .OfType<MockMessageChatActionResult>()
            .ToArray();
        
        switch (mockMessageChatActions.Length)
        {
            case > 1:
                throw new InvalidOperationException("Multiple MockMessageChatActionResults found. Only one is allowed per iteration.");
            case 1:
                // var mockMessageChatActionResult = mockMessageChatActions[0];
                // testExecutionContext.AddAgentChatMessage(new ChatMessage(ChatRole.User, mockMessageChatActionResult.Content));
                throw new NotSupportedException("Multiple MockMessageChatActionResults found.");
                break;
            case 0:
                var message = await BuildTestAgentMessageAsync(testExecutionContext, token);
                
                var agentChatClientResponse = await testingAgentChatClient.GetResponseAsync(
                    [testExecutionContext.SystemMessage, new ChatMessage(ChatRole.User, message)],
                    chatOptions,
                    token);

                ProcessChatMessage(testExecutionContext, agentChatClientResponse);
                
                if (!string.IsNullOrWhiteSpace(agentChatClientResponse.Text))
                {
                    testExecutionContext.AddMessage(new DialUserMessage(agentChatClientResponse.Text));
                }
                
                break;
        }
        
        EvaluateCompletionConditions(testExecutionContext);
    }
    
    private async Task InvokeTestAppAsync(
        IDialChatApiClient testAppChatClient,
        IDialFileApiClient fileClient, 
        LlmAgentTestStrategyExecutionContext executionContext,
        CancellationToken token)
    {
        var dialChatResponse = await testAppChatClient.CompleteChatAsync(executionContext.Messages, new DialChatOptions(), token);
        if (dialChatResponse.Usage is not null)
        {
            executionContext.AddDialUsage(dialChatResponse.Usage);
        }
        
        executionContext.AddMessage(dialChatResponse.Message);
        
        EvaluateCompletionConditions(executionContext);
        
        //executionContext.AddAgentChatMessage(new ChatMessage(ChatRole.User, dialChatResponse.Message.Content));
    }
    
    private IChatClient GetChatClient()
    {
        return new AzureOpenAIClient(_uri, new ApiKeyCredential(_apiKey))
            .GetChatClient(_modelName)
            .AsIChatClient();
    }

    private void ProcessChatMessage(LlmAgentTestStrategyExecutionContext testExecutionContext, ChatResponse chatResponse)
    {
        var lastAgentAssistantMessage = chatResponse.Messages.LastOrDefault(m => m.Role == ChatRole.Assistant);

        if (lastAgentAssistantMessage is null)
            return;

        var functionCallContent = lastAgentAssistantMessage.Contents
            .OfType<FunctionCallContent>()
            .SingleOrDefault();
        
        if (functionCallContent is null)
            return;

        if (functionCallContent.Name.Equals(Constants.SendNextMessageToChatToolName, StringComparison.OrdinalIgnoreCase))
        {
            ProcessSendNextMessageToChatToolCall(testExecutionContext, functionCallContent);
        }
        else if (functionCallContent.Name.Equals(Constants.MarkTestAsSuccessfulToolName, StringComparison.OrdinalIgnoreCase))
        {
            ProcessMarkTestAsSuccessfulToolCall(testExecutionContext, functionCallContent);
        }
        else if (functionCallContent.Name.Equals(Constants.MarkTestAsFailedToolName, StringComparison.OrdinalIgnoreCase))
        {
            ProcessMarkTestAsFailedToolCall(testExecutionContext, functionCallContent);
        }
    }

    private static void ProcessSendNextMessageToChatToolCall(LlmAgentTestStrategyExecutionContext testExecutionContext, 
        FunctionCallContent functionCallContent)
    {
        if (functionCallContent.Arguments is null)
            return;

        if (!functionCallContent.Arguments.TryGetValue("message", out var messageArg) ||
            messageArg is not JsonElement { ValueKind: JsonValueKind.String }) return;
        
        var message = messageArg.ToString();
        if (message != null)
        {
            testExecutionContext.AddMessage(new DialUserMessage(message));
        }
    }
    
    private static void ProcessMarkTestAsSuccessfulToolCall(LlmAgentTestStrategyExecutionContext testExecutionContext, 
        FunctionCallContent _)
    {
        testExecutionContext.TestCasePassed = true;
        testExecutionContext.MarkComplete("Test marked as successful by testing agent.");
    }

    private static void ProcessMarkTestAsFailedToolCall(LlmAgentTestStrategyExecutionContext testExecutionContext, 
        FunctionCallContent functionCallContent)
    {
        if (functionCallContent.Arguments is null)
            return;
        
        testExecutionContext.TestCasePassed = false;
                
        if (functionCallContent.Arguments.TryGetValue("expectedBehavior", out var expectedBehaviorParam) 
            && expectedBehaviorParam is JsonElement { ValueKind: JsonValueKind.String } expectedBehavior)
        {
            testExecutionContext.TestCaseExpectedBehavior = expectedBehavior.GetString();
        }
                
        if (functionCallContent.Arguments.TryGetValue("actualBehavior", out var actualBehaviorParam) 
            && actualBehaviorParam is JsonElement { ValueKind: JsonValueKind.String } actualBehavior)
        {
            testExecutionContext.TestCaseActualBehavior = actualBehavior.GetString();
        }
        
        testExecutionContext.MarkComplete("Test marked as failed by testing agent.");
    }
    
    private async Task<string> BuildTestAgentMessageAsync(LlmAgentTestStrategyExecutionContext testExecutionContext, CancellationToken token)
    {
        var messageBuilder = new StringBuilder();

        messageBuilder.AppendLine("Current call history history:");
        
        var filteredMessages = FilterInternalMessages(testExecutionContext.Messages).ToArray();
        messageBuilder.AppendLine(JsonSerializer.Serialize(filteredMessages, GlobalJsonSettings.ChatCompletionRequestJsonSerializerOptions));
        
        var attachmentsToPropagate = GetAttachmentsToPropagate(testExecutionContext.Messages, _mimeTypesToPropagate);
        var attachmentContent = await BuildAttachmentContentAsync(testExecutionContext, attachmentsToPropagate, token);
        
        messageBuilder.AppendLine("Attachments content:");
        messageBuilder.AppendLine(attachmentContent);
        
        return messageBuilder.ToString();
    }

    private static async Task<string> BuildAttachmentContentAsync(LlmAgentTestStrategyExecutionContext context, IReadOnlyList<DialAttachment> attachments, CancellationToken ct)
    {
        var contentBuilder = new StringBuilder();

        foreach (var attachment in attachments)
        {
            if (attachment.Url is null)
                continue;

            var attachmentContent = await context.FileClient.GetDataAsStringAsync(attachment.Url, ct);

            contentBuilder.AppendLine($"--- Attachment Content Start (Url: '{attachment.Url}', Type: '{attachment.Type}') ---");
            contentBuilder.AppendLine(attachmentContent);
            contentBuilder.AppendLine($"--- Attachment Content End (Url: '{attachment.Url}', Type: '{attachment.Type}') ---");
        }

        return contentBuilder.ToString();
    }
    
    private static IReadOnlyList<DialAttachment> GetAttachmentsToPropagate(IReadOnlyList<BaseMessage> messages, IReadOnlySet<string> mimeTypesToPropagate)
    {
        var attachmentsToPropagate = new List<DialAttachment>();
        foreach (var assistantMessage in messages.OfType<DialAssistantMessage>())
        {
            var visualizerAttachment = assistantMessage.CustomContent?.Attachments?.FirstOrDefault(a => 
                a.Type is not null && mimeTypesToPropagate.Contains(a.Type, StringComparer.OrdinalIgnoreCase));
            
            if (visualizerAttachment?.Url is not null)
            {
                attachmentsToPropagate.Add(visualizerAttachment);
            }
        }

        return attachmentsToPropagate;
    }
    
    private static IEnumerable<BaseMessage> FilterInternalMessages(IReadOnlyList<BaseMessage> messages)
    {
        var serializedMessages = JsonSerializer.Serialize(messages, GlobalJsonSettings.ChatCompletionRequestJsonSerializerOptions);
        var clonedMessages = JsonSerializer.Deserialize<List<BaseMessage>>(serializedMessages, GlobalJsonSettings.ChatCompletionResponseJsonSerializerOptions);
        
        if (clonedMessages is null)
            throw new InvalidOperationException("Failed to clone messages for filtering.");
        
        foreach (var message in clonedMessages)
        {
            if (message.Role is Role.Assistant && message is DialAssistantMessage assistantMessage)
            {
                if (assistantMessage.CustomContent?.State is null)
                {
                    yield return message;
                    continue;
                }
                
                var toolExecutionHistory = assistantMessage.CustomContent.State.GetToolExecutionHistory();
                
                var filteredToolExecutionHistory = new List<BaseMessage>();
                var toolCallIdsToExclude = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var baseMessage in toolExecutionHistory)
                {
                    switch (baseMessage)
                    {
                        case AssistantMessage { ToolCalls: not null } toolExecutionHistoryAssistantMessage:
                        {
                            var internalToolCalls = toolExecutionHistoryAssistantMessage.ToolCalls
                                .Where(tc => Constants.QuickApps.SystemToolNames.Contains(tc.Function.Name, StringComparer.OrdinalIgnoreCase))
                                .ToArray();

                            if (internalToolCalls.Length == 0)
                            {
                                filteredToolExecutionHistory.Add(baseMessage);
                                continue;
                            }

                            if (internalToolCalls.Length < toolExecutionHistoryAssistantMessage.ToolCalls.Count)
                            {
                                throw new Exception("Mixed internal and non-internal tool calls found in a single AssistantMessage. This is not supported.");
                            }

                            foreach (var internalToolCall in toolExecutionHistoryAssistantMessage.ToolCalls)
                            {
                                toolCallIdsToExclude.Add(internalToolCall.Id);
                            }

                            break;
                        }
                        case ToolMessage toolExecutionHistoryToolMessage:
                        {
                            if (!toolCallIdsToExclude.Contains(toolExecutionHistoryToolMessage.ToolCallId))
                            {
                                filteredToolExecutionHistory.Add(baseMessage);
                            }

                            break;
                        }
                        default:
                            filteredToolExecutionHistory.Add(baseMessage);
                            break;
                    }
                }
                
                assistantMessage.CustomContent.State[Constants.ToolExecutionHistoryKey] = filteredToolExecutionHistory;
            }

            yield return message;
        }
    }
}
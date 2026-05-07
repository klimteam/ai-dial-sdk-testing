using System.ClientModel;
using System.Text;
using AiDialSdk.Api.Chat;
using AiDialSdk.Api.Data;
using AiDialSdk.Api.Files;
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
    
    private readonly IReadOnlyList<ChatAction<LlmAgentTestStrategyExecutionContext>> _chatActions;
    
    public LlmAgentTestStrategy(
        string prompt, 
        Uri uri, 
        string modelName, 
        string apiKey, 
        int maxIterations, 
        IReadOnlyList<CompletionCondition> completionConditions,
        IReadOnlyList<ChatAction<LlmAgentTestStrategyExecutionContext>> chatActions) 
        : base(completionConditions)
    {
        _prompt = prompt;
        _uri = uri;
        _modelName = modelName;
        _apiKey = apiKey;
        _maxIterations = maxIterations;

        _chatActions = chatActions;
    }
    
    public async Task<TestResult> RunAsync(IDialChatApiClient chatClient, IDialFileApiClient fileClient, CancellationToken token)
    {
        var testExecutionContext = new LlmAgentTestStrategyExecutionContext(_prompt, chatClient, fileClient);
        var testAgentChatClient = GetChatClient();
        
        for (var i = 0; i < _maxIterations; i++)
        {
            await InvokeTestingAgentAsync(testExecutionContext, testAgentChatClient, token);
            
            await InvokeTestAppAsync(chatClient, fileClient, testExecutionContext, token);

            if (testExecutionContext.IsComplete)
                break;
            
            testExecutionContext.Iteration = i + 1;
        }

        return new LlmAgentTestResult(
            testExecutionContext.Messages,
            testExecutionContext.CompleteReasons,
            testExecutionContext.DialUsage,
            testExecutionContext.AgentUsage);
    }

    private async Task InvokeTestingAgentAsync(
        LlmAgentTestStrategyExecutionContext testExecutionContext,
        IChatClient testingAgentChatClient,
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
        
            testExecutionContext.AddAgentChatMessage(new ChatMessage(ChatRole.User, lastTestAppMessageContent.ToString()));
        }
        
        var mockMessageChatActions = chatActionResults
            .OfType<MockMessageChatActionResult>()
            .ToArray();
        
        switch (mockMessageChatActions.Length)
        {
            case > 1:
                throw new InvalidOperationException("Multiple MockMessageChatActionResults found. Only one is allowed per iteration.");
            case 1:
                var mockMessageChatActionResult = mockMessageChatActions[0];
                testExecutionContext.AddAgentChatMessage(new ChatMessage(ChatRole.User, mockMessageChatActionResult.Content));
                break;
            case 0:
                var agentChatClientResponse = await testingAgentChatClient.GetResponseAsync(
                    testExecutionContext.AgentChatHistory,
                    new ChatOptions(),
                    token);
                
                testExecutionContext.AddAgentChatMessages(agentChatClientResponse.Messages);
                testExecutionContext.AddMessage(new DialUserMessage(agentChatClientResponse.Text));
                testExecutionContext.AddAgentUsage(agentChatClientResponse.Usage);
                break;
        }
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
}
using System.ClientModel;
using AiDialSdk.Api.Chat;
using AiDialSdk.Api.Data;
using AiDialSdk.Testing.Core;
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
    
    public LlmAgentTestStrategy(
        string prompt, 
        Uri uri, 
        string modelName, 
        string apiKey, 
        int maxIterations, 
        IReadOnlyList<CompletionCondition> completionConditions,
        IReadOnlyList<ChatActionCondition<LlmAgentTestStrategyExecutionContext>> chatActionConditions) 
        : base(completionConditions, chatActionConditions)
    {
        _prompt = prompt;
        _uri = uri;
        _modelName = modelName;
        _apiKey = apiKey;
        _maxIterations = maxIterations;
    }
    
    public async Task<TestStrategyResult> RunAsync(IDialChatApiClient chatClient, CancellationToken token)
    {
        var testExecutionContext = new LlmAgentTestStrategyExecutionContext(_prompt);
        var agentChatClient = GetChatClient();
        
        bool skipNextAgentIteration = false;
        
        for (var i = 0; i < _maxIterations; i++)
        {
            testExecutionContext.Iteration = i + 1;
            
            if (skipNextAgentIteration)
            {
                skipNextAgentIteration = false;
            }
            else
            {
                var agentChatClientResponse = await agentChatClient.GetResponseAsync(
                    testExecutionContext.AgentChatHistory,
                    new ChatOptions(),
                    token);

                testExecutionContext.AddAgentChatMessages(agentChatClientResponse.Messages);
                testExecutionContext.AddMessage(new DialUserMessage(agentChatClientResponse.Text));

                if (agentChatClientResponse.Usage is not null)
                {
                    testExecutionContext.AddAgentUsage(agentChatClientResponse.Usage);
                }
            }

            var dialChatResponse = await chatClient.CompleteChatAsync(testExecutionContext.Messages, new DialChatOptions(), token);
            testExecutionContext.AddMessage(dialChatResponse.Message);

            if (dialChatResponse.Usage is not null)
            {
                testExecutionContext.AddDialUsage(dialChatResponse.Usage);
            }
            
            EvaluateCompletionConditions(testExecutionContext);
            if (testExecutionContext.IsComplete)
                break;

            var mockChatActionEvaluated = EvaluateChatActionConditions(testExecutionContext);
            if (!mockChatActionEvaluated)
            {
                testExecutionContext.AddAgentChatMessage(new ChatMessage(ChatRole.User, dialChatResponse.Message.Content));
            }
            else
            {
                skipNextAgentIteration = true;
            }
        }

        return new LlmAgentTestStrategyResult(
            testExecutionContext.Messages,
            testExecutionContext.CompleteReasons,
            testExecutionContext.DialUsage,
            testExecutionContext.AgentUsage);
    }

    private IChatClient GetChatClient()
    {
        return new AzureOpenAIClient(_uri, new ApiKeyCredential(_apiKey))
            .GetChatClient(_modelName)
            .AsIChatClient();
    }
}
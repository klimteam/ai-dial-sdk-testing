using AiDialSdk.Api.OpenAi.Data;
using AiDialSdk.Testing.Core;
using AiDialSdk.Testing.Extensions;
using AiDialSdk.Testing.TestStrategies.Implementations;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;

namespace AiDialSdk.Testing.TestStrategies.Builders;

public class LlmAgentTestStrategyBuilder : BaseTestStrategyBuilder<LlmAgentTestStrategyExecutionContext>
{
    private readonly IConfiguration _configuration;
    
    private string? _prompt;
    private string? _endpoint;
    private string? _modelName;
    private string? _apiKey;
    private int _maxIterations = 5;

    public LlmAgentTestStrategyBuilder(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    
    public LlmAgentTestStrategyBuilder WithPrompt(string prompt)
    {
        _prompt = prompt;
        return this;
    }
    
    public LlmAgentTestStrategyBuilder WithEndpoint(string uri)
    {
        _endpoint = uri;
        return this;
    }
    
    public LlmAgentTestStrategyBuilder WithModelName(string modelName)
    {
        if (string.IsNullOrWhiteSpace(modelName))
            throw new ArgumentException("Model name cannot be null or whitespace.", nameof(modelName));
        
        _modelName = modelName;
        return this;
    }
    
    public LlmAgentTestStrategyBuilder WithApiKey(string apiKey)
    {
        _apiKey = apiKey;
        return this;
    }
    
    public LlmAgentTestStrategyBuilder WithMaxIterations(int maxIterations)
    {
        _maxIterations = maxIterations;
        return this;
    }
    
    public LlmAgentTestStrategyBuilder CompletedWithToolCallByAnotherTool(string callingToolName, string calledToolName)
    {
        WithToolCallByToolCompletionInternal(callingToolName, calledToolName);
        return this;
    }
    
    public LlmAgentTestStrategyBuilder WithMessageAfterVisualizer(string visualizerName, string message, string llmAgentMockMessage)
    {
        ChatActionConditions.Add(new ChatActionCondition<LlmAgentTestStrategyExecutionContext>(context =>
        {
            var lastAssistantMessage = context.LastDialAssistantMessageOrDefault();
            var visualizerAttachment = lastAssistantMessage?.CustomContent?.Attachments?.FirstOrDefault(a => 
                a.Type is not null && a.Type.Equals(visualizerName, StringComparison.OrdinalIgnoreCase));
            return visualizerAttachment is not null;
        }, context =>
        {
            context.AddMessage(new DialUserMessage(message));
            context.AddAgentChatMessage(new ChatMessage(ChatRole.User, llmAgentMockMessage));
            context.AddAgentChatMessage(new ChatMessage(ChatRole.Assistant, message));
        }));
        
        return this;
    }
    
    internal LlmAgentTestStrategy Build()
    {
        if (string.IsNullOrWhiteSpace(_prompt))
            throw new InvalidOperationException("Prompt is not set.");
        
        if (string.IsNullOrWhiteSpace(_modelName))
            throw new InvalidOperationException("ModelName is not set.");
        
        return new LlmAgentTestStrategy(
            _prompt, 
            new Uri(GetEndpoint()), 
            _modelName, 
            GetApiKey(), 
            _maxIterations, 
            CompletionConditions, 
            ChatActionConditions);
    }
    
    private string GetEndpoint()
    {
        var endpoint = _configuration.GetValue<string>(LlmTestDefinitionBuilder.EndpointConfigurationKey);
        return endpoint ?? _endpoint ?? throw new InvalidOperationException(
            $"Endpoint must be provided either through configuration key '{LlmTestDefinitionBuilder.EndpointConfigurationKey}' or constructor parameter.");
    }

    private string GetApiKey()
    {
        var apiKey = _configuration.GetValue<string>(LlmTestDefinitionBuilder.ApiKeyConfigurationKey);
        return apiKey ?? _apiKey ?? throw new InvalidOperationException(
            $"API key must be provided either through configuration key '{LlmTestDefinitionBuilder.ApiKeyConfigurationKey}' or constructor parameter.");
    }
}
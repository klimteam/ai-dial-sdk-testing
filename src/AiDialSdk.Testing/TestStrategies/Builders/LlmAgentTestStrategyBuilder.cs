using AiDialSdk.Api.OpenAi.Data;
using AiDialSdk.Testing.Core;
using AiDialSdk.Testing.Extensions;
using AiDialSdk.Testing.TestStrategies.Implementations;
using Microsoft.Extensions.AI;

namespace AiDialSdk.Testing.TestStrategies.Builders;

public class LlmAgentTestStrategyBuilder : BaseTestStrategyBuilder<LlmAgentTestStrategyExecutionContext>
{
    private string? _prompt;
    private Uri? _uri;
    private string? _modelName;
    private string? _apiKey;
    private int _maxIterations = 5;
    
    public LlmAgentTestStrategyBuilder WithPrompt(string prompt)
    {
        _prompt = prompt;
        return this;
    }
    
    public LlmAgentTestStrategyBuilder WithEndpoint(string uri)
    {
        _uri = new Uri(uri);
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
    
    public LlmAgentTestStrategyBuilder CompletedWithToolCall(string toolName)
    {
        WithToolCallCompletionInternal(toolName);
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
            context.AddMessage(new UserMessage(message));
            context.AddAgentChatMessage(new ChatMessage(ChatRole.User, llmAgentMockMessage));
            context.AddAgentChatMessage(new ChatMessage(ChatRole.Assistant, message));
        }));
        
        return this;
    }
    
    internal LlmAgentTestStrategy Build()
    {
        if (string.IsNullOrWhiteSpace(_prompt))
            throw new InvalidOperationException("Prompt is not set.");
        
        if (_uri is null)
            throw new InvalidOperationException("Uri is not set.");
        
        if (string.IsNullOrWhiteSpace(_modelName))
            throw new InvalidOperationException("ModelName is not set.");
        
        if (string.IsNullOrWhiteSpace(_apiKey))
            throw new InvalidOperationException("ApiKey is not set.");
        
        return new LlmAgentTestStrategy(
            _prompt, 
            _uri, 
            _modelName, 
            _apiKey, 
            _maxIterations, 
            CompletionConditions, 
            ChatActionConditions);
    }
}
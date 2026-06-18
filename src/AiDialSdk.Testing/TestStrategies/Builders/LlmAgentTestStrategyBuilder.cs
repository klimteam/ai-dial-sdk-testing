using System.Text;
using AiDialSdk.Api.Files.Extensions;
using AiDialSdk.Testing.Core;
using AiDialSdk.Testing.Extensions;
using AiDialSdk.Testing.TestStrategies.Implementations;
using Microsoft.Extensions.Configuration;

namespace AiDialSdk.Testing.TestStrategies.Builders;

public class LlmAgentTestStrategyBuilder : BaseTestStrategyBuilder<LlmAgentTestStrategyExecutionContext>
{
    private const string LlmAgentTestStrategyConfigurationKey = "LlmAgentTestStrategyConfiguration";
    
    private const string EndpointConfigurationKey = $"{LlmAgentTestStrategyConfigurationKey}:Endpoint";
    private const string ModelNameKey = $"{LlmAgentTestStrategyConfigurationKey}:ModelName";
    private const string ApiKeyConfigurationKey = $"{LlmAgentTestStrategyConfigurationKey}:ApiKey";
    
    private readonly IConfiguration _configuration;
    private readonly List<string> _mimeTypesToPropagate = [];
    
    private string? _prompt;
    private string? _firstMessage;
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
    
    public LlmAgentTestStrategyBuilder WithFirstMessage(string firstMessage)
    {
        _firstMessage = firstMessage;
        return this;
    }
    
    public LlmAgentTestStrategyBuilder WithEndpoint(string uri)
    {
        _endpoint = uri;
        return this;
    }

    public LlmAgentTestStrategyBuilder WithAttachmentToContentPropagation(string mimeType)
    {
        _mimeTypesToPropagate.Add(mimeType);
        
        // ChatActionConditions.Add(new ChatAction<LlmAgentTestStrategyExecutionContext>(context =>
        // {
        //     var lastAssistantMessage = context.LastDialAssistantMessageOrDefault();
        //     var visualizerAttachment = lastAssistantMessage?.CustomContent?.Attachments?.FirstOrDefault(a => 
        //         a.Type is not null && a.Type.Equals(mimeType, StringComparison.OrdinalIgnoreCase));
        //     return visualizerAttachment is not null && !string.IsNullOrWhiteSpace(visualizerAttachment.Url);
        // }, async (context, ct) =>
        // {
        //     var lastAssistantMessage = context.LastDialAssistantMessageOrDefault();
        //     
        //     if (lastAssistantMessage is null)
        //         throw new InvalidOperationException("No assistant message found in context.");
        //     
        //     var visualizerAttachment = lastAssistantMessage.CustomContent?.Attachments?.FirstOrDefault(a => 
        //         a.Type is not null && a.Type.Equals(mimeType, StringComparison.OrdinalIgnoreCase));
        //     
        //     if (visualizerAttachment is null)
        //         throw new InvalidOperationException("No visualizer attachment found in context.");
        //
        //     if (string.IsNullOrWhiteSpace(visualizerAttachment.Url))
        //         throw new InvalidOperationException("Visualizer attachment does not have a URL.");
        //     
        //     var attachmentContent = await context.FileClient.GetDataAsStringAsync(visualizerAttachment.Url, ct);
        //
        //     var content = new StringBuilder();
        //     content.AppendLine("--- Attachment Content Start ---");
        //     content.AppendLine(attachmentContent);
        //     content.AppendLine("--- Attachment Content End ---");
        //
        //     return new AppendMessageContentChatActionResult(content.ToString());
        // }));// ChatActionConditions.Add(new ChatAction<LlmAgentTestStrategyExecutionContext>(context =>
        // {
        //     var lastAssistantMessage = context.LastDialAssistantMessageOrDefault();
        //     var visualizerAttachment = lastAssistantMessage?.CustomContent?.Attachments?.FirstOrDefault(a => 
        //         a.Type is not null && a.Type.Equals(mimeType, StringComparison.OrdinalIgnoreCase));
        //     return visualizerAttachment is not null && !string.IsNullOrWhiteSpace(visualizerAttachment.Url);
        // }, async (context, ct) =>
        // {
        //     var lastAssistantMessage = context.LastDialAssistantMessageOrDefault();
        //     
        //     if (lastAssistantMessage is null)
        //         throw new InvalidOperationException("No assistant message found in context.");
        //     
        //     var visualizerAttachment = lastAssistantMessage.CustomContent?.Attachments?.FirstOrDefault(a => 
        //         a.Type is not null && a.Type.Equals(mimeType, StringComparison.OrdinalIgnoreCase));
        //     
        //     if (visualizerAttachment is null)
        //         throw new InvalidOperationException("No visualizer attachment found in context.");
        //
        //     if (string.IsNullOrWhiteSpace(visualizerAttachment.Url))
        //         throw new InvalidOperationException("Visualizer attachment does not have a URL.");
        //     
        //     var attachmentContent = await context.FileClient.GetDataAsStringAsync(visualizerAttachment.Url, ct);
        //
        //     var content = new StringBuilder();
        //     content.AppendLine("--- Attachment Content Start ---");
        //     content.AppendLine(attachmentContent);
        //     content.AppendLine("--- Attachment Content End ---");
        //
        //     return new AppendMessageContentChatActionResult(content.ToString());
        // }));
        
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
    
    public LlmAgentTestStrategyBuilder WithMessageAfterVisualizer(string visualizerName, string message)
    {
        ChatActionConditions.Add(new ChatAction<LlmAgentTestStrategyExecutionContext>(context =>
        {
            var lastAssistantMessage = context.LastDialAssistantMessageOrDefault();
            var visualizerAttachment = lastAssistantMessage?.CustomContent?.Attachments?.FirstOrDefault(a => 
                a.Type is not null && a.Type.Equals(visualizerName, StringComparison.OrdinalIgnoreCase));
            return visualizerAttachment is not null;
        }, (_, _) => Task.FromResult<ChatActionResult>(new MockMessageChatActionResult(message))));
        
        return this;
    }
    
    internal LlmAgentTestStrategy Build()
    {
        if (string.IsNullOrWhiteSpace(_prompt))
            throw new InvalidOperationException("Prompt is not set.");
        
        return new LlmAgentTestStrategy(
            _prompt, 
            _firstMessage,
            new Uri(GetEndpoint()), 
            GetModelName(), 
            GetApiKey(), 
            _maxIterations, 
            _mimeTypesToPropagate,
            CompletionConditions, 
            ChatActionConditions);
    }
    
    private string GetEndpoint()
    {
        var endpoint = _configuration.GetValue<string>(EndpointConfigurationKey);
        return endpoint ?? _endpoint ?? throw new InvalidOperationException(
            $"Endpoint must be provided either through configuration key '{EndpointConfigurationKey}' or constructor parameter.");
    }

    private string GetModelName()
    {
        var modelName = _configuration.GetValue<string>(ModelNameKey);
        return modelName ?? _modelName ?? throw new InvalidOperationException(
            $"Model name must be provided either through configuration key '{ModelNameKey}' or constructor parameter.");
    }
    
    private string GetApiKey()
    {
        var apiKey = _configuration.GetValue<string>(ApiKeyConfigurationKey);
        return apiKey ?? _apiKey ?? throw new InvalidOperationException(
            $"API key must be provided either through configuration key '{ApiKeyConfigurationKey}' or constructor parameter.");
    }
}
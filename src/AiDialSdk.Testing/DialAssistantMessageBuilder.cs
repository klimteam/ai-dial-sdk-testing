using System.Text.Json;
using AiDialSdk.Api.Data;
using AiDialSdk.Api.Infrastructure;
using AiDialSdk.Api.OpenAi.Data;

namespace AiDialSdk.Testing;

public class DialAssistantMessageBuilder
{
    private string? _content = null;
    private string? _name = null;
    private string? _refusal = null;
    private List<ToolCall>? _toolCalls = null;
    
    private List<DialAttachment>? _attachments = null;
    private List<DialStage>? _stages = null;
    private DialFormSchema? _formSchema = null;
    private DialState? _state = null;
    private List<BaseMessage>? _toolExecutionHistory = null;
    
    public DialAssistantMessageBuilder WithContent(string content)
    {
        _content = content;
        return this;
    }

    public DialAssistantMessageBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public DialAssistantMessageBuilder WithRefusal(string refusal)
    {
        _refusal = refusal;
        return this;
    }

    public DialAssistantMessageBuilder WithToolCall(ToolCall toolCall)
    {
        _toolCalls ??= [];
        _toolCalls.Add(toolCall);
        return this;
    }

    public DialAssistantMessageBuilder WithToolCallInToolExecutionHistory<TArguments, TContent>(
        string toolCallId, 
        string toolName,
        TArguments arguments,
        TContent? contentData,
        IReadOnlyList<DialAttachment>? attachments = null)
    {
        _toolExecutionHistory ??= [];
        
        var toolCallMessages = BuildToolCallMessages(
            toolCallId,
            toolName,
            arguments,
            contentData,
            attachments);
        
        _toolExecutionHistory.AddRange(toolCallMessages);
        return this;
    }
    
    public DialAssistantMessage Build()
    {
        return new DialAssistantMessage(
            _content,
            _name,
            _refusal,
            _toolCalls,
            BuildDialCustomContent());
    }

    public static DialAssistantMessageBuilder Create()
    {
        return new DialAssistantMessageBuilder();
    }
    
    private DialCustomContent? BuildDialCustomContent()
    {
        if (_attachments is null && _stages is null && _formSchema is null && _state is null && _toolExecutionHistory is null)
            return null;

        if (_toolExecutionHistory is not null)
        {
            _state ??= new DialState(new Dictionary<string, object?>());
            _state.Add(Constants.ToolExecutionHistoryKey, BuildToolExecutionHistory(_toolExecutionHistory));
        }
        
        return new DialCustomContent(_attachments, _stages, _formSchema, _state);
    }
    
    private static BaseMessage[] BuildToolCallMessages<TArguments, TContent>(
        string toolCallId,
        string toolName,
        TArguments arguments, 
        TContent? contentData, 
        IReadOnlyList<DialAttachment>? attachments)
    {
        BaseMessage[] messages =
        [
            new DialAssistantMessage(
                string.Empty,
                null,
                null,
                [
                    new ToolCall(
                        0,
                        toolCallId,
                        ToolType.Function,
                        new Function(
                            toolName,
                            JsonSerializer.Serialize(arguments, GlobalJsonSettings.DefaultJsonSerializerOptions))
                    )
                ]),
            new DialToolMessage(
                contentData is null 
                    ? string.Empty 
                    : JsonSerializer.Serialize(contentData, GlobalJsonSettings.DefaultJsonSerializerOptions),
                toolCallId,
                new DialCustomContent(attachments, null, null, null))
        ];

        return messages;
    }
    
    private static object BuildToolExecutionHistory(IEnumerable<BaseMessage> messages)
    {
        var content = JsonSerializer.Serialize(messages, GlobalJsonSettings.ChatCompletionRequestJsonSerializerOptions);
        
        var result = JsonSerializer.Deserialize<object>(
                         content, 
                         GlobalJsonSettings.ChatCompletionRequestJsonSerializerOptions)
                     ?? throw new NullReferenceException();
        
        return result;
    }
}
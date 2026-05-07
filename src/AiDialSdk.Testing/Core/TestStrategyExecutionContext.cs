using AiDialSdk.Api.Chat;
using AiDialSdk.Api.Files;
using AiDialSdk.Api.OpenAi.Data;

namespace AiDialSdk.Testing.Core;

public class TestStrategyExecutionContext
{
    private readonly List<BaseMessage> _messages = [];
    private readonly List<string> _completeReasons = [];
    
    private Usage? _dialUsage;

    public TestStrategyExecutionContext(IDialChatApiClient chatClient, IDialFileApiClient fileClient)
    {
        ChatClient = chatClient;
        FileClient = fileClient;
    }
    
    public IDialChatApiClient ChatClient { get; }
    
    public IDialFileApiClient FileClient { get; }
    
    public IReadOnlyList<BaseMessage> Messages => _messages;
    
    public IReadOnlyList<string> CompleteReasons => _completeReasons;

    public Usage? DialUsage => _dialUsage;

    internal void AddMessage(BaseMessage message)
    {
        _messages.Add(message);
    }
    
    public bool IsComplete { get; private set; }
    
    internal void MarkComplete(string reason)
    {
        IsComplete = true;
        _completeReasons.Add(reason);
    }
    
    internal void AddDialUsage(Usage dialUsage)
    {
        _dialUsage = _dialUsage is null 
            ? dialUsage 
            : _dialUsage.Combine(dialUsage);
    }
}
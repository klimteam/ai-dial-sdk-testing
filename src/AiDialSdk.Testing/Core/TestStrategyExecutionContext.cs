using AiDialSdk.Api.OpenAi.Data;

namespace AiDialSdk.Testing.Core;

public class TestStrategyExecutionContext
{
    private readonly List<BaseMessage> _messages = [];
    private readonly List<string> _completeReasons = [];
    private readonly List<ChatAction> _chatActionsHistory = [];
    
    private Usage? _dialUsage;
    
    public IReadOnlyList<BaseMessage> Messages => _messages;

    public IReadOnlyList<ChatAction> ChatActionsHistory => _chatActionsHistory;
    
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
    
    internal void AddChatAction(ChatAction chatAction)
    {
        _chatActionsHistory.Add(chatAction);
    }
    
    internal void AddDialUsage(Usage dialUsage)
    {
        _dialUsage = _dialUsage is null 
            ? dialUsage 
            : _dialUsage.Combine(dialUsage);
    }
}
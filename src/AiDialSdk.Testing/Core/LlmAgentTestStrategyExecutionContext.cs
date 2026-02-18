using Microsoft.Extensions.AI;

namespace AiDialSdk.Testing.Core;

public class LlmAgentTestStrategyExecutionContext : TestStrategyExecutionContext
{
    private readonly List<ChatMessage> _agentChatHistory;

    public LlmAgentTestStrategyExecutionContext(string prompt)
    {
        _agentChatHistory = [new ChatMessage(ChatRole.System, prompt)];
    }
    
    public IReadOnlyList<ChatMessage> AgentChatHistory => _agentChatHistory;
    
    public UsageDetails? AgentUsage { get; private set; }
    
    public int Iteration { get; internal set; }
    
    internal void AddAgentChatMessage(ChatMessage message)
    {
        _agentChatHistory.Add(message);
    }
    
    internal void AddAgentChatMessages(IList<ChatMessage> messages)
    {
        _agentChatHistory.AddRange(messages);
    }
    
    public void AddAgentUsage(UsageDetails agentUsage)
    {
        if (AgentUsage is null)
        {
            AgentUsage = agentUsage;
        }
        else
        {
            AgentUsage.Add(agentUsage);
        }
    }
}
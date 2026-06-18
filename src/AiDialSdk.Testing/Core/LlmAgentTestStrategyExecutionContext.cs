using AiDialSdk.Api.Chat;
using AiDialSdk.Api.Files;
using Microsoft.Extensions.AI;

namespace AiDialSdk.Testing.Core;

public class LlmAgentTestStrategyExecutionContext : TestStrategyExecutionContext
{
    public LlmAgentTestStrategyExecutionContext(string prompt, IDialChatApiClient chatClient, 
        IDialFileApiClient fileClient) : base(chatClient, fileClient)
    {
        SystemMessage = new ChatMessage(ChatRole.System, prompt);
    }
    
    public ChatMessage SystemMessage { get; }
    
    public UsageDetails? AgentUsage { get; private set; }
    public int Iteration { get; internal set; }
    
    public bool IsFirstIteration => Iteration == 0;
    
    public bool? TestCasePassed { get; internal set; }
    
    public string? TestCaseExpectedBehavior { get; internal set; }
    
    public string? TestCaseActualBehavior { get; internal set; }
    
    public void AddAgentUsage(UsageDetails? agentUsage)
    {
        if (agentUsage is null)
            return;
        
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
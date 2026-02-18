using AiDialSdk.Api.OpenAi.Data;
using Microsoft.Extensions.AI;

namespace AiDialSdk.Testing.TestStrategies.Models;

public class LlmAgentTestStrategyResult : TestStrategyResult
{
    public LlmAgentTestStrategyResult(
        IReadOnlyList<BaseMessage> messages, 
        IReadOnlyList<string> completeReasons, 
        Usage? dialUsage,
        UsageDetails? agentUsage)
        : base(messages, completeReasons, dialUsage)
    {
        AgentUsage = agentUsage;
    }
    
    public UsageDetails? AgentUsage { get; }
}
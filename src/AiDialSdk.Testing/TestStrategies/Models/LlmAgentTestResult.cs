using AiDialSdk.Api.OpenAi.Data;
using Microsoft.Extensions.AI;

namespace AiDialSdk.Testing.TestStrategies.Models;

public class LlmAgentTestResult : TestResult
{
    public LlmAgentTestResult(
        IReadOnlyList<BaseMessage> messages, 
        IReadOnlyList<string> completeReasons, 
        Usage? dialUsage,
        UsageDetails? agentUsage,
        bool? testCasePassed,
        string? testCaseExpectedBehavior,
        string? testCaseActualBehavior)
        : base(messages, completeReasons, dialUsage)
    {
        AgentUsage = agentUsage;
        
        TestCasePassed = testCasePassed;
        TestCaseExpectedBehavior = testCaseExpectedBehavior;
        TestCaseActualBehavior = testCaseActualBehavior;
    }
    
    public UsageDetails? AgentUsage { get; }
    
    public bool? TestCasePassed { get; }
    
    public string? TestCaseExpectedBehavior { get; }
    public string? TestCaseActualBehavior { get; }
}
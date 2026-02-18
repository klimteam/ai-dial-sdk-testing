using AiDialSdk.Api.OpenAi.Data;

namespace AiDialSdk.Testing.TestStrategies.Models;

public class TestStrategyResult
{
    public TestStrategyResult(IReadOnlyList<BaseMessage> messages, IReadOnlyList<string> completeReasons, Usage? dialUsage)
    {
        Messages = messages;
        CompleteReasons = completeReasons;
        DialUsage = dialUsage;
    }
    
    public IReadOnlyList<BaseMessage> Messages { get; }
    
    public IReadOnlyList<string> CompleteReasons { get; }
    
    public Usage? DialUsage { get; }
}
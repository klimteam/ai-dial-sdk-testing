using AiDialSdk.Api.Data;
using AiDialSdk.Api.OpenAi.Data;
using AiDialSdk.Testing.Core;

namespace AiDialSdk.Testing.Extensions;

public static class TestExecutionContextExtensions
{
    public static bool ContainsMessageOfType<T>(this TestStrategyExecutionContext testStrategyExecutionContext) where T : BaseMessage
    {
        return testStrategyExecutionContext.Messages.OfType<T>().Any();
    }

    public static DialAssistantMessage? LastDialAssistantMessageOrDefault(this TestStrategyExecutionContext testStrategyExecutionContext)
    {
        var lastMessage = testStrategyExecutionContext.Messages.Count > 0
            ? testStrategyExecutionContext.Messages[^1]
            : null;
            
        return lastMessage as DialAssistantMessage;
    }
}
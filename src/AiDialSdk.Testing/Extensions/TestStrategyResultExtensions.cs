using AiDialSdk.Api.Data;
using AiDialSdk.Api.OpenAi.Data;
using AiDialSdk.Testing.Models;
using AiDialSdk.Testing.TestStrategies.Models;

namespace AiDialSdk.Testing.Extensions;

public static class TestStrategyResultExtensions
{
    private const string ToolExecutionHistoryKey = "tool_execution_history";

    public static bool ContainsMessageOfType<T>(this TestStrategyResult testStrategyResult) where T : BaseMessage
    {
        return testStrategyResult.Messages.OfType<T>().Any();
    }

    public static DialAssistantMessage WithLastAssistantMessage(this TestStrategyResult testStrategyResult)
    {
        var lastMessage = testStrategyResult.Messages.LastOrDefault();
        return lastMessage as DialAssistantMessage ?? throw new InvalidOperationException("The last message is not an AssistantMessage.");
    }

    public static bool ToolWasCalled(this AssistantMessage assistantMessage, string toolName)
    {
        return assistantMessage.ToolCalls != null && assistantMessage.ToolCalls.Any(tc => tc.Function.Name == toolName);
    }
    
    public static IReadOnlyList<ToolExecutionHistory> GetQuickAppToolExecutionHistory(this DialAssistantMessage assistantMessage)
    {
        if (assistantMessage.CustomContent?.State is null || 
            !assistantMessage.CustomContent.State.TryGetValue<ToolExecutionHistory[]>(ToolExecutionHistoryKey,
                out var toolExecutionHistory))
            return [];

        return toolExecutionHistory;
    }
    
    public static bool ToolWasCalledByQuickApp(this DialAssistantMessage assistantMessage, string toolName)
    {
        if (assistantMessage.CustomContent?.State is null)
            return false;

        if (!assistantMessage.CustomContent.State.TryGetValue<ToolExecutionHistory[]>(ToolExecutionHistoryKey,
                out var toolExecutionHistory))
        {
            return false;
        }
            
        return toolExecutionHistory
            .Select(tc => tc.ToolCall.Function.Name)
            .Select(SanitizeToolName)
            .Any(n => n.Equals(toolName));
    }
    
    private static string SanitizeToolName(string toolName)
    {
        return toolName.Trim().Substring(0, toolName.Length - 5).ToLowerInvariant();
    }
}
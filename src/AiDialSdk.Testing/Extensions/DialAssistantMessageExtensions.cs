using AiDialSdk.Api.Data;
using AiDialSdk.Api.OpenAi.Data;

namespace AiDialSdk.Testing.Extensions;

public static class DialAssistantMessageExtensions
{
    public static bool QuickAppWasCalled(this DialAssistantMessage assistantMessage, string quickAppName)
    {
        var quickAppExecutionHistory = assistantMessage.GetToolExecutionHistory();
        return quickAppExecutionHistory
            .OfType<DialAssistantMessage>()
            .Any(am => 
                am.ToolCalls is not null && am.ToolCalls.WhereName(quickAppName).Any());
    }
    
    public static bool ToolWasCalledByQuickApp(this DialAssistantMessage assistantMessage, string quickAppName, 
        string toolName)
    {
        var quickAppCallsHistory = assistantMessage.GetQuickAppCallsHistory(quickAppName);
        return quickAppCallsHistory
            .OfType<DialAssistantMessage>()
            .Any(m => m.ToolCalls is not null && m.ToolCalls.WhereName(toolName).Any());
    }
    
    public static IReadOnlyList<BaseMessage> GetQuickAppCallsHistory(
        this DialAssistantMessage assistantMessage,
        string quickAppName)
    {
        var quickAppExecutionHistory = assistantMessage.GetToolExecutionHistory();
        var quickAppToolCallIds = quickAppExecutionHistory
            .OfType<DialAssistantMessage>()
            .SelectMany(am => am.ToolCalls ?? [])
            .WhereName(quickAppName)
            .Select(tc => tc.Id);

        var result = new List<BaseMessage>();
        foreach (var toolCallId in quickAppToolCallIds)
        {
            var toolMessage = quickAppExecutionHistory
                .OfType<DialToolMessage>()
                .Single(tm => tm.ToolCallId.Equals(toolCallId, StringComparison.OrdinalIgnoreCase));
            
            result.AddRange(toolMessage.GetQuickAppExecutionHistory());
        }

        return result;
    }
    
    public static IReadOnlyList<BaseMessage> GetToolExecutionHistory(this DialAssistantMessage assistantMessage)
    {
        return assistantMessage.CustomContent?.State is null 
            ? [] 
            : assistantMessage.CustomContent.State.GetToolExecutionHistory();
    }
}
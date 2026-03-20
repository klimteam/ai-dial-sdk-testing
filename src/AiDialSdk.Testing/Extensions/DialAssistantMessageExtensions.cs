using AiDialSdk.Api.Data;
using AiDialSdk.Api.OpenAi.Data;
using AiDialSdk.Testing.Helpers;

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
    
    public static IEnumerable<string> GetCalledQuickAppHistoryNames(this DialAssistantMessage dialAssistantMessage)
    {
        return dialAssistantMessage
            .GetToolExecutionHistory()
            .OfType<DialAssistantMessage>()
            .SelectMany(am => am.ToolCalls ?? [])
            .Select(tc => ToolHelpers.SanitizeToolName(tc.Function.Name));
    }
    
    public static IEnumerable<string> GetCalledQuickAppHistory(this DialAssistantMessage dialAssistantMessage)
    {
        return dialAssistantMessage
            .GetToolExecutionHistory()
            .OfType<DialAssistantMessage>()
            .SelectMany(am => am.ToolCalls ?? [])
            .Select(tc => ToolHelpers.SanitizeToolName(tc.Function.Name))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }
    
    public static bool ToolWasCalledByQuickApp(this DialAssistantMessage assistantMessage, string quickAppName, 
        string toolName)
    {
        return assistantMessage
            .QuickAppMessages(quickAppName)
            .OfType<DialAssistantMessage>()
            .Any(m => m.ToolCalls is not null && m.ToolCalls.WhereName(toolName).Any());
    }
    
    public static IEnumerable<BaseMessage> QuickAppMessages(this DialAssistantMessage dialAssistantMessage, string quickAppName)
    {
        var quickAppExecutionHistory = dialAssistantMessage.GetToolExecutionHistory();
        var quickAppToolCallIds = quickAppExecutionHistory
            .OfType<DialAssistantMessage>()
            .SelectMany(am => am.ToolCalls ?? [])
            .WhereName(quickAppName)
            .Select(tc => tc.Id);
            
        foreach (var toolCallId in quickAppToolCallIds)
        {
            var toolMessage = quickAppExecutionHistory
                .OfType<DialToolMessage>()
                .Single(tm => tm.ToolCallId.Equals(toolCallId, StringComparison.OrdinalIgnoreCase));

            var quickAppMessages = toolMessage.GetQuickAppExecutionHistory();
            foreach (var quickAppMessage in quickAppMessages)
            {
                yield return quickAppMessage;
            }
        }
    }
    
    public static IReadOnlyList<BaseMessage> GetToolExecutionHistory(this DialAssistantMessage assistantMessage)
        => assistantMessage.CustomContent?.State?.GetToolExecutionHistory() ?? [];
    
    public static IReadOnlyList<BaseMessage> GetToolExecutionHistory(this DialToolMessage toolMessage)
        => toolMessage.CustomContent?.State?.GetToolExecutionHistory() ?? [];
}
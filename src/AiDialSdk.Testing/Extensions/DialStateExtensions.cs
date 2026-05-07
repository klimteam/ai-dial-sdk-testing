using System.Text.Json;
using AiDialSdk.Api.Data;
using AiDialSdk.Api.Infrastructure;
using AiDialSdk.Api.OpenAi.Data;

namespace AiDialSdk.Testing.Extensions;

public static class DialStateExtensions
{
    public static bool HasToolExecutionHistory(this DialState state)
    {
        return state.TryGetValue(Constants.ToolExecutionHistoryKey, out var toolExecutionHistory)
               && toolExecutionHistory?.ToString() is not null;
    }
    
    public static IReadOnlyList<BaseMessage> GetToolExecutionHistory(this DialState state)
    {
        if (!state.TryGetValue(Constants.ToolExecutionHistoryKey, out var toolExecutionHistory) || toolExecutionHistory is null)
            return [];

        var toolExecutionHistoryContent = toolExecutionHistory.ToString();
        
        if (toolExecutionHistoryContent is null)
            return [];
        
        var result  = JsonSerializer.Deserialize<BaseMessage[]>(
            toolExecutionHistoryContent, 
            GlobalJsonSettings.ChatCompletionResponseJsonSerializerOptions);

        return result ?? [];
    }
}
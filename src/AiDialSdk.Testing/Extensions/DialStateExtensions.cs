using System.Text.Json;
using AiDialSdk.Api.Data;
using AiDialSdk.Api.Infrastructure;
using AiDialSdk.Api.OpenAi.Data;

namespace AiDialSdk.Testing.Extensions;

public static class DialStateExtensions
{
    private const string ToolExecutionHistoryKey = "tool_execution_history";
    
    public static IReadOnlyList<BaseMessage> GetToolExecutionHistory(this DialState state)
    {
        if (!state.TryGetValue(ToolExecutionHistoryKey, out var toolExecutionHistory) || toolExecutionHistory is null)
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
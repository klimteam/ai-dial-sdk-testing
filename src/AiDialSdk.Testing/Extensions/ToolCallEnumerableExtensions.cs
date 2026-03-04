using AiDialSdk.Api.OpenAi.Data;
using AiDialSdk.Testing.Helpers;

namespace AiDialSdk.Testing.Extensions;

public static class ToolCallEnumerableExtensions
{
    public static ToolCall SingleByName(this IEnumerable<ToolCall> toolCalls, string name)
    {
        return toolCalls.WhereName(name).Single();
    }
    
    public static int CountByName(this IEnumerable<ToolCall> toolCalls, string name)
    {
        return toolCalls.WhereName(name).Count();
    }
    
    public static IEnumerable<ToolCall> WhereName(this IEnumerable<ToolCall> toolCalls, string name)
    {
        return toolCalls.Where(tc => ToolHelpers.SanitizeToolName(tc.Function.Name).Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
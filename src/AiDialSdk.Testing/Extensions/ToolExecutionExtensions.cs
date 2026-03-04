using AiDialSdk.Testing.Helpers;
using AiDialSdk.Testing.Models;

namespace AiDialSdk.Testing.Extensions;

public static class ToolExecutionExtensions
{
    public static IEnumerable<ToolExecution> WhereName(this IEnumerable<ToolExecution> toolExecutions, string name)
    {
        return toolExecutions.Where(te => ToolHelpers.SanitizeToolName(te.Call.Function.Name).Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}


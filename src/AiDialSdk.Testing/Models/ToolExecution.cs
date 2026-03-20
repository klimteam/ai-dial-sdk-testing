using AiDialSdk.Api.OpenAi.Data;

namespace AiDialSdk.Testing.Models;

public class ToolExecution(ToolCall call, ToolMessage result)
{
    public ToolCall Call { get; } = call;

    public ToolMessage Result { get; } = result;
}
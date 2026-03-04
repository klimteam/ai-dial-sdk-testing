using AiDialSdk.Api.OpenAi.Data;

namespace AiDialSdk.Testing.Models;

public class ToolExecution
{
    public ToolExecution(ToolCall call, ToolMessage result)
    {
        Call = call;
        Result = result;
    }

    public ToolCall Call { get; }
    
    public ToolMessage Result { get; }
}
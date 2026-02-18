using AiDialSdk.Api.OpenAi.Data;

namespace AiDialSdk.Testing.Models;

public class ToolExecutionHistory
{
    public ToolExecutionHistory(ToolCall toolCall, ToolMessage toolExecutionResult)
    {
        ToolCall = toolCall;
        ToolExecutionResult = toolExecutionResult;
    }
        
    public ToolCall ToolCall { get; }
    
    public ToolMessage ToolExecutionResult { get; }
}
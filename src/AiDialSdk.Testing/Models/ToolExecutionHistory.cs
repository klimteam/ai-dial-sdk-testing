using AiDialSdk.Api.OpenAi.Data;

namespace AiDialSdk.Testing.Models;

public class ToolExecutionHistory
{
    public ToolExecutionHistory()
    {
        Messages = [];
    }
    
    public ToolExecutionHistory(IReadOnlyList<BaseMessage> messages)
    {
        Messages = messages;
    }

    public IReadOnlyList<BaseMessage> Messages { get; }
}
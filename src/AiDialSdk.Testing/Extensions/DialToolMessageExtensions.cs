using AiDialSdk.Api.Data;
using AiDialSdk.Api.OpenAi.Data;

namespace AiDialSdk.Testing.Extensions;

public static class DialToolMessageExtensions
{
    public static IReadOnlyList<BaseMessage> GetQuickAppExecutionHistory(this DialToolMessage toolMessage)
    {
        return toolMessage.CustomContent?.State is null 
            ? [] 
            : toolMessage.CustomContent.State.GetToolExecutionHistory();
    }
}
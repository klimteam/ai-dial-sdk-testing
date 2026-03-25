using AiDialSdk.Api.Data;
using AiDialSdk.Api.OpenAi.Data;

namespace AiDialSdk.Testing.Extensions;

public static class BaseMessageExtensions
{
    public static IEnumerable<BaseMessage> FlattenExecutionHistory(this BaseMessage message)
    {
        switch (message)
        {
            case DialAssistantMessage assistantMessage:
            {
                var toolExecutionHistory = assistantMessage.GetToolExecutionHistory() ?? [];
                foreach (var toolExecutionHistoryMessage in toolExecutionHistory)
                {
                    foreach (var innerToolExecutionHistoryMessage in toolExecutionHistoryMessage.FlattenExecutionHistory())
                    {
                        yield return innerToolExecutionHistoryMessage;
                    }
                }

                yield return assistantMessage;
            }
                break;
            case DialToolMessage toolMessage:
            {
                var toolExecutionHistory = toolMessage.GetToolExecutionHistory();
                foreach (var toolExecutionHistoryMessage in toolExecutionHistory)
                {
                    foreach (var innerToolExecutionHistoryMessage in toolExecutionHistoryMessage.FlattenExecutionHistory())
                    {
                        yield return innerToolExecutionHistoryMessage;
                    }
                }
                    
                yield return toolMessage;
            } 
                break;
            default:
                yield return message;
                break;
        }
    }
}
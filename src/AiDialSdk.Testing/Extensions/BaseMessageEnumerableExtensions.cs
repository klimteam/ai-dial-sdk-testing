using AiDialSdk.Api.Data;
using AiDialSdk.Api.OpenAi.Data;
using AiDialSdk.Testing.Helpers;
using AiDialSdk.Testing.Models;

namespace AiDialSdk.Testing.Extensions;

public static class BaseMessageEnumerableExtensions
{
    public static bool ContainsMessageOfType<T>(this IEnumerable<BaseMessage> messages) where T : BaseMessage
    {
        return messages.OfType<T>().Any();
    }
    
    public static DialToolMessage FirstToolMessage(this IEnumerable<BaseMessage> messages)
    {
        var firstDialToolMessage = messages.OfType<DialToolMessage>().FirstOrDefault();
        return firstDialToolMessage ?? throw new InvalidOperationException("No DialToolMessage found in the messages.");
    }
    
    public static DialAssistantMessage LastAssistantMessage(this IEnumerable<BaseMessage> messages)
    {
        var lastDialAssistantMessage = messages.OfType<DialAssistantMessage>().LastOrDefault();
        return lastDialAssistantMessage ?? throw new InvalidOperationException("No DialAssistantMessage found in the messages.");
    }
    
    public static bool QuickAppWasCalled(this IEnumerable<BaseMessage> messages, string quickAppName)
    {
        return messages.QuickAppMessages(quickAppName).Any();
    }
    
    public static bool ToolWasCalled(this IEnumerable<BaseMessage> messages, string toolName)
    {
        return messages
            .OfType<DialAssistantMessage>()
            .SelectMany(am => am.ToolCalls ?? [])
            .WhereName(toolName)
            .Any();
    }

    public static IEnumerable<string> GetCalledToolNames(this IEnumerable<BaseMessage> messages)
    {
        return messages
            .OfType<DialAssistantMessage>()
            .SelectMany(am => am.ToolCalls ?? [])
            .Select(tc => ToolHelpers.SanitizeToolName(tc.Function.Name));
    }
    
    public static IEnumerable<BaseMessage> FlattenExecutionHistory
        (this IEnumerable<BaseMessage> messages)
    {
        return messages.SelectMany(message => message.FlattenExecutionHistory());
    }

    public static IEnumerable<BaseMessage> FlattenExecutionHistory(this BaseMessage message)
    {
        switch (message)
        {
            case DialAssistantMessage assistantMessage:
            {
                var toolExecutionHistory = assistantMessage.GetToolExecutionHistory();
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
    
    public static IEnumerable<BaseMessage> QuickAppMessages(this IEnumerable<BaseMessage> messages, string quickAppName)
    {
        return messages
            .OfType<DialAssistantMessage>()
            .SelectMany(dialAssistantMessage => dialAssistantMessage.QuickAppMessages(quickAppName));
    }
    
    public static IEnumerable<string> GetCalledQuickAppHistoryNames(this IEnumerable<BaseMessage> messages)
    {
        return messages
            .OfType<DialAssistantMessage>()
            .SelectMany(dam => dam.GetCalledQuickAppHistoryNames());
    }
    
    public static IEnumerable<string> GetCalledQuickAppNames(this IEnumerable<BaseMessage> messages)
    {
        return messages
            .OfType<DialAssistantMessage>()
            .SelectMany(dam => dam.GetCalledQuickAppHistoryNames())
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }
    
    public static IEnumerable<ToolExecution> ToolExecutions(this IEnumerable<BaseMessage> messages)
    {
        var toolCalls = new Dictionary<string, ToolCall>();
        foreach (var message in messages)
        {
            switch (message)
            {
                case DialAssistantMessage { ToolCalls: not null } assistantMessage:
                {
                    foreach (var toolCall in assistantMessage.ToolCalls)
                    {
                        toolCalls[toolCall.Id] = toolCall;
                    }

                    break;
                }
                case DialToolMessage toolMessage when toolCalls.TryGetValue(toolMessage.ToolCallId, out var toolCall):
                    yield return new ToolExecution(toolCall, toolMessage);
                    break;
            }
        }
    }
}
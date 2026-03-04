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
        var lastDialAssistantMessage = messages.OfType<DialAssistantMessage>().FirstOrDefault();
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
    
    public static IEnumerable<BaseMessage> QuickAppMessages(this IEnumerable<BaseMessage> messages, 
        string quickAppName)
    {
        foreach (var dialAssistantMessage in messages.OfType<DialAssistantMessage>())
        {
            var quickAppExecutionHistory = dialAssistantMessage.GetToolExecutionHistory();
            var quickAppToolCallIds = quickAppExecutionHistory
                .OfType<DialAssistantMessage>()
                .SelectMany(am => am.ToolCalls ?? [])
                .WhereName(quickAppName)
                .Select(tc => tc.Id);
            
            foreach (var toolCallId in quickAppToolCallIds)
            {
                var toolMessage = quickAppExecutionHistory
                    .OfType<DialToolMessage>()
                    .Single(tm => tm.ToolCallId.Equals(toolCallId, StringComparison.OrdinalIgnoreCase));

                var quickAppMessages = toolMessage.GetQuickAppExecutionHistory();
                foreach (var quickAppMessage in quickAppMessages)
                {
                    yield return quickAppMessage;
                }
            }
        }
    }
    
    public static IEnumerable<string> GetCalledQuickAppNames(this IEnumerable<BaseMessage> messages)
    {
        return messages
            .OfType<DialAssistantMessage>()
            .SelectMany(dam =>
            {
                return dam
                    .GetToolExecutionHistory()
                    .OfType<DialAssistantMessage>()
                    .SelectMany(am => am.ToolCalls ?? [])
                    .Select(tc => ToolHelpers.SanitizeToolName(tc.Function.Name));
            })
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
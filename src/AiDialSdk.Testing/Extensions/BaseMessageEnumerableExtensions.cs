using AiDialSdk.Api.Data;
using AiDialSdk.Api.OpenAi.Data;
using AiDialSdk.Testing.Models;

namespace AiDialSdk.Testing.Extensions;

public static class BaseMessageEnumerableExtensions
{
    extension(IEnumerable<BaseMessage> messages)
    {
        public bool ContainsMessageOfType<T>() where T : BaseMessage
        {
            return messages.OfType<T>().Any();
        }
        
        public DialToolMessage FirstToolMessage()
        {
            var firstDialToolMessage = messages.OfType<DialToolMessage>().FirstOrDefault();
            return firstDialToolMessage ?? throw new InvalidOperationException("No DialToolMessage found in the messages.");
        }
        
        public DialAssistantMessage LastAssistantMessage()
        {
            var lastDialAssistantMessage = messages.OfType<DialAssistantMessage>().LastOrDefault();
            return lastDialAssistantMessage ?? throw new InvalidOperationException("No DialAssistantMessage found in the messages.");
        }
        
        public bool ToolWasCalled(string toolName)
        {
            return messages
                .OfType<DialAssistantMessage>()
                .Any(dam => dam.ToolWasCalled(toolName));
        }
        
        public bool ToolWasCalled(string callingToolName, string calledToolName)
        {
            return messages
                .OfType<DialAssistantMessage>()
                .Any(dam => dam.ToolWasCalled(callingToolName, calledToolName));
        }
        
        public IEnumerable<BaseMessage> GetToolExecutionHistoryMessages()
        {
            return messages
                .OfType<DialAssistantMessage>()
                .SelectMany(dialAssistantMessage => dialAssistantMessage.GetToolExecutionHistoryMessages());
        }
        
        public IEnumerable<BaseMessage> GetToolExecutionHistoryMessages(string toolName)
        {
            return messages
                .OfType<DialAssistantMessage>()
                .SelectMany(dialAssistantMessage => dialAssistantMessage.GetToolExecutionHistoryMessages(toolName));
        }
        
        public IEnumerable<string> GetCalledToolNames()
        {
            return messages
                .OfType<DialAssistantMessage>()
                .SelectMany(dam => dam.GetCalledToolNames());
        }
    
        public IEnumerable<string> GetCalledDistinctToolNames()
        {
            return messages
                .OfType<DialAssistantMessage>()
                .SelectMany(dam => dam.GetCalledToolNames())
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }
        
        public IEnumerable<BaseMessage> FlattenExecutionHistory()
        {
            return messages.SelectMany(message => message.FlattenExecutionHistory());
        }
        
        public IEnumerable<ToolExecution> ToolExecutions()
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
}
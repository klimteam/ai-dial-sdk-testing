using AiDialSdk.Api.Data;
using AiDialSdk.Api.OpenAi.Data;
using AiDialSdk.Testing.Helpers;

namespace AiDialSdk.Testing.Extensions;

public static class DialAssistantMessageExtensions
{
    extension(DialAssistantMessage assistantMessage)
    {
        public bool ToolWasCalled(string toolName)
        {
            if (!assistantMessage.HasToolExecutionHistory())
            {
                return assistantMessage.ToolCalls is not null && assistantMessage.ToolCalls.WhereName(toolName).Any();
            }
            
            var toolExecutionHistory = assistantMessage.GetToolExecutionHistory();
            return toolExecutionHistory
                .OfType<DialAssistantMessage>()
                .Any(am => am.ToolCalls is not null && am.ToolCalls.WhereName(toolName).Any());
        }

        public bool ToolWasCalled(string callingToolName, string calledToolName)
        {
            if (!assistantMessage.HasToolExecutionHistory())
                return false;
            
            return assistantMessage
                .GetToolExecutionHistoryMessages(callingToolName)
                .OfType<DialAssistantMessage>()
                .Any(m => m.ToolCalls is not null && m.ToolCalls.WhereName(calledToolName).Any());
        }
        
        public IEnumerable<BaseMessage> GetToolExecutionHistoryMessages()
        {
            var historyMessages = assistantMessage.GetToolExecutionHistory() ?? [];
            foreach (var historyMessage in historyMessages)
            {
                yield return historyMessage;
            }
        }
        
        public IEnumerable<BaseMessage> GetToolExecutionHistoryMessages(string toolName)
        {
            var historyMessages = assistantMessage.GetToolExecutionHistory() ?? [];
            var matchingToolCallIds = historyMessages
                .OfType<DialAssistantMessage>()
                .SelectMany(am => am.ToolCalls ?? [])
                .WhereName(toolName)
                .Select(tc => tc.Id);
            
            foreach (var toolCallId in matchingToolCallIds)
            {
                var toolCallHistoryMessages = historyMessages
                    .OfType<DialToolMessage>()
                    .Single(tm => tm.ToolCallId.Equals(toolCallId, StringComparison.OrdinalIgnoreCase))
                    .GetToolExecutionHistory();
            
                foreach (var historyMessage in toolCallHistoryMessages)
                {
                    yield return historyMessage;
                }
            }
        }
        
        public IEnumerable<string> GetCalledToolNames()
        {
            return assistantMessage.EnumerateCalledToolNames();
        }

        public IEnumerable<string> GetCalledDistinctToolNames()
        {
            return assistantMessage
                .EnumerateCalledToolNames()
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }
        
        public IReadOnlyList<BaseMessage> GetToolExecutionHistory()
            => assistantMessage.CustomContent?.State?.GetToolExecutionHistory() ?? [];
        
        private IEnumerable<string> EnumerateCalledToolNames()
        {
            if (!assistantMessage.HasToolExecutionHistory())
            {
                return assistantMessage.ToolCalls is not null
                    ? assistantMessage.ToolCalls.Select(tc => ToolHelpers.SanitizeToolName(tc.Function.Name))
                    : [];
            }
        
            var toolExecutionHistory = assistantMessage.GetToolExecutionHistory();
            return toolExecutionHistory
                .OfType<DialAssistantMessage>()
                .SelectMany(am => am.ToolCalls ?? [])
                .Select(tc => ToolHelpers.SanitizeToolName(tc.Function.Name))
                .Where(n => !Constants.QuickApps.SystemToolNames.Contains(n, StringComparer.OrdinalIgnoreCase));
        }
        
        private bool HasToolExecutionHistory() 
            => assistantMessage.CustomContent?.State?.HasToolExecutionHistory() ?? false;
    }
}
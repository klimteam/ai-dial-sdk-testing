using AiDialSdk.Testing.Core;
using AiDialSdk.Testing.Extensions;

namespace AiDialSdk.Testing.TestStrategies.Builders;

public abstract class BaseTestStrategyBuilder<TContext> where TContext : TestStrategyExecutionContext
{
    protected readonly List<CompletionCondition> CompletionConditions = [];
    protected readonly List<ChatActionCondition<LlmAgentTestStrategyExecutionContext>> ChatActionConditions = [];
    
    protected void WithToolCallByQuickAppCompletionInternal(string quickApp, string toolName)
    {
        CompletionConditions.Add(new CompletionCondition(
            context => context.LastDialAssistantMessageOrDefault()?.ToolWasCalledByQuickApp(quickApp, toolName) ?? false, 
            $"Tool '{toolName}' was called in the last assistant message"));
    }
    
    protected void WithVisualizerCompletionInternal(string visualizerType)
    {
        CompletionConditions.Add(new CompletionCondition(
            context =>
            {
                var lastAssistantMessage = context.LastDialAssistantMessageOrDefault();
                var dialAttachment = lastAssistantMessage?.CustomContent?.Attachments?.FirstOrDefault(a => 
                    a.Type is not null && a.Type.Equals(visualizerType, StringComparison.OrdinalIgnoreCase));
                return dialAttachment != null;
            }, 
            $"Visualizer '{visualizerType}' was found in the last assistant message"));
    }
}
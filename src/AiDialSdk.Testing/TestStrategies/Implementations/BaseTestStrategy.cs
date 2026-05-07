using AiDialSdk.Testing.Core;

namespace AiDialSdk.Testing.TestStrategies.Implementations;

public abstract class BaseTestStrategy<TContext> where TContext : TestStrategyExecutionContext
{
    private readonly IReadOnlyList<CompletionCondition> _completionConditions;
    
    protected BaseTestStrategy(
        IReadOnlyList<CompletionCondition> completionConditions)
    {
        _completionConditions = completionConditions;
    }
    
    protected void EvaluateCompletionConditions(TestStrategyExecutionContext context)
    {
        foreach (var condition in _completionConditions)
        {
            condition.Evaluate(context);
        }
    }
}
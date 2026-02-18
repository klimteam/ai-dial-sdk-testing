using AiDialSdk.Testing.Core;

namespace AiDialSdk.Testing.TestStrategies.Implementations;

public abstract class BaseTestStrategy<TContext> where TContext : TestStrategyExecutionContext
{
    private readonly IReadOnlyList<CompletionCondition> _completionConditions;
    private readonly IReadOnlyList<ChatActionCondition<TContext>> _chatActionConditions;
    
    protected BaseTestStrategy(
        IReadOnlyList<CompletionCondition> completionConditions, 
        IReadOnlyList<ChatActionCondition<TContext>> chatActionConditions)
    {
        _completionConditions = completionConditions;
        _chatActionConditions = chatActionConditions;
    }
    
    protected void EvaluateCompletionConditions(TestStrategyExecutionContext context)
    {
        foreach (var condition in _completionConditions)
        {
            condition.Evaluate(context);
        }
    }
    
    protected bool EvaluateChatActionConditions(TContext context)
    {
        var needToEvaluate = _chatActionConditions.Count(c => c.NeedToEvaluate(context));
        switch (needToEvaluate)
        {
            case 0:
                return false;
            case > 1:
                throw new InvalidOperationException("Multiple chat action conditions need to be evaluated at the same time, which is not supported.");
        }

        var conditionToEvaluate = _chatActionConditions.First(c => c.NeedToEvaluate(context));
        conditionToEvaluate.Evaluate(context);
        return true;
    }
}
namespace AiDialSdk.Testing.Core;

public class CompletionCondition
{
    private readonly Func<TestStrategyExecutionContext, bool> _condition;
    private readonly string _reason;

    public CompletionCondition(Func<TestStrategyExecutionContext, bool> condition, string reason)
    {
        _condition = condition;
        _reason = reason;
    }
    
    public void Evaluate(TestStrategyExecutionContext context)
    {
        if (_condition.Invoke(context))
            context.MarkComplete(_reason);
    }
}
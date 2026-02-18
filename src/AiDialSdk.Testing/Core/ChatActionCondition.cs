namespace AiDialSdk.Testing.Core;

public class ChatActionCondition<TContext> where TContext : TestStrategyExecutionContext
{
    private readonly Func<TContext, bool> _condition;
    private readonly Action<TContext> _action;

    public ChatActionCondition(Func<TContext, bool> condition, Action<TContext> action)
    {
        _condition = condition;
        _action = action;
    }
    
    public bool NeedToEvaluate(TContext context)
    {
        return _condition.Invoke(context);
    }
    
    public void Evaluate(TContext context)
    {
        if (NeedToEvaluate(context))
            _action.Invoke(context);
    }
}
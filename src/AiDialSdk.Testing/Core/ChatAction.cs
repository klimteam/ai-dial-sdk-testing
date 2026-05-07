namespace AiDialSdk.Testing.Core;

public class ChatAction<TContext> where TContext : TestStrategyExecutionContext
{
    private readonly Func<TContext, bool> _condition;
    private readonly Func<TContext, CancellationToken, Task<ChatActionResult>> _action;

    public ChatAction(
        Func<TContext, bool> condition, 
        Func<TContext, CancellationToken, 
        Task<ChatActionResult>> action)
    {
        _condition = condition;
        _action = action;
    }
    
    public bool NeedToEvaluate(TContext context)
    {
        return _condition.Invoke(context);
    }
    
    public async Task<ChatActionResult> EvaluateAsync(TContext context, CancellationToken token)
    {
        return await _action.Invoke(context, token);
    }
}
using AiDialSdk.Testing.TestStrategies.Implementations;

namespace AiDialSdk.Testing.TestStrategies.Builders;

public class OneMessageTestStrategyBuilder
{
    private string? _message;

    public OneMessageTestStrategyBuilder WithMessage(string message)
    {
        if (!string.IsNullOrWhiteSpace(_message))
            throw new InvalidOperationException("Message is already set.");
        
        _message = message;
        return this;
    }
    
    public OneMessageTestStrategy Build()
    {
        return string.IsNullOrWhiteSpace(_message)
            ? throw new InvalidOperationException("Message is not set.")
            : new OneMessageTestStrategy(_message);
    }
}
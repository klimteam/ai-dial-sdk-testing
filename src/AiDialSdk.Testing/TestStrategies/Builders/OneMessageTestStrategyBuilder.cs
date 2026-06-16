using AiDialSdk.Api.Data;
using AiDialSdk.Testing.TestStrategies.Implementations;

namespace AiDialSdk.Testing.TestStrategies.Builders;

public class OneMessageTestStrategyBuilder
{
    private string? _message;
    private List<DialAttachment>? _attachments;

    public OneMessageTestStrategyBuilder WithMessage(string message)
    {
        if (!string.IsNullOrWhiteSpace(_message))
            throw new InvalidOperationException("Message is already set.");
        
        _message = message;
        return this;
    }

    public OneMessageTestStrategyBuilder WithAttachment(string type, string title, string url)
    {
        _attachments ??= [];
        _attachments.Add(new DialAttachment(type, title, url));

        return this;
    }
    
    public OneMessageTestStrategy Build()
    {
        return string.IsNullOrWhiteSpace(_message)
            ? throw new InvalidOperationException("Message is not set.")
            : new OneMessageTestStrategy(_message, _attachments);
    }
}
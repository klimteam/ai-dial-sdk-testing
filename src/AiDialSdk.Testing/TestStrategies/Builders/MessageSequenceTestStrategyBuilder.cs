using AiDialSdk.Api.OpenAi.Data;
using AiDialSdk.Testing.TestStrategies.Implementations;

namespace AiDialSdk.Testing.TestStrategies.Builders;

public class MessageSequenceTestStrategyBuilder
{
    private BaseMessage[] _messages = [];
    
    public MessageSequenceTestStrategyBuilder WithMessageSequence(IEnumerable<BaseMessage> messages)
    {
        if (_messages.Length > 0)
            throw new InvalidOperationException("Message sequence is already set.");
        
        _messages = messages.ToArray();
        return this;
    }
    
    public MessageSequenceTestStrategy Build()
    {
        return _messages.Length == 0
            ? throw new InvalidOperationException("Message sequence is not set.")
            : new MessageSequenceTestStrategy(_messages);
    }
}
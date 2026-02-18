using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal.Commands;

namespace AiDialSdk.Testing.NUnit;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class RepeatWithSuccessRateAttribute : Attribute, IWrapSetUpTearDown
{
    private readonly int _totalRuns;
    private readonly int _requiredSuccesses;

    public RepeatWithSuccessRateAttribute(int totalRuns, int requiredSuccesses)
    {
        _totalRuns = totalRuns;
        _requiredSuccesses = requiredSuccesses;
    }
    
    public TestCommand Wrap(TestCommand command)
    {
        return new RepeatWithSuccessRateCommand(
            command,
            _totalRuns,
            _requiredSuccesses);
    }
}
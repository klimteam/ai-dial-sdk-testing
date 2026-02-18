using System.Text;
using NUnit.Framework.Interfaces;
using NUnit.Framework.Internal;
using NUnit.Framework.Internal.Commands;

namespace AiDialSdk.Testing.NUnit;

public class RepeatWithSuccessRateCommand : DelegatingTestCommand
{
    private readonly int _totalRuns;
    private readonly int _requiredSuccesses;

    public RepeatWithSuccessRateCommand(TestCommand innerCommand, int totalRuns, int requiredSuccesses)
        : base(innerCommand)
    {
        _totalRuns = totalRuns;
        _requiredSuccesses = requiredSuccesses;
    }

    public override TestResult Execute(TestExecutionContext context)
    {
        int successCount = 0;
        var allAttempts = new List<(int attemptNumber, bool success, string? errorMessage, string? stackTrace)>();
        
        var finalResult = context.CurrentTest.MakeTestResult();
        context.CurrentResult = finalResult;

        try
        {
            for (int i = 0; i < _totalRuns; i++)
            {
                var attemptResult = context.CurrentTest.MakeTestResult();

                try
                {
                    context.CurrentResult = attemptResult;
                    innerCommand.Execute(context);
                    context.CurrentResult = finalResult;

                    if (attemptResult.ResultState == ResultState.Success)
                    {
                        successCount++;
                        allAttempts.Add((i + 1, true, null, null));
                    }
                    else
                    {
                        allAttempts.Add((
                            i + 1, 
                            false, 
                            attemptResult.Message, 
                            attemptResult.StackTrace));
                    }
                }
                catch (Exception ex)
                {
                    context.CurrentResult = finalResult;
                    
                    attemptResult.RecordException(ex);
                    allAttempts.Add((
                        i + 1, 
                        false, 
                        string.IsNullOrWhiteSpace(attemptResult.Message) ? ex.Message : attemptResult.Message, 
                        string.IsNullOrWhiteSpace(attemptResult.StackTrace) ? ex.StackTrace : attemptResult.StackTrace));
                }
            }
        }
        finally
        {
            context.CurrentResult = finalResult;
        }

        var messageBuilder = new StringBuilder();
        messageBuilder.AppendLine($"Passed {successCount}/{_totalRuns} runs (required {_requiredSuccesses})");
        messageBuilder.AppendLine();
        
        foreach (var (attemptNumber, success, errorMessage, stackTrace) in allAttempts)
        {
            if (success)
            {
                messageBuilder.AppendLine($"--- Attempt {attemptNumber} successful ---");
            }
            else
            {
                messageBuilder.AppendLine($"--- Attempt {attemptNumber} failed ---");

                if (!string.IsNullOrWhiteSpace(errorMessage))
                    messageBuilder.AppendLine(errorMessage);

                if (!string.IsNullOrWhiteSpace(stackTrace))
                {
                    messageBuilder.AppendLine("Stack Trace:");
                    messageBuilder.AppendLine(stackTrace);
                }
            }

            messageBuilder.AppendLine();
        }

        finalResult.SetResult(
            successCount >= _requiredSuccesses ? ResultState.Success : ResultState.Failure,
            messageBuilder.ToString().TrimEnd());

        return finalResult;
    }
}
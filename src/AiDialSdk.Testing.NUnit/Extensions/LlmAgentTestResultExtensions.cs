using AiDialSdk.Testing.TestStrategies.Models;
using NUnit.Framework;

namespace AiDialSdk.Testing.NUnit.Extensions;

public static class LlmAgentTestResultExtensions
{
    public static void AssertThatTestCasePassed(this LlmAgentTestResult testResult)
    {
        if (testResult.TestCasePassed is null)
        {
            Assert.Fail($"Test case failed. Complete reasons: '{string.Join("; ", testResult.CompleteReasons)}'.");
        }
        else
        {
            Assert.That(testResult.TestCasePassed, Is.Not.Null, 
                "Expected test case pass status to be not null but it was null.");
        
            Assert.That(
                testResult.TestCasePassed ?? false, 
                Is.True,
                $"Expected behavior: {testResult.TestCaseExpectedBehavior ?? "Not provided"}. Actual behavior: {testResult.TestCaseActualBehavior ?? "Not provided"}");
        }
    }

    public static void WriteTestCaseStatusAsync(this LlmAgentTestResult testResult)
    {
        switch (testResult.TestCasePassed)
        {
            case false:
                TestContext.Out.WriteLine($"Test case failed {testResult.TestCaseActualBehavior ?? "Not provided"}. Expected behavior: {testResult.TestCaseExpectedBehavior ?? "Not provided"}");
                break;
            case true:
                TestContext.Out.WriteLine($"Test case passed.");
                break;
        }
    }
}
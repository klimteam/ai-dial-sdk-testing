using AiDialSdk.Api.OpenAi.Data;
using AiDialSdk.Testing.Extensions;
using NUnit.Framework;

namespace AiDialSdk.Testing.NUnit.Extensions;

public static class BaseMessageEnumerableExtensions
{
    public static void AssertToolWasCalled(this IEnumerable<BaseMessage> messages, string toolName)
    {
        Assert.That(messages.ToolWasCalled(toolName), Is.True, $"Expected tool `{toolName}` to be called.");
    }
    
    public static void AssertToolWasCalled(this IEnumerable<BaseMessage> messages, string callingToolName, string calledToolName)
    {
        Assert.That(messages.ToolWasCalled(callingToolName, calledToolName), Is.True, $"Expected tool `{calledToolName}` to be called by `{callingToolName}`.");
    }
    
    public static void AssertToolWasNotCalled(this IEnumerable<BaseMessage> messages, string toolName)
    {
        Assert.That(messages.ToolWasCalled(toolName), Is.False, $"Expected tool `{toolName}` to not be called.");
    }

    public static void AssertCalledToolsAreSubsetOf(this IEnumerable<BaseMessage> messages, string[] expectedToolNames)
    {
        var calledQuickAppNames = messages.GetCalledDistinctToolNames().ToArray();
        Assert.That(
            calledQuickAppNames, 
            Is.SubsetOf(expectedToolNames), 
            $"Expected only {string.Join(", ", expectedToolNames)} to be called, but found {string.Join(", ", calledQuickAppNames)}.");

    }
    
    public static void AssertToolWasCalledOnce(this IEnumerable<BaseMessage> messages, string toolName)
    {
        var calledToolNames = messages.GetCalledToolNames();
        var callCount = calledToolNames.Count(name => name == toolName);
        
        Assert.That(callCount, Is.EqualTo(1), $"Expected tool `{toolName}` to be called once, but it was called {callCount} times.");
    }
    
    public static void AssertOnlySpecifiedToolWasCalled(this IEnumerable<BaseMessage> messages, string toolName)
    {
        var calledToolNames = messages.GetCalledDistinctToolNames().ToArray();
        
        Assert.That(calledToolNames.Count, Is.EqualTo(1), $"Expected only one tool to be called, but found {calledToolNames.Length}: {string.Join(", ", calledToolNames)}.");
        Assert.That(calledToolNames.First(), Is.EqualTo(toolName), $"Expected the only called tool to be `{toolName}`, but it was `{calledToolNames.First()}`.");
    }
}
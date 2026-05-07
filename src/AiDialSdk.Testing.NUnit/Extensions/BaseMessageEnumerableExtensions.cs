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

    public static void AssertToolsWhereNotCalled(this IEnumerable<BaseMessage> messages)
    {
        var calledToolNames = messages
            .GetCalledDistinctToolNames()
            .ToArray();
        
        Assert.That(
            calledToolNames, 
            Is.Empty, 
            $"Expected no tools to be called, but found the following called tools: {string.Join(", ", calledToolNames)}.");
    }
    
    public static void AssertCalledToolsAreSubsetOf(this IEnumerable<BaseMessage> messages, params string[] expectedToolNames)
    {
        var calledQuickAppNames = messages
            .GetCalledDistinctToolNames()
            .ToArray();
        
        Assert.That(
            calledQuickAppNames, 
            Is.SubsetOf(expectedToolNames), 
            $"Expected only {string.Join(", ", expectedToolNames)} to be called, but found {string.Join(", ", calledQuickAppNames)}.");

    }
    
    public static void AssertToolWasCalledOnce(this IEnumerable<BaseMessage> messages, string toolName)
    {
        var calledToolNames = messages
            .GetCalledToolNames();
        
        var callCount = calledToolNames.Count(name => name == toolName);
        
        Assert.That(callCount, Is.EqualTo(1), $"Expected tool `{toolName}` to be called once, but it was called {callCount} times.");
    }
    
    public static void AssertOnlySpecifiedToolWasCalled(this IEnumerable<BaseMessage> messages, string toolName)
    {
        var calledToolNames = messages
            .GetCalledDistinctToolNames()
            .ToArray();
        
        Assert.That(calledToolNames.Count, Is.EqualTo(1), $"Expected only one tool to be called, but found {calledToolNames.Length}: {string.Join(", ", calledToolNames)}.");
        Assert.That(calledToolNames.First(), Is.EqualTo(toolName), $"Expected the only called tool to be `{toolName}`, but it was `{calledToolNames.First()}`.");
    }
    
    public static void AssertToolWasCalledBefore(this IEnumerable<BaseMessage> messages, 
        string callingToolName, string firstToolName, string secondToolName)
    {
        var toolMessages = messages.GetToolExecutionHistoryMessages(callingToolName).ToArray();
        
        var calledToolNames = toolMessages
            .GetCalledToolNames()
            .ToArray();
        
        var firstToolIndex = Array.IndexOf(calledToolNames, firstToolName);
        var secondToolIndex = Array.IndexOf(calledToolNames, secondToolName);
        
        Assert.That(firstToolIndex, Is.GreaterThan(-1), $"Expected tool `{firstToolName}` to be called, but it was not found in the messages.");
        Assert.That(secondToolIndex, Is.GreaterThan(-1), $"Expected tool `{secondToolName}` to be called, but it was not found in the messages.");
        Assert.That(firstToolIndex, Is.LessThan(secondToolIndex), $"Expected tool `{firstToolName}` to be called before `{secondToolName}`, but it was called at index {firstToolIndex} and `{secondToolName}` was called at index {secondToolIndex}.");
    }
    
    public static void AssertToolWasCalledBefore(this IEnumerable<BaseMessage> messages, string firstToolName, string secondToolName)
    {
        var toolMessages = messages
            .GetToolExecutionHistoryMessages()
            .ToArray();
        
        var calledToolNames = toolMessages
            .GetCalledToolNames()
            .ToArray();
        
        var firstToolIndex = Array.IndexOf(calledToolNames, firstToolName);
        var secondToolIndex = Array.IndexOf(calledToolNames, secondToolName);
        
        Assert.That(firstToolIndex, Is.GreaterThan(-1), $"Expected tool `{firstToolName}` to be called, but it was not found in the messages.");
        Assert.That(secondToolIndex, Is.GreaterThan(-1), $"Expected tool `{secondToolName}` to be called, but it was not found in the messages.");
        Assert.That(firstToolIndex, Is.LessThan(secondToolIndex), $"Expected tool `{firstToolName}` to be called before `{secondToolName}`, but it was called at index {firstToolIndex} and `{secondToolName}` was called at index {secondToolIndex}.");
    }
}
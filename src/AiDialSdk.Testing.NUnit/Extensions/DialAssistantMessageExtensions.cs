using AiDialSdk.Api.Data;
using AiDialSdk.Testing.Extensions;
using NUnit.Framework;

namespace AiDialSdk.Testing.NUnit.Extensions;

public static class DialAssistantMessageExtensions
{
    public static void AssertToolWasCalled(this DialAssistantMessage dialAssistantMessage, string toolName)
    {
        Assert.That(dialAssistantMessage.ToolWasCalled(toolName), Is.True, $"Expected tool `{toolName}` to be called.");
    }

    public static void AssertToolWasCalled(this DialAssistantMessage assistantMessage, string callingToolName, string calledToolName)
    {
        var toolCalled = assistantMessage.ToolWasCalled(callingToolName, calledToolName);
        Assert.That(toolCalled, Is.True, $"Expected tool `{calledToolName}` to be called by tool `{callingToolName}`.");
    }
    
    public static void AssertToolWasNotCalled(this DialAssistantMessage dialAssistantMessage, string toolName)
    {
        Assert.That(dialAssistantMessage.ToolWasCalled(toolName), Is.False, $"Expected tool `{toolName}` to not be called.");
    }
    
    public static void AssertToolWasCalledOnce(this DialAssistantMessage dialAssistantMessage, string toolName)
    {
        var toolExecutionHistory = dialAssistantMessage.GetToolExecutionHistory();
        var toolCallCount = toolExecutionHistory
            .OfType<DialAssistantMessage>()
            .SelectMany(am => am.ToolCalls ?? [])
            .WhereName(toolName)
            .Count();
        
        Assert.That(toolCallCount, Is.EqualTo(1), $"Expected tool `{toolName}` to be called once, but it was called {toolCallCount} times.");
    }
    
    public static void AssertOnlySpecifiedToolWasCalled(this DialAssistantMessage dialAssistantMessage, string toolName)
    {
        var calledToolNames = dialAssistantMessage.GetCalledDistinctToolNames().ToArray();
        
        Assert.That(calledToolNames.Count, Is.EqualTo(1), $"Expected only one tool name to be called, but found {calledToolNames.Count()}: {string.Join(", ", calledToolNames)}.");
        Assert.That(calledToolNames.First(), Is.EqualTo(toolName), $"Expected the only called tool name to be `{toolName}`, but it was `{calledToolNames.First()}`.");
    }
}
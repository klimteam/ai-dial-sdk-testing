using AiDialSdk.Api.Data;
using AiDialSdk.Testing.Extensions;
using NUnit.Framework;

namespace AiDialSdk.Testing.NUnit.Extensions;

public static class DialAssistantMessageExtensions
{
    public static void AssertQuickAppWasCalled(this DialAssistantMessage dialAssistantMessage, string quickAppName)
    {
        Assert.That(dialAssistantMessage.QuickAppWasCalled(quickAppName), Is.True, $"Expected quick app `{quickAppName}` to be called.");
    }

    public static void AssertToolWasCalledByQuickApp(this DialAssistantMessage assistantMessage, string quickAppName, string toolName)
    {
        var toolCalled = assistantMessage
            .QuickAppMessages(quickAppName)
            .OfType<DialAssistantMessage>()
            .Any(m => m.ToolCalls is not null && m.ToolCalls.WhereName(toolName).Any());
        
        Assert.That(toolCalled, Is.True, $"Expected tool `{toolName}` to be called by quick app `{quickAppName}`.");
    }
    
    public static void AssertQuickAppWasCalledOnce(this DialAssistantMessage dialAssistantMessage, string quickAppName)
    {
        var quickAppExecutionHistory = dialAssistantMessage.GetToolExecutionHistory();
        var quickAppCallCount = quickAppExecutionHistory
            .OfType<DialAssistantMessage>()
            .SelectMany(am => am.ToolCalls ?? [])
            .WhereName(quickAppName)
            .Count();
        
        Assert.That(quickAppCallCount, Is.EqualTo(1), $"Expected quick app `{quickAppName}` to be called once, but it was called {quickAppCallCount} times.");
    }
    
    public static void AssertOnlySpecifiedQuickAppWasCalled(this DialAssistantMessage dialAssistantMessage, string quickAppName)
    {
        var calledQuickApps = dialAssistantMessage.GetCalledQuickAppHistoryNames().ToArray();
        
        Assert.That(calledQuickApps.Count, Is.EqualTo(1), $"Expected only one quick app to be called, but found {calledQuickApps.Count()}: {string.Join(", ", calledQuickApps)}.");
        Assert.That(calledQuickApps.First(), Is.EqualTo(quickAppName), $"Expected the only called quick app to be `{quickAppName}`, but it was `{calledQuickApps.First()}`.");
    }
}
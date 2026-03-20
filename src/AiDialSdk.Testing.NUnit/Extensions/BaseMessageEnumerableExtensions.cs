using AiDialSdk.Api.OpenAi.Data;
using AiDialSdk.Testing.Extensions;
using NUnit.Framework;

namespace AiDialSdk.Testing.NUnit.Extensions;

public static class BaseMessageEnumerableExtensions
{
    public static void AssertQuickAppWasCalled(this IEnumerable<BaseMessage> messages, string quickAppName)
    {
        Assert.That(messages.QuickAppWasCalled(quickAppName), Is.True, $"Expected quick app `{quickAppName}` to be called.");
    }
    
    public static void AssertQuickAppWasCalledOnce(this IEnumerable<BaseMessage> messages, string quickAppName)
    {
        var calledQuickAppHistoryNames = messages.GetCalledQuickAppHistoryNames();
        var callCount = calledQuickAppHistoryNames.Count(name => name == quickAppName);
        
        Assert.That(callCount, Is.EqualTo(1), $"Expected quick app `{quickAppName}` to be called once, but it was called {callCount} times.");
    }
    
    public static void AssertOnlySpecifiedQuickAppWasCalled(this IEnumerable<BaseMessage> messages, string quickAppName)
    {
        var calledQuickApps = messages.GetCalledQuickAppNames().ToArray();
        
        Assert.That(calledQuickApps.Count, Is.EqualTo(1), $"Expected only one quick app to be called, but found {calledQuickApps.Count()}: {string.Join(", ", calledQuickApps)}.");
        Assert.That(calledQuickApps.First(), Is.EqualTo(quickAppName), $"Expected the only called quick app to be `{quickAppName}`, but it was `{calledQuickApps.First()}`.");
    }
}
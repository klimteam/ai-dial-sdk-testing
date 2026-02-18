using AiDialSdk.Api.Chat;
using AiDialSdk.Testing.TestStrategies.Models;

namespace AiDialSdk.Testing.TestStrategies;

public interface ITestStrategy
{
    Task<TestStrategyResult> RunAsync(IDialChatApiClient chatClient, CancellationToken token);
}
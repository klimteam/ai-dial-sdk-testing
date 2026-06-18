using AiDialSdk.Api.Chat;
using AiDialSdk.Api.Files;
using AiDialSdk.Testing.TestStrategies.Models;

namespace AiDialSdk.Testing.TestStrategies;

public interface ITestStrategy
{
    Task<TestResult> RunAsync(IDialChatApiClient chatClient, IDialFileApiClient fileClient, CancellationToken token);
}
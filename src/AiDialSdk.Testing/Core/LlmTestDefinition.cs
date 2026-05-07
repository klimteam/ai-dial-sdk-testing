using AiDialSdk.Api.Chat;
using AiDialSdk.Api.Files;
using AiDialSdk.Testing.TestStrategies;
using AiDialSdk.Testing.TestStrategies.Models;

namespace AiDialSdk.Testing.Core;

public class LlmTestDefinition
{
    private readonly IDialChatApiClient _chatClient;
    private readonly IDialFileApiClient _fileClient;
    private readonly ITestStrategy _testStrategy;
    private readonly IReadOnlyList<WebApplicationFactoryBuilder> _webApplicationFactoryBuilders;
    
    public LlmTestDefinition(
        IDialChatApiClient chatClient,
        IDialFileApiClient fileClient,
        ITestStrategy testStrategy,
        IReadOnlyList<WebApplicationFactoryBuilder> webApplicationFactoryBuilders)
    {
        _chatClient = chatClient;
        _fileClient = fileClient;
        _testStrategy = testStrategy;
        _webApplicationFactoryBuilders = webApplicationFactoryBuilders;
    }

    public async Task<TestResult> RunAsync(CancellationToken token = default)
    {
        var webApplications = BuildWebApplicationFactories();
        try
        {
            await StartWebApplicationsAsync(webApplications);
            
            // Give the web applications some time to start before running the test strategy.
            // This is a simple approach and can be improved by implementing a more robust health check mechanism.
            await Task.Delay(2000, token);
            
            var testStrategyResult = await _testStrategy.RunAsync(_chatClient, _fileClient, token);

            return testStrategyResult;
        }
        finally
        {
            foreach (var webApplicationFactory in webApplications)
            {
                webApplicationFactory.Dispose();
            }
        }
    }
    
    private List<IWebApplicationFactory> BuildWebApplicationFactories()
    {
        var webApplications = new List<IWebApplicationFactory>();
        foreach (var webApplicationFactoryBuilder in _webApplicationFactoryBuilders)
        {
            webApplications.Add(webApplicationFactoryBuilder.Build());
        }

        return webApplications;
    }
    
    private static async Task StartWebApplicationsAsync(IReadOnlyList<IWebApplicationFactory> webApplications)
    {
        foreach (var webApplication in webApplications)
        {
            webApplication.StartServer();
        }

        await Task.CompletedTask;
    }
}
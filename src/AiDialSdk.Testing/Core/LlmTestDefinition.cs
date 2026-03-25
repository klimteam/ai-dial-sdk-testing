using AiDialSdk.Api.Chat;
using AiDialSdk.Testing.TestStrategies;
using AiDialSdk.Testing.TestStrategies.Models;

namespace AiDialSdk.Testing.Core;

public class LlmTestDefinition
{
    private readonly IDialChatApiClient _chatClient;
    private readonly ITestStrategy _testStrategy;
    private readonly IReadOnlyList<WebApplicationFactoryBuilder> _webApplicationFactoryBuilders;
    
    public LlmTestDefinition(
        IDialChatApiClient chatClient,
        ITestStrategy testStrategy,
        IReadOnlyList<WebApplicationFactoryBuilder> webApplicationFactoryBuilders)
    {
        _chatClient = chatClient;
        _testStrategy = testStrategy;
        _webApplicationFactoryBuilders = webApplicationFactoryBuilders;
    }

    public async Task<TestResult> RunAsync(CancellationToken token = default)
    {
        var webApplications = BuildWebApplicationFactories();
        try
        {
            await StartWebApplicationsAsync(webApplications);
            var testStrategyResult = await _testStrategy.RunAsync(_chatClient, token);

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
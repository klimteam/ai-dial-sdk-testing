using AiDialSdk.Api.Chat;
using AiDialSdk.Api.Chat.Implementations;
using AiDialSdk.Testing.TestStrategies;
using AiDialSdk.Testing.TestStrategies.Builders;
using Microsoft.AspNetCore.Hosting;

namespace AiDialSdk.Testing.Core;

public class LlmTestDefinitionBuilder
{
    private readonly List<WebApplicationFactoryBuilder> _webApplicationFactoryBuilders = [];
    
    private readonly IDialChatApiClient _chatClient;

    private ITestStrategy? _testStrategy;
    
    public LlmTestDefinitionBuilder(string deploymentName, string endpoint, string apiKey)
    {
        _chatClient = new DialChatApiClient(new HttpClient(), new Uri(endpoint), apiKey, deploymentName, null);
    }
    
    public LlmTestDefinitionBuilder WithWebApplication<TEntryPoint>(Action<IWebHostBuilder> webHostBuilder, int? port)
        where TEntryPoint : class
    {
        _webApplicationFactoryBuilders.Add(new WebApplicationFactoryBuilder<TEntryPoint>(webHostBuilder, port));
        return this;
    }

    public LlmTestDefinitionBuilder WithLlmAgentTestStrategy(Action<LlmAgentTestStrategyBuilder> configure)
    {
        if (_testStrategy != null)
            throw new InvalidOperationException("Test strategy is already configured.");
        
        var builder = new LlmAgentTestStrategyBuilder();
        configure(builder);
        _testStrategy = builder.Build();
        return this;
    }
    
    public LlmTestDefinitionBuilder WithOneMessageTestStrategy(Action<OneMessageTestStrategyBuilder> configure)
    {
        if (_testStrategy != null)
            throw new InvalidOperationException("Test strategy is already configured.");
        
        var builder = new OneMessageTestStrategyBuilder();
        configure(builder);
        _testStrategy = builder.Build();
        return this;
    }
    
    public LlmTestDefinition Build()
    {
        if (_testStrategy is null)
            throw new InvalidOperationException("Test strategy is not configured.");
        
        return new LlmTestDefinition(_chatClient, _testStrategy, _webApplicationFactoryBuilders);
    }
}
using AiDialSdk.Api.Chat.Implementations;
using AiDialSdk.Api.Clients.Implementations;
using AiDialSdk.Api.Files.Implementations;
using AiDialSdk.Testing.TestStrategies;
using AiDialSdk.Testing.TestStrategies.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace AiDialSdk.Testing.Core;

public class LlmTestDefinitionBuilder
{
    private const string LlmTestDefinitionConfigurationKey = "LlmTestDefinitionConfiguration";
    
    internal const string EndpointConfigurationKey = $"{LlmTestDefinitionConfigurationKey}:Endpoint";
    internal const string ApiKeyConfigurationKey = $"{LlmTestDefinitionConfigurationKey}:ApiKey";
    
    private readonly string _deploymentName;
    private readonly string? _endpoint;
    private readonly string? _apiKey;
    
    private readonly IConfiguration _configuration;
    
    private readonly List<WebApplicationFactoryBuilder> _webApplicationFactoryBuilders = [];
    private ITestStrategy? _testStrategy;
    
    public LlmTestDefinitionBuilder(string deploymentName, string? endpoint = null, string? apiKey = null)
    {
        _deploymentName = deploymentName;
        _endpoint = endpoint;
        _apiKey = apiKey;
        _configuration = BuildConfiguration();
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
        
        var builder = new LlmAgentTestStrategyBuilder(_configuration);
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
    
    public LlmTestDefinitionBuilder WithMessageSequenceTestStrategy(Action<MessageSequenceTestStrategyBuilder> configure)
    {
        if (_testStrategy != null)
            throw new InvalidOperationException("Test strategy is already configured.");
        
        var builder = new MessageSequenceTestStrategyBuilder();
        configure(builder);
        _testStrategy = builder.Build();
        return this;
    }
    
    public LlmTestDefinition Build()
    {
        var httpClient = new HttpClient();
        
        var chatClient = BuildChatClient(httpClient);
        var fileClient = BuildFileClient(httpClient);
        
        return _testStrategy is null 
            ? throw new InvalidOperationException("Test strategy is not configured.") 
            : new LlmTestDefinition(chatClient, fileClient, _testStrategy, _webApplicationFactoryBuilders);
    }

    private DialFileApiClient BuildFileClient(HttpClient httpClient)
    {
        return new DialFileApiClient(httpClient, new Uri(GetEndpoint()), GetApiKey());
    }

    
    private DialChatApiClient BuildChatClient(HttpClient httpClient)
    {
        return new DialChatApiClient(httpClient, new Uri(GetEndpoint()), GetApiKey(), _deploymentName, null);
    }
    
    private static IConfiguration BuildConfiguration()
    {
        return new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();
    }
    
    private string GetEndpoint()
    {
        var endpoint = _configuration.GetValue<string>(EndpointConfigurationKey);
        return endpoint ?? _endpoint ?? throw new InvalidOperationException(
            $"Endpoint must be provided either through configuration key '{EndpointConfigurationKey}' or constructor parameter.");
    }

    private string GetApiKey()
    {
        var apiKey = _configuration.GetValue<string>(ApiKeyConfigurationKey);
        return apiKey ?? _apiKey ?? throw new InvalidOperationException(
            $"API key must be provided either through configuration key '{ApiKeyConfigurationKey}' or constructor parameter.");
    }
}
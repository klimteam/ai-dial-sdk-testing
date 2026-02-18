using Microsoft.AspNetCore.Mvc.Testing;

namespace AiDialSdk.Testing.Core;

public class WebApplicationFactoryWrapper<TEntryPoint> : IWebApplicationFactory where TEntryPoint : class
{
    private readonly WebApplicationFactory<TEntryPoint> _factory;
    private readonly WebApplicationFactory<TEntryPoint> _delegatedFactory;

    public WebApplicationFactoryWrapper(
        WebApplicationFactory<TEntryPoint> factory, 
        WebApplicationFactory<TEntryPoint> delegatedFactory)
    {
        _factory = factory;
        _delegatedFactory = delegatedFactory;
    }
    
    public void StartServer() => _delegatedFactory.StartServer();

    public void Dispose()
    {
        _factory.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _factory.DisposeAsync();
    }
}
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace AiDialSdk.Testing.Core;

public abstract class WebApplicationFactoryBuilder
{
    public abstract IWebApplicationFactory Build();
}

public class WebApplicationFactoryBuilder<TEntryPoint> : WebApplicationFactoryBuilder 
    where TEntryPoint : class
{
    private readonly Action<IWebHostBuilder> _webHostBuilder;
    private readonly int? _port;

    public WebApplicationFactoryBuilder(Action<IWebHostBuilder> webHostBuilder, int? port)
    {
        _webHostBuilder = webHostBuilder;
        _port = port;
    }
    
    public override IWebApplicationFactory Build()
    {
        var factory = new WebApplicationFactory<TEntryPoint>();
        
        if (_port.HasValue)
            factory.UseKestrel(_port.Value);
        else
            factory.UseKestrel();
        
        var delegatedFactory = factory.WithWebHostBuilder(_webHostBuilder);
        
        if (_port.HasValue)
            delegatedFactory.UseKestrel(_port.Value);
        else
            delegatedFactory.UseKestrel();
        
        return new WebApplicationFactoryWrapper<TEntryPoint>(factory, delegatedFactory);
    }
}
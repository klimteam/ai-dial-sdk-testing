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

    public WebApplicationFactoryBuilder(Action<IWebHostBuilder> webHostBuilder)
    {
        _webHostBuilder = webHostBuilder;
    }
    
    public override IWebApplicationFactory Build()
    {
        var factory = new WebApplicationFactory<TEntryPoint>();
        
        var delegatedFactory = factory.WithWebHostBuilder(_webHostBuilder); 
        delegatedFactory.UseKestrel();
        
        return new WebApplicationFactoryWrapper<TEntryPoint>(factory, delegatedFactory);
    }
}
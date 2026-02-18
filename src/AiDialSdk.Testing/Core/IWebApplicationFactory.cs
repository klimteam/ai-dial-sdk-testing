namespace AiDialSdk.Testing.Core;

public interface IWebApplicationFactory : IDisposable, IAsyncDisposable
{
    void StartServer();
}
using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.Mediator.Tests;

[TestClass]
public class ConfigureServicesTests
{
    [TestMethod]
    public async Task AddMediatorServicesRegistersCoreDependenciesAndResolvesHandlers()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediatorServices();
        services.AddTransient<IRequestHandler<PingRequest, string>, PingHandler>();
        var provider = services.BuildServiceProvider();

        // Act
        var sender = provider.GetRequiredService<ISender>();
        var dispatcher = provider.GetRequiredService<IRequestDispatcher>();
        var handler = provider.GetRequiredService<IRequestHandler<PingRequest, string>>();
        var response = await sender.Send(new PingRequest(), CancellationToken.None);

        // Assert
        Assert.IsNotNull(sender);
        Assert.IsNotNull(dispatcher);
        Assert.IsNotNull(handler);
        Assert.AreEqual("pong", response);
    }

    public record PingRequest() : IRequest<string>;

    public class PingHandler : IRequestHandler<PingRequest, string>
    {
        public Task<string> Handle(PingRequest request, CancellationToken cancellationToken)
            => Task.FromResult("pong");
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.Mediator.Tests;

[TestClass]
public class RequestDispatcherTests
{
    [TestMethod]
    public async Task SendCommandHandlerIsCalled()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestCommand>, TestCommandHandler>();
        var provider = services.BuildServiceProvider();
        var dispatcher = new RequestDispatcher(provider);

        var command = new TestCommand();

        // Act
        await dispatcher.Send(command);

        // Assert
        Assert.IsTrue(command.Handled);
    }

    [TestMethod]
    public async Task SendQueryHandlerReturnsResponse()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestQuery, string>, TestQueryHandler>();
        var provider = services.BuildServiceProvider();
        var dispatcher = new RequestDispatcher(provider);

        var query = new TestQuery();

        // Act
        var result = await dispatcher.Send<string>(query);

        // Assert
        Assert.AreEqual("response", result);
    }
}

// Test fakes
public class TestCommand : IRequest { public bool Handled { get; set; } }
public class TestCommandHandler : IRequestHandler<TestCommand>
{
    public Task Handle(TestCommand request, CancellationToken cancellationToken)
    {
        request.Handled = true;
        return Task.CompletedTask;
    }
}
public class TestQuery : IRequest<string> { }
public class TestQueryHandler : IRequestHandler<TestQuery, string>
{
    public Task<string> Handle(TestQuery request, CancellationToken cancellationToken) => Task.FromResult("response");
}
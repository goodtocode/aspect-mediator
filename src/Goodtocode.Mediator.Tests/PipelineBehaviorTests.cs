using Microsoft.Extensions.DependencyInjection;

namespace Goodtocode.Mediator.Tests;

[TestClass]
public class PipelineBehaviorTests
{
    [TestMethod]
    public void AddMediatorServicesRegistersCoreServices()
    {
        // Arrange
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddMediatorServices();
        var provider = services.BuildServiceProvider();

        // Act
        var sender = provider.GetService<ISender>();
        var dispatcher = provider.GetService<IRequestDispatcher>();

        // Assert
        Assert.IsNotNull(sender);
        Assert.IsNotNull(dispatcher);
    }

    [TestMethod]
    public async Task PipelineBehaviorInvokesNext()
    {
        // Arrange
        var behavior = new TestBehavior();
        bool nextCalled = false;

        // Act
        await behavior.Handle(
            new TestCommand(),
            () => { nextCalled = true; return Task.CompletedTask; },
            CancellationToken.None);

        // Assert
        Assert.IsTrue(nextCalled);
        Assert.IsTrue(behavior.Called);
    }

    class TestBehavior : IPipelineBehavior<TestCommand>
    {
        public bool Called { get; private set; }
        public Task Handle(TestCommand request, RequestDelegateInvoker next, CancellationToken cancellationToken)
        {
            Called = true;
            return next();
        }
    }

    [TestMethod]
    public async Task HandlerResolutionViaSenderWorks()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediatorServices();
        services.AddTransient<IRequestHandler<EchoRequest, string>, EchoHandler>();
        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        // Act
        var result = await sender.Send(new EchoRequest("hello"), TestContext.CancellationToken);

        // Assert
        Assert.AreEqual("hello", result);
    }

    public record EchoRequest(string Message) : IRequest<string>;

    class EchoHandler : IRequestHandler<EchoRequest, string>
    {
        public Task<string> Handle(EchoRequest request, CancellationToken cancellationToken)
            => Task.FromResult(request.Message);
    }

    [TestMethod]
    public async Task SendThrowsWhenNoHandlerRegistered()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMediatorServices();
        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await sender.Send(new NoHandlerRequest(), TestContext.CancellationToken);
        });
    }

    public record NoHandlerRequest() : IRequest<string>;

    [TestMethod]
    public async Task PipelineBehaviorThrowsExceptionIsPropagated()
    {
        // Arrange
        var behavior = new ExceptionBehavior();
        var exception = new InvalidOperationException("Test exception");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await behavior.Handle(
                new TestCommand(),
                () => throw exception,
                CancellationToken.None);
        });
        Assert.AreEqual("Test exception", ex.Message);
    }

    class ExceptionBehavior : IPipelineBehavior<TestCommand>
    {
        public Task Handle(TestCommand request, RequestDelegateInvoker next, CancellationToken cancellationToken)
        {
            return next();
        }
    }

    public TestContext TestContext { get; set; }
}
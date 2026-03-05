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
        await dispatcher.Send(command, TestContext.CancellationToken);

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
        var result = await dispatcher.Send<string>(query, CancellationToken.None);

        // Assert
        Assert.AreEqual("response", result);
    }

    [TestMethod]
    public async Task SendWithValidationBehaviorThrowsCustomValidationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestCommand>, TestCommandHandler>();
        services.AddTransient<IPipelineBehavior<TestCommand>, ValidationBehaviorForCommand>();
        var provider = services.BuildServiceProvider();
        var dispatcher = new RequestDispatcher(provider);

        var command = new TestCommand();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomValidationException>(async () =>
        {
            await dispatcher.Send(command, TestContext.CancellationToken);
        });

        Assert.AreEqual("Validation failed for TestCommand", exception.Message);
        Assert.IsFalse(command.Handled, "Handler should not be called when validation fails");
    }

    [TestMethod]
    public async Task SendWithResponseWithValidationBehaviorThrowsCustomValidationException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestQuery, string>, TestQueryHandler>();
        services.AddTransient<IPipelineBehavior<TestQuery, string>, ValidationBehaviorForQuery>();
        var provider = services.BuildServiceProvider();
        var dispatcher = new RequestDispatcher(provider);

        var query = new TestQuery();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<CustomValidationException>(async () =>
        {
            await dispatcher.Send<string>(query, TestContext.CancellationToken);
        });

        Assert.AreEqual("Validation failed for TestQuery", exception.Message);
    }

    [TestMethod]
    public async Task SendWithHandlerExceptionThrowsOriginalException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestCommand>, ThrowingCommandHandler>();
        var provider = services.BuildServiceProvider();
        var dispatcher = new RequestDispatcher(provider);

        var command = new TestCommand();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await dispatcher.Send(command, TestContext.CancellationToken);
        });

        Assert.AreEqual("Handler threw exception", exception.Message);
    }

    [TestMethod]
    public async Task SendWithResponseWithHandlerExceptionThrowsOriginalException()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddTransient<IRequestHandler<TestQuery, string>, ThrowingQueryHandler>();
        var provider = services.BuildServiceProvider();
        var dispatcher = new RequestDispatcher(provider);

        var query = new TestQuery();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await dispatcher.Send<string>(query, TestContext.CancellationToken);
        });

        Assert.AreEqual("Handler threw exception", exception.Message);
    }

    public TestContext TestContext { get; set; }
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

public class ThrowingCommandHandler : IRequestHandler<TestCommand>
{
    public Task Handle(TestCommand request, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Handler threw exception");
    }
}

public class TestQuery : IRequest<string> { }
public class TestQueryHandler : IRequestHandler<TestQuery, string>
{
    public Task<string> Handle(TestQuery request, CancellationToken cancellationToken) => Task.FromResult("response");
}

public class ThrowingQueryHandler : IRequestHandler<TestQuery, string>
{
    public Task<string> Handle(TestQuery request, CancellationToken cancellationToken)
    {
        throw new InvalidOperationException("Handler threw exception");
    }
}

// Validation behaviors that mimic CustomValidationBehavior
public class ValidationBehaviorForCommand : IPipelineBehavior<TestCommand>
{
    public Task Handle(TestCommand request, RequestDelegateInvoker nextInvoker, CancellationToken cancellationToken)
    {
        throw new CustomValidationException("Validation failed for TestCommand");
    }
}

public class ValidationBehaviorForQuery : IPipelineBehavior<TestQuery, string>
{
    public Task<string> Handle(TestQuery request, RequestDelegateInvoker<string> nextInvoker, CancellationToken cancellationToken)
    {
        throw new CustomValidationException("Validation failed for TestQuery");
    }
}

// Custom exception to mimic Goodtocode.Validation.CustomValidationException
public class CustomValidationException : Exception
{
    public CustomValidationException(string message) : base(message)
    {
    }
}
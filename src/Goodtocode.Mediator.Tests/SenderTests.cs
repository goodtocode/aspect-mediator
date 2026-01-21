namespace Goodtocode.Mediator.Tests;

[TestClass]
public class SenderTests
{
    [TestMethod]
    public async Task SendCommandDelegatesToDispatcher()
    {
        // Arrange
        var dispatcher = new TestDispatcher();
        var sender = new Sender(dispatcher);
        var command = new TestCommand();

        // Act
        await sender.Send(command);

        // Assert
        Assert.IsTrue(dispatcher.CommandSent);
        Assert.AreSame(command, dispatcher.LastCommand);
    }

    [TestMethod]
    public async Task SendQueryDelegatesToDispatcher()
    {
        // Arrange
        var dispatcher = new TestDispatcher();
        var sender = new Sender(dispatcher);
        var query = new TestQuery();

        // Act
        var result = await sender.Send<string>(query);

        // Assert
        Assert.IsTrue(dispatcher.QuerySent);
        Assert.AreSame(query, dispatcher.LastQuery);
        Assert.AreEqual("ok", result);
    }

    // Manual test double for IRequestDispatcher
    private class TestDispatcher : IRequestDispatcher
    {
        public bool CommandSent { get; private set; }
        public IRequest LastCommand { get; private set; }
        public bool QuerySent { get; private set; }
        public IRequest<string> LastQuery { get; private set; }

        public Task Send(IRequest request, CancellationToken cancellationToken = default)
        {
            CommandSent = true;
            LastCommand = request;
            return Task.CompletedTask;
        }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            QuerySent = true;
            LastQuery = request as IRequest<string>;
            return Task.FromResult((TResponse)(object)"ok");
        }
    }

    // Minimal test request types
    private class TestCommand : IRequest { }
    private class TestQuery : IRequest<string> { }
}
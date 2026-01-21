namespace Goodtocode.Mediator.Tests;

[TestClass]
public class PipelineBehaviorTests
{
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
}
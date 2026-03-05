using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Goodtocode.Mediator;

public class RequestDispatcher(IServiceProvider serviceProvider) : IRequestDispatcher
{
    public async Task Send(IRequest request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<>).MakeGenericType(requestType);
        var handler = serviceProvider.GetRequiredService(handlerType);

        var behaviorType = typeof(IPipelineBehavior<>).MakeGenericType(requestType);
        var behaviors = serviceProvider.GetServices(behaviorType).ToList();

        RequestDelegateInvoker handlerDelegate = () =>
            UnwrapInvoke<Task>(() =>
                (Task)handlerType.GetMethod("Handle")!.Invoke(handler, new object[] { request, cancellationToken })!);

        foreach (var behavior in behaviors.AsEnumerable().Reverse())
        {
            if (behavior is null)
                throw new InvalidOperationException("Pipeline behavior is null.");
            var next = handlerDelegate;
            handlerDelegate = () =>
                UnwrapInvoke<Task>(() =>
                    (Task)behaviorType.GetMethod("Handle")!.Invoke(behavior, new object[] { request, next, cancellationToken })!);
        }

        await handlerDelegate();
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var requestType = request.GetType();
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
        var handler = serviceProvider.GetRequiredService(handlerType);

        var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
        var behaviors = serviceProvider.GetServices(behaviorType)?.ToList() ?? [];

        RequestDelegateInvoker<TResponse> handlerDelegate = () =>
            UnwrapInvoke<Task<TResponse>>(() =>
                (Task<TResponse>)handlerType.GetMethod("Handle")!.Invoke(handler, [request, cancellationToken])!);

        foreach (var behavior in behaviors.AsEnumerable().Reverse())
        {
            if (behavior is null)
                throw new InvalidOperationException("Pipeline behavior is null.");
            var next = handlerDelegate;
            handlerDelegate = () =>
                UnwrapInvoke<Task<TResponse>>(() =>
                    (Task<TResponse>)behaviorType.GetMethod("Handle")!.Invoke(behavior, [request, next, cancellationToken])!);
        }

        return await handlerDelegate();
    }

    private static T UnwrapInvoke<T>(Func<T> invoke)
    {
        try
        {
            return invoke();
        }
        catch (TargetInvocationException ex) when (ex.InnerException != null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }
}

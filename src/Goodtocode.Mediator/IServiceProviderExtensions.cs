using System.Reflection;

namespace Goodtocode.Mediator;

/// <summary>
/// BCL-only equivalents of Microsoft.Extensions.DependencyInjection's
/// ServiceProviderServiceExtensions.GetRequiredService/GetServices.
/// No package reference required; works on netstandard2.1+.
/// </summary>
internal static class IServiceProviderExtensions
{
#pragma warning disable CA1510
#pragma warning disable CA2263
    // Lazily resolve the DI abstractions interface (if the assembly is loaded)
    // so providers that implement ISupportRequiredService can use their fast path.
    // Type full name + assembly name matches the real interface.
    private static readonly Type? _isupportRequiredServiceType =
        Type.GetType(
            "Microsoft.Extensions.DependencyInjection.ISupportRequiredService, Microsoft.Extensions.DependencyInjection.Abstractions",
            throwOnError: false
        );

    private static readonly MethodInfo? _supportRequiredServiceMethod =
        _isupportRequiredServiceType?.GetMethod(
            "GetRequiredService",
            BindingFlags.Public | BindingFlags.Instance,
            binder: null,
            types: new[] { typeof(Type) },
            modifiers: null
        );

    /// <summary>
    /// Get service of type <paramref name="serviceType"/> from the provider
    /// or throw if not registered (matches DI behavior).
    /// </summary>
    internal static object GetRequiredService(this IServiceProvider provider, Type serviceType)
    {
        if (provider == null) throw new ArgumentNullException(nameof(provider));        
        if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

        if (_isupportRequiredServiceType != null &&
            _supportRequiredServiceMethod != null &&
            _isupportRequiredServiceType.IsInstanceOfType(provider))
        {
            return _supportRequiredServiceMethod.Invoke(provider, new object[] { serviceType })!;
        }

        var service = provider.GetService(serviceType);
        if (service == null)
        {
            throw new InvalidOperationException($"No service for type '{serviceType}' has been registered.");
        }

        return service;
    }

    /// <summary>
    /// Get service of type T from the provider or throw if not registered.
    /// </summary>
    internal static T GetRequiredService<T>(this IServiceProvider provider)
    {
        if (provider == null) throw new ArgumentNullException(nameof(provider));
        return (T)provider.GetRequiredService(typeof(T));
    }

    /// <summary>
    /// Get all services of type <paramref name="serviceType"/> from the provider.
    /// </summary>
    internal static IEnumerable<object> GetServices(this IServiceProvider provider, Type serviceType)
    {
        if (provider == null) throw new ArgumentNullException(nameof(provider));
        if (serviceType == null) throw new ArgumentNullException(nameof(serviceType));

        var enumerableType = typeof(IEnumerable<>).MakeGenericType(serviceType);
        var result = provider.GetRequiredService(enumerableType);

        return (IEnumerable<object>)result;
    }

    /// <summary>
    /// Get all services of type T from the provider.
    /// </summary>
    internal static IEnumerable<T> GetServices<T>(this IServiceProvider provider)
    {
        if (provider == null) throw new ArgumentNullException(nameof(provider));
        return (IEnumerable<T>)provider.GetRequiredService(typeof(IEnumerable<T>));
    }
#pragma warning restore CA1510
#pragma warning restore CA2263
}

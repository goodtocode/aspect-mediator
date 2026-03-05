using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Goodtocode.Mediator;

public static class ConfigureServices
{
    public static IServiceCollection AddMediatorServices(this IServiceCollection services)
    {
        var executingAssembly = Assembly.GetExecutingAssembly();
        var callingAssembly = Assembly.GetCallingAssembly();

        return services.AddMediatorServices(executingAssembly, callingAssembly);
    }

    public static IServiceCollection AddMediatorServices(this IServiceCollection services, params Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
            return services;
        var assembliesToScan = new HashSet<Assembly>(assemblies);

        foreach (var assembly in assembliesToScan)
        {
            var handlerTypes = assembly
             .GetTypes()
             .Where(t => t.GetInterfaces().Any(i =>
                 i.IsGenericType &&
                 (
                     i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                     i.GetGenericTypeDefinition() == typeof(IRequestHandler<>)
                 )
             ));

            foreach (var handlerType in handlerTypes)
            {
                var interfaceType = handlerType.GetInterfaces().First(i =>
                    i.IsGenericType &&
                    (
                        i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                        i.GetGenericTypeDefinition() == typeof(IRequestHandler<>)
                    )
                );
                services.AddTransient(interfaceType, handlerType);
            }
        }

        services.AddTransient<IRequestDispatcher, RequestDispatcher>();
        services.AddTransient<ISender, Sender>();

        return services;
    }
}
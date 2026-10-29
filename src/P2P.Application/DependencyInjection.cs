using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using P2P.Application.Abstractions.Messaging;
using P2P.Application.Behaviors;

namespace P2P.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddScoped<IDispatcher, Dispatcher>();

        RegisterHandlers(services, assembly, typeof(ICommandHandler<,>));
        RegisterHandlers(services, assembly, typeof(IQueryHandler<,>));
        RegisterHandlers(services, assembly, typeof(IDomainEventHandler<>));

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        // Order matters: first registered is outermost.
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

        return services;
    }

    private static void RegisterHandlers(
        IServiceCollection services, Assembly assembly, Type openHandlerType)
    {
        var implementations = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false });

        foreach (var implementation in implementations)
        {
            var closedInterfaces = implementation.GetInterfaces()
                .Where(i => i.IsGenericType
                            && i.GetGenericTypeDefinition() == openHandlerType);

            foreach (var closedInterface in closedInterfaces)
            {
                services.AddScoped(closedInterface, implementation);
            }
        }
    }
}

using System.Reflection;

namespace P2P.Web.Endpoints;

public static class EndpointModuleExtensions
{
    public static IServiceCollection AddEndpointModules(
        this IServiceCollection services)
    {
        var modules = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false }
                        && typeof(IEndpointModule).IsAssignableFrom(t))
            .Select(Activator.CreateInstance)
            .Cast<IEndpointModule>()
            .ToArray();

        services.AddSingleton<IReadOnlyCollection<IEndpointModule>>(modules);

        return services;
    }

    public static IApplicationBuilder MapEndpointModules(this WebApplication app)
    {
        var modules = app.Services
            .GetRequiredService<IReadOnlyCollection<IEndpointModule>>();

        foreach (var module in modules)
        {
            module.MapEndpoints(app);
        }

        return app;
    }
}

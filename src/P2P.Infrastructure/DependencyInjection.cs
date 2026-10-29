using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using P2P.Application.Abstractions;
using P2P.Application.Abstractions.Messaging;
using P2P.Infrastructure.Persistence;
using P2P.Infrastructure.Persistence.Interceptors;
using P2P.Infrastructure.Services;

namespace P2P.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("P2PDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'P2PDatabase' is not configured.");

        services.AddScoped<AuditInterceptor>();
        services.AddScoped<DomainEventInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "infrastructure");
                sql.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorNumbersToAdd: null);
                sql.CommandTimeout(60);
            });

            options.AddInterceptors(
                sp.GetRequiredService<AuditInterceptor>(),
                sp.GetRequiredService<DomainEventInterceptor>());

#if DEBUG
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
#endif
        });

        services.AddScoped<IApplicationDbContext>(sp =>
            sp.GetRequiredService<ApplicationDbContext>());

        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<INumberSequence, SqlNumberSequence>();
        services.AddScoped<IApprovalRouter, ApprovalRouter>();
        services.AddScoped<IDomainEventPublisher, DomainEventPublisher>();
        services.AddScoped<IAuditTrailReader, AuditTrailReader>();

        return services;
    }
}

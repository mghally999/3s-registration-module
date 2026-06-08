using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Threes.Registration.Application.Common.Abstractions;
using Threes.Registration.Persistence.Interceptors;
using Threes.Registration.Persistence.Repositories;

namespace Threes.Registration.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        string connectionString)
    {
        // the outbox interceptor is stateless, so a singleton is fine. it is
        // attached to every dbcontext instance below.
        services.AddSingleton<ConvertDomainEventsToOutboxInterceptor>();

        services.AddDbContext<RegistrationDbContext>((serviceProvider, options) =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(RegistrationDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure();
            });

            options.AddInterceptors(
                serviceProvider.GetRequiredService<ConvertDomainEventsToOutboxInterceptor>());
        });

        services.AddScoped<IRegistrationRepository, RegistrationRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ILookupReadStore, LookupReadStore>();

        return services;
    }
}

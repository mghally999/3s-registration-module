using System.Reflection;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Threes.Registration.Application.Common.Behaviors;

namespace Threes.Registration.Application;

// one place to wire up everything the application layer owns: mediatr and its
// pipeline, the fluentvalidation validators, and the automapper profiles.
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
            // order matters: logging wraps the whole thing, validation runs
            // just before the handler.
            cfg.AddOpenBehavior(typeof(RequestLoggingBehavior<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        services.AddAutoMapper(assembly);

        return services;
    }
}

using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Threes.Registration.Application.Common.Abstractions;
using Threes.Registration.Infrastructure.Lookups;
using Threes.Registration.Infrastructure.Messaging;
using Threes.Registration.Infrastructure.Notifications;
using Threes.Registration.Infrastructure.Phone;
using Threes.Registration.Infrastructure.Time;

namespace Threes.Registration.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // clock + normalizer are stateless, the cache holds a shared snapshot,
        // so all three are singletons.
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IMobileNumberNormalizer, LibPhoneNumberNormalizer>();
        services.AddSingleton<ILookupCache, LookupCache>();

        // email: use the real SendGrid sender when an API key is configured,
        // otherwise the logging stub (so local dev and tests need no account).
        var emailOptions = new EmailOptions();
        configuration.GetSection(EmailOptions.SectionName).Bind(emailOptions);
        services.AddSingleton(emailOptions);

        if (!string.IsNullOrWhiteSpace(emailOptions.SendGridApiKey))
        {
            // a single long-lived HttpClient is the recommended pattern for a
            // fixed endpoint; no IHttpClientFactory dependency needed.
            services.AddSingleton<IEmailSender>(sp => new SendGridEmailSender(
                new HttpClient(),
                emailOptions,
                sp.GetRequiredService<ILogger<SendGridEmailSender>>()));
        }
        else
        {
            services.AddSingleton<IEmailSender, LoggingEmailSender>();
        }

        services.AddSingleton<ISmsSender, LoggingSmsSender>();

        var messaging = new MessagingOptions();
        configuration.GetSection(MessagingOptions.SectionName).Bind(messaging);

        services.AddMassTransit(x =>
        {
            x.AddConsumer<RegistrationCreatedConsumer>();
            x.SetKebabCaseEndpointNameFormatter();

            if (messaging.UseInMemory)
            {
                x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context));
            }
            else
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host(messaging.Host, messaging.VirtualHost, h =>
                    {
                        h.Username(messaging.Username);
                        h.Password(messaging.Password);
                    });
                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        // the outbox publisher loop.
        services.AddHostedService<OutboxProcessor>();

        return services;
    }
}

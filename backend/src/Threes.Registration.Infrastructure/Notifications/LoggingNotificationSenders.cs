using Microsoft.Extensions.Logging;

namespace Threes.Registration.Infrastructure.Notifications;

// stub senders that just log. swap these out for a real esp / sms gateway
// implementation and nothing else in the pipeline has to change.
public sealed class LoggingEmailSender : IEmailSender
{
    private readonly ILogger<LoggingEmailSender> _logger;

    public LoggingEmailSender(ILogger<LoggingEmailSender> logger) => _logger = logger;

    public Task SendWelcomeAsync(string email, CancellationToken cancellationToken)
    {
        // we log that we sent a welcome mail, but not to whom at info level
        // beyond the domain part, to keep pii out of the logs.
        _logger.LogInformation("welcome email queued for domain {Domain}", DomainOf(email));
        return Task.CompletedTask;
    }

    private static string DomainOf(string email)
    {
        var at = email.IndexOf('@');
        return at >= 0 ? email[(at + 1)..] : "unknown";
    }
}

public sealed class LoggingSmsSender : ISmsSender
{
    private readonly ILogger<LoggingSmsSender> _logger;

    public LoggingSmsSender(ILogger<LoggingSmsSender> logger) => _logger = logger;

    public Task SendWelcomeAsync(string mobileE164, CancellationToken cancellationToken)
    {
        _logger.LogInformation("welcome sms queued (country code {CountryCode})", CountryCodeOf(mobileE164));
        return Task.CompletedTask;
    }

    private static string CountryCodeOf(string e164) =>
        e164.Length >= 3 ? e164[..3] : e164;
}

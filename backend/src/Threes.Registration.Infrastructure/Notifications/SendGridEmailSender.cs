using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace Threes.Registration.Infrastructure.Notifications;

// sends a real welcome email through SendGrid's v3 Web API. it is only wired up
// when an API key is configured (see DependencyInjection); otherwise the logging
// stub is used. failures are logged and swallowed — this runs after the create
// transaction has committed, so a mail hiccup must never roll a registration
// back or crash the consumer.
public sealed class SendGridEmailSender : IEmailSender
{
    private readonly HttpClient _http;
    private readonly EmailOptions _options;
    private readonly ILogger<SendGridEmailSender> _logger;

    public SendGridEmailSender(HttpClient http, EmailOptions options, ILogger<SendGridEmailSender> logger)
    {
        _http = http;
        _options = options;
        _logger = logger;
    }

    public async Task SendWelcomeAsync(string email, CancellationToken cancellationToken)
    {
        // SendGrid requires the text/plain part before text/html.
        var payload = new
        {
            personalizations = new[] { new { to = new[] { new { email } } } },
            from = new { email = _options.FromAddress, name = _options.FromName },
            subject = "Welcome to 3S — your registration was received",
            content = new object[]
            {
                new
                {
                    type = "text/plain",
                    value =
                        "Thank you for registering with 3S (Secured Smart Systems). " +
                        "Your registration has been received and is being processed.",
                },
                new
                {
                    type = "text/html",
                    value =
                        "<p>Thank you for registering with <strong>3S (Secured Smart Systems)</strong>.</p>" +
                        "<p>Your registration has been received and is being processed.</p>",
                },
            },
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "https://api.sendgrid.com/v3/mail/send")
        {
            Content = JsonContent.Create(payload),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.SendGridApiKey);

        try
        {
            var response = await _http.SendAsync(request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("welcome email sent to domain {Domain}", DomainOf(email));
            }
            else
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "welcome email send failed with {StatusCode}: {Body}",
                    (int)response.StatusCode,
                    body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "welcome email send threw; continuing");
        }
    }

    private static string DomainOf(string email)
    {
        var at = email.IndexOf('@');
        return at >= 0 ? email[(at + 1)..] : "unknown";
    }
}

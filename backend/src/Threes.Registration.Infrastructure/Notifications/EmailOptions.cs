namespace Threes.Registration.Infrastructure.Notifications;

// configuration for outbound email. when SendGridApiKey is empty the app falls
// back to the logging stub, so local dev and tests need no external account.
public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string? SendGridApiKey { get; set; }

    // must be a SendGrid-verified sender (single-sender verification is enough).
    public string FromAddress { get; set; } = "no-reply@example.com";

    public string FromName { get; set; } = "3S Secured Smart Systems";
}

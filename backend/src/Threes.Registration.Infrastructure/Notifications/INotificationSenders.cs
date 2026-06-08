namespace Threes.Registration.Infrastructure.Notifications;

// the two outbound channels a welcome message could go through. real
// implementations would call an esp / sms gateway; here they are logged stubs
// so the flow is exercised end to end without external accounts.
public interface IEmailSender
{
    Task SendWelcomeAsync(string email, CancellationToken cancellationToken);
}

public interface ISmsSender
{
    Task SendWelcomeAsync(string mobileE164, CancellationToken cancellationToken);
}

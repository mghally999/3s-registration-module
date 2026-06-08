namespace Threes.Registration.Application.Common.IntegrationEvents;

// the message that leaves the service after a registration is saved. it is
// published through the outbox (so it only goes out if the transaction
// committed) and carries no sensitive payload beyond what a downstream
// welcome-email/audit consumer needs. masstransit serializes this as the
// message contract.
public sealed record RegistrationCreatedIntegrationEvent
{
    public Guid RegistrationId { get; init; }
    public string Email { get; init; } = string.Empty;
    public string MobileNumber { get; init; } = string.Empty;
    public DateTimeOffset OccurredOnUtc { get; init; }
}

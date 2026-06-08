using Threes.Registration.Domain.Common;

namespace Threes.Registration.Domain.Registrations.Events;

// raised once when a registration is first created. the infrastructure side
// turns this into an outbox message and, later, a published integration event
// (welcome email/sms, audit, etc). the create transaction itself does not wait
// on any of that.
public sealed record RegistrationCreatedDomainEvent(
    Guid RegistrationId,
    string Email,
    string MobileNumber,
    DateTimeOffset OccurredOnUtc) : IDomainEvent;

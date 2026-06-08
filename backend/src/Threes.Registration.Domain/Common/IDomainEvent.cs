namespace Threes.Registration.Domain.Common;

// marker for something that happened inside the domain that the outside world
// might want to react to (welcome email, audit trail, etc).
//
// on purpose this is our own little interface and not mediatr's INotification.
// the domain project is not allowed to reference mediatr, so we keep the
// contract here and let the application layer adapt it to mediatr later.
public interface IDomainEvent
{
    // when the thing happened. we pass this in from a clock instead of calling
    // DateTime.UtcNow inside the domain, so unit tests stay deterministic.
    DateTimeOffset OccurredOnUtc { get; }
}

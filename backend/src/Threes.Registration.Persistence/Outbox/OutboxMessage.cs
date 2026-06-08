namespace Threes.Registration.Persistence.Outbox;

// one row of the transactional outbox. when a registration is saved, the
// domain events it raised are turned into these rows inside the SAME database
// transaction (see ConvertDomainEventsToOutboxInterceptor). a background
// processor then reads the unprocessed rows and publishes them to the broker.
//
// this is the whole point of the outbox pattern: the registration and the
// "i must publish an event" fact commit together atomically, so we never lose
// an event because rabbitmq happened to be down, and we never publish an event
// for a registration that rolled back.
public sealed class OutboxMessage
{
    public Guid Id { get; set; }

    // the integration event's type name, used by the processor to deserialize
    // and publish the right contract.
    public string Type { get; set; } = string.Empty;

    // the serialized integration event payload (json).
    public string Content { get; set; } = string.Empty;

    public DateTimeOffset OccurredOnUtc { get; set; }

    // null until the processor successfully publishes it.
    public DateTimeOffset? ProcessedOnUtc { get; set; }

    // last error, if a publish attempt failed. lets us retry and inspect.
    public string? Error { get; set; }
}

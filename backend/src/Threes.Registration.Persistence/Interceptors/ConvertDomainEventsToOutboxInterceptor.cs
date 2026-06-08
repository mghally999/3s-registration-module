using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Threes.Registration.Application.Common.IntegrationEvents;
using Threes.Registration.Domain.Common;
using Threes.Registration.Domain.Registrations.Events;
using Threes.Registration.Persistence.Outbox;

namespace Threes.Registration.Persistence.Interceptors;

// the bridge between domain events and the outbox. right before ef saves, this
// interceptor sweeps every tracked aggregate, turns its raised domain events
// into integration events, and writes them as outbox rows in the same
// SaveChanges call (so same transaction). then it clears the events off the
// aggregate so they are never written twice.
public sealed class ConvertDomainEventsToOutboxInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            WriteOutboxMessages(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            WriteOutboxMessages(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void WriteOutboxMessages(DbContext context)
    {
        var aggregates = context.ChangeTracker
            .Entries<AggregateRoot>()
            .Select(e => e.Entity)
            .Where(a => a.DomainEvents.Count > 0)
            .ToList();

        var messages = new List<OutboxMessage>();

        foreach (var aggregate in aggregates)
        {
            foreach (var domainEvent in aggregate.DomainEvents)
            {
                var integrationEvent = ToIntegrationEvent(domainEvent);
                if (integrationEvent is null)
                {
                    continue;
                }

                messages.Add(new OutboxMessage
                {
                    Id = Guid.NewGuid(),
                    Type = integrationEvent.GetType().FullName!,
                    Content = JsonSerializer.Serialize(integrationEvent, integrationEvent.GetType(), SerializerOptions),
                    OccurredOnUtc = domainEvent.OccurredOnUtc,
                });
            }

            aggregate.ClearDomainEvents();
        }

        if (messages.Count > 0)
        {
            context.Set<OutboxMessage>().AddRange(messages);
        }
    }

    // map each domain event onto the public integration contract. returns null
    // for events that are internal-only and should not leave the service.
    private static object? ToIntegrationEvent(IDomainEvent domainEvent) => domainEvent switch
    {
        RegistrationCreatedDomainEvent e => new RegistrationCreatedIntegrationEvent
        {
            RegistrationId = e.RegistrationId,
            Email = e.Email,
            MobileNumber = e.MobileNumber,
            OccurredOnUtc = e.OccurredOnUtc,
        },
        _ => null,
    };
}

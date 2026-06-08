using System.Text.Json;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Threes.Registration.Application.Common.IntegrationEvents;
using Threes.Registration.Persistence;

namespace Threes.Registration.Infrastructure.Messaging;

// the publish side of the outbox. on a timer it picks up unprocessed outbox
// rows (oldest first), deserializes each back into its integration contract,
// publishes it through masstransit, and stamps it processed. a publish failure
// is recorded on the row and retried on the next tick, so a broker outage just
// delays delivery instead of losing the event.
public sealed class OutboxProcessor : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(10);
    private const int BatchSize = 20;

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    // map the stored type name back to a concrete contract type. add new
    // integration events here as they are introduced.
    private static readonly IReadOnlyDictionary<string, Type> KnownTypes = new Dictionary<string, Type>
    {
        [typeof(RegistrationCreatedIntegrationEvent).FullName!] = typeof(RegistrationCreatedIntegrationEvent),
    };

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxProcessor> _logger;

    public OutboxProcessor(IServiceScopeFactory scopeFactory, ILogger<OutboxProcessor> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(PollInterval);

        do
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                // never let a bad batch kill the loop.
                _logger.LogError(ex, "outbox batch failed");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<RegistrationDbContext>();
        var publisher = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

        var messages = await db.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(BatchSize)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                if (!KnownTypes.TryGetValue(message.Type, out var contractType))
                {
                    message.Error = $"unknown message type '{message.Type}'";
                    continue;
                }

                var payload = JsonSerializer.Deserialize(message.Content, contractType, SerializerOptions);
                if (payload is null)
                {
                    message.Error = "payload deserialized to null";
                    continue;
                }

                await publisher.Publish(payload, contractType, cancellationToken);

                message.ProcessedOnUtc = DateTimeOffset.UtcNow;
                message.Error = null;
            }
            catch (Exception ex)
            {
                message.Error = ex.Message;
                _logger.LogWarning(ex, "failed to publish outbox message {MessageId}", message.Id);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}

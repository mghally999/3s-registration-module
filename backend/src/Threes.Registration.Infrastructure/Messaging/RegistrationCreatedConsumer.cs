using MassTransit;
using Microsoft.Extensions.Logging;
using Threes.Registration.Application.Common.IntegrationEvents;
using Threes.Registration.Infrastructure.Notifications;

namespace Threes.Registration.Infrastructure.Messaging;

// the masstransit consumer for the registration-created integration event. it
// does the post-registration side work (welcome email + sms). this runs well
// after the create transaction committed, so nothing here can roll the
// registration back, which is exactly the decoupling the task asks for.
public sealed class RegistrationCreatedConsumer : IConsumer<RegistrationCreatedIntegrationEvent>
{
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;
    private readonly ILogger<RegistrationCreatedConsumer> _logger;

    public RegistrationCreatedConsumer(
        IEmailSender emailSender,
        ISmsSender smsSender,
        ILogger<RegistrationCreatedConsumer> logger)
    {
        _emailSender = emailSender;
        _smsSender = smsSender;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<RegistrationCreatedIntegrationEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "processing registration-created for {RegistrationId}",
            message.RegistrationId);

        await _emailSender.SendWelcomeAsync(message.Email, context.CancellationToken);
        await _smsSender.SendWelcomeAsync(message.MobileNumber, context.CancellationToken);
    }
}

namespace Threes.Registration.Infrastructure.Messaging;

// bound from the "Messaging" config section. UseInMemory lets tests (and a
// no-broker local run) spin masstransit up without rabbitmq.
public sealed class MessagingOptions
{
    public const string SectionName = "Messaging";

    public bool UseInMemory { get; set; }
    public string Host { get; set; } = "localhost";
    public string VirtualHost { get; set; } = "/";
    public string Username { get; set; } = "guest";
    public string Password { get; set; } = "guest";
}

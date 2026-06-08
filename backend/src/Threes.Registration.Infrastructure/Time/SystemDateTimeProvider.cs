using Threes.Registration.Application.Common.Abstractions;

namespace Threes.Registration.Infrastructure.Time;

// the real clock. the only place in the running app that reads the system time
// for business decisions, so swapping it for a fake in tests freezes "now".
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;

    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
}

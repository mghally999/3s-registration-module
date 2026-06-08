namespace Threes.Registration.Application.Common.Abstractions;

// a thin clock so handlers and the age check never touch DateTime.UtcNow
// directly. the real implementation lives in infrastructure; tests pass a
// fake that returns a fixed point in time.
public interface IDateTimeProvider
{
    DateTimeOffset UtcNow { get; }

    DateOnly Today { get; }
}

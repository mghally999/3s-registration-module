using Threes.Registration.Domain.Common;

namespace Threes.Registration.Domain.ValueObjects;

// a street line. required, trimmed, and capped at 200 chars. no character
// restriction beyond that, since street names are free text.
public sealed class Street : ValueObject
{
    public const int MaxLength = 200;

    private Street(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Street Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new DomainException("Street is required.");
        }

        var trimmed = raw.Trim();
        if (trimmed.Length > MaxLength)
        {
            throw new DomainException($"Street must be at most {MaxLength} characters.");
        }

        return new Street(trimmed);
    }

    public static Street FromPersisted(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}

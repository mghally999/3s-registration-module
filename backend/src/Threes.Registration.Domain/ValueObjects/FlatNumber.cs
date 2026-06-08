using System.Text.RegularExpressions;
using Threes.Registration.Domain.Common;

namespace Threes.Registration.Domain.ValueObjects;

// a flat / apartment number. same allowed characters as a building number:
// letters, digits, slash, dash and spaces. required, max 20 chars.
public sealed partial class FlatNumber : ValueObject
{
    public const int MaxLength = 20;

    [GeneratedRegex(@"^[A-Za-z0-9 /\-]+$")]
    private static partial Regex AllowedPattern();

    [GeneratedRegex(@"\s+")]
    private static partial Regex RepeatedWhitespace();

    private FlatNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static FlatNumber Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new DomainException("Flat number is required.");
        }

        var normalized = RepeatedWhitespace().Replace(raw.Trim(), " ");

        if (normalized.Length > MaxLength)
        {
            throw new DomainException($"Flat number must be at most {MaxLength} characters.");
        }

        if (!AllowedPattern().IsMatch(normalized))
        {
            throw new DomainException(
                "Flat number may only contain letters, numbers, slash, dash and spaces.");
        }

        return new FlatNumber(normalized);
    }

    public static FlatNumber FromPersisted(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}

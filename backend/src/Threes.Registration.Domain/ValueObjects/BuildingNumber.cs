using System.Text.RegularExpressions;
using Threes.Registration.Domain.Common;

namespace Threes.Registration.Domain.ValueObjects;

// a building number. real ones are messy ("12A", "10/2", "5 - 7") so we allow
// letters, digits, slash, dash and spaces. required, max 20 chars.
public sealed partial class BuildingNumber : ValueObject
{
    public const int MaxLength = 20;

    [GeneratedRegex(@"^[A-Za-z0-9 /\-]+$")]
    private static partial Regex AllowedPattern();

    [GeneratedRegex(@"\s+")]
    private static partial Regex RepeatedWhitespace();

    private BuildingNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static BuildingNumber Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new DomainException("Building number is required.");
        }

        var normalized = RepeatedWhitespace().Replace(raw.Trim(), " ");

        if (normalized.Length > MaxLength)
        {
            throw new DomainException($"Building number must be at most {MaxLength} characters.");
        }

        if (!AllowedPattern().IsMatch(normalized))
        {
            throw new DomainException(
                "Building number may only contain letters, numbers, slash, dash and spaces.");
        }

        return new BuildingNumber(normalized);
    }

    public static BuildingNumber FromPersisted(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}

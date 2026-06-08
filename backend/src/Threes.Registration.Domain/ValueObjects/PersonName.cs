using System.Text.RegularExpressions;
using Threes.Registration.Domain.Common;

namespace Threes.Registration.Domain.ValueObjects;

// one piece of a person's name (first, middle or last). holds the cleaned up,
// already validated value. the same rules apply to every name part, so they
// all flow through here.
public sealed partial class PersonName : ValueObject
{
    public const int MaxLength = 50;

    // letters we accept: english a-z (either case) and the core arabic block
    // (U+0621..U+064A, which covers hamza, ta marbuta, alef maqsura, etc).
    // between two letters we allow a single space, hyphen or apostrophe, and
    // the value may not start or end with one of those separators or stack
    // two of them in a row.
    [GeneratedRegex(@"^[A-Za-zء-ي]+(?:[ '\-][A-Za-zء-ي]+)*$")]
    private static partial Regex AllowedPattern();

    [GeneratedRegex(@"\s+")]
    private static partial Regex RepeatedWhitespace();

    private PersonName(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static PersonName Create(string? raw, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new DomainException($"{fieldName} is required.");
        }

        // trim the ends, then squeeze any run of whitespace down to one space
        // so "ahmed   ali" becomes "ahmed ali".
        var normalized = RepeatedWhitespace().Replace(raw.Trim(), " ");

        if (normalized.Length > MaxLength)
        {
            throw new DomainException($"{fieldName} must be at most {MaxLength} characters.");
        }

        if (!AllowedPattern().IsMatch(normalized))
        {
            throw new DomainException(
                $"{fieldName} may only contain Arabic or English letters, spaces, hyphen and apostrophe.");
        }

        return new PersonName(normalized);
    }

    // ef core uses this when reading a value that was already valid when saved,
    // so it skips the validation work.
    public static PersonName FromPersisted(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}

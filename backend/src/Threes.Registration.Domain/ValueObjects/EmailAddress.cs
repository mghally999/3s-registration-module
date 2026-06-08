using System.Text.RegularExpressions;
using Threes.Registration.Domain.Common;

namespace Threes.Registration.Domain.ValueObjects;

// an email plus its normalized form. we keep the original so we can show it
// back to the user exactly as typed, and we keep a lower-cased copy that the
// unique index and all comparisons run against (case-insensitive uniqueness).
public sealed partial class EmailAddress : ValueObject
{
    public const int MaxLength = 254;

    // deliberately simple: something, then @, then something, a dot, and more.
    // fluentvalidation does a friendlier check on the way in; this is the guard.
    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$")]
    private static partial Regex EmailPattern();

    private EmailAddress(string value, string normalized)
    {
        Value = value;
        Normalized = normalized;
    }

    public string Value { get; }
    public string Normalized { get; }

    public static EmailAddress Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            throw new DomainException("Email is required.");
        }

        var trimmed = raw.Trim();

        if (trimmed.Length > MaxLength)
        {
            throw new DomainException($"Email must be at most {MaxLength} characters.");
        }

        if (!EmailPattern().IsMatch(trimmed))
        {
            throw new DomainException("Email format is not valid.");
        }

        return new EmailAddress(trimmed, trimmed.ToLowerInvariant());
    }

    public static EmailAddress FromPersisted(string value, string normalized) => new(value, normalized);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        // equality is on the normalized form so "A@x.com" == "a@x.com".
        yield return Normalized;
    }

    public override string ToString() => Value;
}

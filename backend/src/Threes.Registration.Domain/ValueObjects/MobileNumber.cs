using System.Text.RegularExpressions;
using Threes.Registration.Domain.Common;

namespace Threes.Registration.Domain.ValueObjects;

// a mobile number already in normalized e.164 form, e.g. +201006158123.
// the actual parsing/normalization from whatever the user typed happens in the
// infrastructure layer (libphonenumber) before we get here, so this value
// object only has to confirm the shape and store it.
public sealed partial class MobileNumber : ValueObject
{
    // e.164: a plus sign, a leading non-zero country digit, then up to 14 more
    // digits (15 digits total max).
    [GeneratedRegex(@"^\+[1-9]\d{6,14}$")]
    private static partial Regex E164Pattern();

    private MobileNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static MobileNumber Create(string? e164)
    {
        if (string.IsNullOrWhiteSpace(e164))
        {
            throw new DomainException("Mobile number is required.");
        }

        var value = e164.Trim();

        if (!E164Pattern().IsMatch(value))
        {
            throw new DomainException(
                "Mobile number must be a valid E.164 number, for example +201006158123.");
        }

        return new MobileNumber(value);
    }

    public static MobileNumber FromPersisted(string value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}

using System.Text.RegularExpressions;

namespace Threes.Registration.Application.Common.Validation;

// the same shape rules the domain value objects enforce, exposed as plain
// booleans so the fluentvalidation validators can produce friendly 400
// messages before the request ever reaches the aggregate. the domain re-checks
// everything anyway, so this is the nice front door and the value objects are
// the locked back door.
public static partial class InputRules
{
    [GeneratedRegex(@"^[A-Za-zء-ي]+(?:[ '\-][A-Za-zء-ي]+)*$")]
    private static partial Regex NamePattern();

    [GeneratedRegex(@"^[A-Za-z0-9 /\-]+$")]
    private static partial Regex BuildingOrFlatPattern();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    public static string Normalize(string? raw) =>
        raw is null ? string.Empty : Whitespace().Replace(raw.Trim(), " ");

    public static bool IsValidName(string? raw)
    {
        var value = Normalize(raw);
        return value.Length > 0 && NamePattern().IsMatch(value);
    }

    public static bool IsValidBuildingOrFlat(string? raw)
    {
        var value = Normalize(raw);
        return value.Length > 0 && BuildingOrFlatPattern().IsMatch(value);
    }

    public static bool WithinLength(string? raw, int maxLength) =>
        Normalize(raw).Length <= maxLength;
}

namespace Threes.Registration.Application.Common.Abstractions;

// turns whatever the user typed ("01006158123", "+20 100 615 8123", ...) into
// a single canonical e.164 string, or tells us it cannot. the real
// implementation uses libphonenumber and lives in infrastructure.
public interface IMobileNumberNormalizer
{
    bool TryNormalize(string? raw, out string e164);
}

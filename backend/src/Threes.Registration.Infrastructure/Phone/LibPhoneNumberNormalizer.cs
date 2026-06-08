using PhoneNumbers;
using Threes.Registration.Application.Common.Abstractions;

namespace Threes.Registration.Infrastructure.Phone;

// normalizes whatever the user typed into canonical e.164 using google's
// libphonenumber. it accepts both already-international input (+2010...) and
// local egyptian input (010...), defaulting the region to EG, and it rejects
// anything that is not actually a mobile number.
public sealed class LibPhoneNumberNormalizer : IMobileNumberNormalizer
{
    // when the input has no country code we assume egypt. an input that already
    // starts with "+" ignores this and uses its own country code.
    private const string DefaultRegion = "EG";

    private readonly PhoneNumberUtil _util = PhoneNumberUtil.GetInstance();

    public bool TryNormalize(string? raw, out string e164)
    {
        e164 = string.Empty;

        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        try
        {
            var number = _util.Parse(raw.Trim(), DefaultRegion);

            if (!_util.IsValidNumber(number))
            {
                return false;
            }

            var type = _util.GetNumberType(number);
            if (type is not PhoneNumberType.MOBILE and not PhoneNumberType.FIXED_LINE_OR_MOBILE)
            {
                return false;
            }

            e164 = _util.Format(number, PhoneNumberFormat.E164);
            return true;
        }
        catch (NumberParseException)
        {
            return false;
        }
    }
}

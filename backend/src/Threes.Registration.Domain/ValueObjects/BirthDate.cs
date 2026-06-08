using Threes.Registration.Domain.Common;

namespace Threes.Registration.Domain.ValueObjects;

// a date of birth that already passed the "not in the future" and "old enough"
// rules. we take "today" as a parameter instead of reading the clock here so
// the age check is fully deterministic in unit tests.
public sealed class BirthDate : ValueObject
{
    public const int MinimumAgeYears = 20;

    private BirthDate(DateOnly value)
    {
        Value = value;
    }

    public DateOnly Value { get; }

    public static BirthDate Create(DateOnly value, DateOnly today)
    {
        if (value > today)
        {
            throw new DomainException("Birth date cannot be in the future.");
        }

        var age = CalculateAge(value, today);
        if (age < MinimumAgeYears)
        {
            throw new DomainException($"Minimum age is {MinimumAgeYears} years.");
        }

        return new BirthDate(value);
    }

    // accurate age: years difference, minus one if this year's birthday has not
    // happened yet. so someone born 2005-06-08 is still 19 on 2025-06-07.
    public static int CalculateAge(DateOnly birthDate, DateOnly today)
    {
        var age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age))
        {
            age--;
        }

        return age;
    }

    public static BirthDate FromPersisted(DateOnly value) => new(value);

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value.ToString("yyyy-MM-dd");
}

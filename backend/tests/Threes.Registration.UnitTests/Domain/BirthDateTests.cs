using FluentAssertions;
using Threes.Registration.Domain.Common;
using Threes.Registration.Domain.ValueObjects;
using Xunit;

namespace Threes.Registration.UnitTests.Domain;

public class BirthDateTests
{
    private static readonly DateOnly Today = new(2026, 6, 7);

    [Fact]
    public void CalculateAge_counts_full_years_only()
    {
        // birthday already happened this year -> full age.
        BirthDate.CalculateAge(new DateOnly(2000, 1, 1), Today).Should().Be(26);

        // birthday is tomorrow -> still one year short. this is the "accurate
        // age, not just the year difference" rule.
        BirthDate.CalculateAge(new DateOnly(2000, 6, 8), Today).Should().Be(25);

        // birthday is today -> counts.
        BirthDate.CalculateAge(new DateOnly(2000, 6, 7), Today).Should().Be(26);
    }

    [Fact]
    public void Create_accepts_someone_exactly_twenty_today()
    {
        var act = () => BirthDate.Create(new DateOnly(2006, 6, 7), Today);
        act.Should().NotThrow();
    }

    [Fact]
    public void Create_rejects_someone_a_day_under_twenty()
    {
        var act = () => BirthDate.Create(new DateOnly(2006, 6, 8), Today);
        act.Should().Throw<DomainException>().WithMessage("*20*");
    }

    [Fact]
    public void Create_rejects_a_future_date()
    {
        var act = () => BirthDate.Create(new DateOnly(2027, 1, 1), Today);
        act.Should().Throw<DomainException>().WithMessage("*future*");
    }
}

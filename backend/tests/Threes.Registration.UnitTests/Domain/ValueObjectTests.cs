using FluentAssertions;
using Threes.Registration.Domain.Common;
using Threes.Registration.Domain.ValueObjects;
using Xunit;

namespace Threes.Registration.UnitTests.Domain;

public class ValueObjectTests
{
    [Theory]
    [InlineData("Mohammed")]
    [InlineData("Mohammed Ahmed")]
    [InlineData("Al-Sayed")]
    [InlineData("O'Brien")]
    [InlineData("محمد")]
    [InlineData("محمد علي")]
    public void PersonName_accepts_arabic_and_english_with_allowed_separators(string value)
    {
        var act = () => PersonName.Create(value, "First name");
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData("Mohammed123")]
    [InlineData("John@Doe")]
    [InlineData("-leading")]
    [InlineData("trailing-")]
    [InlineData("")]
    [InlineData("   ")]
    public void PersonName_rejects_digits_specials_and_empty(string value)
    {
        var act = () => PersonName.Create(value, "First name");
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void PersonName_trims_and_collapses_repeated_spaces()
    {
        var name = PersonName.Create("  Mohammed    Ahmed  ", "First name");
        name.Value.Should().Be("Mohammed Ahmed");
    }

    [Fact]
    public void PersonName_rejects_value_longer_than_50()
    {
        var act = () => PersonName.Create(new string('a', 51), "First name");
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("Test@Example.com", "test@example.com")]
    [InlineData("  USER@Domain.COM ", "user@domain.com")]
    public void EmailAddress_keeps_original_but_normalizes_lowercase(string raw, string expectedNormalized)
    {
        var email = EmailAddress.Create(raw);
        email.Normalized.Should().Be(expectedNormalized);
        email.Value.Should().Be(raw.Trim());
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@dot")]
    [InlineData("")]
    public void EmailAddress_rejects_invalid_format(string raw)
    {
        var act = () => EmailAddress.Create(raw);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void MobileNumber_accepts_e164_and_rejects_other_shapes()
    {
        MobileNumber.Create("+201006158123").Value.Should().Be("+201006158123");

        var act = () => MobileNumber.Create("01006158123");
        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("12A")]
    [InlineData("10/2")]
    [InlineData("5 - 7")]
    public void BuildingNumber_allows_real_world_values(string value)
    {
        BuildingNumber.Create(value).Value.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void BuildingNumber_rejects_disallowed_characters()
    {
        var act = () => BuildingNumber.Create("12#A");
        act.Should().Throw<DomainException>();
    }
}

using FluentAssertions;
using Threes.Registration.Application.Registrations.Commands.CreateRegistration;
using Threes.Registration.Infrastructure.Phone;
using Threes.Registration.UnitTests.TestSupport;
using Xunit;

namespace Threes.Registration.UnitTests.Application;

// the validator stitches together every front-door rule, including the async
// lookup checks, so it gets the real normalizer and the real cache.
public class CreateRegistrationValidatorTests
{
    private static readonly DateOnly Today = new(2026, 6, 7);

    private static CreateRegistrationCommandValidator BuildValidator() =>
        new(new FixedClock(Today), new LibPhoneNumberNormalizer(), TestLookupCache.Create());

    private static CreateRegistrationCommand ValidCommand() => new()
    {
        FirstName = "Mohammed",
        MiddleName = "Ahmed",
        LastName = "Ghaly",
        BirthDate = new DateOnly(1995, 4, 12),
        MobileNumber = "+201006158123",
        Email = "mohammed@example.com",
        Addresses = new List<CreateAddressDto>
        {
            new() { GovernorateId = 1, CityId = 101, Street = "Abbas El Akkad", BuildingNumber = "12A", FlatNumber = "10/2", IsPrimary = true },
        },
    };

    [Fact]
    public async Task A_fully_valid_command_passes()
    {
        var result = await BuildValidator().ValidateAsync(ValidCommand());
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Local_mobile_format_is_accepted_because_it_normalizes()
    {
        var command = ValidCommand();
        command.MobileNumber = "01006158123"; // egyptian local form

        var result = await BuildValidator().ValidateAsync(command);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Name_with_digits_is_rejected()
    {
        var command = ValidCommand();
        command.FirstName = "Mohammed2";

        var result = await BuildValidator().ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.FirstName));
    }

    [Fact]
    public async Task Under_twenty_is_rejected()
    {
        var command = ValidCommand();
        command.BirthDate = new DateOnly(2010, 1, 1);

        var result = await BuildValidator().ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.BirthDate));
    }

    [Fact]
    public async Task Invalid_email_is_rejected()
    {
        var command = ValidCommand();
        command.Email = "nope";

        var result = await BuildValidator().ValidateAsync(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task City_under_the_wrong_governorate_is_rejected()
    {
        var command = ValidCommand();
        command.Addresses[0].GovernorateId = 2; // giza
        command.Addresses[0].CityId = 101;      // but nasr city is cairo

        var result = await BuildValidator().ValidateAsync(command);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName.Contains("CityId"));
    }

    [Fact]
    public async Task Zero_addresses_is_rejected()
    {
        var command = ValidCommand();
        command.Addresses.Clear();

        var result = await BuildValidator().ValidateAsync(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task More_than_one_primary_address_is_rejected()
    {
        var command = ValidCommand();
        command.Addresses.Add(new CreateAddressDto
        {
            GovernorateId = 1, CityId = 102, Street = "B", BuildingNumber = "1", FlatNumber = "2", IsPrimary = true,
        });
        command.Addresses[0].IsPrimary = true;

        var result = await BuildValidator().ValidateAsync(command);
        result.IsValid.Should().BeFalse();
    }
}

using AutoMapper;
using FluentAssertions;
using Threes.Registration.Application.Common.Mapping;
using Threes.Registration.Application.Registrations.Queries.GetRegistrationById;
using Threes.Registration.Domain.Registrations;
using Xunit;

namespace Threes.Registration.UnitTests.Application;

public class MappingTests
{
    private static MapperConfiguration BuildConfig() =>
        new(cfg => cfg.AddProfile<RegistrationMappingProfile>());

    [Fact]
    public void Mapping_configuration_is_valid()
    {
        // catches any unmapped member that is not explicitly ignored.
        BuildConfig().AssertConfigurationIsValid();
    }

    [Fact]
    public void Registration_maps_value_objects_to_their_primitives()
    {
        var registration = RegistrationAggregate.Create(
            "Mohammed", "Ahmed", "Ghaly",
            new DateOnly(1995, 4, 12),
            "+201006158123",
            "Mohammed@Example.com",
            new[] { Address.Create(1, 101, "Abbas El Akkad", "12A", "10/2", true) },
            new DateOnly(2026, 6, 7),
            new DateTimeOffset(2026, 6, 7, 10, 0, 0, TimeSpan.Zero),
            "test");

        var mapper = BuildConfig().CreateMapper();
        var dto = mapper.Map<RegistrationDetailsDto>(registration);

        dto.FirstName.Should().Be("Mohammed");
        dto.MiddleName.Should().Be("Ahmed");
        dto.LastName.Should().Be("Ghaly");
        dto.Email.Should().Be("Mohammed@Example.com");
        dto.MobileNumber.Should().Be("+201006158123");
        dto.Addresses.Should().ContainSingle();
        dto.Addresses[0].BuildingNumber.Should().Be("12A");
        dto.Addresses[0].IsPrimary.Should().BeTrue();
    }
}

using FluentAssertions;
using Threes.Registration.Domain.Common;
using Threes.Registration.Domain.Registrations;
using Threes.Registration.Domain.Registrations.Events;
using Xunit;

namespace Threes.Registration.UnitTests.Domain;

public class RegistrationAggregateTests
{
    private static readonly DateOnly Today = new(2026, 6, 7);
    private static readonly DateTimeOffset Now = new(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);

    private static Address MakeAddress(int governorateId = 1, int cityId = 101, bool isPrimary = false) =>
        Address.Create(governorateId, cityId, "Main Street", "12A", "3", isPrimary);

    private static RegistrationAggregate Create(params Address[] addresses) =>
        RegistrationAggregate.Create(
            "Mohammed", null, "Ghaly",
            new DateOnly(1995, 1, 1),
            "+201006158123",
            "mohammed@example.com",
            addresses,
            Today, Now, "test");

    [Fact]
    public void Create_with_one_address_marks_it_primary_even_when_not_requested()
    {
        var registration = Create(MakeAddress(isPrimary: false));

        registration.Addresses.Should().HaveCount(1);
        registration.Addresses.Single().IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void Create_raises_a_registration_created_event()
    {
        var registration = Create(MakeAddress());

        registration.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<RegistrationCreatedDomainEvent>();
    }

    [Fact]
    public void Create_rejects_zero_addresses()
    {
        var act = () => Create();
        act.Should().Throw<DomainException>().WithMessage("*at least one address*");
    }

    [Fact]
    public void Create_rejects_more_than_five_addresses()
    {
        var six = Enumerable.Range(0, 6).Select(_ => MakeAddress()).ToArray();
        var act = () => Create(six);
        act.Should().Throw<DomainException>().WithMessage("*at most 5*");
    }

    [Fact]
    public void Create_rejects_two_primary_addresses()
    {
        var act = () => Create(MakeAddress(isPrimary: true), MakeAddress(cityId: 102, isPrimary: true));
        act.Should().Throw<DomainException>().WithMessage("*one address*primary*");
    }

    [Fact]
    public void AddAddress_beyond_five_is_rejected()
    {
        var registration = Create(MakeAddress(), MakeAddress(cityId: 102), MakeAddress(cityId: 103), MakeAddress(cityId: 104), MakeAddress(cityId: 105));
        var act = () => registration.AddAddress(MakeAddress(cityId: 101), Now);
        act.Should().Throw<DomainException>().WithMessage("*at most 5*");
    }

    [Fact]
    public void RemoveAddress_promotes_a_new_primary_when_the_primary_is_removed()
    {
        var primary = MakeAddress(cityId: 101, isPrimary: true);
        var second = MakeAddress(cityId: 102);
        var registration = Create(primary, second);

        registration.RemoveAddress(primary.Id, Now);

        registration.Addresses.Should().HaveCount(1);
        registration.Addresses.Single().IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void RemoveAddress_rejects_removing_the_last_address()
    {
        var only = MakeAddress();
        var registration = Create(only);

        var act = () => registration.RemoveAddress(only.Id, Now);
        act.Should().Throw<DomainException>().WithMessage("*at least one address*");
    }
}

using Threes.Registration.Domain.Common;
using Threes.Registration.Domain.ValueObjects;

namespace Threes.Registration.Domain.Registrations;

// an address belonging to a registration. it is part of the registration
// aggregate, so it never gets saved or loaded on its own. the governorate /
// city links are kept as plain ids here; checking that they exist and that the
// city really belongs to the governorate needs the database, so that check
// lives in the application layer.
public sealed class Address : Entity
{
    private Address()
    {
        // ef core materialisation ctor.
    }

    private Address(
        Guid id,
        int governorateId,
        int cityId,
        Street street,
        BuildingNumber buildingNumber,
        FlatNumber flatNumber,
        bool isPrimary) : base(id)
    {
        GovernorateId = governorateId;
        CityId = cityId;
        Street = street;
        BuildingNumber = buildingNumber;
        FlatNumber = flatNumber;
        IsPrimary = isPrimary;
    }

    public int GovernorateId { get; private set; }
    public int CityId { get; private set; }
    public Street Street { get; private set; } = null!;
    public BuildingNumber BuildingNumber { get; private set; } = null!;
    public FlatNumber FlatNumber { get; private set; } = null!;
    public bool IsPrimary { get; private set; }

    // back-reference to the owning registration.
    public Guid RegistrationId { get; private set; }

    public static Address Create(
        int governorateId,
        int cityId,
        string? street,
        string? buildingNumber,
        string? flatNumber,
        bool isPrimary)
    {
        if (governorateId <= 0)
        {
            throw new DomainException("Governorate is required.");
        }

        if (cityId <= 0)
        {
            throw new DomainException("City is required.");
        }

        return new Address(
            Guid.NewGuid(),
            governorateId,
            cityId,
            Street.Create(street),
            BuildingNumber.Create(buildingNumber),
            FlatNumber.Create(flatNumber),
            isPrimary);
    }

    // these are internal so only the registration aggregate can flip the flag
    // or wire up the parent id. callers go through Registration, never here.
    internal void AttachTo(Guid registrationId) => RegistrationId = registrationId;

    internal void MarkAsPrimary() => IsPrimary = true;

    internal void ClearPrimary() => IsPrimary = false;
}

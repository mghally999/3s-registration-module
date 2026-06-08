using Threes.Registration.Domain.Common;
using Threes.Registration.Domain.Registrations.Events;
using Threes.Registration.Domain.ValueObjects;

namespace Threes.Registration.Domain.Registrations;

// the aggregate root. a person plus the addresses they registered with. all of
// the field-level rules are enforced by the value objects, and the collection
// rules (at least one address, at most five, only one primary) are enforced
// here so the registration can never exist in a half-valid state.
public sealed class Registration : AggregateRoot
{
    private readonly AddressBook _addresses = new();

    private Registration()
    {
        // ef core materialisation ctor.
    }

    public PersonName FirstName { get; private set; } = null!;
    public PersonName? MiddleName { get; private set; }
    public PersonName LastName { get; private set; } = null!;
    public BirthDate BirthDate { get; private set; } = null!;
    public MobileNumber Mobile { get; private set; } = null!;
    public EmailAddress Email { get; private set; } = null!;

    // exposed read-only. ef maps the private _addresses backing field.
    public IReadOnlyCollection<Address> Addresses => _addresses;

    // audit columns. set on create; UpdatedAt/By move whenever the aggregate
    // is mutated after creation.
    //
    // Audit user is a conscious choice: there is no authentication in this task,
    // so the create flow stamps CreatedBy = "self-registration" (passed in by the
    // command handler) and post-creation mutators default UpdatedBy = "system".
    // When auth is added, an ICurrentUserProvider would supply the real principal
    // to Create/the mutators without changing this aggregate's shape.
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public string CreatedBy { get; private set; } = "system";
    public DateTimeOffset? UpdatedAtUtc { get; private set; }
    public string? UpdatedBy { get; private set; }

    // the one entry point for building a registration. addresses are passed in
    // already built (each one went through Address.Create), and we apply the
    // collection rules to the whole set.
    public static Registration Create(
        string? firstName,
        string? middleName,
        string? lastName,
        DateOnly birthDate,
        string mobileE164,
        string? email,
        IReadOnlyList<Address> addresses,
        DateOnly today,
        DateTimeOffset nowUtc,
        string createdBy)
    {
        var registration = new Registration
        {
            Id = Guid.NewGuid(),
            FirstName = PersonName.Create(firstName, "First name"),
            MiddleName = string.IsNullOrWhiteSpace(middleName)
                ? null
                : PersonName.Create(middleName, "Middle name"),
            LastName = PersonName.Create(lastName, "Last name"),
            BirthDate = BirthDate.Create(birthDate, today),
            Mobile = MobileNumber.Create(mobileE164),
            Email = EmailAddress.Create(email),
            CreatedAtUtc = nowUtc,
            CreatedBy = string.IsNullOrWhiteSpace(createdBy) ? "system" : createdBy,
        };

        registration.SetInitialAddresses(addresses);

        registration.Raise(new RegistrationCreatedDomainEvent(
            registration.Id,
            registration.Email.Value,
            registration.Mobile.Value,
            nowUtc));

        return registration;
    }

    private void SetInitialAddresses(IReadOnlyList<Address> addresses)
    {
        if (addresses is null || addresses.Count < AddressBook.MinAddresses)
        {
            throw new DomainException("At least one address is required.");
        }

        if (addresses.Count > AddressBook.MaxAddresses)
        {
            throw new DomainException(
                $"A registration can have at most {AddressBook.MaxAddresses} addresses.");
        }

        var primaryCount = 0;
        foreach (var address in addresses)
        {
            if (address.IsPrimary)
            {
                primaryCount++;
            }
        }

        if (primaryCount > 1)
        {
            throw new DomainException("Only one address can be marked as primary.");
        }

        foreach (var address in addresses)
        {
            address.AttachTo(Id);
            _addresses.Add(address);
        }

        // spec: if there is exactly one address it is treated as primary even
        // if the caller did not tick the box.
        if (_addresses.Count == 1 && primaryCount == 0)
        {
            _addresses.First!.MarkAsPrimary();
        }
    }

    // post-creation mutators. there is no update endpoint in this task, but the
    // aggregate still owns these so the rules live in one place and can be unit
    // tested. each one re-checks the invariants. the current time is passed IN
    // (never read from the wall clock here) so the domain stays deterministic and
    // testable — the same discipline Create() already follows.
    public void AddAddress(Address address, DateTimeOffset nowUtc, string updatedBy = "system")
    {
        if (_addresses.Count >= AddressBook.MaxAddresses)
        {
            throw new DomainException(
                $"A registration can have at most {AddressBook.MaxAddresses} addresses.");
        }

        address.AttachTo(Id);

        if (address.IsPrimary)
        {
            _addresses.ClearPrimaryExcept(address);
        }

        _addresses.Add(address);

        if (_addresses.Count == 1)
        {
            _addresses.First!.MarkAsPrimary();
        }

        Touch(nowUtc, updatedBy);
    }

    public void RemoveAddress(Guid addressId, DateTimeOffset nowUtc, string updatedBy = "system")
    {
        if (_addresses.Count <= AddressBook.MinAddresses)
        {
            throw new DomainException("At least one address is required.");
        }

        if (!_addresses.RemoveById(addressId))
        {
            throw new DomainException("Address not found on this registration.");
        }

        // if we just removed the primary, promote whatever is now first so the
        // registration always has exactly one primary.
        if (_addresses.PrimaryCount() == 0 && _addresses.First is not null)
        {
            _addresses.First.MarkAsPrimary();
        }

        Touch(nowUtc, updatedBy);
    }

    public void MarkPrimaryAddress(Guid addressId, DateTimeOffset nowUtc, string updatedBy = "system")
    {
        var target = _addresses.FirstOrDefault(a => a.Id == addressId)
            ?? throw new DomainException("Address not found on this registration.");

        _addresses.ClearPrimaryExcept(target);
        target.MarkAsPrimary();
        Touch(nowUtc, updatedBy);
    }

    private void Touch(DateTimeOffset nowUtc, string updatedBy)
    {
        UpdatedAtUtc = nowUtc;
        UpdatedBy = updatedBy;
    }
}

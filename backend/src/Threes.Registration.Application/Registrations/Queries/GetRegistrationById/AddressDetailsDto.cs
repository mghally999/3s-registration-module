namespace Threes.Registration.Application.Registrations.Queries.GetRegistrationById;

// one address on the details response. governorate/city names are resolved from
// the lookup cache so the client does not have to make extra calls.
public sealed class AddressDetailsDto
{
    public Guid Id { get; set; }
    public int GovernorateId { get; set; }
    public string GovernorateName { get; set; } = string.Empty;
    public int CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
    public string Street { get; set; } = string.Empty;
    public string BuildingNumber { get; set; } = string.Empty;
    public string FlatNumber { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

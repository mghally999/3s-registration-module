namespace Threes.Registration.Application.Registrations.Commands.CreateRegistration;

// one address as it arrives on the create request. ids are the lookup integer
// ids; the text fields are validated and trimmed downstream.
public sealed class CreateAddressDto
{
    public int GovernorateId { get; set; }
    public int CityId { get; set; }
    public string? Street { get; set; }
    public string? BuildingNumber { get; set; }
    public string? FlatNumber { get; set; }
    public bool IsPrimary { get; set; }
}

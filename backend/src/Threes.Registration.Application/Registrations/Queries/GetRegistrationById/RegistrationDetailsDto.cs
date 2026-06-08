namespace Threes.Registration.Application.Registrations.Queries.GetRegistrationById;

// the full read model returned by GET /api/registrations/{id}.
public sealed class RegistrationDetailsDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string LastName { get; set; } = string.Empty;
    public DateOnly BirthDate { get; set; }
    public string MobileNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public List<AddressDetailsDto> Addresses { get; set; } = new();
}

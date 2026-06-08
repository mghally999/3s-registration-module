using MediatR;

namespace Threes.Registration.Application.Registrations.Commands.CreateRegistration;

// the create-registration command. it doubles as the request body shape, which
// keeps the api thin (the controller just hands it to mediatr).
public sealed class CreateRegistrationCommand : IRequest<CreateRegistrationResult>
{
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public DateOnly BirthDate { get; set; }
    public string? MobileNumber { get; set; }
    public string? Email { get; set; }
    public List<CreateAddressDto> Addresses { get; set; } = new();
}

namespace Threes.Registration.Application.Registrations.Commands.CreateRegistration;

// returned to the controller so it can build the 201 Created + Location header.
public sealed record CreateRegistrationResult(Guid Id);

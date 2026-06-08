// same aliasing trick as the application project: the "Registration" aggregate
// name collides with the "Threes.Registration" namespace, so we give it a
// stable, non-ambiguous alias for use throughout persistence.
global using RegistrationAggregate = Threes.Registration.Domain.Registrations.Registration;

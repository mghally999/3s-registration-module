// the aggregate is named "Registration" and it lives under the
// "Threes.Registration" namespace, so the bare name "Registration" is
// ambiguous everywhere outside the domain project. this alias gives it a
// non-colliding name we can use in handlers, repositories and mapping.
global using RegistrationAggregate = Threes.Registration.Domain.Registrations.Registration;

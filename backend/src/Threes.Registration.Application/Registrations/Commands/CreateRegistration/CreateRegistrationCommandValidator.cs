using FluentValidation;
using Threes.Registration.Application.Common.Abstractions;
using Threes.Registration.Application.Common.Validation;
using Threes.Registration.Domain.Registrations;
using Threes.Registration.Domain.ValueObjects;

namespace Threes.Registration.Application.Registrations.Commands.CreateRegistration;

// the front-door validation for a create request. it mirrors every rule the
// domain enforces so a bad request comes back as a clean 400 instead of a
// thrown DomainException. the data-dependent bits (age "today", mobile
// normalization, lookup existence) come from injected services.
public sealed class CreateRegistrationCommandValidator : AbstractValidator<CreateRegistrationCommand>
{
    public CreateRegistrationCommandValidator(
        IDateTimeProvider clock,
        IMobileNumberNormalizer mobileNormalizer,
        ILookupCache lookupCache)
    {
        RuleFor(x => x.FirstName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("First name is required.")
            .Must(v => InputRules.WithinLength(v, PersonName.MaxLength))
            .WithMessage($"First name must be at most {PersonName.MaxLength} characters.")
            .Must(InputRules.IsValidName)
            .WithMessage("First name may only contain Arabic or English letters, spaces, hyphen and apostrophe.");

        // middle name is optional. only validate it when something was typed.
        When(x => !string.IsNullOrWhiteSpace(x.MiddleName), () =>
        {
            RuleFor(x => x.MiddleName)
                .Cascade(CascadeMode.Stop)
                .Must(v => InputRules.WithinLength(v, PersonName.MaxLength))
                .WithMessage($"Middle name must be at most {PersonName.MaxLength} characters.")
                .Must(InputRules.IsValidName)
                .WithMessage("Middle name may only contain Arabic or English letters, spaces, hyphen and apostrophe.");
        });

        RuleFor(x => x.LastName)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Last name is required.")
            .Must(v => InputRules.WithinLength(v, PersonName.MaxLength))
            .WithMessage($"Last name must be at most {PersonName.MaxLength} characters.")
            .Must(InputRules.IsValidName)
            .WithMessage("Last name may only contain Arabic or English letters, spaces, hyphen and apostrophe.");

        RuleFor(x => x.BirthDate)
            .Cascade(CascadeMode.Stop)
            .Must(d => d != default).WithMessage("Birth date is required.")
            .Must(d => d <= clock.Today).WithMessage("Birth date cannot be in the future.")
            .Must(d => BirthDate.CalculateAge(d, clock.Today) >= BirthDate.MinimumAgeYears)
            .WithMessage($"Minimum age is {BirthDate.MinimumAgeYears} years.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Email is required.")
            .MaximumLength(EmailAddress.MaxLength)
            .WithMessage($"Email must be at most {EmailAddress.MaxLength} characters.")
            .EmailAddress().WithMessage("Email format is not valid.");

        RuleFor(x => x.MobileNumber)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Mobile number is required.")
            .Must(raw => mobileNormalizer.TryNormalize(raw, out _))
            .WithMessage("Mobile number must be a valid mobile number, for example +201006158123.");

        RuleFor(x => x.Addresses)
            .NotNull().WithMessage("At least one address is required.")
            .Must(list => list is { Count: >= AddressBook.MinAddresses })
            .WithMessage($"At least {AddressBook.MinAddresses} address is required.")
            .Must(list => list is { Count: <= AddressBook.MaxAddresses })
            .WithMessage($"A registration can have at most {AddressBook.MaxAddresses} addresses.")
            .Must(list => list is null || list.Count(a => a.IsPrimary) <= 1)
            .WithMessage("Only one address can be marked as primary.");

        RuleForEach(x => x.Addresses).SetValidator(new CreateAddressValidator(lookupCache));
    }
}

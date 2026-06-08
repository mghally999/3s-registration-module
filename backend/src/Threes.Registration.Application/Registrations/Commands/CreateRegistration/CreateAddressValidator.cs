using FluentValidation;
using Threes.Registration.Application.Common.Abstractions;
using Threes.Registration.Application.Common.Validation;
using Threes.Registration.Domain.ValueObjects;

namespace Threes.Registration.Application.Registrations.Commands.CreateRegistration;

// validates a single address line, including the two checks that need the
// database: the governorate must exist and the city must both exist and belong
// to that governorate. those run through the lookup cache so they stay cheap.
public sealed class CreateAddressValidator : AbstractValidator<CreateAddressDto>
{
    public CreateAddressValidator(ILookupCache lookupCache)
    {
        RuleFor(x => x.GovernorateId)
            .GreaterThan(0).WithMessage("Governorate is required.")
            .MustAsync((id, ct) => lookupCache.GovernorateExistsAsync(id, ct))
            .WithMessage("Governorate does not exist.")
            .When(x => x.GovernorateId > 0);

        RuleFor(x => x.CityId)
            .GreaterThan(0).WithMessage("City is required.");

        // city-belongs-to-governorate. only worth checking once both ids look
        // sane, otherwise we'd stack a confusing second error on an empty field.
        RuleFor(x => x)
            .MustAsync((dto, ct) =>
                lookupCache.CityBelongsToGovernorateAsync(dto.CityId, dto.GovernorateId, ct))
            .WithName(nameof(CreateAddressDto.CityId))
            .WithMessage("City does not exist or does not belong to the selected governorate.")
            .When(x => x.GovernorateId > 0 && x.CityId > 0);

        RuleFor(x => x.Street)
            .NotEmpty().WithMessage("Street is required.")
            .Must(v => InputRules.WithinLength(v, Street.MaxLength))
            .WithMessage($"Street must be at most {Street.MaxLength} characters.");

        RuleFor(x => x.BuildingNumber)
            .NotEmpty().WithMessage("Building number is required.")
            .Must(v => InputRules.WithinLength(v, BuildingNumber.MaxLength))
            .WithMessage($"Building number must be at most {BuildingNumber.MaxLength} characters.")
            .Must(InputRules.IsValidBuildingOrFlat)
            .WithMessage("Building number may only contain letters, numbers, slash, dash and spaces.");

        RuleFor(x => x.FlatNumber)
            .NotEmpty().WithMessage("Flat number is required.")
            .Must(v => InputRules.WithinLength(v, FlatNumber.MaxLength))
            .WithMessage($"Flat number must be at most {FlatNumber.MaxLength} characters.")
            .Must(InputRules.IsValidBuildingOrFlat)
            .WithMessage("Flat number may only contain letters, numbers, slash, dash and spaces.");
    }
}

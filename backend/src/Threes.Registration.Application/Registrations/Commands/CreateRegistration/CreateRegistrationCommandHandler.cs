using MediatR;
using Microsoft.Extensions.Logging;
using Threes.Registration.Application.Common.Abstractions;
using Threes.Registration.Application.Common.Exceptions;
using Threes.Registration.Domain.Registrations;

namespace Threes.Registration.Application.Registrations.Commands.CreateRegistration;

// the write side. by the time we get here fluentvalidation has already cleared
// the structural rules, so this handler is about the two things that need the
// database: rejecting duplicates (409) and persisting the aggregate.
public sealed class CreateRegistrationCommandHandler
    : IRequestHandler<CreateRegistrationCommand, CreateRegistrationResult>
{
    private readonly IRegistrationRepository _registrations;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMobileNumberNormalizer _mobileNormalizer;
    private readonly IDateTimeProvider _clock;
    private readonly ILogger<CreateRegistrationCommandHandler> _logger;

    public CreateRegistrationCommandHandler(
        IRegistrationRepository registrations,
        IUnitOfWork unitOfWork,
        IMobileNumberNormalizer mobileNormalizer,
        IDateTimeProvider clock,
        ILogger<CreateRegistrationCommandHandler> logger)
    {
        _registrations = registrations;
        _unitOfWork = unitOfWork;
        _mobileNormalizer = mobileNormalizer;
        _clock = clock;
        _logger = logger;
    }

    public async Task<CreateRegistrationResult> Handle(
        CreateRegistrationCommand request,
        CancellationToken cancellationToken)
    {
        // normalize the mobile to e.164. the validator already proved this
        // succeeds, but we need the canonical value to store and compare on.
        if (!_mobileNormalizer.TryNormalize(request.MobileNumber, out var mobileE164))
        {
            throw new ValidationException(new Dictionary<string, string[]>
            {
                [nameof(request.MobileNumber)] = new[] { "Mobile number is not a valid mobile number." },
            });
        }

        var normalizedEmail = request.Email!.Trim().ToLowerInvariant();

        // pre-flight duplicate check so the common case returns a clean 409
        // without hitting a unique-index violation. the unit of work still
        // guards the race at save time.
        if (await _registrations.ExistsByNormalizedEmailAsync(normalizedEmail, cancellationToken))
        {
            _logger.LogInformation("rejected duplicate email on registration create");
            throw new ConflictException("A registration with this email already exists.", "email");
        }

        if (await _registrations.ExistsByMobileAsync(mobileE164, cancellationToken))
        {
            _logger.LogInformation("rejected duplicate mobile number on registration create");
            throw new ConflictException("A registration with this mobile number already exists.", "mobileNumber");
        }

        // build each address through the aggregate's factory so the field rules
        // (value objects) run again here as the last guard.
        var addresses = request.Addresses
            .Select(a => Address.Create(
                a.GovernorateId,
                a.CityId,
                a.Street,
                a.BuildingNumber,
                a.FlatNumber,
                a.IsPrimary))
            .ToList();

        var registration = Domain.Registrations.Registration.Create(
            request.FirstName,
            request.MiddleName,
            request.LastName,
            request.BirthDate,
            mobileE164,
            request.Email,
            addresses,
            _clock.Today,
            _clock.UtcNow,
            createdBy: "self-registration");

        await _registrations.AddAsync(registration, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("created registration {RegistrationId}", registration.Id);

        return new CreateRegistrationResult(registration.Id);
    }
}

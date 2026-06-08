using AutoMapper;
using MediatR;
using Threes.Registration.Application.Common.Abstractions;
using Threes.Registration.Application.Common.Exceptions;

namespace Threes.Registration.Application.Registrations.Queries.GetRegistrationById;

// reads a registration, maps it to the dto, then decorates each address with
// its governorate/city display names pulled from the lookup cache.
public sealed class GetRegistrationByIdQueryHandler
    : IRequestHandler<GetRegistrationByIdQuery, RegistrationDetailsDto>
{
    private readonly IRegistrationRepository _registrations;
    private readonly ILookupCache _lookupCache;
    private readonly IMapper _mapper;

    public GetRegistrationByIdQueryHandler(
        IRegistrationRepository registrations,
        ILookupCache lookupCache,
        IMapper mapper)
    {
        _registrations = registrations;
        _lookupCache = lookupCache;
        _mapper = mapper;
    }

    public async Task<RegistrationDetailsDto> Handle(
        GetRegistrationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var registration = await _registrations.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new NotFoundException("Registration", request.Id);

        var dto = _mapper.Map<RegistrationDetailsDto>(registration);

        // resolve names per governorate. group addresses by governorate so we
        // ask the cache once per governorate rather than once per address.
        foreach (var byGovernorate in dto.Addresses.GroupBy(a => a.GovernorateId))
        {
            var governorate = (await _lookupCache.GetGovernoratesAsync(cancellationToken))
                .FirstOrDefault(g => g.Id == byGovernorate.Key);

            var cities = await _lookupCache.GetCitiesAsync(byGovernorate.Key, cancellationToken);

            foreach (var address in byGovernorate)
            {
                address.GovernorateName = governorate?.Name ?? string.Empty;
                address.CityName = cities.FirstOrDefault(c => c.Id == address.CityId)?.Name ?? string.Empty;
            }
        }

        return dto;
    }
}

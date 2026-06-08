using MediatR;
using Threes.Registration.Application.Common.Abstractions;
using Threes.Registration.Application.Lookups.Contracts;

namespace Threes.Registration.Application.Lookups.Queries.GetCities;

// returns the cities for one governorate, sorted by name. an unknown
// governorate id just comes back as an empty list.
public sealed record GetCitiesQuery(int GovernorateId) : IRequest<IReadOnlyList<CityDto>>;

public sealed class GetCitiesQueryHandler
    : IRequestHandler<GetCitiesQuery, IReadOnlyList<CityDto>>
{
    private readonly ILookupCache _lookupCache;

    public GetCitiesQueryHandler(ILookupCache lookupCache) => _lookupCache = lookupCache;

    public Task<IReadOnlyList<CityDto>> Handle(
        GetCitiesQuery request,
        CancellationToken cancellationToken) =>
        _lookupCache.GetCitiesAsync(request.GovernorateId, cancellationToken);
}

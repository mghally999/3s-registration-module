using MediatR;
using Threes.Registration.Application.Common.Abstractions;
using Threes.Registration.Application.Lookups.Contracts;

namespace Threes.Registration.Application.Lookups.Queries.GetGovernorates;

// returns the active governorates, already sorted by name by the cache.
public sealed record GetGovernoratesQuery : IRequest<IReadOnlyList<GovernorateDto>>;

public sealed class GetGovernoratesQueryHandler
    : IRequestHandler<GetGovernoratesQuery, IReadOnlyList<GovernorateDto>>
{
    private readonly ILookupCache _lookupCache;

    public GetGovernoratesQueryHandler(ILookupCache lookupCache) => _lookupCache = lookupCache;

    public Task<IReadOnlyList<GovernorateDto>> Handle(
        GetGovernoratesQuery request,
        CancellationToken cancellationToken) =>
        _lookupCache.GetGovernoratesAsync(cancellationToken);
}

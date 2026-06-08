using Threes.Registration.Application.Lookups.Contracts;

namespace Threes.Registration.Application.Common.Abstractions;

// raw read access to the lookup tables. the implementation just pulls rows out
// of the database. it is wrapped by ILookupCache, which is the thing handlers
// and validators actually use.
public interface ILookupReadStore
{
    Task<IReadOnlyList<GovernorateDto>> GetActiveGovernoratesAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<CityDto>> GetActiveCitiesAsync(CancellationToken cancellationToken);
}

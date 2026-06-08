using Threes.Registration.Application.Lookups.Contracts;

namespace Threes.Registration.Application.Common.Abstractions;

// an in-memory view over the lookup tables. lookups change rarely but get read
// on every form load and every registration validation, so we load them once,
// index them, and answer from memory.
//
// the implementation (see infrastructure) is where the data-structure work
// happens: a hashmap from governorate id to its cities, each city bucket kept
// sorted by name (merge sort) for display and sorted by id (binary search) for
// the belongs-to check.
public interface ILookupCache
{
    Task<IReadOnlyList<GovernorateDto>> GetGovernoratesAsync(CancellationToken cancellationToken);

    // cities for one governorate, already sorted by name. empty if the
    // governorate is unknown.
    Task<IReadOnlyList<CityDto>> GetCitiesAsync(int governorateId, CancellationToken cancellationToken);

    Task<bool> GovernorateExistsAsync(int governorateId, CancellationToken cancellationToken);

    // true only if the city exists AND hangs off the given governorate.
    Task<bool> CityBelongsToGovernorateAsync(int cityId, int governorateId, CancellationToken cancellationToken);

    // drop the cached snapshot so the next read reloads from the database.
    void Invalidate();
}

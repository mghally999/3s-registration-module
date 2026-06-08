using Microsoft.Extensions.DependencyInjection;
using Threes.Registration.Application.Common.Abstractions;
using Threes.Registration.Application.Common.Algorithms;
using Threes.Registration.Application.Lookups.Contracts;

namespace Threes.Registration.Infrastructure.Lookups;

// the in-memory lookup cache. this is where the data-structure work the task
// asked for actually earns its keep:
//
//   * a hashmap (Dictionary) from governorate id to its bucket of cities, so
//     "give me the cities for governorate X" is an o(1) bucket fetch.
//   * merge sort to order governorates and cities by name once, up front, so
//     every dropdown is already alphabetised (stable, culture-aware).
//   * binary search over a by-id-sorted copy of each bucket, so the
//     "does this city belong to this governorate" check that runs on every
//     submitted address is o(log n) instead of a linear scan.
//
// the snapshot is built lazily and reused until something calls Invalidate.
public sealed class LookupCache : ILookupCache
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private volatile Snapshot? _snapshot;

    public LookupCache(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    public async Task<IReadOnlyList<GovernorateDto>> GetGovernoratesAsync(CancellationToken cancellationToken)
    {
        var snapshot = await GetSnapshotAsync(cancellationToken);
        return snapshot.GovernoratesByName;
    }

    public async Task<IReadOnlyList<CityDto>> GetCitiesAsync(int governorateId, CancellationToken cancellationToken)
    {
        var snapshot = await GetSnapshotAsync(cancellationToken);
        return snapshot.CitiesByGovernorate.TryGetValue(governorateId, out var bucket)
            ? bucket.ByName
            : Array.Empty<CityDto>();
    }

    public async Task<bool> GovernorateExistsAsync(int governorateId, CancellationToken cancellationToken)
    {
        var snapshot = await GetSnapshotAsync(cancellationToken);
        return snapshot.GovernorateIds.Contains(governorateId);
    }

    public async Task<bool> CityBelongsToGovernorateAsync(
        int cityId,
        int governorateId,
        CancellationToken cancellationToken)
    {
        var snapshot = await GetSnapshotAsync(cancellationToken);

        // o(1) bucket fetch, then o(log n) probe inside the bucket.
        if (!snapshot.CitiesByGovernorate.TryGetValue(governorateId, out var bucket))
        {
            return false;
        }

        return BinarySearch.Contains(bucket.ById, cityId, c => c.Id);
    }

    public void Invalidate() => _snapshot = null;

    private async Task<Snapshot> GetSnapshotAsync(CancellationToken cancellationToken)
    {
        var current = _snapshot;
        if (current is not null)
        {
            return current;
        }

        await _gate.WaitAsync(cancellationToken);
        try
        {
            // double-check: another caller may have built it while we waited.
            if (_snapshot is not null)
            {
                return _snapshot;
            }

            var built = await BuildAsync(cancellationToken);
            _snapshot = built;
            return built;
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<Snapshot> BuildAsync(CancellationToken cancellationToken)
    {
        // the cache outlives a request scope, so open a short-lived scope to
        // borrow the scoped read store / dbcontext.
        using var scope = _scopeFactory.CreateScope();
        var store = scope.ServiceProvider.GetRequiredService<ILookupReadStore>();

        var governorates = await store.GetActiveGovernoratesAsync(cancellationToken);
        var cities = await store.GetActiveCitiesAsync(cancellationToken);

        var governoratesByName = MergeSort.Sort(governorates, g => g.Name);
        var governorateIds = governorates.Select(g => g.Id).ToHashSet();

        var byId = Comparer<CityDto>.Create((a, b) => a.Id.CompareTo(b.Id));
        var buckets = new Dictionary<int, CityBucket>();

        foreach (var group in cities.GroupBy(c => c.GovernorateId))
        {
            var list = group.ToList();
            buckets[group.Key] = new CityBucket(
                ByName: MergeSort.Sort(list, c => c.Name),
                ById: MergeSort.Sort(list, byId));
        }

        return new Snapshot(governoratesByName, governorateIds, buckets);
    }

    private sealed record Snapshot(
        GovernorateDto[] GovernoratesByName,
        HashSet<int> GovernorateIds,
        Dictionary<int, CityBucket> CitiesByGovernorate);

    // two sorted views of the same cities: one by name for display, one by id
    // for the binary-search membership check.
    private sealed record CityBucket(CityDto[] ByName, CityDto[] ById);
}

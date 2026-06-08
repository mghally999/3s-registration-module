using Microsoft.EntityFrameworkCore;
using Threes.Registration.Application.Common.Abstractions;
using Threes.Registration.Application.Lookups.Contracts;

namespace Threes.Registration.Persistence.Repositories;

// pulls the raw active lookup rows out of the database. ordering, indexing and
// caching are the cache layer's job (infrastructure); this just reads.
public sealed class LookupReadStore : ILookupReadStore
{
    private readonly RegistrationDbContext _db;

    public LookupReadStore(RegistrationDbContext db) => _db = db;

    public async Task<IReadOnlyList<GovernorateDto>> GetActiveGovernoratesAsync(CancellationToken cancellationToken) =>
        await _db.Governorates
            .AsNoTracking()
            .Where(g => g.IsActive)
            .Select(g => new GovernorateDto(g.Id, g.Name))
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CityDto>> GetActiveCitiesAsync(CancellationToken cancellationToken) =>
        await _db.Cities
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Select(c => new CityDto(c.Id, c.GovernorateId, c.Name))
            .ToListAsync(cancellationToken);
}

using Microsoft.EntityFrameworkCore;
using Threes.Registration.Application.Common.Abstractions;
using Threes.Registration.Application.Common.Models;
using Threes.Registration.Application.Registrations.Queries.SearchRegistrations;

namespace Threes.Registration.Persistence.Repositories;

// the ef core implementation of the registration repository. all the reads use
// the queryable columns so they push down to sql.
public sealed class RegistrationRepository : IRegistrationRepository
{
    private readonly RegistrationDbContext _db;

    public RegistrationRepository(RegistrationDbContext db) => _db = db;

    public async Task AddAsync(RegistrationAggregate registration, CancellationToken cancellationToken) =>
        await _db.Registrations.AddAsync(registration, cancellationToken);

    public Task<RegistrationAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        _db.Registrations
            .Include(r => r.Addresses)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken) =>
        _db.Registrations.AnyAsync(r => r.Email.Normalized == normalizedEmail, cancellationToken);

    public Task<bool> ExistsByMobileAsync(string mobileE164, CancellationToken cancellationToken) =>
        _db.Registrations.AnyAsync(r => r.Mobile.Value == mobileE164, cancellationToken);

    public async Task<PagedResult<RegistrationSummaryDto>> SearchAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken)
    {
        var query = _db.Registrations.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            // both columns are stored in a queryable form (normalized email,
            // e.164 mobile), so this pushes down to sql.
            var term = search.Trim().ToLowerInvariant();
            query = query.Where(r =>
                r.Email.Normalized.Contains(term) || r.Mobile.Value.Contains(term));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // project to the value objects + a correlated address count, newest
        // first, then unwrap to primitives in memory (value-object members do
        // not translate inside a projection).
        var rows = await query
            .OrderByDescending(r => r.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(r => new
            {
                r.Id,
                r.FirstName,
                r.LastName,
                r.Email,
                r.Mobile,
                r.CreatedAtUtc,
                AddressCount = r.Addresses.Count,
            })
            .ToListAsync(cancellationToken);

        var items = rows
            .Select(r => new RegistrationSummaryDto(
                r.Id,
                $"{r.FirstName.Value} {r.LastName.Value}",
                r.Email.Value,
                r.Mobile.Value,
                r.CreatedAtUtc,
                r.AddressCount))
            .ToList();

        return new PagedResult<RegistrationSummaryDto>(items, page, pageSize, totalCount);
    }
}

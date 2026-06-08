using Threes.Registration.Application.Common.Models;
using Threes.Registration.Application.Registrations.Queries.SearchRegistrations;

namespace Threes.Registration.Application.Common.Abstractions;

// the persistence contract for the registration aggregate. the application
// layer talks to this, never to ef core directly, so the dependency arrow
// keeps pointing inward.
public interface IRegistrationRepository
{
    Task AddAsync(RegistrationAggregate registration, CancellationToken cancellationToken);

    // loads the registration together with its addresses, or null if missing.
    Task<RegistrationAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    // a page of registration summaries, optionally filtered by email/mobile.
    Task<PagedResult<RegistrationSummaryDto>> SearchAsync(
        int page,
        int pageSize,
        string? search,
        CancellationToken cancellationToken);

    // uniqueness checks done against the normalized columns.
    Task<bool> ExistsByNormalizedEmailAsync(string normalizedEmail, CancellationToken cancellationToken);

    Task<bool> ExistsByMobileAsync(string mobileE164, CancellationToken cancellationToken);
}

using MediatR;
using Threes.Registration.Application.Common.Abstractions;
using Threes.Registration.Application.Common.Models;

namespace Threes.Registration.Application.Registrations.Queries.SearchRegistrations;

// paged + searchable listing of registrations. search matches on the
// normalized email or the mobile number (both stored in queryable columns).
public sealed record SearchRegistrationsQuery(int Page = 1, int PageSize = 20, string? Search = null)
    : IRequest<PagedResult<RegistrationSummaryDto>>;

public sealed class SearchRegistrationsQueryHandler
    : IRequestHandler<SearchRegistrationsQuery, PagedResult<RegistrationSummaryDto>>
{
    private const int MaxPageSize = 100;

    private readonly IRegistrationRepository _registrations;

    public SearchRegistrationsQueryHandler(IRegistrationRepository registrations) =>
        _registrations = registrations;

    public Task<PagedResult<RegistrationSummaryDto>> Handle(
        SearchRegistrationsQuery request,
        CancellationToken cancellationToken)
    {
        // clamp the paging inputs so a bad client cannot ask for page 0 or a
        // 10000-row page.
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize switch
        {
            < 1 => 20,
            > MaxPageSize => MaxPageSize,
            _ => request.PageSize,
        };

        return _registrations.SearchAsync(page, pageSize, request.Search, cancellationToken);
    }
}

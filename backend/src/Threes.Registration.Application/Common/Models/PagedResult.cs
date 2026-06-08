namespace Threes.Registration.Application.Common.Models;

// a single page of results plus the paging metadata a client needs to render
// pagination controls.
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount)
{
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling(TotalCount / (double)PageSize);
}

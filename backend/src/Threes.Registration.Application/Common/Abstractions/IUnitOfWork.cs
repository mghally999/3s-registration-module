namespace Threes.Registration.Application.Common.Abstractions;

// commits whatever the repositories have staged in one transaction. keeping
// this separate from the repositories means a handler decides the transaction
// boundary, not the repository.
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}

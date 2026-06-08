using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Threes.Registration.Application.Common.Abstractions;
using Threes.Registration.Application.Common.Exceptions;

namespace Threes.Registration.Persistence.Repositories;

// commits the current change set. its second job is to turn a unique-index
// violation into a clean ConflictException, which covers the race where two
// requests pass the pre-flight duplicate check at the same time and only one
// can win at the database. sql server raises 2601/2627 for unique violations.
public sealed class UnitOfWork : IUnitOfWork
{
    private const int UniqueIndexViolation = 2601;
    private const int UniqueConstraintViolation = 2627;

    private readonly RegistrationDbContext _db;

    public UnitOfWork(RegistrationDbContext db) => _db = db;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex, out var sql))
        {
            if (sql!.Message.Contains("EmailNormalized", StringComparison.OrdinalIgnoreCase))
            {
                throw new ConflictException("A registration with this email already exists.", "email");
            }

            if (sql.Message.Contains("MobileNumber", StringComparison.OrdinalIgnoreCase))
            {
                throw new ConflictException("A registration with this mobile number already exists.", "mobileNumber");
            }

            throw new ConflictException("A registration with the same unique value already exists.");
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex, out SqlException? sqlException)
    {
        if (ex.InnerException is SqlException sql &&
            sql.Number is UniqueIndexViolation or UniqueConstraintViolation)
        {
            sqlException = sql;
            return true;
        }

        sqlException = null;
        return false;
    }
}

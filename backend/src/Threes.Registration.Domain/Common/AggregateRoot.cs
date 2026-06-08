namespace Threes.Registration.Domain.Common;

// marks the one entity in a cluster that the outside world is allowed to load
// and save as a unit. for us that is Registration: its addresses only exist
// through it, so nobody saves an Address on its own.
public abstract class AggregateRoot : Entity
{
    protected AggregateRoot(Guid id) : base(id)
    {
    }

    protected AggregateRoot()
    {
    }
}

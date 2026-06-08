using Threes.Registration.Domain.Common.Collections;

namespace Threes.Registration.Domain.Common;

// base class for anything with identity. equality is by id, not by reference,
// which is what you want for entities loaded from the database.
public abstract class Entity
{
    // events raised but not yet dispatched. backed by our singly linked list
    // because we only append and then drain once after save.
    private readonly SinglyLinkedList<IDomainEvent> _domainEvents = new();

    protected Entity(Guid id)
    {
        Id = id;
    }

    // ef core needs a parameterless ctor it can use when materialising rows.
    protected Entity()
    {
    }

    public Guid Id { get; protected set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.ToArray();

    protected void Raise(IDomainEvent domainEvent) => _domainEvents.Append(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (GetType() != other.GetType())
        {
            return false;
        }

        return Id != Guid.Empty && Id == other.Id;
    }

    public override int GetHashCode() => Id.GetHashCode();
}

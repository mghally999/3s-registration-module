using System.Collections;
using Threes.Registration.Domain.Common.Collections;

namespace Threes.Registration.Domain.Registrations;

// the collection of addresses on a registration, backed by our doubly linked
// list. it implements ICollection<Address> for two reasons:
//   1. ef core can add items into it while materialising a row from the db.
//   2. it can be exposed as a read-only navigation on the aggregate.
//
// the business rules (max 5, single primary, etc) are NOT enforced in here on
// purpose. raw Add is the hydration path and must stay dumb, because rows
// coming back from the database were already valid when they were written. the
// rules live on the Registration aggregate, which is the only thing allowed to
// mutate this book.
public sealed class AddressBook : ICollection<Address>, IReadOnlyCollection<Address>
{
    public const int MaxAddresses = 5;
    public const int MinAddresses = 1;

    private readonly DoublyLinkedList<Address> _items = new();

    public int Count => _items.Count;

    public bool IsReadOnly => false;

    // raw append. used by ef core hydration and by the aggregate after it has
    // already checked the rules.
    public void Add(Address item) => _items.AddLast(item);

    public bool Remove(Address item)
    {
        var node = _items.Find(a => a.Equals(item));
        if (node is null)
        {
            return false;
        }

        _items.Remove(node);
        return true;
    }

    public bool RemoveById(Guid addressId)
    {
        var node = _items.Find(a => a.Id == addressId);
        if (node is null)
        {
            return false;
        }

        _items.Remove(node);
        return true;
    }

    public void Clear() => _items.Clear();

    public bool Contains(Address item) => _items.Any(a => a.Equals(item));

    public void CopyTo(Address[] array, int arrayIndex)
    {
        foreach (var item in _items)
        {
            array[arrayIndex++] = item;
        }
    }

    public Address? First => _items.First?.Value;

    public int PrimaryCount() => _items.CountWhere(a => a.IsPrimary);

    public Address? FindPrimary() => _items.Find(a => a.IsPrimary)?.Value;

    // clear the primary flag on every address except the one we are keeping.
    // walks the whole list once, head to tail.
    public void ClearPrimaryExcept(Address keep) =>
        _items.ForEach(a =>
        {
            if (!ReferenceEquals(a, keep))
            {
                a.ClearPrimary();
            }
        });

    public IEnumerator<Address> GetEnumerator() => _items.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

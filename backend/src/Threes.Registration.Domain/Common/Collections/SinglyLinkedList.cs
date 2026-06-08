using System.Collections;

namespace Threes.Registration.Domain.Common.Collections;

// a hand rolled singly linked list.
//
// we use this to hold the domain events that an aggregate raises. events are
// only ever appended at the tail and read back once in order, so a singly
// linked list is the natural fit: o(1) append with a tail pointer, no array
// resizing, and we never need to walk backwards.
//
// yes, the bcl has List<T> and LinkedList<T>. this is written by hand on
// purpose so the data-structure choice is explicit and visible.
public sealed class SinglyLinkedList<T> : IEnumerable<T>
{
    private sealed class Node
    {
        public Node(T value)
        {
            Value = value;
        }

        public T Value { get; }
        public Node? Next { get; set; }
    }

    private Node? _head;
    private Node? _tail;

    public int Count { get; private set; }

    public bool IsEmpty => Count == 0;

    // append to the tail in o(1) using the cached tail pointer.
    public void Append(T value)
    {
        var node = new Node(value);

        if (_head is null)
        {
            _head = node;
            _tail = node;
        }
        else
        {
            _tail!.Next = node;
            _tail = node;
        }

        Count++;
    }

    public void Clear()
    {
        // dropping the head lets the gc collect the whole chain.
        _head = null;
        _tail = null;
        Count = 0;
    }

    // snapshot to a plain array so callers can iterate without touching nodes.
    public T[] ToArray()
    {
        var items = new T[Count];
        var index = 0;
        var current = _head;

        while (current is not null)
        {
            items[index++] = current.Value;
            current = current.Next;
        }

        return items;
    }

    public IEnumerator<T> GetEnumerator()
    {
        var current = _head;
        while (current is not null)
        {
            yield return current.Value;
            current = current.Next;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

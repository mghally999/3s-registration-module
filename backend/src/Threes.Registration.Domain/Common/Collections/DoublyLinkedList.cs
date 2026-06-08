using System.Collections;

namespace Threes.Registration.Domain.Common.Collections;

// a hand rolled doubly linked list.
//
// the registration aggregate keeps its addresses in one of these. an address
// list is small (max 5) but it is genuinely a mutable ordered sequence: the
// user adds at the tail, removes from the middle, and we sometimes need to
// walk it both ways (e.g. when re-pointing the primary flag we look at
// neighbours). a doubly linked list gives o(1) add/remove once you hold the
// node and lets us move forward and backward, which a singly linked list
// cannot do cheaply.
public sealed class DoublyLinkedList<T> : IEnumerable<T>
{
    public sealed class Node
    {
        internal Node(T value)
        {
            Value = value;
        }

        public T Value { get; internal set; }
        public Node? Previous { get; internal set; }
        public Node? Next { get; internal set; }
    }

    private Node? _head;
    private Node? _tail;

    public int Count { get; private set; }

    public bool IsEmpty => Count == 0;

    public Node? First => _head;
    public Node? Last => _tail;

    // add to the tail, wiring up both the next and previous links. o(1).
    public Node AddLast(T value)
    {
        var node = new Node(value);

        if (_tail is null)
        {
            _head = node;
            _tail = node;
        }
        else
        {
            node.Previous = _tail;
            _tail.Next = node;
            _tail = node;
        }

        Count++;
        return node;
    }

    // unlink a node we already hold. o(1) because we have both neighbours.
    public void Remove(Node node)
    {
        var previous = node.Previous;
        var next = node.Next;

        if (previous is null)
        {
            _head = next;
        }
        else
        {
            previous.Next = next;
        }

        if (next is null)
        {
            _tail = previous;
        }
        else
        {
            next.Previous = previous;
        }

        node.Previous = null;
        node.Next = null;
        Count--;
    }

    public void Clear()
    {
        _head = null;
        _tail = null;
        Count = 0;
    }

    // first node whose value matches the predicate, walking head -> tail.
    public Node? Find(Func<T, bool> predicate)
    {
        var current = _head;
        while (current is not null)
        {
            if (predicate(current.Value))
            {
                return current;
            }

            current = current.Next;
        }

        return null;
    }

    public bool Any(Func<T, bool> predicate) => Find(predicate) is not null;

    public int CountWhere(Func<T, bool> predicate)
    {
        var count = 0;
        var current = _head;
        while (current is not null)
        {
            if (predicate(current.Value))
            {
                count++;
            }

            current = current.Next;
        }

        return count;
    }

    public void ForEach(Action<T> action)
    {
        var current = _head;
        while (current is not null)
        {
            action(current.Value);
            current = current.Next;
        }
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

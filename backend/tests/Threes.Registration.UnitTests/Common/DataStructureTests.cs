using FluentAssertions;
using Threes.Registration.Domain.Common.Collections;
using Xunit;

namespace Threes.Registration.UnitTests.Common;

// the custom linked lists back the aggregate's address book and its domain
// events, so they get their own focused tests.
public class DataStructureTests
{
    [Fact]
    public void DoublyLinkedList_adds_to_tail_in_order()
    {
        var list = new DoublyLinkedList<int>();
        list.AddLast(1);
        list.AddLast(2);
        list.AddLast(3);

        list.Count.Should().Be(3);
        list.Should().Equal(1, 2, 3);
        list.First!.Value.Should().Be(1);
        list.Last!.Value.Should().Be(3);
    }

    [Fact]
    public void DoublyLinkedList_removes_a_middle_node_and_relinks_both_sides()
    {
        var list = new DoublyLinkedList<string>();
        list.AddLast("a");
        var middle = list.AddLast("b");
        list.AddLast("c");

        list.Remove(middle);

        list.Should().Equal("a", "c");
        list.Count.Should().Be(2);
        // the relink should hold in both directions.
        list.First!.Next!.Value.Should().Be("c");
        list.Last!.Previous!.Value.Should().Be("a");
    }

    [Fact]
    public void DoublyLinkedList_find_and_countwhere_walk_the_chain()
    {
        var list = new DoublyLinkedList<int>();
        foreach (var n in new[] { 2, 4, 5, 6 })
        {
            list.AddLast(n);
        }

        list.Find(x => x == 5)!.Value.Should().Be(5);
        list.Find(x => x == 99).Should().BeNull();
        list.CountWhere(x => x % 2 == 0).Should().Be(3);
    }

    [Fact]
    public void SinglyLinkedList_appends_and_snapshots_in_order()
    {
        var list = new SinglyLinkedList<int>();
        list.IsEmpty.Should().BeTrue();

        list.Append(10);
        list.Append(20);

        list.IsEmpty.Should().BeFalse();
        list.Count.Should().Be(2);
        list.ToArray().Should().Equal(10, 20);
    }

    [Fact]
    public void SinglyLinkedList_clear_empties_the_chain()
    {
        var list = new SinglyLinkedList<int>();
        list.Append(1);
        list.Append(2);

        list.Clear();

        list.Count.Should().Be(0);
        list.ToArray().Should().BeEmpty();
    }
}

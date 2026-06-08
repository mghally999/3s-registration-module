using FluentAssertions;
using Threes.Registration.Application.Common.Algorithms;
using Xunit;

namespace Threes.Registration.UnitTests.Common;

public class AlgorithmTests
{
    [Fact]
    public void MergeSort_sorts_strings_case_insensitively()
    {
        var input = new[] { "banha", "Aswan", "cairo", "Alexandria" };

        var sorted = MergeSort.Sort(input, s => s);

        sorted.Should().Equal("Alexandria", "Aswan", "banha", "cairo");
    }

    [Fact]
    public void MergeSort_is_stable_for_equal_keys()
    {
        // same key ("x"), different tag. a stable sort keeps original order.
        var input = new[] { ("x", 1), ("x", 2), ("a", 3), ("x", 4) };

        var sorted = MergeSort.Sort(input, t => t.Item1);

        sorted.Should().Equal(("a", 3), ("x", 1), ("x", 2), ("x", 4));
    }

    [Fact]
    public void MergeSort_handles_empty_and_single()
    {
        MergeSort.Sort(Array.Empty<int>(), Comparer<int>.Default).Should().BeEmpty();
        MergeSort.Sort(new[] { 42 }, Comparer<int>.Default).Should().Equal(42);
    }

    [Fact]
    public void BinarySearch_finds_existing_keys()
    {
        var sorted = new[] { 101, 102, 103, 104, 105 };

        BinarySearch.Contains(sorted, 101, x => x).Should().BeTrue();
        BinarySearch.Contains(sorted, 103, x => x).Should().BeTrue();
        BinarySearch.Contains(sorted, 105, x => x).Should().BeTrue();
        BinarySearch.IndexOf(sorted, 104, x => x).Should().Be(3);
    }

    [Fact]
    public void BinarySearch_returns_miss_for_absent_keys()
    {
        var sorted = new[] { 2, 4, 6, 8 };

        BinarySearch.Contains(sorted, 5, x => x).Should().BeFalse();
        BinarySearch.IndexOf(sorted, 9, x => x).Should().Be(-1);
        BinarySearch.IndexOf(Array.Empty<int>(), 1, x => x).Should().Be(-1);
    }
}

namespace Threes.Registration.Application.Common.Algorithms;

// a generic, stable, recursive merge sort.
//
// we use it to order lookup values (governorates and cities) alphabetically
// before they are cached and served to the dropdowns. stability matters: when
// two names compare equal under a culture-aware comparer we want the original
// relative order kept, which is exactly what merge sort gives and quicksort
// does not. it runs in o(n log n) and the recursion depth is log n.
public static class MergeSort
{
    public static T[] Sort<T>(IReadOnlyList<T> source, IComparer<T> comparer)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(comparer);

        var working = new T[source.Count];
        for (var i = 0; i < source.Count; i++)
        {
            working[i] = source[i];
        }

        if (working.Length < 2)
        {
            return working;
        }

        var buffer = new T[working.Length];
        SortRange(working, buffer, 0, working.Length - 1, comparer);
        return working;
    }

    public static T[] Sort<T>(IReadOnlyList<T> source, Func<T, string> keySelector) =>
        Sort(source, new ProjectionComparer<T>(keySelector));

    // classic divide and conquer: split in half, sort each half, merge.
    private static void SortRange<T>(T[] items, T[] buffer, int left, int right, IComparer<T> comparer)
    {
        if (left >= right)
        {
            return;
        }

        var middle = left + ((right - left) / 2);
        SortRange(items, buffer, left, middle, comparer);
        SortRange(items, buffer, middle + 1, right, comparer);
        Merge(items, buffer, left, middle, right, comparer);
    }

    private static void Merge<T>(T[] items, T[] buffer, int left, int middle, int right, IComparer<T> comparer)
    {
        for (var i = left; i <= right; i++)
        {
            buffer[i] = items[i];
        }

        var leftIndex = left;
        var rightIndex = middle + 1;

        for (var current = left; current <= right; current++)
        {
            if (leftIndex > middle)
            {
                items[current] = buffer[rightIndex++];
            }
            else if (rightIndex > right)
            {
                items[current] = buffer[leftIndex++];
            }
            // "<= 0" keeps equal elements in their original order -> stable.
            else if (comparer.Compare(buffer[leftIndex], buffer[rightIndex]) <= 0)
            {
                items[current] = buffer[leftIndex++];
            }
            else
            {
                items[current] = buffer[rightIndex++];
            }
        }
    }

    // sorts by a string key using a culture-aware, case-insensitive compare so
    // arabic and english names land in a sensible order.
    private sealed class ProjectionComparer<T> : IComparer<T>
    {
        private readonly Func<T, string> _keySelector;

        public ProjectionComparer(Func<T, string> keySelector) => _keySelector = keySelector;

        public int Compare(T? x, T? y) =>
            string.Compare(_keySelector(x!), _keySelector(y!), StringComparison.CurrentCultureIgnoreCase);
    }
}

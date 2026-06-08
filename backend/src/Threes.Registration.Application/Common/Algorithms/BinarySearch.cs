namespace Threes.Registration.Application.Common.Algorithms;

// a generic binary search over an array that is already sorted by the same key
// we search on.
//
// the lookup cache uses this for the "does this city belong to this
// governorate" check: each governorate's cities are kept in an array sorted by
// city id, so confirming membership is an o(log n) probe instead of a linear
// scan. on a hot validation path that runs for every submitted address, the
// log-time lookup is worth keeping.
public static class BinarySearch
{
    // returns the index of the item whose key equals target, or -1 if none.
    public static int IndexOf<T>(IReadOnlyList<T> sorted, int target, Func<T, int> keySelector)
    {
        ArgumentNullException.ThrowIfNull(sorted);
        ArgumentNullException.ThrowIfNull(keySelector);

        var low = 0;
        var high = sorted.Count - 1;

        while (low <= high)
        {
            // (low + high) / 2 can overflow on huge arrays; this form cannot.
            var mid = low + ((high - low) / 2);
            var key = keySelector(sorted[mid]);

            if (key == target)
            {
                return mid;
            }

            if (key < target)
            {
                low = mid + 1;
            }
            else
            {
                high = mid - 1;
            }
        }

        return -1;
    }

    public static bool Contains<T>(IReadOnlyList<T> sorted, int target, Func<T, int> keySelector) =>
        IndexOf(sorted, target, keySelector) >= 0;
}

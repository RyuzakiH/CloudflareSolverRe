using System.Collections.Generic;
using System.Linq;

namespace CloudflareSolverRe.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> eePrepend<T>(this IEnumerable<T> values, T value)
        {
            yield return value;
            foreach (T item in values)
                yield return item;
        }

        public static IEnumerable<T> eeAppend<T>(this IEnumerable<T> values, T value)
        {
            return values.Concat(new T[] { value });
        }

    }
}

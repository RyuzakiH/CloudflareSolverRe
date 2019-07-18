using System.Collections.Generic;
using System.Linq;

namespace CloudflareSolverRe.Extensions
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> Prepend<T>(this IEnumerable<T> values, T value)
        {
            yield return value;
            foreach (T item in values)
                yield return item;
        }

        public static IEnumerable<T> Append<T>(this IEnumerable<T> values, T value)
        {
            return values.Concat(new T[] { value });
        }

    }
}

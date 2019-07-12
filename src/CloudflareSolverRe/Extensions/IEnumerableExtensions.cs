using System.Collections.Generic;

namespace System.Linq
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

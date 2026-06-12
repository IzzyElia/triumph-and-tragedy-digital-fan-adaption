using System.Collections.Generic;

namespace IzzysConsole.Utils
{
    public static class ArrayUtils
    {
        public static T[] ToArray<T>(this IReadOnlyCollection<T> collection)
        {
            T[] array = new T[collection.Count];
            IEnumerator<T> enumerator = collection.GetEnumerator();
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = enumerator.Current;
                enumerator.MoveNext();
            }
            enumerator.Dispose();
            return array;
        }
        public static T[] Concat<T>(this T[] array, T[] with)
        {
            T[] output = new T[array.Length + with.Length];
            array.CopyTo(output, 0);
            with.CopyTo(output, array.Length);
            return output;
        }
    }
}

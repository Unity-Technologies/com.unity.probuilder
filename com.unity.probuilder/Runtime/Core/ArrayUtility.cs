using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder
{
    static class ArrayUtility
    {
        public static T[] ValuesWithIndexes<T>(this T[] arr, int[] indexes)
        {
            T[] vals = new T[indexes.Length];
            for (int i = 0; i < indexes.Length; i++)
                vals[i] = arr[indexes[i]];
            return vals;
        }

        public static List<T> ValuesWithIndexes<T>(this List<T> arr, IList<int> indexes)
        {
            List<T> vals = new List<T>(indexes.Count);

            foreach (var i in indexes)
                vals.Add(arr[i]);

            return vals;
        }

        public static IEnumerable<int> AllIndexesOf<T>(this IList<T> list, Func<T, bool> lambda)
        {
            var indexes = new List<int>();
            for (int i = 0, c = list.Count; i < c; i++)
                if (lambda(list[i]))
                    indexes.Add(i);
            return indexes;
        }

        public static T[] Add<T>(this T[] arr, T val)
        {
            T[] v = new T[arr.Length + 1];
            System.Array.ConstrainedCopy(arr, 0, v, 0, arr.Length);
            v[arr.Length] = val;
            return v;
        }

        public static T[] AddRange<T>(this T[] arr, T[] val)
        {
            T[] ret = new T[arr.Length + val.Length];
            System.Array.ConstrainedCopy(arr, 0, ret, 0, arr.Length);
            System.Array.ConstrainedCopy(val, 0, ret, arr.Length, val.Length);
            return ret;
        }

        /**
         * Remove @val from @arr.
         */
        public static T[] Remove<T>(this T[] arr, T val)
        {
            List<T> list = new List<T>(arr);
            list.Remove(val);
            return list.ToArray();
        }

        public static T[] Remove<T>(this T[] arr, IEnumerable<T> val)
        {
            return arr.Except(val).ToArray();
        }

        public static T[] RemoveAt<T>(this T[] arr, int index)
        {
            T[] newArray = new T[arr.Length - 1];
            int n = 0;
            for (int i = 0; i < arr.Length; i++)
            {
                if (i != index)
                {
                    newArray[n] = arr[i];
                    n++;
                }
            }
            return newArray;
        }

        public static T[] RemoveAt<T>(this IList<T> list, IEnumerable<int> indexes)
        {
            List<int> sorted = new List<int>(indexes);
            sorted.Sort();
            return SortedRemoveAt(list, sorted);
        }

        /**
         * Remove elements at indexes from an array, but accepts a pre-sorted indexes list.
         */
        public static T[] SortedRemoveAt<T>(this IList<T> list, IList<int> sorted)
        {
            int indexeSortedCount = sorted.Count;
            int len = list.Count;

            T[] newArray = new T[len - indexeSortedCount];
            int n = 0;

            for (int i = 0; i < len; i++)
            {
                if (n < indexeSortedCount && sorted[n] == i)
                {
                    // handle duplicate indexes
                    while (n < indexeSortedCount && sorted[n] == i)
                        n++;

                    continue;
                }

                newArray[i - n] = list[i];
            }

            return newArray;
        }

        /**
         * Holds a start and end index for a binary search.
         */
        struct SearchRange
        {
            public int begin, end;

            public SearchRange(int begin, int end)
            {
                this.begin = begin;
                this.end = end;
            }

            public bool Valid() { return end - begin > 1; }
            public int Center() { return begin + (end - begin) / 2; }

            public override string ToString()
            {
                return "{" + begin + ", " + end + "} : " + Center();
            }
        }

        /**
         * Given a sorted list and value, returns the index of the greatest value in sorted_list that is
         * less than value.  Ex: List( { 0, 1, 4, 6, 7 } ) Value(5) returns 2, which is the index of value 4.
         * If value is less than sorted[0], -1 is returned.  If value is greater than sorted[end], sorted.Count-1
         * is returned.  If an exact match is found, the index prior to that match is returned.
         */
        public static int NearestIndexPriorToValue<T>(IList<T> sorted_list, T value) where T : System.IComparable<T>
        {
            int count = sorted_list.Count;
            if (count < 1) return -1;

            SearchRange range = new SearchRange(0, count - 1);

            if (value.CompareTo(sorted_list[0]) < 0)
                return -1;

            if (value.CompareTo(sorted_list[count - 1]) > 0)
                return count - 1;

            while (range.Valid())
            {
                if (sorted_list[range.Center()].CompareTo(value) > 0)
                {
                    // sb.AppendLine(sorted_list[range.Center()] + " > " + value + " [" + range.begin + ", " + range.end +"] -> [" + range.begin + ", " + range.Center() + "]");
                    range.end = range.Center();
                }
                else
                {
                    // sb.AppendLine(sorted_list[range.Center()] + " < " + value + " [" + range.begin + ", " + range.end +"] -> [" + range.Center() + ", " + range.end + "]");
                    range.begin = range.Center();

                    if (sorted_list[range.begin + 1].CompareTo(value) >= 0)
                    {
                        return range.begin;
                    }
                }
            }

            return 0;
        }

        public static List<T> Fill<T>(System.Func<int, T> ctor, int length)
        {
            List<T> list = new List<T>(length);
            for (int i = 0; i < length; i++)
                list.Add(ctor(i));
            return list;
        }

        public static T[] Fill<T>(T val, int length)
        {
            T[] arr = new T[length];
            for (int i = 0; i < length; i++)
            {
                arr[i] = val;
            }
            return arr;
        }

        /**
         * True if any value is present in both arrays.
         */
        public static bool ContainsMatch<T>(this T[] a, T[] b)
        {
            int ind = -1;
            for (int i = 0; i < a.Length; i++)
            {
                ind = System.Array.IndexOf(b, a[i]);
                if (ind > -1) return true;//ind;
            }
            return false;// ind;
        }

        /**
         * True if any value is present in both arrays, setting index_a and index_b to the index in the array of each match.
         */
        public static bool ContainsMatch<T>(this T[] a, T[] b, out int index_a, out int index_b)
        {
            index_b = -1;
            for (index_a = 0; index_a < a.Length; index_a++)
            {
                index_b = Array.IndexOf(b, a[index_a]);
                if (index_b > -1)
                    return true; //ind;
            }

            return false; // ind;
        }

        /**
         * Concatenate two arrays.
         */
        public static T[] Concat<T>(this T[] x, T[] y)
        {
            if (x == null) throw new ArgumentNullException("x");
            if (y == null) throw new ArgumentNullException("y");
            int oldLen = x.Length;
            Array.Resize<T>(ref x, x.Length + y.Length);
            Array.Copy(y, 0, x, oldLen, y.Length);
            return x;
        }

        /**
         * Returns the index of the array that contains this value.  -1 if not found.
         */
        public static int IndexOf<T>(this List<List<T>> InList, T InValue)
        {
            for (int i = 0; i < InList.Count; i++)
            {
                for (int x = 0; x < InList[i].Count; x++)
                    if (InList[i][x].Equals(InValue))
                        return i;
            }

            return -1;
        }

        /**
         *  Generate a new array with count using the constructor.  ctor recieves the index and returns a new instance of T.
         */
        public static T[] Fill<T>(int count, System.Func<int, T> ctor)
        {
            T[] arr = new T[count];
            for (int i = 0; i < count; i++)
                arr[i] = ctor(i);
            return arr;
        }

        /**
         *  Add a value to a key in dictionary, adding a new entry if necessray.
         */
        public static void AddOrAppend<T, K>(this Dictionary<T, List<K>> dictionary, T key, K value)
        {
            List<K> list;

            if (dictionary.TryGetValue(key, out list))
                list.Add(value);
            else
                dictionary.Add(key, new List<K>() { value });
        }

        /**
         *  Add a value to a key in dictionary, adding a new entry if necessray.
         */
        public static void AddOrAppendRange<T, K>(this Dictionary<T, List<K>> dictionary, T key, List<K> value)
        {
            List<K> list;

            if (dictionary.TryGetValue(key, out list))
                list.AddRange(value);
            else
                dictionary.Add(key, value);
        }

        /**
         * http://stackoverflow.com/questions/1300088/distinct-with-lambda
         */
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> knownKeys = new HashSet<TKey>();
            return source.Where(x => knownKeys.Add(keySelector(x)));
        }

        public static string ToString<TKey, TValue>(this Dictionary<TKey, TValue> dict)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            foreach (KeyValuePair<TKey, TValue> kvp in dict)
                sb.AppendLine(string.Format("Key: {0}  Value: {1}", kvp.Key, kvp.Value));
            return sb.ToString();
        }

        public static string ToString<T>(this IEnumerable<T> arr, string separator = ", ")
        {
            return string.Join(separator, arr.Select(x => x.ToString()).ToArray());
        }
    }
}

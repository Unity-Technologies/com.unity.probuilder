using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using UnityEngine.Serialization;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Defines associations between vertex positions that are [coincident](../manual/gloss.html#coincident).
    /// The indexes stored in this collection correspond to the <see cref="ProBuilderMesh.positions" /> array.
    /// </summary>
    /// <remarks>
    /// Coincident vertices are vertices that share the same coordinate position, but are separate entries
    /// in the vertex array.
    /// </remarks>
    [Serializable]
    public sealed class SharedVertex : ICollection<int>
    {
        /// <summary>
        /// Stores an array of vertex indices that are coincident.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("array")]
        [FormerlySerializedAs("m_Vertexes")]
        int[] m_Vertices;

        internal int[] arrayInternal
        {
            get { return m_Vertices; }
        }

        /// <summary>
        /// Creates a new SharedVertex from the specified indices.
        /// </summary>
        /// <param name="indexes">An array of indices to set.</param>
        public SharedVertex(IEnumerable<int> indexes)
        {
            if (indexes == null)
                throw new ArgumentNullException("indexes");
            m_Vertices = indexes.ToArray();
        }

        /// <summary>
        /// Creates a new SharedVertex from the specified SharedVertex object.
        /// </summary>
        /// <param name="sharedVertex">The SharedVertex object to copy.</param>
        public SharedVertex(SharedVertex sharedVertex)
        {
            if (sharedVertex == null)
                throw new ArgumentNullException("sharedVertex");
            m_Vertices = new int[sharedVertex.Count];
            Array.Copy(sharedVertex.m_Vertices, m_Vertices, m_Vertices.Length);
        }

        /// <summary>
        /// Gets and sets the vertex by index.
        /// </summary>
        /// <param name="i">The index to access.</param>
        public int this[int i]
        {
            get { return m_Vertices[i]; }
            set { m_Vertices[i] = value; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through this collection.
        /// </summary>
        /// <returns>An IEnumerator object that you can use to iterate through the collection.</returns>
        public IEnumerator<int> GetEnumerator()
        {
            return ((IEnumerable<int>)m_Vertices).GetEnumerator();
        }

        /// <summary>
        /// Returns a string that represents this SharedVertex.
        /// </summary>
        /// <returns>A comma-delimited string (for example `"2,0,6,3"`).</returns>
        public override string ToString()
        {
            return m_Vertices.ToString(",");
        }

        /// <summary>
        /// Returns an enumerator that iterates through this collection.
        /// </summary>
        /// <returns>An IEnumerator object that you can use to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Adds a new entry using the specified index.
        /// </summary>
        /// <param name="item">The index to add.</param>
        public void Add(int item)
        {
            m_Vertices = ArrayUtility.Add(m_Vertices, item);
        }

        /// <summary>
        /// Resets this SharedVertex object to an empty collection.
        /// </summary>
        public void Clear()
        {
            m_Vertices = new int[0];
        }

        /// <summary>
        /// Returns whether the specified item exists in this collection.
        /// </summary>
        /// <param name="item">The index of the item to check.</param>
        /// <returns>True if the index was found; false otherwise</returns>
        public bool Contains(int item)
        {
            return Array.IndexOf(m_Vertices, item) > -1;
        }

        /// <summary>
        /// Copies the elements of this collection to an array, starting at the specified `arrayIndex`.
        /// </summary>
        /// <param name="array">The destination array.</param>
        /// <param name="arrayIndex">The index in the destination array where the collection items will be copied.</param>
        public void CopyTo(int[] array, int arrayIndex)
        {
            m_Vertices.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Removes the specified item from this collection.
        /// </summary>
        /// <param name="item">The index of the item to remove.</param>
        /// <returns>True if the index was found and removed; false otherwise</returns>
        public bool Remove(int item)
        {
            int ind = Array.IndexOf(m_Vertices, item);
            if (ind < 0)
                return false;
            m_Vertices = m_Vertices.RemoveAt(item);
            return true;
        }

        /// <summary>
        /// Gets the number of items in this collection.
        /// </summary>
        /// <value>The length of this collection</value>
        public int Count
        {
            get { return m_Vertices.Length; }
        }

        /// <summary>
        /// Gets whether this collection is read-only.
        /// </summary>
        /// <value>The value of the IsReadOnly flag.</value>
        public bool IsReadOnly
        {
            get { return m_Vertices.IsReadOnly; }
        }

        /// <summary>
        /// Creates a lookup Dictionary in order to quickly find the index of a `SharedVertex` in the
        /// <see cref="ProBuilderMesh.sharedVertices"/> array using a vertex index.
        /// </summary>
        /// <param name="sharedVertices">A collection of SharedVertex values.</param>
        /// <param name="lookup">
        /// A Dictionary where the 'key' represents an index in the Mesh positions array, and the
        /// 'value' is the index of its placement in the sharedVertices array.
        /// </param>
        public static void GetSharedVertexLookup(IList<SharedVertex> sharedVertices, Dictionary<int, int> lookup)
        {
            lookup.Clear();

            for(int i = 0, c = sharedVertices.Count; i < c; i++)
            {
                foreach (var index in sharedVertices[i])
                {
                    if (!lookup.ContainsKey(index))
                        lookup.Add(index, i);
                }
            }
        }

        internal void ShiftIndexes(int offset)
        {
            for (int i = 0, c = Count; i < c; i++)
                m_Vertices[i] += offset;
        }

        /// <summary>
        /// Converts a lookup Dictionary (<see cref="SharedVertex.GetSharedVertexLookup"/>) to a `SharedVertex` array.
        /// </summary>
        /// <param name="lookup">An existing Dictionary where the 'key' corresponds to a vertex index, and the 'value'
        /// corresponds to a common index.</param>
        /// <returns>A new IntArray[] converted from the lookup dictionary.</returns>
        internal static SharedVertex[] ToSharedVertices(IEnumerable<KeyValuePair<int, int>> lookup)
        {
            if (lookup == null)
                return new SharedVertex[0];

            Dictionary<int, int> map = new Dictionary<int, int>();
            List<List<int>> shared = new List<List<int>>();

            foreach (var kvp in lookup)
            {
                if (kvp.Value < 0)
                {
                    shared.Add(new List<int>() { kvp.Key });
                }
                else
                {
                    int index = -1;

                    if (map.TryGetValue(kvp.Value, out index))
                    {
                        shared[index].Add(kvp.Key);
                    }
                    else
                    {
                        map.Add(kvp.Value, shared.Count);
                        shared.Add(new List<int>() { kvp.Key });
                    }
                }
            }

            return ToSharedVertices(shared);
        }

        static SharedVertex[] ToSharedVertices(List<List<int>> list)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            SharedVertex[] arr = new SharedVertex[list.Count];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = new SharedVertex(list[i]);
            return arr;
        }

        /// <summary>
        /// Creates a new array of SharedVertex objects by comparing points in the specified positions collection.
        /// </summary>
        /// <example>
        /// ```lang-csharp
        /// <![CDATA[var mesh = gameObject.AdComponent<ProBuilderMesh>();
        /// mesh.SetPositions(myNewPositions);
        /// mesh.SetFaces(myNewFaces);
        /// mesh.SetSharedIndexes(SharedVertex.GetSharedVerticesWithPositions(myNewPositions));]]>
        /// ```
        /// </example>
        /// <param name="positions">A collection of Vector3 positions to test for equality.</param>
        /// <returns>A new SharedVertex array where each item is a list of indices that share the same position.</returns>
        public static SharedVertex[] GetSharedVerticesWithPositions(IList<Vector3> positions)
        {
            if (positions == null)
                throw new ArgumentNullException("positions");

            Dictionary<IntVec3, List<int>> sorted = new Dictionary<IntVec3, List<int>>();

            for (int i = 0; i < positions.Count; i++)
            {
                List<int> ind;
                if (sorted.TryGetValue(positions[i], out ind))
                    ind.Add(i);
                else
                    sorted.Add(new IntVec3(positions[i]), new List<int>() { i });
            }

            SharedVertex[] share = new SharedVertex[sorted.Count];

            int t = 0;
            foreach (KeyValuePair<IntVec3, List<int>> kvp in sorted)
                share[t++] = new SharedVertex(kvp.Value.ToArray());

            return share;
        }

        internal static SharedVertex[] RemoveAndShift(Dictionary<int, int> lookup, IEnumerable<int> remove)
        {
            var removedVertices = new List<int>(remove);
            removedVertices.Sort();
            return SortedRemoveAndShift(lookup, removedVertices);
        }

        internal static SharedVertex[] SortedRemoveAndShift(Dictionary<int, int> lookup, List<int> remove)
        {
            foreach (int i in remove)
                lookup[i] = -1;

            var shared = ToSharedVertices(lookup.Where(x => x.Value > -1));

            for (int i = 0, c = shared.Length; i < c; i++)
            {
                for (int n = 0, l = shared[i].Count; n < l; n++)
                {
                    int index = ArrayUtility.NearestIndexPriorToValue(remove, shared[i][n]);
                    // add 1 because index is zero based
                    shared[i][n] -= index + 1;
                }
            }

            return shared;
        }

        internal static void SetCoincident(ref Dictionary<int, int> lookup, IEnumerable<int> vertices)
        {
            int index = lookup.Count;
            foreach (var v in vertices)
                lookup[v] = index;
        }
    }
}

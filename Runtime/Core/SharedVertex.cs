using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;
using UnityEngine.Serialization;

namespace UnityEngine.ProBuilder
{
    /// <inheritdoc />
    /// <summary>
    /// Defines associations between vertex positions that are coincident. The indexes stored in this collection correspond to the ProBuilderMesh.positions array.
    /// <br />
    /// Coincident vertices are vertices that despite sharing the same coordinate position, are separate entries in the vertex array.
    /// </summary>
    [Serializable]
    public sealed class SharedVertex : ICollection<int>
    {
        /// <summary>
        /// An array of vertex indexes that are coincident.
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
        /// Create a new SharedVertex from an int array.
        /// </summary>
        /// <param name="indexes">The array to copy.</param>
        public SharedVertex(IEnumerable<int> indexes)
        {
            if (indexes == null)
                throw new ArgumentNullException("indexes");
            m_Vertices = indexes.ToArray();
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="sharedVertex">The array to copy.</param>
        public SharedVertex(SharedVertex sharedVertex)
        {
            if (sharedVertex == null)
                throw new ArgumentNullException("sharedVertex");
            m_Vertices = new int[sharedVertex.Count];
            Array.Copy(sharedVertex.m_Vertices, m_Vertices, m_Vertices.Length);
        }

        /// <summary>
        /// Index accessor.
        /// </summary>
        /// <param name="i">The index to access.</param>
        public int this[int i]
        {
            get { return m_Vertices[i]; }
            set { m_Vertices[i] = value; }
        }

        /// <inheritdoc />
        public IEnumerator<int> GetEnumerator()
        {
            return ((IEnumerable<int>)m_Vertices).GetEnumerator();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return m_Vertices.ToString(",");
        }

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <inheritdoc />
        public void Add(int item)
        {
            m_Vertices = ArrayUtility.Add(m_Vertices, item);
        }

        /// <inheritdoc />
        public void Clear()
        {
            m_Vertices = new int[0];
        }

        /// <inheritdoc />
        public bool Contains(int item)
        {
            return Array.IndexOf(m_Vertices, item) > -1;
        }

        /// <inheritdoc />
        public void CopyTo(int[] array, int arrayIndex)
        {
            m_Vertices.CopyTo(array, arrayIndex);
        }

        /// <inheritdoc />
        public bool Remove(int item)
        {
            int ind = Array.IndexOf(m_Vertices, item);
            if (ind < 0)
                return false;
            m_Vertices = m_Vertices.RemoveAt(item);
            return true;
        }

        /// <inheritdoc />
        public int Count
        {
            get { return m_Vertices.Length; }
        }

        /// <inheritdoc />
        public bool IsReadOnly
        {
            get { return m_Vertices.IsReadOnly; }
        }

        /// <summary>
        /// A <see cref="SharedVertex"/> is used to associate discrete vertices that share a common position. A lookup
        /// Dictionary provides a fast way to find the index of a <see cref="SharedVertex"/> in the
        /// <see cref="ProBuilderMesh.sharedVertices"/> array with a vertex index.
        /// </summary>
        /// <param name="sharedVertices">
        /// A collection of SharedVertex values.
        /// </param>
        /// <param name="lookup">
        /// A Dictionary where the Key represents an index in the Mesh positions array, and the
        /// Value is the index of it's placement in the sharedVertices array.
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
        /// Convert a lookup dictionary (<see cref="SharedVertex.GetSharedVertexLookup"/>) back to <see cref="SharedVertex"/>[].
        /// </summary>
        /// <param name="lookup">A Dictionary where Key corresponds to a vertex index, and Value to a common index.</param>
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
        /// Create a new array of SharedVertex objects by comparing points in the passed positions collection.
        /// </summary>
        /// <example>
        /// ```
        /// <![CDATA[var mesh = gameObject.AdComponent<ProBuilderMesh>();]]>
        /// mesh.SetPositions(myNewPositions);
        /// mesh.SetFaces(myNewFaces);
        /// mesh.SetSharedIndexes(SharedVertex.GetSharedVerticesWithPositions(myNewPositions));
        /// ```
        /// </example>
        /// <param name="positions">A collection of Vector3 positions to be tested for equality.</param>
        /// <returns>A new SharedVertex[] where each SharedIndex is a list of indexes that are sharing the same position.</returns>
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

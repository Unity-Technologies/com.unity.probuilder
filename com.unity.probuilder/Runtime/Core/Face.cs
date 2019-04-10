using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Serialization;
using System.Collections.ObjectModel;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// A face is composed of a set of triangles, and a material.
    /// <br />
    /// Triangle indexes may point to the same vertex index as long as the vertices are unique to the face. Ie, every vertex that a face references should only be used by that face's indices. To associate vertices that share common attributes (usually position), use the @"UnityEngine.ProBuilder.ProBuilderMesh.sharedIndexes" property.
    /// <br />
    /// ProBuilder automatically manages condensing common vertices in the EditorMeshUtility.Optimize function.
    /// </summary>
    [Serializable]
    public sealed class Face
    {
        [FormerlySerializedAs("_indices")]
        [SerializeField]
        int[] m_Indexes;

        /// <summary>
        /// Adjacent faces sharing this smoothingGroup will have their abutting edge normals averaged.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("_smoothingGroup")]
        int m_SmoothingGroup;

        /// <summary>
        /// If manualUV is false, these parameters determine how this face's vertices are projected to 2d space.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("_uv")]
        AutoUnwrapSettings m_Uv;

        /// <summary>
        /// What material does this face use.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("_mat")]
        Material m_Material;

        [SerializeField]
        int m_SubmeshIndex;


        [SerializeField]
        [FormerlySerializedAs("manualUV")]
        bool m_ManualUV;

        /// <value>
        /// If this face has had it's UV coordinates done by hand, don't update them with the auto unwrap crowd.
        /// </value>
        public bool manualUV
        {
            get { return m_ManualUV; }
            set { m_ManualUV = value; }
        }

        /// <value>
        /// UV element group. Used by the UV editor to group faces.
        /// </value>
        [SerializeField]
        internal int elementGroup;


        [SerializeField]
        int m_TextureGroup;

        /// <value>
        /// What texture group this face belongs to. Used when projecting auto UVs.
        /// </value>
        public int textureGroup
        {
            get { return m_TextureGroup;}
            set { m_TextureGroup = value; }
        }

        /// <value>
        /// Return a reference to the triangle indexes that make up this face.
        /// </value>
        internal int[] indexesInternal
        {
            get { return m_Indexes; }
            set
            {
                if (m_Indexes == null)
                    throw new ArgumentNullException("value");
                if (m_Indexes.Length % 3 != 0)
                    throw new ArgumentException("Face indexes must be a multiple of 3.");
                m_Indexes = value;
                InvalidateCache();
            }
        }

        /// <value>
        /// The triangle indexes that make up this face.
        /// </value>
        public ReadOnlyCollection<int> indexes
        {
            get { return new ReadOnlyCollection<int>(m_Indexes); }
        }

        /// <summary>
        /// Set the triangles that compose this face.
        /// </summary>
        /// <param name="array">The new triangle array.</param>
        public void SetIndexes(int[] array)
        {
            if (array == null)
                throw new ArgumentNullException("array");

            int len = array.Length;

            if (len % 3 != 0)
                throw new ArgumentException("Face indexes must be a multiple of 3.");

            m_Indexes = new int[len];
            Array.Copy(array, m_Indexes, len);
            InvalidateCache();
        }

        [NonSerialized]
        int[] m_DistinctIndexes;

        [NonSerialized]
        Edge[] m_Edges;

        /// <value>
        /// Returns a reference to the cached distinct indexes (each vertex index is only referenced once in m_DistinctIndexes).
        /// </value>
        internal int[] distinctIndexesInternal
        {
            get { return m_DistinctIndexes == null ? CacheDistinctIndexes() : m_DistinctIndexes; }
        }

        /// <value>
        /// A collection of the vertex indexes that the indexes array references, made distinct.
        /// </value>
        public ReadOnlyCollection<int> distinctIndexes
        {
            get { return new ReadOnlyCollection<int>(distinctIndexesInternal); }
        }

        internal Edge[] edgesInternal
        {
            get { return m_Edges == null ? CacheEdges() : m_Edges; }
        }

        /// <value>
        /// Get the perimeter edges that commpose this face.
        /// </value>
        public ReadOnlyCollection<Edge> edges
        {
            get { return new ReadOnlyCollection<Edge>(edgesInternal); }
        }

        /// <value>
        /// What smoothing group this face belongs to, if any. This is used to calculate vertex normals.
        /// </value>
        public int smoothingGroup
        {
            get { return m_SmoothingGroup; }
            set { m_SmoothingGroup = value; }
        }

        /// <value>
        /// Get the material that face uses.
        /// </value>
        [Obsolete("Face.material is deprecated. Please use submeshIndex instead.")]
        public Material material
        {
            get { return m_Material; }
            set { m_Material = value; }
        }

        public int submeshIndex
        {
            get { return m_SubmeshIndex; }
            set { m_SubmeshIndex = value; }
        }

        /// <value>
        /// A reference to the Auto UV mapping parameters.
        /// </value>
        public AutoUnwrapSettings uv
        {
            get { return m_Uv; }
            set { m_Uv = value; }
        }

        /// <summary>
        /// Accesses the indexes array.
        /// </summary>
        /// <param name="i"></param>
        public int this[int i]
        {
            get { return indexesInternal[i]; }
        }

        /// <summary>
        /// Default constructor creates a face with an empty triangles array.
        /// </summary>
        public Face()
        {
            m_SubmeshIndex = 0;
        }

        /// <summary>
        /// Initialize a Face with a set of triangles and default values.
        /// </summary>
        /// <param name="array">The new triangles array.</param>
        public Face(int[] array)
        {
            SetIndexes(array);
            m_Uv = AutoUnwrapSettings.tile;
            m_Material = BuiltinMaterials.defaultMaterial;
            m_SmoothingGroup = Smoothing.smoothingGroupNone;
            m_SubmeshIndex = 0;
            textureGroup = -1;
            elementGroup = 0;
        }

        [Obsolete("Face.material is deprecated. Please use \"submeshIndex\" instead.")]
        internal Face(int[] triangles, Material m, AutoUnwrapSettings u, int smoothing, int texture, int element, bool manualUVs)
        {
            SetIndexes(triangles);
            m_Uv = new AutoUnwrapSettings(u);
            m_Material = m;
            m_SmoothingGroup = smoothing;
            textureGroup = texture;
            elementGroup = element;
            manualUV = manualUVs;
            m_SubmeshIndex = 0;
        }

        internal Face(int[] triangles, int submeshIndex, AutoUnwrapSettings u, int smoothing, int texture, int element, bool manualUVs)
        {
            SetIndexes(triangles);
            m_Uv = new AutoUnwrapSettings(u);
            m_SmoothingGroup = smoothing;
            textureGroup = texture;
            elementGroup = element;
            manualUV = manualUVs;
            m_SubmeshIndex = submeshIndex;
        }

        /// <summary>
        /// Deep copy constructor.
        /// </summary>
        /// <param name="other">The Face from which to copy properties.</param>
        public Face(Face other)
        {
            CopyFrom(other);
        }

        /// <summary>
        /// Copies values from other to this face.
        /// </summary>
        /// <param name="other">The Face from which to copy properties.</param>
        public void CopyFrom(Face other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            int len = other.indexesInternal.Length;
            m_Indexes = new int[len];
            Array.Copy(other.indexesInternal, m_Indexes, len);

            m_SmoothingGroup = other.smoothingGroup;
            m_Uv = new AutoUnwrapSettings(other.uv);
#pragma warning disable 618
            m_Material = other.material;
#pragma warning restore 618
            manualUV = other.manualUV;
            m_TextureGroup = other.textureGroup;
            elementGroup = other.elementGroup;
            m_SubmeshIndex = other.m_SubmeshIndex;
            InvalidateCache();
        }

        internal void InvalidateCache()
        {
            m_Edges = null;
            m_DistinctIndexes = null;
        }

        Edge[] CacheEdges()
        {
            if (m_Indexes == null)
                return null;

            HashSet<Edge> dist = new HashSet<Edge>();
            List<Edge> dup = new List<Edge>();

            for (int i = 0; i < indexesInternal.Length; i += 3)
            {
                Edge a = new Edge(indexesInternal[i + 0], indexesInternal[i + 1]);
                Edge b = new Edge(indexesInternal[i + 1], indexesInternal[i + 2]);
                Edge c = new Edge(indexesInternal[i + 2], indexesInternal[i + 0]);

                if (!dist.Add(a)) dup.Add(a);
                if (!dist.Add(b)) dup.Add(b);
                if (!dist.Add(c)) dup.Add(c);
            }

            dist.ExceptWith(dup);
            m_Edges = dist.ToArray();
            return m_Edges;
        }

        int[] CacheDistinctIndexes()
        {
            if (m_Indexes == null)
                return null;
            m_DistinctIndexes = m_Indexes.Distinct().ToArray();
            return distinctIndexesInternal;
        }

        /// <summary>
        /// Test if a triangle is contained within the triangles array of this face.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public bool Contains(int a, int b, int c)
        {
            for (int i = 0, cnt = indexesInternal.Length; i < cnt; i += 3)
            {
                if (a == indexesInternal[i + 0]
                    && b == indexesInternal[i + 1]
                    && c == indexesInternal[i + 2])
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Is this face representable as quad?
        /// </summary>
        /// <returns></returns>
        public bool IsQuad()
        {
            return edgesInternal != null && edgesInternal.Length == 4;
        }

        /// <summary>
        /// Convert a 2 triangle face to a quad representation.
        /// </summary>
        /// <returns>A quad (4 indexes), or null if indexes are not able to be represented as a quad.</returns>
        public int[] ToQuad()
        {
            if (!IsQuad())
                throw new InvalidOperationException("Face is not representable as a quad. Use Face.IsQuad to check for validity.");

            int[] quad = new int[4] { edgesInternal[0].a, edgesInternal[0].b, -1, -1 };

            if (edgesInternal[1].a == quad[1])
                quad[2] = edgesInternal[1].b;
            else if (edgesInternal[2].a == quad[1])
                quad[2] = edgesInternal[2].b;
            else if (edgesInternal[3].a == quad[1])
                quad[2] = edgesInternal[3].b;

            if (edgesInternal[1].a == quad[2])
                quad[3] = edgesInternal[1].b;
            else if (edgesInternal[2].a == quad[2])
                quad[3] = edgesInternal[2].b;
            else if (edgesInternal[3].a == quad[2])
                quad[3] = edgesInternal[3].b;

            return quad;
        }

        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();

            for (int i = 0; i < indexesInternal.Length; i += 3)
            {
                sb.Append("[");
                sb.Append(indexesInternal[i]);
                sb.Append(", ");
                sb.Append(indexesInternal[i + 1]);
                sb.Append(", ");
                sb.Append(indexesInternal[i + 2]);
                sb.Append("]");

                if (i < indexesInternal.Length - 3)
                    sb.Append(", ");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Add offset to each value in the indexes array.
        /// </summary>
        /// <param name="offset">The value to add to each index.</param>
        public void ShiftIndexes(int offset)
        {
            for (int i = 0, c = m_Indexes.Length; i < c; i++)
                m_Indexes[i] += offset;

            InvalidateCache();
        }

        /// <summary>
        /// Find the smallest value in the triangles array.
        /// </summary>
        /// <returns>The smallest value in the indexes array.</returns>
        int SmallestIndexValue()
        {
            int smallest = m_Indexes[0];

            for (int i = 1; i < m_Indexes.Length; i++)
            {
                if (m_Indexes[i] < smallest)
                    smallest = m_Indexes[i];
            }

            return smallest;
        }

        /// <summary>
        /// Finds the smallest value in the indexes array, then offsets by subtracting that value from each index.
        /// </summary>
        /// <example>
        /// ```
        /// // sets the indexes array to `{0, 1, 2}`.
        /// new Face(3,4,5).ShiftIndexesToZero();
        /// ```
        /// </example>
        public void ShiftIndexesToZero()
        {
            int offset = SmallestIndexValue();

            for (int i = 0; i < m_Indexes.Length; i++)
                m_Indexes[i] -= offset;

            InvalidateCache();
        }

        /// <summary>
        /// Reverse the order of the triangle array. This has the effect of reversing the direction that this face renders.
        /// </summary>
        public void Reverse()
        {
            Array.Reverse(m_Indexes);
            InvalidateCache();
        }

        internal static void GetIndices(IEnumerable<Face> faces, List<int> indices)
        {
            indices.Clear();

            foreach (var face in faces)
            {
                for (int i = 0, c = face.indexesInternal.Length; i < c; ++i)
                    indices.Add(face.indexesInternal[i]);
            }
        }

        internal static void GetDistinctIndices(IEnumerable<Face> faces, List<int> indices)
        {
            indices.Clear();

            foreach (var face in faces)
            {
                for (int i = 0, c = face.distinctIndexesInternal.Length; i < c; ++i)
                    indices.Add(face.distinctIndexesInternal[i]);
            }
        }
    }
}

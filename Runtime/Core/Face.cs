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
    ///
    /// Triangle indices may point to the same vertex index as long as the vertices are unique to the face. That is,
    /// every vertex that a face references should only be used by that face's indices. To associate vertices that
    /// share common attributes (usually position), use the <see cref="ProBuilderMesh.sharedVertices">sharedIndexes</see> property.
    ///
    /// ProBuilder automatically manages condensing common vertices in the
    /// <see cref="UnityEditor.ProBuilder.EditorMeshUtility.Optimize">EditorMeshUtility.Optimize</see> function.
    /// </summary>
    [Serializable]
    public sealed class Face
    {
        [FormerlySerializedAs("_indices")]
        [SerializeField]
        int[] m_Indexes;

        /// <summary>
        /// Stores the ID of the smoothing group that this Face is part of. ProBuilder averages the edge
        /// normals for all faces that share a [Smoothing Group](../manual/workflow-edit-smoothing.md).
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("_smoothingGroup")]
        int m_SmoothingGroup;

        /// <summary>
        /// Determines how ProBuilder projects this face's vertices 2D space when <see cref="manualUV" /> is false.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("_uv")]
        AutoUnwrapSettings m_Uv;

        /// <summary>
        /// Stores the material for this face to use.
        /// </summary>
        [SerializeField]
        [FormerlySerializedAs("_mat")]
        Material m_Material;

        [SerializeField]
        int m_SubmeshIndex;


        [SerializeField]
        [FormerlySerializedAs("manualUV")]
        bool m_ManualUV;

        /// <summary>
        /// Gets or sets whether to map this face's UV coordinates manually or automatically. See
        /// [Mapping Textures with UVs](../manual/workflow-uvs.html) for an overview of the differences.
        /// </summary>
        /// <value>
        /// True to set UV coordinates manually; false to use <see cref="AutoUnwrapSettings" />.
        /// </value>
        public bool manualUV
        {
            get { return m_ManualUV; }
            set { m_ManualUV = value; }
        }

        /// <summary>
        /// UV element group. Used by the UV editor to group faces.
        /// </summary>
        [SerializeField]
        internal int elementGroup;


        [SerializeField]
        int m_TextureGroup;

        /// <summary>
        /// Gets or sets which texture group this face belongs to. ProBuilder uses texture groups when
        /// projecting Auto UVs. See [Continuous tiling](../manual/workflow-uvs.html#continuous-tiling).
        /// </summary>
        /// <value>
        /// ID of the texture group for this face.
        /// </value>
        public int textureGroup
        {
            get { return m_TextureGroup;}
            set { m_TextureGroup = value; }
        }

        /// <summary>
        /// Gets or sets a reference to the array of triangle indices that make up this face.
        /// </summary>
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

        /// <summary>
        /// Gets the triangle indices that compose this face.
        /// </summary>
        /// <value>
        /// The array of indices representing this face's triangles.
        /// </value>
        public ReadOnlyCollection<int> indexes
        {
            get { return new ReadOnlyCollection<int>(m_Indexes); }
        }

        /// <summary>
        /// Sets the triangles that compose this face.
        /// </summary>
        /// <param name="indices">The new triangle array.</param>
        public void SetIndexes(IEnumerable<int> indices)
        {
            if (indices == null)
                throw new ArgumentNullException("indices");
            var array = indices.ToArray();
            int len = array.Length;
            if (len % 3 != 0)
                throw new ArgumentException("Face indexes must be a multiple of 3.");
            m_Indexes = array;
            InvalidateCache();
        }

        [NonSerialized]
        int[] m_DistinctIndexes;

        [NonSerialized]
        Edge[] m_Edges;

        /// <summary>
        /// Returns a reference to the cached distinct indexes (each vertex index is only referenced once in m_DistinctIndexes).
        /// </summary>
        internal int[] distinctIndexesInternal
        {
            get { return m_DistinctIndexes == null ? CacheDistinctIndexes() : m_DistinctIndexes; }
        }

        /// <summary>
        /// Gets a collection of the vertex indexes that the indexes array references, made distinct.
        /// </summary>
        /// <value>
        /// A unique collection of vertices.
        /// </value>
        public ReadOnlyCollection<int> distinctIndexes
        {
            get { return new ReadOnlyCollection<int>(distinctIndexesInternal); }
        }

        internal Edge[] edgesInternal
        {
            get { return m_Edges == null ? CacheEdges() : m_Edges; }
        }

        /// <summary>
        /// Gets the perimeter edges that compose this face.
        /// </summary>
        /// <value>
        /// The collection of edges on this face.
        /// </value>
        public ReadOnlyCollection<Edge> edges
        {
            get { return new ReadOnlyCollection<Edge>(edgesInternal); }
        }

        /// <summary>
        /// Gets or sets which smoothing group this face belongs to, if any. This is used to calculate vertex normals.
        /// </summary>
        /// <value>
        /// The ID of this smoothing group as an integer.
        /// </value>
        public int smoothingGroup
        {
            get { return m_SmoothingGroup; }
            set { m_SmoothingGroup = value; }
        }

        /// <summary>
        /// Gets the material that this face uses.
        /// </summary>
        [Obsolete("Face.material is deprecated. Please use submeshIndex instead.")]
        public Material material
        {
            get { return m_Material; }
            set { m_Material = value; }
        }

        /// <summary>
        /// Gets or sets the index of the submesh that this face belongs to.
        /// </summary>
        /// <value>
        /// The ID of the submesh as an integer.
        /// </value>
        public int submeshIndex
        {
            get { return m_SubmeshIndex; }
            set { m_SubmeshIndex = value; }
        }

        /// <summary>
        /// Gets or sets a reference to the [Auto UV](../manual/workflow-uvs.html#auto-uv-mode-features) mapping parameters.
        /// </summary>
        /// <value>
        /// The ID of this submesh as an integer.
        /// </value>
        public AutoUnwrapSettings uv
        {
            get { return m_Uv; }
            set { m_Uv = value; }
        }

        /// <summary>
        /// Gets the index for the specified triangle in this face's array of triangle indices.
        /// </summary>
        /// <param name="i">The triangle to access</param>
        /// <value>
        /// The index of the specified triangle.
        /// </value>
        public int this[int i]
        {
            get { return indexesInternal[i]; }
        }

        /// <summary>
        /// Creates a Face with an empty triangles array.
        /// </summary>
        public Face()
        {
            m_SubmeshIndex = 0;
        }

        /// <summary>
        /// Creates a face with default values and the specified set of triangles.
        /// </summary>
        /// <param name="indices">The new triangles array.</param>
        public Face(IEnumerable<int> indices)
        {
            SetIndexes(indices);
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

        internal Face(IEnumerable<int> triangles, int submeshIndex, AutoUnwrapSettings u, int smoothing, int texture, int element, bool manualUVs)
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
        /// Creates a new Face as a copy of another face.
        /// </summary>
        /// <param name="other">The Face from which to copy properties and triangles.</param>
        public Face(Face other)
        {
            CopyFrom(other);
        }

        /// <summary>
        /// Copies properties and triangles from the specified face to this face.
        /// </summary>
        /// <param name="other">The Face from which to copy properties and triangles.</param>
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
        /// Tests whether a triangle matches one of the triangles of this face.
        /// </summary>
        /// <param name="a">First index in the triangle</param>
        /// <param name="b">Second index in the triangle</param>
        /// <param name="c">Third index in the triangle</param>
        /// <returns>True if {a,b,c} is found in this face's list of triangles; otherwise false.</returns>
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
        /// Returns whether this face can be converted to a quad (a polygon with four sides).
        /// </summary>
        /// <returns>True if this face is divisible by 4; false otherwise.</returns>
        public bool IsQuad()
        {
            return edgesInternal != null && edgesInternal.Length == 4;
        }

        /// <summary>
        /// Converts a two-triangle face to a quad representation.
        /// </summary>
        /// <returns>A quad (an array of four indices); or null if indices are not able to be represented as a quad.</returns>
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

        /// <summary>
        /// Returns a string representation of the face.
        /// </summary>
        /// <returns>String formatted as `[a, b, c], ...`.</returns>
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
        /// Adds an offset to each value in the indices array.
        /// </summary>
        /// <param name="offset">The value to add to each index.</param>
        public void ShiftIndexes(int offset)
        {
            for (int i = 0, c = m_Indexes.Length; i < c; i++)
                m_Indexes[i] += offset;

            InvalidateCache();
        }

        /// <summary>
        /// Finds the smallest value in the triangles array.
        /// </summary>
        /// <returns>The smallest value in the indices array.</returns>
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
        /// Finds the smallest value in the indices array, then offsets by subtracting that value from each index.
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
        /// Reverses the order of the triangle array. This has the effect of reversing the direction that this face renders.
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

        /// <summary>
        /// Advances to the next connected edge given a source edge and the index connect.
        /// </summary>
        internal bool TryGetNextEdge(Edge source, int index, ref Edge nextEdge, ref int nextIndex)
        {
            for (int i = 0, c = edgesInternal.Length; i < c; i++)
            {
                if (edgesInternal[i] == source)
                    continue;

                nextEdge = edgesInternal[i];

                if (nextEdge.Contains(index))
                {
                    nextIndex = nextEdge.a == index ? nextEdge.b : nextEdge.a;
                    return true;
                }
            }

            return false;
        }
    }
}

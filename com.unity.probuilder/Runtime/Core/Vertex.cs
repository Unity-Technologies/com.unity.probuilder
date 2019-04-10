using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections.ObjectModel;
using System.Text;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Holds information about a single vertex, and provides methods for averaging between many.
    /// <remarks>All values are optional. Where not present a default value will be substituted if necessary.</remarks>
    /// </summary>
    /// <inheritdoc cref="IEquatable{T}"/>
    [Serializable]
    public sealed class Vertex : IEquatable<Vertex>
    {
        [SerializeField]
        Vector3 m_Position;

        [SerializeField]
        Color m_Color;

        [SerializeField]
        Vector3 m_Normal;

        [SerializeField]
        Vector4 m_Tangent;

        [SerializeField]
        Vector2 m_UV0;

        [SerializeField]
        Vector2 m_UV2;

        [SerializeField]
        Vector4 m_UV3;

        [SerializeField]
        Vector4 m_UV4;

        [SerializeField]
        MeshArrays m_Attributes;

        /// <value>
        /// The position in model space.
        /// </value>
        /// <seealso cref="ProBuilderMesh.positions"/>
        public Vector3 position
        {
            get { return m_Position; }
            set
            {
                hasPosition = true;
                m_Position = value;
            }
        }

        /// <value>
        /// Vertex color.
        /// </value>
        /// <seealso cref="ProBuilderMesh.colors"/>
        public Color color
        {
            get { return m_Color; }
            set
            {
                hasColor = true;
                m_Color = value;
            }
        }

        /// <value>
        /// Unit vector normal.
        /// </value>
        /// <seealso cref="ProBuilderMesh.GetNormals"/>
        public Vector3 normal
        {
            get { return m_Normal; }
            set
            {
                hasNormal = true;
                m_Normal = value;
            }
        }

        /// <value>
        /// Vertex tangent (sometimes called binormal).
        /// </value>
        /// <seealso cref="ProBuilderMesh.tangents"/>
        public Vector4 tangent
        {
            get { return m_Tangent; }
            set
            {
                hasTangent = true;
                m_Tangent = value;
            }
        }

        /// <value>
        /// UV 0 channel. Also called textures.
        /// </value>
        /// <seealso cref="ProBuilderMesh.textures"/>
        /// <seealso cref="ProBuilderMesh.GetUVs"/>
        public Vector2 uv0
        {
            get { return m_UV0; }
            set
            {
                hasUV0 = true;
                m_UV0 = value;
            }
        }

        /// <value>
        /// UV 2 channel.
        /// </value>
        /// <seealso cref="ProBuilderMesh.GetUVs"/>
        public Vector2 uv2
        {
            get { return m_UV2; }
            set
            {
                hasUV2 = true;
                m_UV2 = value;
            }
        }

        /// <value>
        /// UV 3 channel.
        /// </value>
        /// <seealso cref="ProBuilderMesh.GetUVs"/>
        public Vector4 uv3
        {
            get { return m_UV3; }
            set
            {
                hasUV3 = true;
                m_UV3 = value;
            }
        }

        /// <value>
        /// UV 4 channel.
        /// </value>
        /// <seealso cref="ProBuilderMesh.GetUVs"/>
        public Vector4 uv4
        {
            get { return m_UV4; }
            set
            {
                hasUV4 = true;
                m_UV4 = value;
            }
        }

        internal MeshArrays attributes
        {
            get { return m_Attributes; }
        }

        /// <summary>
        /// Find if a vertex attribute has been set.
        /// </summary>
        /// <param name="attribute">The attribute or attributes to test for.</param>
        /// <returns>True if this vertex has the specified attributes set, false if they are default values.</returns>
        public bool HasArrays(MeshArrays attribute)
        {
            return (m_Attributes & attribute) == attribute;
        }

        bool hasPosition
        {
            get { return (m_Attributes & MeshArrays.Position) == MeshArrays.Position; }
            set { m_Attributes = value ? (m_Attributes | MeshArrays.Position) : (m_Attributes & ~(MeshArrays.Position)); }
        }

        bool hasColor
        {
            get { return (m_Attributes & MeshArrays.Color) == MeshArrays.Color; }
            set { m_Attributes = value ? (m_Attributes | MeshArrays.Color) : (m_Attributes & ~(MeshArrays.Color)); }
        }

        bool hasNormal
        {
            get { return (m_Attributes & MeshArrays.Normal) == MeshArrays.Normal; }
            set { m_Attributes = value ? (m_Attributes | MeshArrays.Normal) : (m_Attributes & ~(MeshArrays.Normal)); }
        }

        bool hasTangent
        {
            get { return (m_Attributes & MeshArrays.Tangent) == MeshArrays.Tangent; }
            set { m_Attributes = value ? (m_Attributes | MeshArrays.Tangent) : (m_Attributes & ~(MeshArrays.Tangent)); }
        }

        bool hasUV0
        {
            get { return (m_Attributes & MeshArrays.Texture0) == MeshArrays.Texture0; }
            set { m_Attributes = value ? (m_Attributes | MeshArrays.Texture0) : (m_Attributes & ~(MeshArrays.Texture0)); }
        }

        bool hasUV2
        {
            get { return (m_Attributes & MeshArrays.Texture1) == MeshArrays.Texture1; }
            set { m_Attributes = value ? (m_Attributes | MeshArrays.Texture1) : (m_Attributes & ~(MeshArrays.Texture1)); }
        }

        bool hasUV3
        {
            get { return (m_Attributes & MeshArrays.Texture2) == MeshArrays.Texture2; }
            set { m_Attributes = value ? (m_Attributes | MeshArrays.Texture2) : (m_Attributes & ~(MeshArrays.Texture2)); }
        }

        bool hasUV4
        {
            get { return (m_Attributes & MeshArrays.Texture3) == MeshArrays.Texture3; }
            set { m_Attributes = value ? (m_Attributes | MeshArrays.Texture3) : (m_Attributes & ~(MeshArrays.Texture3)); }
        }

        /// <summary>
        /// Initialize a Vertex with no values.
        /// </summary>
        public Vertex()
        {
        }

        /// <summary>
        /// Compare Vertex to an object.
        /// </summary>
        /// <param name="obj">The object to compare this vertex to.</param>
        /// <returns>True if obj is a Vertex, and equal to this.</returns>
        public override bool Equals(object obj)
        {
            return obj is Vertex && Equals((Vertex)obj);
        }

        /// <summary>
        /// Compare the equality of vertex values. Uses the @"UnityEngine.ProBuilder.Math" Approx functions to compare float values.
        /// </summary>
        /// <param name="other">The vertex to compare.</param>
        /// <returns>True if all values are the same (within float.Epsilon).</returns>
        public bool Equals(Vertex other)
        {
            if (ReferenceEquals(other, null))
                return false;

            return Math.Approx3(m_Position, other.m_Position) &&
                Math.ApproxC(m_Color, other.m_Color) &&
                Math.Approx3(m_Normal, other.m_Normal) &&
                Math.Approx4(m_Tangent, other.m_Tangent) &&
                Math.Approx2(m_UV0, other.m_UV0) &&
                Math.Approx2(m_UV2, other.m_UV2) &&
                Math.Approx4(m_UV3, other.m_UV3) &&
                Math.Approx4(m_UV4, other.m_UV4);
        }

        public bool Equals(Vertex other, MeshArrays mask)
        {
            if (ReferenceEquals(other, null))
                return false;

            return ((mask & MeshArrays.Position) != MeshArrays.Position || Math.Approx3(m_Position, other.m_Position)) &&
                ((mask & MeshArrays.Color) != MeshArrays.Color || Math.ApproxC(m_Color, other.m_Color)) &&
                ((mask & MeshArrays.Normal) != MeshArrays.Normal || Math.Approx3(m_Normal, other.m_Normal)) &&
                ((mask & MeshArrays.Tangent) != MeshArrays.Tangent || Math.Approx4(m_Tangent, other.m_Tangent)) &&
                ((mask & MeshArrays.Texture0) != MeshArrays.Texture0 || Math.Approx2(m_UV0, other.m_UV0)) &&
                ((mask & MeshArrays.Texture1) != MeshArrays.Texture1 || Math.Approx2(m_UV2, other.m_UV2)) &&
                ((mask & MeshArrays.Texture2) != MeshArrays.Texture2 || Math.Approx4(m_UV3, other.m_UV3)) &&
                ((mask & MeshArrays.Texture3) != MeshArrays.Texture3 || Math.Approx4(m_UV4, other.m_UV4));
        }

        /// <summary>
        /// Creates a new hashcode from position, uv0, and normal.
        /// </summary>
        /// <returns>A hashcode for this object.</returns>
        public override int GetHashCode()
        {
            // 783 is 27 * 29
            unchecked
            {
                int hash = 783 + VectorHash.GetHashCode(position);
                hash = hash * 29 + VectorHash.GetHashCode(uv0);
                hash = hash * 31 + VectorHash.GetHashCode(normal);
                return hash;
            }
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="vertex">The Vertex to copy field data from.</param>
        public Vertex(Vertex vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException("vertex");

            m_Position = vertex.m_Position;
            hasPosition = vertex.hasPosition;
            m_Color = vertex.m_Color;
            hasColor = vertex.hasColor;
            m_UV0 = vertex.m_UV0;
            hasUV0 = vertex.hasUV0;
            m_Normal = vertex.m_Normal;
            hasNormal = vertex.hasNormal;
            m_Tangent = vertex.m_Tangent;
            hasTangent = vertex.hasTangent;
            m_UV2 = vertex.m_UV2;
            hasUV2 = vertex.hasUV2;
            m_UV3 = vertex.m_UV3;
            hasUV3 = vertex.hasUV3;
            m_UV4 = vertex.m_UV4;
            hasUV4 = vertex.hasUV4;
        }

        /// <inheritdoc cref="Vertex.Equals(Vertex)"/>
        public static bool operator==(Vertex a, Vertex b)
        {
            if (ReferenceEquals(a, b))
                return true;

            if (object.ReferenceEquals(a, null) || object.ReferenceEquals(b, null))
                return false;

            return a.Equals(b);
        }

        /// <summary>
        /// Test for inequality.
        /// </summary>
        /// <param name="a">Left parameter.</param>
        /// <param name="b">Right parameter.</param>
        /// <returns>True if a does not equal b.</returns>
        public static bool operator!=(Vertex a, Vertex b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Addition is performed component-wise for every property.
        /// </summary>
        /// <remarks>
        /// Color, normal, and tangent values are not normalized within this function. If you are expecting unit vectors, make sure to normalize these properties.
        /// </remarks>
        /// <param name="a">Left side addition parameter.</param>
        /// <param name="b">Right side addition parameter.</param>
        /// <returns>A new Vertex with the sum of a + b.</returns>
        public static Vertex operator+(Vertex a, Vertex b)
        {
            return Add(a, b);
        }

        /// <summary>
        /// Addition is performed component-wise for every property.
        /// </summary>
        /// <remarks>
        /// Color, normal, and tangent values are not normalized within this function. If you are expecting unit vectors, make sure to normalize these properties.
        /// </remarks>
        /// <param name="a">Left side addition parameter.</param>
        /// <param name="b">Right side addition parameter.</param>
        /// <returns>A new Vertex with the sum of a + b.</returns>
        public static Vertex Add(Vertex a, Vertex b)
        {
            Vertex v = new Vertex(a);
            v.Add(b);
            return v;
        }

        /// <summary>
        /// Addition is performed component-wise for every property.
        /// </summary>
        /// <remarks>
        /// Color, normal, and tangent values are not normalized within this function. If you are expecting unit vectors, make sure to normalize these properties.
        /// </remarks>
        /// <param name="b">Right side addition parameter.</param>
        public void Add(Vertex b)
        {
            if (b == null)
                throw new ArgumentNullException("b");

            m_Position += b.m_Position;
            m_Color += b.m_Color;
            m_Normal += b.m_Normal;
            m_Tangent += b.m_Tangent;
            m_UV0 += b.m_UV0;
            m_UV2 += b.m_UV2;
            m_UV3 += b.m_UV3;
            m_UV4 += b.m_UV4;
        }

        /// <summary>
        /// Subtraction is performed component-wise for every property.
        /// </summary>
        /// <remarks>
        /// Color, normal, and tangent values are not normalized within this function. If you are expecting unit vectors, make sure to normalize these properties.
        /// </remarks>
        /// <param name="a">Left side subtraction parameter.</param>
        /// <param name="b">Right side subtraction parameter.</param>
        /// <returns>A new Vertex with the sum of a - b.</returns>
        public static Vertex operator-(Vertex a, Vertex b)
        {
            return Subtract(a, b);
        }

        /// <summary>
        /// Subtraction is performed component-wise for every property.
        /// </summary>
        /// <remarks>
        /// Color, normal, and tangent values are not normalized within this function. If you are expecting unit vectors, make sure to normalize these properties.
        /// </remarks>
        /// <param name="a">Left side subtraction parameter.</param>
        /// <param name="b">Right side subtraction parameter.</param>
        /// <returns>A new Vertex with the sum of a - b.</returns>
        public static Vertex Subtract(Vertex a, Vertex b)
        {
            var c = new Vertex(a);
            c.Subtract(b);
            return c;
        }

        /// <summary>
        /// Subtraction is performed component-wise for every property.
        /// </summary>
        /// <remarks>
        /// Color, normal, and tangent values are not normalized within this function. If you are expecting unit vectors, make sure to normalize these properties.
        /// </remarks>
        /// <param name="b">Right side subtraction parameter.</param>
        public void Subtract(Vertex b)
        {
            if (b == null)
                throw new ArgumentNullException("b");

            m_Position -= b.m_Position;
            m_Color -= b.m_Color;
            m_Normal -= b.m_Normal;
            m_Tangent -= b.m_Tangent;
            m_UV0 -= b.m_UV0;
            m_UV2 -= b.m_UV2;
            m_UV3 -= b.m_UV3;
            m_UV4 -= b.m_UV4;
        }

        /// <summary>
        /// Multiplication is performed component-wise for every property.
        /// </summary>
        /// <remarks>
        /// Color, normal, and tangent values are not normalized within this function. If you are expecting unit vectors, make sure to normalize these properties.
        /// </remarks>
        /// <param name="a">Left side multiplication parameter.</param>
        /// <param name="value">Right side multiplication parameter.</param>
        /// <returns>A new Vertex with the sum of a * b.</returns>
        public static Vertex operator*(Vertex a, float value)
        {
            return Multiply(a, value);
        }

        /// <summary>
        /// Multiplication is performed component-wise for every property.
        /// </summary>
        /// <param name="a">Left side multiplication parameter.</param>
        /// <param name="value">Right side multiplication parameter.</param>
        /// <returns>A new Vertex with the sum of a * b.</returns>
        public static Vertex Multiply(Vertex a, float value)
        {
            Vertex v = new Vertex(a);
            v.Multiply(value);
            return v;
        }

        /// <summary>
        /// Multiplication is performed component-wise for every property.
        /// </summary>
        /// <remarks>
        /// Color, normal, and tangent values are not normalized within this function. If you are expecting unit vectors, make sure to normalize these properties.
        /// </remarks>
        /// <param name="value">Right side multiplication parameter.</param>
        public void Multiply(float value)
        {
            m_Position *= value;
            m_Color *= value;
            m_Normal *= value;
            m_Tangent *= value;
            m_UV0 *= value;
            m_UV2 *= value;
            m_UV3 *= value;
            m_UV4 *= value;
        }

        /// <summary>
        /// Division is performed component-wise for every property.
        /// </summary>
        /// <remarks>
        /// Color, normal, and tangent values are not normalized within this function. If you are expecting unit vectors, make sure to normalize these properties.
        /// </remarks>
        /// <param name="a">Left side division parameter.</param>
        /// <param name="value">Right side division parameter.</param>
        /// <returns>A new Vertex with the sum of a / b.</returns>
        public static Vertex operator/(Vertex a, float value)
        {
            return Divide(a, value);
        }

        /// <summary>
        /// Division is performed component-wise for every property.
        /// </summary>
        /// <remarks>
        /// Color, normal, and tangent values are not normalized within this function. If you are expecting unit vectors, make sure to normalize these properties.
        /// </remarks>
        /// <param name="a">Left side division parameter.</param>
        /// <param name="value">Right side division parameter.</param>
        /// <returns>A new Vertex with the sum of a / b.</returns>
        public static Vertex Divide(Vertex a, float value)
        {
            Vertex v = new Vertex(a);
            v.Divide(value);
            return v;
        }

        /// <summary>
        /// Division is performed component-wise for every property.
        /// </summary>
        /// <remarks>
        /// Color, normal, and tangent values are not normalized within this function. If you are expecting unit vectors, make sure to normalize these properties.
        /// </remarks>
        /// <param name="value">Right side Division parameter.</param>
        public void Divide(float value)
        {
            m_Position /= value;
            m_Color /= value;
            m_Normal /= value;
            m_Tangent /= value;
            m_UV0 /= value;
            m_UV2 /= value;
            m_UV3 /= value;
            m_UV4 /= value;
        }

        /// <summary>
        /// Normalize all vector values in place.
        /// </summary>
        public void Normalize()
        {
            m_Position.Normalize();
            Vector4 color4 = m_Color;
            color4.Normalize();
            m_Color = color4;
            m_Normal.Normalize();
            m_Tangent.Normalize();
            m_UV0.Normalize();
            m_UV2.Normalize();
            m_UV3.Normalize();
            m_UV4.Normalize();
        }

        /// <summary>
        /// ToString override that prints every available property.
        /// </summary>
        /// <param name="args">An optional string argument that is provided to the component ToString calls.</param>
        /// <returns>A string with the values of all set properties.</returns>
        public string ToString(string args = null)
        {
            StringBuilder sb = new StringBuilder();
            if (hasPosition) sb.AppendLine("position: " + m_Position.ToString(args));
            if (hasColor) sb.AppendLine("color: " + m_Color.ToString(args));
            if (hasNormal) sb.AppendLine("normal: " + m_Normal.ToString(args));
            if (hasTangent) sb.AppendLine("tangent: " + m_Tangent.ToString(args));
            if (hasUV0) sb.AppendLine("uv0: " + m_UV0.ToString(args));
            if (hasUV2) sb.AppendLine("uv2: " + m_UV2.ToString(args));
            if (hasUV3) sb.AppendLine("uv3: " + m_UV3.ToString(args));
            if (hasUV4) sb.AppendLine("uv4: " + m_UV4.ToString(args));
            return sb.ToString();
        }

        /// <summary>
        /// Allocate and fill all attribute arrays. This method will fill all arrays, regardless of whether or not real data populates the values (check what attributes a Vertex contains with HasAttribute()).
        /// </summary>
        /// <remarks>
        /// If you are using this function to rebuild a mesh, use SetMesh instead. SetMesh handles setting null arrays where appropriate for you.
        /// </remarks>
        /// <seealso cref="SetMesh"/>
        /// <param name="vertices">The source vertices.</param>
        /// <param name="position">A new array of the vertex position values.</param>
        /// <param name="color">A new array of the vertex color values.</param>
        /// <param name="uv0">A new array of the vertex uv0 values.</param>
        /// <param name="normal">A new array of the vertex normal values.</param>
        /// <param name="tangent">A new array of the vertex tangent values.</param>
        /// <param name="uv2">A new array of the vertex uv2 values.</param>
        /// <param name="uv3">A new array of the vertex uv3 values.</param>
        /// <param name="uv4">A new array of the vertex uv4 values.</param>
        public static void GetArrays(
            IList<Vertex> vertices,
            out Vector3[] position,
            out Color[] color,
            out Vector2[] uv0,
            out Vector3[] normal,
            out Vector4[] tangent,
            out Vector2[] uv2,
            out List<Vector4> uv3,
            out List<Vector4> uv4)
        {
            GetArrays(vertices, out position, out color, out uv0, out normal, out tangent, out uv2, out uv3, out uv4, MeshArrays.All);
        }

        /// <summary>
        /// Allocate and fill the requested attribute arrays.
        /// </summary>
        /// <remarks>
        /// If you are using this function to rebuild a mesh, use SetMesh instead. SetMesh handles setting null arrays where appropriate for you.
        /// </remarks>
        /// <seealso cref="SetMesh"/>
        /// <param name="vertices">The source vertices.</param>
        /// <param name="position">A new array of the vertex position values if requested by the attributes parameter, or null.</param>
        /// <param name="color">A new array of the vertex color values if requested by the attributes parameter, or null.</param>
        /// <param name="uv0">A new array of the vertex uv0 values if requested by the attributes parameter, or null.</param>
        /// <param name="normal">A new array of the vertex normal values if requested by the attributes parameter, or null.</param>
        /// <param name="tangent">A new array of the vertex tangent values if requested by the attributes parameter, or null.</param>
        /// <param name="uv2">A new array of the vertex uv2 values if requested by the attributes parameter, or null.</param>
        /// <param name="uv3">A new array of the vertex uv3 values if requested by the attributes parameter, or null.</param>
        /// <param name="uv4">A new array of the vertex uv4 values if requested by the attributes parameter, or null.</param>
        /// <param name="attributes">A flag with the MeshAttributes requested.</param>
        /// <seealso cref="HasArrays"/>
        public static void GetArrays(
            IList<Vertex> vertices,
            out Vector3[] position,
            out Color[] color,
            out Vector2[] uv0,
            out Vector3[] normal,
            out Vector4[] tangent,
            out Vector2[] uv2,
            out List<Vector4> uv3,
            out List<Vector4> uv4,
            MeshArrays attributes)
        {
            if (vertices == null)
                throw new ArgumentNullException("vertices");

            int vc = vertices.Count;
            var first = vc < 1 ? new Vertex() : vertices[0];

            bool hasPosition = ((attributes & MeshArrays.Position) == MeshArrays.Position) && first.hasPosition;
            bool hasColor = ((attributes & MeshArrays.Color) == MeshArrays.Color) && first.hasColor;
            bool hasUv0 = ((attributes & MeshArrays.Texture0) == MeshArrays.Texture0) && first.hasUV0;
            bool hasNormal = ((attributes & MeshArrays.Normal) == MeshArrays.Normal) && first.hasNormal;
            bool hasTangent = ((attributes & MeshArrays.Tangent) == MeshArrays.Tangent) && first.hasTangent;
            bool hasUv2 = ((attributes & MeshArrays.Texture1) == MeshArrays.Texture1) && first.hasUV2;
            bool hasUv3 = ((attributes & MeshArrays.Texture2) == MeshArrays.Texture2) && first.hasUV3;
            bool hasUv4 = ((attributes & MeshArrays.Texture3) == MeshArrays.Texture3) && first.hasUV4;

            position = hasPosition ? new Vector3[vc] : null;
            color = hasColor ? new Color[vc] : null;
            uv0 = hasUv0 ? new Vector2[vc] : null;
            normal = hasNormal ? new Vector3[vc] : null;
            tangent = hasTangent ? new Vector4[vc] : null;
            uv2 = hasUv2 ? new Vector2[vc] : null;
            uv3 = hasUv3 ? new List<Vector4>(vc) : null;
            uv4 = hasUv4 ? new List<Vector4>(vc) : null;

            for (int i = 0; i < vc; i++)
            {
                if (hasPosition) position[i] = vertices[i].m_Position;
                if (hasColor) color[i] = vertices[i].m_Color;
                if (hasUv0) uv0[i] = vertices[i].m_UV0;
                if (hasNormal) normal[i] = vertices[i].m_Normal;
                if (hasTangent) tangent[i] = vertices[i].m_Tangent;
                if (hasUv2) uv2[i] = vertices[i].m_UV2;
                if (hasUv3) uv3.Add(vertices[i].m_UV3);
                if (hasUv4) uv4.Add(vertices[i].m_UV4);
            }
        }

        /// <summary>
        /// Replace mesh values with vertex array. Mesh is cleared during this function, so be sure to set the triangles after calling.
        /// </summary>
        /// <param name="mesh">The target mesh.</param>
        /// <param name="vertices">The vertices to replace the mesh attributes with.</param>
        public static void SetMesh(Mesh mesh, IList<Vertex> vertices)
        {
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (vertices == null)
                throw new ArgumentNullException("vertices");

            Vector3[] positions = null;
            Color[] colors = null;
            Vector2[] uv0s = null;
            Vector3[] normals = null;
            Vector4[] tangents = null;
            Vector2[] uv2s = null;
            List<Vector4> uv3s = null;
            List<Vector4> uv4s = null;

            GetArrays(vertices, out positions,
                out colors,
                out uv0s,
                out normals,
                out tangents,
                out uv2s,
                out uv3s,
                out uv4s);

            mesh.Clear();

            Vertex first = vertices[0];

            if (first.hasPosition) mesh.vertices = positions;
            if (first.hasColor) mesh.colors = colors;
            if (first.hasUV0) mesh.uv = uv0s;
            if (first.hasNormal) mesh.normals = normals;
            if (first.hasTangent) mesh.tangents = tangents;
            if (first.hasUV2) mesh.uv2 = uv2s;
#if !UNITY_4_7 && !UNITY_5_0
            if (first.hasUV3)
                if (uv3s != null)
                    mesh.SetUVs(2, uv3s);
            if (first.hasUV4)
                if (uv4s != null)
                    mesh.SetUVs(3, uv4s);
#endif
        }

        /// <summary>
        /// Average all vertices to a single vertex.
        /// </summary>
        /// <param name="vertices">A list of vertices.</param>
        /// <param name="indexes">If indexes is null, all vertices will be averaged. If indexes is provided, only the vertices referenced by the indexes array are averaged.</param>
        /// <returns>An averaged vertex value.</returns>
        public static Vertex Average(IList<Vertex> vertices, IList<int> indexes = null)
        {
            if (vertices == null)
                throw new ArgumentNullException("vertices");

            Vertex v = new Vertex();

            int vertexCount = indexes != null ? indexes.Count : vertices.Count;

            int positionCount = 0,
                colorCount = 0,
                uv0Count = 0,
                normalCount = 0,
                tangentCount = 0,
                uv2Count = 0,
                uv3Count = 0,
                uv4Count = 0;

            for (int i = 0; i < vertexCount; i++)
            {
                int index = indexes == null ? i : indexes[i];


                if (vertices[index].hasPosition)
                {
                    positionCount++;
                    v.m_Position += vertices[index].m_Position;
                }

                if (vertices[index].hasColor)
                {
                    colorCount++;
                    v.m_Color += vertices[index].m_Color;
                }

                if (vertices[index].hasUV0)
                {
                    uv0Count++;
                    v.m_UV0 += vertices[index].m_UV0;
                }

                if (vertices[index].hasNormal)
                {
                    normalCount++;
                    v.m_Normal += vertices[index].m_Normal;
                }

                if (vertices[index].hasTangent)
                {
                    tangentCount++;
                    v.m_Tangent += vertices[index].m_Tangent;
                }

                if (vertices[index].hasUV2)
                {
                    uv2Count++;
                    v.m_UV2 += vertices[index].m_UV2;
                }

                if (vertices[index].hasUV3)
                {
                    uv3Count++;
                    v.m_UV3 += vertices[index].m_UV3;
                }

                if (vertices[index].hasUV4)
                {
                    uv4Count++;
                    v.m_UV4 += vertices[index].m_UV4;
                }
            }

            if (positionCount > 0)
            {
                v.hasPosition = true;
                v.m_Position *= (1f / positionCount);
            }

            if (colorCount > 0)
            {
                v.hasColor = true;
                v.m_Color *= (1f / colorCount);
            }

            if (uv0Count > 0)
            {
                v.hasUV0 = true;
                v.m_UV0 *= (1f / uv0Count);
            }


            if (normalCount > 0)
            {
                v.hasNormal = true;
                v.m_Normal *= (1f / normalCount);
            }

            if (tangentCount > 0)
            {
                v.hasTangent = true;
                v.m_Tangent *= (1f / tangentCount);
            }

            if (uv2Count > 0)
            {
                v.hasUV2 = true;
                v.m_UV2 *= (1f / uv2Count);
            }

            if (uv3Count > 0)
            {
                v.hasUV3 = true;
                v.m_UV3 *= (1f / uv3Count);
            }

            if (uv4Count > 0)
            {
                v.hasUV4 = true;
                v.m_UV4 *= (1f / uv4Count);
            }

            return v;
        }

        /// <summary>
        /// Linearly interpolate between two vertices.
        /// </summary>
        /// <param name="x">Left parameter.</param>
        /// <param name="y">Right parameter.</param>
        /// <param name="weight">The weight of the interpolation. 0 is fully x, 1 is fully y.</param>
        /// <returns>A new vertex interpolated by weight between x and y.</returns>
        public static Vertex Mix(Vertex x, Vertex y, float weight)
        {
            if (x == null || y == null)
                throw new ArgumentNullException("x", "Mix does accept null vertices.");

            float i = 1f - weight;

            Vertex v = new Vertex();

            v.m_Position = x.m_Position * i + y.m_Position * weight;

            if (x.hasColor && y.hasColor)
                v.m_Color = x.m_Color * i + y.m_Color * weight;
            else if (x.hasColor)
                v.m_Color = x.m_Color;
            else if (y.hasColor)
                v.m_Color = y.m_Color;

            if (x.hasNormal && y.hasNormal)
                v.m_Normal = x.m_Normal * i + y.m_Normal * weight;
            else if (x.hasNormal)
                v.m_Normal = x.m_Normal;
            else if (y.hasNormal)
                v.m_Normal = y.m_Normal;

            if (x.hasTangent && y.hasTangent)
                v.m_Tangent = x.m_Tangent * i + y.m_Tangent * weight;
            else if (x.hasTangent)
                v.m_Tangent = x.m_Tangent;
            else if (y.hasTangent)
                v.m_Tangent = y.m_Tangent;

            if (x.hasUV0 && y.hasUV0)
                v.m_UV0 = x.m_UV0 * i + y.m_UV0 * weight;
            else if (x.hasUV0)
                v.m_UV0 = x.m_UV0;
            else if (y.hasUV0)
                v.m_UV0 = y.m_UV0;

            if (x.hasUV2 && y.hasUV2)
                v.m_UV2 = x.m_UV2 * i + y.m_UV2 * weight;
            else if (x.hasUV2)
                v.m_UV2 = x.m_UV2;
            else if (y.hasUV2)
                v.m_UV2 = y.m_UV2;

            if (x.hasUV3 && y.hasUV3)
                v.m_UV3 = x.m_UV3 * i + y.m_UV3 * weight;
            else if (x.hasUV3)
                v.m_UV3 = x.m_UV3;
            else if (y.hasUV3)
                v.m_UV3 = y.m_UV3;

            if (x.hasUV4 && y.hasUV4)
                v.m_UV4 = x.m_UV4 * i + y.m_UV4 * weight;
            else if (x.hasUV4)
                v.m_UV4 = x.m_UV4;
            else if (y.hasUV4)
                v.m_UV4 = y.m_UV4;

            return v;
        }
    }
}

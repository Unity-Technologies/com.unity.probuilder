using System;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Holds information about a single vertex, and provides methods for averaging between many.
    /// <remarks>All values are optional. Where not present a default value will be substituted if necessary.</remarks>
    /// </summary>
    struct CSG_Vertex
    {
        Vector3 m_Position;
        Color m_Color;
        Vector3 m_Normal;
        Vector4 m_Tangent;
        Vector2 m_UV0;
        Vector2 m_UV2;
        Vector4 m_UV3;
        Vector4 m_UV4;
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

        /// <summary>
        /// Find if a vertex attribute has been set.
        /// </summary>
        /// <param name="attribute">The attribute or attributes to test for.</param>
        /// <returns>True if this vertex has the specified attributes set, false if they are default values.</returns>
        public bool HasArrays(MeshArrays attribute)
        {
            return (m_Attributes & attribute) == attribute;
        }

        public bool hasPosition
        {
            get { return (m_Attributes & MeshArrays.Position) == MeshArrays.Position; }
            private set { m_Attributes = value ? (m_Attributes | MeshArrays.Position) : (m_Attributes & ~(MeshArrays.Position)); }
        }

        public bool hasColor
        {
            get { return (m_Attributes & MeshArrays.Color) == MeshArrays.Color; }
            private set { m_Attributes = value ? (m_Attributes | MeshArrays.Color) : (m_Attributes & ~(MeshArrays.Color)); }
        }

        public bool hasNormal
        {
            get { return (m_Attributes & MeshArrays.Normal) == MeshArrays.Normal; }
            private set { m_Attributes = value ? (m_Attributes | MeshArrays.Normal) : (m_Attributes & ~(MeshArrays.Normal)); }
        }

        public bool hasTangent
        {
            get { return (m_Attributes & MeshArrays.Tangent) == MeshArrays.Tangent; }
            private set { m_Attributes = value ? (m_Attributes | MeshArrays.Tangent) : (m_Attributes & ~(MeshArrays.Tangent)); }
        }

        public bool hasUV0
        {
            get { return (m_Attributes & MeshArrays.Texture0) == MeshArrays.Texture0; }
            private set { m_Attributes = value ? (m_Attributes | MeshArrays.Texture0) : (m_Attributes & ~(MeshArrays.Texture0)); }
        }

        public bool hasUV2
        {
            get { return (m_Attributes & MeshArrays.Texture1) == MeshArrays.Texture1; }
            private set { m_Attributes = value ? (m_Attributes | MeshArrays.Texture1) : (m_Attributes & ~(MeshArrays.Texture1)); }
        }

        public bool hasUV3
        {
            get { return (m_Attributes & MeshArrays.Texture2) == MeshArrays.Texture2; }
            private set { m_Attributes = value ? (m_Attributes | MeshArrays.Texture2) : (m_Attributes & ~(MeshArrays.Texture2)); }
        }

        public bool hasUV4
        {
            get { return (m_Attributes & MeshArrays.Texture3) == MeshArrays.Texture3; }
            private set { m_Attributes = value ? (m_Attributes | MeshArrays.Texture3) : (m_Attributes & ~(MeshArrays.Texture3)); }
        }

        public static explicit operator CSG_Vertex(Vertex vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException("vertex");

            var c = new CSG_Vertex();

            c.m_Attributes = vertex.attributes;
            c.m_Position = vertex.position;
            c.m_Color = vertex.color;
            c.m_UV0 = vertex.uv0;
            c.m_Normal = vertex.normal;
            c.m_Tangent = vertex.tangent;
            c.m_UV2 = vertex.uv2;
            c.m_UV3 = vertex.uv3;
            c.m_UV4 = vertex.uv4;

            return c;
        }

        public void Flip()
        {
            if(hasNormal)
                m_Normal *= -1f;

            if (hasTangent)
                m_Tangent *= -1f;
        }
    }
}

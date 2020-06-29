using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;


namespace UnityEngine.ProBuilder
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ProBuilderMesh))]
    [ExcludeFromPreset, ExcludeFromObjectFactory]
    public sealed class PolygonalCut : MonoBehaviour
    {
        /// <summary>
        /// Describes the different input states this tool operates in.
        /// </summary>
        internal enum PolygonEditMode
        {
            None,
            Add
        }

        /// <summary>
        /// Describes the different vertex types on the path.
        /// </summary>
        [System.Flags]
        public enum VertexType
        {
            None = 0 << 0,
            NewVertex = 1 << 0,
            AddedOnEdge = 1 << 1,
            ExistingVertex = 1 << 2,
            VertexInShape = 1 << 3
        }

        [Serializable]
        public class InsertedVertexData
        {
            public Vertex m_Vertex;
            public VertexType m_Type;

            public Vector3 Position
            {
                set {m_Vertex.position = value;}
                get { return m_Vertex.position; }
            }

            public InsertedVertexData(Vertex vertex, VertexType type = VertexType.None)
            {
                m_Vertex = vertex;
                m_Type = type;
            }
        }

        ProBuilderMesh m_Mesh;

        [SerializeField]
        internal List<InsertedVertexData> m_verticesToAdd = new List<InsertedVertexData>();

        public bool IsALoop
        {
            get
            {
                if (m_verticesToAdd.Count < 3)
                    return false;
                else
                    return m_verticesToAdd[0].m_Vertex.Equals(m_verticesToAdd[m_verticesToAdd.Count - 1].m_Vertex);
            }
        }

        [SerializeField]
        PolygonEditMode m_EditMode;

        internal PolygonEditMode polygonEditMode
        {
            get { return m_EditMode; }
            set { m_EditMode = value; }
        }

        internal ProBuilderMesh mesh
        {
            get
            {
                if (m_Mesh == null)
                    m_Mesh = GetComponent<ProBuilderMesh>();

                return m_Mesh;
            }

            set
            {
                m_Mesh = value;
            }
        }

    }
}

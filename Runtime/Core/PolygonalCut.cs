using System;
using System.Collections.Generic;
using System.Linq;


namespace UnityEngine.ProBuilder
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ProBuilderMesh))]
    [ExcludeFromPreset, ExcludeFromObjectFactory]
    public sealed class PolygonalCut : MonoBehaviour
    {
        /// <summary>
        /// Describes the different vertex types on the path.
        /// </summary>
        [System.Flags]
        public enum VertexTypes
        {
            None = 0 << 0,
            NewVertex = 1 << 0,
            AddedOnEdge = 1 << 1,
            ExistingVertex = 1 << 2,
            VertexInShape = 1 << 3,
        }

        [Serializable]
        public struct InsertedVertexData
        {
            [SerializeField]
            Vector3 m_Position;
            [SerializeField]
            Vector3 m_Normal;
            [SerializeField]
            VertexTypes m_Types;

            public Vector3 position
            {
                get => m_Position;
                set => m_Position = value;
            }

            public Vector3 normal
            {
                get => m_Normal;
                set => m_Normal = value;
            }

            public VertexTypes types
            {
                get => m_Types;
                set => m_Types = value;
            }

            public InsertedVertexData(Vector3 position, VertexTypes types = VertexTypes.None)
            {
                m_Position = position;
                m_Normal = Vector3.up;
                m_Types = types;
            }

            public InsertedVertexData(Vector3 position, Vector3 normal, VertexTypes types = VertexTypes.None)
            {
                m_Position = position;
                m_Normal = normal;
                m_Types = types;
            }
        }

        ProBuilderMesh m_Mesh;

        [SerializeField]
        internal List<InsertedVertexData> m_cutPath = new List<InsertedVertexData>();

        bool m_DoCut = false;
        public bool doCut
        {
            get
            {
                if (m_DoCut)
                {
                    m_DoCut = false;
                    return true;
                }

                return false;
            }
            set => m_DoCut = value;
        }

        public bool IsALoop
        {
            get
            {
                if (m_cutPath.Count < 3)
                    return false;
                else
                    return Math.Approx3(m_cutPath[0].position,
                        m_cutPath[m_cutPath.Count - 1].position);
            }
        }

        public int ConnectionsToFaceBordersCount
        {
            get
            {
                return m_cutPath.Count(data => (data.types & (VertexTypes.AddedOnEdge | VertexTypes.ExistingVertex)) != 0 );
            }
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

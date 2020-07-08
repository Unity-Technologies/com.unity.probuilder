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
            public Vector3 m_Position;
            public Vector3 m_Normal;
            public VertexType m_Type;

            public InsertedVertexData(Vector3 position, VertexType type = VertexType.None)
            {
                m_Position = position;
                m_Normal = Vector3.up;
                m_Type = type;
            }

            public InsertedVertexData(Vector3 position, Vector3 normal, VertexType type = VertexType.None)
            {
                m_Position = position;
                m_Normal = normal;
                m_Type = type;
            }
        }

        ProBuilderMesh m_Mesh;

        [SerializeField]
        internal List<InsertedVertexData> m_cutPath = new List<InsertedVertexData>();

        private bool m_CutEnded = false;

        public bool CutEnded
        {
            get => m_CutEnded;
            set => m_CutEnded = value;
        }

        public bool IsALoop
        {
            get
            {
                if (m_cutPath.Count < 3)
                    return false;
                else
                    return Math.Approx3(m_cutPath[0].m_Position,
                        m_cutPath[m_cutPath.Count - 1].m_Position);
            }
        }

        public int ConnectionsToFaceBordersCount
        {
            get
            {
                return m_cutPath.Count(data => (data.m_Type & (VertexType.AddedOnEdge | VertexType.ExistingVertex)) != 0 );
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

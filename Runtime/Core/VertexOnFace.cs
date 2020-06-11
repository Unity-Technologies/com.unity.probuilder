using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace UnityEngine.ProBuilder
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ProBuilderMesh))]
    [ExcludeFromPreset, ExcludeFromObjectFactory]
    public sealed class VertexOnFace : MonoBehaviour
    {
        /// <summary>
        /// Describes the different input states this tool operates in.
        /// </summary>
        internal enum VertexEditMode
        {
            None,
            Add,
            Edit
        }

        ProBuilderMesh m_Mesh;

        [SerializeField]
        internal List<Tuple<Face,List<Vector3>>> m_verticesToAdd = new List<Tuple<Face,List<Vector3>>>();

        [SerializeField]
        VertexEditMode m_EditMode;

        public ReadOnlyCollection<Tuple<Face,List<Vector3>>> verticesToAdd
        {
            get { return new ReadOnlyCollection<Tuple<Face,List<Vector3>>>(m_verticesToAdd); }
        }

        internal VertexEditMode vertexEditMode
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

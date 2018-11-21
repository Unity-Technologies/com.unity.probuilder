using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using System.Collections.ObjectModel;
using System.Linq;

namespace UnityEngine.ProBuilder
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    [ProGridsConditionalSnap]
    sealed class PolyShape : MonoBehaviour
    {
        /// <summary>
        /// Describes the different input states this tool operates in.
        /// </summary>
        public enum PolyEditMode
        {
            None,
            Path,
            Height,
            Edit
        }

        ProBuilderMesh m_Mesh;

        [FormerlySerializedAs("points")]
        [SerializeField]
        internal List<Vector3> m_Points = new List<Vector3>();

        [FormerlySerializedAs("extrude")]
        [SerializeField]
        float m_Extrude = 0f;

        [FormerlySerializedAs("polyEditMode")]
        [SerializeField]
        PolyEditMode m_EditMode;

        [FormerlySerializedAs("flipNormals")]
        [SerializeField]
        bool m_FlipNormals;

        [SerializeField]
        internal bool isOnGrid = true;

        public ReadOnlyCollection<Vector3> controlPoints
        {
            get { return new ReadOnlyCollection<Vector3>(m_Points); }
        }

        public void SetControlPoints(IList<Vector3> points)
        {
            m_Points = points.ToList();
        }

        public float extrude
        {
            get { return m_Extrude; }
            set { m_Extrude = value; }
        }

        public PolyEditMode polyEditMode
        {
            get { return m_EditMode; }
            set { m_EditMode = value; }
        }

        public bool flipNormals
        {
            get { return m_FlipNormals; }
            set { m_FlipNormals = value; }
        }

        public ProBuilderMesh mesh
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

        /// <summary>
        /// ProGridsConditionalSnap tells pg_Editor to reflect this value.
        /// </summary>
        /// <returns></returns>
        bool IsSnapEnabled()
        {
            return isOnGrid;
        }
    }
}

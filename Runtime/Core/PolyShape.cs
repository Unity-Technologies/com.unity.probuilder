using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using System.Collections.ObjectModel;
using System.Linq;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// PolyShape is a component that handles the creation of <see cref="ProBuilderMesh"/> shapes from a set of contiguous points.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    [ExcludeFromPreset, ExcludeFromObjectFactory]
    [ProGridsConditionalSnap]
    public sealed class PolyShape : MonoBehaviour
    {
        /// <summary>
        /// Describes the different input states this tool operates in.
        /// </summary>
        internal enum PolyEditMode
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

        /// <value>
        /// Get the points that form the path for the base of this shape.
        /// </value>
        public ReadOnlyCollection<Vector3> controlPoints
        {
            get { return new ReadOnlyCollection<Vector3>(m_Points); }
        }

        /// <summary>
        /// Set the points that form the path for the base of this shape.
        /// </summary>
        public void SetControlPoints(IList<Vector3> points)
        {
            m_Points = points.ToList();
        }

        /// <value>
        /// Set the distance that this shape should extrude from the base. After setting this value, you will need to
        /// invoke <see cref="MeshOperations.AppendElements.CreateShapeFromPolygon"/> to rebuild the <see cref="ProBuilderMesh"/> component.
        /// </value>
        public float extrude
        {
            get { return m_Extrude; }
            set { m_Extrude = value; }
        }

        internal PolyEditMode polyEditMode
        {
            get { return m_EditMode; }
            set { m_EditMode = value; }
        }

        /// <value>
        /// Defines what direction the normals of this shape will face. Use this to invert the normals, creating a volume with the normals facing inwards.
        /// </value>
        public bool flipNormals
        {
            get { return m_FlipNormals; }
            set { m_FlipNormals = value; }
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

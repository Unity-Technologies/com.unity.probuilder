using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;
using System.Collections.ObjectModel;
using System.Linq;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// Represents a component that handles the creation of <see cref="ProBuilderMesh"/> shapes
    /// from a set of contiguous points.
    /// </summary>
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    [ExcludeFromPreset, ExcludeFromObjectFactory]
    [Icon(k_IconPath)]
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
        const string k_IconPath = "Packages/com.unity.probuilder/Content/Icons/EditableMesh/EditableMesh.png";

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

        /// <summary>
        /// Gets the points that form the path for the base of this shape.
        /// </summary>
        public ReadOnlyCollection<Vector3> controlPoints
        {
            get { return new ReadOnlyCollection<Vector3>(m_Points); }
        }

        /// <summary>
        /// Sets the list of points that form the path for the base of this shape.
        /// </summary>
        /// <param name="points">List of positions that define this PolyShape.</param>
        public void SetControlPoints(IList<Vector3> points)
        {
            m_Points = points.ToList();
        }

        /// <summary>
        /// Gets or sets the distance that this shape should extrude from the base. After setting this value,
        /// you need to invoke <see cref="MeshOperations.AppendElements.CreateShapeFromPolygon"/> to
        /// rebuild the <see cref="ProBuilderMesh"/> component.
        /// </summary>
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

        /// <summary>
        /// Defines the direction for this shape's normals. Use this to invert the normals, creating a
        /// volume with the normals facing inwards.
        /// </summary>
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

using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;


namespace UnityEngine.ProBuilder
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(ProBuilderMesh))]
    [ExcludeFromPreset, ExcludeFromObjectFactory]
    public sealed class CutShape : MonoBehaviour
    {
        /// <summary>
        /// Describes the different input states this tool operates in.
        /// </summary>
        internal enum CutEditMode
        {
            None,
            Path,
            Edit
        }

        ProBuilderMesh m_Mesh;

        [SerializeField]
        internal List<Vector3> m_Points = new List<Vector3>();

        [SerializeField]
        CutEditMode m_EditMode;

        /// <value>
        /// Get the points that form the path for the base of this shape.
        /// </value>
        public ReadOnlyCollection<Vector3> cuttingPoints
        {
            get { return new ReadOnlyCollection<Vector3>(m_Points); }
        }

        /// <summary>
        /// Set the points that form the path for the base of this shape.
        /// </summary>
        public void SetCuttingPoints(IList<Vector3> points)
        {
            m_Points = points.ToList();
        }

        internal CutEditMode cutEditMode
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

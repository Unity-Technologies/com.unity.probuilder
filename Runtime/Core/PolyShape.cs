using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace UnityEngine.ProBuilder
{
    [AddComponentMenu("")]
    [DisallowMultipleComponent]
    [ProGridsConditionalSnap]
    public class PolyShape : MonoBehaviour
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

        [SerializeField]
        List<Vector3> m_Points = new List<Vector3>();

        [SerializeField]
        float m_Extrude = 0f;

        [SerializeField]
        PolyEditMode m_EditMode;

        [SerializeField]
        bool m_FlipNormals;

        [SerializeField]
        bool m_IsOnGrid = true;

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
				if(m_Mesh == null)
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
			return m_IsOnGrid;
		}
	}
}

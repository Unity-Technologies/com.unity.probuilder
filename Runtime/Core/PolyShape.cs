using UnityEngine;
using System.Collections.Generic;

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

		public List<Vector3> points = new List<Vector3>();
		public float extrude = 0f;
		public PolyEditMode polyEditMode = PolyEditMode.None;
		public bool flipNormals = false;
		public bool isOnGrid = true;

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
			return isOnGrid;
		}
	}
}

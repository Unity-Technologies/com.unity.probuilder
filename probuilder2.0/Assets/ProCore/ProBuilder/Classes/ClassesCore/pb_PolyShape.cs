using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	[AddComponentMenu("")]
	[DisallowMultipleComponent]
	[ProGridsConditionalSnap]
	public class pb_PolyShape : MonoBehaviour
	{
		/**
		 *	Describes the different input states this tool operates in.
		 */
		public enum PolyEditMode
		{
			None,
			Path,
			Height,
			Edit
		}

		public List<Vector3> points = new List<Vector3>();
		public float extrude = .25f;
		public PolyEditMode polyEditMode = PolyEditMode.None;
		public bool flipNormals = false;
		private pb_Object m_Mesh;
		public bool isOnGrid = true;

		public pb_Object mesh
		{
			get
			{
				if(m_Mesh == null)
					m_Mesh = GetComponent<pb_Object>();

				return m_Mesh;
			}

			set
			{
				m_Mesh = value;
			}
		}

		/**
		 *	ProGridsConditionalSnap tells pg_Editor to reflect this value.
		 */
		private bool IsSnapEnabled()
		{
			return isOnGrid;
		}
	}
}

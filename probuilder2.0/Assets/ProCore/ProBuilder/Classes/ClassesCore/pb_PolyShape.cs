using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.MeshOperations;

namespace ProBuilder2.Common
{
	[AddComponentMenu("")]
	[DisallowMultipleComponent]
	public class pb_PolyShape : MonoBehaviour
	{
		public List<Vector3> m_Points = new List<Vector3>();
		public float m_Extrude = 0.1f;

		private pb_Object m_Mesh;

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
		 * @todo
		 */
		public void Init()
		{
			m_Points.Add(new Vector3(0f, 0f, 0f));
			m_Points.Add(new Vector3(1f, 0f, 0f));
			m_Points.Add(new Vector3(1f, 0f, 1f));
		}

		/**
		 *	@todo
		 */
		public void Refresh()
		{
			pb_Object m = mesh;

			Vector3[] vertices = m_Points.ToArray();
			List<int> triangles;

			if(pb_Triangulation.TriangulateVertices(vertices, out triangles, false))
			{
				m.GeometryWithVerticesFaces(vertices, new pb_Face[] { new pb_Face(triangles.ToArray() ) });
			}

			m.ToMesh();
			m.Refresh();
		}
	}
}

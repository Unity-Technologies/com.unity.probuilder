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
		public List<Vector3> points = new List<Vector3>();
		public float extrude = 0.1f;
		public bool isEditing = false;

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
		}

		/**
		 *	@todo
		 */
		public void Refresh()
		{
			pb_Object m = mesh;

			if(points.Count < 3)
			{
				m.SetVertices(new Vector3[0]);
				m.SetFaces(new pb_Face[0]);
				m.SetSharedIndices(new pb_IntArray[0]);
				return;
			}

			Vector3[] vertices = points.ToArray();
			List<int> triangles;

			if(pb_Triangulation.TriangulateVertices(vertices, out triangles, false))
			{
				m.GeometryWithVerticesFaces(vertices, new pb_Face[] { new pb_Face(triangles.ToArray() ) });
				m.DuplicateAndFlip(m.faces);
				m.Extrude(new pb_Face[] { m.faces[1] }, ExtrudeMethod.IndividualFaces, extrude);
			}
			else
			{
				points.RemoveAt(points.Count - 1);
			}

			m.ToMesh();
			m.Refresh();
		}
	}
}

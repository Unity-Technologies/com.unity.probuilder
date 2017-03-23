using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.MeshOperations;

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
		public float extrude = 1f;
		public PolyEditMode polyEditMode = PolyEditMode.None;
		public bool flipNormals = false;
		private pb_Object m_Mesh;
		public bool isOnGrid = true;

		void Reset()
		{
		}

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
		public bool Refresh()
		{
			pb_Object m = mesh;

			if(points.Count < 3)
			{
				m.SetVertices(new Vector3[0]);
				m.SetFaces(new pb_Face[0]);
				m.SetSharedIndices(new pb_IntArray[0]);
				return true;
			}

			Vector3[] vertices = points.ToArray();
			List<int> triangles;

			pb_Log.PushLogLevel(pb_LogLevel.Error);

			if(pb_Triangulation.TriangulateVertices(vertices, out triangles, false))
			{
				int[] indices = triangles.ToArray();

				if(pb_Math.PolygonArea(vertices, indices) < Mathf.Epsilon )
				{
					m.SetVertices(new Vector3[0]);
					m.SetFaces(new pb_Face[0]);
					m.SetSharedIndices(new pb_IntArray[0]);
					pb_Log.PopLogLevel();
					return false;
				}

				m.GeometryWithVerticesFaces(vertices, new pb_Face[] { new pb_Face(indices) });

				Vector3 nrm = pb_Math.Normal(m, m.faces[0]);

				if(Vector3.Dot(Vector3.up, nrm) > 0f)
					m.faces[0].ReverseIndices();

				m.DuplicateAndFlip(m.faces);

				m.Extrude(new pb_Face[] { m.faces[1] }, ExtrudeMethod.IndividualFaces, extrude);

				if((extrude < 0f && !flipNormals) || (extrude > 0f && flipNormals))
					m.ReverseWindingOrder(m.faces);
			}
			else
			{
				pb_Log.PopLogLevel();
				return false;
			}
			
			pb_Log.PopLogLevel();

			m.ToMesh();
			m.Refresh();

			return true;
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

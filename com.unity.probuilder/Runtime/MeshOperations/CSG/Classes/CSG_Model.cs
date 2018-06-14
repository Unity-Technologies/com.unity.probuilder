using UnityEngine;
using System.Collections.Generic;

namespace UnityEngine.ProBuilder.Experimental.CSG
{
	/// <summary>
	/// Representation of a mesh in CSG terms.  Contains methods for translating to and from UnityEngine.Mesh.
	/// </summary>
	sealed class CSG_Model
	{
		public List<CSG_Vertex> vertexes;
		public List<int> indexes;

		public CSG_Model()
		{
			vertexes = new List<CSG_Vertex>();
			indexes = new List<int>();
		}

		/**
		 * Initialize a CSG_Model with the mesh of a gameObject.
		 */
		public CSG_Model(GameObject go)
		{
			vertexes = new List<CSG_Vertex>();

			Mesh m = go.GetComponent<MeshFilter>().sharedMesh;
			Transform trans = go.GetComponent<Transform>();

			Vector3[] v = m.vertices;
			Vector3[] n = m.normals;
			Vector2[] u = m.uv;
			Color[] c = m.colors;

			for(int i = 0; i < v.Length; i++)
				vertexes.Add( new CSG_Vertex(trans.TransformPoint(v[i]), trans.TransformDirection(n[i]), u[i], c[i]) );

			indexes = new List<int>(m.triangles);
		}

		public CSG_Model(List<CSG_Polygon> list)
		{
			this.vertexes = new List<CSG_Vertex>();
			this.indexes = new List<int>();

			int p = 0;
			for (int i = 0; i < list.Count; i++)
			{
				CSG_Polygon poly = list[i];

				for (int j = 2; j < poly.vertexes.Count; j++)
				{
					this.vertexes.Add(poly.vertexes[0]);
					this.indexes.Add(p++);

					this.vertexes.Add(poly.vertexes[j - 1]);
					this.indexes.Add(p++);

					this.vertexes.Add(poly.vertexes[j]);
					this.indexes.Add(p++);
				}
			}
		}

		public List<CSG_Polygon> ToPolygons()
		{
			List<CSG_Polygon> list = new List<CSG_Polygon>();

			for (int i = 0; i < indexes.Count; i+= 3)
			{
				List<CSG_Vertex> triangle = new List<CSG_Vertex>()
				{
					vertexes[indexes[i+0]],
					vertexes[indexes[i+1]],
					vertexes[indexes[i+2]]
				};

				list.Add(new CSG_Polygon(triangle));
			}

			return list;
		}

		/**
		 * Converts a CSG_Model to a Unity mesh.
		 */
		public Mesh ToMesh()
		{
			Mesh m = new Mesh();

			int vc = vertexes.Count;

			Vector3[] v = new Vector3[vc];
			Vector3[] n = new Vector3[vc];
			Vector2[] u = new Vector2[vc];
			Color[] c = new Color[vc];

			for(int i = 0; i < vc; i++)
			{
				v[i] = this.vertexes[i].position;
				n[i] = this.vertexes[i].normal;
				u[i] = this.vertexes[i].uv;
				c[i] = this.vertexes[i].color;
			}

			m.vertices = v;
			m.normals = n;
			m.colors = c;
			m.uv = u;
			m.triangles = this.indexes.ToArray();

			return m;
		}
	}
}
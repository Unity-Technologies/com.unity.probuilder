using UnityEngine;
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	public class pb_Mesh_Utility
	{

		/**
		 * Generate a line segment bounds representation.
		 */
		public static Mesh BoundsWireframe(Bounds bounds)
		{
			Vector3 cen = bounds.center;
			Vector3 ext = bounds.extents;

			// Draw Wireframe
			List<Vector3> v = new List<Vector3>();

			v.AddRange( DrawBoundsEdge(cen, -ext.x, -ext.y, -ext.z, .2f) );
			v.AddRange( DrawBoundsEdge(cen, -ext.x, -ext.y,  ext.z, .2f) );
			v.AddRange( DrawBoundsEdge(cen,  ext.x, -ext.y, -ext.z, .2f) );
			v.AddRange( DrawBoundsEdge(cen,  ext.x, -ext.y,  ext.z, .2f) );

			v.AddRange( DrawBoundsEdge(cen, -ext.x,  ext.y, -ext.z, .2f) );
			v.AddRange( DrawBoundsEdge(cen, -ext.x,  ext.y,  ext.z, .2f) );
			v.AddRange( DrawBoundsEdge(cen,  ext.x,  ext.y, -ext.z, .2f) );
			v.AddRange( DrawBoundsEdge(cen,  ext.x,  ext.y,  ext.z, .2f) );

			Vector2[] u = new Vector2[48];
			int[] t = new int[48];
			Color[] c = new Color[48];

			for(int i = 0; i < 48; i++)
			{
				t[i] = i;
				u[i] = Vector2.zero;
				c[i] = Color.white;
				c[i].a = .5f;
			}

			Mesh m = new Mesh();
			m.vertices = v.ToArray();
			m.subMeshCount = 1;
			m.SetIndices(t, MeshTopology.Lines, 0);

			m.uv = u;
			m.normals = v.ToArray();
			m.colors = c; 

			return m;
		}

		private static Vector3[] DrawBoundsEdge(Vector3 center, float x, float y, float z, float size)
		{
			Vector3 p = center;
			Vector3[] v = new Vector3[6];

			p.x += x;
			p.y += y;
			p.z += z;

			v[0] = p;
			v[1] = (p + ( -(x/Mathf.Abs(x)) * Vector3.right 	* Mathf.Min(size, Mathf.Abs(x))));

			v[2] = p;
			v[3] = (p + ( -(y/Mathf.Abs(y)) * Vector3.up 		* Mathf.Min(size, Mathf.Abs(y))));

			v[4] = p;
			v[5] = (p + ( -(z/Mathf.Abs(z)) * Vector3.forward 	* Mathf.Min(size, Mathf.Abs(z))));

			return v;
		}
	}
}
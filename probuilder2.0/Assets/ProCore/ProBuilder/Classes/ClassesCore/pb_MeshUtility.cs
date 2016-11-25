using UnityEngine;
using System.Linq;
using System.Collections.Generic;

namespace ProBuilder2.Common
{
	/**
	 * Utilities for working with UnityEngine.Mesh.
	 */
	public class pb_MeshUtility
	{
		public static string Print(Mesh m)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			sb.AppendLine(string.Format("vertices: {0}\ntriangles: {1}\nsubmeshes: {2}", m.vertexCount, m.triangles.Length, m.subMeshCount));

			sb.AppendLine(string.Format("{0,-28}{1,-28}{2,-28}{3,-28}{4,-28}{5,-28}{6,-28}",
				"Positions",
				"Colors",
				"Tangents",
				"UV0",
				"UV2",
				"UV3",
				"UV4"));

			Vector3[] positions = m.vertices;
			Color[] colors 		= m.colors;
			Vector4[] tangents 	= m.tangents;

			List<Vector4> uv0 	= new List<Vector4>();
			Vector2[] uv2 		= m.uv2;
			List<Vector4> uv3 	= new List<Vector4>();
			List<Vector4> uv4 	= new List<Vector4>();

#if !UNITY_4_7 && !UNITY_5_0
			m.GetUVs(0, uv0);
			m.GetUVs(2, uv3);
			m.GetUVs(3, uv4);
#else
			uv0 = m.uv.Cast<Vector4>().ToList();
#endif

			if( positions != null && positions.Count() != m.vertexCount)
				positions = null;
			if( colors != null && colors.Count() != m.vertexCount)
				colors = null;
			if( tangents != null && tangents.Count() != m.vertexCount)
				tangents = null;
			if( uv0 != null && uv0.Count() != m.vertexCount)
				uv0 = null;
			if( uv2 != null && uv2.Count() != m.vertexCount)
				uv2 = null;
			if( uv3 != null && uv3.Count() != m.vertexCount)
				uv3 = null;
			if( uv4 != null && uv4.Count() != m.vertexCount)
				uv4 = null;

			for(int i = 0; i < m.vertexCount; i ++)
			{
				sb.AppendLine(string.Format("{0,-28}{1,-28}{2,-28}{3,-28}{4,-28}{5,-28}{6,-28}",
					positions == null 	? "null" : string.Format("{0:F2}, {1:F2}, {2:F2}", positions[i].x, positions[i].y, positions[i].z),
					colors == null 		? "null" : string.Format("{0:F2}, {1:F2}, {2:F2}, {3:F2}", colors[i].r, colors[i].g, colors[i].b, colors[i].a),
					tangents == null 	? "null" : string.Format("{0:F2}, {1:F2}, {2:F2}, {3:F2}", tangents[i].x, tangents[i].y, tangents[i].z, tangents[i].w),
					uv0 == null 		? "null" : string.Format("{0:F2}, {1:F2}, {2:F2}, {3:F2}", uv0[i].x, uv0[i].y, uv0[i].z, uv0[i].w),
					uv2 == null 		? "null" : string.Format("{0:F2}, {1:F2}", uv2[i].x, uv2[i].y),
					uv3 == null 		? "null" : string.Format("{0:F2}, {1:F2}, {2:F2}, {3:F2}", uv3[i].x, uv3[i].y, uv3[i].z, uv3[i].w),
					uv4 == null 		? "null" : string.Format("{0:F2}, {1:F2}, {2:F2}, {3:F2}", uv4[i].x, uv4[i].y, uv4[i].z, uv4[i].w)));
			}

			for(int i = 0; i < m.triangles.Length; i+=3)
				sb.AppendLine(string.Format("{0}, {1}, {2}", m.triangles[i], m.triangles[i+1], m.triangles[i+2]));

			return sb.ToString();
		}

		/**
		 *	Set a mesh to use individual triangle topology.  Returns a pb_Vertex array
		 *	of the per-triangle vertices.
		 */
		public static pb_Vertex[] GeneratePerTriangleMesh(Mesh m)
		{
			pb_Vertex[] vertices 	= pb_Vertex.GetVertices(m);
			int smc 				= m.subMeshCount;
			pb_Vertex[] tv 			= new pb_Vertex[m.triangles.Length];
			int[][] triangles 		= new int[smc][];
			int triIndex 			= 0;

			for(int s = 0; s < smc; s++)
			{
				triangles[s] = m.GetTriangles(s);
				int tl = triangles[s].Length;

				for(int i = 0; i < tl; i++)
				{
					tv[triIndex++] = new pb_Vertex( vertices[triangles[s][i]] );
					triangles[s][i] = triIndex - 1;
				}
			}

			pb_Vertex.SetMesh(m, tv);

			m.subMeshCount = smc;

			for(int s = 0; s < smc; s++)
				m.SetTriangles(triangles[s], s);

			return tv;
		}

		/**
		 *	Collapse vertices where possible and apply to mesh m.
		 */
		public static void CollapseSharedVertices(Mesh m, pb_Vertex[] vertices = null)
		{
			if(vertices == null)
				vertices = pb_Vertex.GetVertices(m);

			int smc = m.subMeshCount;
			List<Dictionary<pb_Vertex, int>> sub_vertices = new List<Dictionary<pb_Vertex, int>>();
			int[][] tris = new int[smc][];
			int sub_index = 0;

			for(int i = 0; i < smc; ++i)
			{
				tris[i] = m.GetTriangles(i);
				Dictionary<pb_Vertex, int> new_vertices = new Dictionary<pb_Vertex, int>();

				for(int n = 0; n < tris[i].Length; n++)
				{
					pb_Vertex v = vertices[tris[i][n]];
					int index;

					if(new_vertices.TryGetValue(v, out index))
					{
						tris[i][n] = index;
					}
					else
					{
						tris[i][n] = sub_index;
						new_vertices.Add(v, sub_index);
						sub_index++;
					}
				}

				sub_vertices.Add(new_vertices);
			}

			pb_Vertex[] collapsed = sub_vertices.SelectMany(x => x.Keys).ToArray();

			pb_Vertex.SetMesh(m, collapsed);

			m.subMeshCount = smc;

			for(int i = 0; i < smc; i++)
				m.SetTriangles(tris[i], i);
		}

		/**
		 * Generate tangents for the mesh.
		 */
		public static void GenerateTangent(ref Mesh InMesh)
		{
			// http://answers.unity3d.com/questions/7789/calculating-tangents-vector4.html

			// speed up math by copying the mesh arrays
			int[] triangles 	= InMesh.triangles;
			Vector3[] vertices 	= InMesh.vertices;
			Vector2[] uv 		= InMesh.uv;
			Vector3[] normals 	= InMesh.normals;

			//variable definitions
			int triangleCount = triangles.Length;
			int vertexCount = vertices.Length;

			Vector3[] tan1 = new Vector3[vertexCount];
			Vector3[] tan2 = new Vector3[vertexCount];

			Vector4[] tangents = new Vector4[vertexCount];

			for (long a = 0; a < triangleCount; a += 3)
			{
				long i1 = triangles[a + 0];
				long i2 = triangles[a + 1];
				long i3 = triangles[a + 2];

				Vector3 v1 = vertices[i1];
				Vector3 v2 = vertices[i2];
				Vector3 v3 = vertices[i3];

				Vector2 w1 = uv[i1];
				Vector2 w2 = uv[i2];
				Vector2 w3 = uv[i3];

				float x1 = v2.x - v1.x;
				float x2 = v3.x - v1.x;
				float y1 = v2.y - v1.y;
				float y2 = v3.y - v1.y;
				float z1 = v2.z - v1.z;
				float z2 = v3.z - v1.z;

				float s1 = w2.x - w1.x;
				float s2 = w3.x - w1.x;
				float t1 = w2.y - w1.y;
				float t2 = w3.y - w1.y;

				float r = 1.0f / (s1 * t2 - s2 * t1);

				Vector3 sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
				Vector3 tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

				tan1[i1] += sdir;
				tan1[i2] += sdir;
				tan1[i3] += sdir;

				tan2[i1] += tdir;
				tan2[i2] += tdir;
				tan2[i3] += tdir;
			}


			for (long a = 0; a < vertexCount; ++a)
			{
				Vector3 n = normals[a];
				Vector3 t = tan1[a];

				Vector3.OrthoNormalize(ref n, ref t);
				tangents[a].x = t.x;
				tangents[a].y = t.y;
				tangents[a].z = t.z;

				tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
			}

			InMesh.tangents = tangents;
		}

		/**
		 *	\brief Performs a deep copy of a mesh and returns a new mesh object.
		 *	@param _mesh The mesh to copy.
		 *	\returns Copied mesh object.
		 */
		public static Mesh DeepCopy(Mesh mesh)
		{
			Mesh m = new Mesh();
			CopyTo(mesh, m);
			return m;
		}

		/**
		 *	Copy source mesh values to destination mesh.
		 */
		public static void CopyTo(Mesh source, Mesh destination)
		{
			Vector3[] v = new Vector3[source.vertices.Length];
			int[][]   t = new int[source.subMeshCount][];
			Vector2[] u = new Vector2[source.uv.Length];
			Vector2[] u2 = new Vector2[source.uv2.Length];
			Vector4[] tan = new Vector4[source.tangents.Length];
			Vector3[] n = new Vector3[source.normals.Length];
			Color32[] c = new Color32[source.colors32.Length];

			System.Array.Copy(source.vertices, v, v.Length);

			for(int i = 0; i < t.Length; i++)
				t[i] = source.GetTriangles(i);

			System.Array.Copy(source.uv, u, u.Length);
			System.Array.Copy(source.uv2, u2, u2.Length);
			System.Array.Copy(source.normals, n, n.Length);
			System.Array.Copy(source.tangents, tan, tan.Length);
			System.Array.Copy(source.colors32, c, c.Length);

			destination.Clear();
			destination.name = source.name;

			destination.vertices = v;

			destination.subMeshCount = t.Length;

			for(int i = 0; i < t.Length; i++)
				destination.SetTriangles(t[i], i);

			destination.uv = u;
			destination.uv2 = u2;
			destination.tangents = tan;
			destination.normals = n;
			destination.colors32 = c;
		}
	}
}

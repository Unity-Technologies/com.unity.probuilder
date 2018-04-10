using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace ProBuilder.Core
{
	/// <summary>
	/// Utilities for working with UnityEngine.Mesh.
	/// </summary>
	public class pb_MeshUtility
	{
		/// <summary>
		/// Set a mesh to use individual triangle topology.
		/// </summary>
		/// <param name="mesh">The mesh to extract vertices from.</param>
		/// <returns>A pb_Vertex array of the per-triangle vertices.</returns>
		public static pb_Vertex[] GeneratePerTriangleMesh(Mesh mesh)
		{
			pb_Vertex[] vertices 	= pb_Vertex.GetVertices(mesh);
			int smc 				= mesh.subMeshCount;
			pb_Vertex[] tv 			= new pb_Vertex[mesh.triangles.Length];
			int[][] triangles 		= new int[smc][];
			int triIndex 			= 0;

			for(int s = 0; s < smc; s++)
			{
				triangles[s] = mesh.GetTriangles(s);
				int tl = triangles[s].Length;

				for(int i = 0; i < tl; i++)
				{
					tv[triIndex++] = new pb_Vertex( vertices[triangles[s][i]] );
					triangles[s][i] = triIndex - 1;
				}
			}

			pb_Vertex.SetMesh(mesh, tv);

			mesh.subMeshCount = smc;

			for(int s = 0; s < smc; s++)
				mesh.SetTriangles(triangles[s], s);

			return tv;
		}

		/// <summary>
		/// Generate tangents for the mesh.
		/// </summary>
		/// <param name="InMesh"></param>
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
		 * \brief Performs a deep copy of a mesh and returns a new mesh object.
		 * @param _mesh The mesh to copy.
		 * \returns Copied mesh object.
		 */
		public static Mesh DeepCopy(Mesh mesh)
		{
			Mesh m = new Mesh();
			CopyTo(mesh, m);
			return m;
		}

		/**
		 * Copy source mesh values to destination mesh.
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

		/**
		 * Calculate mesh normals.
		 */
		public static Vector3[] GenerateNormals(pb_Object pb)
		{
			int vertexCount = pb.vertexCount;
			Vector3[] perTriangleNormal = new Vector3[vertexCount];
			Vector3[] vertices = pb.vertices;
			Vector3[] normals = new Vector3[vertexCount];
			int[] perTriangleAvg = new int[vertexCount];
			pb_Face[] faces = pb.faces;

			for(int find = 0; find < faces.Length; find++)
			{
				int[] indices = faces[find].indices;

				for(int tri = 0; tri < indices.Length; tri += 3)
				{
					int a = indices[tri], b = indices[tri + 1], c = indices[tri + 2];

					Vector3 cross = pb_Math.Normal(vertices[a], vertices[b], vertices[c]);

					perTriangleNormal[a].x += cross.x;
					perTriangleNormal[b].x += cross.x;
					perTriangleNormal[c].x += cross.x;

					perTriangleNormal[a].y += cross.y;
					perTriangleNormal[b].y += cross.y;
					perTriangleNormal[c].y += cross.y;

					perTriangleNormal[a].z += cross.z;
					perTriangleNormal[b].z += cross.z;
					perTriangleNormal[c].z += cross.z;

					perTriangleAvg[a]++;
					perTriangleAvg[b]++;
					perTriangleAvg[c]++;
				}
			}

			for(int i = 0; i < vertexCount; i++)
			{
				normals[i].x = perTriangleNormal[i].x * (float) perTriangleAvg[i];
				normals[i].y = perTriangleNormal[i].y * (float) perTriangleAvg[i];
				normals[i].z = perTriangleNormal[i].z * (float) perTriangleAvg[i];
			}

			return normals;
		}

		/**
		 * Apply smoothing groups to a set of per-face normals.
		 */
		public static void SmoothNormals(pb_Object pb, ref Vector3[] normals)
		{
			// average the soft edge faces
			int vertexCount = pb.vertexCount;
			int[] smoothGroup = new int[vertexCount];
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			pb_Face[] faces = pb.faces;
			int smoothGroupMax = 24;

			// Create a lookup of each triangles smoothing group.
			foreach(pb_Face face in faces)
			{
				foreach(int tri in face.distinctIndices)
				{
					smoothGroup[tri] = face.smoothingGroup;

					if(face.smoothingGroup >= smoothGroupMax)
						smoothGroupMax = face.smoothingGroup + 1;
				}
			}

			Vector3[] averages 	= new Vector3[smoothGroupMax];
			float[] counts 		= new float[smoothGroupMax];

			/**
			 * For each sharedIndices group (individual vertex), find vertices that are in the same smoothing
			 * group and average their normals.
			 */
			for(int i = 0; i < sharedIndices.Length; i++)
			{
				for(int n = 0; n < smoothGroupMax; n++)
				{
					averages[n].x = 0f;
					averages[n].y = 0f;
					averages[n].z = 0f;
					counts[n] = 0f;
				}

				for(int n = 0; n < sharedIndices[i].array.Length; n++)
				{
					int index = sharedIndices[i].array[n];
					int group = smoothGroup[index];

					// Ideally this should only continue on group == NONE, but historically negative values have also
					// been treated as no smoothing.
					if(	group <= pb_Smoothing.SMOOTHING_GROUP_NONE ||
						(group > pb_Smoothing.SMOOTH_RANGE_MAX && group < pb_Smoothing.HARD_RANGE_MAX))
						continue;

					averages[group].x += normals[index].x;
					averages[group].y += normals[index].y;
					averages[group].z += normals[index].z;
					counts[group] += 1f;
				}

				for(int n = 0; n < sharedIndices[i].array.Length; n++)
				{
					int index = sharedIndices[i].array[n];
					int group = smoothGroup[index];

					if( group <= pb_Smoothing.SMOOTHING_GROUP_NONE ||
						(group > pb_Smoothing.SMOOTH_RANGE_MAX && group < pb_Smoothing.HARD_RANGE_MAX))
						continue;

					normals[index].x = averages[group].x / counts[group];
					normals[index].y = averages[group].y / counts[group];
					normals[index].z = averages[group].z / counts[group];
				}
			}
		}

		/**
		 * Get a mesh attribute from either the MeshFilter.sharedMesh or the
		 * MeshRenderer.additionalVertexStreams mesh. If returned array does not
		 * match the vertex count NULL is returned.
		 */
		public static T GetMeshAttribute<T>(GameObject go, System.Func<Mesh, T> attributeGetter) where T : IList
		{
			MeshFilter mf = go.GetComponent<MeshFilter>();
			Mesh mesh = mf != null ? mf.sharedMesh : null;
			T res = default(T);

			if(mesh == null)
				return res;

			int vertexCount = mesh.vertexCount;

#if !UNITY_4_6 && !UNITY_4_7
			MeshRenderer renderer = go.GetComponent<MeshRenderer>();
			Mesh vertexStream = renderer != null ? renderer.additionalVertexStreams : null;

			if(vertexStream != null)
			{
				res = attributeGetter(vertexStream);

				if(res != null && res.Count == vertexCount)
					return res;
			}
#endif
			res = attributeGetter(mesh);

			return res != null && res.Count == vertexCount ? res : default(T);
		}

		/**
		 * Return a detailed account of the mesh.
		 */
		public static string Print(Mesh m)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			sb.AppendLine(string.Format("vertices: {0}\ntriangles: {1}\nsubmeshes: {2}", m.vertexCount, m.triangles.Length, m.subMeshCount));

			sb.AppendLine(string.Format("     {0,-28}{7,-16}{1,-28}{2,-28}{3,-28}{4,-28}{5,-28}{6,-28}",
				"Positions",
				"Colors",
				"Tangents",
				"UV0",
				"UV2",
				"UV3",
				"UV4",
				"Position Hash"));

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
				sb.AppendLine(string.Format("{7,-5}{0,-28}{8,-16}{1,-28}{2,-28}{3,-28}{4,-28}{5,-28}{6,-28}",
					positions == null 	? "null" : string.Format("{0:F3}, {1:F3}, {2:F3}", positions[i].x, positions[i].y, positions[i].z),
					colors == null 		? "null" : string.Format("{0:F2}, {1:F2}, {2:F2}, {3:F2}", colors[i].r, colors[i].g, colors[i].b, colors[i].a),
					tangents == null 	? "null" : string.Format("{0:F2}, {1:F2}, {2:F2}, {3:F2}", tangents[i].x, tangents[i].y, tangents[i].z, tangents[i].w),
					uv0 == null 		? "null" : string.Format("{0:F2}, {1:F2}, {2:F2}, {3:F2}", uv0[i].x, uv0[i].y, uv0[i].z, uv0[i].w),
					uv2 == null 		? "null" : string.Format("{0:F2}, {1:F2}", uv2[i].x, uv2[i].y),
					uv3 == null 		? "null" : string.Format("{0:F2}, {1:F2}, {2:F2}, {3:F2}", uv3[i].x, uv3[i].y, uv3[i].z, uv3[i].w),
					uv4 == null 		? "null" : string.Format("{0:F2}, {1:F2}, {2:F2}, {3:F2}", uv4[i].x, uv4[i].y, uv4[i].z, uv4[i].w),
					i,
					pb_Vector.GetHashCode(positions[i])));
			}

			for(int i = 0; i < m.triangles.Length; i+=3)
				sb.AppendLine(string.Format("{0}, {1}, {2}", m.triangles[i], m.triangles[i+1], m.triangles[i+2]));

			return sb.ToString();
		}

		/// <summary>
		/// Get the number of indices this mesh contains.
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		public static uint GetIndexCount(Mesh m)
		{
			uint sum = 0;

			if (m == null)
				return sum;

			for (int i = 0, c = m.subMeshCount; i < c; i++)
				sum += m.GetIndexCount(i);

			return sum;
		}

		/// <summary>
		/// Get the number of triangles (or quads) this mesh contains.
		/// </summary>
		/// <param name="m"></param>
		/// <returns></returns>
		public static uint GetTriangleCount(Mesh m)
		{
			uint sum = 0;

			if (m == null)
				return sum;

			for (int i = 0, c = m.subMeshCount; i < c; i++)
			{
				if(m.GetTopology(i) == MeshTopology.Triangles)
					sum += m.GetIndexCount(i) / 3;
				else if(m.GetTopology(i) == MeshTopology.Quads)
					sum += m.GetIndexCount(i) / 4;
			}

			return sum;
		}
	}
}

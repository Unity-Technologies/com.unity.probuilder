using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using ProBuilder2.Math;

namespace ProBuilder2.Common
{
	/**
	 * Utilities for working with UnityEngine.Mesh.
	 */
	public class pb_Mesh_Utility
	{
		/**
		 * Collapse shared vertices to a single vertex on the mesh object.  Does not affect
		 * pb_Object vertices.
		 */
		public static void CollapseSharedVertices(pb_Object pb)
		{
			List<List<int>> merge = pb_Mesh_Utility.FindDuplicateVertices(pb);
			Mesh m = pb.msh;
			pb_Mesh_Utility.MergeVertices(merge, ref m);		
		}

		/**
		 * Merge indices to a single vertex.  Operates on a Mesh, not pb_Object.
		 */
		public static void MergeVertices(List<List<int>> InIndices, ref Mesh InMesh)
		{
			// Iterate triangles and point collapse-able verts to the first index
			for(int submeshIndex = 0; submeshIndex < InMesh.subMeshCount; submeshIndex++)
			{
				int[] tris = InMesh.GetTriangles(submeshIndex);

				for(int i = 0; i < tris.Length; i++)
				{
					int index = InIndices.IndexOf(tris[i]);

					if(index < 0)
						continue;

					tris[i] = InIndices[index][0];
				}

				InMesh.SetTriangles(tris, submeshIndex);
			}

			// populate list of unused vertices post-collapse
			List<int> unused = InIndices.SelectMany( x => x.GetRange(1, x.Count - 1) ).ToList();

			RemoveVertices(unused, ref InMesh);
		}

		private static int NearestIndexLessThan(List<int> InArray, int InValue)
		{
			for(int i = 0; i < InArray.Count; i++)
			{
				if( InArray[i] >= InValue )
					return i-1;
			}

			return InArray.Count - 1;
		}

		/**
		 * Rebuild mesh without InUnusedVertices, including shifting the triangle array to compensate.
		 */
		public static void RemoveVertices(List<int> InUnusedVertices, ref Mesh InMesh)
		{
			int vertexCount = InMesh.vertexCount;
			int unusedCount = InUnusedVertices.Count;

			Vector3[] v = InMesh.vertices, v_n = new Vector3[vertexCount - unusedCount];
			Vector3[] n = InMesh.normals, n_n = new Vector3[vertexCount - unusedCount];
			Vector4[] t = InMesh.tangents, t_n = new Vector4[vertexCount - unusedCount];
			Vector2[] u = InMesh.uv, u_n = new Vector2[vertexCount - unusedCount];
			Color[] c 	= InMesh.colors, c_n = new Color[vertexCount - unusedCount];

			InUnusedVertices.Sort();

			int unusedIndex = 0;

			// shift triangles
			for(int submeshIndex = 0; submeshIndex < InMesh.subMeshCount; submeshIndex++)
			{
				int[] tris = InMesh.GetTriangles(submeshIndex);

				for(int i = 0; i < tris.Length; i++)
				{
					unusedIndex = NearestIndexLessThan( InUnusedVertices, tris[i] ) + 1;
					tris[i] -= unusedIndex;
				}

				InMesh.SetTriangles(tris, submeshIndex);
			}

			unusedIndex = 0;
			int newIndex = 0;

			// rebuild vertex arrays without duplicate indices
			for(int i = 0; i < vertexCount; i++)
			{
				if(unusedIndex < unusedCount && i == InUnusedVertices[unusedIndex])
				{
					unusedIndex++;
					continue;
				}

				v_n[newIndex] = v[i];
				n_n[newIndex] = n[i];
				t_n[newIndex] = t[i];
				u_n[newIndex] = u[i];
				c_n[newIndex] = c[i];

				newIndex++;
			}

			InMesh.vertices = v_n;
			InMesh.normals = n_n;
			InMesh.tangents = t_n;
			InMesh.uv = u_n;
			InMesh.colors = c_n;
		}

		/**
		 * Returns a jagged array of indices that share the same position, texture coordinate, and smoothing group.
		 * Must be called after Refresh() but before GenerateUV2().
		 */
		public static List<List<int>> FindDuplicateVertices(pb_Object pb)
		{
			/**
			 * Merge faces in to their groups so the next we can test which indices are actually on
			 * top of one another.
			 */
			Dictionary<int, List<pb_Face>> groups = new Dictionary<int, List<pb_Face>>();
			for(int i = 0; i < pb.faces.Length; i++) {
				// smoothing groups 
				// 0 		= none
				// 1 - 24 	= smooth
				// 25 - 42	= hard
				if(pb.faces[i].smoothingGroup > 0 && pb.faces[i].smoothingGroup < 25)
				{
					if(groups.ContainsKey(pb.faces[i].smoothingGroup))
						groups[pb.faces[i].smoothingGroup].Add(pb.faces[i]);
					else
						groups.Add(pb.faces[i].smoothingGroup, new List<pb_Face>(){pb.faces[i]});
				}
			}

			List<List<int>> indicesToCollapse = new List<List<int>>();

			foreach(KeyValuePair<int, List<pb_Face>> kvp in groups)
			{
				List<int> distinct = pb_Face.AllTrianglesDistinct(kvp.Value);
				Dictionary<int, List<int>> shared = new Dictionary<int, List<int>>();
				int i = 0;

				/**
				 * Find each vertex in the smoothing group that belongs to a sharedIndices group.
				 */
				for(i = 0; i < distinct.Count; i++)
				{
					int sharedIndex = pb.sharedIndices.IndexOf(distinct[i]);
					
					if(shared.ContainsKey(sharedIndex))
						shared[sharedIndex].Add(distinct[i]);
					else
						shared.Add(sharedIndex, new List<int>(){distinct[i]});
				}

				i = 0;
				Vector3[] vertices = pb.vertices;
				Vector2[] uv = pb.uv;
				List<List<int>> merge = new List<List<int>>();	///< Vertices that are smooth, and share the same world and texture coordinate.
				
				/**
				 * Now go through and average the values of each vertex normal that is shared.
				 */
				foreach(KeyValuePair<int, List<int>> skvp in shared)
				{
					List<int> indices = skvp.Value;

					for(int vertexNormalIndex = 0; vertexNormalIndex < indices.Count; vertexNormalIndex++)
					{
						/**
						 * Group indices that share the same vertex position and texture coordinate together.
						 */
						bool merged = false;
						foreach(List<int> mergeGroup in merge)
						{
							if( vertices[mergeGroup[0]].Approx(vertices[indices[vertexNormalIndex]], .001f) &&
							 	uv[mergeGroup[0]].Approx(uv[indices[vertexNormalIndex]], .001f) )
							{
								mergeGroup.Add( indices[vertexNormalIndex] );
								merged = true;
								break;
							}
						}

						if(!merged)
							merge.Add(new List<int>(1) { indices[vertexNormalIndex] });
					}

					// add vertices that can be collapsed to a single vertex to the master list
					indicesToCollapse.AddRange( merge.Where(x => x.Count > 1) );
				}
			}

			return indicesToCollapse.Distinct().ToList();
		}

		/**
		 * Generate tangents for the mesh.
		 */
		public static void GenerateTangent(ref Mesh InMesh)
		{
			// implementation found here (no sense re-inventing the wheel, eh?)
			// http://answers.unity3d.com/questions/7789/calculating-tangents-vector4.html

			//speed up math by copying the mesh arrays
			int[] triangles = InMesh.triangles;
			Vector3[] vertices = InMesh.vertices;
			Vector2[] uv = InMesh.uv;
			Vector3[] normals = InMesh.normals;

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

				//Vector3 tmp = (t - n * Vector3.Dot(n, t)).normalized;
				//tangents[a] = new Vector4(tmp.x, tmp.y, tmp.z);
				Vector3.OrthoNormalize(ref n, ref t);
				tangents[a].x = t.x;
				tangents[a].y = t.y;
				tangents[a].z = t.z;

				tangents[a].w = (Vector3.Dot(Vector3.Cross(n, t), tan2[a]) < 0.0f) ? -1.0f : 1.0f;
			}

			InMesh.tangents = tangents;
		}
	}
}
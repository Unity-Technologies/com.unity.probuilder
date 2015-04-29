using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using ProBuilder2.Math;

namespace ProBuilder2.Common
{
	/**
	 * Utilities for working with UnityEngine.Mesh.
	 */
	public class pb_MeshUtility
	{
		/**
		 * Collapse shared vertices to a single vertex on the mesh object.  Does not affect
		 * pb_Object vertices.
		 */
		public static void CollapseSharedVertices(pb_Object pb)
		{
			List<List<int>> merge = pb_MeshUtility.FindDuplicateVertices(pb);

			Mesh m = pb.msh;

			pb_MeshUtility.MergeVertices(merge, ref m);		
		}

		/**
		 * Merge indices to a single vertex.  Operates on a Mesh, not pb_Object.
		 */
		public static void MergeVertices(List<List<int>> InIndices, ref Mesh InMesh)
		{
			Dictionary<int, int> swapTable = new Dictionary<int, int>();

			foreach(List<int> group in InIndices)
			{
				for(int i = 1; i < group.Count; i++)
				{
					swapTable.Add(group[i], group[0]);
				}
			}				

			// Iterate triangles and point collapse-able verts to the first index
			for(int submeshIndex = 0; submeshIndex < InMesh.subMeshCount; submeshIndex++)
			{
				int[] tris = InMesh.GetTriangles(submeshIndex);

				for(int i = 0; i < tris.Length; i++)
				{
					if( swapTable.ContainsKey(tris[i]) )
						tris[i] = swapTable[tris[i]];
				}
				InMesh.SetTriangles(tris, submeshIndex);
			}

			// populate list of unused vertices post-collapse
			List<int> unused = InIndices.SelectMany( x => x.GetRange(1, x.Count - 1) ).ToList();

			RemoveVertices(unused, ref InMesh);
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
					unusedIndex = pbUtil.NearestIndexPriorToValue( InUnusedVertices, tris[i] ) + 1;
					tris[i] -= unusedIndex;
				}

				InMesh.SetTriangles(tris, submeshIndex);
			}

			unusedIndex = 0;
			int newIndex = 0;

			// rebuild vertex arrays without duplicate indices
			for(int i = 0; i < vertexCount; i++)
			{
				if(unusedIndex < unusedCount && i >= InUnusedVertices[unusedIndex])
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
			Vector3[] normals = pb.msh.normals;
			Vector2[] textures = pb.uv;

			int[] smoothGroup = new int[normals.Length];

			/**
			 * Create a lookup of each triangles smoothing group.
			 */
			foreach(pb_Face face in pb.faces)
			{
				foreach(int tri in face.distinctIndices)
					smoothGroup[tri] = face.smoothingGroup;
			}

			List<int> list;
			List<List<int>> merge = new List<List<int>>();

			/**
			 * For each sharedIndices group (individual vertex), find vertices that are in the same smoothing
			 * group and check if their texture coordinates are similar enough to collapse to a single vertex.
			 */
			for(int i = 0; i < pb.sharedIndices.Length; i++)
			{
				Dictionary<int, List<int>> shareable = new Dictionary<int, List<int>>();

				/**
				 * Sort indices that share a smoothing group
				 */
				foreach(int tri in pb.sharedIndices[i].array)
				{
					if(smoothGroup[tri] < 1 || smoothGroup[tri] > 24)	
						continue;

					if( shareable.TryGetValue(smoothGroup[tri], out list) )
						list.Add(tri);
					else
						shareable.Add(smoothGroup[tri], new List<int>() { tri });
				}

				/**
				 * At this point, `shareable` contains a key value pair of 
				 * { SmoothingGroupKey, All valid triangles pointing to this vertex }
				 */

				/**
				 * Now go through each key value pair and sort them into vertices that
				 * share a 'close enough' texture coordinate to be considered the same.
				 * Don't bother checking position since if they're in the same shared
				 * index group that should always means they're on top of one-another.
				 */

				foreach(KeyValuePair<int, List<int>> group in shareable)
				{			
					List<List<int>> textureMatches = new List<List<int>>();

					foreach(int tri in group.Value)
					{
						bool foundMatch = false;

						for(int n = 0; n < textureMatches.Count; n++)
						{
							if( textures[textureMatches[n][0]].Approx(textures[tri], .001f) )
							{
								textureMatches[n].Add(tri);
								foundMatch = true;
								break;
							}
						}

						if(!foundMatch)
							textureMatches.Add( new List<int>() { tri } );
					}
	
					merge.AddRange( textureMatches.Where(x => x.Count > 1) );
				}
			}

			return merge;
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
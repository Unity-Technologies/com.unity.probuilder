using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.Math;
using ProBuilder2.Triangulator;
using ProBuilder2.Triangulator.Geometry;

namespace ProBuilder2.MeshOperations
{
	public static class pbTriangleOps
	{
		/**
		 *	\brief Reverse the winding order for each passed #pb_Face.
		 *	@param faces The faces to apply normal flippin' to.
		 *	\returns Nothing.  No soup for you.
		 *	\sa SelectedFaces pb_Face
		 */
		public static void ReverseWindingOrder(this pb_Object pb, pb_Face[] faces)
		{
			for(int i = 0; i < faces.Length; i++)
				faces[i].ReverseIndices();
		}	

		/**
		 * Attempt to figure out the winding order the passed face.  Note that 
		 * this may return WindingOrder.Unknown.
		 */
		public static WindingOrder GetWindingOrder(this pb_Object pb, pb_Face face)
		{
			Vector2[] p = pb_Math.PlanarProject(pb.GetVertices( face.edges.AllTriangles() ), pb_Math.Normal(pb, face));

			float sum = 0f;

			// http://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
			for(int i = 0; i < p.Length; i++)
			{
				Vector2 a = p[i];
				Vector2 b = i < p.Length - 1 ? p[i+1] : p[0];

				sum += ( (b.x-a.x) * (b.y+a.y) );
			}

			return sum == 0f ? WindingOrder.Unknown : (sum >= 0f ? WindingOrder.Clockwise : WindingOrder.CounterClockwise);
		}

		/**
		 *	Iterates through all triangles in a pb_Object and removes triangles with area <= 0 and 
		 *	tris with indices that point to the same vertex.
		 * \returns True if Degenerate tris were found, false if no changes.
		 */
		public static bool RemoveDegenerateTriangles(this pb_Object pb, out int[] removed)
		{
			pb_IntArray[] sharedIndices = pb.sharedIndices;
			Vector3[] v = pb.vertices;
			List<pb_Face> del = new List<pb_Face>();

			List<pb_Face> f = new List<pb_Face>();

			foreach(pb_Face face in pb.faces)
			{
				List<int> tris = new List<int>();
		
				int[] ind = face.indices;
				for(int i = 0; i < ind.Length; i+=3)
				{
					int[] s = new int[3]
					{
						sharedIndices.IndexOf(ind[i+0]),
						sharedIndices.IndexOf(ind[i+1]),
						sharedIndices.IndexOf(ind[i+2])
					};

					float area = pb_Math.TriangleArea(v[ind[i+0]], v[ind[i+1]], v[ind[i+2]]);

					if( (s[0] == s[1] || s[0] == s[2] || s[1] == s[2]) || area <= 0 )
					{
						// don't include this face in the reconstruct
						;
					}
					else
					{
						tris.Add(ind[i+0]);
						tris.Add(ind[i+1]);
						tris.Add(ind[i+2]);
					}
				}

				if(tris.Count > 0)
				{
					face.SetIndices(tris.ToArray());
					face.RebuildCaches();

					f.Add(face);
				}
				else
				{
					del.Add(face);
				}
			}

			pb.SetFaces(f.ToArray());

			removed = pb.RemoveUnusedVertices();
			return removed.Length > 0;
		}

		/**
		 * Merge all faces into a sigle face.
		 */
		public static pb_Face MergeFaces(this pb_Object pb, pb_Face[] faces)
		{
			List<int> collectedIndices = new List<int>(faces[0].indices);
			
			for(int i = 1; i < faces.Length; i++)
			{
				collectedIndices.AddRange(faces[i].indices);
			}

			pb_Face mergedFace = new pb_Face(collectedIndices.ToArray(),
			                                 faces[0].material,
			                                 faces[0].uv,
			                                 faces[0].smoothingGroup,
			                                 faces[0].textureGroup,
			                                 faces[0].elementGroup,
			                                 faces[0].manualUV);

			pb_Face[] rebuiltFaces = new pb_Face[pb.faces.Length - faces.Length + 1];

			int n = 0;
			foreach(pb_Face f in pb.faces)
			{
				if(System.Array.IndexOf(faces, f) < 0)
				{
					rebuiltFaces[n++] = f;
				}
			}
			
			rebuiltFaces[n] = mergedFace;

			pb.SetFaces(rebuiltFaces);

			// merge vertices that are on top of one another now that they share a face
			Dictionary<int, int> shared = new Dictionary<int, int>();

			for(int i = 0; i < mergedFace.indices.Length; i++)
			{
				int sharedIndex = pb.sharedIndices.IndexOf(mergedFace.indices[i]);

				if(shared.ContainsKey(sharedIndex))
				{
					mergedFace.indices[i] = shared[sharedIndex];
				}
				else
				{
					shared.Add(sharedIndex, mergedFace.indices[i]);
				}
			}

			pb.RemoveUnusedVertices();

			return mergedFace;
		}

		/**
		 * Re-triangulates a face with existing indices and vertices.
		 */
		public static void TriangulateFace(this pb_Object pb, pb_Face face, Vector3? projectionAxis)
		{
			int[] orig = face.indices;

			Vector3[] v3d = pbUtil.ValuesWithIndices(pb.vertices, orig);
			Vector3 nrm = (Vector3)(projectionAxis ?? Vector3.Cross(v3d[2]-v3d[0], v3d[1]-v3d[0]));
			Vector2[] v2d = pb_Math.PlanarProject(v3d, nrm);

			int[] tris = Delaunay.Triangulate(new List<Vector2>( v2d )).ToIntArray();

			int[] new_indices = new int[tris.Length];

			for(int i = 0; i < tris.Length; i++)
			{
				new_indices[i] = orig[ tris[i] ];
			}

			face.SetIndices(new_indices);
		}

		/**
		 * Triangulate an entire pb_Object.
		 */
		public static void Triangulate(pb_Object pb)
		{
			Vector3[] 	v = pb.vertices;
			Color[] 	c = pb.colors;
			Vector2[] 	u = pb.uv;

			int triangleCount = pb.TriangleCount();
			// int triangleCount = pb_Face.AllTriangles(pb.faces).Length; // pb.msh.triangles.Length;

			if(triangleCount == v.Length)
			{
				Debug.LogWarning("We can't pull over any further!\npb_Object: " + pb.name + " is already triangulated.");
			}

			int vertexCount = triangleCount;
			int faceCount = vertexCount / 3;

			Vector3[]	tri_vertices = new Vector3[vertexCount];
			Color[] 	tri_colors = new Color[vertexCount];
			Vector2[]	tri_uvs = new Vector2[vertexCount];
			pb_Face[]	tri_faces = new pb_Face[faceCount];

			int n = 0, f = 0;
			foreach(pb_Face face in pb.faces)
			{
				int[] indices = face.indices;

				for(int i = 0; i < indices.Length; i+=3)
				{
					tri_vertices[n+0] = v[indices[i+0]];
					tri_vertices[n+1] = v[indices[i+1]];
					tri_vertices[n+2] = v[indices[i+2]];

					tri_colors[n+0] = c[indices[i+0]];
					tri_colors[n+1] = c[indices[i+1]];
					tri_colors[n+2] = c[indices[i+2]];

					tri_uvs[n+0] = u[indices[i+0]];
					tri_uvs[n+1] = u[indices[i+1]];
					tri_uvs[n+2] = u[indices[i+2]];
		
					tri_faces[f++] = new pb_Face( new int[] { n+0, n+1, n+2 },
												face.material,
												face.uv,
												face.smoothingGroup,
												face.textureGroup,		// textureGroup -> force to manual uv mode
												face.elementGroup,
												face.manualUV
											);	
					n += 3;
				}

			}

			pb.SetVertices(tri_vertices);
			pb.SetColors(tri_colors);
			pb.SetUV(tri_uvs);
			pb.SetFaces(tri_faces);

			pb.SetSharedIndices( pb_IntArrayUtility.ExtractSharedIndices(tri_vertices) );
			pb.SetSharedIndicesUV( new pb_IntArray[0] );
		}
	}
}
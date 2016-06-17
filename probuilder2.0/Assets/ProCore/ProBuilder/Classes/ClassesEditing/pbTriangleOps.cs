using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using System.Linq;

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
		 *	Attempt to figure out the winding order the passed face.  
		 *
		 *	Note that this may return WindingOrder.Unknown.
		 */
		public static WindingOrder GetWindingOrder(this pb_Object pb, pb_Face face)
		{
			Vector2[] p = pb_Projection.PlanarProject(pb.vertices.ValuesWithIndices( face.edges.AllTriangles() ), pb_Math.Normal(pb, face));

			return GetWindingOrder(p);
		}

		public static WindingOrder GetWindingOrder(IList<Vector2> points)
		{
			float sum = 0f;

			int len = points.Count;

			// http://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order
			for(int i = 0; i < len; i++)
			{
				Vector2 a = points[i];
				Vector2 b = i < len - 1 ? points[i+1] : points[0];

				sum += ( (b.x - a.x) * (b.y+a.y) );
			}

			return sum == 0f ? WindingOrder.Unknown : (sum >= 0f ? WindingOrder.Clockwise : WindingOrder.CounterClockwise);
		}

		/**
		 * Reverses the orientation of the middle edge in a quad.
		 */
		public static bool FlipEdge(this pb_Object pb, pb_Face face)
		{
			int[] indices = face.indices;

			if(indices.Length != 6)
				return false;

			int[] mode = pbUtil.FilledArray<int>(1, indices.Length);

			for(int x = 0; x < indices.Length - 1; x++)
			{
				for(int y = x+1; y < indices.Length; y++)
				{
					if(indices[x] == indices[y])
					{
						mode[x]++;
						mode[y]++;
					}
				}
			}

			if(	mode[0] + mode[1] + mode[2] != 5 ||
				mode[3] + mode[4] + mode[5] != 5 )
				return false;

			int i0 = indices[ mode[0] == 1 ? 0 : mode[1] == 1 ? 1 : 2 ];
			int i1 = indices[ mode[3] == 1 ? 3 : mode[4] == 1 ? 4 : 5 ];

			int used = -1;

			if(mode[0] == 2)
			{
				used = indices[0];
				indices[0] =  i1;
			}
			else if(mode[1] == 2)
			{
				used = indices[1];
				indices[1] = i1;
			}
			else if(mode[2] == 2)
			{
				used = indices[2];
				indices[2] = i1;
			}

			if(mode[3] == 2 && indices[3] != used)
				indices[3] = i0;
			else if(mode[4] == 2 && indices[4] != used)
				indices[4] = i0;
			else if(mode[5] == 2 && indices[5] != used)
				indices[5] = i0;

			return true;
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
		 *	Split all n-gons into triangles.
		 */
		public static void Facetize(pb_Object pb)
		{
			pb_Vertex[] v = pb_Vertex.GetVertices(pb);

			int triangleCount = pb.faces.Sum(x => x.indices.Length);

			if(triangleCount == v.Length)
			{
				Debug.LogWarning("We can't pull over any further!\npb_Object: " + pb.name + " is already triangulated.");
			}

			int vertexCount = triangleCount;
			int faceCount = vertexCount / 3;

			pb_Vertex[] tri_vertices = new pb_Vertex[triangleCount];
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
			pb.SetFaces(tri_faces);
			pb.SetSharedIndices( pb_IntArrayUtility.ExtractSharedIndices(pb.vertices) );
			pb.SetSharedIndicesUV( new pb_IntArray[0] );
		}
	}
}

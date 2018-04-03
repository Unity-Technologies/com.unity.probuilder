using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder.Core;

namespace ProBuilder.MeshOperations
{
	/// <summary>
	/// Functions for working with triangle data.
	/// </summary>
	public static class pb_TriangleOps
	{
		/// <summary>
		/// Reverse the winding order for each passed pb_Face.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces"></param>
		public static void ReverseWindingOrder(this pb_Object pb, pb_Face[] faces)
		{
			for(int i = 0; i < faces.Length; i++)
				faces[i].ReverseIndices();
		}

		/// <summary>
		/// Attempt to figure out the winding order the passed face.
		/// </summary>
		/// <remarks>May return WindingOrder.Unknown.</remarks>
		/// <param name="pb"></param>
		/// <param name="face"></param>
		/// <returns></returns>
		public static WindingOrder GetWindingOrder(this pb_Object pb, pb_Face face)
		{
			Vector2[] p = pb_Projection.PlanarProject(pb, face);
			return GetWindingOrder(p);
		}

		static WindingOrder GetWindingOrder(IList<pb_Vertex> vertices, IList<int> indices)
		{
			Vector2[] p = pb_Projection.PlanarProject(vertices, indices);
			return GetWindingOrder(p);
		}

		/// <summary>
		/// Return the winding order of a set of ordered points.
		/// </summary>
		/// <remarks>http://stackoverflow.com/questions/1165647/how-to-determine-if-a-list-of-polygon-points-are-in-clockwise-order</remarks>
		/// <param name="points">A set of unordered indices.</param>
		/// <returns>The winding order if found, WindingOrder.Unknown if not.</returns>
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

			return sum == 0f ? WindingOrder.Unknown : (sum > 0f ? WindingOrder.Clockwise : WindingOrder.CounterClockwise);
		}

		/// <summary>
		/// Reverses the orientation of the middle edge in a quad.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="face"></param>
		/// <returns></returns>
		public static bool FlipEdge(this pb_Object pb, pb_Face face)
		{
			int[] indices = face.indices;

			if(indices.Length != 6)
				return false;

			int[] mode = pb_Util.FilledArray<int>(1, indices.Length);

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

		/// <summary>
		/// Iterates through all triangles in a pb_Object and removes triangles with area <= 0 and tris with indices that point to the same vertex.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="removed"></param>
		/// <returns>True if Degenerate tris were found, false if no changes.</returns>
		public static bool RemoveDegenerateTriangles(this pb_Object pb, out int[] removed)
		{
			Dictionary<int, int> m_Lookup = pb.sharedIndices.ToDictionary();
			Dictionary<int, int> m_LookupUV = pb.sharedIndicesUV != null ? pb.sharedIndicesUV.ToDictionary() : new Dictionary<int, int>();
			Vector3[] m_Vertices = pb.vertices;
			Dictionary<int, int> m_RebuiltLookup = new Dictionary<int, int>();
			Dictionary<int, int> m_RebuiltLookupUV = new Dictionary<int, int>();
			List<pb_Face> m_RebuiltFaces = new List<pb_Face>();

			foreach(pb_Face face in pb.faces)
			{
				List<int> tris = new List<int>();

				int[] ind = face.indices;

				for(int i = 0; i < ind.Length; i+=3)
				{
					float area = pb_Math.TriangleArea(m_Vertices[ind[i+0]], m_Vertices[ind[i+1]], m_Vertices[ind[i+2]]);

					if(area > Mathf.Epsilon)
					{
						int a = m_Lookup[ind[i  ]],
							b = m_Lookup[ind[i+1]],
							c = m_Lookup[ind[i+2]];

						if( !(a == b || a == c || b == c) )
						{
							tris.Add(ind[i+0]);
							tris.Add(ind[i+1]);
							tris.Add(ind[i+2]);

							if(!m_RebuiltLookup.ContainsKey(ind[i  ]))
								m_RebuiltLookup.Add(ind[i  ], a);
							if(!m_RebuiltLookup.ContainsKey(ind[i+1]))
								m_RebuiltLookup.Add(ind[i+1], b);
							if(!m_RebuiltLookup.ContainsKey(ind[i+2]))
								m_RebuiltLookup.Add(ind[i+2], c);

							if(m_LookupUV.ContainsKey(ind[i]) && !m_RebuiltLookupUV.ContainsKey(ind[i]))
								m_RebuiltLookupUV.Add(ind[i], m_LookupUV[ind[i]]);
							if(m_LookupUV.ContainsKey(ind[i+1]) && !m_RebuiltLookupUV.ContainsKey(ind[i+1]))
								m_RebuiltLookupUV.Add(ind[i+1], m_LookupUV[ind[i+1]]);
							if(m_LookupUV.ContainsKey(ind[i+2]) && !m_RebuiltLookupUV.ContainsKey(ind[i+2]))
								m_RebuiltLookupUV.Add(ind[i+2], m_LookupUV[ind[i+2]]);
						}
					}
				}

				if(tris.Count > 0)
				{
					face.SetIndices(tris.ToArray());
					face.RebuildCaches();
					m_RebuiltFaces.Add(face);
				}
			}

			pb.SetFaces(m_RebuiltFaces.ToArray());
			pb.SetSharedIndices(m_RebuiltLookup);
			pb.SetSharedIndicesUV(m_RebuiltLookupUV);
			removed = pb.RemoveUnusedVertices();
			return removed.Length > 0;
		}

		/// <summary>
		/// Merge all faces into a single face.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces"></param>
		/// <returns></returns>
		[System.Obsolete("Please use pb_MergeFaces.Merge(pb_Object target, IEnumerable<pb_Face> faces)")]
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
	}
}

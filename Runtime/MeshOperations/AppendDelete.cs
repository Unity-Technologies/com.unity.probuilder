using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEngine.ProBuilder.MeshOperations
{
	/// <summary>
	/// Functions for appending or deleting faces from meshes.
	/// </summary>
	public static class AppendDelete
	{
		/// <summary>
		/// Append a new face to the pb_Object using sharedIndex array to set the face indices to sharedIndex groups.
		/// </summary>
		/// <param name="pb">The pb_Object mesh target.</param>
		/// <param name="positions">The new vertex positions to add.</param>
		/// <param name="colors">The new colors to add (must match positions length).</param>
		/// <param name="uvs">The new uvs to add (must match positions length).</param>
		/// <param name="face">A </param>
		/// <returns></returns>
		public static Face AppendFace(this ProBuilderMesh pb, Vector3[] positions, Color[] colors, Vector2[] uvs, Face face)
		{
			int[] shared = new int[positions.Length];
			for(int i = 0; i < positions.Length; i++)
				shared[i] = -1;
			return pb.AppendFace(positions, colors, uvs, face, shared);
		}

		/// <summary>
		/// Append a new face to the pb_Object using sharedIndex array to set the face indices to sharedIndex groups.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="v"></param>
		/// <param name="c"></param>
		/// <param name="u"></param>
		/// <param name="face"></param>
		/// <param name="sharedIndex"></param>
		/// <returns></returns>
		public static Face AppendFace(this ProBuilderMesh pb, Vector3[] v, Color[] c, Vector2[] u, Face face, int[] sharedIndex)
		{
			int vertexCount = pb.vertexCount;

			Vector3[] positions = new Vector3[vertexCount + v.Length];
			Color[] colors = new Color[vertexCount + c.Length];
			Vector2[] uv0 = new Vector2[pb.texturesInternal.Length + u.Length];

			List<Face> faces = new List<Face>(pb.facesInternal);
			IntArray[] sharedIndices = pb.sharedIndicesInternal;

			// copy new vertices
			System.Array.Copy(pb.positionsInternal, 0, positions, 0, vertexCount);
			System.Array.Copy(v, 0, positions, vertexCount, v.Length);

			// copy new colors
			System.Array.Copy(pb.colorsInternal, 0, colors, 0, vertexCount);
			System.Array.Copy(c, 0, colors, vertexCount, c.Length);

			// copy new uvs
			System.Array.Copy(pb.texturesInternal, 0, uv0, 0, pb.texturesInternal.Length);
			System.Array.Copy(u, 0, uv0, pb.texturesInternal.Length, u.Length);

			face.ShiftIndicesToZero();
			face.ShiftIndices(vertexCount);

			faces.Add(face);

			for(int i = 0; i < sharedIndex.Length; i++)
				IntArrayUtility.AddValueAtIndex(ref sharedIndices, sharedIndex[i], i+vertexCount);

			pb.SetPositions(positions);
			pb.SetColors(colors);
			pb.SetUVs(uv0);

			pb.SetSharedIndices(sharedIndices);
			pb.SetFaces(faces.ToArray());

			return face;
		}

		/// <summary>
		/// Append a group of new faces to the pb_Object. Significantly faster than calling AppendFace multiple times.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="appendedVertices"></param>
		/// <param name="appendedColors"></param>
		/// <param name="appendedUvs"></param>
		/// <param name="appendedFaces"></param>
		/// <param name="appendedSharedIndices"></param>
		/// <returns></returns>
		public static Face[] AppendFaces(this ProBuilderMesh pb, Vector3[][] appendedVertices, Color[][] appendedColors, Vector2[][] appendedUvs, Face[] appendedFaces, int[][] appendedSharedIndices)
		{
			List<Vector3> vertices = new List<Vector3>(pb.positionsInternal);
			List<Color> colors = new List<Color>(pb.colorsInternal);
			List<Vector2> uvs = new List<Vector2>(pb.texturesInternal);

			List<Face> faces = new List<Face>(pb.facesInternal);
			IntArray[] sharedIndices = pb.sharedIndicesInternal;

			int vc = pb.vertexCount;

			for(int i = 0; i < appendedFaces.Length; i++)
			{
				vertices.AddRange(appendedVertices[i]);
				colors.AddRange(appendedColors[i]);
				uvs.AddRange(appendedUvs[i]);

				appendedFaces[i].ShiftIndicesToZero();
				appendedFaces[i].ShiftIndices(vc);
				faces.Add(appendedFaces[i]);

				if(appendedSharedIndices != null && appendedVertices[i].Length != appendedSharedIndices[i].Length)
				{
					Debug.LogError("Append Face failed because sharedIndex array does not match new vertex array.");
					return null;
				}

				if(appendedSharedIndices != null)
				{
					for(int j = 0; j < appendedSharedIndices[i].Length; j++)
					{
						IntArrayUtility.AddValueAtIndex(ref sharedIndices, appendedSharedIndices[i][j], j+vc);
					}
				}
				else
				{
					for(int j = 0; j < appendedVertices[i].Length; j++)
					{
						IntArrayUtility.AddValueAtIndex(ref sharedIndices, -1, j+vc);
					}
				}

				vc = vertices.Count;
			}

			pb.SetPositions(vertices.ToArray());
			pb.SetColors(colors.ToArray());
			pb.SetUVs(uvs.ToArray());
			pb.SetFaces(faces.ToArray());
			pb.sharedIndicesInternal = sharedIndices;

			return appendedFaces;
		}

		/// <summary>
		/// Duplicate and reverse the winding direction for each face.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces"></param>
		public static void DuplicateAndFlip(this ProBuilderMesh pb, Face[] faces)
		{
			List<FaceRebuildData> rebuild = new List<FaceRebuildData>();
			List<Vertex> vertices = new List<Vertex>(Vertex.GetVertices(pb));
			Dictionary<int, int> lookup = pb.sharedIndicesInternal.ToDictionary();

			foreach(Face face in faces)
			{
				FaceRebuildData data = new FaceRebuildData();

				data.vertices = new List<Vertex>();
				data.face = new Face(face);
				data.sharedIndices = new List<int>();

				Dictionary<int, int> map = new Dictionary<int, int>();
				int len = data.face.indices.Length;

				for(int i = 0; i < len; i++)
				{
					if(map.ContainsKey(face.indices[i]))
						continue;

					map.Add(face.indices[i], map.Count);
					data.vertices.Add(vertices[face.indices[i]]);
					data.sharedIndices.Add(lookup[face.indices[i]]);
				}

				for(int i = 0; i < len; i++)
					data.face.indices[i] = map[data.face.indices[i]];

				data.face.InvalidateCache();
				rebuild.Add(data);
			}

			FaceRebuildData.Apply(rebuild, pb, vertices, null, lookup, null);
		}

		/// <summary>
		/// Removes the passed face from this pb_Object.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="face"></param>
		/// <returns></returns>
		public static int[] DeleteFace(this ProBuilderMesh pb, Face face)
		{
			return DeleteFaces(pb, new Face[] { face });
		}

		/// <summary>
		/// Remove a set of faces from a pb_Object.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faces"></param>
		/// <returns></returns>
		public static int[] DeleteFaces(this ProBuilderMesh pb, IEnumerable<Face> faces)
		{
			return DeleteFaces(pb, faces.Select(x => System.Array.IndexOf(pb.facesInternal, x)).ToList());
		}

		/// <summary>
		/// Remove faces from an object by their index in the pb_Object.faces array.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="faceIndices"></param>
		/// <returns></returns>
		public static int[] DeleteFaces(this ProBuilderMesh pb, IList<int> faceIndices)
		{
			Face[] faces = new Face[faceIndices.Count];

			for(int i = 0; i < faces.Length; i++)
				faces[i] = pb.facesInternal[faceIndices[i]];

			List<int> indicesToRemove = faces.SelectMany(x => x.distinctIndices).Distinct().ToList();
			indicesToRemove.Sort();

			int vertexCount = pb.positionsInternal.Length;

			Vector3[] verts = pb.positionsInternal.SortedRemoveAt(indicesToRemove);
			Color[] cols = pb.colorsInternal.SortedRemoveAt(indicesToRemove);
			Vector2[] uvs = pb.texturesInternal.SortedRemoveAt(indicesToRemove);
			Face[] nFaces = pb.facesInternal.RemoveAt(faceIndices);

			Dictionary<int, int> shiftmap = new Dictionary<int, int>();

			for(var i = 0;  i < vertexCount; i++)
				shiftmap.Add(i, InternalUtility.NearestIndexPriorToValue<int>(indicesToRemove, i) + 1);

			// shift all other face indices down to account for moved vertex positions
			for(var i = 0; i < nFaces.Length; i++)
			{
				int[] tris = nFaces[i].indices;

				for(var n = 0; n < tris.Length; n++)
					tris[n] -= shiftmap[tris[n]];

				nFaces[i].indices = tris;
			}

			// shift all other face indices in the shared index array down to account for moved vertex positions
			IntArray[] si = pb.sharedIndicesInternal;
			IntArray[] si_uv = pb.sharedIndicesUVInternal;

			IntArrayUtility.RemoveValuesAndShift(ref si, indicesToRemove);
			if(si_uv != null) IntArrayUtility.RemoveValuesAndShift(ref si_uv, indicesToRemove);

			pb.sharedIndicesInternal = si;
			pb.sharedIndicesUVInternal = si_uv;
			pb.positionsInternal = verts;
			pb.colorsInternal = cols;
			pb.texturesInternal = uvs;
			pb.facesInternal = nFaces;

			int[] array = indicesToRemove.ToArray();

			return array;
		}

		/// <summary>
		/// Iterates through all triangles in a pb_Object and removes triangles with area <= 0 and tris with indices that point to the same vertex.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="removed"></param>
		/// <returns>True if Degenerate tris were found, false if no changes.</returns>
		public static bool RemoveDegenerateTriangles(this ProBuilderMesh pb, out int[] removed)
		{
			Dictionary<int, int> m_Lookup = pb.sharedIndicesInternal.ToDictionary();
			Dictionary<int, int> m_LookupUV = pb.sharedIndicesUVInternal != null ? pb.sharedIndicesUVInternal.ToDictionary() : new Dictionary<int, int>();
			Vector3[] m_Vertices = pb.positionsInternal;
			Dictionary<int, int> m_RebuiltLookup = new Dictionary<int, int>();
			Dictionary<int, int> m_RebuiltLookupUV = new Dictionary<int, int>();
			List<Face> m_RebuiltFaces = new List<Face>();

			foreach(Face face in pb.facesInternal)
			{
				List<int> tris = new List<int>();

				int[] ind = face.indices;

				for(int i = 0; i < ind.Length; i+=3)
				{
					float area = ProBuilderMath.TriangleArea(m_Vertices[ind[i+0]], m_Vertices[ind[i+1]], m_Vertices[ind[i+2]]);

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
					face.indices = tris.ToArray();
					m_RebuiltFaces.Add(face);
				}
			}

			pb.SetFaces(m_RebuiltFaces.ToArray());
			pb.SetSharedIndices(m_RebuiltLookup);
			pb.SetSharedIndicesUV(m_RebuiltLookupUV);
			removed = pb.RemoveUnusedVertices();
			return removed.Length > 0;
		}

	}
}

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using System;

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
		/// <param name="face">A face with the new face triangle indexes. The indices should be 0 indexed.</param>
		/// <returns></returns>
		public static Face AppendFace(this ProBuilderMesh pb, Vector3[] positions, Color[] colors, Vector2[] uvs, Face face)
		{
            if (positions == null)
                throw new ArgumentNullException("positions");
			int[] shared = new int[positions.Length];
			for(int i = 0; i < positions.Length; i++)
				shared[i] = -1;
			return pb.AppendFace(positions, colors, uvs, face, shared);
		}

		/// <summary>
		/// Append a new face to the pb_Object using sharedIndex array to set the face indices to sharedIndex groups.
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="positions"></param>
		/// <param name="colors"></param>
		/// <param name="uvs"></param>
		/// <param name="face">A face with the new face triangle indexes. The indices should be 0 indexed.</param>
		/// <param name="sharedIndex"></param>
		/// <returns></returns>
		public static Face AppendFace(this ProBuilderMesh mesh, Vector3[] positions, Color[] colors, Vector2[] uvs, Face face, int[] sharedIndexes)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (positions == null)
                throw new ArgumentNullException("positions");

            if (colors == null)
                throw new ArgumentNullException("colors");

            if (uvs == null)
                throw new ArgumentNullException("uvs");

            if (face == null)
                throw new ArgumentNullException("face");

            if (sharedIndexes == null)
                throw new ArgumentNullException("sharedIndexes");

			int vertexCount = mesh.vertexCount;

			Vector3[] newPositions = new Vector3[vertexCount + positions.Length];
			Color[] newColors = new Color[vertexCount + colors.Length];
			Vector2[] newTextures = new Vector2[mesh.texturesInternal.Length + uvs.Length];

			List<Face> faces = new List<Face>(mesh.facesInternal);
			IntArray[] sharedIndices = mesh.sharedIndicesInternal;

			Array.Copy(mesh.positionsInternal, 0, newPositions, 0, vertexCount);
			Array.Copy(positions, 0, newPositions, vertexCount, positions.Length);
			Array.Copy(mesh.colorsInternal, 0, newColors, 0, vertexCount);
			Array.Copy(colors, 0, newColors, vertexCount, colors.Length);
			Array.Copy(mesh.texturesInternal, 0, newTextures, 0, mesh.texturesInternal.Length);
			Array.Copy(uvs, 0, newTextures, mesh.texturesInternal.Length, uvs.Length);

			face.ShiftIndexesToZero();
			face.ShiftIndexes(vertexCount);

			faces.Add(face);

			for(int i = 0; i < sharedIndexes.Length; i++)
				IntArrayUtility.AddValueAtIndex(ref sharedIndices, sharedIndexes[i], i+vertexCount);

			mesh.SetPositions(newPositions);
			mesh.SetColors(newColors);
			mesh.SetUVs(newTextures);
			mesh.SetSharedIndexes(sharedIndices);
			mesh.SetFaces(faces.ToArray());

			return face;
		}

		/// <summary>
		/// Append a group of new faces to the mesh. Significantly faster than calling AppendFace multiple times.
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="appendedVertices"></param>
		/// <param name="appendedColors"></param>
		/// <param name="appendedUvs"></param>
		/// <param name="appendedFaces"></param>
		/// <param name="appendedSharedIndexes"></param>
		/// <returns></returns>
		public static Face[] AppendFaces(this ProBuilderMesh mesh, Vector3[][] appendedVertices, Color[][] appendedColors, Vector2[][] appendedUvs, Face[] appendedFaces, int[][] appendedSharedIndexes)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (appendedVertices == null)
                throw new ArgumentNullException("appendedVertices");

            if (appendedColors == null)
                throw new ArgumentNullException("appendedColors");

            if (appendedUvs == null)
                throw new ArgumentNullException("appendedUvs");

            if (appendedFaces == null)
                throw new ArgumentNullException("appendedFaces");

            List<Vector3> vertices = new List<Vector3>(mesh.positionsInternal);
			List<Color> colors = new List<Color>(mesh.colorsInternal);
			List<Vector2> uvs = new List<Vector2>(mesh.texturesInternal);

			List<Face> faces = new List<Face>(mesh.facesInternal);
			IntArray[] sharedIndices = mesh.sharedIndicesInternal;

			int vc = mesh.vertexCount;

			for(int i = 0; i < appendedFaces.Length; i++)
			{
				vertices.AddRange(appendedVertices[i]);
				colors.AddRange(appendedColors[i]);
				uvs.AddRange(appendedUvs[i]);

				appendedFaces[i].ShiftIndexesToZero();
				appendedFaces[i].ShiftIndexes(vc);
				faces.Add(appendedFaces[i]);

				if(appendedSharedIndexes != null && appendedVertices[i].Length != appendedSharedIndexes[i].Length)
				{
					Debug.LogError("Append Face failed because sharedIndex array does not match new vertex array.");
					return null;
				}

				if(appendedSharedIndexes != null)
				{
					for(int j = 0; j < appendedSharedIndexes[i].Length; j++)
					{
						IntArrayUtility.AddValueAtIndex(ref sharedIndices, appendedSharedIndexes[i][j], j+vc);
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

			mesh.SetPositions(vertices.ToArray());
			mesh.SetColors(colors.ToArray());
			mesh.SetUVs(uvs.ToArray());
			mesh.SetFaces(faces.ToArray());
			mesh.sharedIndicesInternal = sharedIndices;

			return appendedFaces;
		}

		/// <summary>
		/// Duplicate and reverse the winding direction for each face.
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="faces"></param>
		public static void DuplicateAndFlip(this ProBuilderMesh mesh, Face[] faces)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (faces == null)
                throw new ArgumentNullException("faces");

			List<FaceRebuildData> rebuild = new List<FaceRebuildData>();
			List<Vertex> vertices = new List<Vertex>(Vertex.GetVertices(mesh));
			Dictionary<int, int> lookup = mesh.sharedIndicesInternal.ToDictionary();

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

			FaceRebuildData.Apply(rebuild, mesh, vertices, null, lookup, null);
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
		/// <param name="mesh"></param>
		/// <param name="faceIndexes"></param>
		/// <returns></returns>
		public static int[] DeleteFaces(this ProBuilderMesh mesh, IList<int> faceIndexes)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            if (faceIndexes == null)
                throw new ArgumentNullException("faceIndexes");

			Face[] faces = new Face[faceIndexes.Count];

			for(int i = 0; i < faces.Length; i++)
				faces[i] = mesh.facesInternal[faceIndexes[i]];

			List<int> indicesToRemove = faces.SelectMany(x => x.distinctIndices).Distinct().ToList();
			indicesToRemove.Sort();

			int vertexCount = mesh.positionsInternal.Length;

			Vector3[] verts = mesh.positionsInternal.SortedRemoveAt(indicesToRemove);
			Color[] cols = mesh.colorsInternal.SortedRemoveAt(indicesToRemove);
			Vector2[] uvs = mesh.texturesInternal.SortedRemoveAt(indicesToRemove);
			Face[] nFaces = mesh.facesInternal.RemoveAt(faceIndexes);

			Dictionary<int, int> shiftmap = new Dictionary<int, int>();

			for(var i = 0;  i < vertexCount; i++)
				shiftmap.Add(i, ArrayUtility.NearestIndexPriorToValue<int>(indicesToRemove, i) + 1);

			// shift all other face indices down to account for moved vertex positions
			for(var i = 0; i < nFaces.Length; i++)
			{
				int[] tris = nFaces[i].indices;

				for(var n = 0; n < tris.Length; n++)
					tris[n] -= shiftmap[tris[n]];

				nFaces[i].indices = tris;
			}

			// shift all other face indices in the shared index array down to account for moved vertex positions
			IntArray[] si = mesh.sharedIndicesInternal;
			IntArray[] si_uv = mesh.sharedIndicesUVInternal;

			IntArrayUtility.RemoveValuesAndShift(ref si, indicesToRemove);
			if(si_uv != null) IntArrayUtility.RemoveValuesAndShift(ref si_uv, indicesToRemove);

			mesh.sharedIndicesInternal = si;
			mesh.sharedIndicesUVInternal = si_uv;
			mesh.positionsInternal = verts;
			mesh.colorsInternal = cols;
			mesh.texturesInternal = uvs;
			mesh.facesInternal = nFaces;

			int[] array = indicesToRemove.ToArray();

			return array;
		}

		/// <summary>
		/// Iterates through all triangles in a mesh and removes triangles with an area less than float.Epsilon, and tris with indices that point to the same vertex.
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="removed"></param>
		/// <returns>The number of vertices deleted as a result of the degenerate triangle cleanup.</returns>
		public static int[] RemoveDegenerateTriangles(this ProBuilderMesh mesh)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

			Dictionary<int, int> m_Lookup = mesh.sharedIndicesInternal.ToDictionary();
			Dictionary<int, int> m_LookupUV = mesh.sharedIndicesUVInternal != null ? mesh.sharedIndicesUVInternal.ToDictionary() : new Dictionary<int, int>();
			Vector3[] m_Vertices = mesh.positionsInternal;
			Dictionary<int, int> m_RebuiltLookup = new Dictionary<int, int>();
			Dictionary<int, int> m_RebuiltLookupUV = new Dictionary<int, int>();
			List<Face> m_RebuiltFaces = new List<Face>();

			foreach(Face face in mesh.facesInternal)
			{
				List<int> tris = new List<int>();

				int[] ind = face.indices;

				for(int i = 0; i < ind.Length; i+=3)
				{
					float area = Math.TriangleArea(m_Vertices[ind[i+0]], m_Vertices[ind[i+1]], m_Vertices[ind[i+2]]);

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

			mesh.SetFaces(m_RebuiltFaces.ToArray());
			mesh.SetSharedIndexes(m_RebuiltLookup);
			mesh.SetSharedIndexesUV(m_RebuiltLookupUV);
			return mesh.RemoveUnusedVertices();
		}

	}
}

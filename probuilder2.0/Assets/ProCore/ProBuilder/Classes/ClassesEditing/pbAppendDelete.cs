using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;

#if PB_DEBUG
using Parabox.Debug;
#endif

namespace ProBuilder2.MeshOperations
{
public static class pbAppendDelete
{

#region Append Face

/**
	 *	\brief
	 *	param sharedIndex An optional array that sets the new pb_Face indices to use the _sharedIndices array.
	 *	\returns The newly appended pb_Face.
	 */
	public static pb_Face AppendFace(this pb_Object pb, Vector3[] v, Color[] c, IList<Vector4> u, pb_Face face)
	{
		int[] shared = new int[v.Length];
		for(int i = 0; i < v.Length; i++)
			shared[i] = -1;
		return pb.AppendFace(v, c, u, face, shared);
	}
	
	/**
	 * Append a new face to the pb_Object using sharedIndex array to set the face indices to sharedIndex groups.
	 */
	public static pb_Face AppendFace(this pb_Object pb, Vector3[] v, Color[] c, IList<Vector4> u, pb_Face face, int[] sharedIndex)
	{
		int vertexCount = pb.vertexCount;

		Vector3[] _verts = new Vector3[vertexCount + v.Length];
		Color[] _colors = new Color[vertexCount + c.Length];
		List<Vector4> _uvs = pb.GetUVs(0);

		List<pb_Face> _faces = new List<pb_Face>(pb.faces);
		pb_IntArray[] sharedIndices = pb.sharedIndices;

		// copy new vertices
		System.Array.Copy(pb.vertices, 0, _verts, 0, vertexCount);
		System.Array.Copy(v, 0, _verts, vertexCount, v.Length);

		// copy new colors
		System.Array.Copy(pb.colors, 0, _colors, 0, vertexCount);
		System.Array.Copy(c, 0, _colors, vertexCount, c.Length);

		// copy new uvs
		_uvs.AddRange(u);

		face.ShiftIndicesToZero();
		face.ShiftIndices(vertexCount);
		face.RebuildCaches();

		_faces.Add(face);

		for(int i = 0; i < sharedIndex.Length; i++)
			pb_IntArrayUtility.AddValueAtIndex(ref sharedIndices, sharedIndex[i], i+vertexCount);

		pb.SetVertices( _verts );
		pb.SetColors( _colors );
		pb.SetUVs(0, _uvs);
		
		pb.SetSharedIndices(sharedIndices);
		pb.SetFaces(_faces.ToArray());

		return face;
	}

	/**
	 * Append a group of new faces to the pb_Object.  Significantly faster than calling AppendFace multiple times.
	 */
	public static pb_Face[] AppendFaces(this pb_Object pb, Vector3[][] new_Vertices, Color[][] new_Colors, IList<IList<Vector4>> new_uvs, pb_Face[] new_Faces, int[][] new_SharedIndices)
	{
		List<Vector3> _verts = new List<Vector3>(pb.vertices);
		List<Color> _colors = new List<Color>(pb.colors);
		List<Vector4> _uv = pb.GetUVs(0);

		List<pb_Face> _faces = new List<pb_Face>(pb.faces);
		pb_IntArray[] sharedIndices = pb.sharedIndices;

		int vc = pb.vertexCount;

		for(int i = 0; i < new_Faces.Length; i++)
		{
			_verts.AddRange(new_Vertices[i]);
			_colors.AddRange(new_Colors[i]);
			_uv.AddRange(new_uvs[i]);

			new_Faces[i].ShiftIndicesToZero();
			new_Faces[i].ShiftIndices(vc);
			new_Faces[i].RebuildCaches();
			_faces.Add(new_Faces[i]);

			if(new_SharedIndices != null && new_Vertices[i].Length != new_SharedIndices[i].Length)
			{
				Debug.LogError("Append Face failed because sharedIndex array does not match new vertex array.");
				return null;
			}

			if(new_SharedIndices != null)
			{
				for(int j = 0; j < new_SharedIndices[i].Length; j++)
				{
					pb_IntArrayUtility.AddValueAtIndex(ref sharedIndices, new_SharedIndices[i][j], j+vc);
				}
			}
			else
			{
				for(int j = 0; j < new_Vertices[i].Length; j++)
				{
					pb_IntArrayUtility.AddValueAtIndex(ref sharedIndices, -1, j+vc);
				}
			}

			vc = _verts.Count;
		}

		pb.SetSharedIndices(sharedIndices);

		pb.SetVertices(_verts.ToArray());
		pb.SetColors(_colors.ToArray());
		pb.SetUVs(0, _uv);
		pb.SetFaces(_faces.ToArray());

		return new_Faces;
	}
#endregion

#region Delete Face

	/**
	 * Removes faces from a pb_Object.  Overrides available for pb_Face[] and int[] faceIndices.  handles
	 * all the sharedIndices moving stuff for you.
	 */
	public static void DeleteFaces(this pb_Object pb, pb_Face[] faces)
	{	
		int[] f_ind = new int[faces.Length];

		// test for triangle array equality, not reference equality
		for(int i = 0; i < faces.Length; i++)
			f_ind[i] = System.Array.IndexOf(pb.faces, faces[i]);
		
		List<int> indices_to_remove = new List<int>( pb_Face.AllTrianglesDistinct(faces) );
		indices_to_remove.Sort();

		Vector3[] verts = pb.vertices.SortedRemoveAt(indices_to_remove);
		Color[] cols = pb.colors.SortedRemoveAt(indices_to_remove);

		pb_Face[] nFaces = pb.faces.RemoveAt(f_ind);

		// shift all other face indices down to account for moved vertex positions
		for(int i = 0; i < nFaces.Length; i++)
		{
			int[] tris = nFaces[i].indices;

			for(int n = 0; n < tris.Length; n++)
			{
				int index = pbUtil.NearestIndexPriorToValue(indices_to_remove, tris[n]);
				// add 1 because index is zero based
				tris[n] -= index + 1;
			}

			nFaces[i].SetIndices(tris);
		}

		// shift all other face indices in the shared index array down to account for moved vertex positions
		pb_IntArray[] si = pb.sharedIndices;
		pb_IntArray[] si_uv = pb.sharedIndicesUV;

		pb_IntArrayUtility.RemoveValuesAndShift(ref si, indices_to_remove);
		pb_IntArrayUtility.RemoveValuesAndShift(ref si_uv, indices_to_remove);
		
		pb.SetSharedIndices(si);
		pb.SetSharedIndicesUV(si_uv);
		
		pb.SetVertices(verts);
		pb.SetColors(cols);
		pb.SetUVs(0, pb.uv0.SortedRemoveAt(indices_to_remove));
#if UNITY_5_3
		if(pb.uv3 != null) pb.SetUVs(3, new List<Vector4>(pb.uv3.SortedRemoveAt(indices_to_remove)));
		if(pb.uv4 != null) pb.SetUVs(4, new List<Vector4>(pb.uv4.SortedRemoveAt(indices_to_remove)));
#endif
		pb.SetFaces(nFaces);
		pb.RebuildFaceCaches();
	}

	/**
	 * Removes faces from a pb_Object.  Overrides available for pb_Face[] and int[] faceIndices.  handles
	 * all the sharedIndices moving stuff for you.
	 */
	public static void DeleteFaces(this pb_Object pb, int[] faceIndices)
	{	
		pb_Face[] faces = new pb_Face[faceIndices.Length];

		for(int i = 0; i < faces.Length; i++)
			faces[i] = pb.faces[faceIndices[i]];

		DeleteFaces(pb, faces);	
	}

	/**
	 *	Removes the passed face from this pb_Object.  Handles shifting vertices and triangles, as well as messing with the sharedIndices cache.
	 */
	public static void DeleteFace(this pb_Object pb, pb_Face face)
	{		
		DeleteFaces(pb, new pb_Face[1] { face });
	}
#endregion
}
}

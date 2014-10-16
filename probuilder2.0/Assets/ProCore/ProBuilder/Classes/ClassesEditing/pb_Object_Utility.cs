using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ProCore.Common;

#if PB_DEBUG
using Parabox.Debug;
#endif

namespace ProBuilder2.Common {
public static class pb_Object_Utility
{

#region Coordinate Translation

	/**
	 *	\brief Gets vertices in world space
	 *	\returns A Vector3[] arry containing all vertex points in world space.
	 */
	public static Vector3[] VerticesInWorldSpace(this pb_Object pb)
	{
		Vector3[] worldPoints = new Vector3[pb.vertices.Length];

		System.Array.Copy(pb.vertices, worldPoints, worldPoints.Length);

		for(int i = 0; i < worldPoints.Length; i++)
			worldPoints[i] = pb.transform.TransformPoint(worldPoints[i]);

		return worldPoints;
	}

	/**
	 * Returns requested vertices in world space coordinates.
	 */
	public static Vector3[] VerticesInWorldSpace(this pb_Object pb, int[] indices)
	{
		if(indices == null)
			Debug.LogWarning("indices == null -> VerticesInWorldSpace");

		Vector3[] worldPoints = pb.GetVertices(indices);

		for(int i = 0; i < worldPoints.Length; i++)
			worldPoints[i] = pb.transform.TransformPoint(worldPoints[i]);

		return worldPoints;
	}
#endregion

#region Mesh Modify

	/**
	 *	\brief Use this for moving vertices.  Arguments are selected indices (distinct), and the offset to apply.
	 *	@param selectedTriangles Triangles to apply the offset to.
	 *	@param offset Offset in meters to apply.  Note that offset is in world space coordinates for this overload.
	 *	\notes This method also applies a snap value if one is set.  Snaps vertices in world space, not locally.
	 */
	public static void TranslateVertices_World(this pb_Object pb, int[] selectedTriangles, Vector3 offset)
	{
		pb.TranslateVertices_World(selectedTriangles, offset, false);
	}

	public static void TranslateVertices_World(this pb_Object pb, int[] selectedTriangles, Vector3 offset, bool forceDisableSnap)
	{	
		Vector3 orig = offset;
		
		int i = 0;
		int[] indices = pb.sharedIndices.AllIndicesWithValues(selectedTriangles);

		offset = pb.transform.worldToLocalMatrix * offset;

		Vector3[] verts = pb.vertices;
		for(i = 0; i < indices.Length; i++)
			verts[indices[i]] += offset;
		
		// Snaps to world grid
		if(pbUtil.SharedSnapEnabled && !forceDisableSnap)
		{
			float snapValue = pbUtil.SharedSnapValue;
			for(i = 0; i < indices.Length; i++)
			{
				if(pbUtil.SharedUseAxisConstraints)
					verts[indices[i]] = pb.transform.InverseTransformPoint(pbUtil.SnapValue(pb.transform.TransformPoint(verts[indices[i]]), orig /*mask*/, snapValue));
				else
					verts[indices[i]] = pb.transform.InverseTransformPoint(pbUtil.SnapValue(pb.transform.TransformPoint(verts[indices[i]]), Vector3.one, snapValue));
			}
		}

		// don't bother calling a full ToMesh() here because we know for certain that the _vertices and msh.vertices arrays are equal in length
		pb.SetVertices(verts);
		pb.msh.vertices = verts;
	}

	/**
	 *	\brief Use this for moving vertices.  Arguments are selected indices (distinct), and the offset to apply (in local space).
	 *	@param selectedTriangles Triangles to apply the offset to.
	 *	@param offset Offset in meters to apply (Vector3 : direction * float : distance).
	 */
	public static void TranslateVertices(this pb_Object pb, int[] selectedTriangles, Vector3 offset)
	{	
		int i = 0;
		int[] indices = pb.sharedIndices.AllIndicesWithValues(selectedTriangles);

		Vector3[] verts = pb.vertices;
		for(i = 0; i < indices.Length; i++)
			verts[indices[i]] += offset;

		// don't bother calling a full ToMesh() here because we know for certain that the _vertices and msh.vertices arrays are equal in length
		pb.SetVertices(verts);
		pb.msh.vertices = verts;
	}

	/**
	 *	\brief Given a triangle index, locate buddy indices and move all vertices to this new position
	 */
	public static void SetSharedVertexPosition(this pb_Object pb, int index, Vector3 position) { pb.SetSharedVertexPosition(index, position, false); }

	public static void SetSharedVertexPosition(this pb_Object pb, int index, Vector3 position, bool snap)
	{
		int sharedIndicesIndex = pb.sharedIndices.IndexOf(index);
		Vector3[] v = pb.vertices;

		foreach(int n in pb.sharedIndices[sharedIndicesIndex].array)
		{
			v[n] = position;
		}	

		pb.SetVertices(v);
		pb.msh.vertices = v;
	}
#endregion

#region Seek

	/**
	 * \brief Returns a #pb_Face which contains the passed triangle. 
	 * @param tri int[] composed of three indices.
	 */
	public static bool FaceWithTriangle(this pb_Object pb, int[] tri, out pb_Face face)
	{
		for(int i = 0; i < pb.faces.Length; i++)
		{
			if(	pb.faces[i].Contains(tri) )
			{
				face = pb.faces[i];
				return true;
			}
		}

		face = null;
		return false;
	}

	/**
	 * \brief Returns the index of the #pb_Face which contains the passed triangle. 
	 * @param tri int[] composed of three indices.
	 */
	public static bool FaceWithTriangle(this pb_Object pb, int[] tri, out int face)
	{
		for(int i = 0; i < pb.faces.Length; i++)
		{
			if(	pb.faces[i].Contains(tri) )
			{
				face = i;
				return true;
			}
		}

		face = -1;
		return false;
	}
#endregion
}
}
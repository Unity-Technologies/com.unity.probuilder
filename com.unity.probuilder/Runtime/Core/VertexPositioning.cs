using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// A set of commonly used functions for modifying mesh positions.
	/// </summary>
	public static class VertexPositioning
	{
		/// <summary>
		/// Get a copy of a mesh positions array transformed into world coordinates.
		/// </summary>
		/// <param name="mesh">The source mesh.</param>
		/// <returns>An array containing all vertex positions in world space.</returns>
		public static Vector3[] VerticesInWorldSpace(this ProBuilderMesh mesh)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

			int len = mesh.vertexCount;
			Vector3[] worldPoints = new Vector3[len];
			Vector3[] localPoints = mesh.positionsInternal;

			for(int i = 0; i < len; i++)
				worldPoints[i] = mesh.transform.TransformPoint(localPoints[i]);

			return worldPoints;
		}

		/// <summary>
		/// Translate a set of vertices with a world space offset.
		/// <br />
		/// Unlike most other mesh operations, this function applies the mesh positions to both ProBuilderMesh and the UnityEngine.Mesh.
		/// </summary>
		/// <param name="mesh">The mesh to be affected.</param>
		/// <param name="indexes">A set of triangles pointing to the vertex positions that are to be affected.</param>
		/// <param name="offset">The offset to apply in world coordinates.</param>
		public static void TranslateVerticesInWorldSpace(this ProBuilderMesh mesh, int[] indexes, Vector3 offset)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            mesh.TranslateVerticesInWorldSpace(indexes, offset, 0f, false);
		}

		/// <summary>
		/// Translate a set of vertices with a world space offset.
		/// <br />
		/// Unlike most other mesh operations, this function applies the mesh positions to both ProBuilderMesh and the UnityEngine.Mesh.
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="indexes">A distinct list of vertex indexes.</param>
		/// <param name="offset">The direction and magnitude to translate selectedTriangles, in world space.</param>
		/// <param name="snapValue">If > 0 snap each vertex to the nearest on-grid point in world space.</param>
		/// <param name="snapAxisOnly">If true vertices will only be snapped along the active axis.</param>
		internal static void TranslateVerticesInWorldSpace(this ProBuilderMesh mesh,
			int[] indexes,
			Vector3 offset,
			float snapValue,
			bool snapAxisOnly)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            int i = 0;

			List<int> distinct = mesh.GetCoincidentVertices(indexes);

			Matrix4x4 w2l = mesh.transform.worldToLocalMatrix;

			Vector3 localOffset = w2l * offset;

			Vector3[] verts = mesh.positionsInternal;

			// Snaps to world grid
			if(Mathf.Abs(snapValue) > Mathf.Epsilon)
			{
				Matrix4x4 l2w = mesh.transform.localToWorldMatrix;
				Vector3 mask = snapAxisOnly ? offset.ToMask(Math.handleEpsilon) : Vector3.one;

				for(i = 0; i < distinct.Count; i++)
				{
					var v = l2w.MultiplyPoint3x4(verts[distinct[i]] + localOffset);
					verts[distinct[i]] = w2l.MultiplyPoint3x4( Snapping.SnapValue(v, snapValue * mask) );
				}
			}
			else
			{
				for(i = 0; i < distinct.Count; i++)
					verts[distinct[i]] += localOffset;
			}

			// don't bother calling a full ToMesh() here because we know for certain that the vertices and msh.vertices arrays are equal in length
			mesh.positions = verts;
			mesh.mesh.vertices = verts;
		}

		/// <summary>
		/// Translate a set of vertices with an offset provided in local (model) coordinates.
		/// <br />
		/// Unlike most other mesh operations, this function applies the mesh positions to both ProBuilderMesh and the UnityEngine.Mesh.
		/// </summary>
		/// <param name="mesh">The mesh to be affected.</param>
		/// <param name="indexes">A set of triangles pointing to the vertex positions that are to be affected.</param>
		/// <param name="offset"></param>
		public static void TranslateVertices(this ProBuilderMesh mesh, IEnumerable<int> indexes, Vector3 offset)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

			List<int> all = mesh.GetCoincidentVertices(indexes);
			Vector3[] verts = mesh.positionsInternal;

			for(int i = 0, c = all.Count; i < c; i++)
				verts[all[i]] += offset;

			// don't bother calling a full ToMesh() here because we know for certain that the vertices and msh.vertices arrays are equal in length
			mesh.mesh.vertices = verts;
		}

		/// <summary>
		/// Given a shared vertex index (index of the triangle in the sharedIndexes array), move all vertices to new position.
		/// Position is in model space coordinates.
		/// <br /><br />
		/// Use @"UnityEngine.ProBuilder.ProBuilderMesh.sharedIndexes" and IntArrayUtility.IndexOf to get a shared (or common) index.
		/// </summary>
		/// <param name="mesh">The target mesh.</param>
		/// <param name="sharedVertexHandle">The shared (or common) index to set the vertex position of.</param>
		/// <param name="position">The new position in model coordinates.</param>
		public static void SetSharedVertexPosition(this ProBuilderMesh mesh, int sharedVertexHandle, Vector3 position)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            Vector3[] v = mesh.positionsInternal;

			foreach(var index in mesh.sharedVerticesInternal[sharedVertexHandle])
				v[index] = position;

			mesh.positions = v;
			mesh.mesh.vertices = v;
		}

		/// <summary>
		/// Set a collection of mesh attributes with a Vertex.
		/// <br /><br />
		/// Use @"UnityEngine.ProBuilder.ProBuilderMesh.sharedIndexes" and IntArrayUtility.IndexOf to get a shared (or common) index.
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="sharedVertexHandle"></param>
		/// <param name="vertex"></param>
		internal static void SetSharedVertexValues(this ProBuilderMesh mesh, int sharedVertexHandle, Vertex vertex)
		{
			Vertex[] vertices = mesh.GetVertices();

			foreach(var index in mesh.sharedVerticesInternal[sharedVertexHandle])
				vertices[index] = vertex;

			mesh.SetVertices(vertices);
		}
	}
}

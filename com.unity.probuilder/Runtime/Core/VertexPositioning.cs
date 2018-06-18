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
		public static Vector3[] VertexesInWorldSpace(this ProBuilderMesh mesh)
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
		/// Translate a set of vertexes with a world space offset.
		/// <br />
		/// Unlike most other mesh operations, this function applies the mesh positions to both ProBuilderMesh and the UnityEngine.Mesh.
		/// </summary>
		/// <param name="mesh">The mesh to be affected.</param>
		/// <param name="indexes">A set of triangles pointing to the vertex positions that are to be affected.</param>
		/// <param name="offset">The offset to apply in world coordinates.</param>
		public static void TranslateVertexesInWorldSpace(this ProBuilderMesh mesh, int[] indexes, Vector3 offset)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            mesh.TranslateVertexesInWorldSpace(indexes, offset, 0f, false, null);
		}

		/// <summary>
		/// Translate a set of vertexes with a world space offset.
		/// <br />
		/// Unlike most other mesh operations, this function applies the mesh positions to both ProBuilderMesh and the UnityEngine.Mesh.
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="indexes">A distinct list of vertex indexes.</param>
		/// <param name="offset">The direction and magnitude to translate selectedTriangles, in world space.</param>
		/// <param name="snapValue">If > 0 snap each vertex to the nearest on-grid point in world space.</param>
		/// <param name="snapAxisOnly">If true vertexes will only be snapped along the active axis.</param>
		/// <param name="lookup">A shared index lookup table.  Can pass NULL to have this automatically calculated.</param>
		internal static void TranslateVertexesInWorldSpace(this ProBuilderMesh mesh, int[] indexes, Vector3 offset, float snapValue, bool snapAxisOnly, Dictionary<int, int> lookup)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            int i = 0;
			int[] distinct = lookup != null ? mesh.sharedIndexesInternal.AllIndexesWithValues(lookup, indexes).ToArray() : mesh.sharedIndexesInternal.AllIndexesWithValues(indexes).ToArray();

			Matrix4x4 w2l = mesh.transform.worldToLocalMatrix;

			Vector3 localOffset = w2l * offset;

			Vector3[] verts = mesh.positionsInternal;

			// Snaps to world grid
			if(Mathf.Abs(snapValue) > Mathf.Epsilon)
			{
				Matrix4x4 l2w = mesh.transform.localToWorldMatrix;
				Vector3 mask = snapAxisOnly ? offset.ToMask(Math.handleEpsilon) : Vector3.one;

				for(i = 0; i < distinct.Length; i++)
				{
					var v = l2w.MultiplyPoint3x4(verts[distinct[i]] + localOffset);
					verts[distinct[i]] = w2l.MultiplyPoint3x4( Snapping.SnapValue(v, snapValue * mask) );
				}
			}
			else
			{
				for(i = 0; i < distinct.Length; i++)
					verts[distinct[i]] += localOffset;
			}

			// don't bother calling a full ToMesh() here because we know for certain that the vertexes and msh.vertexes arrays are equal in length
			mesh.positions = verts;
			mesh.mesh.vertices = verts;
		}

		/// <summary>
		/// Translate a set of vertexes with an offset provided in local (model) coordinates.
		/// <br />
		/// Unlike most other mesh operations, this function applies the mesh positions to both ProBuilderMesh and the UnityEngine.Mesh.
		/// </summary>
		/// <param name="mesh">The mesh to be affected.</param>
		/// <param name="indexes">A set of triangles pointing to the vertex positions that are to be affected.</param>
		/// <param name="offset"></param>
		public static void TranslateVertexes(this ProBuilderMesh mesh, IEnumerable<int> indexes, Vector3 offset)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

			int[] all = mesh.sharedIndexesInternal.AllIndexesWithValues(indexes).ToArray();

			Vector3[] verts = mesh.positionsInternal;

			for(int i = 0, c = all.Length; i < c; i++)
				verts[all[i]] += offset;

			// don't bother calling a full ToMesh() here because we know for certain that the vertexes and msh.vertices arrays are equal in length
			mesh.mesh.vertices = verts;
		}

		/// <summary>
		/// Given a shared vertex index (index of the triangle in the sharedIndexes array), move all vertexes to new position.
		/// Position is in model space coordinates.
		/// <br /><br />
		/// Use @"UnityEngine.ProBuilder.ProBuilderMesh.sharedIndexes" and IntArrayUtility.IndexOf to get a shared (or common) index.
		/// </summary>
		/// <param name="mesh">The target mesh.</param>
		/// <param name="sharedIndex">The shared (or common) index to set the vertex position of.</param>
		/// <param name="position">The new position in model coordinates.</param>
		public static void SetSharedVertexPosition(this ProBuilderMesh mesh, int sharedIndex, Vector3 position)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

            Vector3[] v = mesh.positionsInternal;
			int[] array = mesh.sharedIndexesInternal[sharedIndex].array;

			for(int i = 0; i < array.Length; i++)
				v[array[i]] = position;

			mesh.positions = v;
			mesh.mesh.vertices = v;
		}

		/// <summary>
		/// Set a collection of mesh attributes with a Vertex.
		/// <br /><br />
		/// Use @"UnityEngine.ProBuilder.ProBuilderMesh.sharedIndexes" and IntArrayUtility.IndexOf to get a shared (or common) index.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="sharedIndex"></param>
		/// <param name="vertex"></param>
		internal static void SetSharedVertexValues(this ProBuilderMesh pb, int sharedIndex, Vertex vertex)
		{
			Vertex[] vertexes = pb.GetVertexes();

			int[] array = pb.sharedIndexesInternal[sharedIndex].array;

			for(int i = 0; i < array.Length; i++)
				vertexes[array[i]] = vertex;

			pb.SetVertexes(vertexes);
		}
	}
}

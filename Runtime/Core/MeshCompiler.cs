using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using System.Collections;

namespace UnityEngine.ProBuilder
{
	/// <summary>
	/// Utilities for working converting from a pb_Object to a UnityEngine.Mesh.
	/// </summary>
	public static class MeshCompiler
	{
        /// <summary>
        /// Compile a UnityEngine::Mesh from a pb_Object.
        /// </summary>
        /// <param name="pb">The mesh source.</param>
        /// <param name="target">Destination UnityEngine.Mesh.</param>
        /// <param name="preferredTopology">If specified, the function will try to create topology matching the reqested format (and falling back on triangles where necessary).</param>
        /// <returns>The resulting material array from the compiled faces array.</returns>
        public static Material[] Compile(ProBuilderMesh pb, Mesh target, MeshTopology preferredTopology = MeshTopology.Triangles)
        {
            if (pb == null)
                throw new ArgumentNullException("pb");

            if (target == null)
                throw new ArgumentNullException("target");

            target.Clear();

            target.vertices = pb.positionsInternal;
            target.uv = pb.texturesInternal;

            if (pb.hasUv3)
            {
                List<Vector4> uvChannel = new List<Vector4>();
                pb.GetUVs(2, uvChannel);
                target.SetUVs(2, uvChannel);
            }

            if (pb.hasUv4)
            {
                List<Vector4> uvChannel = new List<Vector4>();
                pb.GetUVs(3, uvChannel);
                target.SetUVs(3, uvChannel);
            }

            target.normals = MeshUtility.CalculateNormals(pb);

            MeshUtility.GenerateTangent(target);

            if (pb.colorsInternal != null && pb.colorsInternal.Length == target.vertexCount)
                target.colors = pb.colorsInternal;

            var submeshes = Face.GetSubmeshes(pb.facesInternal, preferredTopology);
            target.subMeshCount = submeshes.Length;

            for (int i = 0; i < target.subMeshCount; i++)
#if UNITY_5_5_OR_NEWER
                target.SetIndices(submeshes[i].m_Indices, submeshes[i].m_Topology, i, false);
#else
        		target.SetIndices(submeshes[i].indices, submeshes[i].topology, i);
#endif

            target.name = string.Format("pb_Mesh{0}", pb.id);

            return submeshes.Select(x => x.m_Material).ToArray();
        }

        /// <summary>
        /// Create UV0 channel and return it.
        /// </summary>
        /// <param name="pb"></param>
        /// <returns></returns>
        internal static Vector2[] GetUVs(ProBuilderMesh pb)
		{
			int n = -2;
			Dictionary<int, List<Face>> textureGroups = new Dictionary<int, List<Face>>();
			bool anyWorldSpace = false;
			List<Face> group;

			foreach (Face f in pb.facesInternal)
			{
				if (f.uv.useWorldSpace)
					anyWorldSpace = true;

				if (f == null || f.manualUV)
					continue;

				if (f.textureGroup > 0 && textureGroups.TryGetValue(f.textureGroup, out group))
					group.Add(f);
				else
					textureGroups.Add(f.textureGroup > 0 ? f.textureGroup : n--, new List<Face>() { f });
			}

			n = 0;

			Vector3[] world = anyWorldSpace ? pb.VerticesInWorldSpace() : null;
			Vector2[] uvs = pb.texturesInternal != null && pb.texturesInternal.Length == pb.vertexCount ? pb.texturesInternal : new Vector2[pb.vertexCount];

			foreach (KeyValuePair<int, List<Face>> kvp in textureGroups)
			{
				Vector3 nrm;
				int[] indices = kvp.Value.SelectMany(x => x.distinctIndices).ToArray();

				if (kvp.Value.Count > 1)
					nrm = Projection.FindBestPlane(pb.positionsInternal, indices).normal;
				else
					nrm = Math.Normal(pb, kvp.Value[0]);

				if (kvp.Value[0].uv.useWorldSpace)
					UnwrappingUtility.PlanarMap2(world, uvs, indices, kvp.Value[0].uv, pb.transform.TransformDirection(nrm));
				else
					UnwrappingUtility.PlanarMap2(pb.positionsInternal, uvs, indices, kvp.Value[0].uv, nrm);

				// Apply UVs to array, and update the localPivot and localSize caches.
				Vector2 pivot = kvp.Value[0].uv.localPivot;

				foreach (Face f in kvp.Value)
					f.uv.localPivot = pivot;
			}

			return uvs;
		}

		/// <summary>
		/// Merge coincident vertices where possible, optimizing the vertex count of a UnityEngine.Mesh.
		/// </summary>
		/// <param name="mesh">The mesh to optimize.</param>
		/// <param name="vertices">
		/// If provided these values are used in place of extracting attributes from the Mesh.
		/// This is a performance optimization for when this array already exists. If not provided this array will be
		/// automatically generated for you.
		/// </param>
		public static void CollapseSharedVertices(Mesh mesh, Vertex[] vertices = null)
		{
            if (mesh == null)
                throw new System.ArgumentNullException("mesh");

			if (vertices == null)
				vertices = Vertex.GetVertices(mesh);

			int smc = mesh.subMeshCount;
			List<Dictionary<Vertex, int>> sub_vertices = new List<Dictionary<Vertex, int>>();
			int[][] tris = new int[smc][];
			int subIndex = 0;

			for (int i = 0; i < smc; ++i)
			{
				tris[i] = mesh.GetTriangles(i);
				Dictionary<Vertex, int> new_vertices = new Dictionary<Vertex, int>();

				for (int n = 0; n < tris[i].Length; n++)
				{
					Vertex v = vertices[tris[i][n]];
					int index;

					if (new_vertices.TryGetValue(v, out index))
					{
						tris[i][n] = index;
					}
					else
					{
						tris[i][n] = subIndex;
						new_vertices.Add(v, subIndex);
						subIndex++;
					}
				}

				sub_vertices.Add(new_vertices);
			}

			Vertex[] collapsed = sub_vertices.SelectMany(x => x.Keys).ToArray();
			Vertex.SetMesh(mesh, collapsed);
			mesh.subMeshCount = smc;
			for (int i = 0; i < smc; i++)
				mesh.SetTriangles(tris[i], i);
		}
	}
}

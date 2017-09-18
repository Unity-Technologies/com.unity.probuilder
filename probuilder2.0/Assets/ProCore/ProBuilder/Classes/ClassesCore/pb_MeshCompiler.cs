using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using System.Collections;

namespace ProBuilder2.Common
{
	/**
	 * Utilities for working with UnityEngine.Mesh.
	 */
	public class pb_MeshCompiler
	{
		/**
		 * Compile a UnityEngine::Mesh from a pb_Object.
		 */
		public static void Compile(pb_Object pb, ref Mesh target, out Material[] materials, MeshTopology preferredTopology = MeshTopology.Triangles)
		{
			target.Clear();

			target.vertices = pb.vertices;
			target.uv = GetUVs(pb);
#if UNITY_5_3_OR_NEWER
		if(pb.hasUv3) target.SetUVs(2, pb.uv3);
		if(pb.hasUv4) target.SetUVs(3, pb.uv4);
#endif
			Vector3[] normals = pb_MeshUtility.GenerateNormals(pb);
			pb_MeshUtility.SmoothNormals(pb, ref normals);
			target.normals = normals;
			pb_MeshUtility.GenerateTangent(ref target);
			if(pb.colors != null && pb.colors.Length == target.vertexCount)
				target.colors = pb.colors;

			pb_Submesh[] submeshes;

			target.subMeshCount = pb_Face.GetMeshIndices(pb.faces, out submeshes, preferredTopology);

			for(int i = 0; i < target.subMeshCount; i++)
#if UNITY_5_5_OR_NEWER
				target.SetIndices(submeshes[i].indices, submeshes[i].topology, i, false);
#else
				target.SetIndices(submeshes[i].indices, submeshes[i].topology, i);
#endif

			target.name = string.Format("pb_Mesh{0}", pb.id);

			materials = submeshes.Select(x => x.material).ToArray();
		}

		/**
		 * Create UV0 channel and return it.
		 */
		internal static Vector2[] GetUVs(pb_Object pb)
		{
			int n = -2;
			Dictionary<int, List<pb_Face>> textureGroups = new Dictionary<int, List<pb_Face>>();
			bool anyWorldSpace = false;
			List<pb_Face> group;

			foreach(pb_Face f in pb.faces)
			{
				if(f.uv.useWorldSpace)
					anyWorldSpace = true;

				if(f == null || f.manualUV)
					continue;

				if(f.textureGroup > 0 && textureGroups.TryGetValue(f.textureGroup, out group))
					group.Add(f);
				else
					textureGroups.Add(f.textureGroup > 0 ? f.textureGroup : n--, new List<pb_Face>() { f });
			}

			n = 0;

			Vector3[] world = anyWorldSpace ? pb.transform.ToWorldSpace(pb.vertices) : null;
			Vector2[] uvs = new Vector2[pb.vertexCount];

			foreach(KeyValuePair<int, List<pb_Face>> kvp in textureGroups)
			{
				Vector3 nrm;
				int[] indices = pb_Face.AllTrianglesDistinct(kvp.Value).ToArray();

				if(kvp.Value.Count > 1)
					nrm = pb_Projection.FindBestPlane(pb.vertices, indices).normal;
				else
					nrm = pb_Math.Normal(pb, kvp.Value[0]);

				if(kvp.Value[0].uv.useWorldSpace)
					pb_UVUtility.PlanarMap2(world, uvs, indices, kvp.Value[0].uv, pb.transform.TransformDirection(nrm));
				else
					pb_UVUtility.PlanarMap2(pb.vertices, uvs, indices, kvp.Value[0].uv, nrm);

				// Apply UVs to array, and update the localPivot and localSize caches.
				Vector2 pivot = kvp.Value[0].uv.localPivot;

				foreach(pb_Face f in kvp.Value)
					f.uv.localPivot = pivot;
			}

			return uvs;
		}

	}
}

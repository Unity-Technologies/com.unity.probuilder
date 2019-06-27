#if OPEN_SUBDIV_ENABLED

using UnityEngine;
using UnityEngine.OSD;

namespace UnityEngine.ProBuilder
{
    static class OpenSubdiv
	{
		const int k_MinSubdivLevel = 0;
		const int k_MaxSubdivLevel = 5;

		public static bool CompileMesh(ProBuilderMesh input, Mesh output, int materialCount, MeshTopology preferredTopology)
		{
			output.Clear();

			Submesh[] submeshes = Submesh.GetSubmeshes(input.faces, materialCount, MeshTopology.Quads);

			output.subMeshCount = submeshes.Length;

            input.Refresh(RefreshMask.UV);

			Vector3[] positions = input.positionsInternal;
			Color[] colors = input.HasArrays(MeshArrays.Color) ? input.colorsInternal : null;
			Vector2[] textures = input.HasArrays(MeshArrays.Texture0) ? input.texturesInternal : null;

            var settings = input.subdivisionSettings;

			foreach (var submesh in submeshes)
			{
				using (var refiner = new TopologyRefiner())
                {
                    refiner.autoPopulateBoundaryVertices = settings.generateBoundaryVertexWeights;
					refiner.defaultBoundaryVertexWeight = settings.generatedBoundaryVertexWeight;

                    refiner.SetInputIndices(submesh.m_Indexes, submesh.topology);
					refiner.SetInputChannel(VertexAttribute.Position, positions);

					if (input.HasArrays(MeshArrays.Color))
						refiner.SetInputChannel(VertexAttribute.Color, colors);

					if (input.HasArrays(MeshArrays.Texture0))
						refiner.SetInputChannel(VertexAttribute.TexCoord0, textures);

					if (!refiner.Evaluate(settings.subdivisionLevel, settings.subdivisionMethod))
						return false;

					int[] indices;
					MeshTopology topo;

					if (!refiner.GetOutputChannel(VertexAttribute.Position, out positions)
						|| !refiner.GetOutputIndices(out indices, out topo))
						return false;

					submesh.topology = topo;
					submesh.m_Indexes = indices;

					if (input.HasArrays(MeshArrays.Color))
						refiner.GetOutputChannel(VertexAttribute.Color, out colors);

					if (input.HasArrays(MeshArrays.Texture0))
						refiner.GetOutputChannel(VertexAttribute.TexCoord0, out textures);
				}
			}

			output.SetVertices(positions);

			if (input.HasArrays(MeshArrays.Color))
				output.SetColors(colors);

			if (input.HasArrays(MeshArrays.Texture0))
				output.SetUVs(0, textures);

			foreach (var submesh in submeshes)
				output.SetIndices(submesh.m_Indexes, submesh.topology, submesh.submeshIndex);

			output.RecalculateNormals();

			output.name = string.Format("pb_Mesh{0}", input.id);

			return true;
		}
    }
}
#endif

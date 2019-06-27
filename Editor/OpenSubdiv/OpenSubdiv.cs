#if OPEN_SUBDIV_ENABLED

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.ProBuilder;
using UnityEngine.OSD;

namespace UnityEngine.ProBuilder.Addons.OpenSubdiv
{
	class ProBuilderIntegrationSettings : EditorWindow
	{
		internal const int k_MinSubdivLevel = 0;
		internal const int k_MaxSubdivLevel = 5;

		[MenuItem("Tools/OpenSubdiv Settings")]
		static void Init()
		{
			GetWindow<ProBuilderIntegrationSettings>(true);
		}

		void OnGUI()
		{
			EditorGUI.BeginChangeCheck();

			ProBuilderIntegration.s_SubdivisionEnabled = EditorGUILayout.Toggle("Enabled", ProBuilderIntegration.s_SubdivisionEnabled);

			ProBuilderIntegration.s_SubdivisionLevel = (int)EditorGUILayout.Slider("Level", ProBuilderIntegration.s_SubdivisionLevel, k_MinSubdivLevel, k_MaxSubdivLevel);
			ProBuilderIntegration.s_SubdivisionMethod = (SubdivisionMethod)EditorGUILayout.EnumPopup("Subdivision Method", ProBuilderIntegration.s_SubdivisionMethod);

			ProBuilderIntegration.s_GenerateBoundaryVertices = EditorGUILayout.Toggle("Generate Boundary Vertices", ProBuilderIntegration.s_GenerateBoundaryVertices);
			EditorGUI.indentLevel++;
			using (new EditorGUI.DisabledScope(!ProBuilderIntegration.s_GenerateBoundaryVertices))
			{
				ProBuilderIntegration.s_BoundaryVertexWeight = EditorGUILayout.Slider("Weight", ProBuilderIntegration.s_BoundaryVertexWeight, 0f, 10f);
			}

			EditorGUI.indentLevel--;

			if (EditorGUI.EndChangeCheck())
				ProBuilderIntegration.Subdivide(MeshSelection.top);
		}
	}

	[InitializeOnLoad]
	static class ProBuilderIntegration
	{
		const int k_MinSubdivLevel = 0;
		const int k_MaxSubdivLevel = 5;

		internal static bool s_SubdivisionEnabled = true;
		internal static int s_SubdivisionLevel = 3;
		internal static SubdivisionMethod s_SubdivisionMethod = SubdivisionMethod.FeatureAdaptive;
		internal static bool s_GenerateBoundaryVertices;
		internal static float s_BoundaryVertexWeight;

		static ProBuilderIntegration()
		{
			ProBuilderMesh.willCompileMesh += CompileMesh;
		}

		public static void Subdivide(IEnumerable<ProBuilderMesh> meshes)
		{
			foreach (var mesh in meshes)
			{
				mesh.AssignToMeshFilter();
				mesh.Optimize();
			}
		}

		public static bool CompileMesh(ProBuilderMesh input, Mesh output, int materialCount, MeshTopology preferredTopology)
		{
			if (!s_SubdivisionEnabled)
				return false;

			output.Clear();

			Submesh[] submeshes = Submesh.GetSubmeshes(input.faces, materialCount, MeshTopology.Quads);

			output.subMeshCount = submeshes.Length;

			Vector3[] positions = input.positionsInternal;
			Color[] colors = input.HasArrays(MeshArrays.Color) ? input.colorsInternal : null;
			Vector2[] textures = input.HasArrays(MeshArrays.Texture0) ? input.texturesInternal : null;

			foreach (var submesh in submeshes)
			{
				using (var refiner = new TopologyRefiner())
				{
					refiner.autoPopulateBoundaryVertices = s_GenerateBoundaryVertices;
					refiner.defaultBoundaryVertexWeight = s_BoundaryVertexWeight;

					refiner.SetInputIndices(submesh.m_Indexes, submesh.topology);
					refiner.SetInputChannel(VertexAttribute.Position, positions);

					if (input.HasArrays(MeshArrays.Color))
						refiner.SetInputChannel(VertexAttribute.Color, colors);

					if (input.HasArrays(MeshArrays.Texture0))
						refiner.SetInputChannel(VertexAttribute.TexCoord0, textures);

					if (!refiner.Evaluate(s_SubdivisionLevel, s_SubdivisionMethod))
						return false;

					int[] indices;
					MeshTopology topo;

					if (!refiner.GetOutputChannelVector3(VertexAttribute.Position, out positions)
						|| !refiner.GetOutputIndices(out indices, out topo))
						return false;

					submesh.topology = topo;
					submesh.m_Indexes = indices;

					if (input.HasArrays(MeshArrays.Color))
						refiner.GetOutputChannelColor(VertexAttribute.Color, out colors);

					if (input.HasArrays(MeshArrays.Texture0))
						refiner.GetOutputChannelVector2(VertexAttribute.TexCoord0, out textures);
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

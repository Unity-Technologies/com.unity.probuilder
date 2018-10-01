using UnityEngine;
using UnityEditor;
using System;
#if PROBUILDER_FBX_PLUGIN_ENABLED
using Autodesk.Fbx;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEditor.Formats.Fbx.Exporter;
#endif

namespace UnityEngine.ProBuilder.Addons.FBX
{
	/// <summary>
	/// ProBuilder-specific options when exporting FBX files with the Unity FBX Exporter.
	/// </summary>
	class FbxOptions
	{
		/// <summary>
		/// Export mesh topology as quads if possible.
		/// </summary>
		#pragma warning disable 649
		public bool quads;
		#pragma warning restore 649
	}

	/// <summary>
	/// Provides some additional functionality when the FbxSdk and FbxExporter packages are available in the project.
	/// </summary>
	[InitializeOnLoad]
	static class Fbx
	{
		static bool s_FbxIsLoaded = false;


#pragma warning disable 414
		static readonly string[] k_ProBuilderTypes = new string[]
		{
			"BezierShape",
			"PolyShape",
			"Entity",
		};
#pragma warning restore 414

		public static bool fbxEnabled { get { return s_FbxIsLoaded; } }

#if PROBUILDER_FBX_PLUGIN_ENABLED

		static FbxOptions m_FbxOptions = new FbxOptions() {
			quads = true
		};

		static Fbx()
		{
			TryLoadFbxSupport();
		}

		static void TryLoadFbxSupport()
		{
			if(s_FbxIsLoaded)
				return;
			ModelExporter.RegisterMeshCallback<ProBuilderMesh>(GetMeshForComponent, true);
			m_FbxOptions.quads = PreferencesInternal.GetBool("Export::m_FbxQuads", true);
			s_FbxIsLoaded = true;
		}

		static bool GetMeshForComponent(ModelExporter exporter, ProBuilderMesh pmesh, FbxNode node)
		{
			Mesh mesh = new Mesh();
			var materials = MeshUtility.Compile(pmesh, mesh, m_FbxOptions.quads ? MeshTopology.Quads : MeshTopology.Triangles);
			exporter.ExportMesh(mesh, node, materials);
			Object.DestroyImmediate(mesh);

			// probuilder can't handle mesh assets that may be externally reloaded, just strip pb stuff for now.
			foreach (var type in k_ProBuilderTypes)
			{
				var component = pmesh.GetComponent(type);
				if(component != null)
					Object.DestroyImmediate(component);
			}

			pmesh.preserveMeshAssetOnDestroy = true;
			Object.DestroyImmediate(pmesh);

			return true;
		}
#endif
	}
}

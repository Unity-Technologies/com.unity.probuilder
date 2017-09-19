/**
 * Provides some additional functionality when the FbxSdk and FbxExporter packages
 * are available in the project.
 */

using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;
#if PROBUILDER_FBX_ENABLED
using Unity.FbxSdk;
using FbxExporters;
using FbxExporters.Editor;
#endif

namespace ProBuilder2.Common
{
	/*
	* Options when exporting FBX files.
	*/
	public class pb_FbxOptions
	{
		public bool quads;
	}

	[InitializeOnLoad]
	public static class pb_Fbx
	{
		private static bool m_FbxIsLoaded = false;

		public static bool FbxEnabled { get { return m_FbxIsLoaded; } }

		private static pb_FbxOptions m_FbxOptions = new pb_FbxOptions() {
			quads = true
		};

#if PROBUILDER_FBX_ENABLED
		static pb_Fbx()
		{
			TryLoadFbxSupport();
		}

		static void TryLoadFbxSupport()
		{
			pb_Log.Debug("Attempt load FBX hooks");
			if(m_FbxIsLoaded)
				return;
			FbxPrefab.OnUpdate += OnFbxUpdate;
			ModelExporter.RegisterMeshCallback<pb_Object>(GetMeshForComponent, true);
			m_FbxOptions.quads = pb_PreferencesInternal.GetBool("Export::m_FbxQuads", true);
			m_FbxIsLoaded = true;
			pb_Log.Debug("FBX support successfully loaded");
		}


		private static void OnFbxUpdate(FbxPrefab updatedInstance, IEnumerable<GameObject> updatedObjects)
		{
			pb_Log.Debug("OnFbxUpdate");
		}

		private static bool GetMeshForComponent(ModelExporter exporter, pb_Object component, FbxNode fbxNode)
		{
			pb_Log.Debug("GetMeshForComponent: " + component.name);
			Mesh mesh = new Mesh();
			Material[] materials = null;
			pb_MeshCompiler.Compile(component, ref mesh, out materials, m_FbxOptions.quads ? MeshTopology.Quads : MeshTopology.Triangles);
			exporter.ExportMesh(mesh, fbxNode, materials);
			UnityEngine.Object.DestroyImmediate(mesh);
			return true;
		}
#endif
	}
}

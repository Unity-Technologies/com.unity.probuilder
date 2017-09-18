#define FBX_ENABLED

using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;

#if FBX_ENABLED
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
		public bool ngons; // @todo
	}

	[InitializeOnLoad]
	public static class pb_Fbx
	{
		private static bool m_FbxIsLoaded = false;

		public static bool FbxEnabled { get { return m_FbxIsLoaded; } }

		static pb_Fbx()
		{
			TryLoadFbxSupport();
		}

		static void TryLoadFbxSupport()
		{
			if(m_FbxIsLoaded)
				return;
			FbxPrefab.OnUpdate += OnFbxUpdate;
			ModelExporter.RegisterMeshCallback<pb_Object>(GetMeshForComponent, true);
			m_FbxIsLoaded = true;
		}

		private static void OnFbxUpdate(FbxPrefab updatedInstance, IEnumerable<GameObject> updatedObjects)
		{
			pb_Log.Info("OnFbxUpdate");
		}

		private static bool GetMeshForComponent(ModelExporter exporter, pb_Object component, FbxNode fbxNode)
		{
			pb_Log.Debug("GetMeshForComponent: " + component.name);

			Mesh mesh = new Mesh();
			Material[] materials = null;
			pb_MeshCompiler.Compile(component, ref mesh, out materials, MeshTopology.Quads);
			exporter.ExportMesh(mesh, fbxNode, materials);
			UnityEngine.Object.DestroyImmediate(mesh);
			return true;
		}
	}
}

using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Reflection;


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
	static class pb_FbxListener
	{
		private static bool m_FbxIsLoaded = false; 

		public static bool FbxExportEnabled { get { return m_FbxIsLoaded; } } 

		static pb_FbxListener() 
		{
			TryLoadFbxSupport(); 
			// FbxExporters.FbxPrefab.OnUpdate += OnFbxUpdate;
		}
	 
		static void TryLoadFbxSupport() 
		{ 
			if(m_FbxIsLoaded) 
				return; 

			Type modelExporterType = pb_Reflection.GetType("FbxExporters.Editor.ModelExporter"); 

			m_FbxIsLoaded = modelExporterType != null;

		 //  EventInfo onGetMeshInfoEvent = modelExporterType != null ? modelExporterType.GetEvent("onGetMeshInfo") : null; 
		 //  m_FbxIsLoaded = false; 
		 //  if(onGetMeshInfoEvent != null) 
		 //  { 
			// try 
			// { 
			//   Type delegateType = onGetMeshInfoEvent.EventHandlerType; 
			//   MethodInfo add = onGetMeshInfoEvent.GetAddMethod(); 
			//   MethodInfo ogmiMethod = typeof(pb_FbxExportListener).GetMethod("OnGetMeshInfo", BindingFlags.Static | BindingFlags.NonPublic); 
			//   Delegate d = Delegate.CreateDelegate(delegateType, ogmiMethod); 
			//   add.Invoke(null, new object[] { d }); 
			//   m_FbxIsLoaded = true; 
			// } 
			// catch 
			// { 
			//   pb_Log.Warning("Failed loading FbxExporter delegates. Fbx export will still work correctly, but ProBuilder will not be able export quads or ngons."); 
			// } 
	 
			// ReloadOptions(); 
		 //  } 
		} 

		[MenuItem("Tools/Debug/ProBuilder/Reset with MeshFilter")]
		static void ResetSelection()
		{
			OnFbxUpdate(null, Selection.gameObjects);
		}

		static void OnFbxUpdate(FbxExporters.FbxPrefab updatedInstance, IEnumerable<GameObject> updatedObjects)
		{
			foreach(GameObject go in updatedObjects)
			{
				pb_Object pb = go.GetComponent<pb_Object>();

				if(pb == null)
					continue;

				pbMeshOps.ResetPbObjectWithMeshFilter(pb, true);

				// @todo Rebuild()
				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();

				pb_Log.Info("reset: " + go.name + " with MeshFilter");
			}
		}
	}
}

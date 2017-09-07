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

		public static bool FbxEnabled { get { return m_FbxIsLoaded; } } 

		static pb_FbxListener() 
		{
			TryLoadFbxSupport(); 
		}
	 
		static void TryLoadFbxSupport() 
		{ 
			if(m_FbxIsLoaded) 
				return; 

			Type fbxPrefabComponentType = pb_Reflection.GetType("FbxExporters.FbxPrefab"); 

			if(fbxPrefabComponentType != null)
			{
				EventInfo onFbxUpdateEvent = fbxPrefabComponentType.GetEvent("OnUpdate");

				if(onFbxUpdateEvent != null)
				{
					try
					{
						Type delegateType = onFbxUpdateEvent.EventHandlerType;
						MethodInfo addMethod = onFbxUpdateEvent.GetAddMethod();
						MethodInfo updateHandler = typeof(pb_FbxListener).GetMethod("OnFbxUpdate", BindingFlags.Static | BindingFlags.NonPublic);
						Delegate del = Delegate.CreateDelegate(delegateType, updateHandler);
						addMethod.Invoke(null, new object[] { del });
						m_FbxIsLoaded = true;
					}
					catch
					{
						pb_Log.Warning("Failed loading ProBuilder FBX Listener delegates. FBX export and import still work correctly, but ProBuilder will not be able export quads or see changes made to the FBX file."); 
					}
				}
			}
		} 

		// [MenuItem("Tools/Debug/ProBuilder/Reset with MeshFilter")]
		// static void ResetSelection()
		// {
		// 	OnFbxUpdate(null, Selection.gameObjects);
		// }

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

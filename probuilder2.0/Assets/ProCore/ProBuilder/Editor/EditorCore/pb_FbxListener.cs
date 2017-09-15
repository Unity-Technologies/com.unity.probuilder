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
	public static class pb_FbxListener
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

			bool onUpdateHook = false, onExportHook = false;

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
						onUpdateHook = true;
					}
					catch
					{
						pb_Log.Warning("Failed loading ProBuilder FBX Listener delegates. FBX export and import still work correctly, but ProBuilder will not be able export quads or see changes made to the FBX file.");
					}
				}
				else
				{
					pb_Log.Warning("Failed to find FbxPrefab::OnUpdate event.");
				}
			}

			try
			{
				Type modelExporterType = pb_Reflection.GetType("FbxExporters.Editor.ModelExporter");

				if(modelExporterType != null)
				{
					// There are two overloads to this function, one generic and one not. Reflection with a type list isn't
					// easy because of the GetMeshForComponent type not being available (also difficult to reflect that out).
					MethodInfo registerCallbackMethod = modelExporterType.GetMethods(BindingFlags.Static | BindingFlags.Public)
						.Where(x => !x.IsGenericMethod && x.Name.Equals("RegisterMeshCallback"))
							.FirstOrDefault();

					ParameterInfo delegateType = registerCallbackMethod.GetParameters().FirstOrDefault(x => x.ParameterType.Name.Equals("GetMeshForComponent"));

					if(registerCallbackMethod != null && delegateType != null)
					{
						MethodInfo onGetMeshForComponent = typeof(pb_FbxListener).GetMethod("OnGetMeshForComponent", BindingFlags.Static | BindingFlags.NonPublic);
						Delegate getMeshDelegate = Delegate.CreateDelegate(delegateType.ParameterType, onGetMeshForComponent);
						registerCallbackMethod.Invoke(null, new object[] { typeof(pb_Object), getMeshDelegate, true });
						onExportHook = true;
					}
					else
					{
						pb_Log.Warning("Failed to find ModelExporter::RegisterMeshCallback function! ProBuilder may not work correctly with FBX export.");
					}
				}
				else
				{
					pb_Log.Warning("Failed to find FbxExporters::Editor::ModelExporter! ProBuilder may not work correctly with FBX export.");
				}
			}
			catch
			{
				pb_Log.Warning("Failed loading FBX export delegate. ProBuilder may not work correctly with FBX exporter.");
			}

			m_FbxIsLoaded = onUpdateHook && onExportHook;
		}

		private static void OnFbxUpdate(object updatedInstance, IEnumerable<GameObject> updatedObjects)
		{
			pb_Log.Info("OnFbxUpdate");

			foreach(GameObject go in updatedObjects)
			{
				pb_Object pb = go.GetComponent<pb_Object>();

				if(pb == null)
					continue;

				pbMeshOps.ResetPbObjectWithMeshFilter(pb, false);

				// @todo Rebuild()
				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			pb_Editor.Refresh();
		}

		private static bool OnGetMeshForComponent(MonoBehaviour component, out Mesh mesh)
		{
			pb_Log.Info("export ->" + component.gameObject.name);
			mesh = null;
			return true;
		}
	}
}

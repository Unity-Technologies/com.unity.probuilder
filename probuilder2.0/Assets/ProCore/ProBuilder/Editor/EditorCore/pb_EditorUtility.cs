#pragma warning disable 0168

using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Reflection;
using ProBuilder.Actions;
using ProBuilder.Core;
using ProBuilder.MeshOperations;
using UnityEngine.Rendering;
using UObject = UnityEngine.Object;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Delegate to be raised when a ProBuilder object is created.
	/// </summary>
	/// <param name="pb"></param>
	public delegate void OnObjectCreated(pb_Object pb);

	/// <summary>
	/// Utilities for working in Unity editor: Showing notifications in windows, getting the sceneview, setting EntityTypes, OBJ export, etc.
	/// </summary>
	static class pb_EditorUtility
	{
		const float k_DefaultNotificationDuration = 1f;
		static float s_NotificationTimer = 0f;
		static EditorWindow s_NotificationWindow;
		static bool s_IsNotificationDisplayed = false;

		const BindingFlags k_BindingFlagsAll =
			BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		/// <summary>
		/// Subscribe to this delegate to be notified when a pb_Object has been created and initialized through ProBuilder.
		/// </summary>
		/// <remarks>
		/// This is only called when an object is initialized in editor. Ie, pb_ShapeGenerator.GenerateCube(Vector3.one) won't fire this callback.
		/// </remarks>
		public static event OnObjectCreated onObjectCreated = null;

		/// <summary>
		/// Add a listener to the multicast onObjectCreated delegate.
		/// </summary>
		/// <param name="onProBuilderObjectCreated"></param>
		public static void AddOnObjectCreatedListener(OnObjectCreated onProBuilderObjectCreated)
		{
			if(onObjectCreated == null)
				onObjectCreated = onProBuilderObjectCreated;
			else
				onObjectCreated += onProBuilderObjectCreated;
		}

		/// <summary>
		/// Remove a listener from the onObjectCreated delegate.
		/// </summary>
		/// <param name="onProBuilderObjectCreated"></param>
		public static void RemoveOnObjectCreatedListener(OnObjectCreated onProBuilderObjectCreated)
		{
			if(onObjectCreated != null)
				onObjectCreated -= onProBuilderObjectCreated;
		}

		/// <summary>
		/// Set the selected render state for an object.  In Unity 5.4 and lower, this just toggles wireframe on or off.
		/// </summary>
		/// <param name="renderer"></param>
		/// <param name="state"></param>
		public static void SetSelectionRenderState(Renderer renderer, SelectionRenderState state)
		{
			EditorUtility.SetSelectedRenderState(renderer, (EditorSelectedRenderState) state );
		}

		public static SelectionRenderState GetSelectionRenderState()
		{
			bool wireframe = false, outline = false;

			try {
				wireframe = (bool) pb_Reflection.GetValue(null, "UnityEditor.AnnotationUtility", "showSelectionWire");
				outline = (bool) pb_Reflection.GetValue(null, "UnityEditor.AnnotationUtility", "showSelectionOutline");
			} catch {
				pb_Log.Warning("Looks like Unity changed the AnnotationUtility \"showSelectionOutline\"\nPlease email contact@procore3d.com and let Karl know!");
			}

			SelectionRenderState state = SelectionRenderState.None;

			if(wireframe) state |= SelectionRenderState.Wireframe;
			if(outline) state |= SelectionRenderState.Outline;

			return state;
		}

		/// <summary>
		/// Show a timed notification in the SceneView window.
		/// </summary>
		/// <param name="notif"></param>
		public static void ShowNotification(string notif)
		{
			SceneView scnview = SceneView.lastActiveSceneView;
			if(scnview == null)
				scnview = EditorWindow.GetWindow<SceneView>();

			ShowNotification(scnview, notif);
		}

		public static void ShowNotification(EditorWindow window, string notif)
		{
			if(pb_PreferencesInternal.HasKey(pb_Constant.pbShowEditorNotifications) && !pb_PreferencesInternal.GetBool(pb_Constant.pbShowEditorNotifications))
				return;

			window.ShowNotification(new GUIContent(notif, ""));
			window.Repaint();

			if(EditorApplication.update != NotifUpdate)
				EditorApplication.update += NotifUpdate;

			s_NotificationTimer = Time.realtimeSinceStartup + k_DefaultNotificationDuration;
			s_NotificationWindow = window;
			s_IsNotificationDisplayed = true;
		}

		public static void RemoveNotification(EditorWindow window)
		{
			EditorApplication.update -= NotifUpdate;

			window.RemoveNotification();
			window.Repaint();
		}

		static void NotifUpdate()
		{
			if(s_IsNotificationDisplayed && Time.realtimeSinceStartup > s_NotificationTimer)
			{
				s_IsNotificationDisplayed = false;
				RemoveNotification(s_NotificationWindow);
			}
		}

		[System.Obsolete("Please use pb_Obj.Export")]
		public static string ExportOBJ(pb_Object[] pb)
		{
			return ExportObj.ExportWithFileDialog(pb);
		}

		/**
		 * Open a save file dialog, and save the image to that path.
		 */
		public static void SaveTexture(Texture2D texture)
		{
			string path = EditorUtility.SaveFilePanel("Save Image", Application.dataPath, "", "png");
			SaveTexture(texture, path);
		}

		/**
		 * Save an image to the specified path.
		 */
		public static void SaveTexture(Texture2D texture, string path)
		{
			byte[] bytes = texture.EncodeToPNG();

			if(path == "") return;

			System.IO.File.WriteAllBytes(path, bytes);

			AssetDatabase.Refresh();
		}

		/**
		 * Returns true if this object is a prefab instanced in the scene.
		 */
		public static bool IsPrefabInstance(GameObject go)
		{
			return PrefabUtility.GetPrefabType(go) == PrefabType.PrefabInstance;
		}

		/**
		 * Returns true if this object is a prefab in the Project view.
		 */
		public static bool IsPrefabRoot(GameObject go)
		{
			return PrefabUtility.GetPrefabType(go) == PrefabType.Prefab;
		}

		/**
		 *	Returns true if Asset Store window is open, false otherwise.
		 */
		public static bool AssetStoreWindowIsOpen()
		{
			return Resources.FindObjectsOfTypeAll<EditorWindow>().Any(x => x.GetType().ToString().Contains("AssetStoreWindow"));
		}

		/**
		 * Ensure that this object has a valid mesh reference, and the geometry is
		 * current.
		 */
		public static MeshRebuildReason VerifyMesh(pb_Object pb)
		{
		 	Mesh oldMesh = pb.msh;
	 		MeshRebuildReason reason = pb.Verify();
			bool meshesAreAssets = pb_PreferencesInternal.GetBool(pb_Constant.pbMeshesAreAssets);

			if( reason != MeshRebuildReason.None )
			{
				/**
				 * If the mesh ID doesn't match the gameObject Id, it could mean two things -
				 * 1. The object was just duplicated, and then made unique
				 * 2. The scene was reloaded, and gameObject ids were recalculated.
				 * If the latter, we need to clean up the old mesh.  If the former,
				 * the old mesh needs to *not* be destroyed.
				 */
				if(oldMesh)
				{
					int meshNo = -1;
					int.TryParse(oldMesh.name.Replace("pb_Mesh", ""), out meshNo);

					UnityEngine.Object dup = EditorUtility.InstanceIDToObject(meshNo);
					GameObject go = dup as GameObject;

					if(go == null)
					{
						// Debug.Log("scene reloaded - false positive.");
						pb.msh.name = "pb_Mesh" + pb.id;
					}
					else
					{
						// Debug.Log("duplicate mesh");

						if(!meshesAreAssets || !(pb_EditorUtility.IsPrefabRoot(pb.gameObject) || IsPrefabInstance(pb.gameObject)))
						{
							// deep copy arrays & ToMesh/Refresh
							pb.MakeUnique();
							pb.Optimize();
						}
					}
				}
				else
				{
					// old mesh didn't exist, so this is probably a prefab being instanced

					if(pb_EditorUtility.IsPrefabRoot(pb.gameObject))
						pb.msh.hideFlags = (HideFlags) (1 | 2 | 4 | 8);

					pb.Optimize();
				}
			}
			else
			{
				if(meshesAreAssets)
					pb_EditorMeshUtility.TryCacheMesh(pb);
			}

			return reason;
		}

		/// <summary>
		/// Returns true if GameObject contains flags.
		/// </summary>
		/// <param name="go"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		public static bool HasStaticFlag(this GameObject go, StaticEditorFlags flags)
		{
			return (GameObjectUtility.GetStaticEditorFlags(go) & flags) == flags;
		}

		/// <summary>
		/// Initialize this object with the various editor-only parameters, and invoke the object creation callback.
		/// </summary>
		/// <param name="pb"></param>
		public static void InitObject(pb_Object pb)
		{
			ShadowCastingMode scm = pb_PreferencesInternal.GetEnum<ShadowCastingMode>(pb_Constant.pbShadowCastingMode);
			pb.GetComponent<MeshRenderer>().shadowCastingMode = scm;
			ScreenCenter( pb.gameObject );

			var flags = pb_PreferencesInternal.HasKey(pb_Constant.pbDefaultStaticFlags)
				? pb_PreferencesInternal.GetEnum<StaticEditorFlags>(pb_Constant.pbDefaultStaticFlags)
				: StaticEditorFlags.LightmapStatic |
				  StaticEditorFlags.OccluderStatic |
				  StaticEditorFlags.OccludeeStatic |
				  StaticEditorFlags.BatchingStatic |
				  StaticEditorFlags.NavigationStatic |
				  StaticEditorFlags.OffMeshLinkGeneration |
				  StaticEditorFlags.ReflectionProbeStatic;

			GameObjectUtility.SetStaticEditorFlags(pb.gameObject, flags);

			switch(pb_PreferencesInternal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider))
			{
				case ColliderType.BoxCollider:
					pb.gameObject.AddComponent<BoxCollider>();
					break;

				case ColliderType.MeshCollider:
					pb.gameObject.AddComponent<MeshCollider>().convex = pb_PreferencesInternal.GetBool(pb_Constant.pbForceConvex, false);
					break;
			}

			pb.Optimize();

			if( onObjectCreated != null )
				onObjectCreated(pb);
		}

		[System.Obsolete("pb_Entity is deprecated, please use InitObject(pb_Object)")]
		public static void InitObject(pb_Object pb, ColliderType colliderType, EntityType entityType)
		{
			switch(colliderType)
			{
				case ColliderType.BoxCollider:
					pb.gameObject.AddComponent<BoxCollider>();
				break;

				case ColliderType.MeshCollider:
					pb.gameObject.AddComponent<MeshCollider>().convex = pb_PreferencesInternal.HasKey(pb_Constant.pbForceConvex) ? pb_PreferencesInternal.GetBool(pb_Constant.pbForceConvex) : false;
					break;
			}

			ShadowCastingMode scm = pb_PreferencesInternal.GetEnum<ShadowCastingMode>(pb_Constant.pbShadowCastingMode);
			pb.GetComponent<MeshRenderer>().shadowCastingMode = scm;

			pb_EntityUtility.SetEntityType(entityType, pb.gameObject);
			ScreenCenter( pb.gameObject );
			pb.Optimize();
		}

		/**
		 * Puts the selected gameObject at the pivot point of the SceneView camera.
		 */
		public static void ScreenCenter(GameObject _gameObject)
		{
			if(_gameObject == null)
				return;

			// If in the unity editor, attempt to center the object the sceneview or main camera, in that order
			_gameObject.transform.position = ScenePivot();

			Selection.activeObject = _gameObject;
		}

		/**
		 * Gets the current SceneView's camera's pivot point.
		 */
		public static Vector3 ScenePivot()
		{
			return GetSceneView().pivot;
		}

		/**
		 * If pb_Preferences_Internal.say to set pivot to corner and ProGrids or PB pref says snap to grid, do it.
		 * @param indicesToCenterPivot If any values are passed here, the pivot is set to an average of all vertices at indices.  If null, the first vertex is used as the pivot.
		 */
		public static void SetPivotAndSnapWithPref(pb_Object pb, int[] indicesToCenterPivot)
		{
			if(pb_PreferencesInternal.GetBool(pb_Constant.pbForceGridPivot))
				pb.CenterPivot( indicesToCenterPivot == null ? new int[1]{0} : indicesToCenterPivot );
			else
				pb.CenterPivot(indicesToCenterPivot);

			if(pb_ProGridsInterface.SnapEnabled())
				pb.transform.position = pb_Snap.SnapValue(pb.transform.position, pb_ProGridsInterface.SnapValue());
			else
			if(pb_PreferencesInternal.GetBool(pb_Constant.pbForceVertexPivot))
				pb.transform.position = pb_Snap.SnapValue(pb.transform.position, 1f);

			pb.Optimize();
		}

		/**
		 * Returns the last active SceneView window, or creates a new one if no last SceneView is found.
		 */
		public static SceneView GetSceneView()
		{
			return SceneView.lastActiveSceneView == null ? EditorWindow.GetWindow<SceneView>() : SceneView.lastActiveSceneView;
		}

		static SceneView.OnSceneFunc onPreSceneGuiDelegate
		{
			get
			{
				var fi = typeof(SceneView).GetField("onPreSceneGUIDelegate", k_BindingFlagsAll);
				return fi != null ? fi.GetValue(null) as SceneView.OnSceneFunc : null;
			}

			set
			{
				var fi = typeof(SceneView).GetField("onPreSceneGUIDelegate", k_BindingFlagsAll);

				if (fi != null)
					fi.SetValue(null, value);
			}
		}

		public static void RegisterOnPreSceneGUIDelegate(SceneView.OnSceneFunc func)
		{
			var del = onPreSceneGuiDelegate;

			if (del == null)
				onPreSceneGuiDelegate = func;
			else
				del += func;
		}

		public static void UnregisterOnPreSceneGUIDelegate(SceneView.OnSceneFunc func)
		{
			var del = onPreSceneGuiDelegate;

			if (del != null)
				del -= func;
		}

		/**
		 *	Is this code running on a Unix OS?
		 *
		 *	Alt summary: Do you know this?
		 */
		public static bool IsUnix()
		{
			System.PlatformID platform = System.Environment.OSVersion.Platform;
			return 	platform == System.PlatformID.MacOSX ||
					platform == System.PlatformID.Unix ||
					(int)platform == 128;
		}

		/**
		 *	CreateCachedEditor didn't exist until 5.0, so recreate it's contents if necessary or pass it on.
		 */
		public static void CreateCachedEditor<T>(UnityEngine.Object[] targetObjects, ref UnityEditor.Editor previousEditor) where T : UnityEditor.Editor
		{
			#if UNITY_4_7
			if (previousEditor != null && pbUtil.IsEqual(previousEditor.targets, targetObjects) )
				return;

			if (previousEditor != null)
				UnityEngine.Object.DestroyImmediate(previousEditor);

			previousEditor = Editor.CreateEditor(targetObjects, typeof(T));
			#else
			UnityEditor.Editor.CreateCachedEditor(targetObjects, typeof(T), ref previousEditor);
			#endif
		}
	}
}

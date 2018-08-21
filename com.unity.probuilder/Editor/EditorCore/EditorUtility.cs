#pragma warning disable 0168

using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Reflection;
using UnityEditor.ProBuilder.Actions;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.Rendering;
using UObject = UnityEngine.Object;

namespace UnityEditor.ProBuilder
{
	/// <summary>
	/// Utilities for working in Unity editor.
	/// </summary>
	public static class EditorUtility
	{
		const float k_DefaultNotificationDuration = 1f;
		static float s_NotificationTimer = 0f;
		static EditorWindow s_NotificationWindow;
		static bool s_IsNotificationDisplayed = false;

		const BindingFlags k_BindingFlagsAll =
			BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		/// <value>
		/// Subscribe to this delegate to be notified when a new mesh has been created and initialized through ProBuilder.
		/// </value>
		/// <remarks>
		/// This is only called when an object is initialized in editor, and created by ProBuilder menu items.
		/// </remarks>
		public static event Action<ProBuilderMesh> meshCreated = null;

		/// <summary>
		/// Set the selected render state for an object. In Unity 5.4 and lower, this just toggles wireframe on or off.
		/// </summary>
		/// <param name="renderer"></param>
		/// <param name="state"></param>
		internal static void SetSelectionRenderState(Renderer renderer, SelectionRenderState state)
		{
			UnityEditor.EditorUtility.SetSelectedRenderState(renderer, (EditorSelectedRenderState) state );
		}

		internal static SelectionRenderState GetSelectionRenderState()
		{
			bool wireframe = false, outline = false;

			try {
				wireframe = (bool) ReflectionUtility.GetValue(null, "UnityEditor.AnnotationUtility", "showSelectionWire");
				outline = (bool) ReflectionUtility.GetValue(null, "UnityEditor.AnnotationUtility", "showSelectionOutline");
			} catch {
				Log.Warning("Looks like Unity changed the AnnotationUtility \"showSelectionOutline\"\nPlease email contact@procore3d.com and let Karl know!");
			}

			SelectionRenderState state = SelectionRenderState.None;

			if(wireframe) state |= SelectionRenderState.Wireframe;
			if(outline) state |= SelectionRenderState.Outline;

			return state;
		}

		/// <summary>
		/// Show a timed (1 second) notification in the SceneView window.
		/// </summary>
		/// <param name="message">The text to display in the notification.</param>
		/// <seealso cref="RemoveNotification"/>
		internal static void ShowNotification(string message)
		{
			SceneView scnview = SceneView.lastActiveSceneView;
			if(scnview == null)
				scnview = EditorWindow.GetWindow<SceneView>();

			ShowNotification(scnview, message);
		}

		/// <inheritdoc cref="ShowNotification(string)"/>
		/// <param name="window">The EditorWindow to display this notification in.</param>
		/// <param name="message">The text to display in the notification.</param>
		/// <exception cref="ArgumentNullException">Window is null.</exception>
		internal static void ShowNotification(EditorWindow window, string message)
		{
			if(PreferencesInternal.HasKey(PreferenceKeys.pbShowEditorNotifications) && !PreferencesInternal.GetBool(PreferenceKeys.pbShowEditorNotifications))
				return;

            if (window == null)
                throw new ArgumentNullException("window");

			window.ShowNotification(new GUIContent(message, ""));
			window.Repaint();

			if(EditorApplication.update != NotifUpdate)
				EditorApplication.update += NotifUpdate;

			s_NotificationTimer = Time.realtimeSinceStartup + k_DefaultNotificationDuration;
			s_NotificationWindow = window;
			s_IsNotificationDisplayed = true;
		}

		/// <summary>
		/// Remove any currently displaying notifications from an <see cref="UnityEditor.EditorWindow"/>.
		/// </summary>
		/// <param name="window">The EditorWindow from which all currently displayed notifications will be removed.</param>
		/// <exception cref="ArgumentNullException">Thrown if window is null.</exception>
		internal static void RemoveNotification(EditorWindow window)
		{
            if (window == null)
                throw new ArgumentNullException("window");

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

		internal static bool IsPrefab(ProBuilderMesh mesh)
		{
#if UNITY_2018_3_OR_NEWER
			return PrefabUtility.GetPrefabAssetType(mesh.gameObject) != PrefabAssetType.NotAPrefab;
#else
			PrefabType type = PrefabUtility.GetPrefabType(mesh.gameObject);
			return type == PrefabType.Prefab || type == PrefabType.PrefabInstance || type == PrefabType.DisconnectedPrefabInstance;
#endif
		}

		/// <summary>
		/// Returns true if this object is a prefab instanced in the scene.
		/// </summary>
		/// <param name="go"></param>
		/// <returns></returns>
		internal static bool IsPrefabInstance(GameObject go)
		{
#if UNITY_2018_3_OR_NEWER
			var status = PrefabUtility.GetPrefabInstanceStatus(go);
			return status == PrefabInstanceStatus.Connected || status == PrefabInstanceStatus.Disconnected;
#else
			return PrefabUtility.GetPrefabType(go) == PrefabType.PrefabInstance;
#endif
		}

		/**
		 * Returns true if this object is a prefab in the Project view.
		 */
		internal static bool IsPrefabAsset(GameObject go)
		{
#if UNITY_2018_3_OR_NEWER
			return PrefabUtility.IsPartOfPrefabAsset(go);
#else
			return PrefabUtility.GetPrefabType(go) == PrefabType.Prefab;
#endif
		}

		/**
		 *	Returns true if Asset Store window is open, false otherwise.
		 */
		internal static bool AssetStoreWindowIsOpen()
		{
			return Resources.FindObjectsOfTypeAll<EditorWindow>().Any(x => x.GetType().ToString().Contains("AssetStoreWindow"));
		}

		/// <summary>
		/// Ensure that this object has a valid mesh reference, and the geometry is current. If it is not valid, this function will attempt to repair the sync state.
		/// </summary>
		/// <param name="mesh">The component to test.</param>
		/// <seealso cref="ProBuilderMesh.meshSyncState"/>
		public static void SynchronizeWithMeshFilter(ProBuilderMesh mesh)
		{
            if (mesh == null)
                throw new ArgumentNullException("mesh");

		 	Mesh oldMesh = mesh.mesh;
	 		MeshSyncState reason = mesh.meshSyncState;
			bool meshesAreAssets = PreferencesInternal.GetBool(PreferenceKeys.pbMeshesAreAssets);

			if( reason != MeshSyncState.None )
			{
				if (reason == MeshSyncState.Null)
				{
					mesh.Rebuild();
					mesh.Optimize();
				}
				else
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

					UnityEngine.Object dup = UnityEditor.EditorUtility.InstanceIDToObject(meshNo);
					GameObject go = dup as GameObject;

					if(go == null)
					{
						// Debug.Log("scene reloaded - false positive.");
						mesh.mesh.name = "pb_Mesh" + mesh.id;
					}
					else
					{
						// Debug.Log("duplicate mesh");

						if(!meshesAreAssets || !(EditorUtility.IsPrefabAsset(mesh.gameObject) || IsPrefabInstance(mesh.gameObject)))
						{
							// deep copy arrays & ToMesh/Refresh
							mesh.MakeUnique();
							mesh.Optimize();
						}
					}
				}
				else
				{
					// old mesh didn't exist, so this is probably a prefab being instanced

					if(EditorUtility.IsPrefabAsset(mesh.gameObject))
						mesh.mesh.hideFlags = (HideFlags) (1 | 2 | 4 | 8);

					mesh.Optimize();
				}
			}
			else
			{
				if(meshesAreAssets)
					EditorMeshUtility.TryCacheMesh(mesh);
			}
		}

		/// <summary>
		/// Returns true if GameObject contains flags.
		/// </summary>
		/// <param name="go"></param>
		/// <param name="flags"></param>
		/// <returns></returns>
		internal static bool HasStaticFlag(this GameObject go, StaticEditorFlags flags)
		{
			return (GameObjectUtility.GetStaticEditorFlags(go) & flags) == flags;
		}

		/// <summary>
		/// Initialize this object with the various editor-only parameters, and invoke the object creation callback.
		/// </summary>
		/// <param name="pb"></param>
		internal static void InitObject(ProBuilderMesh pb)
		{
			ShadowCastingMode scm = PreferencesInternal.GetEnum<ShadowCastingMode>(PreferenceKeys.pbShadowCastingMode);
			pb.GetComponent<MeshRenderer>().shadowCastingMode = scm;
			ScreenCenter(pb.gameObject);

			var flags = PreferencesInternal.HasKey(PreferenceKeys.pbDefaultStaticFlags)
				? PreferencesInternal.GetEnum<StaticEditorFlags>(PreferenceKeys.pbDefaultStaticFlags)
				: StaticEditorFlags.LightmapStatic |
				  StaticEditorFlags.OccluderStatic |
				  StaticEditorFlags.OccludeeStatic |
				  StaticEditorFlags.BatchingStatic |
				  StaticEditorFlags.NavigationStatic |
				  StaticEditorFlags.OffMeshLinkGeneration |
				  StaticEditorFlags.ReflectionProbeStatic;

			GameObjectUtility.SetStaticEditorFlags(pb.gameObject, flags);

			switch(PreferencesInternal.GetEnum<ColliderType>(PreferenceKeys.pbDefaultCollider))
			{
				case ColliderType.BoxCollider:
					pb.gameObject.AddComponent<BoxCollider>();
					break;

				case ColliderType.MeshCollider:
					pb.gameObject.AddComponent<MeshCollider>().convex = PreferencesInternal.GetBool(PreferenceKeys.pbForceConvex, false);
					break;
			}

			pb.Optimize();

			if( meshCreated != null )
				meshCreated(pb);
		}

		/**
		 * Puts the selected gameObject at the pivot point of the SceneView camera.
		 */
		internal static void ScreenCenter(GameObject _gameObject)
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
		internal static Vector3 ScenePivot()
		{
			return GetSceneView().pivot;
		}

		/// <summary>
		/// Set the pivot point of a mesh.
		/// </summary>
		/// <param name="mesh"></param>
		/// <param name="vertexes">If any values are passed here, the pivot is set to an average of all vertexes at indexes. If null, the first vertex is used as the pivot.</param>
		internal static void SetPivotAndSnapWithPref(ProBuilderMesh mesh, int[] vertexes)
		{
			if(PreferencesInternal.GetBool(PreferenceKeys.pbForceGridPivot))
				mesh.CenterPivot( vertexes == null ? new int[1]{0} : vertexes );
			else
				mesh.CenterPivot(vertexes);

			if(ProGridsInterface.SnapEnabled())
				mesh.transform.position = Snapping.SnapValue(mesh.transform.position, ProGridsInterface.SnapValue());
			else
			if(PreferencesInternal.GetBool(PreferenceKeys.pbForceVertexPivot))
				mesh.transform.position = Snapping.SnapValue(mesh.transform.position, 1f);

			mesh.Optimize();
		}

		/**
		 * Returns the last active SceneView window, or creates a new one if no last SceneView is found.
		 */
		internal static SceneView GetSceneView()
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

		internal static void RegisterOnPreSceneGUIDelegate(SceneView.OnSceneFunc func)
		{
			var del = onPreSceneGuiDelegate;

			if (del == null)
				onPreSceneGuiDelegate = func;
			else
				del += func;
		}

		internal static void UnregisterOnPreSceneGUIDelegate(SceneView.OnSceneFunc func)
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
		internal static bool IsUnix()
		{
			System.PlatformID platform = System.Environment.OSVersion.Platform;
			return 	platform == System.PlatformID.MacOSX ||
					platform == System.PlatformID.Unix ||
					(int)platform == 128;
		}

		/**
		 *	CreateCachedEditor didn't exist until 5.0, so recreate it's contents if necessary or pass it on.
		 */
		internal static void CreateCachedEditor<T>(UnityEngine.Object[] targetObjects, ref UnityEditor.Editor previousEditor) where T : UnityEditor.Editor
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

		internal static SelectMode GetSelectMode(EditLevel edit, ComponentMode component)
		{
			switch (edit)
			{
				case EditLevel.Top:
					return SelectMode.Object;

				case EditLevel.Geometry:
					{
						switch (component)
						{
							case ComponentMode.Vertex:
								return SelectMode.Vertex;
							case ComponentMode.Edge:
								return SelectMode.Edge;
							default:
								return SelectMode.Face;
						}
					}

				case EditLevel.Texture:
					return SelectMode.Texture;

				default:
					return SelectMode.None;
			}
		}

		internal static EditLevel GetEditLevel(SelectMode mode)
		{
			switch (mode)
			{
				case SelectMode.Object:
					return EditLevel.Top;
				case SelectMode.Texture:
					return EditLevel.Texture;
				case SelectMode.None:
					return EditLevel.Plugin;
				default:
					return EditLevel.Geometry;
			}
		}

		internal static ComponentMode GetComponentMode(SelectMode mode)
		{
			switch (mode)
			{
				case SelectMode.Vertex:
					return ComponentMode.Vertex;
				case SelectMode.Edge:
					return ComponentMode.Edge;
				default:
					return ComponentMode.Face;
			}
		}
	}
}

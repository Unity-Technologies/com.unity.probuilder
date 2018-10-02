#pragma warning disable 0168

using UnityEngine;
using System.Linq;
using System;
using System.Reflection;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.Rendering;
using UObject = UnityEngine.Object;
using UnityEditor.SettingsManagement;

namespace UnityEditor.ProBuilder
{
	/// <summary>
	/// Utilities for working in Unity editor.
	/// </summary>
	public static class EditorUtility
	{
		internal enum PivotLocation
		{
			Center,
			FirstVertex
		}

		const float k_DefaultNotificationDuration = 1f;
		static float s_NotificationTimer = 0f;
		static EditorWindow s_NotificationWindow;
		static bool s_IsNotificationDisplayed = false;

		const BindingFlags k_BindingFlagsAll =
			BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		[UserSetting("General", "Show Action Notifications", "Enable or disable notification popups when performing actions.")]
		static Pref<bool> s_ShowNotifications = new Pref<bool>("editor.showEditorNotifications", false);

		[UserSetting("Mesh Settings", "Static Editor Flags", "Default static flags to apply to new shapes.")]
		static Pref<StaticEditorFlags> s_StaticEditorFlags = new Pref<StaticEditorFlags>("mesh.defaultStaticEditorFlags", 0);

		[UserSetting("Mesh Settings", "Material", "The default material to be applied to newly created shapes.")]
		static Pref<Material> s_DefaultMaterial = new Pref<Material>("mesh.userMaterial", null);

		[UserSetting("Mesh Settings", "Mesh Collider is Convex", "If a MeshCollider is set as the default collider component, this sets the convex setting.")]
		static Pref<bool> s_MeshColliderIsConvex = new Pref<bool>("mesh.meshColliderIsConvex", false);

		[UserSetting("Mesh Settings", "Pivot Location", "Determines the placement of new shape's pivot.")]
		static Pref<PivotLocation> s_NewShapesPivotAtVertex = new Pref<PivotLocation>("mesh.newShapePivotLocation", PivotLocation.FirstVertex);

		[UserSetting("Mesh Settings", "Pivot on Vertex", "When enabled, new shapes will have their pivot point set to a vertex instead of the center.")]
		static Pref<bool> s_SnapNewShapesToGrid = new Pref<bool>("mesh.newShapesSnapToGrid", true);

		[UserSetting("Mesh Settings", "Shadow Casting Mode", "The default ShadowCastingMode to apply to MeshRenderer components.")]
		static Pref<ShadowCastingMode> s_ShadowCastingMode = new Pref<ShadowCastingMode>("mesh.shadowCastingMode", ShadowCastingMode.On);

		[UserSetting("Mesh Settings", "Collider Type", "What type of Collider to apply to new Shapes.")]
		static Pref<ColliderType> s_ColliderType = new Pref<ColliderType>("mesh.newShapeColliderType", ColliderType.MeshCollider);

		[UserSetting]
		static Pref<bool> s_ExperimentalFeatures = new Pref<bool>("experimental.featuresEnabled", false, SettingScope.User);

		[UserSetting]
		static Pref<bool> s_MeshesAreAssets = new Pref<bool>("experimental.meshesAreAssets", false, SettingScope.Project);

		internal static bool meshesAreAssets
		{
			get { return s_ExperimentalFeatures && s_MeshesAreAssets; }
		}

		internal static bool experimentalFeaturesEnabled
		{
			get { return s_ExperimentalFeatures; }
		}

		[UserSettingBlock("Experimental", new[] { "store", "mesh", "asset", "experimental", "features", "enabled" })]
		static void ExperimentalFeaturesSettings(string searchContext)
		{
			s_ExperimentalFeatures.value = SettingsGUILayout.SettingsToggle("Experimental Features Enabled", s_ExperimentalFeatures, searchContext);

			if (s_ExperimentalFeatures.value)
			{
				using (new UI.EditorStyles.IndentedBlock())
				{
					s_MeshesAreAssets.value = SettingsGUILayout.SettingsToggle("Store Mesh as Asset", s_MeshesAreAssets, searchContext);
				}
			}
		}

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

		internal static void ShowNotification(ActionResult result)
		{
			ShowNotification(result.notification);
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
			if (!s_ShowNotifications)
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
			bool meshesAreAssets = EditorUtility.s_MeshesAreAssets;

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
			SetPivotLocationAndSnap(pb);

			pb.renderer.shadowCastingMode = s_ShadowCastingMode;
			pb.renderer.sharedMaterial = GetUserMaterial();

			ScreenCenter(pb.gameObject);

			GameObjectUtility.SetStaticEditorFlags(pb.gameObject, s_StaticEditorFlags);

			switch(s_ColliderType.value)
			{
				case ColliderType.BoxCollider:
					pb.gameObject.AddComponent<BoxCollider>();
					break;

				case ColliderType.MeshCollider:
					pb.gameObject.AddComponent<MeshCollider>().convex = s_MeshColliderIsConvex;
					break;
			}

			pb.unwrapParameters = new UnwrapParameters(Lightmapping.s_UnwrapParameters);

			pb.Optimize();

			if( meshCreated != null )
				meshCreated(pb);
		}

		internal static void SetPivotLocationAndSnap(ProBuilderMesh mesh)
		{
			switch (s_NewShapesPivotAtVertex.value)
			{
				case PivotLocation.Center:
					mesh.CenterPivot(null);
					break;

				case PivotLocation.FirstVertex:
					mesh.CenterPivot(new int[1] { 0 });
					break;
			}

			if (ProGridsInterface.SnapEnabled())
				mesh.transform.position = Snapping.SnapValue(mesh.transform.position, ProGridsInterface.SnapValue());
			else if (s_SnapNewShapesToGrid)
				mesh.transform.position = Snapping.SnapValue(mesh.transform.position, new Vector3(
					EditorPrefs.GetFloat("MoveSnapX"),
					EditorPrefs.GetFloat("MoveSnapY"),
					EditorPrefs.GetFloat("MoveSnapZ")));

			mesh.Optimize();
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

		/// <summary>
		/// Is this mode one of the mesh element modes (vertex, edge, face, texture).
		/// </summary>
		/// <param name="mode"></param>
		/// <returns></returns>
		internal static bool IsMeshElementMode(this SelectMode mode)
		{
			return mode.ContainsFlag(SelectMode.Vertex | SelectMode.Edge | SelectMode.Face | SelectMode.TextureFace);
		}

		// HasFlag doesn't exist in .NET 3.5
		internal static bool ContainsFlag(this SelectMode target, SelectMode value)
		{
			return (target & value) != SelectMode.None;
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
					return SelectMode.TextureFace;

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
				case SelectMode.TextureFace:
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

		internal static Material GetUserMaterial()
		{
			var mat = (Material) s_DefaultMaterial;

			if (mat != null)
				return mat;

			return BuiltinMaterials.defaultMaterial;
		}
	}
}

#pragma warning disable 0168

using UnityEngine;
using System.Linq;
using System;

using UnityEngine.ProBuilder;
using UnityEngine.Rendering;
using UObject = UnityEngine.Object;
using UnityEditor.SettingsManagement;
using UnityEditorInternal;
using UnityEngine.SceneManagement;
#if !UNITY_2019_1_OR_NEWER
using System.Reflection;
#endif

#if UNITY_2021_2_OR_NEWER
using PrefabStageUtility = UnityEditor.SceneManagement.PrefabStageUtility;
#else
using PrefabStageUtility = UnityEditor.Experimental.SceneManagement.PrefabStageUtility;
#endif

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Utilities for working in Unity editor.
    /// </summary>
    public static class EditorUtility
    {
        const float k_DefaultNotificationDuration = 1f;
        static float s_NotificationTimer;
        static EditorWindow s_NotificationWindow;
        static bool s_IsNotificationDisplayed;

        [UserSetting("General", "Show Action Notifications", "Enable or disable notification popups when performing actions.")]
        static Pref<bool> s_ShowNotifications = new Pref<bool>("editor.showEditorNotifications", false);

        [UserSetting("Mesh Settings", "Static Editor Flags", "Default static flags to apply to new shapes.")]
        static Pref<StaticEditorFlags> s_StaticEditorFlags = new Pref<StaticEditorFlags>("mesh.defaultStaticEditorFlags", 0);

        [UserSetting("Mesh Settings", "Mesh Collider is Convex", "If a MeshCollider is set as the default collider component, this sets the convex setting.")]
        static Pref<bool> s_MeshColliderIsConvex = new Pref<bool>("mesh.meshColliderIsConvex", false);

        [UserSetting("Mesh Settings", "Pivot Location", "Determines the placement of new shape's pivot.")]
        static Pref<PivotLocation> s_NewShapesPivotAtCenter = new Pref<PivotLocation>("mesh.newShapePivotLocation", PivotLocation.Center);

        [UserSetting("Mesh Settings", "Snap New Shape To Grid", "When enabled, new shapes will snap to the closest point on grid.")]
        static Pref<bool> s_SnapNewShapesToGrid = new Pref<bool>("mesh.newShapesSnapToGrid", true);

        [UserSetting("Mesh Settings", "Shadow Casting Mode", "The default ShadowCastingMode to apply to MeshRenderer components.")]
        static Pref<ShadowCastingMode> s_ShadowCastingMode = new Pref<ShadowCastingMode>("mesh.shadowCastingMode", ShadowCastingMode.On);

        [UserSetting("Mesh Settings", "Collider Type", "What type of Collider to apply to new Shapes.")]
        static Pref<ColliderType> s_ColliderType = new Pref<ColliderType>("mesh.newShapeColliderType", ColliderType.MeshCollider);

        internal static PivotLocation newShapePivotLocation
        {
            get { return s_NewShapesPivotAtCenter; }
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
        internal static void SetSelectionRenderState(Renderer renderer, EditorSelectedRenderState state)
        {
            UnityEditor.EditorUtility.SetSelectedRenderState(renderer, state);
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
            if (scnview == null)
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

            if (EditorApplication.update != NotifUpdate)
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
            if (s_IsNotificationDisplayed && Time.realtimeSinceStartup > s_NotificationTimer)
            {
                s_IsNotificationDisplayed = false;
                RemoveNotification(s_NotificationWindow);
            }
        }

        internal static bool IsPrefab(ProBuilderMesh mesh)
        {
            return PrefabUtility.GetPrefabAssetType(mesh.gameObject) != PrefabAssetType.NotAPrefab;
        }

        /// <summary>
        /// Returns true if this object is a prefab instanced in the scene.
        /// </summary>
        /// <param name="go"></param>
        /// <returns></returns>
        internal static bool IsPrefabInstance(GameObject go)
        {
            var status = PrefabUtility.GetPrefabInstanceStatus(go);
            return status == PrefabInstanceStatus.Connected || status == PrefabInstanceStatus.Disconnected;
        }

        /**
         * Returns true if this object is a prefab in the Project view.
         */
        internal static bool IsPrefabAsset(GameObject go)
        {
            return PrefabUtility.IsPartOfPrefabAsset(go);
        }

        /**
         *  Returns true if Asset Store window is open, false otherwise.
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

            mesh.EnsureMeshFilterIsAssigned();
            mesh.EnsureMeshColliderIsAssigned();
            MeshSyncState state = mesh.meshSyncState;
            bool meshesAreAssets = Experimental.meshesAreAssets;

            if (state != MeshSyncState.InSync)
            {
                Mesh oldMesh;

                if (state == MeshSyncState.Null)
                {
                    mesh.Rebuild();
                    mesh.Optimize();
                }
                else
                // If the mesh ID doesn't match the gameObject Id, it could mean two things:
                //   1. The object was just duplicated, and then made unique
                //   2. The scene was reloaded, and gameObject ids were recalculated.
                // If (2) we need to clean up the old mesh. If the (1) the old mesh needs to *not* be destroyed.
                if ((oldMesh = mesh.mesh) != null)
                {
                    int meshNo = -1;
                    int.TryParse(oldMesh.name.Replace("pb_Mesh", ""), out meshNo);

                    UnityEngine.Object dup = UnityEditor.EditorUtility.InstanceIDToObject(meshNo);
                    GameObject go = dup as GameObject;

                    // Scene reload, just rename the mesh to the correct ID
                    if (go == null)
                    {
                        mesh.mesh.name = "pb_Mesh" + mesh.id;
                    }
                    else
                    {
                        // Mesh was duplicated, need to instantiate a unique mesh asset
                        if (!meshesAreAssets || !(IsPrefabAsset(mesh.gameObject) || IsPrefabInstance(mesh.gameObject)))
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
                    if (IsPrefabAsset(mesh.gameObject))
                        mesh.mesh.hideFlags = (HideFlags)(1 | 2 | 4 | 8);

                    mesh.Optimize();
                }
            }
            else
            {
                if (meshesAreAssets)
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
        /// Move a GameObject to the proper active root.
        /// Checks if a default parent exists, otherwise it adds the object as a root of the active scene, which can be a prefab stage.
        /// </summary>
        /// <param name="gameObject"></param>
        internal static void MoveToActiveRoot(GameObject gameObject)
        {
#if UNITY_2020_2_OR_NEWER
            var parent = SceneView.GetDefaultParentObjectIfSet();
            if (parent != null)
            {
                gameObject.transform.SetParent(parent);
                return;
            }
#endif
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            var activeScene = SceneManager.GetActiveScene();

            if (prefabStage != null)
            {
                if (gameObject.scene != prefabStage.scene)
                {
                    SceneManager.MoveGameObjectToScene(gameObject, prefabStage.scene);

                    // Prefabs cannot have multiple roots
                    gameObject.transform.SetParent(prefabStage.prefabContentsRoot.transform, true);
                }
            }
            else if(gameObject.scene != activeScene)
            {
                gameObject.transform.SetParent(null);
                SceneManager.MoveGameObjectToScene(gameObject, activeScene);
            }
        }

        /// <summary>
        /// Initialize this object with the various editor-only parameters, and invoke the object creation callback.
        /// </summary>
        /// <param name="pb"></param>
        internal static void InitObject(ProBuilderMesh pb)
        {
            MoveToActiveRoot(pb.gameObject);

            GameObjectUtility.EnsureUniqueNameForSibling(pb.gameObject);
            ScreenCenter(pb.gameObject);
            SnapInstantiatedObject(pb);

#if UNITY_2019_1_OR_NEWER
            ComponentUtility.MoveComponentRelativeToComponent(pb, pb.transform, false);
#endif
            pb.renderer.shadowCastingMode = s_ShadowCastingMode;
            pb.renderer.sharedMaterial = EditorMaterialUtility.GetUserMaterial();

            GameObjectUtility.SetStaticEditorFlags(pb.gameObject, s_StaticEditorFlags);

            switch (s_ColliderType.value)
            {
                case ColliderType.BoxCollider:
                    if(!pb.gameObject.TryGetComponent<BoxCollider>(out _))
                        Undo.AddComponent(pb.gameObject, typeof(BoxCollider));
                    break;

                case ColliderType.MeshCollider:
                    MeshCollider collider;
                    if (!pb.gameObject.TryGetComponent<MeshCollider>(out collider))
                        collider = Undo.AddComponent<MeshCollider>(pb.gameObject);
                    // This little dance is required to prevent the Prefab system from detecting an overridden property
                    // before ProBuilderMesh.RefreshCollisions has a chance to mark the MeshCollider.sharedMesh property
                    // as driven. "AddComponent<MeshCollider>" constructs the MeshCollider and simultaneously assigns
                    // the "m_Mesh" property, marking the property dirty. So we undo that change here, then assign the
                    // mesh through our own method.
                    collider.sharedMesh = null;
                    collider.convex = s_MeshColliderIsConvex;
                    pb.Refresh(RefreshMask.Collisions);
                    break;
            }

            pb.unwrapParameters = new UnwrapParameters(Lightmapping.s_UnwrapParameters);

            pb.Optimize();

            if (meshCreated != null)
                meshCreated(pb);
        }

        // If s_SnapNewShapesToGrid is enabled, always snap to the grid size. If it is not, use the active snap  settings
        internal static void SnapInstantiatedObject(ProBuilderMesh mesh)
        {
            mesh.transform.position = ProBuilderSnapping.Snap(
                mesh.transform.position,
                s_SnapNewShapesToGrid
                    ? EditorSnapping.worldSnapMoveValue
                    : EditorSnapping.activeMoveSnapValue);
        }

        /**
         * Puts the selected gameObject at the pivot point of the SceneView camera.
         */
        internal static void ScreenCenter(GameObject _gameObject)
        {
            if (_gameObject == null)
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

#if !UNITY_2019_1_OR_NEWER
        const BindingFlags k_BindingFlagsAll = BindingFlags.NonPublic
            | BindingFlags.Public
            | BindingFlags.Instance
            | BindingFlags.Static;

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
#endif

        internal static bool IsUnix()
        {
            System.PlatformID platform = System.Environment.OSVersion.Platform;
            return platform == System.PlatformID.MacOSX ||
                platform == System.PlatformID.Unix ||
                (int)platform == 128;
        }

        /// <summary>
        /// Is this mode one of the mesh element modes (vertex, edge, face, texture).
        /// </summary>
        /// <param name="mode"></param>
        /// <returns></returns>
        internal static bool IsMeshElementMode(this SelectMode mode)
        {
            return mode.ContainsFlag(
                SelectMode.Vertex
                | SelectMode.Edge
                | SelectMode.Face
                | SelectMode.TextureEdge
                | SelectMode.TextureFace
                | SelectMode.TextureVertex
                );
        }

        internal static bool IsTextureMode(this SelectMode mode)
        {
            return mode.ContainsFlag(
                SelectMode.TextureEdge
                | SelectMode.TextureFace
                | SelectMode.TextureVertex
                );
        }

        internal static bool IsPositionMode(this SelectMode mode)
        {
            return mode.ContainsFlag(
                SelectMode.Edge
                | SelectMode.Face
                | SelectMode.Vertex
                );
        }

        internal static SelectMode GetPositionMode(this SelectMode mode)
        {
            if (mode.ContainsFlag(SelectMode.TextureFace))
                mode = (mode & ~SelectMode.TextureFace) | SelectMode.Face;

            if (mode.ContainsFlag(SelectMode.TextureEdge))
                mode = (mode & ~SelectMode.TextureEdge) | SelectMode.Edge;

            if (mode.ContainsFlag(SelectMode.TextureVertex))
                mode = (mode & ~SelectMode.TextureVertex) | SelectMode.Vertex;

            return mode;
        }

        internal static SelectMode GetTextureMode(this SelectMode mode)
        {
            if (mode.ContainsFlag(SelectMode.Face))
                mode = (mode & ~SelectMode.Face) | SelectMode.TextureFace;

            if (mode.ContainsFlag(SelectMode.Edge))
                mode = (mode & ~SelectMode.Edge) | SelectMode.TextureEdge;

            if (mode.ContainsFlag(SelectMode.Vertex))
                mode = (mode & ~SelectMode.Vertex) | SelectMode.TextureVertex;

            return mode;
        }

        /// <summary>
        /// Test if SelectMode contains any of the value bits.
        /// </summary>
        /// <remarks>
        /// HasFlag doesn't exist in .NET 3.5
        /// </remarks>
        /// <param name="target"></param>
        /// <param name="value"></param>
        /// <returns></returns>
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

        internal static bool IsDeveloperMode()
        {
            return EditorPrefs.GetBool("DeveloperMode", false);
        }

        public static void SetGizmoIconEnabled(Type script, bool enabled)
        {
#if UNITY_2019_1_OR_NEWER
            var annotations = AnnotationUtility.GetAnnotations();
            var annotation = annotations.FirstOrDefault(x => x.scriptClass.Contains(script.Name));
            AnnotationUtility.SetIconEnabled(annotation.classID, annotation.scriptClass, enabled ? 1 : 0);
#else
            Type annotationUtility = typeof(Editor).Assembly.GetType("UnityEditor.AnnotationUtility");
            MethodInfo setGizmoIconEnabled = annotationUtility.GetMethod("SetIconEnabled",
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                new Type[] { typeof(int), typeof(string), typeof(int) },
                null);
            var name = script.Name;
            setGizmoIconEnabled.Invoke(null, new object[] { 114, name, enabled ? 1 : 0});
#endif
        }
    }
}

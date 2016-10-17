#pragma warning disable 0168

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.IO;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
#if !UNITY_4_7
using UnityEngine.Rendering;
#endif

#if PB_DEBUG
using Parabox.Debug;
#endif

namespace ProBuilder2.EditorCommon
{
	/**
	 * Utilities for working in Unity editor.  Showing notifications in windows, getting the sceneview,
	 * setting EntityTypes, OBJ export, etc.
	 */
	public static class pb_EditorUtility
	{
		const float TIMER_DISPLAY_TIME = 1f;
		private static float notifTimer = 0f;
		private static EditorWindow notifWindow;
		private static bool notifDisplayed = false;

		public delegate void OnObjectCreated(pb_Object pb);

		/**
		 *	Subscribe to this delegate to be notified when a pb_Object has been created and initialized through ProBuilder.
		 *	Note that this is only called when an object is initialized, not just created.  Eg, pb_ShapeGenerator.GenerateCube(Vector3.one) won't
		 * 	fire this callback.
		 *
		 *	\sa pb_EditorUtility.InitObject
		 */
		public static OnObjectCreated onObjectCreated = null;

		/**
		 *	Add a listener to the multicast onObjectCreated delegate.
		 */
		public static void AddOnObjectCreatedListener(OnObjectCreated onProBuilderObjectCreated)
		{
			if(onObjectCreated == null)
				onObjectCreated = onProBuilderObjectCreated;
			else
				onObjectCreated += onProBuilderObjectCreated;
		}

		/**
		 *	Remove a listener from the onObjectCreated delegate.
		 */
		public static void RemoveOnObjectCreatedListener(OnObjectCreated onProBuilderObjectCreated)
		{
			if(onObjectCreated != null)
				onObjectCreated -= onProBuilderObjectCreated;
		}

		/**
		 *	Set the selected render state for an object.  In Unity 5.4 and lower, this just toggles wireframe 
		 *	on or off.
		 */
		public static void SetSelectionRenderState(Renderer renderer, SelectionRenderState state)
		{
			#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 ||UNITY_5_3 || UNITY_5_4
				EditorUtility.SetSelectedWireframeHidden(renderer, state == 0);
			#else
				EditorUtility.SetSelectedRenderState(renderer, (EditorSelectedRenderState) state ); 
			#endif
		}

		public static SelectionRenderState GetSelectionRenderState()
		{

			#if UNITY_4_6 || UNITY_4_7 || UNITY_5_0 || UNITY_5_1 || UNITY_5_2 ||UNITY_5_3 || UNITY_5_4

			return SelectionRenderState.Wireframe;

			#else

			bool wireframe = false, outline = false;

			try {			
				wireframe = (bool) pb_Reflection.GetValue(null, "UnityEditor.AnnotationUtility", "showSelectionWire");
				outline = (bool) pb_Reflection.GetValue(null, "UnityEditor.AnnotationUtility", "showSelectionOutline");
			} catch {
				Debug.LogWarning("Looks like Unity changed the AnnotationUtility \"showSelectionOutline\"\nPlease email contact@procore3d.com and let Karl know!");
			}

			SelectionRenderState state = SelectionRenderState.None;

			if(wireframe) state |= SelectionRenderState.Wireframe;
			if(outline) state |= SelectionRenderState.Outline;

			return state;

			#endif
		}

		/**
		 * Show a timed notification in the SceneView window.
		 */
		public static void ShowNotification(string notif)
		{
			SceneView scnview = SceneView.lastActiveSceneView;
			if(scnview == null)
				scnview = EditorWindow.GetWindow<SceneView>();

			ShowNotification(scnview, notif);
		}

		public static void ShowNotification(EditorWindow window, string notif)
		{
			if(EditorPrefs.HasKey(pb_Constant.pbShowEditorNotifications) && !EditorPrefs.GetBool(pb_Constant.pbShowEditorNotifications))
				return;

			window.ShowNotification(new GUIContent(notif, ""));
			window.Repaint();

			if(EditorApplication.update != NotifUpdate)
				EditorApplication.update += NotifUpdate;

			notifTimer = Time.realtimeSinceStartup + TIMER_DISPLAY_TIME;
			notifWindow = window;
			notifDisplayed = true;
		}

		public static void RemoveNotification(EditorWindow window)
		{
			EditorApplication.update -= NotifUpdate;

			window.RemoveNotification();
			window.Repaint();
		}

		private static void NotifUpdate()
		{
			if(notifDisplayed && Time.realtimeSinceStartup > notifTimer)
			{
				notifDisplayed = false;
				RemoveNotification(notifWindow);
			}
		}


		public static string ExportOBJ(pb_Object[] pb)
		{
			if(pb.Length < 1)
				return "";

			pb_Object combined;

			if(pb.Length > 1)
				pbMeshOps.CombineObjects(pb, out combined);
			else
				combined = pb[0];

			string path = EditorUtility.SaveFilePanel("Save ProBuilder Object as Obj", "", "pb" + pb[0].id, "obj");

			if(path == null || path == "")
			{
				if(pb.Length > 1)
				{
					GameObject.DestroyImmediate(combined.GetComponent<MeshFilter>().sharedMesh);
					GameObject.DestroyImmediate(combined.gameObject);
				}

				return "";
			}

			EditorObjExporter.MeshToFile(combined.GetComponent<MeshFilter>(), path);

			AssetDatabase.Refresh();

			if(pb.Length > 1)
			{
				GameObject.DestroyImmediate(combined.GetComponent<MeshFilter>().sharedMesh);
				GameObject.DestroyImmediate(combined.gameObject);
			}
			return path;
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
		 *	\brief Sets the EntityType for the passed gameObject.
		 *	@param newEntityType The type to set.
		 *	@param target The gameObject to apply the EntityType to.  Must contains pb_Object and pb_Entity components.  Method does contain null checks.
		 */
		public static void SetEntityType(EntityType newEntityType, GameObject target)
		{
			pb_Entity ent = target.GetComponent<pb_Entity>();

			if(ent == null)
				ent = target.AddComponent<pb_Entity>();

			pb_Object pb = target.GetComponent<pb_Object>();

			if(!ent || !pb)
				return;

			SetEditorFlags(StaticEditorFlags_All, target);

			switch(newEntityType)
			{
				case EntityType.Detail:
				case EntityType.Occluder:
					SetBrush(target);
					break;

				case EntityType.Trigger:
					SetTrigger(target);
					break;

				case EntityType.Collider:
					SetCollider(target);
					break;

				case EntityType.Mover:
					SetDynamic(target);
					break;
			}

			ent.SetEntity(newEntityType);
		}

		private static void SetBrush(GameObject target)
		{
			EntityType et = target.GetComponent<pb_Entity>().entityType;

			if(	et == EntityType.Trigger ||
				et == EntityType.Collider )
			{
				pb_Object pb = target.GetComponent<pb_Object>();

				#if !PROTOTYPE
				pb.SetFaceMaterial(pb.faces, pb_Constant.DefaultMaterial );
				#else
				target.GetComponent<MeshRenderer>().sharedMaterial = pb_Constant.DefaultMaterial;
				#endif

				pb.ToMesh();
				pb.Refresh();
			}
		}

		private static void SetDynamic(GameObject target)
		{
			EntityType et = target.GetComponent<pb_Entity>().entityType;

			SetEditorFlags((StaticEditorFlags)0, target);

			if(	et == EntityType.Trigger ||
				et == EntityType.Collider )
			{
				pb_Object pb = target.GetComponent<pb_Object>();

				#if !PROTOTYPE
					pb.SetFaceMaterial(pb.faces, pb_Constant.DefaultMaterial );
				#else
					target.GetComponent<MeshRenderer>().sharedMaterial = pb_Constant.DefaultMaterial;
				#endif

				pb.ToMesh();
				pb.Refresh();
			}
		}

		private static void SetTrigger(GameObject target)
		{
			pb_Object pb = target.GetComponent<pb_Object>();

			#if !PROTOTYPE
			pb.SetFaceMaterial(pb.faces, pb_Constant.TriggerMaterial );
			#else
			target.GetComponent<MeshRenderer>().sharedMaterial = pb_Constant.TriggerMaterial;
			#endif

			SetIsTrigger(true, target);
			SetEditorFlags((StaticEditorFlags)0, target);

			pb.ToMesh();
			pb.Refresh();
		}

		private static void SetCollider(GameObject target)
		{
			pb_Object pb = target.GetComponent<pb_Object>();

			#if !PROTOTYPE
			pb.SetFaceMaterial(pb.faces, pb_Constant.ColliderMaterial );
			#else
			target.GetComponent<MeshRenderer>().sharedMaterial = pb_Constant.ColliderMaterial;
			#endif

			pb.ToMesh();
			pb.Refresh();

			SetEditorFlags( (StaticEditorFlags)(StaticEditorFlags.NavigationStatic | StaticEditorFlags.OffMeshLinkGeneration), target);
		}

		private static void SetEditorFlags(StaticEditorFlags editorFlags, GameObject target)
		{
			GameObjectUtility.SetStaticEditorFlags(target, editorFlags);
		}

		private static void SetIsTrigger(bool val, GameObject target)
		{
			Collider[] colliders = pbUtil.GetComponents<Collider>(target);
			foreach(Collider col in colliders)
			{
				if(val && col is MeshCollider)
					((MeshCollider)col).convex = true;
				col.isTrigger = val;
			}
		}

		#if !UNITY_4_7
		const StaticEditorFlags StaticEditorFlags_All =
				StaticEditorFlags.LightmapStatic |
				StaticEditorFlags.OccluderStatic |
				StaticEditorFlags.BatchingStatic |
				StaticEditorFlags.OccludeeStatic |
				StaticEditorFlags.NavigationStatic |
				StaticEditorFlags.OffMeshLinkGeneration |
				StaticEditorFlags.ReflectionProbeStatic;
		#else
		const StaticEditorFlags StaticEditorFlags_All =
				StaticEditorFlags.LightmapStatic |
				StaticEditorFlags.OccluderStatic |
				StaticEditorFlags.BatchingStatic |
				StaticEditorFlags.OccludeeStatic |
				StaticEditorFlags.NavigationStatic |
				StaticEditorFlags.OffMeshLinkGeneration;
		#endif

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
			bool meshesAreAssets = pb_Preferences_Internal.GetBool(pb_Constant.pbMeshesAreAssets);

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

					Object dup = EditorUtility.InstanceIDToObject(meshNo);
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

		public static T LoadAssetAtPath<T>(string InPath) where T : UnityEngine.Object
		{
			return (T) AssetDatabase.LoadAssetAtPath(InPath, typeof(T));
		}

		[System.Obsolete("Please us InitObject(pb_Object pb) instead.")]
		public static void InitObjectFlags(pb_Object pb)
		{
			InitObject(pb);
		}

		[System.Obsolete("Please us InitObject instead.")]
		public static void InitObjectFlags(pb_Object pb, ColliderType colliderType, EntityType entityType)
		{
			InitObject(pb, colliderType, entityType);
		}

		/**
		 * \brief ProBuilder objects created in Editor need to be initialized with a number of additional Editor-only settings.
		 *	This method provides an easy method of doing so in a single call.  #InitObject will set the Entity Type, generate
		 *	a UV2 channel, set the unwrapping parameters, and center the object in the screen.
		 */
		public static void InitObject(pb_Object pb)
		{
			ColliderType col = pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider);
			EntityType et = pb_Preferences_Internal.GetEnum<EntityType>(pb_Constant.pbDefaultEntity);
			InitObject(pb, col, et);

			if( onObjectCreated != null )
				onObjectCreated(pb);
		}

		public static void InitObject(pb_Object pb, ColliderType colliderType, EntityType entityType)
		{
			switch(colliderType)
			{
				case ColliderType.BoxCollider:
					pb.gameObject.AddComponent<BoxCollider>();
				break;

				case ColliderType.MeshCollider:
					pb.gameObject.AddComponent<MeshCollider>().convex = EditorPrefs.HasKey(pb_Constant.pbForceConvex) ? EditorPrefs.GetBool(pb_Constant.pbForceConvex) : false;
					break;
			}

			#if !UNITY_4_7
			ShadowCastingMode scm = pb_Preferences_Internal.GetEnum<ShadowCastingMode>(pb_Constant.pbShadowCastingMode);
			pb.GetComponent<MeshRenderer>().shadowCastingMode = scm;
			#endif

			pb_EditorUtility.SetEntityType(entityType, pb.gameObject);
			pb_EditorUtility.ScreenCenter( pb.gameObject );
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
		 * If EditorPrefs say to set pivot to corner and ProGrids or PB pref says snap to grid, do it.
		 * @param indicesToCenterPivot If any values are passed here, the pivot is set to an average of all vertices at indices.  If null, the first vertex is used as the pivot.
		 */
		public static void SetPivotAndSnapWithPref(pb_Object pb, int[] indicesToCenterPivot)
		{
			if(pb_Preferences_Internal.GetBool(pb_Constant.pbForceGridPivot))
				pb.CenterPivot( indicesToCenterPivot == null ? new int[1]{0} : indicesToCenterPivot );
			else
				pb.CenterPivot(indicesToCenterPivot);

			if(pb_ProGrids_Interface.SnapEnabled())
				pb.transform.position = pbUtil.SnapValue(pb.transform.position, pb_ProGrids_Interface.SnapValue());
			else
			if(pb_Preferences_Internal.GetBool(pb_Constant.pbForceVertexPivot))
				pb.transform.position = pbUtil.SnapValue(pb.transform.position, 1f);
		}

		/**
		 * Returns the last active SceneView window, or creates a new one if no last SceneView is found.
		 */
		public static SceneView GetSceneView()
		{
			return SceneView.lastActiveSceneView == null ? EditorWindow.GetWindow<SceneView>() : SceneView.lastActiveSceneView;
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
		public static void CreateCachedEditor<T>(UnityEngine.Object[] targetObjects, ref Editor previousEditor) where T : Editor
		{
			#if UNITY_4_7
			if (previousEditor != null && pbUtil.IsEqual(previousEditor.targets, targetObjects) )
				return;

			if (previousEditor != null)
				UnityEngine.Object.DestroyImmediate(previousEditor);

			previousEditor = Editor.CreateEditor(targetObjects, typeof(T));
			#else
			Editor.CreateCachedEditor(targetObjects, typeof(T), ref previousEditor);
			#endif
		}
	}
}

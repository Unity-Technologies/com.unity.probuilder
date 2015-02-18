using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.IO;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using ProCore.Common;

#if PB_DEBUG
using Parabox.Debug;
#endif

public static class pb_Editor_Utility
{
#region NOTIFICATION MANAGER

	const float TIMER_DISPLAY_TIME = 1f;
	private static float notifTimer = 0f;
	private static EditorWindow notifWindow;
	private static bool notifDisplayed = false;

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
#endregion

#region GUI 
	
	public static Rect GUIRectWithObject(GameObject go)
	{
		Vector3 cen = go.GetComponent<Renderer>().bounds.center;
		Vector3 ext = go.GetComponent<Renderer>().bounds.extents;
		Vector2[] extentPoints = new Vector2[8]
		{
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z-ext.z)),
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z-ext.z)),
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y-ext.y, cen.z+ext.z)),
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y-ext.y, cen.z+ext.z)),

			HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z-ext.z)),
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z-ext.z)),
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x-ext.x, cen.y+ext.y, cen.z+ext.z)),
			HandleUtility.WorldToGUIPoint(new Vector3(cen.x+ext.x, cen.y+ext.y, cen.z+ext.z))
		};

		Vector2 min = extentPoints[0];
		Vector2 max = extentPoints[0];

		foreach(Vector2 v in extentPoints)
		{
			min = Vector2.Min(min, v);
			max = Vector2.Max(max, v);
		}

		return new Rect(min.x, min.y, max.x-min.x, max.y-min.y);
	}
#endregion

#region UV WRAPPING
#endregion

#region OBJ EXPORT

	public static string ExportOBJ(pb_Object[] pb)
	{
		if(pb.Length < 1) return "";

		pb_Object combined;
		if(pb.Length > 1)
			pbMeshOps.CombineObjects(pb, out combined);
		else
			combined = pb[0];

		string path = EditorUtility.SaveFilePanel("Save ProBuilder Object as Obj", "", "pb" + pb[0].id + ".obj", "");
		if(path == null || path == "")
		{
			if(pb.Length > 1) {
				GameObject.DestroyImmediate(combined.GetComponent<MeshFilter>().sharedMesh);
				GameObject.DestroyImmediate(combined.gameObject);
			}
			return "";
		}
		EditorObjExporter.MeshToFile(combined.GetComponent<MeshFilter>(), path);
		AssetDatabase.Refresh();

		if(pb.Length > 1) {
			GameObject.DestroyImmediate(combined.GetComponent<MeshFilter>().sharedMesh);
			GameObject.DestroyImmediate(combined.gameObject);
		}
		return path;
	}
#endregion

#region Screenshots

	public static void SaveTexture(Texture2D texture)
	{
		// int width = texture.width;
		// int height = texture.height;

		byte[] bytes = texture.EncodeToPNG();

		string path = EditorUtility.SaveFilePanel("Save Image", Application.dataPath, "", "png");

		if(path == "") return;

		System.IO.File.WriteAllBytes(path, bytes);

		AssetDatabase.Refresh();
	}
#endregion

#region ENTITY

	/**
	 *	\brief Sets the EntityType for the passed gameObject. 
	 *	@param newEntityType The type to set.
	 *	@param target The gameObject to apply the EntityType to.  Must contains pb_Object and pb_Entity components.  Method does contain null checks.
	 */
	public static void SetEntityType(this pb_Entity pb, EntityType newEntityType)
	{
		SetEntityType(newEntityType, pb.gameObject);
	}

	public static void SetEntityType(EntityType newEntityType, GameObject target)
	{
		pb_Entity ent = target.GetComponent<pb_Entity>();
		
		if(ent == null)
			ent = target.AddComponent<pb_Entity>();

		pb_Object pb = target.GetComponent<pb_Object>();

		if(!ent || !pb)
			return;

		SetDefaultEditorFlags(target);

		switch(newEntityType)
		{
			case EntityType.Detail:
				SetBrush(target);
				break;

			case EntityType.Occluder:
				SetOccluder(target);
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
			#if !PROTOTYPE
			target.GetComponent<pb_Object>().SetFaceMaterial(target.GetComponent<pb_Object>().faces, pb_Constant.DefaultMaterial );
			#else
			target.GetComponent<MeshRenderer>().sharedMaterial = pb_Constant.DefaultMaterial;
			#endif
		}
	}

	private static void SetDynamic(GameObject target)
	{
		EntityType et = target.GetComponent<pb_Entity>().entityType;

		SetEditorFlags((StaticEditorFlags)0, target);

		if(	et == EntityType.Trigger || 
			et == EntityType.Collider )
		#if !PROTOTYPE
			target.GetComponent<pb_Object>().SetFaceMaterial(target.GetComponent<pb_Object>().faces, pb_Constant.DefaultMaterial );
			#else
			target.GetComponent<MeshRenderer>().sharedMaterial = pb_Constant.DefaultMaterial;
			#endif
	}

	private static void SetOccluder(GameObject target)
	{
		EntityType et = target.GetComponent<pb_Entity>().entityType;	
		
		if(	et == EntityType.Trigger || 
			et == EntityType.Collider )
		{
			#if !PROTOTYPE
			target.GetComponent<pb_Object>().SetFaceMaterial(target.GetComponent<pb_Object>().faces, pb_Constant.DefaultMaterial );
			#else
			target.GetComponent<MeshRenderer>().sharedMaterial = pb_Constant.DefaultMaterial;
			#endif
		}

		StaticEditorFlags editorFlags;
		editorFlags = StaticEditorFlags.BatchingStatic | StaticEditorFlags.LightmapStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic | StaticEditorFlags.NavigationStatic | StaticEditorFlags.OffMeshLinkGeneration;
		
		SetEditorFlags(editorFlags, target);
	}

	private static void SetTrigger(GameObject target)
	{
		#if !PROTOTYPE
		target.GetComponent<pb_Object>().SetFaceMaterial(target.GetComponent<pb_Object>().faces, (Material)Resources.Load("Materials/Trigger", typeof(Material)) );
		#else
		target.GetComponent<MeshRenderer>().sharedMaterial = (Material)Resources.Load("Materials/Trigger", typeof(Material));
		#endif

		SetIsTrigger(true, target);
		SetEditorFlags((StaticEditorFlags)0, target);
	}

	private static void SetCollider(GameObject target)
	{
		#if !PROTOTYPE
		target.GetComponent<pb_Object>().SetFaceMaterial(target.GetComponent<pb_Object>().faces, (Material)Resources.Load("Materials/Collider", typeof(Material)) );
		#else
		target.GetComponent<MeshRenderer>().sharedMaterial = (Material)Resources.Load("Materials/Collider", typeof(Material));
		#endif

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

	/**
	 * Use Default static flags - StaticEditorFlags.BatchingStatic | StaticEditorFlags.LightmapStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.NavigationStatic | StaticEditorFlags.OffMeshLinkGeneration
	 * If NoDraw is present, BatchingStatic will not be flagged.
	 */
	private static void SetDefaultEditorFlags(GameObject target)
	{
		SetIsTrigger(false, target);
		
		StaticEditorFlags editorFlags;

		editorFlags = StaticEditorFlags.BatchingStatic | StaticEditorFlags.LightmapStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.NavigationStatic | StaticEditorFlags.OffMeshLinkGeneration;

		// if(target.GetComponent<pb_Entity>().entityType == EntityType.Occluder)
		// 	SetEditorFlagsWithBounds(editorFlags, target);

		SetEditorFlags(editorFlags, target);
	}
#endregion

#region EDITOR

	/**
	 * \brief ProBuilder objects created in Editor need to be initialized with a number of additional Editor-only settings.
	 *	This method provides an easy method of doing so in a single call.  #InitObjectFlags will set the Entity Type, generate 
	 *	a UV2 channel, set the unwrapping parameters, and center the object in the screen. 
	 */
	public static void InitObjectFlags(pb_Object pb, ColliderType col, EntityType et)
	{
		switch(col)
		{
			case ColliderType.BoxCollider:
				pb.gameObject.AddComponent<BoxCollider>();
			break;

			case ColliderType.MeshCollider:
				pb.gameObject.AddComponent<MeshCollider>().convex = EditorPrefs.HasKey(pb_Constant.pbForceConvex) ? EditorPrefs.GetBool(pb_Constant.pbForceConvex) : false;
				break;
		}

		pb_Lightmap_Editor.SetObjectUnwrapParamsToDefault(pb);
		pb.GenerateUV2();
		pb_Editor_Utility.SetEntityType(et, pb.gameObject);
		pb_Editor_Utility.ScreenCenter( pb.gameObject );
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

		if(pbUtil.SharedSnapEnabled)
			pb.transform.position = pbUtil.SnapValue(pb.transform.position, pbUtil.SharedSnapValue);
		else
		if(pb_Preferences_Internal.GetBool(pb_Constant.pbForceVertexPivot))
			pb.transform.position = pbUtil.SnapValue(pb.transform.position, 1f);
	}

	/**
	 * Returns all Unity Scenes found within the Project directory.
	 */
	public static string[] GetScenes()
	{
		string[] allFiles = Directory.GetFiles("Assets/", "*.unity", SearchOption.AllDirectories);
		return allFiles;
	}

	/**
	 * Returns the last active SceneView window, or creates a new one if no last SceneView is found.
	 */
	public static SceneView GetSceneView()
	{
		return SceneView.lastActiveSceneView == null ? EditorWindow.GetWindow<SceneView>() : SceneView.lastActiveSceneView;
	}

	public static void FocusSceneView()
	{
		GetSceneView().Focus();
	}
#endregion
}

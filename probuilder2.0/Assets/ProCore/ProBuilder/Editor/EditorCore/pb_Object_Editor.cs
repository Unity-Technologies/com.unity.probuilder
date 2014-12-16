
// #pragma warning disable 0162 // TODO - FIX
#if UNITY_4_3 || UNITY_4_3_0 || UNITY_4_3_1 || UNITY_4_3_2 || UNITY_4_3_3 || UNITY_4_3_4 || UNITY_4_3_5
#define UNITY_4_3
#endif

#if UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_3_0 || UNITY_4_3_1 || UNITY_4_3_2 || UNITY_4_3_3 || UNITY_4_3_4 || UNITY_4_3_5
#define UNITY_4
#endif

#undef UNITY_4

using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;
using System.Collections.Generic;
using System.Linq;

#if PB_DEBUG
using Parabox.Debug;
#endif

[CustomEditor(typeof(pb_Object))]
[CanEditMultipleObjects]
public class pb_Object_Editor : Editor
{
	public delegate void OnGetFrameBoundsDelegate ();
	public static event OnGetFrameBoundsDelegate OnGetFrameBoundsEvent;

	pb_Object pb;
	pb_Editor editor { get { return pb_Editor.instance; } }

	Renderer ren;
	Vector3 offset = Vector3.zero;

	const int EDITLEVEL_TOOLBAR_WIDTH = 222;
	const int TOOLBAR_BUTTON_WIDTH = EDITLEVEL_TOOLBAR_WIDTH / 2;
	static Color SceneToolbarColor_Active;							// Set in OnEnable because it depends on pro/free skin
	bool showToolbar = true;

	public void OnEnable()
	{	
		if(EditorApplication.isPlayingOrWillChangePlaymode)
			return;
		
		showToolbar = pb_Preferences_Internal.GetBool(pb_Constant.pbShowSceneToolbar);

		if(target is pb_Object)
			pb = (pb_Object)target;
		else
			return;

 		SceneToolbarColor_Active = EditorGUIUtility.isProSkin ? new Color(.35f, .35f, .35f, 1f) : new Color(.8f, .8f, .8f, 1f);
		ren = pb.gameObject.GetComponent<Renderer>();

		EditorUtility.SetSelectedWireframeHidden(ren,
			pb_Preferences_Internal.GetBool(pb_Constant.pbHideWireframe) &&
			editor != null
			);

		/* if Verify returns false, that means the mesh was rebuilt - so generate UV2 again */
		if( !pb.Verify() )
			pb.GenerateUV2();
	}

	// bool pbInspectorFoldout = false;
	public override void OnInspectorGUI()
	{
		GUI.backgroundColor = Color.green;

		if(GUILayout.Button("Open " + pb_Constant.PRODUCT_NAME))
			pb_Editor.MenuOpenWindow();

		GUI.backgroundColor = Color.white;

		if(!ren) return;
		Vector3 sz = ren.bounds.size;
		EditorGUILayout.Vector3Field("Object Size (read only)", sz);

		if(pb == null) return;
		
		if(pb.SelectedTriangles.Length > 0)
		{
			GUILayout.Space(5);

			offset = EditorGUILayout.Vector3Field("Quick Offset", offset);
			if(GUILayout.Button("Apply Offset"))
			{
				pbUndo.RecordObject(pb, "Offset Vertices");
				pb.TranslateVertices_World(pb.SelectedTriangles, offset);
				pb.Refresh();
				if(editor != null)
					editor.UpdateSelection();
			}
		}
	}

	Rect LeftButton 	= new Rect(	0, 12, TOOLBAR_BUTTON_WIDTH, 16);
	Rect RightButton 	= new Rect(	0, 12, TOOLBAR_BUTTON_WIDTH, 16);

	GUIContent ObjectMode = new GUIContent("Object", "Edit top level transforms, including non-ProBuilder objects.");
	GUIContent ElementMode = new GUIContent("Element", "Allows editing of vertices, faces, and edges.");

	void OnSceneGUI()
	{
		if(editor != null && showToolbar)
		{
			LeftButton.x = Screen.width/2f - EDITLEVEL_TOOLBAR_WIDTH/2f;
			RightButton.x = LeftButton.x + TOOLBAR_BUTTON_WIDTH;

			Handles.BeginGUI();
				GUI.backgroundColor = (editor.editLevel == EditLevel.Top) ? SceneToolbarColor_Active : Color.white;
				if(GUI.Button(LeftButton, ObjectMode, EditorStyles.miniButtonLeft))
				{
					pb_Editor_Utility.ShowNotification("Top Level Editing");
					editor.SetEditLevel(EditLevel.Top);
				}
				
				GUI.backgroundColor = (editor.editLevel == EditLevel.Geometry) ? SceneToolbarColor_Active : Color.white;
				if(GUI.Button(RightButton, ElementMode, EditorStyles.miniButtonRight))
				{
					pb_Editor_Utility.ShowNotification("Geometry Editing");
					editor.SetEditLevel(EditLevel.Geometry);
				}
			GUI.backgroundColor = Color.white;
			Handles.EndGUI();
		// #endif
		}

		if(EditorApplication.isPlayingOrWillChangePlaymode || pb == null)
			return;

		// if(GUIUtility.hotControl < 1 && pb.transform.localScale != Vector3.one)
		// {
		// 	pb_Object[] selection = pbUtil.GetComponents<pb_Object>(Selection.transforms);
		// 	pbUndo.RecordObjects(selection, "Scale Selection");
		// 	foreach(pb_Object pbs in selection)
		// 		pbs.FreezeScaleTransform();
		// }
	}

	bool HasFrameBounds() 
	{
		if(pb == null)
			pb = (pb_Object)target;

		return pb_Editor.instance != null && pbUtil.GetComponents<pb_Object>(Selection.transforms).Sum(x => x.SelectedTriangles.Length) > 0;
		// return pb_Editor.instance != null && pbUtil.GetComponents<pb_Object>(Selection.transforms).Sum(x => x.sharedIndices.UniqueIndicesWithValues(x.SelectedTriangles).Length) > 1;
	}

	Bounds OnGetFrameBounds()
	{
		if(OnGetFrameBoundsEvent != null) OnGetFrameBoundsEvent();
		
		Vector3 min = Vector3.zero, max = Vector3.zero;
		bool init = false;

		foreach(pb_Object pbo in pbUtil.GetComponents<pb_Object>(Selection.transforms))
		{		
			if(pbo.SelectedTriangles.Length < 1) continue;

			Vector3[] verts = pbo.VerticesInWorldSpace(pbo.SelectedTriangles);

			if(!init) 
			{
				init = true;
				min = verts[0];
				max = verts[0];
			}

			for(int i = 0; i < verts.Length; i++)
			{
				min.x = Mathf.Min(verts[i].x, min.x);
				max.x = Mathf.Max(verts[i].x, max.x);

				min.y = Mathf.Min(verts[i].y, min.y);
				max.y = Mathf.Max(verts[i].y, max.y);

				min.z = Mathf.Min(verts[i].z, min.z);
				max.z = Mathf.Max(verts[i].z, max.z);
			}
		}

		return new Bounds( (min+max)/2f, max != min ? max-min : Vector3.one * .1f );
	}
}
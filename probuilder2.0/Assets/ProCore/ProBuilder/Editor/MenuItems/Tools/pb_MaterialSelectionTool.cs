using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

// namespace ProBuilder2.Actions
// {
public class pb_MaterialSelectionTool : EditorWindow
{
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Tools/Replace Faces with Material", true, pb_Constant.MENU_TOOLS)]
	public static bool VerifyOpenSelectFaces()
	{
		return pb_Editor.instance != null;
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Tools/Replace Faces with Material", false, pb_Constant.MENU_TOOLS)]
	public static void SelectFacesWithMat()
	{
		EditorWindow.GetWindow<pb_MaterialSelectionTool>(true, "Replace Material", true).Show();
	} 

	Material mat, rmat;

	int pad = 6;
	private void OnGUI()
	{
		int w = Screen.width, h = Screen.height;
		h = w/2+140;
		this.minSize = new Vector2(0, h);
		int HalfWindowPad = w/2-pad;
		Rect r;
		
		EditorGUILayout.HelpBox("This utility will set the ProBuilder Editor's selection to all faces that match the provided material.  Optionally, you may also replace every selected material with a new material.", MessageType.Info);
		GUILayout.BeginHorizontal();

		GUILayout.BeginVertical();

		GUILayout.Label("Find Material", EditorStyles.boldLabel);

		mat = (Material)EditorGUILayout.ObjectField(mat, typeof(Material), false, GUILayout.MinWidth(HalfWindowPad), GUILayout.MaxWidth(HalfWindowPad));

		r = new Rect( pad, 92, HalfWindowPad-20, HalfWindowPad-20);

		EditorGUI.ObjectField(r, mat, typeof(Material), false);

		GUILayout.EndVertical();

		GUILayout.BeginVertical();

		GUILayout.Label("Replacement Material", EditorStyles.boldLabel);

		rmat = (Material)EditorGUILayout.ObjectField(rmat, typeof(Material), false, GUILayout.MinWidth(HalfWindowPad), GUILayout.MaxWidth(HalfWindowPad));

		r = new Rect( HalfWindowPad+5 + pad, 92, HalfWindowPad-20, HalfWindowPad-20);

		EditorGUI.ObjectField(r, rmat, typeof(Material), false);

		GUILayout.EndVertical();

		GUILayout.EndHorizontal();
		
		if(GUI.Button(new Rect(4, Screen.height-37, Screen.width-8, 30), "Select Faces with Material"))
			SelectFacesWithMatchingMaterial(mat);

		if(GUI.Button(new Rect(4, Screen.height-70, Screen.width-8, 30), "Replace Materials"))
			ReplaceSelectedMaterials(mat, rmat);
	}

	private void SelectFacesWithMatchingMaterial(Material mat)
	{
		pb_Editor editor = pb_Editor.instance;
		
		// Clear out all selected
		editor.ClearSelection();

		// If we're in Mode based editing, make sure that we're also in geo mode. 
		editor.SetEditLevel(EditLevel.Geometry);

		// aaand also set to face seelction mode
		editor.SetSelectionMode(SelectMode.Face);

		pb_Object[] pbs = FindObjectsOfType(typeof(pb_Object)) as pb_Object[];
		
		// Cycle through every ProBuilder Object
		foreach(pb_Object pb in pbs)
		{
			bool addToSelection = false;

			for(int i = 0; i < pb.faces.Length; i++)
			{
				if(pb.faces[i].material == mat)
				{

					addToSelection = true;
					pb.AddToFaceSelection(i);
				}
			}

			if(addToSelection)
				editor.AddToSelection(pb.gameObject);
		}
		
		editor.UpdateSelection();
	}

	private void ReplaceSelectedMaterials(Material mat, Material replacement)
	{
		pb_Editor editor = pb_Editor.instance;

		pb_Object[] pbs = FindObjectsOfType(typeof(pb_Object)) as pb_Object[];
		// Cycle through every ProBuilder Object
		foreach(pb_Object pb in pbs)
		{
			bool addToSelection = false;

			for(int f = 0; f < pb.faces.Length; f++)
			{
				if(pb.faces[f].material == mat)
				{
					addToSelection = true;
					
					pb.faces[f].SetMaterial(rmat);

					pb.AddToFaceSelection(f);
				}
			}

			pb.Refresh();

			if(addToSelection)
				editor.AddToSelection(pb.gameObject);
		}

		SceneView.RepaintAll();
	}
}
// }
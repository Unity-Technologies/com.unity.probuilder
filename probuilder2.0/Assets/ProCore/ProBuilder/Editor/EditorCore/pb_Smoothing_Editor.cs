using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

public class pb_Smoothing_Editor : EditorWindow
{
#if !PROTOTYPE
#region MEMBERS

	pb_Editor editor { get { return pb_Editor.instance; } }
	pb_Object[] selection;
	List<int> 	smoothGroups = new List<int>();

	const int BUTTON_WIDTH = 28;
	const int pad = 2;

	float normalsLength = 0f;

	int oldWidth = 0, oldHeight = 0;
#endregion

#region INITIALIZATION CALLBACKS

	public static pb_Smoothing_Editor Init()
	{
		pb_Smoothing_Editor pse = (pb_Smoothing_Editor)EditorWindow.GetWindow(typeof(pb_Smoothing_Editor), true, "Smoothing Groups", true);

		return pse;
	}

	void SetDrawNormals(float val)
	{
		if(pb_Editor.instance)
		{
			pb_Editor.instance.drawNormals = val;
			pb_Editor.instance.UpdateSelection(false);
		}

		SceneView.RepaintAll();
	}

	void OnEnable()
	{		
		this.autoRepaintOnSceneChange = true;

		this.minSize = new Vector2(332f, 220f);

		pb_Editor.OnSelectionUpdate += OnSelectionUpdate;

		if(editor != null)
		{
			SetDrawNormals(normalsLength);
			OnSelectionUpdate(editor.selection);
		}
	}

	void OnFocus()
	{
		if(pb_Editor.instance)
			pb_Editor.instance.SetSelectionMode(SelectMode.Face);
	}

	void OnDisable()
	{
		SetDrawNormals(0f);
	}

	void OnWindowResize()
	{
		clearAllRect = new Rect(Screen.width-68-pad, Screen.height-20-pad, 68, 18);
		drawNormalsRect = new Rect(pad, Screen.height-18-pad, 252, 18);
	}
#endregion

#region INTERFACE
	
	Rect smoothLabelRect = new Rect(pad, pad, 200, 18);
	Rect hardLabelRect = new Rect(pad, pad, 200, 18);
	Rect clearAllRect = new Rect(0f, 0f, 0f, 0f);
	Rect drawNormalsRect = new Rect(0f, 0f, 0f, 0f);

	void OnGUI()
	{
		if(Screen.width != oldWidth || Screen.height != oldHeight)
			OnWindowResize();

		// remove all on object
		if(GUI.Button(clearAllRect, "Clear"))
			SetSmoothingGroup(selection, 0);

		GUI.Label(smoothLabelRect, "Smooth", EditorStyles.boldLabel);

		EditorGUI.BeginChangeCheck();
		EditorGUIUtility.labelWidth = 60;
		normalsLength = EditorGUI.Slider(drawNormalsRect, "Normals", normalsLength, 0f, 1f);
		if(EditorGUI.EndChangeCheck())
			SetDrawNormals(normalsLength);

		// smoothingGroup 0 is reserved for 'no group'
		int buttonsPerLine = Screen.width / (BUTTON_WIDTH+pad);
		int row = 0;
		Rect buttonRect = new Rect(pad, smoothLabelRect.y + smoothLabelRect.height + pad, BUTTON_WIDTH, BUTTON_WIDTH);

		for(int i = 1; i < 25; i++)
		{
			if(i - (buttonsPerLine*row) > buttonsPerLine) {
				row++;
				buttonRect = new Rect(pad, buttonRect.y + BUTTON_WIDTH + pad, BUTTON_WIDTH, BUTTON_WIDTH);
			}

			if(smoothGroups.Contains(i))
				GUI.backgroundColor = Color.green;
		
			if(GUI.Button(buttonRect, i.ToString()))
				SetSmoothingGroup(selection, i);

			GUI.backgroundColor = Color.white;

			buttonRect = new Rect(buttonRect.x + BUTTON_WIDTH + pad, buttonRect.y, BUTTON_WIDTH, BUTTON_WIDTH);
		}

		hardLabelRect = new Rect(pad, buttonRect.y + pad + BUTTON_WIDTH + 10, 200, 18);
		GUI.Label(hardLabelRect, "Hard", EditorStyles.boldLabel);
		row = 0;
		buttonRect = new Rect(pad, hardLabelRect.y + hardLabelRect.height + pad, BUTTON_WIDTH, BUTTON_WIDTH);
		for(int i = 25; i < 43; i++)
		{
			if( (i-24) - (buttonsPerLine*row) > buttonsPerLine) {
				row++;
				buttonRect = new Rect(pad, buttonRect.y + BUTTON_WIDTH + pad, BUTTON_WIDTH, BUTTON_WIDTH);
			}

			if(smoothGroups.Contains(i))
				GUI.backgroundColor = Color.green;
		
			if(GUI.Button(buttonRect, i.ToString()))
				SetSmoothingGroup(selection, i);

			GUI.backgroundColor = Color.white;

			buttonRect = new Rect(buttonRect.x + BUTTON_WIDTH + pad, buttonRect.y, BUTTON_WIDTH, BUTTON_WIDTH);
		}
	}
#endregion

#region APPLY
	
	/**
	 * Apply smoothing group to all selected faces in _selection.
	 */
	void SetSmoothingGroup(pb_Object[] _selection, int sg)
	{
		pbUndo.RecordObjects(_selection, "Set Smoothing Groups");

		// If all selected are of the same group, act as a toggle
		if(smoothGroups.Count == 1 && smoothGroups[0] == sg)
			sg = 0;

		foreach(pb_Object pb in _selection)
		{
			if(pb.SelectedFaceCount > 0)
			{
				foreach(pb_Face face in pb.SelectedFaces)
					face.SetSmoothingGroup(sg);
			}
			else
			{
				foreach(pb_Face face in pb.faces)
					face.SetSmoothingGroup(sg);
			}

			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();
		}

		if( pb_Editor.instance != null)
			pb_Editor.instance.UpdateSelection(false);
		else
			OnSelectionUpdate(selection);
	}

	void ClearAllSmoothingGroups(pb_Object[] _selection)
	{
		pbUndo.RecordObjects(_selection, "Clear Smoothing Groups");

		foreach(pb_Object pb in _selection)
		{
			foreach(pb_Face face in pb.faces)
			{
				face.SetSmoothingGroup(0);
			}
			
			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();
		}

		if( pb_Editor.instance != null)
			pb_Editor.instance.UpdateSelection(false);
		else
			OnSelectionUpdate(selection);
	}
#endregion

#region SELECTION CACHE

	void OnSelectionUpdate(pb_Object[] _selection)
	{
		selection = _selection;
		smoothGroups.Clear();

		if (selection != null)
		{
			foreach (pb_Object pb in selection)
			{
				foreach (pb_Face face in pb.SelectedFaces)
				{
					if (!smoothGroups.Contains(face.smoothingGroup))
					  smoothGroups.Add(face.smoothingGroup);
				}
			}
		}

		Repaint();
	}
#endregion

#region VISUALIZATION

	void DrawPreviewMesh(pb_Object pb)
	{

	}
#endregion
	#endif
}
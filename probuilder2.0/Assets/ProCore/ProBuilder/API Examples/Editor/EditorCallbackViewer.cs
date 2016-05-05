using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using ProBuilder2.EditorCommon;	// pb_Editor and pb_Editor_Utility
using ProBuilder2.Interface;	// pb_GUI_Utility
using ProBuilder2.Common;		// EditLevel
using System.Linq;				// Sum()

class EditorCallbackViewer : EditorWindow
{
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/API Examples/Log Callbacks Window")]
	static void MenuInitEditorCallbackViewer()
	{
		EditorWindow.GetWindow<EditorCallbackViewer>(true, "ProBuilder Callbacks", true).Show();
	}

	List<string> logs = new List<string>();
	Vector2 scroll = Vector2.zero;
	Color logBackgroundColor = new Color(.15f, .15f, .15f, 1f);

	void OnEnable()
	{
		// Delegate for Top/Geometry/Texture mode changes.
		pb_Editor.AddOnEditLevelChangedListener(OnEditLevelChanged);

		// Called when a new ProBuilder object is created.
		// note - this was added in ProBuilder 2.5.1
		pb_Editor_Utility.AddOnObjectCreatedListener(OnProBuilderObjectCreated);

		// Called when the ProBuilder selection changes (can be object or element change).
		// @todo add OnSelectionChange, since this also indicates when the mesh is being modified
		pb_Editor.OnSelectionUpdate += OnSelectionUpdate;

		// Called when vertices have been moved by ProBuilder.
		pb_Editor.OnVertexMovementFinished += OnVertexMovementFinished;
	}

	void OnDisable()
	{
		pb_Editor.RemoveOnEditLevelChangedListener(OnEditLevelChanged);
		// pb_Editor_Utility.RemoveOnObjectCreatedListener(OnProBuilderObjectCreated);
		pb_Editor.OnSelectionUpdate -= OnSelectionUpdate;
		pb_Editor.OnVertexMovementFinished -= OnVertexMovementFinished;
	}

	void OnProBuilderObjectCreated(pb_Object pb)
	{
		AddLog("Instantiated new ProBuilder Object: " + pb.name);
	}

	void OnEditLevelChanged(int editLevel)
	{
		AddLog("Edit Level Changed: " + (EditLevel) editLevel);
	}

	void OnSelectionUpdate(pb_Object[] selection)
	{
		AddLog("Selection Changed: " + string.Format("{0} objects and {1} vertices selected.",
			selection != null ? selection.Length : 0,
			selection != null ? selection.Sum(x => x.SelectedTriangleCount) : 0));
	}

	void OnVertexMovementFinished(pb_Object[] selection)
	{
		AddLog("Finished Moving Vertices");
	}

	void AddLog(string summary)
	{
		logs.Add(logs.Count + ": " + summary);
		Repaint();
	}

	void OnGUI()
	{
		GUILayout.BeginHorizontal();
			GUILayout.Label("Callback Log", EditorStyles.boldLabel);
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Clear"))
				logs.Clear();
		GUILayout.EndHorizontal();

		Rect r = GUILayoutUtility.GetLastRect();
		r.x = 0;
		r.y = r.y + r.height + 6;
		r.width = this.position.width;
		r.height = this.position.height;

		GUILayout.Space(4);

		pb_GUI_Utility.DrawSolidColor(r, logBackgroundColor);

		scroll = GUILayout.BeginScrollView(scroll);

		for(int i = logs.Count - 1; i >= 0; i--)
			GUILayout.Label(logs[i]);

		GUILayout.EndScrollView();
	}
}

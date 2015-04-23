using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

public class pb_Vertex_Color_Toolbar : EditorWindow
{
#region CONSTANTS

	public static Color[] DEFAULT_COLORS = new Color[10]
	{
		Color.white,
		Color.red,
		Color.blue,
		Color.yellow,
		Color.green,
		Color.cyan,
		Color.black,
		Color.magenta,
		Color.gray,
		new Color(.4f, 0f, 1f, 1f)
	};

	public static Color[] USER_COLORS;
#endregion

#region INITIALIZATION
	
	/**
	 * Initialize this window.
	 */
	public static void MenuOpenWindow()
	{
		bool dockable = pb_Preferences_Internal.GetBool(pb_Constant.pbVertexPaletteDockable);
		EditorWindow.GetWindow<pb_Vertex_Color_Toolbar>(!dockable, "Vertex Colors", true);
	}


	void OnEnable()
	{
		USER_COLORS = new Color[10];
		for(int i = 0; i < DEFAULT_COLORS.Length; i++)
		{
			if( !pbUtil.ColorWithString( EditorPrefs.GetString(pb_Constant.pbVertexColorPrefs+i), out USER_COLORS[i] ) )
				USER_COLORS[i] = DEFAULT_COLORS[i];
		}
	}

	void OpenContextMenu()
	{
		GenericMenu menu = new GenericMenu();

		menu.AddItem (new GUIContent("Open As Floating Window", ""), false, Menu_OpenAsFloatingWindow);
		menu.AddItem (new GUIContent("Open As Dockable Window", ""), false, Menu_OpenAsDockableWindow);

		menu.ShowAsContext ();
	}		

	void Menu_OpenAsDockableWindow()
	{
		EditorPrefs.SetBool(pb_Constant.pbVertexPaletteDockable, true);

		EditorWindow.GetWindow<pb_Vertex_Color_Toolbar>().Close();
		pb_Vertex_Color_Toolbar.MenuOpenWindow();
	}

	void Menu_OpenAsFloatingWindow()
	{
		EditorPrefs.SetBool(pb_Constant.pbVertexPaletteDockable, false);

		EditorWindow.GetWindow<pb_Vertex_Color_Toolbar>().Close();
		pb_Vertex_Color_Toolbar.MenuOpenWindow();
	}
#endregion

#region ONGUI

	int pad = 4;
	Vector2 scroll = Vector2.zero;
	int ButtonWidth = 58;
	private void OnGUI()
	{
		Event e = Event.current;

		switch(e.type)
		{
			case EventType.ContextClick:
				OpenContextMenu();
				break;
		}

		// this.minSize = new Vector2(404, 68 + 24);
		// this.maxSize = new Vector2(404, 68 + 24);
		int width = Screen.width;

		int rowSize = width / (ButtonWidth+5);
		int curRow = 0;

		scroll = EditorGUILayout.BeginScrollView(scroll);

		GUILayout.BeginVertical();
		GUILayout.BeginHorizontal();

		for(int i = 0; i < USER_COLORS.Length; i++)
		{	
			if( (i - (curRow * rowSize)) >= rowSize)
			{
				curRow++;
				GUILayout.FlexibleSpace();

				GUILayout.EndHorizontal();

				GUILayout.Space(6);

				GUILayout.BeginHorizontal();
			}

			GUILayout.BeginVertical();

			GUI.color = Color.white;

			if(GUILayout.Button(EditorGUIUtility.whiteTexture, GUILayout.Width(ButtonWidth), GUILayout.Height(42)))
				SetFaceColors(USER_COLORS[i]);

			GUI.color = USER_COLORS[i];
			Rect layoutRect = GUILayoutUtility.GetLastRect();
			layoutRect.x += pad;
			layoutRect.y += pad;
			layoutRect.width -= pad*2;
			layoutRect.height -= pad*2;
			EditorGUI.DrawPreviewTexture(layoutRect, EditorGUIUtility.whiteTexture, null, ScaleMode.StretchToFill, 0);

			GUI.changed = false;
			USER_COLORS[i] = EditorGUILayout.ColorField(USER_COLORS[i], GUILayout.Width(ButtonWidth));
			if(GUI.changed) SetColorPreference(i, USER_COLORS[i]);

			GUILayout.EndVertical();

			if(i == USER_COLORS.Length-1)
				GUILayout.FlexibleSpace();
		}

		GUI.color = Color.white;

		GUILayout.EndHorizontal();
		GUILayout.EndVertical();

		EditorGUILayout.EndScrollView();

		// Vertical spacing
		GUILayout.FlexibleSpace();

		GUILayout.BeginHorizontal(EditorStyles.toolbar);

		GUILayout.FlexibleSpace();
		if( GUILayout.Button("Reset", EditorStyles.toolbarButton) )
			ResetColors();

		GUILayout.EndHorizontal();
	}
#endregion

#region Editor

	/**
	 *	\brief Sets the color preference in vertex color window.
	 *	
	 */
	private void SetColorPreference(int index, Color col)
	{
		EditorPrefs.SetString(pb_Constant.pbVertexColorPrefs+index, col.ToString());
	}

	private void ResetColors()
	{
		USER_COLORS = new Color[10];
		for(int i = 0; i < DEFAULT_COLORS.Length; i++)
		{
			if(EditorPrefs.HasKey(pb_Constant.pbVertexColorPrefs+i))
				EditorPrefs.DeleteKey(pb_Constant.pbVertexColorPrefs+i);
			USER_COLORS[i] = DEFAULT_COLORS[i];
		}
	}
#endregion

#region FUNCTION

	public static void SetFaceColors(int userPrefColorIndex)
	{
		if(USER_COLORS != null)
			pb_Vertex_Color_Toolbar.SetFaceColors(pb_Vertex_Color_Toolbar.USER_COLORS[userPrefColorIndex]);
		else
			pb_Vertex_Color_Toolbar.SetFaceColors(pb_Vertex_Color_Toolbar.DEFAULT_COLORS[userPrefColorIndex]);
	}

	public static void SetFaceColors(Color col)
	{
		pb_Object[] selection = pbUtil.GetComponents<pb_Object>(Selection.transforms);

		pbUndo.RecordObjects(selection, "Apply Vertex Colors");

		pb_Editor editor = pb_Editor.instance;

		if(editor && editor.editLevel == EditLevel.Geometry)
		{
			switch(editor.selectionMode)
			{
				case SelectMode.Face:
				case SelectMode.Vertex:
					foreach(pb_Object pb in selection)
					{
						Color[] colors = pb.colors;

						foreach(int i in pb.SelectedTriangles)
							colors[i] = col;					

						pb.SetColors(colors);
					}
					break;
				case SelectMode.Edge:
					foreach(pb_Object pb in selection)
					{
						Color[] colors = pb.colors;

						foreach(int i in pb.sharedIndices.AllIndicesWithValues(pb.SelectedTriangles))
							colors[i] = col;					

						pb.SetColors(colors);
					}
					break;
			}
		}
		else
		{
			foreach(pb_Object pb in selection)
			{
				foreach(pb_Face face in pb.faces)
					pb.SetFaceColor(face, col);	
			}
		}

		foreach(pb_Object pb in selection)
		{
			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();
		}

		pb_Editor_Utility.ShowNotification("Set Vertex Colors\n" + pb_ColorUtil.GetColorName(col));
	}
#endregion
}
using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;

public class pb_VertexColorInterface : EditorWindow
{
#region CONSTANTS

	public static Color[] COLOR_ARRAY = new Color[10]
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

	void OnEnable()
	{
		USER_COLORS = new Color[10];
		for(int i = 0; i < COLOR_ARRAY.Length; i++)
		{
			if( !pbUtil.ColorWithString( EditorPrefs.GetString(pb_Constant.pbVertexColorPrefs+i), out USER_COLORS[i] ) )
				USER_COLORS[i] = COLOR_ARRAY[i];
		}
	}
#endregion

#region ONGUI

	// Color32 col = Color.white;
	public void OnGUI()
	{
		this.minSize = new Vector2(404, 68 + 24);
		this.maxSize = new Vector2(404, 68 + 24);


		GUILayout.BeginHorizontal();

		for(int i = 0; i < USER_COLORS.Length; i++)
		{
			GUI.backgroundColor = USER_COLORS[i];

			GUILayout.BeginVertical();

			if(GUILayout.Button("", 
				GUILayout.MinWidth(36), GUILayout.MaxWidth(36),
				GUILayout.MinHeight(36), GUILayout.MaxHeight(36)))
				SetFaceColors(USER_COLORS[i]);

			GUI.changed = false;
			USER_COLORS[i] = EditorGUILayout.ColorField(USER_COLORS[i], GUILayout.MinWidth(36), GUILayout.MaxWidth(36));
			if(GUI.changed) SetColorPreference(i, USER_COLORS[i]);

			GUILayout.EndVertical();

		}

		GUI.backgroundColor = Color.white;

		GUILayout.EndHorizontal();

		if( GUI.Button(new Rect(Screen.width-44, Screen.height-24, 40, 20), "Reset") )
			ResetColors();
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
		for(int i = 0; i < COLOR_ARRAY.Length; i++)
		{
			if(EditorPrefs.HasKey(pb_Constant.pbVertexColorPrefs+i))
				EditorPrefs.DeleteKey(pb_Constant.pbVertexColorPrefs+i);
			USER_COLORS[i] = COLOR_ARRAY[i];
		}
	}
#endregion

#region FUNCTION

	public static void SetFaceColors(int userPrefColorIndex)
	{
		if(USER_COLORS != null)
			pb_VertexColorInterface.SetFaceColors(pb_VertexColorInterface.USER_COLORS[userPrefColorIndex]);
		else
			pb_VertexColorInterface.SetFaceColors(pb_VertexColorInterface.COLOR_ARRAY[userPrefColorIndex]);
	}

	public static void SetFaceColors(Color32 col)
	{
		pb_Object[] selection = pbUtil.GetComponents<pb_Object>(Selection.transforms);

		pbUndo.RecordObjects(selection, "Apply Vertex Colors");

		foreach(pb_Object pb in selection)
		{
			foreach(pb_Face face in pb.SelectedFaces)
				pb.SetFaceColor(face, col);
	
			pb.ToMesh();
			pb.Refresh();
			pb.GenerateUV2();
		}

		pb_Editor_Utility.ShowNotification("Set Face Color\n" + col);
	}
#endregion
}
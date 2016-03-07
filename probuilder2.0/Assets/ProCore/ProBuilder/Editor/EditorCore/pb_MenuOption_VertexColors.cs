using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	public class pb_MenuOption_VertexColors : pb_MenuOption
	{
		void OnGUI()
		{
			GUILayout.Label("Vertex Color Editor", EditorStyles.boldLabel);

			VertexColorTool tool = pb_Preferences_Internal.GetEnum<VertexColorTool>(pb_Constant.pbVertexColorTool);
			VertexColorTool prev = tool;

			tool = (VertexColorTool) EditorGUILayout.EnumPopup("Editor", tool);

			if(prev != tool)
				EditorPrefs.SetInt(pb_Constant.pbVertexColorTool, (int)tool);

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Open Vertex Editor"))
				pb_Menu_Commands.MenuOpenVertexColorsEditor();
		}

	}	
}

using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	public class pb_MenuOption_Extrude : pb_MenuOption
	{
		void OnGUI()
		{
			GUILayout.Label("Extrude Options", EditorStyles.boldLabel);


			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Extrude Selection"))
				pb_Menu_Commands.MenuOpenVertexColorsEditor();
		}

	}	
}

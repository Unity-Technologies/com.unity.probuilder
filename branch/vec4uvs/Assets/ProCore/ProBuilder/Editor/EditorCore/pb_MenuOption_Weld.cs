using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.Interface;

namespace ProBuilder2.EditorCommon
{
	public class pb_MenuOption_Weld : pb_MenuOption
	{
		const float MIN_WELD_DISTANCE = .0001f;

		void OnGUI()
		{
			GUILayout.Label("Weld Vertices Options", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();

			float weldDistance = pb_Preferences_Internal.GetFloat(pb_Constant.pbWeldDistance);

			if(weldDistance <= MIN_WELD_DISTANCE)
				weldDistance = MIN_WELD_DISTANCE;

			weldDistance = EditorGUILayout.FloatField(pb_GUI_Utility.TempGUIContent("Max Weld Distance", "The maximum distance between two vertices in order to be welded together."), weldDistance);

			if( EditorGUI.EndChangeCheck() )
			{
				if(weldDistance < MIN_WELD_DISTANCE)
					weldDistance = MIN_WELD_DISTANCE;
				EditorPrefs.SetFloat(pb_Constant.pbWeldDistance, weldDistance);
			}

			GUILayout.FlexibleSpace();
			
			if(GUILayout.Button("Weld Vertices"))
				pb_Menu_Commands.MenuWeldVertices(pbUtil.GetComponents<pb_Object>(Selection.transforms));
		}

	}	
}

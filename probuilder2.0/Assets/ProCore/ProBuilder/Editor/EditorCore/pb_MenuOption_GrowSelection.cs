using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	public class pb_MenuOption_GrowSelection : pb_MenuOption
	{
		void OnGUI()
		{
			GUILayout.Label("Grow Selection Options", EditorStyles.boldLabel);

			bool angleGrow = pb_Preferences_Internal.GetBool(pb_Constant.pbGrowSelectionUsingAngle);

			EditorGUI.BeginChangeCheck();

			angleGrow = EditorGUILayout.Toggle("Restrict to Angle", angleGrow);
			float angleVal = pb_Preferences_Internal.GetFloat(pb_Constant.pbGrowSelectionAngle);

			bool te = GUI.enabled;
			GUI.enabled = te ? angleGrow : te;

			angleVal = EditorGUILayout.FloatField("Max Angle", angleVal);

			bool iterative = pb_Preferences_Internal.GetBool(pb_Constant.pbGrowSelectionAngleIterative);
			iterative = EditorGUILayout.Toggle("Iterative", iterative);

			GUI.enabled = te;

			if( EditorGUI.EndChangeCheck() )
			{
				EditorPrefs.SetBool(pb_Constant.pbGrowSelectionUsingAngle, angleGrow);
				EditorPrefs.SetBool(pb_Constant.pbGrowSelectionAngleIterative, iterative);
				EditorPrefs.SetFloat(pb_Constant.pbGrowSelectionAngle, angleVal);
			}

			if(GUILayout.Button("Grow Selection"))
				pb_Menu_Commands.MenuGrowSelection(pbUtil.GetComponents<pb_Object>(Selection.transforms));
		}

	}	
}

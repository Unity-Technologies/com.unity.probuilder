using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;

namespace ProBuilder2.Actions
{
	public class GrowSelection : pb_MenuAction
	{
		public override pb_IconGroup group { get { return pb_IconGroup.Selection; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Selection_Grow"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Grow Selection",
			@"Adds adjacent elements to the current selection, optionally testing to see if they are within a specified angle.

Grow by angle is enabbled by Option + Clicking the <b>Grow Selection</b> button.",
			CMD_ALT, 'G'
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					pb_Menu_Commands.VerifyGrowSelection(selection);
		}

		public override MenuActionState AltState()
		{
			if(	IsEnabled() &&
				pb_Editor.instance.editLevel == EditLevel.Geometry &&
				pb_Editor.instance.selectionMode == SelectMode.Face)
				return MenuActionState.VisibleAndEnabled;

			return MenuActionState.Hidden;
		}

		public override void OnSettingsGUI()
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

			GUILayout.FlexibleSpace();
			
			if(GUILayout.Button("Grow Selection"))
				pb_Menu_Commands.MenuGrowSelection(pbUtil.GetComponents<pb_Object>(Selection.transforms));
		}

		public override pb_ActionResult DoAction()
		{
			return pb_Menu_Commands.MenuGrowSelection(selection);
		}
	}
}

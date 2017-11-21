using ProBuilder.Core;
using ProBuilder.EditorCore;
using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;

namespace ProBuilder.Actions
{
	class GrowSelection : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Selection; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Selection_Grow", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return m_Tooltip; } }

		static readonly pb_TooltipContent m_Tooltip = new pb_TooltipContent
		(
			"Grow Selection",
			@"Adds adjacent elements to the current selection, optionally testing to see if they are within a specified angle.

Grow by angle is enabbled by Option + Clicking the <b>Grow Selection</b> button.",
			CMD_ALT, 'G'
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					pb_MenuCommands.VerifyGrowSelection(selection);
		}

		public override bool IsHidden()
		{
			return 	editLevel != EditLevel.Geometry;
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

			bool angleGrow = pb_PreferencesInternal.GetBool(pb_Constant.pbGrowSelectionUsingAngle);

			EditorGUI.BeginChangeCheck();

			angleGrow = EditorGUILayout.Toggle("Restrict to Angle", angleGrow);
			float angleVal = pb_PreferencesInternal.GetFloat(pb_Constant.pbGrowSelectionAngle);

			GUI.enabled = angleGrow;

			angleVal = EditorGUILayout.FloatField("Max Angle", angleVal);

			GUI.enabled = angleGrow;

			bool iterative = angleGrow ? pb_PreferencesInternal.GetBool(pb_Constant.pbGrowSelectionAngleIterative) : true;

			iterative = EditorGUILayout.Toggle("Iterative", iterative);

			GUI.enabled = true;

			if( EditorGUI.EndChangeCheck() )
			{
				pb_PreferencesInternal.SetBool(pb_Constant.pbGrowSelectionUsingAngle, angleGrow);
				pb_PreferencesInternal.SetBool(pb_Constant.pbGrowSelectionAngleIterative, iterative);
				pb_PreferencesInternal.SetFloat(pb_Constant.pbGrowSelectionAngle, angleVal);
			}

			GUILayout.FlexibleSpace();


			if(GUILayout.Button("Grow Selection"))
				pb_MenuCommands.MenuGrowSelection(selection);
		}

		public override pb_ActionResult DoAction()
		{
			return pb_MenuCommands.MenuGrowSelection(selection);
		}
	}
}

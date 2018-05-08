using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using EditorGUILayout = UnityEditor.EditorGUILayout;
using EditorStyles = UnityEditor.EditorStyles;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class GrowSelection : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Selection; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Selection_Grow", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return s_Tooltip; }
		}

		static readonly TooltipContent s_Tooltip = new TooltipContent
		(
			"Grow Selection",
			@"Adds adjacent elements to the current selection, optionally testing to see if they are within a specified angle.

Grow by angle is enabbled by Option + Clicking the <b>Grow Selection</b> button.",
			keyCommandAlt, 'G'
		);

		public override bool IsEnabled()
		{
			return ProBuilderEditor.instance != null &&
				MenuCommands.VerifyGrowSelection(MeshSelection.Top());
		}

		public override bool IsHidden()
		{
			return editLevel != EditLevel.Geometry;
		}

		public override MenuActionState AltState()
		{
			if (IsEnabled() &&
				ProBuilderEditor.instance.editLevel == EditLevel.Geometry &&
				ProBuilderEditor.instance.selectionMode == SelectMode.Face)
				return MenuActionState.VisibleAndEnabled;

			return MenuActionState.Hidden;
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Grow Selection Options", EditorStyles.boldLabel);

			bool angleGrow = PreferencesInternal.GetBool(PreferenceKeys.pbGrowSelectionUsingAngle);

			EditorGUI.BeginChangeCheck();

			angleGrow = EditorGUILayout.Toggle("Restrict to Angle", angleGrow);
			float angleVal = PreferencesInternal.GetFloat(PreferenceKeys.pbGrowSelectionAngle);

			GUI.enabled = angleGrow;

			angleVal = EditorGUILayout.FloatField("Max Angle", angleVal);

			GUI.enabled = angleGrow;

			bool iterative = angleGrow ? PreferencesInternal.GetBool(PreferenceKeys.pbGrowSelectionAngleIterative) : true;

			iterative = EditorGUILayout.Toggle("Iterative", iterative);

			GUI.enabled = true;

			if (EditorGUI.EndChangeCheck())
			{
				PreferencesInternal.SetBool(PreferenceKeys.pbGrowSelectionUsingAngle, angleGrow);
				PreferencesInternal.SetBool(PreferenceKeys.pbGrowSelectionAngleIterative, iterative);
				PreferencesInternal.SetFloat(PreferenceKeys.pbGrowSelectionAngle, angleVal);
			}

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Grow Selection"))
				MenuCommands.MenuGrowSelection(MeshSelection.Top());
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuGrowSelection(MeshSelection.Top());
		}
	}
}

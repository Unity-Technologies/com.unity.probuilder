using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;
using System.Linq;
using ProBuilder.Core;
using ProBuilder.EditorCore;

namespace ProBuilder.Actions
{
	class FillHole : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Edge_FillHole", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return true; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Fill Hole",
			@"Create a new face connecting all selected vertices."
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					pb_Editor.instance.editLevel == EditLevel.Geometry &&
					pb_Editor.instance.selectionMode != SelectMode.Face &&
					selection != null &&
					selection.Length > 0;
		}

		public override bool IsHidden()
		{
			return 	pb_Editor.instance == null ||
					pb_Editor.instance.editLevel != EditLevel.Geometry ||
					pb_Editor.instance.selectionMode == SelectMode.Face;

		}

		public override MenuActionState AltState()
		{
			return MenuActionState.VisibleAndEnabled;
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Fill Hole Settings", EditorStyles.boldLabel);

			EditorGUILayout.HelpBox("Fill Hole can optionally fill entire holes (default) or just the selected vertices on the hole edges.\n\nIf no elements are selected, the entire object will be scanned for holes.", MessageType.Info);

			bool wholePath = pb_PreferencesInternal.GetBool(pb_Constant.pbFillHoleSelectsEntirePath);

			EditorGUI.BeginChangeCheck();

			wholePath = EditorGUILayout.Toggle("Fill Entire Hole", wholePath);

			if(EditorGUI.EndChangeCheck())
				pb_PreferencesInternal.SetBool(pb_Constant.pbFillHoleSelectsEntirePath, wholePath);

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Fill Hole"))
				pb_EditorUtility.ShowNotification( DoAction().notification );
		}

		public override pb_ActionResult DoAction()
		{
			return pb_MenuCommands.MenuFillHole(selection);
		}
	}
}



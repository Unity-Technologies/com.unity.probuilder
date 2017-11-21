using UnityEngine;
using UnityEditor;
using System.Linq;
using ProBuilder.Core;
using ProBuilder.EditorCore;
using ProBuilder.Interface;

namespace ProBuilder.Actions
{
	class BevelEdges : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Edge_Bevel", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return true; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Bevel",
			@"Smooth the selected edges by adding a slanted face connecting the two adjacent faces."
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					pb_Editor.instance.editLevel == EditLevel.Geometry &&
					selection != null &&
					selection.Length > 0 &&
					selection.Any(x => x.SelectedEdgeCount > 0);
		}

		public override MenuActionState AltState()
		{
			return MenuActionState.VisibleAndEnabled;
		}

		public override bool IsHidden()
		{
			return 	pb_Editor.instance == null ||
					editLevel != EditLevel.Geometry ||
					(selectionMode & (SelectMode.Face | SelectMode.Edge)) == 0;
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Bevel Edge Settings", EditorStyles.boldLabel);

			EditorGUILayout.HelpBox("Amount determines how much space the bevel takes up.  Bigger value means more bevel action.", MessageType.Info);

			float bevelAmount = pb_PreferencesInternal.GetFloat(pb_Constant.pbBevelAmount);

			EditorGUI.BeginChangeCheck();

			bevelAmount = pb_EditorGUIUtility.FreeSlider("Distance", bevelAmount, .001f, .99f);
			if(bevelAmount < .001f) bevelAmount = .001f;

			if(EditorGUI.EndChangeCheck())
				pb_PreferencesInternal.SetFloat(pb_Constant.pbBevelAmount, bevelAmount);

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Bevel Edges"))
				DoAction();
		}

		public override pb_ActionResult DoAction()
		{
			return pb_MenuCommands.MenuBevelEdges(selection);
		}
	}
}

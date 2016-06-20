using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;
using System.Linq;

namespace ProBuilder2.Actions
{
	public class BevelEdges : pb_MenuAction
	{
		public override pb_IconGroup group { get { return pb_IconGroup.Geometry; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Edge_Bevel"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return true; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Bevel Edges",
			@"Smooth the selected edges by adding a slanted face.",
			CMD_ALT, 'Z'
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
					pb_Editor.instance.editLevel != EditLevel.Geometry;
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Bevel Edge Settings", EditorStyles.boldLabel);

			EditorGUILayout.HelpBox("Amount determines how much space the bevel takes up.  Bigger value means more bevel action.", MessageType.Info);

			float bevelAmount = pb_Preferences_Internal.GetFloat(pb_Constant.pbBevelAmount);

			EditorGUI.BeginChangeCheck();

			bevelAmount = EditorGUILayout.FloatField("Amount", bevelAmount);

			if(EditorGUI.EndChangeCheck())
				EditorPrefs.SetFloat(pb_Constant.pbBevelAmount, bevelAmount);

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Bevel Edges"))
				DoAction();
		}

		public override pb_ActionResult DoAction()
		{
			return pb_Menu_Commands.MenuBevelEdges(selection);
		}
	}
}

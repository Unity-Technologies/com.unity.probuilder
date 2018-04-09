using ProBuilder.Core;
using ProBuilder.EditorCore;
using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;

namespace ProBuilder.Actions
{
	class OpenVertexColorEditor : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Tool; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Panel_VertColors", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override string menuTitle { get { return "Vertex Colors"; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Vertex Colors Editor",
			"Opens either the Vertex Color Palette or the Vertex Color Painter.\n\nThe Palette is useful for applying colors to selected faces with hard edges, where the Painter is good for brush strokes and soft edges.\n\nTo select which editor this button opens, Option + Click."
		);

		public override bool IsEnabled()
		{
			return pb_Editor.instance != null;
		}

		public override MenuActionState AltState()
		{
			return MenuActionState.VisibleAndEnabled;
		}

		public override pb_ActionResult DoAction()
		{
			pb_MenuCommands.MenuOpenVertexColorsEditor();
			return new pb_ActionResult(Status.Success, "Open Vertex Color Window");
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Vertex Color Editor", EditorStyles.boldLabel);

			VertexColorTool tool = pb_PreferencesInternal.GetEnum<VertexColorTool>(pb_Constant.pbVertexColorTool);
			VertexColorTool prev = tool;

			tool = (VertexColorTool) EditorGUILayout.EnumPopup("Editor", tool);

			if(prev != tool)
				pb_PreferencesInternal.SetInt(pb_Constant.pbVertexColorTool, (int)tool);

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Open Vertex Editor"))
				pb_MenuCommands.MenuOpenVertexColorsEditor();
		}
	}
}

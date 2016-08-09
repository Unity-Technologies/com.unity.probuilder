using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;

namespace ProBuilder2.Actions
{
	public class OpenVertexColorEditor : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Tool; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Panel_VertColors"); } }
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
			pb_Menu_Commands.MenuOpenVertexColorsEditor();
			return new pb_ActionResult(Status.Success, "Open Vertex Color Window");
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Vertex Color Editor", EditorStyles.boldLabel);

			VertexColorTool tool = pb_Preferences_Internal.GetEnum<VertexColorTool>(pb_Constant.pbVertexColorTool);
			VertexColorTool prev = tool;

			tool = (VertexColorTool) EditorGUILayout.EnumPopup("Editor", tool);

			if(prev != tool)
				EditorPrefs.SetInt(pb_Constant.pbVertexColorTool, (int)tool);

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Open Vertex Editor"))
				pb_Menu_Commands.MenuOpenVertexColorsEditor();
		}
	}
}

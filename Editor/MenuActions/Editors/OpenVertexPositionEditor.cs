using ProBuilder.Core;
using ProBuilder.EditorCore;
using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;

namespace ProBuilder.Actions
{
	class OpenVertexPositionEditor : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Tool; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override string menuTitle { get { return "Vertex Editor"; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Vertex Position Editor",
			"Opens the vertex positions editor window."
		);

		public override bool IsEnabled()
		{
			return pb_Editor.instance != null;
		}

		public override bool IsHidden()
		{
			return true;
		}

		public override pb_ActionResult DoAction()
		{
			pb_VertexEditor.MenuOpenVertexEditor();
			return new pb_ActionResult(Status.Success, "Open Vertex Editor Window");
		}
	}
}

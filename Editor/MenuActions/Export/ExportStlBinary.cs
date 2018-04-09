using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;
using System.Linq;
using Parabox.STL;
using ProBuilder.Core;
using ProBuilder.EditorCore;

namespace ProBuilder.Actions
{
	class ExportStlBinary : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Export; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return false; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Export Stl",
			@"Export an Stl model file."
		);

		public override bool IsHidden() { return true; }

		public override bool IsEnabled()
		{
			return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
		}

		public override pb_ActionResult DoAction()
		{
			if(!string.IsNullOrEmpty(ExportStlAscii.ExportWithFileDialog(Selection.gameObjects, FileType.Binary)))
				return new pb_ActionResult(Status.Success, "Export STL");
			else
				return new pb_ActionResult(Status.Canceled, "User Canceled");
		}
	}
}

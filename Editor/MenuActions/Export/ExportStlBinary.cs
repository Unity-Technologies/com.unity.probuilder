using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using Parabox.STL;
using ProBuilder.Core;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	class ExportStlBinary : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Export; } }
		public override Texture2D icon { get { return null; } }
		public override TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return false; } }

		static readonly TooltipContent _tooltip = new TooltipContent
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

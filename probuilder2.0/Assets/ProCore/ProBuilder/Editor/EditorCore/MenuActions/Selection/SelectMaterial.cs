using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;

namespace ProBuilder2.Actions
{
	public class SelectMaterial : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Selection; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Select Materials",
			"Selects all faces matching the selected materials."
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					pb_Editor.instance.editLevel != EditLevel.Top &&
					selectionMode == SelectMode.Face &&
					selection != null &&
					selection.Length > 0;
		}

		public override pb_ActionResult DoAction()
		{
			

			return new pb_ActionResult(Status.Success, "Select Faces with Material");
		}
	}
}



using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;
using System.Linq;
using ProBuilder.Core;
using ProBuilder.EditorCore;

namespace ProBuilder.Actions
{
	class SetPivotToSelection : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Pivot_CenterOnElements", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override string menuTitle { get { return "Set Pivot"; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Set Pivot to Center of Selection",
			@"Moves the pivot point of each mesh to the average of all selected elements positions.  This means the pivot point moves to where-ever the handle currently is.",
			CMD_SUPER, 'J'
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					selection != null &&
					selection.Length > 0 &&
					selection.Any(x => x.SelectedTriangleCount > 0);
		}

		public override bool IsHidden()
		{
			return editLevel != EditLevel.Geometry;
		}

		public override pb_ActionResult DoAction()
		{
			return pb_MenuCommands.MenuSetPivot( selection );
		}
	}
}


using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;
using System.Linq;

namespace ProBuilder2.Actions
{
	public class SetPivotToSelection : pb_MenuAction
	{
		public override pb_IconGroup group { get { return pb_IconGroup.Geometry; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Pivot_MoveToCenter"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override string MenuTitle { get { return "Set Pivot"; } }

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

		public override pb_ActionResult DoAction()
		{
			return pb_Menu_Commands.MenuSetPivot(selection);
		}
	}
}


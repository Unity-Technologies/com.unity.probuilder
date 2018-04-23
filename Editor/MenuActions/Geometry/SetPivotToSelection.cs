using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using ProBuilder.Core;
using UnityEditor.ProBuilder;

namespace ProBuilder.Actions
{
	class SetPivotToSelection : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Pivot_CenterOnElements", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }
		public override string menuTitle { get { return "Set Pivot"; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Set Pivot to Center of Selection",
			@"Moves the pivot point of each mesh to the average of all selected elements positions.  This means the pivot point moves to where-ever the handle currently is.",
			CMD_SUPER, 'J'
		);

		public override bool IsEnabled()
		{
			return 	ProBuilderEditor.instance != null &&
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
			return MenuCommands.MenuSetPivot( selection );
		}
	}
}


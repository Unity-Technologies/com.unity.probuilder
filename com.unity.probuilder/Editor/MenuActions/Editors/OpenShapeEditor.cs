using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class OpenShapeEditor : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Panel_Shapes", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }
		public override string menuTitle { get { return "New Shape"; } }
		public override int toolbarPriority { get { return 0; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"New Shape Tool",
			"Opens the Shape Editor window.\n\nThe Shape Editor is a window that allows you to interactively create new 3d primitves.",
			keyCommandSuper, keyCommandShift, 'K'
		);

		public override bool IsEnabled()
		{
			return true;
		}

		public override ActionResult DoAction()
		{
			ShapeEditor.MenuOpenShapeCreator();
			return new ActionResult(ActionResult.Status.Success, "Open Shape Tool");
		}
	}
}

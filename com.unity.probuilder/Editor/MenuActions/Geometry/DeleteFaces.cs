using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class DeleteFaces : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Geometry; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Face_Delete", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return s_Tooltip; }
		}

		static readonly TooltipContent s_Tooltip = new TooltipContent
		(
			"Delete Faces",
			@"Delete all selected faces.",
			keyCommandDelete
		);

		public override SelectMode validSelectModes
		{
			get { return SelectMode.Face; }
		}

		public override bool enabled
		{
			get { return base.enabled && MeshSelection.selectedFaceCount > 0; }
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuDeleteFace(MeshSelection.TopInternal());
		}
	}
}

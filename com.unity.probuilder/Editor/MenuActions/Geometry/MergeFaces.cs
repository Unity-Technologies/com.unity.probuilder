using UnityEngine;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class MergeFaces : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Geometry; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Face_Merge", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return s_Tooltip; }
		}

		static readonly TooltipContent s_Tooltip = new TooltipContent
		(
			"Merge Faces",
			@"Tells ProBuilder to treat the selected faces as if they were a single face.  Be careful not to use this with unconnected faces!"
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
			return MenuCommands.MenuMergeFaces(MeshSelection.TopInternal());
		}
	}
}

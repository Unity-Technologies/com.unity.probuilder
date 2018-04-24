using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	class MergeFaces : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Face_Merge", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return true; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Merge Faces",
			@"Tells ProBuilder to treat the selected faces as if they were a single face.  Be careful not to use this with unconnected faces!"
		);

		public override bool IsEnabled()
		{
			return 	ProBuilderEditor.instance != null &&
					selection != null &&
					selection.Length > 0 &&
					selection.Any(x => x.SelectedFaceCount > 1);
		}

		public override bool IsHidden()
		{
			return 	ProBuilderEditor.instance == null ||
					ProBuilderEditor.instance.editLevel != EditLevel.Geometry ||
					ProBuilderEditor.instance.selectionMode != SelectMode.Face;
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuMergeFaces(selection);
		}
	}
}

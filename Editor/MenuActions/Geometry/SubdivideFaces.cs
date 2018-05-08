using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class SubdivideFaces : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Geometry; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Face_Subdivide", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return _tooltip; }
		}

		protected override bool hasFileMenuEntry
		{
			get { return false; }
		}

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Subdivide Faces",
			@"Inserts a new vertex at the center of each selected face and creates a new edge from the center of each perimeter edge to the center vertex.",
			keyCommandAlt, 'S'
		);

		public override bool IsEnabled()
		{
			return ProBuilderEditor.instance != null &&
				MeshSelection.Top().Any(x => x.selectedFaceCount > 0);
		}

		public override bool IsHidden()
		{
			return ProBuilderEditor.instance == null ||
				ProBuilderEditor.instance.editLevel != EditLevel.Geometry ||
				ProBuilderEditor.instance.selectionMode != SelectMode.Face;
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuSubdivideFace(MeshSelection.Top());
		}
	}
}

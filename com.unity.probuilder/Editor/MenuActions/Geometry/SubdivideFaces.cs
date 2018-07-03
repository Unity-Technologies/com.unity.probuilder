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

		public override bool enabled
		{
			get
			{
				return ProBuilderEditor.instance != null &&
					MeshSelection.TopInternal().Any(x => x.selectedFaceCount > 0);
			}
		}

		public override bool hidden
		{
			get
			{
				return ProBuilderEditor.instance == null ||
					ProBuilderEditor.editLevel != EditLevel.Geometry ||
					ProBuilderEditor.componentMode != ComponentMode.Face;
			}
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuSubdivideFace(MeshSelection.TopInternal());
		}
	}
}

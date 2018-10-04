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
			get { return s_Tooltip; }
		}

		protected override bool hasFileMenuEntry
		{
			get { return false; }
		}

		static readonly TooltipContent s_Tooltip = new TooltipContent
		(
			"Subdivide Faces",
			@"Inserts a new vertex at the center of each selected face and creates a new edge from the center of each perimeter edge to the center vertex.",
			keyCommandAlt, 'S'
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
			return MenuCommands.MenuSubdivideFace(MeshSelection.topInternal);
		}
	}
}

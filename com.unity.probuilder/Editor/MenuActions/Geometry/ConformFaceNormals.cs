using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class ConformFaceNormals : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Face_ConformNormals", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }
		public override string menuTitle { get { return "Conform Normals"; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Conform Face Normals",
			@"Orients all selected faces to face the same direction."
		);

		public override bool enabled
		{
			get
			{
				return ProBuilderEditor.instance != null &&
					MeshSelection.TopInternal().Any(x => x.selectedFaceCount > 1);
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
			return MenuCommands.MenuConformNormals(MeshSelection.TopInternal());
		}
	}
}


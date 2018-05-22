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

		public override bool IsEnabled()
		{
			return 	ProBuilderEditor.instance != null &&
				MeshSelection.Top().Any(x => x.selectedFaceCount > 1);
		}

		public override bool IsHidden()
		{
			return 	ProBuilderEditor.instance == null ||
					ProBuilderEditor.instance.editLevel != EditLevel.Geometry ||
					ProBuilderEditor.instance.selectionMode != SelectMode.Face;
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuConformNormals(MeshSelection.Top());
		}
	}
}


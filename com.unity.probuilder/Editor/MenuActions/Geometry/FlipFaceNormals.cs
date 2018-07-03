using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class FlipFaceNormals : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Face_FlipNormals", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Flip Face Normals",
			@"Reverses the direction of all faces in selection.",
			keyCommandAlt, 'N'
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
			return MenuCommands.MenuFlipNormals(MeshSelection.TopInternal());
		}
	}
}

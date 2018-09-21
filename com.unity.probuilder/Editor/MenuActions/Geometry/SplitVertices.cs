using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class SplitVertices : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Geometry; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Vert_Split", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return s_Tooltip; }
		}

		static readonly TooltipContent s_Tooltip = new TooltipContent
		(
			"Split Vertices",
			@"Disconnects vertices that share the same position in space so that they may be moved independently of one another.",
			keyCommandAlt, 'X'
		);

		public override SelectMode validSelectModes
		{
			get { return SelectMode.Vertex; }
		}

		public override bool enabled
		{
			get { return base.enabled && MeshSelection.selectedVertexCount > 0; }
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuSplitVertices(MeshSelection.TopInternal());
		}
	}
}

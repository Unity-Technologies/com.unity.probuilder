using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class OpenLightmapUVEditor : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
		public override Texture2D icon { get { return null; } }
		public override TooltipContent tooltip { get { return _tooltip; } }
		public override string menuTitle { get { return "Lightmap UV Editor"; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Lightmap UV Editor",
			""
		);

		public override bool enabled
		{
			get { return true; }
		}

		public override bool hidden
		{
			get { return true; }
		}

		public override ActionResult DoAction()
		{
			EditorWindow.GetWindow<LightmapUVEditor>(true, "Lightmap UV Editor", true).position = LightmapUVEditor.desiredPosition;
			return new ActionResult(ActionResult.Status.Success, "Open Lightmap UV Editor Window");
		}
	}
}

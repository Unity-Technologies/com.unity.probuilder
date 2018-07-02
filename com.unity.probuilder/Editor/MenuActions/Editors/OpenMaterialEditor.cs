using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using MaterialEditor = UnityEditor.ProBuilder.MaterialEditor;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class OpenMaterialEditor : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Panel_Materials", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Material Editor",
			"Opens the Material Editor window.\n\nThe Material Editor window applies materials to selected faces or objects."
		);

		public override bool enabled
		{
			get { return ProBuilderEditor.instance != null; }
		}

		public override ActionResult DoAction()
		{
			MaterialEditor.MenuOpenMaterialEditor();
			return new ActionResult(ActionResult.Status.Success, "Open Materials Window");
		}
	}
}

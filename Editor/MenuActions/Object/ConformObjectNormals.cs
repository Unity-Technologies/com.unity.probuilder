using ProBuilder.Core;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder.Actions
{
	class ConformObjectNormals : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Object; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Object_ConformNormals", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }
		public override string menuTitle { get { return "Conform Normals"; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Conform Object Normals",
			@"Check the object for faces that are flipped in the opposite direction of most other faces, then reverses any dissenters."
		);

		public override bool IsEnabled()
		{
			return 	ProBuilderEditor.instance != null && selection != null && selection.Length > 0;
		}

		public override pb_ActionResult DoAction()
		{
			return MenuCommands.MenuConformObjectNormals(selection);
		}
	}
}



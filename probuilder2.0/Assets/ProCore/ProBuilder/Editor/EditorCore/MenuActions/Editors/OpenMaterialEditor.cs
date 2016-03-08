using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;

namespace ProBuilder2.Actions
{
	public class OpenMaterialEditor : pb_MenuAction
	{
		public override string group { get { return "Editor"; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Panel_Materials"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Material Editor",
			"Opens the Material Editor window.\n\nThe Material Editor window applies materials to selected faces or objects."
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null;
		}

		public override pb_ActionResult DoAction()
		{
			pb_Material_Editor.MenuOpenMaterialEditor();
			return new pb_ActionResult(Status.Success, "Open Materials Window");
		}
	}
}

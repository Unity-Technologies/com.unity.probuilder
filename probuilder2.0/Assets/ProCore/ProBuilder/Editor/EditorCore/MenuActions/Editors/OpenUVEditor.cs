using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;

namespace ProBuilder2.Actions
{
	public class OpenUVEditor : pb_MenuAction
	{
		public override string group { get { return "Editor"; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Panel_UVEditor"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"UV Editor",
			"Opens the UV Editor window.\n\nThe UV Editor allows you to change how textures are rendered on this mesh."
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null;
		}

		public override pb_ActionResult DoAction()
		{
			pb_UV_Editor.MenuOpenUVEditor();
			return new pb_ActionResult(Status.Success, "Open UV Window");
		}
	}
}


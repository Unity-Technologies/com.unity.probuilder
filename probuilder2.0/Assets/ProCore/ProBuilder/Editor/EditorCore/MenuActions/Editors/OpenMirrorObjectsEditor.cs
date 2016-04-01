using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;

namespace ProBuilder2.Actions
{
	public class OpenMirrorObjectsEditor : pb_MenuAction
	{
		public override pb_IconGroup group { get { return pb_IconGroup.Tool; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Object_Mirror"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override string MenuTitle { get { return "Mirror"; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Mirror Objects Editor",
			"Opens the Mirror Editor.\n\nMirroring objects will duplicate an flip objects on the specified axes."
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null;
		}

		public override pb_ActionResult DoAction()
		{
			pb_Mirror_Tool.MenuOpenMirrorEditor();
			return new pb_ActionResult(Status.Success, "Open Mirror Editor");
		}
	}
}

using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;

namespace ProBuilder2.Actions
{
	public class OpenSmoothingEditor : pb_MenuAction
	{
		public override pb_IconGroup group { get { return pb_IconGroup.Tool; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Panel_Smoothing"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Smoothing Groups Editor",
@"Opens the Smoothing Group Editor.

Smoothing groups average the vertex normals with neighboring planes. This allows lighting to behave in a more realistic manner when dealing with edges that are intended to be smooth.

ProBuilder decides which edges should be smoothed by checking for neighboring faces that are in the same group. It also checks for Hard groups, which hardens edges of neighboring faces."
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null;
		}

		public override pb_ActionResult DoAction()
		{
			pb_Smoothing_Editor.MenuOpenSmoothingEditor();

			return new pb_ActionResult(Status.Success, "Open Smoothing Groups Editor");
		}
	}
}

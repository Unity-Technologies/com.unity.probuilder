using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;
using System.Linq;

namespace ProBuilder2.Actions
{
	public class DeleteFaces : pb_MenuAction
	{
		public override pb_IconGroup group { get { return pb_IconGroup.Geometry; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Face_Delete"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Delete Faces",
			@"Delete all selected faces.",
			CMD_DELETE
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					selection != null &&
					selection.Length > 0 &&
					selection.Sum(x => x.SelectedFaceCount) > 0;
		}

		public override pb_ActionResult DoAction()
		{
			return pb_Menu_Commands.MenuDeleteFace(selection);
		}
	}
}

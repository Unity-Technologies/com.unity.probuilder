using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;
using System.Linq;

namespace ProBuilder2.Actions
{
	public class ConformFaceNormals : pb_MenuAction
	{
		public override pb_IconGroup group { get { return pb_IconGroup.Geometry; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Face_ConformNormals"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override string MenuTitle { get { return "Conform Normals"; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Conform Face Normals",
			@"Orients all selected faces to face the same direction."
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					selection != null &&
					selection.Length > 0 &&
					selection.Any(x => x.SelectedFaceCount > 1);
		}

		public override bool IsHidden()
		{
			return 	pb_Editor.instance == null ||
					pb_Editor.instance.editLevel != EditLevel.Geometry ||
					pb_Editor.instance.selectionMode != SelectMode.Face;
		}

		public override pb_ActionResult DoAction()
		{
			return pb_Menu_Commands.MenuConformNormals(selection);
		}
	}
}


using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;

namespace ProBuilder2.Actions
{
	public class ConformObjectNormals : pb_MenuAction
	{
		public override pb_IconGroup group { get { return pb_IconGroup.Object; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Object_ConformNormals"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override string MenuTitle { get { return "Conform Normals"; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Conform Object Normals",
			@"Check the object for faces that are flipped in the opposite direction of most other faces, then reverses any dissenters."
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null && selection != null && selection.Length > 0;
		}

		public override pb_ActionResult DoAction()
		{
			return pb_Menu_Commands.MenuConformObjectNormals(selection);
		}
	}
}



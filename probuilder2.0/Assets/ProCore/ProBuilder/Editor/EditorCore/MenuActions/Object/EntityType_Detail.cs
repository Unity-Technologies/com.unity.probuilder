using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;

namespace ProBuilder2.Actions
{
	public class EntityType_Detail : pb_MenuAction
	{
		public override pb_IconGroup group { get { return pb_IconGroup.Object; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Set Entity Type: Detail",
			@"Set the selected objects to Detail entity types.

A Detail type is marked with all static flags except Occluding and Reflection Probes."
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null && selection != null && selection.Length > 0;
		}

		public override pb_ActionResult DoAction()
		{
			return pb_Menu_Commands.MenuSubdivide(selection);
		}
	}
}

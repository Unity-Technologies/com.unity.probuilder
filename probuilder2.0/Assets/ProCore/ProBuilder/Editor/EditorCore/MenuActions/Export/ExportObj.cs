using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;
using System.Linq;
using Parabox.STL;

namespace ProBuilder2.Actions
{
	public class ExportObj : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Export; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return false; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Export Obj",
			"Export a Wavefront OBJ file."
		);

		public override bool IsHidden() { return true; }

		public override bool IsEnabled()
		{
			return Selection.gameObjects != null && Selection.gameObjects.Length > 0;
		}

		public override pb_ActionResult DoAction()
		{
			string res = pb_EditorUtility.ExportOBJ(pbUtil.GetComponents<pb_Object>(Selection.transforms));

			if( string.IsNullOrEmpty(res) )
				return new pb_ActionResult(Status.Canceled, "User Canceled");
			else
				return new pb_ActionResult(Status.Success, "Export OBJ");
		}
	}
}

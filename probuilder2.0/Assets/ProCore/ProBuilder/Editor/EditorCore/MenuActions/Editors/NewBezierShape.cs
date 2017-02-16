using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;

namespace ProBuilder2.Actions
{
	public class NewBezierShape : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Tool; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Panel_Shapes"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override string menuTitle { get { return "New Bezier Shape"; } }
		public override int toolbarPriority { get { return 1; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"New Bezier Shape",
			"Creates a new shape that is built by extruding along a bezier spline."
		);

		public override bool IsEnabled()
		{
			return true;
		}

		public override pb_ActionResult DoAction()
		{
			GameObject go = new GameObject();
			pb_BezierShape bezier = go.AddComponent<pb_BezierShape>();
			bezier.Init();
			pb_Object pb = bezier.gameObject.AddComponent<pb_Object>();
			bezier.Refresh();
			pb_EditorUtility.InitObject(pb);
			pb_Selection.SetSelection(go);

			return new pb_ActionResult(Status.Success, "Create Bezier Shape");
		}
	}
}

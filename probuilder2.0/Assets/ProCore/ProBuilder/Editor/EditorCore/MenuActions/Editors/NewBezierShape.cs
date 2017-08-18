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
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/NewBezierSpline"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override string menuTitle { get { return "New Bezier Shape"; } }
		public override int toolbarPriority { get { return 1; } }
		private bool experimentalFeaturesEnabled = false;
		public override bool isProOnly { get { return true; } }

		public NewBezierShape()
		{
			experimentalFeaturesEnabled = pb_PreferencesInternal.GetBool(pb_Constant.pbEnableExperimental);
		}

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"New Bezier Shape",
			"Creates a new shape that is built by extruding along a bezier spline."
		);

		public override bool IsHidden()
		{
			return !experimentalFeaturesEnabled;
		}

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
			pbUndo.RegisterCreatedObjectUndo(go, "Create Bezier Shape");
			bezier.m_IsEditing = true;

			return new pb_ActionResult(Status.Success, "Create Bezier Shape");
		}
	}
}

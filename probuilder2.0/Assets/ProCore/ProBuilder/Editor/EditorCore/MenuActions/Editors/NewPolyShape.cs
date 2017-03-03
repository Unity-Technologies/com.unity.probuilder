using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;

namespace ProBuilder2.Actions
{
	public class NewPolyShape : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Tool; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Panel_Shapes"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override string menuTitle { get { return "New Poly Shape"; } }
		public override int toolbarPriority { get { return 1; } }
		private bool experimentalFeaturesEnabled = false;

		public NewPolyShape()
		{
			experimentalFeaturesEnabled = pb_Preferences_Internal.GetBool(pb_Constant.pbEnableExperimental);
		}

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"New Polygon Shape",
			"Creates a new shape by clicking around a perimeter and extruding."
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
			pb_PolyShape poly = go.AddComponent<pb_PolyShape>();
			poly.Init();
			pb_Object pb = poly.gameObject.AddComponent<pb_Object>();
			poly.Refresh();
			pb_EditorUtility.InitObject(pb);
			pb_Selection.SetSelection(go);
			pbUndo.RegisterCreatedObjectUndo(go, "Create Poly Shape");

			return new pb_ActionResult(Status.Success, "Create Poly Shape");
		}
	}
}

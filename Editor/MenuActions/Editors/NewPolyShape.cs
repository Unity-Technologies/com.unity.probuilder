using ProBuilder.Core;
using ProBuilder.EditorCore;
using UnityEngine;
using UnityEditor;
using ProBuilder.MeshOperations;
using ProBuilder.Interface;

namespace ProBuilder.Actions
{
	class NewPolyShape : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Tool; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/NewPolyShape", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override string menuTitle { get { return "New Poly Shape"; } }
		public override int toolbarPriority { get { return 1; } }
		public override bool isProOnly { get { return true; } }

		public NewPolyShape()
		{}

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"New Polygon Shape",
			"Creates a new shape by clicking around a perimeter and extruding."
		);

		public override bool IsHidden()
		{
			return false;
		}

		public override bool IsEnabled()
		{
			return true;
		}

		public override pb_ActionResult DoAction()
		{
			GameObject go = new GameObject();
			pb_PolyShape poly = go.AddComponent<pb_PolyShape>();
			pb_Object pb = poly.gameObject.AddComponent<pb_Object>();
			pb.CreateShapeFromPolygon(poly.points, poly.extrude, poly.flipNormals);
			pb_EditorUtility.InitObject(pb);
			pb_Selection.SetSelection(go);
			pb_Undo.RegisterCreatedObjectUndo(go, "Create Poly Shape");
			poly.polyEditMode = pb_PolyShape.PolyEditMode.Path;

			Vector3 pivot;

			if(pb_ProGridsInterface.GetPivot(out pivot))
				go.transform.position = pivot;

			return new pb_ActionResult(Status.Success, "Create Poly Shape");
		}
	}
}

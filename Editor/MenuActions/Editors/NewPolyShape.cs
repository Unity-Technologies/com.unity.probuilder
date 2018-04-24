using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using ProBuilder.MeshOperations;
using UnityEditor.ProBuilder.UI;
using EditorUtility = UnityEditor.ProBuilder.EditorUtility;

namespace UnityEditor.ProBuilder.Actions
{
	class NewPolyShape : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/NewPolyShape", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }
		public override string menuTitle { get { return "New Poly Shape"; } }
		public override int toolbarPriority { get { return 1; } }
		public override bool isProOnly { get { return true; } }

		public NewPolyShape()
		{}

		static readonly TooltipContent _tooltip = new TooltipContent
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

		public override ActionResult DoAction()
		{
			GameObject go = new GameObject();
			pb_PolyShape poly = go.AddComponent<pb_PolyShape>();
			pb_Object pb = poly.gameObject.AddComponent<pb_Object>();
			pb.CreateShapeFromPolygon(poly.points, poly.extrude, poly.flipNormals);
			EditorUtility.InitObject(pb);
			MeshSelection.SetSelection(go);
			UndoUtility.RegisterCreatedObjectUndo(go, "Create Poly Shape");
			poly.polyEditMode = pb_PolyShape.PolyEditMode.Path;

			Vector3 pivot;

			if(ProGridsInterface.GetPivot(out pivot))
				go.transform.position = pivot;

			return new ActionResult(Status.Success, "Create Poly Shape");
		}
	}
}

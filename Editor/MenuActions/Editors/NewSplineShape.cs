using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    public class NewSplineShape : MenuAction
    {
        const string k_IconPath = "Toolbar/NewBezierSpline";

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Tool; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon(k_IconPath, IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return _tooltip; }
        }

        public override string menuTitle
        {
            get { return "New Spline Shape"; }
        }

        public override int toolbarPriority
        {
            get { return 1; }
        }

        static readonly TooltipContent _tooltip = new TooltipContent
        (
            "New Spline Shape",
            "Creates a new shape that is built by extruding along a spline."
        );

        public override bool enabled
        {
            get { return true; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            GameObject go = new GameObject();
            var spline = go.AddComponent<SplineShape>();
            go.GetComponent<MeshRenderer>().sharedMaterial = EditorMaterialUtility.GetUserMaterial();
            spline.Init();
            EditorUtility.InitObject(spline.GetComponent<ProBuilderMesh>());
            MeshSelection.SetSelection(go);
            UndoUtility.RegisterCreatedObjectUndo(go, "Create Spline Shape");

            return new ActionResult(ActionResult.Status.Success, "Create Spline Shape");
        }
    }
}

using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    public class NewPolySplineShape : MenuAction
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
            get { return "New PolySpline Shape"; }
        }

        public override int toolbarPriority
        {
            get { return 1; }
        }

        static readonly TooltipContent _tooltip = new TooltipContent
        (
            "New PolySpline Shape",
            "Creates a new shape that is built by connecting several splines together."
        );

        public override bool enabled
        {
            get { return true; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            GameObject go = new GameObject();
            var polyspline = go.AddComponent<PolySplineShape>();
            go.GetComponent<MeshRenderer>().sharedMaterial = EditorMaterialUtility.GetUserMaterial();
            polyspline.Init();
            EditorUtility.InitObject(polyspline.GetComponent<ProBuilderMesh>());
            MeshSelection.SetSelection(go);
            UndoUtility.RegisterCreatedObjectUndo(go, "Create PolySpline Shape");

            return new ActionResult(ActionResult.Status.Success, "Create PolySpline Shape");
        }
    }
}

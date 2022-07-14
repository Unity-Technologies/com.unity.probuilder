using UnityEngine;
using UnityEngine.ProBuilder;


namespace UnityEditor.ProBuilder.Actions
{
    [ProBuilderMenuAction]
    sealed class NewBezierMesh : MenuAction
    {
        const string k_IconPath = "Toolbar/NewBezierSpline";

        static readonly TooltipContent s_Tooltip = new TooltipContent
        (
            "New Bezier Mesh",
            "Create a new shape that is built by extruding along a spline."
        );

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
            get { return s_Tooltip; }
        }

        public override string menuTitle
        {
            get { return "New Bezier Mesh"; }
        }

        public override bool hidden
        {
            get { return !Experimental.experimentalFeaturesEnabled; }
        }

        public override bool enabled
        {
            get { return Experimental.experimentalFeaturesEnabled && ProBuilderEditor.instance != null; }
        }

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Any; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            GameObject go = new GameObject();
            var bezier = go.AddComponent<BezierMesh>();
            go.GetComponent<MeshRenderer>().sharedMaterial = EditorMaterialUtility.GetUserMaterial();
            bezier.Init();
            bezier.ExtrudeMesh();
            UndoUtility.RegisterCreatedObjectUndo(go, "Create Bezier Mesh");

            return new ActionResult(ActionResult.Status.Success, "Created Bezier Mesh using Splines");
        }
    }
}

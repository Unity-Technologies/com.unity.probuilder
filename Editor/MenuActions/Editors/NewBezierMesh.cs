#if USING_SPLINES

using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class NewBezierMesh : MenuAction
    {
        const string k_IconPath = "Toolbar/NewBezierSpline";

        static readonly TooltipContent s_Tooltip = new TooltipContent
        (
            "New Bezier Mesh",
            "Create a new shape that is built by extruding along a Spline."
        );

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Tool; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon(k_IconPath); }
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
            GameObject go = new GameObject("New Bezier Mesh");
            UndoUtility.RegisterCreatedObjectUndo(go, "Create Bezier Mesh");
            go.AddComponent<BezierMesh>();
            go.GetComponent<MeshRenderer>().sharedMaterial = EditorMaterialUtility.GetUserMaterial();
            Selection.activeObject = go;

            EditorApplication.delayCall += BezierMeshEditor.SetSplineToolContext;

            return new ActionResult(ActionResult.Status.Success, "Created Bezier Mesh using Splines");
        }
    }
}
#endif

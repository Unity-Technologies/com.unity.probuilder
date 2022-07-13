using UnityEngine;
using UnityEngine.ProBuilder;


namespace UnityEditor.ProBuilder.Actions
{
    [ProBuilderMenuAction]
    sealed class NewBezierMesh : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Tool; }
        }

        public override Texture2D icon
        {
            get { return null; }
        }

        public override TooltipContent tooltip
        {
            get { return k_Tooltip; }
        }

        public override string menuTitle
        {
            get { return "New Bezier Mesh"; }
        }

        static readonly TooltipContent k_Tooltip = new TooltipContent
        (
            "New Bezier Mesh",
            "Create a bezier mesh using splines package."
        );

        public override bool enabled
        {
            get { return true; }
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
            return new ActionResult(ActionResult.Status.Success, "Created Bezier Mesh using Splines");
        }
    }
}

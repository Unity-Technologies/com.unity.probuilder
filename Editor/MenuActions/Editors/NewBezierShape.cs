using UnityEngine.ProBuilder;
using UnityEngine;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class NewBezierShape : MenuAction
    {
        const string k_IconPath = "Toolbar/NewBezierSpline";

        public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }

        public override string iconPath => k_IconPath;
        public override Texture2D icon => IconUtility.GetIcon(k_IconPath, IconSkin.Pro);
        public override TooltipContent tooltip { get { return _tooltip; } }
        public override string menuTitle { get { return "New Bezier Shape"; } }
        public override int toolbarPriority { get { return 1; } }

        static readonly TooltipContent _tooltip = new TooltipContent
            (
                "New Bezier Shape",
                "Creates a new shape that is built by extruding along a bezier spline."
            );

        public override bool hidden
        {
            get { return !Experimental.experimentalFeaturesEnabled; }
        }

        public override bool enabled
        {
            get { return Experimental.experimentalFeaturesEnabled; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            GameObject go = new GameObject();
            var bezier = go.AddComponent<BezierShape>();
            go.GetComponent<MeshRenderer>().sharedMaterial = EditorMaterialUtility.GetUserMaterial();
            bezier.Init();
            bezier.Refresh();
            UndoUtility.RegisterCreatedObjectUndo(go, "Create Bezier Shape");
            EditorUtility.InitObject(bezier.GetComponent<ProBuilderMesh>());
            MeshSelection.SetSelection(go);
            bezier.isEditing = true;

            return new ActionResult(ActionResult.Status.Success, "Create Bezier Shape");
        }
    }
}

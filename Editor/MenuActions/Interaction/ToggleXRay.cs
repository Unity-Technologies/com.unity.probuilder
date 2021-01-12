using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ToggleXRay : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Selection; }
        }

        public override Texture2D icon
        {
            get { return ProBuilderEditor.backfaceSelectionEnabled ? m_Icons[1] : m_Icons[0]; }
        }

        public override TooltipContent tooltip
        {
            get { return k_Tooltip; }
        }

        public override int toolbarPriority
        {
            get { return 1; }
        }

        // Menu bar item only access for now
        public override bool hidden => true;

        static readonly TooltipContent k_Tooltip = new TooltipContent
            (
                "Toggle X-Ray View",
                @"When enabled, selected mesh elements that are occluded by geometry will be rendered with a faded appearance.",
                keyCommandAlt, keyCommandShift, 'x'
                );

        public override string menuTitle
        {
            get { return EditorHandleDrawing.xRay ? "X-Ray: On" : "X-Ray: Off"; }
        }

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face | SelectMode.TextureFace; }
        }

        readonly Texture2D[] m_Icons;

        public ToggleXRay()
        {
            m_Icons = new Texture2D[]
            {
                IconUtility.GetIcon("Toolbar/Selection_SelectHidden-Off", IconSkin.Pro),
                IconUtility.GetIcon("Toolbar/Selection_SelectHidden-On", IconSkin.Pro)
            };
        }

        protected override ActionResult PerformActionImplementation()
        {
            EditorHandleDrawing.xRay = !EditorHandleDrawing.xRay;
            SceneView.RepaintAll();
            return new ActionResult(ActionResult.Status.Success, "Set X-Ray Vision\n" + (EditorHandleDrawing.xRay ? "On" : "Off"));
        }
    }
}

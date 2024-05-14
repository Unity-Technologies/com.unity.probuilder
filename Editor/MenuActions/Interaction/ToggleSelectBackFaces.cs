using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ToggleSelectBackFaces : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Selection; }
        }

        public override string iconPath => "Toolbar/Selection_SelectHidden-Off";
        public override Texture2D icon => ProBuilderEditor.backfaceSelectionEnabled ? m_Icons[1] : m_Icons[0];

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        public override int toolbarPriority
        {
            get { return 1; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Set Hidden Element Selection",
                "Setting Hidden Element Selection to On allows you to select faces that are either obscured by geometry or facing away from the scene camera (backfaces).\n\nThe default value is On.");

        public override string menuTitle
        {
            get { return ProBuilderEditor.backfaceSelectionEnabled ? "Select Hidden: On" : "Select Hidden: Off"; }
        }

        public override SelectMode validSelectModes
        {
            get
            {
                return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face | SelectMode.TextureFace;
            }
        }

        internal Texture2D[] icons => m_Icons;
        Texture2D[] m_Icons;

        public ToggleSelectBackFaces()
        {
            m_Icons = new Texture2D[]
            {
                IconUtility.GetIcon("Toolbar/Selection_SelectHidden-Off"),
                IconUtility.GetIcon("Toolbar/Selection_SelectHidden-On")
            };
        }

        internal override bool IsMenuItemChecked() => ProBuilderEditor.backfaceSelectionEnabled;

        protected override ActionResult PerformActionImplementation()
        {
            ProBuilderEditor.backfaceSelectionEnabled = !ProBuilderEditor.backfaceSelectionEnabled;
            return new ActionResult(ActionResult.Status.Success, "Set Hidden Element Selection\n" + (!ProBuilderEditor.backfaceSelectionEnabled ? "On" : "Off"));
        }
    }
}

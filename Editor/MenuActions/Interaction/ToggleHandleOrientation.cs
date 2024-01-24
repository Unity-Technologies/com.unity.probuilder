using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ToggleHandleOrientation : MenuAction
    {
        Texture2D[] m_Icons;

        internal HandleOrientation handleOrientation
        {
            get { return VertexManipulationTool.handleOrientation; }
            set
            {
                VertexManipulationTool.handleOrientation = value;
                ProBuilderEditor.Refresh(false);
            }
        }

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Selection; }
        }

        public override string iconPath => "Modes/ToolHandleElement";
        public override Texture2D icon => m_Icons[(int)handleOrientation];

        public override int toolbarPriority
        {
            get { return 0; }
        }

        public override TooltipContent tooltip
        {
            get { return k_Tooltips[(int)handleOrientation]; }
        }

        internal static TooltipContent[] tooltips => k_Tooltips;
        static readonly TooltipContent[] k_Tooltips = new TooltipContent[]
        {
            new TooltipContent("Global", "The transform handle is oriented in a fixed direction."),
            new TooltipContent("Local", "The transform handle is aligned with the active object rotation."),
            new TooltipContent("Element", "The transform handle is aligned with the active element selection.")
        };

        public override string menuTitle
        {
            get { return "Orientation: " + k_Tooltips[(int)handleOrientation].title; }
        }

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face; }
        }

        public override bool hidden
        {
            get { return false; }
        }

        const string k_NormalRotationIconPath = "Packages/com.unity.probuilder/Content/Icons/Modes/ToolHandleElement.png";

        public ToggleHandleOrientation()
        {
            m_Icons = new Texture2D[]
            {
                IconUtility.GetIcon("Modes/ToolHandleGlobal"),
                IconUtility.GetIcon("Modes/ToolHandleLocal"),
                IconUtility.GetIcon("Modes/ToolHandleElement")
            };
        }

        protected override ActionResult PerformActionImplementation()
        {
            handleOrientation = InternalUtility.NextEnumValue(handleOrientation);
            return new ActionResult(ActionResult.Status.Success, "Set Handle Orientation\n" + k_Tooltips[(int)handleOrientation].title);
        }

        public override bool enabled
        {
            get { return ProBuilderEditor.instance != null; }
        }

        public override void RegisterChangedCallbacks() => VertexManipulationTool.pivotRotationChanged += ContentsChanged;

        public override void UnregisterChangedCallbacks() => VertexManipulationTool.pivotRotationChanged -= ContentsChanged;
    }
}

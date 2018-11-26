using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ToggleHandleOrientation : MenuAction
    {
        Texture2D[] m_Icons;

        HandleOrientation handleOrientation
        {
            get { return VertexManipulationTool.handleOrientation; }
            set { VertexManipulationTool.handleOrientation = value; }
        }

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Selection; }
        }

        public override Texture2D icon
        {
            get { return m_Icons[(int)handleOrientation]; }
        }

        public override int toolbarPriority
        {
            get { return 0; }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltips[(int)handleOrientation]; }
        }

        static readonly TooltipContent[] s_Tooltips = new TooltipContent[]
        {
#if PROBUILDER_ENABLE_HANDLE_OVERRIDE
            new TooltipContent("World", "The transform handle is oriented in a fixed direction.", 'P'),
            new TooltipContent("Object", "The transform handle is aligned with the active object rotation.", 'P'),
            new TooltipContent("Active", "The transform handle is aligned with the active element selection.", 'P')
#else
            new TooltipContent("Global", "The transform handle is oriented in a fixed direction.", 'P'),
            new TooltipContent("Local", "The transform handle is aligned with the active object rotation.", 'P'),
            new TooltipContent("Active", "The transform handle is aligned with the active element selection.", 'P')
#endif
        };

        public override string menuTitle
        {
            get { return "Orientation: " + s_Tooltips[(int)handleOrientation].title; }
        }

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face; }
        }

        public override bool hidden
        {
            get { return false; }
        }

        public ToggleHandleOrientation()
        {
            m_Icons = new Texture2D[]
            {
                IconUtility.GetIcon("Toolbar/HandleAlign_World", IconSkin.Pro),
                IconUtility.GetIcon("Toolbar/HandleAlign_Local", IconSkin.Pro),
                IconUtility.GetIcon("Toolbar/HandleAlign_Plane", IconSkin.Pro),
            };
        }

        public override ActionResult DoAction()
        {
            int current = (int)handleOrientation + 1;

            if (current >= System.Enum.GetValues(typeof(HandleOrientation)).Length)
                current = 0;

            handleOrientation = (HandleOrientation)current;

            return new ActionResult(ActionResult.Status.Success, "Set Handle Orientation\n" + s_Tooltips[current].title);
        }

        public override bool enabled
        {
            get { return ProBuilderEditor.instance != null; }
        }
    }
}

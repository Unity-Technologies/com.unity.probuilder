#if PROBUILDER_ENABLE_HANDLE_OVERRIDE

using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ToggleHandlePivotPoint : MenuAction
    {
        Texture2D[] m_Icons;

        PivotPoint pivotPoint
        {
            get { return VertexManipulationTool.pivotPoint; }
            set { VertexManipulationTool.pivotPoint = value; }
        }

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Selection; }
        }

        public override Texture2D icon
        {
            get { return m_Icons[(int)pivotPoint]; }
        }

        public override int toolbarPriority
        {
            get { return 0; }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltips[(int)pivotPoint]; }
        }

        static readonly TooltipContent[] s_Tooltips = new TooltipContent[]
        {
            new TooltipContent("Center", "Transforms are applied from the center point of the selection bounding box."),
            new TooltipContent("Individual Origins", "Transforms are applied from the center of each selection group."),
            new TooltipContent("Active Element", "Transforms are applied from the active selection center.")
        };

        public override string menuTitle
        {
            get { return "Pivot: " + s_Tooltips[(int)pivotPoint]; }
        }

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face; }
        }

        public override bool hidden
        {
            get { return false; }
        }

        // TODO Need icons for PivotPoint
        public ToggleHandlePivotPoint()
        {
            m_Icons = new Texture2D[]
            {
                IconUtility.GetIcon("Toolbar/HandleAlign_World", IconSkin.Pro),
                IconUtility.GetIcon("Toolbar/HandleAlign_Local", IconSkin.Pro),
                IconUtility.GetIcon("Toolbar/HandleAlign_Plane", IconSkin.Pro),
            };
        }

        protected override ActionResult PerformActionImplementation()
        {
            int current = (int)pivotPoint + 1;

            if (current >= System.Enum.GetValues(typeof(PivotPoint)).Length)
                current = 0;

            pivotPoint = (PivotPoint)current;

            return new ActionResult(ActionResult.Status.Success, "Set Pivot Point\n" + s_Tooltips[(int)pivotPoint].title);
        }

        public override bool enabled
        {
            get { return ProBuilderEditor.instance != null; }
        }
    }
}
#endif

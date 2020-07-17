using UnityEngine.ProBuilder;
using UnityEngine;
using System;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class OpenShapeEditor : MenuToggle
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
        public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Panel_Shapes", IconSkin.Pro); } }
        public override TooltipContent tooltip { get { return s_Tooltip; } }
        public override string menuTitle { get { return "New Shape"; } }
        public override int toolbarPriority { get { return 0; } }
        protected override bool hasFileMenuEntry { get { return false; } }

        Type m_lastActiveTool;

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "New Shape Tool",
                "Opens the Shape Editor window.\n\nThe Shape Editor is a window that allows you to interactively create new 3d primitives.",
                keyCommandSuper, keyCommandShift, 'K'
            );

        public override bool enabled {
            get { return true; }
        }

        protected override MenuActionState optionsMenuState {
            get { return MenuActionState.VisibleAndEnabled; }
        }

        protected override void DoAlternateAction()
        {
            DrawShapeTool.CreateLastShape(Vector3.one * 100);
        }

        protected override ActionResult StartActivation(StartEndCallBack callback)
        {
            ProBuilderEditor.selectMode = SelectMode.Object;
            m_lastActiveTool = EditorTools.EditorTools.activeToolType;
            EditorTools.EditorTools.SetActiveTool<DrawShapeTool>();
            return new ActionResult(ActionResult.Status.Success, "Open ahape editor");
        }

        protected override ActionResult EndActivation(StartEndCallBack callback)
        {
            EditorTools.EditorTools.SetActiveTool(m_lastActiveTool);
            return ActionResult.Success;
        }
    }
}

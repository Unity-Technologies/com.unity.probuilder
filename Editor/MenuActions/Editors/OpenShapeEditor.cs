using UnityEngine.ProBuilder;
using UnityEngine;
#if UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.ToolManager;
#else
using ToolManager = UnityEditor.EditorTools.EditorTools;
#endif

namespace UnityEditor.ProBuilder.Actions
{
    sealed class OpenShapeEditor : MenuToolToggle
    {
        bool m_RestorePreviousMode;
        SelectMode m_PreviousMode;

        public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
        public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Panel_Shapes", IconSkin.Pro); } }
        public override TooltipContent tooltip { get { return s_Tooltip; } }
        public override string menuTitle { get { return "New Shape"; } }
        public override int toolbarPriority { get { return 0; } }
        protected override bool hasFileMenuEntry { get { return false; } }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "New Shape Tool",
                "Opens the Shape Editor window.\n\nThe Shape Editor is a window that allows you to interactively create new 3d primitives.",
                keyCommandSuper, keyCommandShift, 'K'
            );

        public override bool enabled {
            get { return true; }
        }

        // protected override MenuActionState optionsMenuState {
        //     get { return MenuActionState.VisibleAndEnabled; }
        // }

        // protected override void DoAlternateAction()
        // {
        //     DrawShapeTool.CreateLastShape();
        //     return new ActionResult(ActionResult.Status.Success, "Create Shape");
        // }

        internal override ActionResult StartActivation()
        {
            m_RestorePreviousMode = true;
            m_PreviousMode = ProBuilderEditor.selectMode;
            ProBuilderEditor.selectMode = SelectMode.Object;

            m_Tool = ScriptableObject.CreateInstance<DrawShapeTool>();
            ToolManager.SetActiveTool(m_Tool);

            Undo.RegisterCreatedObjectUndo(m_Tool, "Open Draw Shape Tool");

            ToolManager.activeToolChanging += LeaveTool;
            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;

            return new ActionResult(ActionResult.Status.Success,"Draw Shape Tool Starts");
        }

        internal override ActionResult EndActivation()
        {
            ToolManager.activeToolChanging -= LeaveTool;
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;

            Object.DestroyImmediate(m_Tool);

            if(m_RestorePreviousMode)
                ProBuilderEditor.selectMode = m_PreviousMode;

            return new ActionResult(ActionResult.Status.Success,"Draw Shape Tool Ends");
        }

        void OnSelectModeChanged(SelectMode obj)
        {
            m_RestorePreviousMode = false;
            LeaveTool();
        }

        void LeaveTool()
        {
            ActionResult result = EndActivation();
            EditorUtility.ShowNotification(result.notification);
        }

    }
}

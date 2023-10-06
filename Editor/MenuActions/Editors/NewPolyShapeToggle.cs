using UnityEngine.ProBuilder;
using UnityEngine;

#if UNITY_2020_2_OR_NEWER
using EditorToolManager = UnityEditor.EditorTools.EditorToolManager;
using ToolManager = UnityEditor.EditorTools.ToolManager;
#else
using EditorToolManager = UnityEditor.EditorTools.EditorToolContext;
using ToolManager = UnityEditor.EditorTools.EditorTools;
#endif

namespace UnityEditor.ProBuilder.Actions
{
    sealed class NewPolyShapeToggle : MenuToolToggle
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Tool; } }
        public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/CreatePolyShape"); } }
        public override TooltipContent tooltip { get { return _tooltip; } }
        public override string menuTitle { get { return "New Poly Shape"; } }
        public override int toolbarPriority { get { return 1; } }

        static readonly TooltipContent _tooltip = new TooltipContent
            (
                "New Polygon Shape",
                "Creates a new shape by clicking around a perimeter and extruding."
            );

        public override bool hidden
        {
            get { return false; }
        }

        public override bool enabled
        {
            get { return ProBuilderEditor.instance != null; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            ToolManager.SetActiveTool(EditorToolManager.GetSingleton<CreatePolyShapeTool>());

            MenuAction.onPerformAction += ActionPerformed;
            ToolManager.activeToolChanged += OnActiveToolChanged;
            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;
            MeshSelection.objectSelectionChanged += OnObjectSelectionChanged;

            return new ActionResult(ActionResult.Status.Success,"Create Poly Shape");
        }

        void Clear()
        {
            //m_Tool = null;
            MenuAction.onPerformAction -= ActionPerformed;
            ToolManager.activeToolChanged -= OnActiveToolChanged;
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
            MeshSelection.objectSelectionChanged -= OnObjectSelectionChanged;

            ProBuilderEditor.Refresh();
        }

        internal override ActionResult EndActivation()
        {
            Clear();
            ToolManager.RestorePreviousPersistentTool();
            return new ActionResult(ActionResult.Status.Success,"End Poly Shape");
        }

        ActionResult QuitTool()
        {
            Clear();
            return new ActionResult(ActionResult.Status.Success,"End Poly Shape");
        }

        void ActionPerformed(MenuAction newActionPerformed)
        {
            if(ToolManager.IsActiveTool(m_Tool) && newActionPerformed.GetType() != this.GetType())
                LeaveTool();
        }

        void OnObjectSelectionChanged()
        {
            if( m_Tool == null )
                return;

            if(MeshSelection.activeMesh == null || MeshSelection.activeMesh.GetComponent<PolyShape>() == null)
                EditorApplication.delayCall += () => LeaveTool();
        }

        void OnSelectModeChanged(SelectMode obj)
        {
            LeaveTool();
        }

        void OnActiveToolChanged()
        {
            if(m_Tool != null && ToolManager.activeToolType != m_Tool.GetType())
                 LeaveTool();
        }

        void LeaveTool()
        {
            ActionResult result = QuitTool();
            EditorUtility.ShowNotification(result.notification);
        }
    }
}

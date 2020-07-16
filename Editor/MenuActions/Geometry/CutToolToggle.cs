using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class CutToolToggle : MenuToggle
    {
        SelectMode m_PreviousMode;
        Tool m_PreviousTool;

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Face_Subdivide", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face | SelectMode.Object; }
        }

        protected override bool hasFileMenuEntry
        {
            get { return false; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
        (
            "Cut Tool",
            @"Inserts vertices in a face and subdivide it accordingly.",
            keyCommandAlt, keyCommandShift, 'V'
        );

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedObjectCount > 0; }
        }

        public override ActionResult StartActivation()
        {
            m_PreviousMode = ProBuilderEditor.selectMode;
            ProBuilderEditor.selectMode = SelectMode.Object;

            m_PreviousTool = Tools.current;
            EditorTools.EditorTools.SetActiveTool<CutTool>();

            EditorTools.EditorTools.activeToolChanged += ActiveToolChanged;
            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;

            return new ActionResult(ActionResult.Status.Success,"Cut Tool Starts");
        }

        public override void UpdateAction(){}

        public override ActionResult EndActivation()
        {
            EditorTools.EditorTools.activeToolChanged -= ActiveToolChanged;
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;

            ProBuilderEditor.selectMode = m_PreviousMode;
            Tools.current = m_PreviousTool;

            return new ActionResult(ActionResult.Status.Success,"Cut Tool Ends");
        }

        void OnSelectModeChanged(SelectMode obj)
        {
            EndActivation();
        }

        void ActiveToolChanged()
        {
            EndActivation();
        }
    }
}

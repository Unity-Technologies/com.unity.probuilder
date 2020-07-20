using UnityEngine;
using UnityEngine.ProBuilder;


#if !UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.EditorTools;
#endif

namespace UnityEditor.ProBuilder.Actions
{
    public class CutToolToggle : MenuToggle
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

        protected override ActionResult StartActivation(StartEndCallBack onStart)
        {
            m_PreviousMode = ProBuilderEditor.selectMode;
            ProBuilderEditor.selectMode = SelectMode.Object;

            m_PreviousTool = Tools.current;
            ToolManager.SetActiveTool<CutTool>();

            ToolManager.activeToolChanged += ActiveToolChanged;
            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;

            onStart();
            return new ActionResult(ActionResult.Status.Success,"Cut Tool Starts");
        }

        protected override ActionResult EndActivation(StartEndCallBack onEnd)
        {
            ToolManager.activeToolChanged -= ActiveToolChanged;
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;

            ProBuilderEditor.selectMode = m_PreviousMode;
            Tools.current = m_PreviousTool;

            onEnd();
            return new ActionResult(ActionResult.Status.Success,"Cut Tool Ends");
        }

        void OnSelectModeChanged(SelectMode obj)
        {
            LeaveTool();
        }

        void ActiveToolChanged()
        {
            EditorApplication.delayCall += () => LeaveTool();
        }

        void LeaveTool()
        {
            ActionResult result = EndActivation(OnEnd);
            EditorUtility.ShowNotification(result.notification);
        }
    }
}

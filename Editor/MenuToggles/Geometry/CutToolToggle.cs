using System;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.ProBuilder;
using Object = UnityEngine.Object;
#if !UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.EditorTools;
#else
using ToolManager = UnityEditor.EditorTools.ToolManager;
#endif

namespace UnityEditor.ProBuilder.Actions
{
    public class CutToolToggle : MenuToolToggle
    {
        bool m_RestorePreviousMode;
        SelectMode m_PreviousMode;

        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/CutTool", IconSkin.Pro); }
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
            @"Create a cut in a face to subdivide it.",
            keyCommandAlt, keyCommandShift, 'C'
        );

        public override bool enabled
        {
            get
            {
                return MeshSelection.selectedObjectCount > 0
                       && ( Tools.current != Tool.Custom || ToolManager.activeToolType == typeof(CutTool) );
            }
        }

        internal override ActionResult StartActivation()
        {
            m_RestorePreviousMode = true;
            m_PreviousMode = ProBuilderEditor.selectMode;
            ProBuilderEditor.selectMode = SelectMode.Object;

            m_Tool = ScriptableObject.CreateInstance<CutTool>();
            ToolManager.SetActiveTool(m_Tool);

            Undo.RegisterCreatedObjectUndo(m_Tool, "Open Cut Tool");

            ToolManager.activeToolChanging += LeaveTool;
            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;

            return new ActionResult(ActionResult.Status.Success,"Cut Tool Starts");
        }

        internal override ActionResult EndActivation()
        {
            ToolManager.activeToolChanging -= LeaveTool;
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;

            Object.DestroyImmediate(m_Tool);

            if(m_RestorePreviousMode)
                ProBuilderEditor.selectMode = m_PreviousMode;

            return new ActionResult(ActionResult.Status.Success,"Cut Tool Ends");
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

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
            get => MeshSelection.selectedObjectCount > 0;
        }

        internal override ActionResult StartActivation()
        {
            ProBuilderEditor.selectMode = SelectMode.Object;

            m_Tool = ScriptableObject.CreateInstance<CutTool>();
            ToolManager.SetActiveTool(m_Tool);

            Undo.RegisterCreatedObjectUndo(m_Tool, "Open Cut Tool");

            ToolManager.activeToolChanging += LeaveTool;
            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;

            //Give the focus back to scene view to handle key inputs directly
            SceneView.lastActiveSceneView.Focus();

            return new ActionResult(ActionResult.Status.Success,"Cut Tool Starts");
        }

        internal override ActionResult EndActivation()
        {
            ToolManager.activeToolChanging -= LeaveTool;
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;

            Object.DestroyImmediate(m_Tool);

            return new ActionResult(ActionResult.Status.Success,"Cut Tool Ends");
        }

        void OnSelectModeChanged(SelectMode obj)
        {
            LeaveTool();
        }

        void LeaveTool()
        {
            ActionResult result = EndActivation();
            EditorUtility.ShowNotification(result.notification);
        }
    }
}

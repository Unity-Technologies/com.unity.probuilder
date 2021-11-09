#if UNITY_2021_2_OR_NEWER
#define OVERLAYS_AVAILABLE
#endif
using System;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{

#if OVERLAYS_AVAILABLE
    abstract class PositionToolContext : EditorToolContext
    {
        SelectionGUI m_SelectionGUI;
        // Does this really need to be static?
        internal static ProBuilderToolManager toolManager;

        internal event Action objectSelectionChanged;

        public override void OnActivated()
        {
            toolManager = new ProBuilderToolManager();
            m_SelectionGUI = new SelectionGUI(this);
            SceneView.duringSceneGui += OnSceneGUI;
            MeshSelection.objectSelectionChanged += OnObjectSelectionChanged;
            SetOverrideWireframe(true);

            EditorApplication.delayCall += () => m_SelectionGUI.UpdateSelection();
        }

        public override void OnWillBeDeactivated()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
            MeshSelection.objectSelectionChanged -= OnObjectSelectionChanged;
            SetOverrideWireframe(false);
        }

        public override void OnToolGUI(EditorWindow window)
        {
        }

        void OnSceneGUI(SceneView sceneView)
        {
            m_SelectionGUI.OnSceneGUI(sceneView);
        }

        void OnObjectSelectionChanged()
        {
            objectSelectionChanged?.Invoke();
            SetOverrideWireframe(true);
        }

        protected override Type GetEditorToolType(Tool tool)
        {
            switch(tool)
            {
                case Tool.Move:
                    return typeof(ProbuilderMoveTool);
                case Tool.Rotate:
                    return typeof(ProbuilderRotateTool);
                case Tool.Scale:
                    return typeof(ProbuilderScaleTool);
                default:
                    return null;
            }
        }

        // REMOVE: SelectionGUI possibly as this controls handle rendering
        /// <summary>
        /// Hide the default unity wireframe renderer
        /// </summary>
        void SetOverrideWireframe(bool overrideWireframe)
        {
            const EditorSelectedRenderState k_DefaultSelectedRenderState = EditorSelectedRenderState.Highlight | EditorSelectedRenderState.Wireframe;

            foreach (var mesh in Selection.transforms.GetComponents<ProBuilderMesh>())
            {
                // Disable Wireframe for meshes when ProBuilder is active
                EditorUtility.SetSelectionRenderState(
                    mesh.renderer,
                    overrideWireframe
                        ? k_DefaultSelectedRenderState & ~(EditorSelectedRenderState.Wireframe)
                        : k_DefaultSelectedRenderState);
            }

            SceneView.RepaintAll();
        }
    }

    class TextureToolContext : EditorToolContext
    {
        TextureToolContext() { }

        protected override Type GetEditorToolType(Tool tool)
        {
            switch(tool)
            {
                case Tool.Move:
                    return typeof(TextureMoveTool);
                case Tool.Rotate:
                    return typeof(TextureRotateTool);
                case Tool.Scale:
                    return typeof(TextureScaleTool);
                default:
                    return null;
            }
        }
    }

    [EditorToolContext("Vertex", typeof(ProBuilderMesh)), Icon(k_IconPath)]
    class VertexToolContext : PositionToolContext
    {
        const string k_IconPath = "Packages/com.unity.probuilder/Content/Icons/Modes/Mode_Vertex.png";

        public override void OnActivated()
        {
            base.OnActivated();
            toolManager.SetSelectMode(SelectMode.Vertex);
        }
    }

    [EditorToolContext("Edge", typeof(ProBuilderMesh)), Icon(k_IconPath)]
    class EdgeToolContext : PositionToolContext
    {
        const string k_IconPath = "Packages/com.unity.probuilder/Content/Icons/Modes/Mode_Edge.png";

        public override void OnActivated()
        {
            base.OnActivated();
            toolManager.SetSelectMode(SelectMode.Edge);
        }
    }

    [EditorToolContext("Face", typeof(ProBuilderMesh)), Icon(k_IconPath)]
    class FaceToolContext : PositionToolContext
    {
        const string k_IconPath = "Packages/com.unity.probuilder/Content/Icons/Modes/Mode_Face.png";

        public override void OnActivated()
        {
            base.OnActivated();
            toolManager.SetSelectMode(SelectMode.Face);
        }
    }
#else
    class PositionToolContext : EditorToolContext
    {
        protected override Type GetEditorToolType(Tool tool)
        {
            switch(tool)
            {
                case Tool.Move:
                    return typeof(ProbuilderMoveTool);
                case Tool.Rotate:
                    return typeof(ProbuilderRotateTool);
                case Tool.Scale:
                    return typeof(ProbuilderScaleTool);
                default:
                    return null;
            }
        }
    }
    class TextureToolContext : EditorToolContext
    {
        TextureToolContext() { }

        protected override Type GetEditorToolType(Tool tool)
        {
            switch(tool)
            {
                case Tool.Move:
                    return typeof(TextureMoveTool);
                case Tool.Rotate:
                    return typeof(TextureRotateTool);
                case Tool.Scale:
                    return typeof(TextureScaleTool);
                default:
                    return null;
            }
        }
    }
#endif
}

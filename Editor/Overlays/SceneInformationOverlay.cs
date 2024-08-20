using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.ProBuilder.Overlays
{
    //[Overlay(typeof(SceneView), "Scene Information", true)]
    class SceneInformationOverlay : Overlay
    {
        Label m_FaceCountLabel = new Label();
        Label m_TriCountLabel = new Label();
        Label m_VertCountLabel = new Label();
        Label m_SelectedFaceCountLabel = new Label();
        Label m_SelectedEdgeCountLabel = new Label();
        Label m_SelectedVertCountLabel = new Label();

        public SceneInformationOverlay()
        {
            ProBuilderEditor.selectionUpdated += _ => UpdateSceneInfo();
            MeshSelection.objectSelectionChanged += UpdateSceneInfo;
        }

        public override VisualElement CreatePanelContent()
        {
            displayName = "ProBuilder Information";
            var root = new VisualElement();
            root.style.flexDirection = FlexDirection.Column;

            root.Add(CreateLabelLine(m_FaceCountLabel, "Faces:"));
            root.Add(CreateLabelLine(m_TriCountLabel, "Triangles:"));
            root.Add(CreateLabelLine(m_VertCountLabel, "Vertices:"));
            var spacer = new VisualElement();
            spacer.style.height = 10;
            root.Add(spacer); // Spacer line
            root.Add(CreateLabelLine(m_SelectedFaceCountLabel, "Selected Faces:"));
            root.Add(CreateLabelLine(m_SelectedEdgeCountLabel, "Selected Edges:"));
            root.Add(CreateLabelLine(m_SelectedVertCountLabel, "Selected Vertices:"));

            UpdateSceneInfo();

            return root;
        }

        void UpdateSceneInfo()
        {
            m_FaceCountLabel.text = MeshSelection.totalFaceCount.ToString();
            m_TriCountLabel.text = MeshSelection.totalTriangleCountCompiled.ToString();
            m_VertCountLabel.text = MeshSelection.totalCommonVertexCount + " (" + MeshSelection.totalVertexCountOptimized + ")";
            m_SelectedFaceCountLabel.text = MeshSelection.selectedFaceCount.ToString();
            m_SelectedEdgeCountLabel.text = MeshSelection.selectedEdgeCount.ToString();
            m_SelectedVertCountLabel.text = MeshSelection.selectedSharedVertexCount + " (" + MeshSelection.selectedVertexCount+ ")";
        }

        VisualElement CreateLabelLine(Label targetLabel, string displayName)
        {
            var line = new VisualElement();
            line.style.flexDirection = FlexDirection.Row;
            line.Add(new Label(displayName));
            targetLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            line.Add(targetLabel);
            return line;
        }
    }
}

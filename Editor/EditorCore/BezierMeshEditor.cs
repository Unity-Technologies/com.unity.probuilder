using UnityEditor.EditorTools;
using UnityEditor.Splines;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    [CustomEditor(typeof(BezierMesh))]
    [CanEditMultipleObjects]
    sealed class BezierMeshEditor : Editor
    {
        private BezierMeshTransientOverlay m_overlay;

        void OnEnable()
        {
            SceneView.AddOverlayToActiveView(m_overlay = new BezierMeshTransientOverlay());
            BezierMesh.BezierMeshModified += RefreshEditor;
        }

        void OnDisable()
        {
            SceneView.RemoveOverlayFromActiveView(m_overlay);
            BezierMesh.BezierMeshModified -= RefreshEditor;
        }

        void RefreshEditor()
        {
            ProBuilderEditor.Refresh();
        }

        /// <summary>
        /// Sets the current active context to <see cref="SplineToolContext"/> and the current active tool
        /// to the Draw Splines Tool (<see cref="KnotPlacementTool"/>).
        /// </summary>
        public static void SetSplineToolContext()
        {
            ToolManager.SetActiveContext<SplineToolContext>();
            ToolManager.SetActiveTool<KnotPlacementTool>();
        }
    }
}

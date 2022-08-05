#if USING_SPLINES && UNITY_2021_3_OR_NEWER

using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEditor.Splines;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    [CustomEditor(typeof(BezierMesh))]
    [CanEditMultipleObjects]
    class BezierMeshEditor : Editor
    {
        BezierMeshOverlay m_Overlay;
        List<BezierMesh> m_SelectedMeshes;
        bool m_isOverlayVisible;

        void BuildSelection()
        {
            m_SelectedMeshes = new List<BezierMesh>();

            foreach (var obj in Selection.gameObjects)
            {
                if (obj.TryGetComponent(out BezierMesh mesh))
                {
                    m_SelectedMeshes.Add(mesh);
                }
            }

            m_isOverlayVisible = m_SelectedMeshes.Count > 0;
        }

        void OnEnable()
        {
            BuildSelection();
#if UNITY_2022_1_OR_NEWER
            SceneView.AddOverlayToActiveView(m_Overlay = new BezierMeshOverlay(m_SelectedMeshes, m_isOverlayVisible));
#endif
            BezierMesh.BezierMeshModified += () => ProBuilderEditor.Refresh();
        }

        void OnDisable()
        {
#if UNITY_2022_1_OR_NEWER
            SceneView.RemoveOverlayFromActiveView(m_Overlay);
#endif
            BezierMesh.BezierMeshModified -= () => ProBuilderEditor.Refresh();
        }

        /// <summary>
        /// Sets the current active context to <see cref="SplineToolContext"/> and the current active tool
        /// to the Draw Splines Tool (<see cref="KnotPlacementTool"/>).
        /// </summary>
        public static void SetSplineToolContext()
        {
            EditorSplineUtility.SetKnotPlacementTool();
        }
    }
}
#endif

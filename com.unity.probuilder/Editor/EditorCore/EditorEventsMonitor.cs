using UnityEngine;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Monitor Editor's related events.
    /// </summary>
    internal sealed class EditorEventsMonitor
    {
        public static event System.Action editorPixelPerPointsChanged;

        private struct EditorGUIData
        {
            public float pixelPerPoint;
        }

        EditorGUIData m_GUIData;

        public void StartMonitor()
        {
            LoadInternalData();
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        public void StopMonitor()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
        }

        void UpdateMonitor()
        {
            UpdateGUIData();
        }

        void UpdateGUIData()
        {
            if (!Mathf.Approximately(EditorGUIUtility.pixelsPerPoint, m_GUIData.pixelPerPoint))
            {
                editorPixelPerPointsChanged?.Invoke();
                m_GUIData.pixelPerPoint = EditorGUIUtility.pixelsPerPoint;
            }
        }

        void OnSceneGUI(SceneView sceneView)
        {
            UpdateMonitor();
        }

        void LoadInternalData()
        {
            m_GUIData = new EditorGUIData();
            m_GUIData.pixelPerPoint = EditorGUIUtility.pixelsPerPoint;
        }
    }
}
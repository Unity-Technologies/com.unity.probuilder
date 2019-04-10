using UnityEngine;

namespace UnityEditor.ProBuilder
{
    [InitializeOnLoad]
    static class VertexManipulationToolSettings
    {
        const bool k_EnableHandleSettingInput = true;

        static VertexManipulationToolSettings()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
        }

        static void OnSceneGUI(SceneView view)
        {
            if (!EditorUtility.IsDeveloperMode()
                || !VertexManipulationTool.showHandleSettingsInScene
                || view != SceneView.lastActiveSceneView)
                return;

            DoHandleSettings(new Rect(
                    8,
                    view.position.height - 40,
                    400f,
                    48f));
        }

        static void DoHandleSettings(Rect rect)
        {
            Handles.BeginGUI();
            using (new EditorGUI.DisabledScope(k_EnableHandleSettingInput))
            {
                GUILayout.BeginArea(rect);
                EditorGUI.BeginChangeCheck();

                GUILayout.BeginHorizontal();
                EditorGUIUtility.labelWidth = 80;

                EditorGUILayout.EnumPopup("Pivot Point", VertexManipulationTool.pivotPoint);
                EditorGUILayout.EnumPopup("Orientation", VertexManipulationTool.handleOrientation);

                EditorGUIUtility.labelWidth = 0;
                GUILayout.EndHorizontal();

                if (EditorGUI.EndChangeCheck())
                    ProBuilderEditor.Refresh();
                GUILayout.EndArea();
            }
            Handles.EndGUI();
        }
    }
}

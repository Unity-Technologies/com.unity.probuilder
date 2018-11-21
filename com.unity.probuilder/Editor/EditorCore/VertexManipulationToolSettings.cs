using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    [InitializeOnLoad]
    static class VertexManipulationToolSettings
    {
#if PROBUILDER_ENABLE_HANDLE_OVERRIDE
        const bool k_EnableHandleSettingInput = false;
#else
        const bool k_EnableHandleSettingInput = true;
#endif

        static VertexManipulationToolSettings()
        {
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        static void OnSceneGUI(SceneView view)
        {
            if (view != SceneView.lastActiveSceneView)
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
#if PROBUILDER_ENABLE_HANDLE_OVERRIDE
                VertexManipulationTool.pivotPoint =
                    (PivotPoint)EditorGUILayout.EnumPopup("Pivot Point", VertexManipulationTool.pivotPoint);
                VertexManipulationTool.handleOrientation =
                    (HandleOrientation)EditorGUILayout.EnumPopup("Orientation",
                        VertexManipulationTool.handleOrientation);
#else
                EditorGUILayout.EnumPopup("Pivot Point", VertexManipulationTool.pivotPoint);
                EditorGUILayout.EnumPopup("Orientation", VertexManipulationTool.handleOrientation);
#endif

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

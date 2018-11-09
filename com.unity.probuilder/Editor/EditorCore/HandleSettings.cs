using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	[InitializeOnLoad]
	static class HandleSettings
	{
		static HandleSettings()
		{
			ToolbarSettings.AddToolSettingsCallback(OnToolbarSettings);
		}

		static void OnToolbarSettings(Rect rect)
		{
			rect.width = 10000;

			GUILayout.BeginArea(rect);

			EditorGUI.BeginChangeCheck();

			GUILayout.BeginHorizontal(GUILayout.MaxWidth(400));
			EditorGUIUtility.labelWidth = 80;
			VertexManipulationTool.pivotPoint = (PivotPoint) EditorGUILayout.EnumPopup("Pivot Point", VertexManipulationTool.pivotPoint);
			VertexManipulationTool.handleOrientation = (HandleOrientation) EditorGUILayout.EnumPopup("Orientation", VertexManipulationTool.handleOrientation);
			EditorGUIUtility.labelWidth = 0;
			GUILayout.EndHorizontal();

			GUILayout.EndArea();

			if(EditorGUI.EndChangeCheck())
				ProBuilderEditor.Refresh();
		}
	}
}

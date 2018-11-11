#if UNITY_2019_1_OR_NEWER && PB_OVERRIDE_TOOL_SETTINGS

using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	[InitializeOnLoad]
	static class VertexManipulationToolSettings
	{
		static bool s_OverrideToolbarSettings;

		static VertexManipulationToolSettings()
		{
			ProBuilderEditor.selectModeChanged += selectMode => { UpdateToolbar(); };
		}

		static void UpdateToolbar()
		{
			var selectMode = ProBuilderEditor.instance == null ? SelectMode.Object : ProBuilderEditor.selectMode;

			if (selectMode == SelectMode.None || selectMode == SelectMode.Object)
			{
				if (s_OverrideToolbarSettings)
					Toolbar.toolSettingsGui -= OnToolbarSettings;
				s_OverrideToolbarSettings = false;
			}
			else if(!s_OverrideToolbarSettings)
			{
				s_OverrideToolbarSettings = true;
				Toolbar.toolSettingsGui += OnToolbarSettings;
			}

			Toolbar.RepaintToolbar();
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

#endif

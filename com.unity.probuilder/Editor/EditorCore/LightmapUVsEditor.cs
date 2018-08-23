using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	sealed class LightmapUVEditor : EditorWindow
	{
		static bool generateUV2PerObject
		{
			get { return PreferencesInternal.GetBool("pbGenerateUV2PerObject", false); }
			set { PreferencesInternal.SetBool("pbGenerateUV2PerObject", value); }
		}

		static bool disableAutoUV2Generation
		{
			get { return PreferencesInternal.GetBool(PreferenceKeys.pbDisableAutoUV2Generation); }
			set { PreferencesInternal.SetBool(PreferenceKeys.pbDisableAutoUV2Generation, value); }
		}

		static class Styles
		{
			public static readonly GUIContent hardAngle = new GUIContent("Hard Angle", "Angle between neighbor triangles that will generate seam.");
			public static readonly GUIContent packMargin = new GUIContent("Pack Margin", "Measured in pixels, assuming mesh will cover an entire 1024x1024 lightmap.");
			public static readonly GUIContent angleError = new GUIContent("Angle Error", "Measured in percents. Angle error measures deviation of UV angles from geometry angles.");
			public static readonly GUIContent areaError = new GUIContent("Area Error", "");

			static bool s_Initialized;
			public static GUIStyle miniButton;

			public static void Init()
			{
				if (s_Initialized)
					return;

				s_Initialized = true;

				miniButton = new GUIStyle(GUI.skin.button);
				miniButton.stretchHeight = false;
				miniButton.stretchWidth = false;
				miniButton.padding = new RectOffset(6, 6, 3, 3);
			}
		}

		UnwrapParameters m_UnwrapParameters;

		void OnEnable()
		{
			m_UnwrapParameters = PreferencesInternal.GetValue<UnwrapParameters>(PreferenceKeys.defaultUnwrapParameters, new UnwrapParameters());
		}

		void OnGUI()
		{
			Styles.Init();

			GUILayout.Label("Default Lightmap UV Parameters", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();

			m_UnwrapParameters.hardAngle = EditorGUILayout.Slider(Styles.hardAngle, m_UnwrapParameters.hardAngle, 1f, 180f);
			m_UnwrapParameters.packMargin = EditorGUILayout.Slider(Styles.packMargin, m_UnwrapParameters.packMargin, 1f, 64f);
			m_UnwrapParameters.angleError = EditorGUILayout.Slider(Styles.angleError, m_UnwrapParameters.angleError, 1f, 75f);
			m_UnwrapParameters.areaError = EditorGUILayout.Slider(Styles.areaError, m_UnwrapParameters.areaError, 1f, 75f);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("Reset", Styles.miniButton))
				m_UnwrapParameters.Reset();
			GUILayout.EndHorizontal();

			if (EditorGUI.EndChangeCheck())
			{
				PreferencesInternal.SetValue(PreferenceKeys.defaultUnwrapParameters, m_UnwrapParameters);
			}

//			GUILayout.Label("Generate UV2 Options", EditorStyles.boldLabel);
//
//			EditorGUILayout.HelpBox("Generate Scene UV2s will rebuild all ProBuilder mesh UV2s when invoked, instead of just the selection.", MessageType.Info);
//			bool perSceneUV2s = !generateUV2PerObject;
//			perSceneUV2s = EditorGUILayout.Toggle("Generate Scene UV2s", perSceneUV2s);
//			generateUV2PerObject = !perSceneUV2s;
//
//			EditorGUI.BeginChangeCheck();
//			bool enableAutoUV2 = !disableAutoUV2Generation;
//			enableAutoUV2 = EditorGUILayout.Toggle("Enable Auto UV2", enableAutoUV2);
//			if (EditorGUI.EndChangeCheck())
//				disableAutoUV2Generation = !enableAutoUV2;
//
//			EditorUtility.CreateCachedEditor<UnwrapParametersEditor>(MeshSelection.TopInternal(), ref m_UnwrapParametersEditor);
//
//			if (m_UnwrapParametersEditor != null)
//			{
//				GUILayout.Space(4);
//				m_UnwrapParametersEditor.OnInspectorGUI();
//			}
//
//			GUILayout.FlexibleSpace();
//
//			if (GUILayout.Button(generateUV2PerObject ? "Rebuild Selected UV2s" : "Rebuild Scene UV2s"))
//				EditorUtility.ShowNotification(DoAction().notification);
		}
	}
}

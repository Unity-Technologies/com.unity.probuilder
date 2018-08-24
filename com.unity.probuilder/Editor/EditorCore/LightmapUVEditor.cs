using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	sealed class LightmapUVEditor : EditorWindow
	{
		static class Styles
		{
			public static readonly GUIContent hardAngle = new GUIContent("Hard Angle", "Angle between neighbor triangles that will generate seam.");
			public static readonly GUIContent packMargin = new GUIContent("Pack Margin", "Measured in pixels, assuming mesh will cover an entire 1024x1024 lightmap.");
			public static readonly GUIContent angleError = new GUIContent("Angle Error", "Measured in percents. Angle error measures deviation of UV angles from geometry angles.");
			public static readonly GUIContent areaError = new GUIContent("Area Error", "");
			public static readonly GUIContent autoLightmapUV = new GUIContent("Auto Lightmap UVs", "Automatically build the lightmap UV array when editing ProBuilder meshes. If this feature is disabled, you will need to use the 'Generate UV2' action to build lightmap UVs for meshes prior to baking lightmaps.");

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

		static bool disableAutoUV2Generation
		{
			get { return PreferencesInternal.GetBool(PreferenceKeys.pbDisableAutoUV2Generation); }
			set { PreferencesInternal.SetBool(PreferenceKeys.pbDisableAutoUV2Generation, value); }
		}

		List<ProBuilderMesh> m_MissingLightmaps = new List<ProBuilderMesh>();
		UnwrapParameters m_UnwrapParameters;

		internal static readonly Rect desiredPosition = new Rect(100, 100, 348, 234);

		void OnEnable()
		{
			m_UnwrapParameters = PreferencesInternal.GetValue<UnwrapParameters>(PreferenceKeys.defaultUnwrapParameters, new UnwrapParameters());
			m_MissingLightmaps = FindMissingLightmaps();
			EditorMeshUtility.meshOptimized += MeshOptimized;
		}

		void OnGUI()
		{
			Styles.Init();

			GUILayout.Label("Lightmap UV Settings", EditorStyles.boldLabel);

			var autoLightmap = !disableAutoUV2Generation;
			autoLightmap = EditorGUILayout.Toggle(Styles.autoLightmapUV, autoLightmap);
			if (autoLightmap == disableAutoUV2Generation)
				disableAutoUV2Generation = !autoLightmap;

			GUILayout.Label("Default Lightmap UVs Settings", EditorStyles.boldLabel);

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

			if (m_MissingLightmaps.Count > 0)
			{
				EditorGUILayout.HelpBox(GetMissingLightmapText(), MessageType.Warning);

				GUILayout.BeginHorizontal();

				GUILayout.FlexibleSpace();

				if (GUILayout.Button("Build Missing Lightmap UVs"))
				{
					// copy the missing lightmaps array so that MeshOptimized does not interfere with the rebuild iterator
					Lightmapping.RebuildMissingLightmapUVs(m_MissingLightmaps.ToArray());
					EditorUtility.ShowNotification("Rebuild Missing Lightmap UVs");
				}

				GUILayout.EndHorizontal();
			}
		}

		void MeshOptimized(ProBuilderMesh mesh, Mesh umesh)
		{
			var missing = IsMissingLightmaps(mesh);

			if (missing)
			{
				if (!m_MissingLightmaps.Contains(mesh))
					m_MissingLightmaps.Add(mesh);
			}
			else
			{
				if (m_MissingLightmaps.Contains(mesh))
					m_MissingLightmaps.Remove(mesh);
			}

			Repaint();
		}

		string GetMissingLightmapText()
		{
			var count = m_MissingLightmaps.Count;

			if (count < 2)
				return "There is 1 mesh missing Lightmap UVs in the open scenes.";

			return "There are " + m_MissingLightmaps.Count + " meshes missing Lightmap UVs in the open scenes.";
		}

		static bool IsMissingLightmaps(ProBuilderMesh mesh)
		{
			return mesh.gameObject.HasStaticFlag(StaticEditorFlags.LightmapStatic) && !mesh.HasArrays(MeshArrays.Lightmap);
		}

		static List<ProBuilderMesh> FindMissingLightmaps()
		{
			return FindObjectsOfType<ProBuilderMesh>().Where(IsMissingLightmaps).ToList();
		}
	}
}

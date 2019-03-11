using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder.UI;
using UnityEditor;
using UnityEngine;
using UnityEditor.SettingsManagement;

namespace UnityEditor.ProBuilder
{
    /// <inheritdoc />
    /// <summary>
    /// Popup window in UV editor with the "Render UV Template" options.
    /// </summary>
    sealed class UVRenderOptions : EditorWindow
    {
        Pref<ImageSize> m_ImageSize = new Pref<ImageSize>("UVRenderOptions.imageSize", ImageSize._1024, SettingsScope.User);
        Pref<Color> m_LineColor = new Pref<Color>("UVRenderOptions.lineColor", Color.green, SettingsScope.User);
        Pref<Color> m_BackgroundColor = new Pref<Color>("UVRenderOptions.backgroundColor", Color.black, SettingsScope.User);
        Pref<bool> m_TransparentBackground = new Pref<bool>("UVRenderOptions.transparentBackground", false, SettingsScope.User);
        Pref<bool> m_HideGrid = new Pref<bool>("UVRenderOptions.hideGrid", true, SettingsScope.User);

        enum ImageSize
        {
            _256 = 256,
            _512 = 512,
            _1024 = 1024,
            _2048 = 2048,
            _4096 = 4096,
        };

        public delegate void ScreenshotFunc(int ImageSize, bool HideGrid, Color LineColor, bool TransparentBackground, Color BackgroundColor);
        public ScreenshotFunc screenFunc;

        void OnGUI()
        {
            GUILayout.Label("Render UVs", EditorStyles.boldLabel);

            UI.EditorGUIUtility.DrawSeparator(2, PreferenceKeys.proBuilderDarkGray);
            GUILayout.Space(2);

            m_ImageSize.value = (ImageSize)EditorGUILayout.EnumPopup(new GUIContent("Image Size", "The pixel size of the image to be rendered."), m_ImageSize);

            m_HideGrid.value = EditorGUILayout.Toggle(new GUIContent("Hide Grid", "Hide or show the grid lines."), m_HideGrid);

            m_LineColor.value = EditorGUILayout.ColorField(new GUIContent("Line Color", "The color of the template lines."), m_LineColor);

            m_TransparentBackground.value = EditorGUILayout.Toggle(new GUIContent("Transparent Background", "If true, only the template lines will be rendered, leaving the background fully transparent."), m_TransparentBackground);

            GUI.enabled = !m_TransparentBackground;
            m_BackgroundColor.value = EditorGUILayout.ColorField(new GUIContent("Background Color", "If `TransparentBackground` is off, this will be the fill color of the image."), m_BackgroundColor);
            GUI.enabled = true;

            if (GUILayout.Button("Save UV Template"))
            {
                if (ProBuilderEditor.instance == null || MeshSelection.selectedObjectCount < 1)
                {
                    Debug.LogWarning("Abandoning UV render because no ProBuilder objects are selected.");
                    Close();
                    return;
                }

                screenFunc((int)m_ImageSize.value, m_HideGrid, m_LineColor, m_TransparentBackground, m_BackgroundColor);
                this.Close();
            }
        }
    }
}

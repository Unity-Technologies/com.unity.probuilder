using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using ColorUtility = UnityEngine.ProBuilder.ColorUtility;

namespace UnityEditor.ProBuilder
{
    sealed class VertexColorPalette : ConfigurableWindow
    {
        // Older versions of probuilder stored a fixed size array of colors in EditorPrefs.
        const int k_EditorPrefsColorPaletteCount = 10;
        const string pbVertexColorPrefs = "pbVertexColorPrefs";

        static Pref<string> m_PreviousColorPalette = new Pref<string>("VertexColorPalette.previousColorPalette", "");

        static VertexColorPalette s_Instance = null;

        [SerializeField]
        ColorPalette m_ColorPalette = null;

        ColorPalette colorPalette
        {
            get { return m_ColorPalette; }
        }

        /// <summary>
        /// Older versions of probuilder stored a fixed size array of colors in EditorPrefs. Use this function to get a
        /// pb_ColorPalette from the older version.
        /// </summary>
        /// <returns>
        /// </returns>
        static void CopyColorsFromEditorPrefs(ColorPalette target)
        {
            List<Color> colors = new List<Color>();

            for (int i = 0; i < k_EditorPrefsColorPaletteCount; i++)
            {
                Color color = Color.white;

                if (InternalUtility.TryParseColor(EditorPrefs.GetString(pbVertexColorPrefs + i), ref color))
                    colors.Add(color);
            }

            if (colors.Count > 0)
            {
                target.SetColors(colors);
                UnityEditor.EditorUtility.SetDirty(target);
            }
        }

        /// <summary>
        /// Initialize this window.
        /// </summary>
        public static void MenuOpenWindow()
        {
            GetWindow<VertexColorPalette>("Vertex Colors");
        }

        static ColorPalette GetLastUsedColorPalette()
        {
            // serialized copy?
            ColorPalette palette = s_Instance != null ? s_Instance.m_ColorPalette : null;

            if (palette != null)
                return palette;

            // last set asset path?
            palette = AssetDatabase.LoadAssetAtPath<ColorPalette>(m_PreviousColorPalette);

            if (palette != null)
                return palette;

            // any existing palette in project?
            palette = FileUtility.FindAssetOfType<ColorPalette>();

            if (palette != null)
            {
                m_PreviousColorPalette.SetValue(AssetDatabase.GetAssetPath(palette), true);
                return palette;
            }

            // create new default
            m_PreviousColorPalette.SetValue(FileUtility.GetLocalDataDirectory() + "Default Color Palette.asset", true);
            palette = FileUtility.LoadRequired<ColorPalette>(m_PreviousColorPalette);
            CopyColorsFromEditorPrefs(palette);

            return palette;
        }

        void OnEnable()
        {
            s_Instance = this;
            m_ColorPalette = GetLastUsedColorPalette();
        }

        Vector2 m_Scroll = Vector2.zero;
        const int k_Padding = 4;
        const int k_ButtonWidth = 58;
        GUIContent m_ColorPaletteGuiContent = new GUIContent("Color Palette");

        void OnGUI()
        {
            var palette = GetLastUsedColorPalette();

            DoContextMenu();

            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Reset", EditorStyles.toolbarButton))
                ResetColors();

            GUILayout.EndHorizontal();

            m_ColorPalette = (ColorPalette)EditorGUILayout.ObjectField(m_ColorPaletteGuiContent, m_ColorPalette, typeof(ColorPalette), false);

            if (m_ColorPalette == null)
            {
                GUILayout.Label("Please Select a Color Palette", EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                return;
            }

            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);

            for (int i = 0; i < palette.Count; i++)
            {
                GUILayout.Space(4);

                GUILayout.BeginHorizontal();

                if (GUILayout.Button("Apply", GUILayout.ExpandWidth(false), GUILayout.MinWidth(60)))
                    SetFaceColors(palette[i]);

                EditorGUI.BeginChangeCheck();
                palette[i] = EditorGUILayout.ColorField(palette[i]);
                if (EditorGUI.EndChangeCheck())
                    UnityEditor.EditorUtility.SetDirty(palette);

                GUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        void ResetColors()
        {
            if (m_ColorPalette == null)
                m_ColorPalette = GetLastUsedColorPalette();

            m_ColorPalette.SetDefaultValues();
            UnityEditor.EditorUtility.SetDirty(m_ColorPalette);
        }

        public static void SetFaceColors(int index)
        {
            var palette = GetLastUsedColorPalette();
            SetFaceColors(palette[index]);
        }

        public static void SetFaceColors(Color col)
        {
            col = PlayerSettings.colorSpace == ColorSpace.Linear ? col.linear : col;

            ProBuilderMesh[] selection = InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms);

            UndoUtility.RecordSelection(selection, "Apply Vertex Colors");

            ProBuilderEditor editor = ProBuilderEditor.instance;

            if (editor && ProBuilderEditor.selectMode.ContainsFlag(SelectMode.Vertex | SelectMode.Edge | SelectMode.Face))
            {
                switch (ProBuilderEditor.selectMode)
                {
                    case SelectMode.Face:
                    case SelectMode.TextureFace:
                        foreach (ProBuilderMesh mesh in selection)
                        {
                            Color[] colors = mesh.GetColors();

                            foreach (int i in mesh.selectedIndexesInternal)
                                colors[i] = col;

                            mesh.colors = colors;
                        }
                        break;
                    case SelectMode.Edge:
                    case SelectMode.Vertex:
                        foreach (var mesh in selection)
                        {
                            Color[] colors = mesh.GetColors();

                            foreach (int i in mesh.GetCoincidentVertices(mesh.selectedIndexesInternal))
                                colors[i] = col;

                            mesh.colors = colors;
                        }
                        break;
                }
            }
            else
            {
                foreach (ProBuilderMesh pb in selection)
                {
                    foreach (Face face in pb.facesInternal)
                        pb.SetFaceColor(face, col);
                }
            }

            foreach (ProBuilderMesh pb in selection)
            {
                pb.ToMesh();
                pb.Refresh();
                pb.Optimize();
            }

            EditorUtility.ShowNotification("Set Vertex Colors\n" + ColorUtility.GetColorName(col));
        }
    }
}

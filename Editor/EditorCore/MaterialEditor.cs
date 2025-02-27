#pragma warning disable 0618

using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShortcutManagement;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using System;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Assign materials to faces and objects.
    /// </summary>
    sealed class MaterialEditor : ConfigurableWindow
    {
        class MaterialShortcutContext : IShortcutContext
        {
            public bool active
                => EditorWindow.focusedWindow is SceneView
                   && instance != null && MeshSelection.selectedObjectCount > 0
                   && instance.m_QueuedMaterial.value != null;
        }

        // Reference to pb_Editor instance.
        static ProBuilderEditor editor { get { return ProBuilderEditor.instance; } }

        // Reference to the currently open pb_Material_Editor
        public static MaterialEditor instance { get; private set; }

        const string k_QuickMaterialPath = "Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Quick Material";

        [MenuItem(k_QuickMaterialPath, true, PreferenceKeys.menuMaterialColors)]
        public static bool VerifyQuickMaterialAction()
        {
            return ProBuilderEditor.instance != null && MeshSelection.selectedObjectCount > 0 && instance != null && instance.m_QueuedMaterial.value != null;
        }

        [MenuItem(k_QuickMaterialPath, false, PreferenceKeys.menuMaterialColors)]
        public static void ApplyQuickMaterial()
        {
            ApplyMaterial(MeshSelection.topInternal, instance.m_QueuedMaterial.value);
        }

        [Shortcut("ProBuilder/Apply Quick Material", typeof(MaterialShortcutContext), KeyCode.Mouse2, defaultShortcutModifiers: ShortcutModifiers.Shift | ShortcutModifiers.Control)]
        public static void ApplyQuickMaterialShortcut()
        {
            ApplyMaterial(MeshSelection.topInternal, instance.m_QueuedMaterial.value);
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 1", true, PreferenceKeys.menuMaterialColors)]
        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 2", true, PreferenceKeys.menuMaterialColors)]
        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 3", true, PreferenceKeys.menuMaterialColors)]
        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 4", true, PreferenceKeys.menuMaterialColors)]
        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 5", true, PreferenceKeys.menuMaterialColors)]
        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 6", true, PreferenceKeys.menuMaterialColors)]
        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 7", true, PreferenceKeys.menuMaterialColors)]
        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 8", true, PreferenceKeys.menuMaterialColors)]
        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 9", true, PreferenceKeys.menuMaterialColors)]
        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 10", true, PreferenceKeys.menuMaterialColors)]
        public static bool VerifyMaterialAction()
        {
            return ProBuilderEditor.instance != null && MeshSelection.selectedObjectCount > 0;
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 1", false, PreferenceKeys.menuMaterialColors)]
        public static void ApplyMaterial0()
        {
            ApplyMaterial(MeshSelection.topInternal, CurrentPalette[0]);
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 2", false, PreferenceKeys.menuMaterialColors)]
        public static void ApplyMaterial1()
        {
            ApplyMaterial(MeshSelection.topInternal, CurrentPalette[1]);
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 3", false, PreferenceKeys.menuMaterialColors)]
        public static void ApplyMaterial2()
        {
            ApplyMaterial(MeshSelection.topInternal, CurrentPalette[2]);
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 4", false, PreferenceKeys.menuMaterialColors)]
        public static void ApplyMaterial3()
        {
            ApplyMaterial(MeshSelection.topInternal, CurrentPalette[3]);
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 5", false, PreferenceKeys.menuMaterialColors)]
        public static void ApplyMaterial4()
        {
            ApplyMaterial(MeshSelection.topInternal, CurrentPalette[4]);
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 6", false, PreferenceKeys.menuMaterialColors)]
        public static void ApplyMaterial5()
        {
            ApplyMaterial(MeshSelection.topInternal, CurrentPalette[5]);
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 7", false, PreferenceKeys.menuMaterialColors)]
        public static void ApplyMaterial6()
        {
            ApplyMaterial(MeshSelection.topInternal, CurrentPalette[6]);
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 8", false, PreferenceKeys.menuMaterialColors)]
        public static void ApplyMaterial7()
        {
            ApplyMaterial(MeshSelection.topInternal, CurrentPalette[7]);
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 9", false, PreferenceKeys.menuMaterialColors)]
        public static void ApplyMaterial8()
        {
            ApplyMaterial(MeshSelection.topInternal, CurrentPalette[8]);
        }

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset 10", false, PreferenceKeys.menuMaterialColors)]
        public static void ApplyMaterial9()
        {
            ApplyMaterial(MeshSelection.topInternal, CurrentPalette[9]);
        }

        // Path to the required default material palette. If not valid material palettes are
        // found a new one will be created with this path (relative to ProBuilder folder).
        static Pref<string> s_MaterialPalettePath = new Pref<string>("editor.materialPalettePath", FileUtility.GetLocalDataDirectory() + "/Default Material Palette.asset");

        // The currently loaded material palette asset.
        static MaterialPalette s_CurrentPalette = null;

        // The user set "quick material"
        Pref<Material> m_QueuedMaterial = new Pref<Material>("materialEditor.quickMaterial", null, SettingsScope.User);

        // Custom style for material row background
        GUIStyle m_RowBackgroundStyle;

        // The view scroll position.
        Vector2 m_ViewScroll = Vector2.zero;

        // All available material palettes
        MaterialPalette[] m_AvailablePalettes = null;

        // List of string names for all available palettes (plus one entry for 'Add New')
        string[] m_AvailablePalettes_Str = null;

        // The index of the currently loaded material palette in m_AvailablePalettes
        int m_CurrentPaletteIndex = 0;

        MaterialShortcutContext m_ShortcutContext;

        /// <summary>
        /// The currently loaded material palette, or a default.
        /// </summary>
        public static MaterialPalette CurrentPalette
        {
            get
            {
                if (s_CurrentPalette == null)
                {
                    // Attempt to load the last user-set material palette
                    s_CurrentPalette = AssetDatabase.LoadAssetAtPath<MaterialPalette>(s_MaterialPalettePath);

                    // If not set (or deleted), fall back on default
                    if (s_CurrentPalette != null)
                        return s_CurrentPalette;

                    // No dice - iterate any other pb_MaterialPalette objects in the project (favoring first found)
                    s_CurrentPalette = FileUtility.FindAssetOfType<MaterialPalette>();

                    if (s_CurrentPalette != null)
                        return s_CurrentPalette;

                    // If no existing pb_MaterialPalette objects in project:
                    // - create a new one
                    // - check for the older pb_ObjectArray and copy data to new default
                    s_CurrentPalette = FileUtility.LoadRequired<MaterialPalette>(s_MaterialPalettePath);

                    string[] m_LegacyMaterialArrays = AssetDatabase.FindAssets("t:pb_ObjectArray");

                    for (int i = 0; m_LegacyMaterialArrays != null && i < m_LegacyMaterialArrays.Length; i++)
                    {
                        pb_ObjectArray poa = AssetDatabase.LoadAssetAtPath<pb_ObjectArray>(AssetDatabase.GUIDToAssetPath(m_LegacyMaterialArrays[i]));

                        // Make sure there's actually something worth copying
                        if (poa != null && poa.array != null && poa.array.Any(x => x != null && x is Material))
                        {
                            s_CurrentPalette.array = poa.GetArray<Material>();
                            break;
                        }
                    }
                }
                return s_CurrentPalette;
            }
        }

        public static void MenuOpenMaterialEditor()
        {
            GetWindow<MaterialEditor>("Material Editor");
        }

        void OnEnable()
        {
            instance = this;
            this.autoRepaintOnSceneChange = true;
            this.minSize = new Vector2(236, 250);
            m_RowBackgroundStyle = new GUIStyle();
            m_RowBackgroundStyle.normal.background = EditorGUIUtility.whiteTexture;
            s_CurrentPalette = null;
            RefreshAvailablePalettes();

            ShortcutManager.RegisterContext(m_ShortcutContext ??= new MaterialShortcutContext());
        }

        void OnDisable()
        {
            ShortcutManager.UnregisterContext(m_ShortcutContext);
            instance = null;
        }

        void OnGUI()
        {
            DoContextMenu();

            GUILayout.Label("Quick Material", EditorStyles.boldLabel);
            Rect r = GUILayoutUtility.GetLastRect();
            int left = (int)position.width - 68;

            GUILayout.BeginHorizontal(GUILayout.MaxWidth(position.width - 74), GUILayout.MinHeight(64));
            GUILayout.BeginVertical();

            GUILayout.BeginHorizontal();
            m_QueuedMaterial.value = (Material)EditorGUILayout.ObjectField(m_QueuedMaterial.value, typeof(Material), true);

            GUILayout.Space(2);

            GUI.enabled = editor != null && MeshSelection.selectedFaceCount > 0;
            if (GUILayout.Button("Match Selection", GUILayout.MaxWidth(120)))
            {
                m_QueuedMaterial.SetValue(EditorMaterialUtility.GetActiveSelection());
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();

            var quickMaterialShortcut = string.Empty;
            try
            {
                quickMaterialShortcut = ShortcutManager.instance.GetShortcutBinding("Main Menu/" + k_QuickMaterialPath).ToString();
            }
            catch (Exception)
            {
                // Do nothing.
            }

            var quickMaterialButtonLabel = string.IsNullOrEmpty(quickMaterialShortcut) ? "Apply" : $"Apply ({quickMaterialShortcut})";
            if (GUILayout.Button(quickMaterialButtonLabel))
                ApplyMaterial(MeshSelection.topInternal, m_QueuedMaterial.value);

            GUILayout.EndVertical();

            GUI.Box(new Rect(left, r.y + r.height + 2, 64, 64), "");

            var previewTexture = EditorMaterialUtility.GetPreviewTexture(m_QueuedMaterial.value);

            if (previewTexture != null)
            {
                GUI.Label(new Rect(left + 2, r.y + r.height + 4, 60, 60), previewTexture);
            }
            else
            {
                GUI.Box(new Rect(left + 2, r.y + r.height + 4, 60, 60), "");
                GUI.Label(new Rect(left + 2, r.height + 28, 120, 32), "None\n(Texture)");
            }

            GUILayout.EndHorizontal();

            GUILayout.Space(4);

            GUI.backgroundColor = PreferenceKeys.proBuilderDarkGray;
            UI.EditorGUIUtility.DrawSeparator(2);
            GUI.backgroundColor = Color.white;

            GUILayout.Label("Material Palette", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            m_CurrentPaletteIndex = EditorGUILayout.Popup("", m_CurrentPaletteIndex, m_AvailablePalettes_Str);

            if (EditorGUI.EndChangeCheck())
            {
                MaterialPalette newPalette = null;

                // Add a new material palette
                if (m_CurrentPaletteIndex >= m_AvailablePalettes.Length)
                {
                    string path = AssetDatabase.GenerateUniqueAssetPath("Assets/Material Palette.asset");
                    newPalette = FileUtility.LoadRequired<MaterialPalette>(path);
                    EditorGUIUtility.PingObject(newPalette);
                }
                else
                {
                    newPalette = m_AvailablePalettes[m_CurrentPaletteIndex];
                }

                SetMaterialPalette(newPalette);
            }

            EditorGUI.BeginChangeCheck();
            s_CurrentPalette = (MaterialPalette)EditorGUILayout.ObjectField(s_CurrentPalette, typeof(MaterialPalette), false);
            if (EditorGUI.EndChangeCheck())
                SetMaterialPalette(s_CurrentPalette);

            GUILayout.Space(4);

            Material[] materials = CurrentPalette;

            m_ViewScroll = GUILayout.BeginScrollView(m_ViewScroll);

            for (int i = 0; i < materials.Length; i++)
            {
                if (i == 10)
                {
                    GUILayout.Space(2);
                    GUI.backgroundColor = PreferenceKeys.proBuilderLightGray;
                    UI.EditorGUIUtility.DrawSeparator(1);
                    GUI.backgroundColor = Color.white;
                    GUILayout.Space(2);
                }

                GUILayout.BeginHorizontal();
                if (i < 10)
                {
                    var applyMaterialPresetShortcut = string.Empty;
                    try
                    {
                        var shortcutPath = "Main Menu/Tools/" + PreferenceKeys.pluginTitle + "/Materials/Apply Material Preset " + (i + 1);
                        applyMaterialPresetShortcut = ShortcutManager.instance.GetShortcutBinding(shortcutPath).ToString();
                    }
                    catch (Exception)
                    {
                        // Do nothing.
                    }

                    var applyMaterialPresetButtonLabel = string.IsNullOrEmpty(applyMaterialPresetShortcut)
                        ? $"Apply {i}"
                        : $"Apply {i} ({applyMaterialPresetShortcut})";
                    if (GUILayout.Button(applyMaterialPresetButtonLabel, EditorStyles.miniButton, GUILayout.MinWidth(50), GUILayout.MaxWidth(150)))
                        ApplyMaterial(MeshSelection.topInternal, materials[i]);
                }
                else
                {
                    if (GUILayout.Button("Apply", EditorStyles.miniButtonLeft, GUILayout.MaxWidth(44)))
                        ApplyMaterial(MeshSelection.topInternal, materials[i]);

                    GUI.backgroundColor = Color.red;
                    if (GUILayout.Button("", EditorStyles.miniButtonRight, GUILayout.MaxWidth(14)))
                    {
                        Material[] temp = new Material[materials.Length - 1];
                        System.Array.Copy(materials, 0, temp, 0, materials.Length - 1);
                        materials = temp;
                        SaveUserMaterials(materials);
                        return;
                    }
                    GUI.backgroundColor = Color.white;
                }

                EditorGUI.BeginChangeCheck();
                materials[i] = (Material)EditorGUILayout.ObjectField(materials[i], typeof(Material), false);
                if (EditorGUI.EndChangeCheck())
                    SaveUserMaterials(materials);

                GUILayout.EndHorizontal();
            }


            if (GUILayout.Button("Add"))
            {
                Material[] temp = new Material[materials.Length + 1];
                System.Array.Copy(materials, 0, temp, 0, materials.Length);
                materials = temp;
                SaveUserMaterials(materials);
            }

            GUILayout.EndScrollView();
        }

        static void ApplyMaterial(IEnumerable<ProBuilderMesh> selection, Material mat)
        {
            if (mat == null)
                return;

            UndoUtility.RecordComponents<MeshRenderer, ProBuilderMesh>(selection, "Set Material");

            foreach (var mesh in selection)
            {
                var applyPerFace = ProBuilderEditor.instance != null && ProBuilderEditor.selectMode.ContainsFlag(SelectMode.Face) && mesh.faceCount > 0;
                mesh.SetMaterial(applyPerFace ? mesh.GetSelectedFaces() : mesh.facesInternal, mat);
                InternalMeshUtility.FilterUnusedSubmeshIndexes(mesh);
                mesh.Rebuild();
                mesh.Optimize();
            }

            if (ProBuilderEditor.instance != null && MeshSelection.selectedFaceCount > 0)
                EditorUtility.ShowNotification("Set Material\n" + mat.name);
        }

        static void SaveUserMaterials(Material[] materials)
        {
            s_CurrentPalette.array = materials;
            UnityEditor.EditorUtility.SetDirty(s_CurrentPalette);
            AssetDatabase.SaveAssets();
        }

        void SetMaterialPalette(MaterialPalette palette)
        {
            s_CurrentPalette = palette;
            RefreshAvailablePalettes();
        }

        void RefreshAvailablePalettes()
        {
            MaterialPalette cur = CurrentPalette;
            m_AvailablePalettes = FileUtility.FindAndLoadAssets<MaterialPalette>();
            m_AvailablePalettes_Str = m_AvailablePalettes.Select(x => x.name).ToArray();
            ArrayUtility.Add<string>(ref m_AvailablePalettes_Str, string.Empty);
            ArrayUtility.Add<string>(ref m_AvailablePalettes_Str, "New Material Palette...");
            m_CurrentPaletteIndex = System.Array.IndexOf(m_AvailablePalettes, cur);
            s_MaterialPalettePath.SetValue(AssetDatabase.GetAssetPath(cur), true);
        }
    }
}

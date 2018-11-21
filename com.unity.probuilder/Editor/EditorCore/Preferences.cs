using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    static class Preferences
    {
#pragma warning disable 618
#pragma warning disable 612

        static int s_ShortcutIndex = 0;
        static Vector2 s_ShortcutScroll = Vector2.zero;
        const int k_ShortcutLineHeight = 20;

        static bool s_PrefsLoaded = false;

        static bool pbUseUnityColors;
        static float pbLineHandleSize;
        static float pbWireframeSize;
        static Color faceSelectedColor;
        static Color pbWireframeColor;
        static Color pbSelectedEdgeColor;
        static Color pbUnselectedEdgeColor;
        static Color vertexSelectedColor;
        static Color vertexUnselectedColor;
        static Color pbPreselectionColor;
        static bool pbSelectedFaceDither;

        static bool defaultOpenInDockableWindow;
        static Material pbDefaultMaterial;
        static Vector2 s_SettingsScroll = Vector2.zero;
        static bool pbShowEditorNotifications;
        static bool pbForceConvex;
        static bool pbForceVertexPivot;
        static bool pbForceGridPivot;
        static bool pbPerimeterEdgeBridgeOnly;
        static bool pbPBOSelectionOnly;
        static bool pbCloseShapeWindow;
        static bool pbUVEditorFloating;
        static bool pbStripProBuilderOnBuild;
        static bool pbDisableAutoUV2Generation;
        static bool pbShowSceneInfo;
        static bool pbUniqueModeShortcuts;
        static bool pbIconGUI;
        static bool pbShiftOnlyTooltips;
        static bool pbMeshesAreAssets;
        static bool pbEnableExperimental;
        static bool pbShowPreselectionHighlight;

        static bool showMissingLightmapUvWarning;
        static bool pbManageLightmappingStaticFlag;
        static ShadowCastingMode pbShadowCastingMode = ShadowCastingMode.On;

        static StaticEditorFlags pbDefaultStaticFlags = (StaticEditorFlags)0xFFFF;

        static ColliderType defaultColliderType = ColliderType.BoxCollider;
        static SceneToolbarLocation pbToolbarLocation = SceneToolbarLocation.UpperCenter;

        static float pbUVGridSnapValue;
        static float pbVertexHandleSize;

        static Shortcut[] defaultShortcuts;

        static void PreferencesGUI()
        {
            LoadPrefs();

            EditorGUIUtility.labelWidth = 200f;

            s_SettingsScroll = EditorGUILayout.BeginScrollView(s_SettingsScroll);

            EditorGUI.BeginChangeCheck();

            if (GUILayout.Button("Reset All Preferences"))
                ResetToDefaults();

            /**
             * GENERAL SETTINGS
             */
            GUILayout.Label("General Settings", EditorStyles.boldLabel);

            pbStripProBuilderOnBuild =
                EditorGUILayout.Toggle(
                    new GUIContent("Strip PB Scripts on Build",
                        "If true, when building an executable all ProBuilder scripts will be stripped from your built product."),
                    pbStripProBuilderOnBuild);
            pbDisableAutoUV2Generation = EditorGUILayout.Toggle(
                    new GUIContent("Disable Auto UV2 Generation",
                        "Disables automatic generation of UV2 channel.  If Unity is sluggish when working with large ProBuilder objects, disabling UV2 generation will improve performance.  Use `Actions/Generate UV2` or `Actions/Generate Scene UV2` to build lightmap UVs prior to baking."),
                    pbDisableAutoUV2Generation);
            pbShowSceneInfo =
                EditorGUILayout.Toggle(
                    new GUIContent("Show Scene Info", "Displays the selected object vertex and triangle counts in the scene view."),
                    pbShowSceneInfo);
            pbShowEditorNotifications = EditorGUILayout.Toggle("Show Editor Notifications", pbShowEditorNotifications);

            /**
             * TOOLBAR SETTINGS
             */
            GUILayout.Label("Toolbar Settings", EditorStyles.boldLabel);

            pbIconGUI = EditorGUILayout.Toggle(
                    new GUIContent("Use Icon GUI", "Toggles the ProBuilder window interface between text and icon versions."),
                    pbIconGUI);
            pbShiftOnlyTooltips =
                EditorGUILayout.Toggle(new GUIContent("Shift Key Tooltips", "Tooltips will only show when the Shift key is held"),
                    pbShiftOnlyTooltips);
            pbToolbarLocation = (SceneToolbarLocation)EditorGUILayout.EnumPopup("Toolbar Location", pbToolbarLocation);

            pbUniqueModeShortcuts = EditorGUILayout.Toggle(
                    new GUIContent("Unique Mode Shortcuts",
                        "When off, the G key toggles between Object and Element modes and H enumerates the element modes.  If on, G, H, J, and K are shortcuts to Object, Vertex, Edge, and Face modes respectively."),
                    pbUniqueModeShortcuts);
            defaultOpenInDockableWindow = EditorGUILayout.Toggle("Open in Dockable Window", defaultOpenInDockableWindow);

            /**
             * DEFAULT SETTINGS
             */
            GUILayout.Label("Defaults", EditorStyles.boldLabel);

            pbDefaultMaterial =
                (Material)EditorGUILayout.ObjectField("Default Material", pbDefaultMaterial, typeof(Material), false);

            pbDefaultStaticFlags = (StaticEditorFlags)EditorGUILayout.EnumFlagsField("Static Flags", pbDefaultStaticFlags);

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Default Collider");
            defaultColliderType = ((ColliderType)EditorGUILayout.EnumPopup((ColliderType)defaultColliderType));
            GUILayout.EndHorizontal();

            if ((ColliderType)defaultColliderType == ColliderType.MeshCollider)
                pbForceConvex = EditorGUILayout.Toggle("Force Convex Mesh Collider", pbForceConvex);

            GUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Shadow Casting Mode");
            pbShadowCastingMode = (ShadowCastingMode)EditorGUILayout.EnumPopup(pbShadowCastingMode);
            GUILayout.EndHorizontal();

            /**
             * HANDLE COLORS
             */
            GUILayout.Label("Handles & Colors", EditorStyles.boldLabel);

            pbUseUnityColors = EditorGUILayout.Toggle("Use Unity Colors", pbUseUnityColors);

            if (!pbUseUnityColors)
            {
                pbWireframeColor = EditorGUILayout.ColorField("Wireframe", pbWireframeColor);
                pbPreselectionColor = EditorGUILayout.ColorField("Preselection", pbPreselectionColor);
                faceSelectedColor = EditorGUILayout.ColorField("Selected Face Color", faceSelectedColor);
                pbSelectedFaceDither = EditorGUILayout.Toggle("Dither Face Overlay", pbSelectedFaceDither);
                pbUnselectedEdgeColor = EditorGUILayout.ColorField("Unselected Edge Color", pbUnselectedEdgeColor);
                pbSelectedEdgeColor = EditorGUILayout.ColorField("Selected Edge Color", pbSelectedEdgeColor);
                vertexUnselectedColor = EditorGUILayout.ColorField("Unselected Vertex Color", vertexUnselectedColor);
                vertexSelectedColor = EditorGUILayout.ColorField("Selected Vertex Color", vertexSelectedColor);
            }

            pbVertexHandleSize = EditorGUILayout.Slider("Vertex Size", pbVertexHandleSize, 1f, 10f);

            bool geoLine = BuiltinMaterials.geometryShadersSupported;
            GUI.enabled = geoLine;
            pbLineHandleSize = EditorGUILayout.Slider("Line Size", geoLine ? pbLineHandleSize : 0f, 0f, 3f);
            pbWireframeSize = EditorGUILayout.Slider("Wireframe Size", geoLine ? pbWireframeSize : 0f, 0f, 3f);
            GUI.enabled = true;

            /**
             * MISC. SETTINGS
             */
            GUILayout.Label("Misc. Settings", EditorStyles.boldLabel);

            pbManageLightmappingStaticFlag = EditorGUILayout.Toggle(
                    new GUIContent("Manage Lightmap Static Flag",
                        "Allow ProBuilder to toggle off the Lightmap Static flag when no UV2 channel is present. This prevents lighting artifacts caused by a missing UV2 channel."),
                    pbManageLightmappingStaticFlag);

            showMissingLightmapUvWarning = EditorGUILayout.Toggle("Show Missing Lightmap UVs Warning", showMissingLightmapUvWarning);

            pbPBOSelectionOnly =
                EditorGUILayout.Toggle(
                    new GUIContent("Only PBO are Selectable",
                        "If true, you will not be able to select non probuilder objects in Geometry and Texture mode"),
                    pbPBOSelectionOnly);
            pbCloseShapeWindow =
                EditorGUILayout.Toggle(
                    new GUIContent("Close shape window after building",
                        "If true the shape window will close after hitting the build button"), pbCloseShapeWindow);

            GUILayout.Space(4);

            /**
             * GEOMETRY EDITING SETTINGS
             */
            GUILayout.Label("Geometry Editing Settings", EditorStyles.boldLabel);

            pbForceVertexPivot =
                EditorGUILayout.Toggle(
                    new GUIContent("Force Pivot to Vertex Point",
                        "If true, new objects will automatically have their pivot point set to a vertex instead of the center."),
                    pbForceVertexPivot);
            pbForceGridPivot = EditorGUILayout.Toggle(
                    new GUIContent("Force Pivot to Grid",
                        "If true, newly instantiated pb_Objects will be snapped to the nearest point on grid.  If ProGrids is present, the snap value will be used, otherwise decimals are simply rounded to whole numbers."),
                    pbForceGridPivot);
            pbPerimeterEdgeBridgeOnly = EditorGUILayout.Toggle(
                    new GUIContent("Bridge Perimeter Edges Only",
                        "If true, only edges on the perimeters of an object may be bridged.  If false, you may bridge any between any two edges you like."),
                    pbPerimeterEdgeBridgeOnly);

            GUILayout.Space(4);

            GUILayout.Label("Experimental", EditorStyles.boldLabel);

            pbEnableExperimental = EditorGUILayout.Toggle(
                    new GUIContent("Experimental Features",
                        "Enables some experimental new features that we're trying out.  These may be incomplete or buggy, so please exercise caution when making use of this functionality!"),
                    pbEnableExperimental);

            pbMeshesAreAssets =
                EditorGUILayout.Toggle(
                    new GUIContent("Meshes Are Assets",
                        "Experimental!  Instead of storing mesh data in the scene, this toggle creates a Mesh cache in the Project that ProBuilder will use."),
                    pbMeshesAreAssets);

            pbShowPreselectionHighlight = EditorGUILayout.Toggle("Show Preselection Highlight", pbShowPreselectionHighlight);

            GUILayout.Space(4);

            /**
             * UV EDITOR SETTINGS
             */
            GUILayout.Label("UV Editing Settings", EditorStyles.boldLabel);
            pbUVGridSnapValue = EditorGUILayout.FloatField("UV Snap Increment", pbUVGridSnapValue);
            pbUVGridSnapValue = Mathf.Clamp(pbUVGridSnapValue, .015625f, 2f);
            pbUVEditorFloating =
                EditorGUILayout.Toggle(
                    new GUIContent("Editor window floating", "If true UV   Editor window will open as a floating window"),
                    pbUVEditorFloating);

            GUILayout.Space(4);

            GUILayout.Label("Shortcut Settings", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.MinWidth(EditorGUIUtility.labelWidth), GUILayout.MaxWidth(EditorGUIUtility.labelWidth));
            ShortcutSelectPanel();
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            ShortcutEditPanel();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            EditorGUILayout.EndScrollView();

            // Save the preferences
            if (EditorGUI.EndChangeCheck())
                SetPrefs();

            EditorGUIUtility.labelWidth = 0f;
        }

        public static void ResetToDefaults()
        {
            if (UnityEditor.EditorUtility.DisplayDialog("Delete ProBuilder editor preferences?",
                    "Are you sure you want to delete all existing ProBuilder preferences?\n\nThis action cannot be undone.", "Yes",
                    "No"))
            {
                PreferencesInternal.DeleteKey(PreferenceKeys.pbDefaultEditLevel);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbDefaultSelectionMode);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbHandleAlignment);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbVertexColorTool);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbToolbarLocation);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbDefaultEntity);

                PreferencesInternal.DeleteKey(PreferenceKeys.pbUseUnityColors);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbLineHandleSize);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbWireframeSize);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbSelectedFaceColor);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbWireframeColor);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbPreselectionColor);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbSelectedFaceDither);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbSelectedVertexColor);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbUnselectedVertexColor);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbSelectedEdgeColor);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbUnselectedEdgeColor);

                PreferencesInternal.DeleteKey(PreferenceKeys.pbDefaultOpenInDockableWindow);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbEditorPrefVersion);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbEditorShortcutsVersion);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbDefaultCollider);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbForceConvex);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbVertexColorPrefs);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbShowEditorNotifications);
// //#pragma warning disable 612
                PreferencesInternal.DeleteKey(PreferenceKeys.pbDragCheckLimit);
// #pragma warning restore 612
                PreferencesInternal.DeleteKey(PreferenceKeys.pbForceVertexPivot);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbForceGridPivot);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbManifoldEdgeExtrusion);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbPerimeterEdgeBridgeOnly);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbPBOSelectionOnly);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbCloseShapeWindow);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbUVEditorFloating);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbUVMaterialPreview);
// #pragma warning disable 612
                PreferencesInternal.DeleteKey(PreferenceKeys.pbShowSceneToolbar);
// #pragma warning restore 612
                PreferencesInternal.DeleteKey(PreferenceKeys.pbNormalizeUVsOnPlanarProjection);
// #pragma warning disable 618
                PreferencesInternal.DeleteKey(PreferenceKeys.pbStripProBuilderOnBuild);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbDisableAutoUV2Generation);
// #pragma warning restore 618
                PreferencesInternal.DeleteKey(PreferenceKeys.pbShowSceneInfo);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbEnableBackfaceSelection);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbVertexPaletteDockable);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbExtrudeAsGroup);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbUniqueModeShortcuts);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbMaterialEditorFloating);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbShapeWindowFloating);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbIconGUI);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbShiftOnlyTooltips);
// #pragma warning disable 612
                PreferencesInternal.DeleteKey(PreferenceKeys.pbDrawAxisLines);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbElementSelectIsHamFisted);
// #pragma warning restore 612
                PreferencesInternal.DeleteKey(PreferenceKeys.pbCollapseVertexToFirst);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbMeshesAreAssets);
// #pragma warning disable 618
                PreferencesInternal.DeleteKey(PreferenceKeys.pbDragSelectWholeElement);
// #pragma warning restore 618
                PreferencesInternal.DeleteKey(PreferenceKeys.pbEnableExperimental);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbFillHoleSelectsEntirePath);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbDetachToNewObject);
// #pragma warning disable 618
                PreferencesInternal.DeleteKey(PreferenceKeys.pbPreserveFaces);
// #pragma warning restore 618
                PreferencesInternal.DeleteKey(PreferenceKeys.pbVertexHandleSize);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbUVGridSnapValue);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbUVWeldDistance);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbWeldDistance);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbExtrudeDistance);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbBevelAmount);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbEdgeSubdivisions);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbDefaultShortcuts);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbDefaultMaterial);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbGrowSelectionUsingAngle);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbGrowSelectionAngle);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbGrowSelectionAngleIterative);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbShowDetail);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbShowOccluder);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbShowMover);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbShowCollider);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbShowTrigger);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbShowNoDraw);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbShowMissingLightmapUvWarning);
// #pragma warning disable 612
                PreferencesInternal.DeleteKey(PreferenceKeys.pbManageLightmappingStaticFlag);
// #pragma warning restore 612
                PreferencesInternal.DeleteKey(PreferenceKeys.pbShadowCastingMode);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbDefaultStaticFlags);
                PreferencesInternal.DeleteKey(PreferenceKeys.pbShowPreselectionHighlight);
            }

            s_PrefsLoaded = false;
            LoadPrefs();
        }

        static void ShortcutSelectPanel()
        {
            GUILayout.Space(4);
            GUI.contentColor = Color.white;

            GUIStyle labelStyle = GUIStyle.none;

            if (EditorGUIUtility.isProSkin)
                labelStyle.normal.textColor = new Color(1f, 1f, 1f, .8f);

            labelStyle.alignment = TextAnchor.MiddleLeft;
            labelStyle.contentOffset = new Vector2(4f, 0f);

            s_ShortcutScroll =
                EditorGUILayout.BeginScrollView(s_ShortcutScroll, false, true, GUILayout.MinHeight(150));

            for (int n = 1; n < defaultShortcuts.Length; n++)
            {
                if (n == s_ShortcutIndex)
                {
                    GUI.backgroundColor = new Color(0.23f, .49f, .89f, 1f);
                    labelStyle.normal.background = EditorGUIUtility.whiteTexture;
                    Color oc = labelStyle.normal.textColor;
                    labelStyle.normal.textColor = Color.white;
                    GUILayout.Box(defaultShortcuts[n].action, labelStyle, GUILayout.MinHeight(k_ShortcutLineHeight),
                        GUILayout.MaxHeight(k_ShortcutLineHeight));
                    labelStyle.normal.background = null;
                    labelStyle.normal.textColor = oc;
                    GUI.backgroundColor = Color.white;
                }
                else
                {
                    if (GUILayout.Button(defaultShortcuts[n].action, labelStyle, GUILayout.MinHeight(k_ShortcutLineHeight),
                            GUILayout.MaxHeight(k_ShortcutLineHeight)))
                    {
                        s_ShortcutIndex = n;
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        static void ShortcutEditPanel()
        {
            GUILayout.Label("Key", EditorStyles.boldLabel);
            KeyCode key = defaultShortcuts[s_ShortcutIndex].key;
            key = (KeyCode)EditorGUILayout.EnumPopup(key);
            defaultShortcuts[s_ShortcutIndex].key = key;

            GUILayout.Label("Modifiers", EditorStyles.boldLabel);
            // EnumMaskField returns a bit-mask where the flags correspond to the indexes of the enum, not the enum values,
            // so this isn't technically correct.
#if UNITY_2017_3_OR_NEWER
            EventModifiers em = (EventModifiers)defaultShortcuts[s_ShortcutIndex].eventModifiers;
            defaultShortcuts[s_ShortcutIndex].eventModifiers = (EventModifiers)EditorGUILayout.EnumFlagsField(em);
#else
            EventModifiers em = (EventModifiers)(((int)defaultShortcuts[shortcutIndex].eventModifiers) * 2);
            em = (EventModifiers)EditorGUILayout.EnumMaskField(em);
            defaultShortcuts[shortcutIndex].eventModifiers = (EventModifiers)(((int)em) / 2);
#endif
            GUILayout.Label("Description", EditorStyles.boldLabel);

            GUILayout.Label(defaultShortcuts[s_ShortcutIndex].description, EditorStyles.wordWrappedLabel);
        }

        static void LoadPrefs()
        {
            if (s_PrefsLoaded)
                return;
            s_PrefsLoaded = true;
            pbStripProBuilderOnBuild = PreferencesInternal.GetBool(PreferenceKeys.pbStripProBuilderOnBuild);
            pbDisableAutoUV2Generation = PreferencesInternal.GetBool(PreferenceKeys.pbDisableAutoUV2Generation);
            pbShowSceneInfo = PreferencesInternal.GetBool(PreferenceKeys.pbShowSceneInfo);
            defaultOpenInDockableWindow = PreferencesInternal.GetBool(PreferenceKeys.pbDefaultOpenInDockableWindow);
            pbForceConvex = PreferencesInternal.GetBool(PreferenceKeys.pbForceConvex);
            pbForceGridPivot = PreferencesInternal.GetBool(PreferenceKeys.pbForceGridPivot);
            pbForceVertexPivot = PreferencesInternal.GetBool(PreferenceKeys.pbForceVertexPivot);
            pbPerimeterEdgeBridgeOnly = PreferencesInternal.GetBool(PreferenceKeys.pbPerimeterEdgeBridgeOnly);
            pbPBOSelectionOnly = PreferencesInternal.GetBool(PreferenceKeys.pbPBOSelectionOnly);
            pbCloseShapeWindow = PreferencesInternal.GetBool(PreferenceKeys.pbCloseShapeWindow);
            pbUVEditorFloating = PreferencesInternal.GetBool(PreferenceKeys.pbUVEditorFloating);
            pbShowEditorNotifications = PreferencesInternal.GetBool(PreferenceKeys.pbShowEditorNotifications);
            pbUniqueModeShortcuts = PreferencesInternal.GetBool(PreferenceKeys.pbUniqueModeShortcuts);
            pbIconGUI = PreferencesInternal.GetBool(PreferenceKeys.pbIconGUI);
            pbShiftOnlyTooltips = PreferencesInternal.GetBool(PreferenceKeys.pbShiftOnlyTooltips);
            pbMeshesAreAssets = PreferencesInternal.GetBool(PreferenceKeys.pbMeshesAreAssets);
            pbEnableExperimental = PreferencesInternal.GetBool(PreferenceKeys.pbEnableExperimental);
            showMissingLightmapUvWarning = PreferencesInternal.GetBool(PreferenceKeys.pbShowMissingLightmapUvWarning, false);
            pbShowPreselectionHighlight = PreferencesInternal.GetBool(PreferenceKeys.pbShowPreselectionHighlight);

            pbUseUnityColors = PreferencesInternal.GetBool(PreferenceKeys.pbUseUnityColors);
            pbLineHandleSize = PreferencesInternal.GetFloat(PreferenceKeys.pbLineHandleSize);
            pbWireframeSize = PreferencesInternal.GetFloat(PreferenceKeys.pbWireframeSize);
            faceSelectedColor = PreferencesInternal.GetColor(PreferenceKeys.pbSelectedFaceColor);
            pbWireframeColor = PreferencesInternal.GetColor(PreferenceKeys.pbWireframeColor);
            pbPreselectionColor = PreferencesInternal.GetColor(PreferenceKeys.pbPreselectionColor);
            pbSelectedFaceDither = PreferencesInternal.GetBool(PreferenceKeys.pbSelectedFaceDither);
            pbSelectedEdgeColor = PreferencesInternal.GetColor(PreferenceKeys.pbSelectedEdgeColor);
            pbUnselectedEdgeColor = PreferencesInternal.GetColor(PreferenceKeys.pbUnselectedEdgeColor);
            vertexSelectedColor = PreferencesInternal.GetColor(PreferenceKeys.pbSelectedVertexColor);
            vertexUnselectedColor = PreferencesInternal.GetColor(PreferenceKeys.pbUnselectedVertexColor);

            pbUVGridSnapValue = PreferencesInternal.GetFloat(PreferenceKeys.pbUVGridSnapValue);
            pbVertexHandleSize = PreferencesInternal.GetFloat(PreferenceKeys.pbVertexHandleSize);

            defaultColliderType = PreferencesInternal.GetEnum<ColliderType>(PreferenceKeys.pbDefaultCollider);
            pbToolbarLocation = PreferencesInternal.GetEnum<SceneToolbarLocation>(PreferenceKeys.pbToolbarLocation);
            pbShadowCastingMode = PreferencesInternal.GetEnum<ShadowCastingMode>(PreferenceKeys.pbShadowCastingMode);
            pbDefaultStaticFlags = PreferencesInternal.GetEnum<StaticEditorFlags>(PreferenceKeys.pbDefaultStaticFlags);

//          pbDefaultMaterial = PreferencesInternal.GetMaterial(PreferenceKeys.pbDefaultMaterial);
            defaultShortcuts = PreferencesInternal.GetShortcuts().ToArray();
        }

        public static void SetPrefs()
        {
            PreferencesInternal.SetBool(PreferenceKeys.pbStripProBuilderOnBuild, pbStripProBuilderOnBuild);
            PreferencesInternal.SetBool(PreferenceKeys.pbDisableAutoUV2Generation, pbDisableAutoUV2Generation);
            PreferencesInternal.SetBool(PreferenceKeys.pbShowSceneInfo, pbShowSceneInfo, PreferenceLocation.Global);
            PreferencesInternal.SetInt(PreferenceKeys.pbToolbarLocation, (int)pbToolbarLocation, PreferenceLocation.Global);

            PreferencesInternal.SetBool(PreferenceKeys.pbUseUnityColors, pbUseUnityColors, PreferenceLocation.Global);
            PreferencesInternal.SetBool(PreferenceKeys.pbSelectedFaceDither, pbSelectedFaceDither, PreferenceLocation.Global);
            PreferencesInternal.SetFloat(PreferenceKeys.pbLineHandleSize, pbLineHandleSize, PreferenceLocation.Global);
            PreferencesInternal.SetFloat(PreferenceKeys.pbVertexHandleSize, pbVertexHandleSize, PreferenceLocation.Global);
            PreferencesInternal.SetFloat(PreferenceKeys.pbWireframeSize, pbWireframeSize, PreferenceLocation.Global);
            PreferencesInternal.SetColor(PreferenceKeys.pbSelectedFaceColor, faceSelectedColor, PreferenceLocation.Global);
            PreferencesInternal.SetColor(PreferenceKeys.pbWireframeColor, pbWireframeColor, PreferenceLocation.Global);
            PreferencesInternal.SetColor(PreferenceKeys.pbPreselectionColor, pbPreselectionColor, PreferenceLocation.Global);
            PreferencesInternal.SetColor(PreferenceKeys.pbSelectedVertexColor, vertexSelectedColor, PreferenceLocation.Global);
            PreferencesInternal.SetColor(PreferenceKeys.pbUnselectedVertexColor, vertexUnselectedColor, PreferenceLocation.Global);
            PreferencesInternal.SetColor(PreferenceKeys.pbSelectedEdgeColor, pbSelectedEdgeColor, PreferenceLocation.Global);
            PreferencesInternal.SetColor(PreferenceKeys.pbUnselectedEdgeColor, pbUnselectedEdgeColor, PreferenceLocation.Global);

            PreferencesInternal.SetString(PreferenceKeys.pbDefaultShortcuts, Shortcut.ShortcutsToString(defaultShortcuts), PreferenceLocation.Global);
//          PreferencesInternal.SetMaterial(PreferenceKeys.pbDefaultMaterial, pbDefaultMaterial);
            PreferencesInternal.SetInt(PreferenceKeys.pbDefaultCollider, (int)defaultColliderType);
            PreferencesInternal.SetInt(PreferenceKeys.pbShadowCastingMode, (int)pbShadowCastingMode);
            PreferencesInternal.SetInt(PreferenceKeys.pbDefaultStaticFlags, (int)pbDefaultStaticFlags);
            PreferencesInternal.SetBool(PreferenceKeys.pbDefaultOpenInDockableWindow, defaultOpenInDockableWindow, PreferenceLocation.Global);
            PreferencesInternal.SetBool(PreferenceKeys.pbShowEditorNotifications, pbShowEditorNotifications, PreferenceLocation.Global);
            PreferencesInternal.SetBool(PreferenceKeys.pbForceConvex, pbForceConvex);
            PreferencesInternal.SetBool(PreferenceKeys.pbForceVertexPivot, pbForceVertexPivot, PreferenceLocation.Global);
            PreferencesInternal.SetBool(PreferenceKeys.pbForceGridPivot, pbForceGridPivot, PreferenceLocation.Global);
            PreferencesInternal.SetBool(PreferenceKeys.pbPerimeterEdgeBridgeOnly, pbPerimeterEdgeBridgeOnly, PreferenceLocation.Global);
            PreferencesInternal.SetBool(PreferenceKeys.pbPBOSelectionOnly, pbPBOSelectionOnly, PreferenceLocation.Global);
            PreferencesInternal.SetBool(PreferenceKeys.pbCloseShapeWindow, pbCloseShapeWindow, PreferenceLocation.Global);
            PreferencesInternal.SetBool(PreferenceKeys.pbUVEditorFloating, pbUVEditorFloating, PreferenceLocation.Global);
            PreferencesInternal.SetBool(PreferenceKeys.pbUniqueModeShortcuts, pbUniqueModeShortcuts, PreferenceLocation.Global);
            PreferencesInternal.SetBool(PreferenceKeys.pbIconGUI, pbIconGUI, PreferenceLocation.Global);
            PreferencesInternal.SetBool(PreferenceKeys.pbShiftOnlyTooltips, pbShiftOnlyTooltips, PreferenceLocation.Global);
            PreferencesInternal.SetBool(PreferenceKeys.pbMeshesAreAssets, pbMeshesAreAssets);
            PreferencesInternal.SetBool(PreferenceKeys.pbEnableExperimental, pbEnableExperimental, PreferenceLocation.Global);
            PreferencesInternal.SetBool(PreferenceKeys.pbShowMissingLightmapUvWarning, showMissingLightmapUvWarning, PreferenceLocation.Global);
            PreferencesInternal.SetBool(PreferenceKeys.pbShowPreselectionHighlight, pbShowPreselectionHighlight, PreferenceLocation.Global);
            PreferencesInternal.SetFloat(PreferenceKeys.pbUVGridSnapValue, pbUVGridSnapValue, PreferenceLocation.Global);

            ProBuilderEditor.ReloadSettings();
            SceneView.RepaintAll();
        }

#pragma warning restore 618
#pragma warning restore 612
    }
}

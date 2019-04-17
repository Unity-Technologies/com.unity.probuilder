#if UNITY_2019_1_OR_NEWER
#define SHORTCUT_MANAGER
#endif

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEditor.SettingsManagement;

namespace UnityEditor.ProBuilder
{
    static class PreferencesUpdater
    {
        static Pref<SemVer> s_PreferencesVersion = new Pref<SemVer>("preferences.version", new SemVer(), SettingsScope.Project);
        static readonly SemVer k_ProBuilder_4_0_0 = new SemVer(4, 0, 0, 16, "preview");

        /// <summary>
        /// Set the editor pref version, and check if any preferences need to be updated or reset.
        /// </summary>
        public static void CheckEditorPrefsVersion()
        {
            // this exists to force update preferences when updating packages
            var stored = s_PreferencesVersion.value;
            var current = Version.currentInfo;

            if (!stored.Equals(current))
            {
                s_PreferencesVersion.SetValue(current);

                if (stored < k_ProBuilder_4_0_0)
                {
                    RecoverOldPreferences();
                    DeleteObsoletePreferences();
                }
            }
        }

        struct FormerPreferenceKeyMap
        {
            public string oldKey;
            public string newKey;
            public System.Type type;
            public SettingsScope scope;

            public FormerPreferenceKeyMap(string oldKey, string newKey, System.Type type, SettingsScope scope)
            {
                this.oldKey = oldKey;
                this.newKey = newKey;
                this.type = type;
                this.scope = scope;
            }
        }

        // for upgrading from pre-4.0.0 preferences this keymap is used. for renaming post 4.0.0 settings, use
        // [FormerlySavedAs] attribute (not yet created).
        static readonly FormerPreferenceKeyMap[] s_FormerPreferenceKeyMap = new FormerPreferenceKeyMap[]
        {
#pragma warning disable 612, 618
            new FormerPreferenceKeyMap("", "about.identifier", typeof(UnityEngine.ProBuilder.SemVer), SettingsScope.Project),
            new FormerPreferenceKeyMap(PreferenceKeys.pbUseUnityColors, "graphics.handlesUseUnityColors", typeof(System.Boolean), SettingsScope.User),
            new FormerPreferenceKeyMap(PreferenceKeys.pbSelectedFaceDither, "graphics.ditherFaceHandles", typeof(System.Boolean), SettingsScope.User),
            new FormerPreferenceKeyMap(PreferenceKeys.pbSelectedFaceColor, "graphics.userSelectedFaceColor", typeof(UnityEngine.Color), SettingsScope.User),
            new FormerPreferenceKeyMap(PreferenceKeys.pbWireframeColor, "graphics.userWireframeColor", typeof(UnityEngine.Color), SettingsScope.User),
            new FormerPreferenceKeyMap(PreferenceKeys.pbUnselectedEdgeColor, "graphics.userUnselectedEdgeColor", typeof(UnityEngine.Color), SettingsScope.User),
            new FormerPreferenceKeyMap(PreferenceKeys.pbSelectedEdgeColor, "graphics.userSelectedEdgeColor", typeof(UnityEngine.Color), SettingsScope.User),
            new FormerPreferenceKeyMap(PreferenceKeys.pbUnselectedVertexColor, "graphics.userUnselectedVertexColor", typeof(UnityEngine.Color), SettingsScope.User),
            new FormerPreferenceKeyMap(PreferenceKeys.pbSelectedVertexColor, "graphics.userSelectedVertexColor", typeof(UnityEngine.Color), SettingsScope.User),
            new FormerPreferenceKeyMap(PreferenceKeys.pbPreselectionColor, "graphics.userPreselectionColor", typeof(UnityEngine.Color), SettingsScope.User),
            new FormerPreferenceKeyMap(PreferenceKeys.pbWireframeSize, "graphics.wireframeLineSize", typeof(System.Single), SettingsScope.User),
            new FormerPreferenceKeyMap(PreferenceKeys.pbLineHandleSize, "graphics.edgeLineSize", typeof(System.Single), SettingsScope.User),
            new FormerPreferenceKeyMap(PreferenceKeys.pbVertexHandleSize, "graphics.vertexPointSize", typeof(System.Single), SettingsScope.User),
            new FormerPreferenceKeyMap(PreferenceKeys.pbShiftOnlyTooltips, "editor.shiftOnlyTooltips", typeof(System.Boolean), SettingsScope.User),
            new FormerPreferenceKeyMap(PreferenceKeys.pbShowEditorNotifications, "editor.showEditorNotifications", typeof(System.Boolean), SettingsScope.Project),
            new FormerPreferenceKeyMap(PreferenceKeys.pbDefaultStaticFlags, "mesh.defaultStaticEditorFlags", typeof(UnityEditor.StaticEditorFlags), SettingsScope.Project),
            new FormerPreferenceKeyMap(PreferenceKeys.pbDefaultMaterial, "mesh.userMaterial", typeof(UnityEngine.Material), SettingsScope.Project),
            new FormerPreferenceKeyMap(PreferenceKeys.pbForceConvex, "mesh.meshColliderIsConvex", typeof(System.Boolean), SettingsScope.Project),
            new FormerPreferenceKeyMap("", "mesh.newShapePivotLocation", typeof(UnityEngine.ProBuilder.PivotLocation), SettingsScope.Project),
            new FormerPreferenceKeyMap("", "mesh.newShapesSnapToGrid", typeof(System.Boolean), SettingsScope.Project),
            new FormerPreferenceKeyMap(PreferenceKeys.pbShadowCastingMode, "mesh.shadowCastingMode", typeof(UnityEngine.Rendering.ShadowCastingMode), SettingsScope.Project),
            new FormerPreferenceKeyMap(PreferenceKeys.pbDefaultCollider, "mesh.newShapeColliderType", typeof(UnityEngine.ProBuilder.ColliderType), SettingsScope.Project),
            new FormerPreferenceKeyMap(PreferenceKeys.pbEnableExperimental, "experimental.featuresEnabled", typeof(System.Boolean), SettingsScope.User),
            new FormerPreferenceKeyMap(PreferenceKeys.pbMeshesAreAssets, "experimental.meshesAreAssets", typeof(System.Boolean), SettingsScope.Project),
            new FormerPreferenceKeyMap("", "entity.detailVisible", typeof(System.Boolean), SettingsScope.Project),
            new FormerPreferenceKeyMap("", "entity.moverVisible", typeof(System.Boolean), SettingsScope.Project),
            new FormerPreferenceKeyMap("", "entity.colliderVisible", typeof(System.Boolean), SettingsScope.Project),
            new FormerPreferenceKeyMap("", "entity.triggerVisible", typeof(System.Boolean), SettingsScope.Project),
            new FormerPreferenceKeyMap("", "lightmapping.autoUnwrapLightmapUV", typeof(System.Boolean), SettingsScope.Project),
            new FormerPreferenceKeyMap(PreferenceKeys.pbShowMissingLightmapUvWarning, "lightmapping.showMissingLightmapWarning", typeof(System.Boolean), SettingsScope.User),
            new FormerPreferenceKeyMap("", "lightmapping.defaultLightmapUnwrapParameters", typeof(UnityEngine.ProBuilder.UnwrapParameters), SettingsScope.Project),
            new FormerPreferenceKeyMap("", "lightmapping.giWorkflowMode", typeof(UnityEditor.Lightmapping.GIWorkflowMode), SettingsScope.User),
            new FormerPreferenceKeyMap("pb_Log::m_LogLevel", "log.level", typeof(UnityEngine.ProBuilder.LogLevel), SettingsScope.Project),
            new FormerPreferenceKeyMap("pb_Log::m_Output", "log.output", typeof(UnityEngine.ProBuilder.LogOutput), SettingsScope.Project),
            new FormerPreferenceKeyMap("pb_Log::m_LogFilePath", "log.path", typeof(System.String), SettingsScope.Project),
            new FormerPreferenceKeyMap("", "editor.materialPalettePath", typeof(System.String), SettingsScope.Project),
            new FormerPreferenceKeyMap("", "preferences.version", typeof(UnityEngine.ProBuilder.SemVer), SettingsScope.Project),
            new FormerPreferenceKeyMap(PreferenceKeys.pbShowSceneInfo, "editor.showSceneInfo", typeof(System.Boolean), SettingsScope.Project),
            new FormerPreferenceKeyMap(PreferenceKeys.pbIconGUI, "editor.toolbarIconGUI", typeof(System.Boolean), SettingsScope.Project),
            new FormerPreferenceKeyMap(PreferenceKeys.pbUniqueModeShortcuts, "editor.uniqueModeShortcuts", typeof(System.Boolean), SettingsScope.User),
            new FormerPreferenceKeyMap("", "editor.allowNonManifoldActions", typeof(System.Boolean), SettingsScope.User),
            new FormerPreferenceKeyMap(PreferenceKeys.pbToolbarLocation, "editor.sceneToolbarLocation", typeof(UnityEditor.ProBuilder.SceneToolbarLocation), SettingsScope.User),
            new FormerPreferenceKeyMap("", "UnityEngine.ProBuilder.ProBuilderEditor-isUtilityWindow", typeof(System.Boolean), SettingsScope.Project),
#if !SHORTCUT_MANAGER
            new FormerPreferenceKeyMap(PreferenceKeys.pbDefaultShortcuts, "editor.sceneViewShortcuts", typeof(UnityEngine.ProBuilder.Shortcut[]), SettingsScope.Project),
#endif
            new FormerPreferenceKeyMap(PreferenceKeys.pbShowPreselectionHighlight, "editor.showPreselectionHighlight", typeof(System.Boolean), SettingsScope.User),
            new FormerPreferenceKeyMap(PreferenceKeys.pbCloseShapeWindow, "editor.closeWindowAfterShapeCreation", typeof(System.Boolean), SettingsScope.Project),
            new FormerPreferenceKeyMap("", "shape.torusDefinesInnerOuter", typeof(System.Boolean), SettingsScope.User),
            new FormerPreferenceKeyMap("pb_SmoothingGroupEditor::m_ShowPreview", "smoothing.showPreview", typeof(System.Boolean), SettingsScope.Project),
            new FormerPreferenceKeyMap("pb_SmoothingGroupEditor::m_DrawNormals", "smoothing.showNormals", typeof(System.Boolean), SettingsScope.Project),
            new FormerPreferenceKeyMap("", "smoothing.showHelp", typeof(System.Boolean), SettingsScope.Project),
            new FormerPreferenceKeyMap("pb_SmoothingGroupEditor::m_NormalsSize", "smoothing.NormalsSize", typeof(System.Single), SettingsScope.Project),
            new FormerPreferenceKeyMap("pb_SmoothingGroupEditor::m_PreviewOpacity", "smoothing.PreviewOpacity", typeof(System.Single), SettingsScope.Project),
            new FormerPreferenceKeyMap("pb_SmoothingGroupEditor::m_PreviewDither", "smoothing.previewDither", typeof(System.Boolean), SettingsScope.Project),
            new FormerPreferenceKeyMap("", "smoothing.showSettings", typeof(System.Boolean), SettingsScope.Project),
            new FormerPreferenceKeyMap(PreferenceKeys.pbStripProBuilderOnBuild, "editor.stripProBuilderScriptsOnBuild", typeof(System.Boolean), SettingsScope.Project),
            new FormerPreferenceKeyMap(PreferenceKeys.pbUVGridSnapValue, "uv.uvEditorGridSnapIncrement", typeof(System.Single), SettingsScope.Project),
            new FormerPreferenceKeyMap("", "VertexColorPalette.previousColorPalette", typeof(System.String), SettingsScope.Project),
#pragma warning restore 612, 618
        };

        static void RecoverOldPreferences()
        {
            int success = 0;

            foreach (var map in s_FormerPreferenceKeyMap)
            {
                object val;
                MethodInfo set = typeof(Settings).GetMethod("Set", BindingFlags.Static | BindingFlags.Public);

#pragma warning disable 618
                if (!string.IsNullOrEmpty(map.oldKey) && PreferencesInternal.TryGetValue(map.oldKey, map.type, out val))
                {
                    try
                    {
                        MethodInfo genericSet = set.MakeGenericMethod(map.type);

                        genericSet.Invoke(null, new object[]
                        {
                            map.newKey,
                            val,
                            map.scope
                        });

                        success++;
                    }
                    catch
                    {
                    }
                }
#pragma warning restore 618
            }

#if PB_DEBUG
            Debug.Log("ProBuilder successfully recovered " + success + " settings.");
#endif

            ProBuilderSettings.Save();
        }

        static void DeleteObsoletePreferences()
        {
#pragma warning disable 612, 618
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
            PreferencesInternal.DeleteKey(PreferenceKeys.pbDragCheckLimit);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbForceVertexPivot);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbForceGridPivot);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbManifoldEdgeExtrusion);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbPerimeterEdgeBridgeOnly);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbPBOSelectionOnly);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbCloseShapeWindow);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbUVEditorFloating);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbUVMaterialPreview);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbShowSceneToolbar);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbNormalizeUVsOnPlanarProjection);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbStripProBuilderOnBuild);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbDisableAutoUV2Generation);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbShowSceneInfo);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbEnableBackfaceSelection);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbVertexPaletteDockable);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbExtrudeAsGroup);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbUniqueModeShortcuts);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbMaterialEditorFloating);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbShapeWindowFloating);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbIconGUI);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbShiftOnlyTooltips);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbDrawAxisLines);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbElementSelectIsHamFisted);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbCollapseVertexToFirst);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbMeshesAreAssets);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbDragSelectWholeElement);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbEnableExperimental);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbFillHoleSelectsEntirePath);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbDetachToNewObject);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbPreserveFaces);
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
            PreferencesInternal.DeleteKey(PreferenceKeys.pbManageLightmappingStaticFlag);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbShadowCastingMode);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbDefaultStaticFlags);
            PreferencesInternal.DeleteKey(PreferenceKeys.pbShowPreselectionHighlight);

            PreferencesInternal.DeleteKey("ProBuilder_AboutWindowIdentifier");
            PreferencesInternal.DeleteKey("pb_UVTemplate_imageSize");
            PreferencesInternal.DeleteKey("pb_UVTemplate_lineColor");
            PreferencesInternal.DeleteKey("pb_UVTemplate_backgroundColor");
            PreferencesInternal.DeleteKey("pb_UVTemplate_transparentBackground");
            PreferencesInternal.DeleteKey("pb_UVTemplate_hideGrid");
            PreferencesInternal.DeleteKey("pb_Log::m_LogFilePath");
            PreferencesInternal.DeleteKey("pb_Log::m_Output");
            PreferencesInternal.DeleteKey("pb_Log::m_LogLevel");
            PreferencesInternal.DeleteKey("pb_SmoothingGroupEditor::m_ShowPreview");
            PreferencesInternal.DeleteKey("pb_SmoothingGroupEditor::m_DrawNormals");
            PreferencesInternal.DeleteKey("pb_SmoothingGroupEditor::m_NormalsSize");
            PreferencesInternal.DeleteKey("pb_SmoothingGroupEditor::m_PreviewOpacity");
            PreferencesInternal.DeleteKey("pb_SmoothingGroupEditor::m_PreviewDither");

            PreferencesInternal.DeleteKey("pbDefaultExportFormat");
            PreferencesInternal.DeleteKey("pbExportRecursive");
            PreferencesInternal.DeleteKey("pbExportAsGroup");
            PreferencesInternal.DeleteKey("pbObjExportRightHanded");
            PreferencesInternal.DeleteKey("pbObjApplyTransform");
            PreferencesInternal.DeleteKey("pbObjExportCopyTextures");
            PreferencesInternal.DeleteKey("pbObjExportVertexColors");
            PreferencesInternal.DeleteKey("pbObjTextureOffsetScale");
            PreferencesInternal.DeleteKey("pbObjQuads");
            PreferencesInternal.DeleteKey("pbStlFormat");
            PreferencesInternal.DeleteKey("pbPlyExportIsRightHanded");
            PreferencesInternal.DeleteKey("pbPlyApplyTransform");
            PreferencesInternal.DeleteKey("pbPlyQuads");
            PreferencesInternal.DeleteKey("pbPlyNGons");
#pragma warning restore 612, 618
        }
    }
}

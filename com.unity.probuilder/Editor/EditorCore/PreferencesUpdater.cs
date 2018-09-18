using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	static class PreferencesUpdater
	{
		static Pref<SemVer> s_PreferencesVersion = new Pref<SemVer>("preferences.version", new SemVer(), Settings.Scope.Project);
		static readonly SemVer k_ProBuilder_4_0_0 = new SemVer(4,0,0);

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
					DeleteObsoletePreferences();
				}
			}
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

			PreferencesInternal.GetInt("pbDefaultExportFormat");
			PreferencesInternal.GetBool("pbExportRecursive");
			PreferencesInternal.GetBool("pbExportAsGroup");
			PreferencesInternal.GetBool("pbObjExportRightHanded");
			PreferencesInternal.GetBool("pbObjApplyTransform");
			PreferencesInternal.GetBool("pbObjExportCopyTextures");
			PreferencesInternal.GetBool("pbObjExportVertexColors");
			PreferencesInternal.GetBool("pbObjTextureOffsetScale");
			PreferencesInternal.GetBool("pbObjQuads");
			PreferencesInternal.GetInt("pbStlFormat");
			PreferencesInternal.GetBool("pbPlyExportIsRightHanded");
			PreferencesInternal.GetBool("pbPlyApplyTransform");
			PreferencesInternal.GetBool("pbPlyQuads");
			PreferencesInternal.GetBool("pbPlyNGons");
#pragma warning restore 612, 618
		}
	}
}

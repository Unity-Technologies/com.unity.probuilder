using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	static class PreferencesUpdater
	{
		static readonly SemVer k_ProBuilder_4_0_0 = new SemVer(4,0,0);

		/// <summary>
		/// Set the editor pref version, and check if any preferences need to be updated or reset.
		/// </summary>
		public static void CheckEditorPrefsVersion()
		{
			// this exists to force update preferences when updating packages
			var stored = new SemVer(PreferencesInternal.GetString(PreferenceKeys.pbEditorPrefVersion)).MajorMinorPatch;
			var current = Version.currentInfo.MajorMinorPatch;

			if (!stored.Equals(current))
			{
				PreferencesInternal.SetString(PreferenceKeys.pbEditorPrefVersion, current.ToString("M.m.p"), PreferenceLocation.Global);

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

#pragma warning restore 612, 618
		}
	}
}

using System;
using UnityEngine;

namespace UnityEngine.ProBuilder
{
    /// <summary>
    /// A collection of constant values the ProBuilder uses regularly.
    /// </summary>
    static class PreferenceKeys
    {
        /// <summary>
        /// The product name ("ProBuilder" or "ProBuilder Basic" depending on assembly definition).
        /// </summary>
        public const string pluginTitle = "ProBuilder";

        internal const float k_MaxPointDistanceFromControl = 20f;
        internal const char DEGREE_SYMBOL   = (char)176;
        internal const char CMD_SUPER       = '\u2318';
        internal const char CMD_SHIFT       = '\u21E7';
        internal const char CMD_OPTION      = '\u2325';
        internal const char CMD_ALT         = '\u2387';
        internal const char CMD_DELETE      = '\u232B';

        internal static readonly Color proBuilderBlue = new Color(0f, .682f, .937f, 1f);
        internal static readonly Color proBuilderLightGray = new Color(.35f, .35f, .35f, .4f);
        internal static readonly Color proBuilderDarkGray = new Color(.1f, .1f, .1f, .3f);

        /// <summary>
        /// The starting range for editor menu items.
        /// </summary>
        public const int menuEditor = 100;

        /// <summary>
        /// The starting range for selection menu items.
        /// </summary>
        public const int menuSelection = 200;

        /// <summary>
        /// Starting range for geometry menu actions.
        /// </summary>
        public const int menuGeometry = 200;

        /// <summary>
        /// Starting range for action menu actions.
        /// </summary>
        public const int menuActions = 300;

        /// <summary>
        /// Starting range for material color menu items.
        /// </summary>
        public const int menuMaterialColors = 400;

        /// <summary>
        /// Starting range for vertex color menu items.
        /// </summary>
        public const int menuVertexColors = 400;

        /// <summary>
        /// Starting range for repair menu items.
        /// </summary>
        public const int menuRepair = 600;

        /// <summary>
        /// Starting range for other misc. menu items that belong in the ProBuilder menu subtree.
        /// </summary>
        public const int menuMisc = 600;

        /// <summary>
        /// Starting range for export menu items.
        /// </summary>
        public const int menuExport = 800;

        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbDefaultEditLevel = "pbDefaultEditLevel";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbDefaultSelectionMode = "pbDefaultSelectionMode";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbHandleAlignment = "pbHandleAlignment";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbVertexColorTool = "pbVertexColorTool";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbToolbarLocation = "pbToolbarLocation";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbDefaultEntity = "pbDefaultEntity";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbExtrudeMethod = "pbExtrudeMethod";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbDefaultStaticFlags = "pbDefaultStaticFlags";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbSelectedFaceColor = "pbDefaultFaceColor";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbWireframeColor = "pbDefaultEdgeColor";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbUnselectedEdgeColor = "pbUnselectedEdgeColor";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbSelectedEdgeColor = "pbSelectedEdgeColor";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbSelectedVertexColor = "pbDefaultSelectedVertexColor";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbUnselectedVertexColor = "pbDefaultVertexColor";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbPreselectionColor = "pbPreselectionColor";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbDefaultOpenInDockableWindow = "pbDefaultOpenInDockableWindow";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbEditorPrefVersion = "pbEditorPrefVersion";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbEditorShortcutsVersion = "pbEditorShortcutsVersion";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbDefaultCollider = "pbDefaultCollider";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbForceConvex = "pbForceConvex";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbVertexColorPrefs = "pbVertexColorPrefs";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbShowEditorNotifications = "pbShowEditorNotifications";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbDragCheckLimit = "pbDragCheckLimit";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbForceVertexPivot = "pbForceVertexPivot";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbForceGridPivot = "pbForceGridPivot";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbManifoldEdgeExtrusion = "pbManifoldEdgeExtrusion";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbPerimeterEdgeBridgeOnly = "pbPerimeterEdgeBridgeOnly";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbPBOSelectionOnly = "pbPBOSelectionOnly";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbCloseShapeWindow = "pbCloseShapeWindow";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbUVEditorFloating = "pbUVEditorFloating";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbUVMaterialPreview = "pbUVMaterialPreview";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbShowSceneToolbar = "pbShowSceneToolbar";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbNormalizeUVsOnPlanarProjection = "pbNormalizeUVsOnPlanarProjection";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbStripProBuilderOnBuild = "pbStripProBuilderOnBuild";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbDisableAutoUV2Generation = "pbDisableAutoUV2Generation";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbShowSceneInfo = "pbShowSceneInfo";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbEnableBackfaceSelection = "pbEnableBackfaceSelection";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbVertexPaletteDockable = "pbVertexPaletteDockable";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbExtrudeAsGroup = "pbExtrudeAsGroup";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbUniqueModeShortcuts = "pbUniqueModeShortcuts";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbMaterialEditorFloating = "pbMaterialEditorFloating";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbShapeWindowFloating = "pbShapeWindowFloating";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbIconGUI = "pbIconGUI";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbShiftOnlyTooltips = "pbShiftOnlyTooltips";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbDrawAxisLines = "pbDrawAxisLines";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbCollapseVertexToFirst = "pbCollapseVertexToFirst";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbMeshesAreAssets = "pbMeshesAreAssets";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbElementSelectIsHamFisted = "pbElementSelectIsHamFisted";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbFillHoleSelectsEntirePath = "pbFillHoleSelectsEntirePath";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbDetachToNewObject = "pbDetachToNewObject";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbPreserveFaces = "pbPreserveFaces";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbDragSelectWholeElement = "pbDragSelectWholeElement";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbShowPreselectionHighlight = "pbShowPreselectionHighlight";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbRectSelectMode = "pbRectSelectMode";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbDragSelectMode = "pbDragSelectMode";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbShadowCastingMode = "pbShadowCastingMode";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbEnableExperimental = "pbEnableExperimental";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbCheckForProBuilderUpdates = "pbCheckForProBuilderUpdates";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbManageLightmappingStaticFlag = "pbManageLightmappingStaticFlag";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbShowMissingLightmapUvWarning = "pb_Lightmapping::showMissingLightmapUvWarning";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbSelectedFaceDither = "pbSelectedFaceDither";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbUseUnityColors = "pbUseUnityColors";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbVertexHandleSize = "pbVertexHandleSize";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbUVGridSnapValue = "pbUVGridSnapValue";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbUVWeldDistance = "pbUVWeldDistance";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbLineHandleSize = "pbLineHandleSize";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbWireframeSize = "pbWireframeSize";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbWeldDistance = "pbWeldDistance";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbExtrudeDistance = "pbExtrudeDistance";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbBevelAmount = "pbBevelAmount";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbEdgeSubdivisions = "pbEdgeSubdivisions";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbDefaultShortcuts = "pbDefaultShortcuts";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbDefaultMaterial = "pbDefaultMaterial";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbCurrentMaterialPalette = "pbCurrentMaterialPalette";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbGrowSelectionUsingAngle = "pbGrowSelectionUsingAngle";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbGrowSelectionAngle = "pbGrowSelectionAngle";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbGrowSelectionAngleIterative = "pbGrowSelectionAngleIterative";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbShowDetail = "pbShowDetail";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbShowOccluder = "pbShowOccluder";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbShowMover = "pbShowMover";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbShowCollider = "pbShowCollider";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbShowTrigger = "pbShowTrigger";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string pbShowNoDraw = "pbShowNoDraw";
        [Obsolete("Use Pref<T> or Settings class directly.")]
        internal const string defaultUnwrapParameters = "pbDefaultUnwrapParameters";
    }
}

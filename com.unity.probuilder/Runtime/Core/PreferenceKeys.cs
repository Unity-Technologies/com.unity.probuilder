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

		[Obsolete("Use pb_Material.Default")]
		public static Material DefaultMaterial
		{
			get { return BuiltinMaterials.defaultMaterial; }
		}

		internal const HideFlags k_EditorHideFlags = (HideFlags) (1 | 2 | 4 | 8);
		internal const float k_MaxPointDistanceFromControl = 20f;

		internal const char DEGREE_SYMBOL 	= (char)176;
		internal const char CMD_SUPER  		= '\u2318';
		internal const char CMD_SHIFT  		= '\u21E7';
		internal const char CMD_OPTION  	= '\u2325';
		internal const char CMD_ALT  		= '\u2387';
		internal const char CMD_DELETE  	= '\u232B';

		// Enum
		internal const string pbDefaultEditLevel = "pbDefaultEditLevel";
		internal const string pbDefaultSelectionMode = "pbDefaultSelectionMode";
		internal const string pbHandleAlignment = "pbHandleAlignment";
		internal const string pbVertexColorTool = "pbVertexColorTool";
		internal const string pbToolbarLocation = "pbToolbarLocation";
		internal const string pbDefaultEntity = "pbDefaultEntity";
		internal const string pbExtrudeMethod = "pbExtrudeMethod";
		internal const string pbDefaultStaticFlags = "pbDefaultStaticFlags";

		// Color
		internal const string pbSelectedFaceColor = "pbDefaultFaceColor";
		internal const string pbWireframeColor = "pbDefaultEdgeColor";
		internal const string pbUnselectedEdgeColor = "pbUnselectedEdgeColor";
		internal const string pbSelectedEdgeColor = "pbSelectedEdgeColor";
		internal const string pbSelectedVertexColor = "pbDefaultSelectedVertexColor";
		internal const string pbUnselectedVertexColor = "pbDefaultVertexColor";
		internal const string pbPreselectionColor = "pbPreselectionColor";

		// Bool
		internal const string pbDefaultOpenInDockableWindow = "pbDefaultOpenInDockableWindow";
		internal const string pbEditorPrefVersion = "pbEditorPrefVersion";
		internal const string pbEditorShortcutsVersion = "pbEditorShortcutsVersion";
		internal const string pbDefaultCollider = "pbDefaultCollider";
		internal const string pbForceConvex = "pbForceConvex";
		internal const string pbVertexColorPrefs = "pbVertexColorPrefs";
		internal const string pbShowEditorNotifications = "pbShowEditorNotifications";
		[Obsolete]
		internal const string pbDragCheckLimit = "pbDragCheckLimit";
		internal const string pbForceVertexPivot = "pbForceVertexPivot";
		internal const string pbForceGridPivot = "pbForceGridPivot";
		internal const string pbManifoldEdgeExtrusion = "pbManifoldEdgeExtrusion";
		internal const string pbPerimeterEdgeBridgeOnly = "pbPerimeterEdgeBridgeOnly";
		internal const string pbPBOSelectionOnly = "pbPBOSelectionOnly";
		internal const string pbCloseShapeWindow = "pbCloseShapeWindow";
		internal const string pbUVEditorFloating = "pbUVEditorFloating";
		internal const string pbUVMaterialPreview = "pbUVMaterialPreview";
		[Obsolete]
		internal const string pbShowSceneToolbar = "pbShowSceneToolbar";
		internal const string pbNormalizeUVsOnPlanarProjection = "pbNormalizeUVsOnPlanarProjection";
		internal const string pbStripProBuilderOnBuild = "pbStripProBuilderOnBuild";
		internal const string pbDisableAutoUV2Generation = "pbDisableAutoUV2Generation";
		internal const string pbShowSceneInfo = "pbShowSceneInfo";
		internal const string pbEnableBackfaceSelection = "pbEnableBackfaceSelection";
		internal const string pbVertexPaletteDockable = "pbVertexPaletteDockable";
		internal const string pbExtrudeAsGroup = "pbExtrudeAsGroup";
		/// <summary>
		/// Toggles the edit level and selection mode shortcuts between:
		/// - 'G' = Toggle edit level, 'J, K, L' Vert, Edge, Face
		/// - 'G, J, K, L' = Object, Vert, Edge, Face modes
		/// </summary>
		internal const string pbUniqueModeShortcuts = "pbUniqueModeShortcuts";
		internal const string pbMaterialEditorFloating = "pbMaterialEditorFloating";
		internal const string pbShapeWindowFloating = "pbShapeWindowFloating";
		internal const string pbIconGUI = "pbIconGUI";
		internal const string pbShiftOnlyTooltips = "pbShiftOnlyTooltips";
		[Obsolete]
		internal const string pbDrawAxisLines = "pbDrawAxisLines";
		internal const string pbCollapseVertexToFirst = "pbCollapseVertexToFirst";
		internal const string pbMeshesAreAssets = "pbMeshesAreAssets";
		[Obsolete]
		internal const string pbElementSelectIsHamFisted = "pbElementSelectIsHamFisted";
		internal const string pbFillHoleSelectsEntirePath = "pbFillHoleSelectsEntirePath";
		internal const string pbDetachToNewObject = "pbDetachToNewObject";
		[Obsolete("Use pb_MeshImporter::quads")]
		internal const string pbPreserveFaces = "pbPreserveFaces";
		/// When drag selecting faces or edges, does the entire element have to be encompassed?
		[Obsolete("Use pbRectSelectMode")]
		internal const string pbDragSelectWholeElement = "pbDragSelectWholeElement";
		internal const string pbShowPreselectionHighlight = "pbShowPreselectionHighlight";

		/// <summary>
		/// When drag selecting elements does the entire element need to be encompassed or just touched by the rect.
		/// </summary>
		internal const string pbRectSelectMode = "pbRectSelectMode";

		/// <summary>
		/// When shift + drag selecting elements, how is the selection modified?
		/// </summary>
		internal const string pbDragSelectMode = "pbDragSelectMode";

		/// If present sets the shadow casting mode on new ProBuilder objects.
		internal const string pbShadowCastingMode = "pbShadowCastingMode";

		/// Are experimental features enabled?
		internal const string pbEnableExperimental = "pbEnableExperimental";

		/// Automatically check for updates?
		internal const string pbCheckForProBuilderUpdates = "pbCheckForProBuilderUpdates";

		/// Enable ProBuilder to manage the lightmapping static flags.
		internal const string pbManageLightmappingStaticFlag = "pbManageLightmappingStaticFlag";
		internal const string pbShowMissingLightmapUvWarning = "pb_Lightmapping::showMissingLightmapUvWarning";
		internal const string pbSelectedFaceDither = "pbSelectedFaceDither";
		internal const string pbUseUnityColors = "pbUseUnityColors";

		// Float
		internal const string pbVertexHandleSize = "pbVertexHandleSize";
		internal const string pbUVGridSnapValue = "pbUVGridSnapValue";
		internal const string pbUVWeldDistance = "pbUVWeldDistance";
		internal const string pbLineHandleSize = "pbLineHandleSize";
		internal const string pbWireframeSize = "pbWireframeSize";

		/// The maximum allowed distance between vertices to weld.
		internal const string pbWeldDistance = "pbWeldDistance";

		internal const string pbExtrudeDistance = "pbExtrudeDistance";
		internal const string pbBevelAmount = "pbBevelAmount";

		// Int
		internal const string pbEdgeSubdivisions = "pbEdgeSubdivisions";

		// Misc
		internal const string pbDefaultShortcuts = "pbDefaultShortcuts";
		internal const string pbDefaultMaterial = "pbDefaultMaterial";
		internal const string pbCurrentMaterialPalette = "pbCurrentMaterialPalette";

		/// Grow using angle check?
		internal const string pbGrowSelectionUsingAngle = "pbGrowSelectionUsingAngle";

		/// The angle value
		internal const string pbGrowSelectionAngle = "pbGrowSelectionAngle";

		/// If true, only one step of outer edges will be added.
		internal const string pbGrowSelectionAngleIterative = "pbGrowSelectionAngleIterative";

		internal const string pbShowDetail = "pbShowDetail";
		internal const string pbShowOccluder = "pbShowOccluder";
		internal const string pbShowMover = "pbShowMover";
		internal const string pbShowCollider = "pbShowCollider";
		internal const string pbShowTrigger = "pbShowTrigger";
		internal const string pbShowNoDraw = "pbShowNoDraw";

		internal const string defaultUnwrapParameters = "pbDefaultUnwrapParameters";

		internal static readonly Rect RectZero = new Rect(0,0,0,0);

	 	internal static readonly Color proBuilderBlue = new Color(0f, .682f, .937f, 1f);
	 	internal static readonly Color proBuilderLightGray = new Color(.35f, .35f, .35f, .4f);
	 	internal static readonly Color proBuilderDarkGray = new Color(.1f, .1f, .1f, .3f);

		/// <summary>
		/// The starting range for about menu items.
		/// </summary>
		public const int menuAbout = 0;

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
	}
}

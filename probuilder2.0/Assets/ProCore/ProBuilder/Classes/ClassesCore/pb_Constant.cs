using UnityEngine;

namespace ProBuilder.Core
{
	/// <summary>
	/// A collection of constant values the ProBuilder uses regularly.
	/// </summary>
	public static class pb_Constant
	{
#if PROTOTYPE
		/// <summary>
		/// The product name ("ProBuilder" or "ProBuilder Basic" depending on assembly definition).
		/// </summary>
		public const string PRODUCT_NAME = "ProBuilder Basic";
#else
		/// <summary>
		/// The product name ("ProBuilder" or "ProBuilder Basic" depending on assembly definition).
		/// </summary>
		public const string PRODUCT_NAME = "ProBuilder";
#endif

		internal static readonly HideFlags EDITOR_OBJECT_HIDE_FLAGS = (HideFlags) (1 | 2 | 4 | 8);
		internal const float MAX_POINT_DISTANCE_FROM_CONTROL = 20f;

		static Material s_DefaultMaterial = null;
		static Material s_FacePickerMaterial;
		static Material s_VertexPickerMaterial;
		static Material s_EdgePickerMaterial;
		static Shader s_SelectionPickerShader = null;
		static Material s_UnityDefaultDiffuse = null;
		static Material s_UnlitVertexColorMaterial;

		/// <summary>
		/// Default ProBuilder material.
		/// </summary>
		public static Material DefaultMaterial
		{
			get
			{
				if(s_DefaultMaterial == null)
				{
					s_DefaultMaterial = (Material) Resources.Load("Materials/Default_Prototype", typeof(Material));

					if(s_DefaultMaterial == null)
						s_DefaultMaterial = UnityDefaultDiffuse;
				}

				return s_DefaultMaterial;
			}
		}

		/// <summary>
		/// Material used for face picking functions.
		/// </summary>
		internal static Material FacePickerMaterial
		{
			get
			{
				if(s_FacePickerMaterial == null)
				{
					var facePickerShader = Shader.Find("Hidden/ProBuilder/FacePicker");

					if(facePickerShader == null)
						pb_Log.Error("pb_FacePicker.shader not found! Re-import ProBuilder to fix.");

					if(s_FacePickerMaterial == null)
						s_FacePickerMaterial = new Material(facePickerShader);
					else
						s_FacePickerMaterial.shader = facePickerShader;
				}
				return s_FacePickerMaterial;
			}
		}

		/// <summary>
		/// Material used for vertex picking functions.
		/// </summary>
		internal static Material VertexPickerMaterial
		{
			get
			{
				if(s_VertexPickerMaterial == null)
				{
					s_VertexPickerMaterial = Resources.Load<Material>("Materials/VertexPicker");

					var vertexPickerShader = Shader.Find("Hidden/ProBuilder/VertexPicker");

					if(vertexPickerShader == null)
						pb_Log.Error("pb_VertexPicker.shader not found! Re-import ProBuilder to fix.");

					if(s_VertexPickerMaterial == null)
						s_VertexPickerMaterial = new Material(vertexPickerShader);
					else
						s_VertexPickerMaterial.shader = vertexPickerShader;
				}
				return s_VertexPickerMaterial;
			}
		}

		/// <summary>
		/// Material used for edge picking functions.
		/// </summary>
		internal static Material EdgePickerMaterial
		{
			get
			{
				if(s_EdgePickerMaterial == null)
				{
					s_EdgePickerMaterial = Resources.Load<Material>("Materials/EdgePicker");

					var edgePickerShader = Shader.Find("Hidden/ProBuilder/EdgePicker");

					if(edgePickerShader == null)
						pb_Log.Error("pb_EdgePicker.shader not found! Re-import ProBuilder to fix.");

					if(s_EdgePickerMaterial == null)
						s_EdgePickerMaterial = new Material(edgePickerShader);
					else
						s_EdgePickerMaterial.shader = edgePickerShader;
				}
				return s_EdgePickerMaterial;
			}
		}

		/// <summary>
		/// Shader used in selection picking functions.
		/// </summary>
		internal static Shader SelectionPickerShader
		{
			get
			{
				if(s_SelectionPickerShader == null)
					s_SelectionPickerShader = (Shader) Shader.Find("Hidden/ProBuilder/SelectionPicker");
				return s_SelectionPickerShader;
			}
		}

		/// <summary>
		/// The ProBuilder "Trigger" entity type material.
		/// </summary>
		internal static Material TriggerMaterial
		{
			get { return (Material) Resources.Load("Materials/Trigger", typeof(Material)); }
		}

		/// <summary>
		/// The ProBuilder "Collider" entity type material.
		/// </summary>
		internal static Material ColliderMaterial
		{
			get { return (Material) Resources.Load("Materials/Collider", typeof(Material)); }
		}

		/// <summary>
		/// The ProBuilder "NoDraw" material. Faces with this material are hidden when the game is played.
		/// </summary>
		internal static Material NoDrawMaterial
		{
			get { return (Material) Resources.Load("Materials/NoDraw", typeof(Material)); }
		}

		/// <summary>
		/// Default Unity diffuse material.
		/// </summary>
		internal static Material UnityDefaultDiffuse
		{
			get
			{
				if( s_UnityDefaultDiffuse == null )
				{
					GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
					s_UnityDefaultDiffuse = go.GetComponent<MeshRenderer>().sharedMaterial;
					GameObject.DestroyImmediate(go);
				}

				return s_UnityDefaultDiffuse;
			}
		}

		/// <summary>
		/// An unlit vertex color material.
		/// </summary>
		internal static Material UnlitVertexColor
		{
			get
			{
				if(s_UnlitVertexColorMaterial == null)
					s_UnlitVertexColorMaterial = (Material)Resources.Load("Materials/UnlitVertexColor", typeof(Material));

				return s_UnlitVertexColorMaterial;
			}
		}

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

		// Color
		internal const string pbDefaultFaceColor = "pbDefaultFaceColor";
		internal const string pbDefaultEdgeColor = "pbDefaultEdgeColor";
		internal const string pbDefaultSelectedVertexColor = "pbDefaultSelectedVertexColor";
		internal const string pbDefaultVertexColor = "pbDefaultVertexColor";

		// Bool
		internal const string pbDefaultOpenInDockableWindow = "pbDefaultOpenInDockableWindow";
		internal const string pbEditorPrefVersion = "pbEditorPrefVersion";
		internal const string pbEditorShortcutsVersion = "pbEditorShortcutsVersion";
		internal const string pbDefaultCollider = "pbDefaultCollider";
		internal const string pbForceConvex = "pbForceConvex";
		internal const string pbVertexColorPrefs = "pbVertexColorPrefs";
		internal const string pbShowEditorNotifications = "pbShowEditorNotifications";
		[System.Obsolete]
		internal const string pbDragCheckLimit = "pbDragCheckLimit";
		internal const string pbForceVertexPivot = "pbForceVertexPivot";
		internal const string pbForceGridPivot = "pbForceGridPivot";
		internal const string pbManifoldEdgeExtrusion = "pbManifoldEdgeExtrusion";
		internal const string pbPerimeterEdgeBridgeOnly = "pbPerimeterEdgeBridgeOnly";
		internal const string pbPBOSelectionOnly = "pbPBOSelectionOnly";
		internal const string pbCloseShapeWindow = "pbCloseShapeWindow";
		internal const string pbUVEditorFloating = "pbUVEditorFloating";
		/// <summary>
		/// Toggles the UV editor material preview
		/// </summary>
		internal const string pbUVMaterialPreview = "pbUVMaterialPreview";
		/// <summary>
		/// Turns on or off the SceneView toolbar.
		/// </summary>
		[System.Obsolete]
		internal const string pbShowSceneToolbar = "pbShowSceneToolbar";
		internal const string pbNormalizeUVsOnPlanarProjection = "pbNormalizeUVsOnPlanarProjection";

		internal const string pbStripProBuilderOnBuild = "pbStripProBuilderOnBuild";
		internal const string pbDisableAutoUV2Generation = "pbDisableAutoUV2Generation";
		internal const string pbShowSceneInfo = "pbShowSceneInfo";
		internal const string pbEnableBackfaceSelection = "pbEnableBackfaceSelection";
		internal const string pbVertexPaletteDockable = "pbVertexPaletteDockable";
		/// <summary>
		/// When extruding, if this is true all faces that share an edge will be extruded as a group.  If false, each face is extruded separately.
		/// </summary>
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
		[System.Obsolete]
		internal const string pbDrawAxisLines = "pbDrawAxisLines";
		internal const string pbCollapseVertexToFirst = "pbCollapseVertexToFirst";
		internal const string pbMeshesAreAssets = "pbMeshesAreAssets";
		internal const string pbElementSelectIsHamFisted = "pbElementSelectIsHamFisted";
		internal const string pbFillHoleSelectsEntirePath = "pbFillHoleSelectsEntirePath";
		internal const string pbDetachToNewObject = "pbDetachToNewObject";

		[System.Obsolete("Use pb_MeshImporter::quads")]
		internal const string pbPreserveFaces = "pbPreserveFaces";

		/// When drag selecting faces or edges, does the entire element have to be encompassed?
		[System.Obsolete("Use pbRectSelectMode")]
		internal const string pbDragSelectWholeElement = "pbDragSelectWholeElement";

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

		// Float
		internal const string pbVertexHandleSize = "pbVertexHandleSize";
		internal const string pbUVGridSnapValue = "pbUVGridSnapValue";
		internal const string pbUVWeldDistance = "pbUVWeldDistance";

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

		internal static readonly Rect RectZero = new Rect(0,0,0,0);

	 	internal static Color ProBuilderBlue = new Color(0f, .682f, .937f, 1f);
	 	internal static Color ProBuilderLightGray = new Color(.35f, .35f, .35f, .4f);
	 	internal static Color ProBuilderDarkGray = new Color(.1f, .1f, .1f, .3f);

		/// <summary>
		/// The starting range for about menu items.
		/// </summary>
		public const int MENU_ABOUT = 0;

		/// <summary>
		/// The starting range for editor menu items.
		/// </summary>
		public const int MENU_EDITOR = 100;

		/// <summary>
		/// The starting range for selection menu items.
		/// </summary>
		public const int MENU_SELECTION 		= 200;

		/// <summary>
		/// Starting range for geometry menu actions.
		/// </summary>
		public const int MENU_GEOMETRY 			= 200;

		/// <summary>
		/// Starting range for action menu actions.
		/// </summary>
		public const int MENU_ACTIONS 			= 300;

		/// <summary>
		/// Starting range for material color menu items.
		/// </summary>
		public const int MENU_MATERIAL_COLORS 	= 400;

		/// <summary>
		/// Starting range for vertex color menu items.
		/// </summary>
		public const int MENU_VERTEX_COLORS	 	= 400;

		/// <summary>
		/// Starting range for repair menu items.
		/// </summary>
		public const int MENU_REPAIR 			= 600;

		/// <summary>
		/// Starting range for other misc. menu items that belong in the ProBuilder menu subtree.
		/// </summary>
		public const int MENU_MISC 				= 600;

		/// <summary>
		/// Starting range for export menu items.
		/// </summary>
		public const int MENU_EXPORT			= 800;
	}
}

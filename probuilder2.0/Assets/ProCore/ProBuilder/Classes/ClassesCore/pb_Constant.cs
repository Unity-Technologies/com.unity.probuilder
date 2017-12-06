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

#if DEBUG
		public const string VersionInfo = "ProBuilder Development Build";
#endif

		internal static readonly HideFlags EDITOR_OBJECT_HIDE_FLAGS = (HideFlags) (1 | 2 | 4 | 8);
		internal const float MAX_POINT_DISTANCE_FROM_CONTROL = 20f;

		static Material m_DefaultMaterial = null;
		static Material m_FacePickerMaterial;
		static Material m_VertexPickerMaterial;
		static Shader m_SelectionPickerShader = null;
		static Material m_UnityDefaultDiffuse = null;
		static Material m_UnlitVertexColorMaterial;

		/// <summary>
		/// Default ProBuilder material.
		/// </summary>
		public static Material DefaultMaterial
		{
			get
			{
				if(m_DefaultMaterial == null)
				{
					m_DefaultMaterial = (Material) Resources.Load("Materials/Default_Prototype", typeof(Material));

					if(m_DefaultMaterial == null)
						m_DefaultMaterial = UnityDefaultDiffuse;
				}

				return m_DefaultMaterial;
			}
		}

		/// <summary>
		/// Material used for face picking functions.
		/// </summary>
		internal static Material FacePickerMaterial
		{
			get
			{
				if(m_FacePickerMaterial == null)
				{
					m_FacePickerMaterial = Resources.Load<Material>("Materials/FacePicker");

					if(m_FacePickerMaterial == null)
						m_FacePickerMaterial = new Material(Shader.Find("Hidden/ProBuilder/FacePicker"));
					else
						m_FacePickerMaterial.shader = Shader.Find("Hidden/ProBuilder/FacePicker");
				}
				return m_FacePickerMaterial;
			}
		}

		/// <summary>
		/// Material used for vertex picking functions.
		/// </summary>
		internal static Material VertexPickerMaterial
		{
			get
			{
				if(m_VertexPickerMaterial == null)
				{
					m_VertexPickerMaterial = Resources.Load<Material>("Materials/VertexPicker");

					if(m_VertexPickerMaterial == null)
						m_VertexPickerMaterial = new Material(Shader.Find("Hidden/ProBuilder/VertexPicker"));
					else
						m_VertexPickerMaterial.shader = Shader.Find("Hidden/ProBuilder/VertexPicker");
				}
				return m_VertexPickerMaterial;
			}
		}

		/// <summary>
		/// Shader used in selection picking functions.
		/// </summary>
		internal static Shader SelectionPickerShader
		{
			get
			{
				if(m_SelectionPickerShader == null)
					m_SelectionPickerShader = (Shader) Shader.Find("Hidden/ProBuilder/SelectionPicker");
				return m_SelectionPickerShader;
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
				if( m_UnityDefaultDiffuse == null )
				{
					GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
					m_UnityDefaultDiffuse = go.GetComponent<MeshRenderer>().sharedMaterial;
					GameObject.DestroyImmediate(go);
				}

				return m_UnityDefaultDiffuse;
			}
		}

		/// <summary>
		/// An unlit vertex color material.
		/// </summary>
		internal static Material UnlitVertexColor
		{
			get
			{
				if(m_UnlitVertexColorMaterial == null)
					m_UnlitVertexColorMaterial = (Material)Resources.Load("Materials/UnlitVertexColor", typeof(Material));

				return m_UnlitVertexColorMaterial;
			}
		}

		internal const char DEGREE_SYMBOL 	= (char)176;
		internal const char CMD_SUPER  		= '\u2318';
		internal const char CMD_SHIFT  		= '\u21E7';
		internal const char CMD_OPTION  	= '\u2325';
		internal const char CMD_ALT  		= '\u2387';
		internal const char CMD_DELETE  	= '\u232B';

		// Enum
		internal const string pbDefaultEditLevel 				= "pbDefaultEditLevel";
		internal const string pbDefaultSelectionMode 			= "pbDefaultSelectionMode";
		internal const string pbHandleAlignment 				= "pbHandleAlignment";
		internal const string pbVertexColorTool 				= "pbVertexColorTool";
		internal const string pbToolbarLocation 				= "pbToolbarLocation";
		internal const string pbDefaultEntity 				= "pbDefaultEntity";
		internal const string pbExtrudeMethod					= "pbExtrudeMethod";

		// Color
		internal const string pbDefaultFaceColor 				= "pbDefaultFaceColor";
		internal const string pbDefaultEdgeColor 				= "pbDefaultEdgeColor";
		internal const string pbDefaultSelectedVertexColor	= "pbDefaultSelectedVertexColor";
		internal const string pbDefaultVertexColor 			= "pbDefaultVertexColor";

		// Bool
		internal const string pbDefaultOpenInDockableWindow	= "pbDefaultOpenInDockableWindow";
		internal const string pbEditorPrefVersion 			= "pbEditorPrefVersion";
		internal const string pbEditorShortcutsVersion		= "pbEditorShortcutsVersion";
		internal const string pbDefaultCollider 				= "pbDefaultCollider";
		internal const string pbForceConvex 					= "pbForceConvex";
		internal const string pbVertexColorPrefs 				= "pbVertexColorPrefs";
		internal const string pbShowEditorNotifications 		= "pbShowEditorNotifications";
		internal const string pbDragCheckLimit 				= "pbDragCheckLimit";
		internal const string pbForceVertexPivot 				= "pbForceVertexPivot";
		internal const string pbForceGridPivot 				= "pbForceGridPivot";
		internal const string pbManifoldEdgeExtrusion 		= "pbManifoldEdgeExtrusion";
		internal const string pbPerimeterEdgeBridgeOnly 		= "pbPerimeterEdgeBridgeOnly";
		internal const string pbPBOSelectionOnly 				= "pbPBOSelectionOnly";
		internal const string pbCloseShapeWindow 				= "pbCloseShapeWindow";
		internal const string pbUVEditorFloating 				= "pbUVEditorFloating";
		internal const string pbUVMaterialPreview 			= "pbUVMaterialPreview";			///< Toggles the UV editor material preview
		internal const string pbShowSceneToolbar 				= "pbShowSceneToolbar";				///< Turns on or off the SceneView toolbar.
		internal const string pbNormalizeUVsOnPlanarProjection= "pbNormalizeUVsOnPlanarProjection";
		internal const string pbStripProBuilderOnBuild 		= "pbStripProBuilderOnBuild";
		internal const string pbDisableAutoUV2Generation 		= "pbDisableAutoUV2Generation";
		internal const string pbShowSceneInfo 				= "pbShowSceneInfo";
		internal const string pbEnableBackfaceSelection		= "pbEnableBackfaceSelection";
		internal const string pbVertexPaletteDockable			= "pbVertexPaletteDockable";
		internal const string pbExtrudeAsGroup				= "pbExtrudeAsGroup";				///< When extruding, if this is true all faces that share an edge will be extruded as a group.  If false, each face is extruded separately.
		internal const string pbUniqueModeShortcuts			= "pbUniqueModeShortcuts";
		internal const string pbMaterialEditorFloating 		= "pbMaterialEditorFloating";
		internal const string pbShapeWindowFloating	 		= "pbShapeWindowFloating";
		internal const string pbIconGUI	 					= "pbIconGUI";
		internal const string pbShiftOnlyTooltips	 			= "pbShiftOnlyTooltips";
		internal const string pbDrawAxisLines					= "pbDrawAxisLines";
		internal const string pbCollapseVertexToFirst			= "pbCollapseVertexToFirst";
		internal const string pbMeshesAreAssets				= "pbMeshesAreAssets";
		internal const string pbElementSelectIsHamFisted		= "pbElementSelectIsHamFisted";
		internal const string pbFillHoleSelectsEntirePath		= "pbFillHoleSelectsEntirePath";
		internal const string pbDetachToNewObject				= "pbDetachToNewObject";
		[System.Obsolete("Use pb_MeshImporter::quads")]
		internal const string pbPreserveFaces					= "pbPreserveFaces";
		internal const string pbDragSelectWholeElement		= "pbDragSelectWholeElement";		///< When drag selecting faces or edges, does the entire element have to be encompassed?
		internal const string pbDragSelectMode				= "pbDragSelectMode";				///< When shift + drag selecting elements, how is the selection modified?
		internal const string pbShadowCastingMode				= "pbShadowCastingMode";			///< If present sets the shadow casting mode on new ProBuilder objects.
		internal const string pbEnableExperimental			= "pbEnableExperimental";			///< Are experimental features enabled?
		internal const string pbCheckForProBuilderUpdates		= "pbCheckForProBuilderUpdates";	///< Automatically check for updates?
		internal const string pbManageLightmappingStaticFlag	= "pbManageLightmappingStaticFlag";	///< Enable ProBuilder to manage the lightmapping static flags.

		// Float
		internal const string pbVertexHandleSize 				= "pbVertexHandleSize";
		internal const string pbUVGridSnapValue				= "pbUVGridSnapValue";
		internal const string pbUVWeldDistance				= "pbUVWeldDistance";
		internal const string pbWeldDistance 					= "pbWeldDistance";					///< The maximum allowed distance between vertices to weld.
		internal const string pbExtrudeDistance 				= "pbExtrudeDistance";
		internal const string pbBevelAmount 					= "pbBevelAmount";

		// Int
		internal const string pbEdgeSubdivisions				= "pbEdgeSubdivisions";

		// Misc
		internal const string pbDefaultShortcuts 				= "pbDefaultShortcuts";
		internal const string pbDefaultMaterial 				= "pbDefaultMaterial";
		internal const string pbCurrentMaterialPalette		= "pbCurrentMaterialPalette";

		// usablility settings (not preferences, just things that need to be saved)
		internal const string pbGrowSelectionUsingAngle 		= "pbGrowSelectionUsingAngle";		///< Grow using angle check?
		internal const string pbGrowSelectionAngle 	 		= "pbGrowSelectionAngle";			///< The angle value
		internal const string pbGrowSelectionAngleIterative	= "pbGrowSelectionAngleIterative";	///< If true, only one step of outer edges will be added.

		internal const string pbShowDetail					= "pbShowDetail";
		internal const string pbShowOccluder					= "pbShowOccluder";
		internal const string pbShowMover						= "pbShowMover";
		internal const string pbShowCollider					= "pbShowCollider";
		internal const string pbShowTrigger					= "pbShowTrigger";
		internal const string pbShowNoDraw					= "pbShowNoDraw";

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

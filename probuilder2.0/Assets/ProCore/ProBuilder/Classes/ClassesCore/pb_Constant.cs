
using UnityEngine;

public static class pb_Constant
{
	public static Material DefaultMaterial { get{ return (Material)Resources.Load("Materials/Default_Prototype", typeof(Material)); } }
	public static Material NoDrawMaterial { get{ return (Material)Resources.Load("Materials/NoDraw", typeof(Material)); } }

	// ProBuilder versions	- only store major/minor/update info - not beta / final status
	public const string PROBUILDER_2_3 = "2.3.0";

	public const string pbVersion = PROBUILDER_2_3;			///< The currently install ProBuilder version.

#if PROTOTYPE
	public const string PRODUCT_NAME = "Prototype";
#else
	public const string PRODUCT_NAME = "ProBuilder";
#endif

	public const char DEGREE_SYMBOL = (char)176;	///< Degree symbol char.

	// Enum
	public const string pbDefaultEditLevel 				= "pbDefaultEditLevel";
	public const string pbDefaultSelectionMode 			= "pbDefaultSelectionMode";
	public const string pbHandleAlignment 				= "pbHandleAlignment";

	// Color
	public const string pbDefaultFaceColor 				= "pbDefaultFaceColor";
	public const string pbDefaultSelectedVertexColor	= "pbDefaultSelectedVertexColor";
	public const string pbDefaultVertexColor 			= "pbDefaultVertexColor";

	// Bool
	public const string pbDefaultOpenInDockableWindow	= "pbDefaultOpenInDockableWindow";
	public const string pbEditorPrefVersion 			= "pbEditorPrefVersion";
	public const string pbDefaultCollider 				= "pbDefaultCollider";
	public const string pbForceConvex 					= "pbForceConvex";
	public const string pbVertexColorPrefs 				= "pbVertexColorPrefs";
	public const string pbShowEditorNotifications 		= "pbShowEditorNotifications";
	public const string pbDragCheckLimit 				= "pbDragCheckLimit";
	public const string pbForceVertexPivot 				= "pbForceVertexPivot";
	public const string pbForceGridPivot 				= "pbForceGridPivot";
	public const string pbManifoldEdgeExtrusion 		= "pbManifoldEdgeExtrusion";
	public const string pbPerimeterEdgeBridgeOnly 		= "pbPerimeterEdgeBridgeOnly";
	public const string pbPBOSelectionOnly 				= "pbPBOSelectionOnly";
	public const string pbCloseShapeWindow 				= "pbCloseShapeWindow";
	public const string pbHideWireframe 				= "pbHideWireframe";
	public const string pbUVEditorFloating 				= "pbUVEditorFloating";
	public const string pbUVMaterialPreview 			= "pbUVMaterialPreview";			///< Toggles the UV editor material preview
	public const string pbShowSceneToolbar 				= "pbShowSceneToolbar";				///< Turns on or off the SceneView toolbar.
	public const string pbShowUVEditorTooltip			= "pbShowUVEditorTooltip";			///< Turns on or off the SceneView toolbar.
	public const string pbNormalizeUVsOnPlanarProjection= "pbNormalizeUVsOnPlanarProjection";
	public const string pbStripProBuilderOnBuild 		= "pbStripProBuilderOnBuild";

	// Float
	public const string pbVertexHandleSize 				= "pbVertexHandleSize";
	public const string pbUVGridSnapValue				= "pbUVGridSnapValue";

	// Misc
	public const string pbDefaultShortcuts 				= "pbDefaultShortcuts";
	public const string pbDefaultMaterial 				= "pbDefaultMaterial";

	public const string pbExtrudeDistance 				= "pbExtrudeDistance";
	public const string pbWeldDistance 					= "pbWeldDistance";					///< The maximum allowed distance between vertices to weld.

	// usablility settings (not preferences, just things that need to be saved)
	public const string pbGrowSelectionUsingAngle 		= "pbGrowSelectionUsingAngle";
	public const string pbGrowSelectionAngle 	 		= "pbGrowSelectionAngle";

	public const string pbShowDetail					= "pbShowDetail";
	public const string pbShowOccluder					= "pbShowOccluder";
	public const string pbShowMover						= "pbShowMover";
	public const string pbShowCollider					= "pbShowCollider";
	public const string pbShowTrigger					= "pbShowTrigger";
	public const string pbShowNoDraw					= "pbShowNoDraw";

	public static Rect RectZero = new Rect(0,0,0,0);
	
 	public static Color ProBuilderBlue = new Color(0f, .682f, .937f, 1f);
 	public static Color ProBuilderLightGray = new Color(.35f, .35f, .35f, .4f);
 	public static Color ProBuilderDarkGray = new Color(.1f, .1f, .1f, .3f);

	// First Tier
	public const int MENU_ABOUT = 0;
	public const int MENU_WINDOW = 100;
	public const int MENU_EDITOR = 200;
	public const int MENU_SELECTION = 300;
	public const int MENU_GEOMETRY = 400;
	public const int MENU_UV = 410;
	public const int MENU_ACTIONS = 610;
	public const int MENU_REPAIR = 620;
	public const int MENU_TOOLS = 630;
	public const int MENU_VERTEX_COLORS = 740;
	public const int MENU_MATERIAL_COLORS = 750;

	// Second Tier
	public const int MENU_GEOMETRY_FACE = 0;
	public const int MENU_GEOMETRY_EDGE = 20;
	public const int MENU_GEOMETRY_VERTEX = 40;
	public const int MENU_GEOMETRY_USEINFERRED = 80;
	public const int MENU_GEOMETRY_OBJECT = 100;


	public static Vector3[] VERTICES_CUBE = new Vector3[] {
		// bottom 4 verts
		new Vector3(-.5f, -.5f, .5f),		// 0	
		new Vector3(.5f, -.5f, .5f),		// 1
		new Vector3(.5f, -.5f, -.5f),		// 2
		new Vector3(-.5f, -.5f, -.5f),		// 3

		// top 4 verts
		new Vector3(-.5f, .5f, .5f),		// 4
		new Vector3(.5f, .5f, .5f),			// 5
		new Vector3(.5f, .5f, -.5f),		// 6
		new Vector3(-.5f, .5f, -.5f)		// 7
	};

	public static int[] TRIANGLES_CUBE = new int[] {
		0, 1, 4, 5, 1, 2, 5, 6, 2, 3, 6, 7, 3, 0, 7, 4, 4, 5, 7, 6, 3, 2, 0, 1
	};
}

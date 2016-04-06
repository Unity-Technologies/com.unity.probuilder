using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using System.Collections;
using System.Linq;

#if PB_DEBUG
using Parabox.Debug;
#endif

public class pb_Preferences
{
	private static bool prefsLoaded = false;

	static Color pbDefaultFaceColor;
	static Color pbDefaultEdgeColor;
	static Color pbDefaultSelectedVertexColor;
	static Color pbDefaultVertexColor;

	static bool defaultOpenInDockableWindow;
	static Material pbDefaultMaterial;
	static Vector2 settingsScroll = Vector2.zero;
	static bool pbShowEditorNotifications;
	static bool pbForceConvex = false;
	static bool pbDragCheckLimit = false;
	static bool pbForceVertexPivot = true;
	static bool pbForceGridPivot = true;
	static bool pbManifoldEdgeExtrusion;
	static bool pbPerimeterEdgeBridgeOnly;
	static bool pbPBOSelectionOnly;
	static bool pbCloseShapeWindow = false;
	static bool pbUVEditorFloating = true;
	// static bool pbShowSceneToolbar = true;
	static bool pbStripProBuilderOnBuild = true;
	static bool pbDisableAutoUV2Generation = false;
	static bool pbShowSceneInfo = false;
	static bool pbEnableBackfaceSelection = false;
	static bool pbUniqueModeShortcuts = false;
	static bool pbIconGUI = false;
	static bool pbShiftOnlyTooltips = false;
	static bool pbDrawAxisLines = true;
	
	static ColliderType defaultColliderType = ColliderType.BoxCollider;
	static SceneToolbarLocation pbToolbarLocation = SceneToolbarLocation.UpperCenter;
	static EntityType pbDefaultEntity = EntityType.Detail;

	static float pbUVGridSnapValue;
	static float pbVertexHandleSize;

	static pb_Shortcut[] defaultShortcuts;

	[PreferenceItem (pb_Constant.PRODUCT_NAME)]
	public static void PreferencesGUI () 
	{
		// Load the preferences
		if (!prefsLoaded) {
			LoadPrefs();
			prefsLoaded = true;
			OnWindowResize();
		}
		
		settingsScroll = EditorGUILayout.BeginScrollView(settingsScroll, GUILayout.MaxHeight(200));

		EditorGUI.BeginChangeCheck();

		/**
		 * GENERAL SETTINGS
		 */
		GUILayout.Label("General Settings", EditorStyles.boldLabel);

		pbStripProBuilderOnBuild = EditorGUILayout.Toggle(new GUIContent("Strip PB Scripts on Build", "If true, when building an executable all ProBuilder scripts will be stripped from your built product."), pbStripProBuilderOnBuild);
		pbDisableAutoUV2Generation = EditorGUILayout.Toggle(new GUIContent("Disable Auto UV2 Generation", "Disables automatic generation of UV2 channel.  If Unity is sluggish when working with large ProBuilder objects, disabling UV2 generation will improve performance.  Use `Actions/Generate UV2` or `Actions/Generate Scene UV2` to build lightmap UVs prior to baking."), pbDisableAutoUV2Generation);
		pbShowSceneInfo = EditorGUILayout.Toggle(new GUIContent("Show Scene Info", "Displays the selected object vertex and triangle counts in the scene view."), pbShowSceneInfo);
		pbShowEditorNotifications = EditorGUILayout.Toggle("Show Editor Notifications", pbShowEditorNotifications);
		pbEnableBackfaceSelection = EditorGUILayout.Toggle(new GUIContent("Enable Back-face Selection", "If enabled, you may select faces that have been culled by their back face."), pbEnableBackfaceSelection);

		/**
		 * TOOLBAR SETTINGS
		 */
		GUILayout.Label("Toolbar Settings", EditorStyles.boldLabel);

		pbIconGUI = EditorGUILayout.Toggle(new GUIContent("Use Icon GUI", "Toggles the ProBuilder window interface between text and icon versions."), pbIconGUI);
		pbShiftOnlyTooltips = EditorGUILayout.Toggle(new GUIContent("Shift Key Tooltips", "Tooltips will only show when the Shift key is held"), pbShiftOnlyTooltips);
		pbToolbarLocation = (SceneToolbarLocation) EditorGUILayout.EnumPopup("Toolbar Location", pbToolbarLocation);

		pbUniqueModeShortcuts = EditorGUILayout.Toggle(new GUIContent("Unique Mode Shortcuts", "When off, the G key toggles between Object and Element modes and H enumerates the element modes.  If on, G, H, J, and K are shortcuts to Object, Vertex, Edge, and Face modes respectively."), pbUniqueModeShortcuts);
		defaultOpenInDockableWindow = EditorGUILayout.Toggle("Open in Dockable Window", defaultOpenInDockableWindow);
	

		/**
		 * DEFAULT SETTINGS
		 */
		GUILayout.Label("Defaults", EditorStyles.boldLabel);

		pbDefaultMaterial = (Material) EditorGUILayout.ObjectField("Default Material", pbDefaultMaterial, typeof(Material), false);
		GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Default Entity");
			pbDefaultEntity = ((EntityType)EditorGUILayout.EnumPopup( (EntityType)pbDefaultEntity ));
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Default Collider");
			defaultColliderType = ((ColliderType)EditorGUILayout.EnumPopup( (ColliderType)defaultColliderType ));
		GUILayout.EndHorizontal();

		if((ColliderType)defaultColliderType == ColliderType.MeshCollider)
			pbForceConvex = EditorGUILayout.Toggle("Force Convex Mesh Collider", pbForceConvex);

		/**
		 * MISC. SETTINGS
		 */
		GUILayout.Label("Misc. Settings", EditorStyles.boldLabel);

		pbDragCheckLimit = EditorGUILayout.Toggle(new GUIContent("Limit Drag Check to Selection", "If true, when drag selecting faces, only currently selected pb-Objects will be tested for matching faces.  If false, all pb_Objects in the scene will be checked.  The latter may be slower in large scenes."), pbDragCheckLimit);
		pbPBOSelectionOnly = EditorGUILayout.Toggle(new GUIContent("Only PBO are Selectable", "If true, you will not be able to select non probuilder objects in Geometry and Texture mode"), pbPBOSelectionOnly);
		pbCloseShapeWindow = EditorGUILayout.Toggle(new GUIContent("Close shape window after building", "If true the shape window will close after hitting the build button"), pbCloseShapeWindow);
		pbDrawAxisLines = EditorGUILayout.Toggle(new GUIContent("Dimension Overlay Lines", "When the Dimensions Overlay is on, this toggle shows or hides the axis lines."), pbDrawAxisLines);

		GUILayout.Space(4);
		
		/**
		 * GEOMETRY EDITING SETTINGS
		 */
		GUILayout.Label("Geometry Editing Settings", EditorStyles.boldLabel);

		pbDefaultFaceColor = EditorGUILayout.ColorField("Selected Face Color", pbDefaultFaceColor);
		pbDefaultEdgeColor = EditorGUILayout.ColorField("Edge Wireframe Color", pbDefaultEdgeColor);
		pbDefaultVertexColor = EditorGUILayout.ColorField("Vertex Color", pbDefaultVertexColor);
		pbDefaultSelectedVertexColor = EditorGUILayout.ColorField("Selected Vertex Color", pbDefaultSelectedVertexColor);
		pbVertexHandleSize = EditorGUILayout.Slider("Vertex Handle Size", pbVertexHandleSize, 0f, 1f);
		pbForceVertexPivot = EditorGUILayout.Toggle(new GUIContent("Force Pivot to Vertex Point", "If true, new objects will automatically have their pivot point set to a vertex instead of the center."), pbForceVertexPivot);
		pbForceGridPivot = EditorGUILayout.Toggle(new GUIContent("Force Pivot to Grid", "If true, newly instantiated pb_Objects will be snapped to the nearest point on grid.  If ProGrids is present, the snap value will be used, otherwise decimals are simply rounded to whole numbers."), pbForceGridPivot);
		pbManifoldEdgeExtrusion = EditorGUILayout.Toggle(new GUIContent("Manifold Edge Extrusion", "If false, only edges non-manifold edges may be extruded.  If true, you may extrude any edge you like (for those who like to live dangerously)."), pbManifoldEdgeExtrusion);
		pbPerimeterEdgeBridgeOnly = EditorGUILayout.Toggle(new GUIContent("Bridge Perimeter Edges Only", "If true, only edges on the perimeters of an object may be bridged.  If false, you may bridge any between any two edges you like."), pbPerimeterEdgeBridgeOnly);

		GUILayout.Space(4);

		/**
		 * UV EDITOR SETTINGS
		 */
		GUILayout.Label("UV Editing Settings", EditorStyles.boldLabel);
		pbUVGridSnapValue = EditorGUILayout.FloatField("UV Snap Increment", pbUVGridSnapValue);
		pbUVGridSnapValue = Mathf.Clamp(pbUVGridSnapValue, .015625f, 2f);
		pbUVEditorFloating = EditorGUILayout.Toggle(new GUIContent("Editor window floating", "If true UV   Editor window will open as a floating window"), pbUVEditorFloating);
		
		EditorGUILayout.EndScrollView();

		GUILayout.Space(4);

		GUILayout.Label("Shortcut Settings", EditorStyles.boldLabel);

		if(GUI.Button(resetRect, "Use defaults"))
			ResetToDefaults();

		ShortcutSelectPanel();
		ShortcutEditPanel();

		// Save the preferences
		if (EditorGUI.EndChangeCheck())
			SetPrefs();
	}

	public static void OnWindowResize()
	{
		int pad = 10, buttonWidth = 100, buttonHeight = 20;
		resetRect = new Rect(Screen.width-pad-buttonWidth, Screen.height-pad-buttonHeight, buttonWidth, buttonHeight);
	}

	public static void ResetToDefaults()
	{
		if(EditorUtility.DisplayDialog("Delete ProBuilder editor preferences?", "Are you sure you want to delete these?, this action cannot be undone.", "Yes", "No")) {
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultFaceColor);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultEdgeColor);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultOpenInDockableWindow);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultShortcuts);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultMaterial);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultCollider);
			EditorPrefs.DeleteKey(pb_Constant.pbForceConvex);
			EditorPrefs.DeleteKey(pb_Constant.pbShowEditorNotifications);
			EditorPrefs.DeleteKey(pb_Constant.pbDragCheckLimit);
			EditorPrefs.DeleteKey(pb_Constant.pbForceVertexPivot);
			EditorPrefs.DeleteKey(pb_Constant.pbForceGridPivot);
			EditorPrefs.DeleteKey(pb_Constant.pbManifoldEdgeExtrusion);
			EditorPrefs.DeleteKey(pb_Constant.pbPerimeterEdgeBridgeOnly);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultSelectedVertexColor);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultVertexColor);
			EditorPrefs.DeleteKey(pb_Constant.pbVertexHandleSize);
			EditorPrefs.DeleteKey(pb_Constant.pbPBOSelectionOnly);
			EditorPrefs.DeleteKey(pb_Constant.pbCloseShapeWindow);
			EditorPrefs.DeleteKey(pb_Constant.pbUVEditorFloating);
			EditorPrefs.DeleteKey(pb_Constant.pbShowSceneToolbar);
			EditorPrefs.DeleteKey(pb_Constant.pbUVGridSnapValue);
			EditorPrefs.DeleteKey(pb_Constant.pbStripProBuilderOnBuild);
			EditorPrefs.DeleteKey(pb_Constant.pbDisableAutoUV2Generation);
			EditorPrefs.DeleteKey(pb_Constant.pbShowSceneInfo);
			EditorPrefs.DeleteKey(pb_Constant.pbEnableBackfaceSelection);
			EditorPrefs.DeleteKey(pb_Constant.pbToolbarLocation);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultEntity);
			EditorPrefs.DeleteKey(pb_Constant.pbUniqueModeShortcuts);
			EditorPrefs.DeleteKey(pb_Constant.pbIconGUI);
			EditorPrefs.DeleteKey(pb_Constant.pbShiftOnlyTooltips);
			EditorPrefs.DeleteKey(pb_Constant.pbDrawAxisLines);
		}

		LoadPrefs();
	}

	static int shortcutIndex = 0;
	static Rect selectBox = new Rect(130, 253, 183, 142);

	static Rect resetRect = new Rect(0,0,0,0);
	static Vector2 shortcutScroll = Vector2.zero;
	static int CELL_HEIGHT = 20;

	static void ShortcutSelectPanel()
	{
		GUILayout.Space(4);
		GUI.contentColor = Color.white;
		GUI.Box(selectBox, "");

		GUIStyle labelStyle = GUIStyle.none;

		if(EditorGUIUtility.isProSkin)
			labelStyle.normal.textColor = new Color(1f, 1f, 1f, .8f);

		labelStyle.alignment = TextAnchor.MiddleLeft;
		labelStyle.contentOffset = new Vector2(4f, 0f);

		shortcutScroll = EditorGUILayout.BeginScrollView(shortcutScroll, false, true, GUILayout.MaxWidth(183), GUILayout.MaxHeight(156));

		for(int n = 1; n < defaultShortcuts.Length; n++)
		{
			if(n == shortcutIndex) 
			{
				GUI.backgroundColor = new Color(0.23f, .49f, .89f, 1f);
					labelStyle.normal.background = EditorGUIUtility.whiteTexture;
					Color oc = labelStyle.normal.textColor;
					labelStyle.normal.textColor = Color.white;
					GUILayout.Box(defaultShortcuts[n].action, labelStyle, GUILayout.MinHeight(CELL_HEIGHT), GUILayout.MaxHeight(CELL_HEIGHT));
					labelStyle.normal.background = null;
					labelStyle.normal.textColor = oc;
				GUI.backgroundColor = Color.white;
			}
			else
			{

				if(GUILayout.Button(defaultShortcuts[n].action, labelStyle, GUILayout.MinHeight(CELL_HEIGHT), GUILayout.MaxHeight(CELL_HEIGHT)))
				{
					shortcutIndex = n;
				}
			}
		}

		EditorGUILayout.EndScrollView();

	}

	static Rect keyRect 		= new Rect(324, 248, 168, 18);
	static Rect keyInputRect 	= new Rect(356, 248, 133, 18);

	static Rect descriptionTitleRect = new Rect(324, 300, 168, 200);
	static Rect descriptionRect = new Rect(324, 320, 168, 200);

	static Rect modifiersRect = new Rect(324, 270, 168, 18);
	static Rect modifiersInputRect = new Rect(383, 270, 107, 18);

	static void ShortcutEditPanel()
	{
		// descriptionTitleRect = EditorGUI.RectField(new Rect(240,150,200,50), descriptionTitleRect);
		GUI.Label(keyRect, "Key");
		KeyCode key = defaultShortcuts[shortcutIndex].key;
		key = (KeyCode) EditorGUI.EnumPopup(keyInputRect, key);
		defaultShortcuts[shortcutIndex].key = key;

		GUI.Label(modifiersRect, "Modifiers");

		// EnumMaskField returns a bit-mask where the flags correspond to the indices of the enum, not the enum values,
		// so this isn't technically correct.
		EventModifiers em = (EventModifiers) (((int)defaultShortcuts[shortcutIndex].eventModifiers) * 2);
		em = (EventModifiers)EditorGUI.EnumMaskField(modifiersInputRect, em);
		defaultShortcuts[shortcutIndex].eventModifiers = (EventModifiers) (((int)em) / 2);

		GUI.Label(descriptionTitleRect, "Description", EditorStyles.boldLabel);

		GUI.Label(descriptionRect, defaultShortcuts[shortcutIndex].description, EditorStyles.wordWrappedLabel);
	}

	static void LoadPrefs()
	{
		pbStripProBuilderOnBuild 			= pb_Preferences_Internal.GetBool(pb_Constant.pbStripProBuilderOnBuild);
		pbDisableAutoUV2Generation 			= pb_Preferences_Internal.GetBool(pb_Constant.pbDisableAutoUV2Generation);
		pbShowSceneInfo 					= pb_Preferences_Internal.GetBool(pb_Constant.pbShowSceneInfo);
		pbEnableBackfaceSelection 			= pb_Preferences_Internal.GetBool(pb_Constant.pbEnableBackfaceSelection);
		defaultOpenInDockableWindow 		= pb_Preferences_Internal.GetBool(pb_Constant.pbDefaultOpenInDockableWindow);
		pbDragCheckLimit 					= pb_Preferences_Internal.GetBool(pb_Constant.pbDragCheckLimit);
		pbForceConvex 						= pb_Preferences_Internal.GetBool(pb_Constant.pbForceConvex);
		pbForceGridPivot 					= pb_Preferences_Internal.GetBool(pb_Constant.pbForceGridPivot);
		pbForceVertexPivot 					= pb_Preferences_Internal.GetBool(pb_Constant.pbForceVertexPivot);
		pbManifoldEdgeExtrusion 			= pb_Preferences_Internal.GetBool(pb_Constant.pbManifoldEdgeExtrusion);
		pbPerimeterEdgeBridgeOnly 			= pb_Preferences_Internal.GetBool(pb_Constant.pbPerimeterEdgeBridgeOnly);
		pbPBOSelectionOnly 					= pb_Preferences_Internal.GetBool(pb_Constant.pbPBOSelectionOnly);
		pbCloseShapeWindow 					= pb_Preferences_Internal.GetBool(pb_Constant.pbCloseShapeWindow);		
		pbUVEditorFloating 					= pb_Preferences_Internal.GetBool(pb_Constant.pbUVEditorFloating);
		// pbShowSceneToolbar 					= pb_Preferences_Internal.GetBool(pb_Constant.pbShowSceneToolbar);
		pbShowEditorNotifications 			= pb_Preferences_Internal.GetBool(pb_Constant.pbShowEditorNotifications);
		pbUniqueModeShortcuts 				= pb_Preferences_Internal.GetBool(pb_Constant.pbUniqueModeShortcuts);
		pbIconGUI 							= pb_Preferences_Internal.GetBool(pb_Constant.pbIconGUI);
		pbShiftOnlyTooltips 				= pb_Preferences_Internal.GetBool(pb_Constant.pbShiftOnlyTooltips);
		pbDrawAxisLines 					= pb_Preferences_Internal.GetBool(pb_Constant.pbDrawAxisLines);

		pbDefaultFaceColor 					= pb_Preferences_Internal.GetColor( pb_Constant.pbDefaultFaceColor );
		pbDefaultEdgeColor 					= pb_Preferences_Internal.GetColor( pb_Constant.pbDefaultEdgeColor );
		pbDefaultSelectedVertexColor 		= pb_Preferences_Internal.GetColor( pb_Constant.pbDefaultSelectedVertexColor );
		pbDefaultVertexColor 				= pb_Preferences_Internal.GetColor( pb_Constant.pbDefaultVertexColor );

		pbUVGridSnapValue 					= pb_Preferences_Internal.GetFloat(pb_Constant.pbUVGridSnapValue);
		pbVertexHandleSize 					= pb_Preferences_Internal.GetFloat(pb_Constant.pbVertexHandleSize);

		defaultColliderType 				= pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider);
		pbToolbarLocation	 				= pb_Preferences_Internal.GetEnum<SceneToolbarLocation>(pb_Constant.pbToolbarLocation);
		pbDefaultEntity	 					= pb_Preferences_Internal.GetEnum<EntityType>(pb_Constant.pbDefaultEntity);

		pbDefaultMaterial 					= pb_Preferences_Internal.GetMaterial(pb_Constant.pbDefaultMaterial);

		defaultShortcuts 					= pb_Preferences_Internal.GetShortcuts().ToArray();

	}

	public static void SetPrefs()
	{
		EditorPrefs.SetBool  	(pb_Constant.pbStripProBuilderOnBuild, pbStripProBuilderOnBuild);
		EditorPrefs.SetBool  	(pb_Constant.pbDisableAutoUV2Generation, pbDisableAutoUV2Generation);
		EditorPrefs.SetBool  	(pb_Constant.pbShowSceneInfo, pbShowSceneInfo);
		EditorPrefs.SetBool  	(pb_Constant.pbEnableBackfaceSelection, pbEnableBackfaceSelection);

		EditorPrefs.SetInt		(pb_Constant.pbToolbarLocation, (int)pbToolbarLocation);
		EditorPrefs.SetInt		(pb_Constant.pbDefaultEntity, (int)pbDefaultEntity);

		EditorPrefs.SetString	(pb_Constant.pbDefaultFaceColor, pbDefaultFaceColor.ToString());
		EditorPrefs.SetString	(pb_Constant.pbDefaultEdgeColor, pbDefaultEdgeColor.ToString());
		EditorPrefs.SetString	(pb_Constant.pbDefaultSelectedVertexColor, pbDefaultSelectedVertexColor.ToString());
		EditorPrefs.SetString	(pb_Constant.pbDefaultVertexColor, pbDefaultVertexColor.ToString());
		EditorPrefs.SetBool  	(pb_Constant.pbDefaultOpenInDockableWindow, defaultOpenInDockableWindow);
		EditorPrefs.SetString	(pb_Constant.pbDefaultShortcuts, pb_Shortcut.ShortcutsToString(defaultShortcuts));

		string matPath = pbDefaultMaterial != null ? AssetDatabase.GetAssetPath(pbDefaultMaterial) : "";
		EditorPrefs.SetString	(pb_Constant.pbDefaultMaterial, matPath);
		
		EditorPrefs.SetInt 		(pb_Constant.pbDefaultCollider, (int)defaultColliderType);	
		EditorPrefs.SetBool  	(pb_Constant.pbShowEditorNotifications, pbShowEditorNotifications);
		EditorPrefs.SetBool  	(pb_Constant.pbForceConvex, pbForceConvex);
		EditorPrefs.SetBool  	(pb_Constant.pbDragCheckLimit, pbDragCheckLimit);
		EditorPrefs.SetBool  	(pb_Constant.pbForceVertexPivot, pbForceVertexPivot);
		EditorPrefs.SetBool  	(pb_Constant.pbForceGridPivot, pbForceGridPivot);
		EditorPrefs.SetBool		(pb_Constant.pbManifoldEdgeExtrusion, pbManifoldEdgeExtrusion);
		EditorPrefs.SetBool		(pb_Constant.pbPerimeterEdgeBridgeOnly, pbPerimeterEdgeBridgeOnly);
		EditorPrefs.SetBool		(pb_Constant.pbPBOSelectionOnly, pbPBOSelectionOnly);
		EditorPrefs.SetBool		(pb_Constant.pbCloseShapeWindow, pbCloseShapeWindow);
		EditorPrefs.SetBool		(pb_Constant.pbUVEditorFloating, pbUVEditorFloating);
		// EditorPrefs.SetBool		(pb_Constant.pbShowSceneToolbar, pbShowSceneToolbar);
		EditorPrefs.SetBool		(pb_Constant.pbUniqueModeShortcuts, pbUniqueModeShortcuts);
		EditorPrefs.SetBool		(pb_Constant.pbIconGUI, pbIconGUI);
		EditorPrefs.SetBool		(pb_Constant.pbShiftOnlyTooltips, pbShiftOnlyTooltips);
		EditorPrefs.SetBool		(pb_Constant.pbDrawAxisLines, pbDrawAxisLines);
		
		EditorPrefs.SetFloat	(pb_Constant.pbVertexHandleSize, pbVertexHandleSize);
		EditorPrefs.SetFloat 	(pb_Constant.pbUVGridSnapValue, pbUVGridSnapValue);

		if(pb_Editor.instance != null)
			pb_Editor.instance.OnEnable();

		SceneView.RepaintAll();
	}
}

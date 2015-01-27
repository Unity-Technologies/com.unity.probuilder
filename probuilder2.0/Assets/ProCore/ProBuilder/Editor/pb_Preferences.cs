using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

#if PB_DEBUG
using Parabox.Debug;
#endif

public class pb_Preferences
{
	private static bool prefsLoaded = false;

	static SelectMode pbDefaultSelectionMode;
	static Color pbDefaultFaceColor;
	static Color pbDefaultEdgeColor;
	static Color pbDefaultSelectedVertexColor;
	static Color pbDefaultVertexColor;
	static bool defaultOpenInDockableWindow;
	static Material _defaultMaterial;
	static Vector2 settingsScroll = Vector2.zero;
	static int defaultColliderType = 2;
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
	static bool pbShowSceneToolbar = true;
	static bool pbStripProBuilderOnBuild = true;
	static bool pbDisableAutoUV2Generation = false;

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

		/**
		 * GENERAL SETTINGS
		 */
		GUILayout.Label("General Settings", EditorStyles.boldLabel);

		pbStripProBuilderOnBuild = EditorGUILayout.Toggle(new GUIContent("Strip PB Scripts on Build", "If true, when building an executable all ProBuilder scripts will be stripped from your built product."), pbStripProBuilderOnBuild);
		pbDisableAutoUV2Generation = EditorGUILayout.Toggle(new GUIContent("Disable Auto UV2 Generation", "Disables automatic generation of UV2 channel.  If Unity is sluggish when working with large ProBuilder objects, disabling UV2 generation will improve performance.  Use `Actions/Generate UV2` or `Actions/Generate Scene UV2` to build lightmap UVs prior to baking."), pbDisableAutoUV2Generation);
		pbDefaultSelectionMode = (SelectMode)EditorGUILayout.EnumPopup("Default Selection Mode", pbDefaultSelectionMode);
		_defaultMaterial = (Material) EditorGUILayout.ObjectField("Default Material", _defaultMaterial, typeof(Material), false);
		defaultOpenInDockableWindow = EditorGUILayout.Toggle("Open in Dockable Window", defaultOpenInDockableWindow);
		GUILayout.BeginHorizontal();
			EditorGUILayout.PrefixLabel("Default Collider");
			defaultColliderType = (int)((ColliderType)EditorGUILayout.EnumPopup( (ColliderType)defaultColliderType ));
		GUILayout.EndHorizontal();
		if((ColliderType)defaultColliderType == ColliderType.MeshCollider)
			pbForceConvex = EditorGUILayout.Toggle("Force Convex Mesh Collider", pbForceConvex);
		pbShowEditorNotifications = EditorGUILayout.Toggle("Show Editor Notifications", pbShowEditorNotifications);
		pbDragCheckLimit = EditorGUILayout.Toggle(new GUIContent("Limit Drag Check to Selection", "If true, when drag selecting faces, only currently selected pb-Objects will be tested for matching faces.  If false, all pb_Objects in the scene will be checked.  The latter may be slower in large scenes."), pbDragCheckLimit);
		pbPBOSelectionOnly = EditorGUILayout.Toggle(new GUIContent("Only PBO are Selectable", "If true, you will not be able to select non probuilder objects in Geometry and Texture mode"), pbPBOSelectionOnly);
		pbCloseShapeWindow = EditorGUILayout.Toggle(new GUIContent("Close shape window after building", "If true the shape window will close after hitting the build button"), pbCloseShapeWindow);
		pbShowSceneToolbar = EditorGUILayout.Toggle(new GUIContent("Show Scene Toolbar", "Hide or show the SceneView mode toolbar."), pbShowSceneToolbar);

		GUILayout.Space(4);
		
		/**
		 * GEOMETRY EDITING SETTINGS
		 */
		GUILayout.Label("Geometry Editing Settings", EditorStyles.boldLabel);

		pbDefaultFaceColor = EditorGUILayout.ColorField("Selected Face Color", pbDefaultFaceColor);
		pbDefaultEdgeColor = EditorGUILayout.ColorField("Edge Wireframe Color", pbDefaultEdgeColor);
		pbDefaultVertexColor = EditorGUILayout.ColorField("Vertex Color", pbDefaultVertexColor);
		pbDefaultSelectedVertexColor = EditorGUILayout.ColorField("Selected Vertex Color", pbDefaultSelectedVertexColor);
		pbVertexHandleSize = EditorGUILayout.FloatField("Vertex Handle Size", pbVertexHandleSize);
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
		if (GUI.changed)
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
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultSelectionMode);
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
		}

		LoadPrefs();
	}

	static int shortcutIndex = 0;
	static Rect selectBox = new Rect(130, 253, 183, 142);

	static Rect resetRect = new Rect(0,0,0,0);
	static Vector2 shortcutScroll = Vector2.zero;
	static int CELL_HEIGHT = 20;
	// static int tmp = 0;
	static void ShortcutSelectPanel()
	{
		// tmp = EditorGUI.IntField(new Rect(400, 340, 80, 24), "", tmp);

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
			if(n == shortcutIndex) {
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
					shortcutIndex = n;
			}
		}

		EditorGUILayout.EndScrollView();

	}

	static Rect keyRect = new Rect(324, 240, 168, 18);
	static Rect keyInputRect = new Rect(356, 240, 133, 18);

	static Rect descriptionTitleRect = new Rect(324, 300, 168, 200);
	static Rect descriptionRect = new Rect(324, 320, 168, 200);

	static Rect modifiersRect = new Rect(324, 270, 168, 18);
	static Rect modifiersInputRect = new Rect(383, 270, 107, 18);

	static void ShortcutEditPanel()
	{
		// descriptionTitleRect = EditorGUI.RectField(new Rect(240,150,200,50), descriptionTitleRect);

		string keyString = defaultShortcuts[shortcutIndex].key.ToString();
	
		GUI.Label(keyRect, "Key");
		keyString = EditorGUI.TextField(keyInputRect, keyString);
		defaultShortcuts[shortcutIndex].key = pbUtil.ParseEnum(keyString, KeyCode.None);

		GUI.Label(modifiersRect, "Modifiers");
		defaultShortcuts[shortcutIndex].eventModifiers = 
			(EventModifiers)EditorGUI.EnumMaskField(modifiersInputRect, defaultShortcuts[shortcutIndex].eventModifiers);

		GUI.Label(descriptionTitleRect, "Description", EditorStyles.boldLabel);

		GUI.Label(descriptionRect, defaultShortcuts[shortcutIndex].description, EditorStyles.wordWrappedLabel);
	}

	static void LoadPrefs()
	{
		pbStripProBuilderOnBuild = pb_Preferences_Internal.GetBool(pb_Constant.pbStripProBuilderOnBuild);
		pbDisableAutoUV2Generation = pb_Preferences_Internal.GetBool(pb_Constant.pbDisableAutoUV2Generation);

		pbDefaultFaceColor = pb_Preferences_Internal.GetColor( pb_Constant.pbDefaultFaceColor );
		pbDefaultEdgeColor = pb_Preferences_Internal.GetColor( pb_Constant.pbDefaultEdgeColor );
		
		pbDefaultSelectedVertexColor = pb_Preferences_Internal.GetColor( pb_Constant.pbDefaultSelectedVertexColor );
		pbDefaultVertexColor = pb_Preferences_Internal.GetColor( pb_Constant.pbDefaultVertexColor );

		if(!EditorPrefs.HasKey( pb_Constant.pbDefaultOpenInDockableWindow))
			EditorPrefs.SetBool(pb_Constant.pbDefaultOpenInDockableWindow, true);

			
		defaultOpenInDockableWindow = EditorPrefs.GetBool(pb_Constant.pbDefaultOpenInDockableWindow);

		pbDefaultSelectionMode = pb_Preferences_Internal.GetEnum<SelectMode>(pb_Constant.pbDefaultSelectionMode);
		defaultColliderType = (int)pb_Preferences_Internal.GetEnum<ColliderType>(pb_Constant.pbDefaultCollider);

		pbUVGridSnapValue = pb_Preferences_Internal.GetFloat(pb_Constant.pbUVGridSnapValue);
		
		pbDragCheckLimit 	= pb_Preferences_Internal.GetBool(pb_Constant.pbDragCheckLimit);
		pbForceConvex 		= pb_Preferences_Internal.GetBool(pb_Constant.pbForceConvex);
		pbForceGridPivot 	= pb_Preferences_Internal.GetBool(pb_Constant.pbForceGridPivot);
		pbForceVertexPivot 	= pb_Preferences_Internal.GetBool(pb_Constant.pbForceVertexPivot);
		
		pbManifoldEdgeExtrusion = pb_Preferences_Internal.GetBool(pb_Constant.pbManifoldEdgeExtrusion);
		pbPerimeterEdgeBridgeOnly = pb_Preferences_Internal.GetBool(pb_Constant.pbPerimeterEdgeBridgeOnly);

		pbPBOSelectionOnly = pb_Preferences_Internal.GetBool(pb_Constant.pbPBOSelectionOnly);

		pbCloseShapeWindow = pb_Preferences_Internal.GetBool(pb_Constant.pbCloseShapeWindow);

		pbVertexHandleSize = pb_Preferences_Internal.GetFloat(pb_Constant.pbVertexHandleSize);

		pbUVEditorFloating = pb_Preferences_Internal.GetBool(pb_Constant.pbUVEditorFloating);

		pbShowSceneToolbar = pb_Preferences_Internal.GetBool(pb_Constant.pbShowSceneToolbar);

		_defaultMaterial = pb_Preferences_Internal.GetMaterial(pb_Constant.pbDefaultMaterial);

		defaultShortcuts = EditorPrefs.HasKey(pb_Constant.pbDefaultShortcuts) ? 
			pb_Shortcut.ParseShortcuts(EditorPrefs.GetString(pb_Constant.pbDefaultShortcuts)) : 
			pb_Shortcut.DefaultShortcuts();

		pbShowEditorNotifications = EditorPrefs.HasKey(pb_Constant.pbShowEditorNotifications) ?
			EditorPrefs.GetBool(pb_Constant.pbShowEditorNotifications) : true;
	}

	public static void SetPrefs()
	{
		EditorPrefs.SetBool  	(pb_Constant.pbStripProBuilderOnBuild, pbStripProBuilderOnBuild);
		EditorPrefs.SetBool  	(pb_Constant.pbDisableAutoUV2Generation, pbDisableAutoUV2Generation);

		EditorPrefs.SetInt		(pb_Constant.pbDefaultSelectionMode, (int)pbDefaultSelectionMode);
		EditorPrefs.SetString	(pb_Constant.pbDefaultFaceColor, pbDefaultFaceColor.ToString());
		EditorPrefs.SetString	(pb_Constant.pbDefaultEdgeColor, pbDefaultEdgeColor.ToString());
		EditorPrefs.SetString	(pb_Constant.pbDefaultSelectedVertexColor, pbDefaultSelectedVertexColor.ToString());
		EditorPrefs.SetString	(pb_Constant.pbDefaultVertexColor, pbDefaultVertexColor.ToString());
		EditorPrefs.SetBool  	(pb_Constant.pbDefaultOpenInDockableWindow, defaultOpenInDockableWindow);
		EditorPrefs.SetString	(pb_Constant.pbDefaultShortcuts, pb_Shortcut.ShortcutsToString(defaultShortcuts));

		string matPath = _defaultMaterial != null ? AssetDatabase.GetAssetPath(_defaultMaterial) : "";
		EditorPrefs.SetString	(pb_Constant.pbDefaultMaterial, matPath == "" ? _defaultMaterial.name : matPath);
		
		EditorPrefs.SetInt 		(pb_Constant.pbDefaultCollider, defaultColliderType);	
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
		EditorPrefs.SetBool		(pb_Constant.pbShowSceneToolbar, pbShowSceneToolbar);
		// pb_Editor.instance.LoadPrefs();
		
		EditorPrefs.SetFloat	(pb_Constant.pbVertexHandleSize, pbVertexHandleSize);
		EditorPrefs.SetFloat 	(pb_Constant.pbUVGridSnapValue, pbUVGridSnapValue);


		pb_Editor_Graphics.LoadColors();
	}
}
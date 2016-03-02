using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using ProBuilder2.Math;
using ProBuilder2.Interface;

#if PB_DEBUG
using Parabox.Debug;
#endif

namespace ProBuilder2.EditorCommon
{
public class pb_Editor : EditorWindow
{
	pb_ElementGraphics graphics { get { return pb_ElementGraphics.instance; } }

#region LOCAL MEMBERS && EDITOR PREFS

	// because editor prefs can change, or shortcuts may be added, certain EditorPrefs need to be force reloaded.
	// adding to this const will force update on updating packages.
	const int EDITOR_PREF_VERSION = 2080;
	const int WINDOW_WIDTH_FlOATING = 102;
	const int WINDOW_WIDTH_DOCKABLE = 108;

	// Toggles for Face, Vertex, and Edge mode.
	const int SELECT_MODE_LENGTH = 3;
	GUIContent[] EditModeIcons;
	Texture2D eye_on, eye_off;
	GUIStyle VertexTranslationInfoStyle;
	GUIStyle eye_style;

	public static pb_Editor instance { get { return _instance; } }
	private static pb_Editor _instance;

	/**
	 *	Subscribe to notifications of edit level changes.
	 */
	public static void AddOnEditLevelChangedListener( System.Action<int> listener )
	{
		onEditLevelChanged += listener;
	}

	public static void RemoveOnEditLevelChangedListener( System.Action<int> listener )
	{
		onEditLevelChanged -= listener;
	}

	public static System.Action<int> onEditLevelChanged;

	MethodInfo findNearestVertex;	///< Needs to be initialized from an instance, not a static class. Don't move to HandleUtility, you tried that already.

	public EditLevel editLevel { get; private set; }
	private EditLevel previousEditLevel;

	public SelectMode selectionMode { get; private set; }
	private SelectMode previousSelectMode;

	public HandleAlignment handleAlignment { get; private set; }

	#if !PROTOTYPE
	private HandleAlignment previousHandleAlignment;
	#endif

	pb_EditorToolbar iconGui = null;

	pb_Shortcut[] shortcuts;

	private bool vertexSelectionMask = true;	///< If true, in EditMode.ModeBased && SelectionMode.Vertex only vertices will be selected when dragging.
	public float drawNormals = 0f;
	public bool drawFaceNormals = false;
	private bool pref_showSceneInfo = false;
	private bool pref_backfaceSelect = false;

	private float pref_snapValue = .25f;
	private bool pref_snapAxisConstraints = true;
	private bool pref_snapEnabled = false;
	private bool prefs_iconGui = false;

	private bool pref_showToolbar = true;
	private SceneToolbarLocation pref_sceneToolbarLocation = SceneToolbarLocation.UpperCenter;

	private bool limitFaceDragCheckToSelection = true;
	private bool isFloatingWindow = false;
#endregion

#region INITIALIZATION AND ONDISABLE

	/**
	 * Open the pb_Editor window with whatever dockable status is preference-d.
	 */
	public static pb_Editor MenuOpenWindow()
	{
		pb_Editor editor = (pb_Editor)EditorWindow.GetWindow(typeof(pb_Editor), !pb_Preferences_Internal.GetBool(pb_Constant.pbDefaultOpenInDockableWindow), pb_Constant.PRODUCT_NAME, true);			// open as floating window
		// would be nice if editorwindow's showMode was exposed
		editor.isFloatingWindow = !pb_Preferences_Internal.GetBool(pb_Constant.pbDefaultOpenInDockableWindow);
		return editor;
	}

	public void OnEnable()
	{
		_instance = this;

		graphics.LoadPrefs( pb_Preferences_Internal.ToHashtable() );

		HookDelegates();

		// make sure load prefs is called first, because other methods depend on the preferences set here
		LoadPrefs();

		InitGUI();

		show_Detail 	= pb_Preferences_Internal.GetBool(pb_Constant.pbShowDetail);
		show_Mover 		= pb_Preferences_Internal.GetBool(pb_Constant.pbShowMover);
		show_Collider 	= pb_Preferences_Internal.GetBool(pb_Constant.pbShowCollider);
		show_Trigger 	= pb_Preferences_Internal.GetBool(pb_Constant.pbShowTrigger);

		// EditorUtility.UnloadUnusedAssets();
		ToggleEntityVisibility(EntityType.Detail, true);

		UpdateSelection(true);

		HideSelectedWireframe();

		findNearestVertex = typeof(HandleUtility).GetMethod("FindNearestVertex", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);

		if( onEditLevelChanged != null )
			onEditLevelChanged( (int) editLevel );
	}

	private void InitGUI()
	{
		if( prefs_iconGui )
		{
			iconGui = ScriptableObject.CreateInstance<pb_EditorToolbar>();
			iconGui.InitWindowProperties(this);
		}
		else
		{
			VertexTranslationInfoStyle = new GUIStyle();
			VertexTranslationInfoStyle.normal.background = EditorGUIUtility.whiteTexture;
			VertexTranslationInfoStyle.normal.textColor = new Color(1f, 1f, 1f, .6f);
			VertexTranslationInfoStyle.padding = new RectOffset(3,3,3,0);

			eye_on = (Texture2D)(Resources.Load(EditorGUIUtility.isProSkin ? "GUI/GenericIcons_16px_Eye_On" : "GUI/GenericIcons_16px_Eye_Off", typeof(Texture2D)));
			eye_off = (Texture2D)(Resources.Load(EditorGUIUtility.isProSkin ? "GUI/GenericIcons_16px_Eye_Off" : "GUI/GenericIcons_16px_Eye_On", typeof(Texture2D)));

			show_Detail = true;
			show_Mover = true;
			show_Collider = true;
			show_Trigger = true;

			this.minSize = new Vector2( isFloatingWindow ? WINDOW_WIDTH_FlOATING : WINDOW_WIDTH_DOCKABLE, 200 );
		}

		// always need the toolbar loaded	
		bool isProSkin = true; // EditorGUIUtility.isProSkin;
		
		Texture2D object_Graphic_off = (Texture2D)(Resources.Load(isProSkin ? "GUI/ProBuilderGUI_Mode_Object_Pro" : "GUI/ProBuilderGUI_Mode_Object", typeof(Texture2D)));
		Texture2D face_Graphic_off = (Texture2D)(Resources.Load(isProSkin ? "GUI/ProBuilderGUI_Mode_Face-Off_Small-Pro" : "GUI/ProBuilderGUI_Mode_Face-Off_Small", typeof(Texture2D)));
		Texture2D vertex_Graphic_off = (Texture2D)(Resources.Load(isProSkin ? "GUI/ProBuilderGUI_Mode_Vertex-Off_Small-Pro" : "GUI/ProBuilderGUI_Mode_Vertex-Off_Small", typeof(Texture2D)));
		Texture2D edge_Graphic_off = (Texture2D)(Resources.Load(isProSkin ? "GUI/ProBuilderGUI_Mode_Edge-Off_Small-Pro" : "GUI/ProBuilderGUI_Mode_Edge-Off_Small", typeof(Texture2D)));
	
		if(pref_showToolbar)
		{
			EditModeIcons = new GUIContent[]
			{
				new GUIContent(object_Graphic_off, "Object Selection"),
				new GUIContent(vertex_Graphic_off, "Vertex Selection"),
				new GUIContent(edge_Graphic_off, "Edge Selection"),
				new GUIContent(face_Graphic_off, "Face Selection")
			};
		}
		else
		{
			EditModeIcons = new GUIContent[]
			{
				new GUIContent(vertex_Graphic_off, "Vertex Selection"),
				new GUIContent(edge_Graphic_off, "Edge Selection"),
				new GUIContent(face_Graphic_off, "Face Selection")
			};

			elementModeToolbarRect.y = 6;
		}
	}

	private void LoadPrefs()
	{
		// this exists to force update preferences when updating packages
		if(!EditorPrefs.HasKey(pb_Constant.pbEditorPrefVersion) || EditorPrefs.GetInt(pb_Constant.pbEditorPrefVersion) != EDITOR_PREF_VERSION )
		{
			EditorPrefs.SetInt(pb_Constant.pbEditorPrefVersion, EDITOR_PREF_VERSION);
			EditorPrefs.DeleteKey(pb_Constant.pbVertexHandleSize);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultFaceColor);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultEdgeColor);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultSelectedVertexColor);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultVertexColor);
			EditorPrefs.DeleteKey(pb_Constant.pbDefaultShortcuts);
		}

		editLevel 			= pb_Preferences_Internal.GetEnum<EditLevel>(pb_Constant.pbDefaultEditLevel);
		selectionMode		= pb_Preferences_Internal.GetEnum<SelectMode>(pb_Constant.pbDefaultSelectionMode);
		handleAlignment		= pb_Preferences_Internal.GetEnum<HandleAlignment>(pb_Constant.pbHandleAlignment);
		pref_showSceneInfo 	= pb_Preferences_Internal.GetBool(pb_Constant.pbShowSceneInfo);
		pref_backfaceSelect = pb_Preferences_Internal.GetBool(pb_Constant.pbEnableBackfaceSelection);

		pref_snapEnabled 	= pb_ProGrids_Interface.SnapEnabled();
		pref_snapValue		= pb_ProGrids_Interface.SnapValue();
		pref_snapAxisConstraints = pb_ProGrids_Interface.UseAxisConstraints();

		shortcuts 			= pb_Shortcut.ParseShortcuts(EditorPrefs.GetString(pb_Constant.pbDefaultShortcuts)).ToArray();
		limitFaceDragCheckToSelection = pb_Preferences_Internal.GetBool(pb_Constant.pbDragCheckLimit);

		pref_showToolbar = pb_Preferences_Internal.GetBool(pb_Constant.pbShowSceneToolbar);
		pref_sceneToolbarLocation = pb_Preferences_Internal.GetEnum<SceneToolbarLocation>(pb_Constant.pbToolbarLocation);
		prefs_iconGui = pb_Preferences_Internal.GetBool(pb_Constant.pbIconGUI);
	}

	private void OnDestroy()
	{
		if(pb_ElementGraphics.nullableInstance != null)
			GameObject.DestroyImmediate(pb_ElementGraphics.nullableInstance.gameObject);

		if(pb_LineRenderer.nullableInstance != null)
			GameObject.DestroyImmediate(pb_LineRenderer.nullableInstance.gameObject);
	}

	private void OnDisable()
	{
		_instance = null;

		ClearFaceSelection();

		UpdateSelection();

		if( OnSelectionUpdate != null )
			OnSelectionUpdate(null);

		pb_ProGrids_Interface.UnsubscribePushToGridEvent(PushToGrid);

		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
		Undo.undoRedoPerformed -= this.UndoRedoPerformed;

		EditorPrefs.SetInt(pb_Constant.pbHandleAlignment, (int)handleAlignment);

		if(pb_LineRenderer.Valid())
			pb_LineRenderer.instance.Clear();

		// re-enable unity wireframe
		foreach(pb_Object pb in FindObjectsOfType(typeof(pb_Object)))
		{
			Renderer ren = pb.gameObject.GetComponent<Renderer>();
			EditorUtility.SetSelectedWireframeHidden(ren, false);
		}

		SceneView.RepaintAll();
	}
#endregion

#region EVENT HANDLERS

	/**
	 * Delegate called on element or object selection change.
	 */
	public delegate void OnSelectionUpdateEventHandler(pb_Object[] selection);
	public static event OnSelectionUpdateEventHandler OnSelectionUpdate;

	public delegate void OnVertexMovementFinishedEventHandler(pb_Object[] selection);
	public static event OnVertexMovementFinishedEventHandler OnVertexMovementFinished;

	public void HookDelegates()
	{
		if(SceneView.onSceneGUIDelegate != this.OnSceneGUI)
		{
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			SceneView.onSceneGUIDelegate += this.OnSceneGUI;
		}

		Undo.undoRedoPerformed += this.UndoRedoPerformed;
		// Undo.postprocessModifications += PostprocessModifications;

		pb_ProGrids_Interface.SubscribePushToGridEvent(PushToGrid);
		pb_ProGrids_Interface.SubscribeToolbarEvent(ProGridsToolbarOpen);
		EditorApplication.playmodeStateChanged += OnPlayModeStateChanged;
	}
#endregion

#region ONGUI

	public void OnInspectorUpdate()
	{
		if(EditorWindow.focusedWindow != this)
			Repaint();
	}

	bool guiInitialized = false;

	/**
	 * Tool setting foldouts
	 */
	bool tool_vertexColors = false;
	bool tool_growSelection = false;
	bool tool_extrudeButton = false;
#if !PROTOTYPE
	bool tool_weldButton = false;
#endif
	Vector2 scroll = Vector2.zero;
	Rect elementModeToolbarRect = new Rect(3,6,128,24);
	private static GUIContent gui_content_bridge = new GUIContent("", "");
#if PROTOTYPE
	private static Color ProOnlyTint
	{
		get
		{
			return EditorGUIUtility.isProSkin ? new Color(.25f, 1f, 1f, 1f) : new Color(0f, .5f, 1f, 1f);
		}
	}
	private static readonly Color UpgradeTint = new Color(.5f, 1f, 1f, 1f);
#endif

	/**
	 *	Set GUI.enabled to true or false based on whether PROTOTYPE is defined or not.
	 */
	private bool ProOnlyButton(string content, string tooltip, GUIStyle style = null)
	{
#if PROTOTYPE
		pb_GUI_Utility.PushGUIEnabled(false);
		gui_content_bridge.text = content;
		gui_content_bridge.tooltip = tooltip + (string.IsNullOrEmpty(tooltip) ? "(ProBuilder Advanced Feature" : "\n(ProBuilder Advanced Feature)");
		pb_GUI_Utility.PushGUIContentColor(ProOnlyTint);

		bool ret = false;

		if(!EditorGUIUtility.isProSkin && style != null)
		{
			Color tc = style.normal.textColor;
			style.normal.textColor = ProOnlyTint;
			ret = style != null ? GUILayout.Button(gui_content_bridge, style) : GUILayout.Button(gui_content_bridge);
			style.normal.textColor = tc;
		}
		else
		{
			ret = style != null ? GUILayout.Button(gui_content_bridge, style) : GUILayout.Button(gui_content_bridge);
		}

		pb_GUI_Utility.PopGUIContentColor();
		pb_GUI_Utility.PopGUIEnabled();
		return ret;
#else
		gui_content_bridge.text = content;
		gui_content_bridge.tooltip = tooltip;
		return style != null ? GUILayout.Button(gui_content_bridge, style) : GUILayout.Button(gui_content_bridge);
#endif
	}

	private bool AutoContentButton(string content, string tooltip, GUIStyle style = null, params GUILayoutOption[] options)
	{
		gui_content_bridge.text = content;
		gui_content_bridge.tooltip = tooltip;

		if(style != null)
			return GUILayout.Button(gui_content_bridge, style, options);
		else
			return GUILayout.Button(gui_content_bridge, options);
	}

	void OnGUI()
	{
		Event e = Event.current;

		switch(e.type)
		{
			case EventType.ContextClick:
				OpenContextMenu();
				break;
		}

		if( prefs_iconGui )
		{
			iconGui.OnGUI();
			return;
		}

		if(!guiInitialized)
		{
			eye_style = new GUIStyle( EditorStyles.miniButtonRight );
			eye_style.padding = new RectOffset(0,0,0,0);
		}

#if PROTOTYPE
		GUI.backgroundColor = UpgradeTint;
		if(AutoContentButton("Upgrade", "Upgrade to ProBuilder Advanced for some seriously excellent additional features."))
		{
			// due to bug in asset store window, this only works if the window is already open
			if(pb_Editor_Utility.AssetStoreWindowIsOpen())
				Application.OpenURL("com.unity3d.kharma:content/3558");
			else
				Application.OpenURL("http://bit.ly/1GJEuIG"); // "http://u3d.as/30b");
		}
		GUI.backgroundColor = Color.white;
#endif

		if(!pref_showToolbar)
		{
			int t_selectionMode = editLevel != EditLevel.Top ? (int)selectionMode : -1;
			elementModeToolbarRect.x = (Screen.width/2 - 48) + (isFloatingWindow ? 1 : -1);

			EditorGUI.BeginChangeCheck();

			t_selectionMode = GUI.Toolbar(elementModeToolbarRect, (int)t_selectionMode, EditModeIcons, "Command");

			if(EditorGUI.EndChangeCheck())
			{
				if(t_selectionMode == (int)selectionMode && editLevel != EditLevel.Top)
				{
					SetEditLevel( EditLevel.Top );
				}
				else
				{
					if(editLevel == EditLevel.Top)
						SetEditLevel( EditLevel.Geometry );

					SetSelectionMode( (SelectMode)t_selectionMode );
					selectionMode = (SelectMode)t_selectionMode;
				}
			}

#if UNITY_5
			GUILayout.Space( isFloatingWindow ? 30 : 34 );
#else
			GUILayout.Space( isFloatingWindow ? 28 : 34 );
#endif

			GUI.backgroundColor = pb_Constant.ProBuilderDarkGray;
			pb_GUI_Utility.DrawSeparator(2);
			GUI.backgroundColor = Color.white;
		}

		scroll = GUILayout.BeginScrollView(scroll);

		GUILayout.Label("Tools", EditorStyles.boldLabel);
		ToolsGUI();

		GUILayout.Label("Selection", EditorStyles.boldLabel);
		SelectionGUI();

		GUILayout.Label("Object", EditorStyles.boldLabel);
		ObjectGUI();

		if(editLevel == EditLevel.Geometry)
		{
			switch(selectionMode)
			{
				case SelectMode.Edge:
					GUILayout.Label("Edge", EditorStyles.boldLabel);
					break;

				case SelectMode.Vertex:
					GUILayout.Label("Vertex", EditorStyles.boldLabel);
					break;

				case SelectMode.Face:
					GUILayout.Label("Face", EditorStyles.boldLabel);
					break;
			}

			ActionsGUI();
		}

		GUILayout.Space(2);
		GUI.backgroundColor = pb_Constant.ProBuilderDarkGray;
		pb_GUI_Utility.DrawSeparator(1);
		GUI.backgroundColor = Color.white;
		GUILayout.Space(2);

		EntityGUI();

		GUILayout.EndScrollView();
	}

	int buttonPad
	{
		get
		{
			if( pb_Preferences_Internal.GetBool(pb_Constant.pbDefaultOpenInDockableWindow))
				return (int) ((bool)pb_Reflection.GetValue(this, "docked") ? 11 : 7);
			else
				return 11;
		}
	}

	void SelectionGUI()
	{
		bool wasEnabled = GUI.enabled;

		EditorGUI.BeginChangeCheck();
		handleAlignment = (HandleAlignment)EditorGUILayout.EnumPopup(new GUIContent("", "Toggle between Global, Local, and Plane Coordinates"), handleAlignment, GUILayout.MaxWidth(Screen.width - buttonPad));

		if(EditorGUI.EndChangeCheck())
			SetHandleAlignment(handleAlignment);

		EditorGUI.BeginChangeCheck();

			if( AutoContentButton(pref_backfaceSelect ? "Select All" : "Select Visible", "If Select All is enabled, drag and click selections will select elements hidden behind faces.  If Select Visible is on, only elements that are viewable in the scene will be selected.", EditorStyles.miniButton) )
				pref_backfaceSelect = !pref_backfaceSelect;

		if(EditorGUI.EndChangeCheck())
			EditorPrefs.SetBool(pb_Constant.pbEnableBackfaceSelection, pref_backfaceSelect);

		GUI.enabled = selectedVertexCount > 0;

		tool_growSelection = pb_GUI_Utility.ToolSettingsGUI("Grow", "Adds adjacent faces to the current selection.  Optionally can restrict augmentation to faces within a restricted angle difference.",
			tool_growSelection,
			pb_Menu_Commands.MenuGrowSelection,
			pb_Menu_Commands.GrowSelectionGUI,
			selectionMode == SelectMode.Face,
			Screen.width,
			54,
			selection);

		if(AutoContentButton("Shrink", "Remove outside elements from the current selection.", EditorStyles.miniButton))
			pb_Menu_Commands.MenuShrinkSelection(selection);

		if(AutoContentButton("Invert", "Set the element selection to the inverse of what is currently selected.", EditorStyles.miniButton))
			pb_Menu_Commands.MenuInvertSelection(selection);

		switch(selectionMode)
		{
			case SelectMode.Edge:
				GUI.enabled = selectedEdgeCount > 0;
				if(AutoContentButton("Loop", "Select all edges in a loop using the current edge selection as a starting point.", EditorStyles.miniButton))
					pb_Menu_Commands.MenuLoopSelection(selection);

				if(AutoContentButton("Ring", "Select all edges that form a ring, using the current edge selection as a starting point.", EditorStyles.miniButton))
					pb_Menu_Commands.MenuRingSelection(selection);
				break;
		}

		GUI.enabled = wasEnabled;
	}

	void ToolsGUI()
	{
		if(AutoContentButton("Shape", "Open Shape Creation Panel", EditorStyles.miniButton))
			pb_Geometry_Interface.MenuOpenShapeCreator();

#if !PROTOTYPE
		if(ProOnlyButton("Material", "Open Material Editor Window.  You can also Drag and Drop materials or textures to selected faces.", EditorStyles.miniButton))
			pb_Material_Editor.MenuOpenMaterialEditor();

		if (ProOnlyButton("UV Editor", "Open UV Editor Window", EditorStyles.miniButton))
			pb_UV_Editor.MenuOpenUVEditor();
#else
		ProOnlyButton("Material", "Open Material Editor Window.  You can also Drag and Drop materials or textures to selected faces.", EditorStyles.miniButton);
		ProOnlyButton("UV Editor", "Open UV Editor Window", EditorStyles.miniButton);
#endif

		tool_vertexColors = pb_GUI_Utility.ToolSettingsGUI("Vertex Color", "Open the vertex color editor.  Assign colors by face and selection with the Color Palette, or paint with a brush using the Color Painter.",
			tool_vertexColors,
			pb_Menu_Commands.MenuOpenVertexColorsEditor2,
			pb_Menu_Commands.VertexColorsGUI,
			Screen.width,
			36,
			selection);

#if !PROTOTYPE
		if(ProOnlyButton("Smoothing", "Opens the Smoothing Groups window.  Use this to achieve either faceted or smooth edges", EditorStyles.miniButton))
			pb_Smoothing_Editor.MenuOpenSmoothingEditor();
#else
		ProOnlyButton("Smoothing", "Opens the Smoothing Groups window.  Use this to achieve either faceted or smooth edges", EditorStyles.miniButton);
#endif
	}

	void ObjectGUI()
	{
		pb_GUI_Utility.PushGUIEnabled( selection != null && selection.Length > 1 );

		if(ProOnlyButton("Merge", "Combine all selected ProBuilder objects into a single object.", EditorStyles.miniButton))
			pb_Menu_Commands.MenuMergeObjects(selection);

		pb_GUI_Utility.PopGUIEnabled();

		pb_GUI_Utility.PushGUIEnabled( selection != null && selection.Length > 0 );

		if(ProOnlyButton("Mirror", "Open the Mirror Tool panel.", EditorStyles.miniButton))
			EditorWindow.GetWindow<pb_Mirror_Tool>(true, "Mirror Tool", true).Show();

		if(GUILayout.Button(new GUIContent("Flip Normals", "Reverse the direction of all faces on selected objects."), EditorStyles.miniButton))
			pb_Menu_Commands.MenuFlipObjectNormals(selection);

		if(ProOnlyButton("Subdivide", "Split all selected faces (or entire object) smaller faces", EditorStyles.miniButton))
			pb_Menu_Commands.MenuSubdivide(selection);

		if(ProOnlyButton("Set Pivot", "Move the mesh pivot to the center of the current element selection", EditorStyles.miniButton))
			pb_Menu_Commands.MenuSetPivot(selection);

		pb_GUI_Utility.PopGUIEnabled();
	}

	void ActionsGUI()
	{
		pb_GUI_Utility.PushGUIEnabled(selectedVertexCount > 0);

		if(AutoContentButton("Set Pivot", "Set the pivot of selected geometry to the center of the current element selection.", EditorStyles.miniButton))
			pb_Menu_Commands.MenuSetPivot(selection);

#if PROTOTYPE
		GUI.enabled = selectedFaceCount > 0;

		if(selectedEdgeCount > 0 && selectedFaceCount < 1)
		{
			ProOnlyButton("Extrude", "Extrude selected edges.  Also try holding 'Shift' while moving the scene handle.", EditorStyles.miniButton);
		}
		else
		{
			tool_extrudeButton = pb_GUI_Utility.ToolSettingsGUI("Extrude", "Extrude the currently selected elements by a set amount.  Also try holding 'Shift' while moving the handle tool.",
				tool_extrudeButton,
				pb_Menu_Commands.MenuExtrude,
				pb_Menu_Commands.ExtrudeButtonGUI,
				Screen.width,
				36,
				selection);
		}
#else
		GUI.enabled = selectedFaceCount > 0 || selectedEdgeCount > 0;

		tool_extrudeButton = pb_GUI_Utility.ToolSettingsGUI("Extrude", "Extrude the currently selected elements by a set amount.  Also try holding 'Shift' while moving the handle tool.",
			tool_extrudeButton,
			pb_Menu_Commands.MenuExtrude,
			pb_Menu_Commands.ExtrudeButtonGUI,
			Screen.width,
			36,
			selection);
#endif

		GUI.enabled = selectedFaceCount > 0;

		if(AutoContentButton("Conform Normals", "Automatically attempts to make selected face normals face the same direction.", EditorStyles.miniButton))
			pb_Menu_Commands.MenuConformNormals(selection);

		if(AutoContentButton("Flip Normals", "Reverses the direction of the selected faces.", EditorStyles.miniButton))
			pb_Menu_Commands.MenuFlipNormals(selection);

		if(AutoContentButton("Flip Edge", "Swaps the orientation of the connecting edge in a quad.", EditorStyles.miniButton))
			pb_Menu_Commands.MenuFlipEdges(selection);

		if(AutoContentButton("Delete", "Delete the selected faces.", EditorStyles.miniButton))
			pb_Menu_Commands.MenuDeleteFace(selection);

		if(ProOnlyButton("Detach", "Split selected faces off to a new submesh or object.", EditorStyles.miniButton))
			pb_Menu_Commands.MenuDetachFaces(selection);

		GUI.enabled = selectedFaceCount > 1;
		if(ProOnlyButton("Merge Faces", "Merge the selected faces to a single face.", EditorStyles.miniButton))
			pb_Menu_Commands.MenuMergeFaces(selection);

		switch(selectionMode)
		{
			case SelectMode.Face:

				GUI.enabled = selectedFaceCount > 0;
				if(ProOnlyButton("Subdiv Face", "Split the face selection into multiple faces by connecting the edges of each face at the center of the face.", EditorStyles.miniButton))
					pb_Menu_Commands.MenuSubdivideFace(selection);
				break;

			case SelectMode.Edge:

				GUI.enabled = selectedEdgeCount == 2;

				if(ProOnlyButton("Bridge", "Create a face between two selected edges.", EditorStyles.miniButton))
					pb_Menu_Commands.MenuBridgeEdges(selection);

				GUI.enabled = selectedEdgeCount > 1;

				if(ProOnlyButton("Connect", "Create an edge by connecting the center of each selected edge.", EditorStyles.miniButton))
					pb_Menu_Commands.MenuConnectEdges(selection);

				GUI.enabled = selectedEdgeCount > 0;

				if(ProOnlyButton("Insert Loop", "Inserts an Edge loop by selecting the edge ring, then connecting the centers of all edges.", EditorStyles.miniButton))
					pb_Menu_Commands.MenuInsertEdgeLoop(selection);

				break;

			case SelectMode.Vertex:

				GUI.enabled = per_object_vertexCount_distinct > 1;

				if(ProOnlyButton("Connect", "Insert edges connecting all selected vertices.", EditorStyles.miniButton))
					pb_Menu_Commands.MenuConnectVertices(selection);

#if PROTOTYPE
				ProOnlyButton("Weld", "Merge selected vertices that are within a specified distance of one another.", EditorStyles.miniButton);
#else
				tool_weldButton = pb_GUI_Utility.ToolSettingsGUI("Weld", "Merge selected vertices that are within a specified distance of one another.",
					tool_weldButton,
					pb_Menu_Commands.MenuWeldVertices,
					pb_Menu_Commands.WeldButtonGUI,
					Screen.width,
					20,
					selection);
#endif

				if(ProOnlyButton("Collapse", "Merge all selected vertices to a single vertex positioned at the center of the selection.", EditorStyles.miniButton))
					pb_Menu_Commands.MenuCollapseVertices(selection);

				GUI.enabled = per_object_vertexCount_distinct > 0;

				if(ProOnlyButton("Split", "Make each selected vertex move independently.", EditorStyles.miniButton))
					pb_Menu_Commands.MenuSplitVertices(selection);

				break;
		}
	}

	void EntityGUI()
	{
		pb_GUI_Utility.PushGUIEnabled( !EditorApplication.isPlaying );

		float entityButtonWidth = this.position.width - 28 - buttonPad;

		GUILayout.BeginHorizontal();
			pb_GUI_Utility.PushGUIEnabled(GUI.enabled && selection != null && selection.Length > 0);
			if(AutoContentButton("Detail", "Sets all objects in selection to the entity type Detail.  Detail objects are marked with all static flags except Occluding and Reflection Probes.", EditorStyles.miniButtonLeft, GUILayout.MaxWidth(entityButtonWidth)))
			{
				pb_Menu_Commands.MenuSetEntityType(selection, EntityType.Detail);
				ToggleEntityVisibility(EntityType.Detail, show_Detail);
			}
			pb_GUI_Utility.PopGUIEnabled();

			if(GUILayout.Button( show_Detail ? eye_on : eye_off, eye_style, GUILayout.MinWidth(28), GUILayout.MaxWidth(28), GUILayout.MaxHeight(15) ))
			{
				show_Detail = !show_Detail;
				EditorPrefs.SetBool(pb_Constant.pbShowDetail, show_Detail);
				ToggleEntityVisibility(EntityType.Detail, show_Detail);
			}
		GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
				pb_GUI_Utility.PushGUIEnabled(GUI.enabled && selection != null && selection.Length > 0);
			if(AutoContentButton("Mover", "Sets all objects in selection to the entity type Mover.  Mover types have no static flags, so they may be moved during play mode.", EditorStyles.miniButtonLeft, GUILayout.MaxWidth(entityButtonWidth)))
			{
				pb_Menu_Commands.MenuSetEntityType(selection, EntityType.Mover);
				ToggleEntityVisibility(EntityType.Mover, show_Mover);
			}
			pb_GUI_Utility.PopGUIEnabled();

			if(GUILayout.Button( show_Mover ? eye_on : eye_off, eye_style, GUILayout.MinWidth(28), GUILayout.MaxWidth(28), GUILayout.MaxHeight(15) )) {
				show_Mover = !show_Mover;
				EditorPrefs.SetBool(pb_Constant.pbShowMover, show_Mover);
				ToggleEntityVisibility(EntityType.Mover, show_Mover);
			}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
			pb_GUI_Utility.PushGUIEnabled(GUI.enabled && selection != null && selection.Length > 0);
			if(AutoContentButton("Collider", "Sets all objects in selection to the entity type Collider.  Collider types have Navigation and Off-Link Nav static flags set by default, and will have their MeshRenderer disabled on entering play mode.", EditorStyles.miniButtonLeft, GUILayout.MaxWidth(entityButtonWidth)))
			{
				pb_Menu_Commands.MenuSetEntityType(selection, EntityType.Collider);
				ToggleEntityVisibility(EntityType.Collider, show_Collider);
			}
			pb_GUI_Utility.PopGUIEnabled();

			if(GUILayout.Button( show_Collider ? eye_on : eye_off, eye_style, GUILayout.MinWidth(28), GUILayout.MaxWidth(28), GUILayout.MaxHeight(15) )) {
				show_Collider = !show_Collider;
				EditorPrefs.SetBool(pb_Constant.pbShowCollider, show_Collider);
				ToggleEntityVisibility(EntityType.Collider, show_Collider);
			}
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
			pb_GUI_Utility.PushGUIEnabled(GUI.enabled && selection != null && selection.Length > 0);
			if(AutoContentButton("Trigger", "Sets all objects in selection to the entity type Trigger.  Trigger types have no static flags, and have a convex collider marked as Is Trigger added.  The MeshRenderer is turned off on entering play mode.", EditorStyles.miniButtonLeft, GUILayout.MaxWidth(entityButtonWidth)))
			{
				pb_Menu_Commands.MenuSetEntityType(selection, EntityType.Trigger);
				ToggleEntityVisibility(EntityType.Trigger, show_Trigger);
			}
			pb_GUI_Utility.PopGUIEnabled();

			if(GUILayout.Button( show_Trigger ? eye_on : eye_off, eye_style, GUILayout.MinWidth(28), GUILayout.MaxWidth(28), GUILayout.MaxHeight(15) )) {
				show_Trigger = !show_Trigger;
				EditorPrefs.SetBool(pb_Constant.pbShowTrigger, show_Trigger);
				ToggleEntityVisibility(EntityType.Trigger, show_Trigger);
			}
		GUILayout.EndHorizontal();

		pb_GUI_Utility.PopGUIEnabled();
	}
#endregion

#region CONTEXT MENU

	void OpenContextMenu()
	{
		GenericMenu menu = new GenericMenu();

		menu.AddItem (new GUIContent("Open As Floating Window", ""), false, Menu_OpenAsFloatingWindow);
		menu.AddItem (new GUIContent("Open As Dockable Window", ""), false, Menu_OpenAsDockableWindow);

		// menu.AddSeparator("");
		menu.ShowAsContext ();
	}

	void Menu_OpenAsDockableWindow()
	{
		EditorPrefs.SetBool(pb_Constant.pbDefaultOpenInDockableWindow, true);

		EditorWindow.GetWindow<pb_Editor>().Close();
		pb_Editor.MenuOpenWindow();
	}

	void Menu_OpenAsFloatingWindow()
	{
		EditorPrefs.SetBool(pb_Constant.pbDefaultOpenInDockableWindow, false);

		EditorWindow.GetWindow<pb_Editor>().Close();
		pb_Editor.MenuOpenWindow();
	}
#endregion

#region ONSCENEGUI

	// GUI Caches
	public pb_Object[] selection = new pb_Object[0];						// All selected pb_Objects

	public int selectedVertexCount { get; private set; }					// Sum of all vertices sleected
	public int selectedFaceCount { get; private set; }						// Sum of all faces sleected
	public int selectedEdgeCount { get; private set; }						// Sum of all edges sleected

	// the mouse vertex selection box
	private Rect mouseRect = new Rect(0,0,0,0);

	// Handles
	Tool currentHandle = Tool.Move;

	// Dragging
	Vector2 mousePosition_initial;
	Rect selectionRect;
	Color dragRectColor = new Color(.313f, .8f, 1f, 1f);
	private bool dragging = false;
	private bool doubleClicked = false;	// prevents leftClickUp from stealing focus after double click

	// vertex handles
	Vector3 newPosition, cachedPosition;
	bool movingVertices = false;

	// top level caching
	bool scaling = false;

	private bool rightMouseDown = false;
	Event currentEvent;// = (Event)0;

	void OnSceneGUI(SceneView scnView)
	{
		currentEvent = Event.current;

		if(editLevel == EditLevel.Geometry)
		{
			if( currentEvent.Equals(Event.KeyboardEvent("v")) )
				snapToVertex = true;
			else if( currentEvent.Equals(Event.KeyboardEvent("c")) )
				snapToFace = true;
		}

		/**
		 * Snap stuff
		 */
		if(currentEvent.type == EventType.KeyUp)
		{
			snapToFace = false;
			snapToVertex = false;
		}

		if(currentEvent.type == EventType.MouseDown && currentEvent.button == 1)
			rightMouseDown = true;

		if(currentEvent.type == EventType.MouseUp && currentEvent.button == 1 || currentEvent.type == EventType.Ignore)
			rightMouseDown = false;

#if !PROTOTYPE
			// e.type == EventType.DragUpdated ||
			if(currentEvent.type == EventType.DragPerform)
			{
				GameObject go = HandleUtility.PickGameObject(currentEvent.mousePosition, false);

				if(go != null && System.Array.Exists(DragAndDrop.objectReferences, x => x is Texture2D || x is Material))
				{
					pb_Object pb = go.GetComponent<pb_Object>();

					if( pb )
					{
						Material mat = null;
						foreach(Object t in DragAndDrop.objectReferences)
						{
							if(t is Material)
							{
								mat = (Material)t;
								break;
							}
							/* This works, but throws some bullshit errors. Not creating a material leaks, so disable this functionality. */
							else
							if(t is Texture2D)
							{
								mat = new Material(Shader.Find("Diffuse"));
								mat.mainTexture = (Texture2D)t;

								string texPath = AssetDatabase.GetAssetPath(mat.mainTexture);
								int lastDot = texPath.LastIndexOf(".");
								texPath = texPath.Substring(0, texPath.Length - (texPath.Length-lastDot));
								texPath = AssetDatabase.GenerateUniqueAssetPath(texPath + ".mat");

								AssetDatabase.CreateAsset(mat, texPath);
								AssetDatabase.Refresh();

								break;
							}
						}

						if(mat != null)
						{
							if(editLevel == EditLevel.Geometry)
							{
								pbUndo.RecordObjects(selection, "Set Face Materials");

								foreach(pb_Object pbs in selection)
									pbs.SetFaceMaterial(pbs.SelectedFaces.Length < 1 ? pbs.faces : pbs.SelectedFaces, mat);

							}
							else
							{
								pbUndo.RecordObject(pb, "Set Object Material");
								pb.SetFaceMaterial(pb.faces, mat);
							}

							pb.ToMesh();
							pb.Refresh();
							pb.Optimize();

							currentEvent.Use();
						}
					}
				}
			}
#endif

		DrawHandleGUI(scnView);

		if(!rightMouseDown && getKeyUp != KeyCode.None)
		{
			if(ShortcutCheck(currentEvent))
			{
				currentEvent.Use();
				return;
			}
		}

		// Finished moving vertices, scaling, or adjusting uvs
#if PROTOTYPE
		if( (movingVertices || scaling) && GUIUtility.hotControl < 1)
		{
			OnFinishVertexModification();
		}
#else
		if( (movingVertices || movingPictures || scaling) && GUIUtility.hotControl < 1)
		{
			OnFinishVertexModification();
			UpdateHandleRotation();
			UpdateTextureHandles();
		}
#endif

		// Check mouse position in scene and determine if we should highlight something
		if(currentEvent.type == EventType.MouseMove)
			UpdateMouse(currentEvent.mousePosition);

		// Draw GUI Handles
		if(editLevel != EditLevel.Top && editLevel != EditLevel.Plugin)
			DrawHandles();

		if(Tools.current != Tool.None && Tools.current != currentHandle)
			SetTool_Internal(Tools.current);

		if( (editLevel == EditLevel.Geometry || editLevel == EditLevel.Texture) && Tools.current != Tool.View)
		{
			if( selectedVertexCount > 0 )
			{
				if(editLevel == EditLevel.Geometry)
				{
					switch(currentHandle)
					{
						case Tool.Move:
							VertexMoveTool();
							break;
						case Tool.Scale:
							VertexScaleTool();
							break;
						case Tool.Rotate:
							VertexRotateTool();
							break;
					}
				}
#if !PROTOTYPE  // TEXTURE HANDLES
				else if(editLevel == EditLevel.Texture && selectedVertexCount > 0)
				{
					switch(currentHandle)
					{
						case Tool.Move:
							TextureMoveTool();
							break;
						case Tool.Rotate:
							TextureRotateTool();
							break;
						case Tool.Scale:
							TextureScaleTool();
							break;
					}
				}
#endif
			}
		}
		else
		{
			return;
		}

		// altClick || Tools.current == Tool.View || GUIUtility.hotControl > 0 || middleClick
		// Tools.viewTool == ViewTool.FPS || Tools.viewTool == ViewTool.Orbit
		if( pb_Handle_Utility.SceneViewInUse(currentEvent) || currentEvent.isKey || selection == null || selection.Length < 1)
		{
			dragging = false;

			return;
		}

		/* * * * * * * * * * * * * * * * * * * * *
		 *	 Vertex & Quad Wranglin' Ahead! 	 *
		 * 	 Everything from this point below	 *
		 *	 overrides something Unity related.  *
		 * * * * * * * * * * * * * * * * * * * * */

		// This prevents us from selecting other objects in the scene,
		// and allows for the selection of faces / vertices.
		int controlID = GUIUtility.GetControlID(FocusType.Passive);
		HandleUtility.AddDefaultControl(controlID);

		// If selection is made, don't use default handle -- set it to Tools.None
		if(selectedVertexCount > 0)
			Tools.current = Tool.None;

		if(leftClick) {
			// double clicking object
			if(currentEvent.clickCount > 1)
			{
				DoubleClick(currentEvent);
			}

			mousePosition_initial = mousePosition;
		}

		if(mouseDrag)
			dragging = true;

		if(ignore)
		{
			if(dragging)
			{
				dragging = false;
				DragCheck();
			}

			if(doubleClicked)
				doubleClicked = false;
		}

		if(leftClickUp)
		{
			if(doubleClicked)
			{
				doubleClicked = false;
			}
			else
			{
				if(!dragging)
				{
#if !PROTOTYPE
					if(pb_UV_Editor.instance)
						pb_UV_Editor.instance.ResetUserPivot();
#endif

					RaycastCheck(currentEvent.mousePosition);
				}
				else
				{
					dragging = false;

#if !PROTOTYPE
					if(pb_UV_Editor.instance)
						pb_UV_Editor.instance.ResetUserPivot();
#endif

					DragCheck();
				}
			}
		}
	}

	void DoubleClick(Event e)
	{
		pb_Object pb = RaycastCheck(e.mousePosition);
		if(pb != null)
		{
			if(selectionMode == SelectMode.Edge)
			{
				if(e.shift)
					pb_Menu_Commands.MenuRingSelection(selection);
				else
					pb_Menu_Commands.MenuLoopSelection(selection);
			}
			else
				pb.SetSelectedFaces(pb.faces);

			UpdateSelection(false);
			SceneView.RepaintAll();
			doubleClicked = true;
		}
	}
#endregion

#region RAYCASTING AND DRAGGING

	public const float MAX_EDGE_SELECT_DISTANCE = 12;
	pb_Object nearestEdgeObject = null;
	pb_Edge nearestEdge;

	/**
	 * If in Edge mode, finds the nearest Edge to the mouse
	 */
	private void UpdateMouse(Vector3 mousePosition)
	{
		if(selection.Length < 1) return;

		switch(selectionMode)
		{
			// default:
			case SelectMode.Edge:

				GameObject go = HandleUtility.PickGameObject(mousePosition, false);

				pb_Edge bestEdge = null;
				pb_Object bestObj = go == null ? null : go.GetComponent<pb_Object>();

				if(bestObj != null && !selection.Contains(bestObj))
				{
					bestObj = null;
					bestEdge = null;
					goto SkipMouseCheck;
				}

				/**
				 * If mouse isn't over a pb object, it still may be near enough to an edge.
				 */
				if(bestObj == null)
				{
					// TODO
					float bestDistance = MAX_EDGE_SELECT_DISTANCE;

					try
					{
						for(int i = 0; i < m_universalEdges.Length; i++)
						{
							pb_Edge[] edges = m_universalEdges[i];

							for(int j = 0; j < edges.Length; j++)
							{
								int x = selection[i].sharedIndices[edges[j].x][0];
								int y = selection[i].sharedIndices[edges[j].y][0];

								Vector3 world_vert_x = m_verticesInWorldSpace[i][x];
								Vector3 world_vert_y = m_verticesInWorldSpace[i][y];

								float d = HandleUtility.DistanceToLine(world_vert_x, world_vert_y);

								if(d < bestDistance)
								{
									bestObj = selection[i];
									bestEdge = new pb_Edge(x, y);
									bestDistance = d;
								}
							}
						}
					} catch {}
				}
				else
				{
					// Test culling
					List<pb_RaycastHit> hits;
					Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

					if(pb_Handle_Utility.MeshRaycast(ray, bestObj, out hits, Mathf.Infinity, Culling.FrontBack))
					{
						Camera cam = SceneView.lastActiveSceneView.camera;

						// Sort from nearest hit to farthest
						hits.Sort( (x, y) => x.distance.CompareTo(y.distance) );

						// Find the nearest edge in the hit faces

						float bestDistance = Mathf.Infinity;
						Vector3[] v = bestObj.vertices;

						for(int i = 0; i < hits.Count; i++)
						{
							if( pb_HandleUtility.PointIsOccluded(cam, bestObj, bestObj.transform.TransformPoint(hits[i].point)) )
								continue;

							foreach(pb_Edge edge in bestObj.faces[hits[i].face].edges)
							{
								float d = HandleUtility.DistancePointLine(hits[i].point, v[edge.x], v[edge.y]);

								if(d < bestDistance)
								{
									bestDistance = d;
									bestEdge = edge;
								}
							}

							if( Vector3.Dot(ray.direction, bestObj.transform.TransformDirection(hits[i].normal)) < 0f )
								break;
						}

						if(bestEdge != null && HandleUtility.DistanceToLine(bestObj.transform.TransformPoint(v[bestEdge.x]), bestObj.transform.TransformPoint(v[bestEdge.y])) > MAX_EDGE_SELECT_DISTANCE)
							bestEdge = null;
					}
				}

				SkipMouseCheck:

				if(bestEdge != nearestEdge || bestObj != nearestEdgeObject)
				{
					nearestEdge = bestEdge;
					nearestEdgeObject = bestObj;

					SceneView.RepaintAll();
				}
				break;
		}
	}

	// Returns the pb_Object modified by this action.  If no action taken, or action is eaten by texture window, return null.
	// A pb_Object is returned because double click actions need to know what the last selected pb_Object was.
	private pb_Object RaycastCheck(Vector3 mousePosition)
	{
		pb_Object pb = null;

		/**
		 * Since Edge or Vertex selection may be valid even if clicking off a gameObject, check them
		 * first.  If no hits, move on to face selection or object change.
		 */
		if( (selectionMode == SelectMode.Edge && EdgeClickCheck(out pb)) ||
			(selectionMode == SelectMode.Vertex && VertexClickCheck(out pb)))
		{
			UpdateSelection(false);
			SceneView.RepaintAll();
			return pb;
		}


		if(!shiftKey && !ctrlKey)
			SetSelection( (GameObject)null );

		GameObject nearestGameObject = HandleUtility.PickGameObject(mousePosition, false);

		if(nearestGameObject)
			pb = nearestGameObject.GetComponent<pb_Object>();

		if(nearestGameObject)
		{
			if(pb != null)
			{
				if(pb.isSelectable)
					AddToSelection(nearestGameObject);
				else
					return null;
			}
			else if( !pb_Preferences_Internal.GetBool(pb_Constant.pbPBOSelectionOnly) )
			{
				// If clicked off a pb_Object but onto another gameobject, set the selection
				// and dip out.
				SetSelection(nearestGameObject);
				return null;
			}
			else
			{
				// clicked on something that isn't allowed at all (ex, pboSelectionOnly on and clicked a camera)
				return null;
			}
		}
		else
		{
			UpdateSelection(true);
			return null;
		}

		// Face click check
		{
			// Check for face hit
			pb_Face selectedFace;

			//  MeshRaycast(Ray InWorldRay, pb_Object pb, out int OutHitFace, out float OutHitPoint)
			Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
			pb_RaycastHit hit;


			if( pb_Handle_Utility.MeshRaycast(ray, pb, out hit, Mathf.Infinity, pref_backfaceSelect ? Culling.FrontBack : Culling.Front) )
			{
				selectedFace = pb.faces[hit.face];

				/**
				 * Check for other editor mouse shortcuts first - todo: better way to do this.
				 */

#if !PROTOTYPE
				pb_Material_Editor matEditor = pb_Material_Editor.instance;
				if( matEditor != null && matEditor.ClickShortcutCheck(Event.current.modifiers, pb, selectedFace) )
					return pb;

				pb_UV_Editor uvEditor = pb_UV_Editor.instance;
				if(uvEditor != null && uvEditor.ClickShortcutCheck(pb, selectedFace))
					return pb;
#endif


				// Check to see if we've already selected this quad.  If so, remove it from selection cache.
				pbUndo.RecordSelection(pb, "Change Face Selection");

				int indx = System.Array.IndexOf(pb.SelectedFaces, selectedFace);
				if( indx > -1 ) {
					pb.RemoveFromFaceSelectionAtIndex(indx);
				} else {
					pb.AddToFaceSelection(hit.face);
				}
			}
		}

		Event.current.Use();

		// OnSelectionChange will also call UpdateSelection, but this needs to remain
		// because it catches element selection changes.
		UpdateSelection(false);
		SceneView.RepaintAll();

		return pb;
	}

	private bool VertexClickCheck(out pb_Object vpb)
	{
		if(!shiftKey && !ctrlKey) ClearFaceSelection();

		Camera cam = SceneView.lastActiveSceneView.camera;

		for(int i = 0; i < selection.Length; i++)
		{
			pb_Object pb = selection[i];
			if(!pb.isSelectable) continue;

			for(int n = 0; n < m_uniqueIndices[i].Length; n++)
			{
				Vector3 v = m_verticesInWorldSpace[i][m_uniqueIndices[i][n]];

				if(mouseRect.Contains(HandleUtility.WorldToGUIPoint(v)))
				{
					if( pb_HandleUtility.PointIsOccluded(cam, pb, v) )
					{
						continue;
					}

					// Check if index is already selected, and if not add it to the pot
					int indx = System.Array.IndexOf(pb.SelectedTriangles, m_uniqueIndices[i][n]);

					pbUndo.RecordObject(pb, "Change Vertex Selection");

					// If we get a match, check to see if it exists in our selection array already, then add / remove
					if( indx > -1 )
						pb.SetSelectedTriangles(pb.SelectedTriangles.RemoveAt(indx));
					else
						pb.SetSelectedTriangles(pb.SelectedTriangles.Add(m_uniqueIndices[i][n]));

					vpb = pb;
					return true;
				}
			}
		}

		vpb = null;
		return false;
	}

	private bool EdgeClickCheck(out pb_Object pb)
	{
		if(!shiftKey && !ctrlKey)
		{
			// don't call ClearFaceSelection b/c that also removes
			// nearestEdge info
			foreach(pb_Object p in selection)
				p.ClearSelection();
		}

		if(nearestEdgeObject != null)
		{
			pb = nearestEdgeObject;

			if(nearestEdge != null && nearestEdge.IsValid())
			{
				pb_Edge edge;

				if( pb_Edge.ValidateEdge(pb, nearestEdge, out edge) )
					nearestEdge = edge;

				int ind = pb.SelectedEdges.IndexOf(nearestEdge, pb.sharedIndices.ToDictionary());

				pbUndo.RecordSelection(pb, "Change Edge Selection");

				if( ind > -1 )
					pb.SetSelectedEdges(pb.SelectedEdges.RemoveAt(ind));
				else
					pb.SetSelectedEdges(pb.SelectedEdges.Add(nearestEdge));

				return true;
			}

			return false;
		}
		else
		{
			if(!shiftKey && !ctrlKey)
				ClearFaceSelection();

			pb = null;

			return false;
		}
	}

	private void DragCheck()
	{
		Camera cam = SceneView.lastActiveSceneView.camera;
		limitFaceDragCheckToSelection = pb_Preferences_Internal.GetBool(pb_Constant.pbDragCheckLimit);

		pbUndo.RecordSelection(selection, "Drag Select");

		switch(selectionMode)
		{
			case SelectMode.Vertex:
			{
				if(!shiftKey && !ctrlKey) ClearFaceSelection();

				// profiler.BeginSample("Drag Select Vertices");
				for(int i = 0; i < selection.Length; i++)
				{
					pb_Object pb = selection[i];
					if(!pb.isSelectable) continue;

					// profiler.BeginSample("Create HashSet");
					HashSet<int> selectedTriangles = new HashSet<int>(pb.SelectedTriangles);
					// profiler.EndSample();

					// selection[i].ToMesh();
					// selection[i].Refresh();
					// Vector3[] normals = selection[i].msh.normals;
					// selection[i].Optimize();
					// Vector3 camDirLocal = -selection[i].transform.InverseTransformDirection(cam.transform.forward);

					for(int n = 0; n < m_uniqueIndices[i].Length; n++)
					{
						Vector3 v = m_verticesInWorldSpace[i][m_uniqueIndices[i][n]];

						// profiler.BeginSample("Contains");
						bool contains = selectionRect.Contains(HandleUtility.WorldToGUIPoint(v));
						// profiler.EndSample();

						if(contains)
						{
							// if point is behind the camera, ignore it.
							// profiler.BeginSample("WorldToScreenPoint");
							if(cam.WorldToScreenPoint(v).z < 0)
							{
								// profiler.EndSample();
								continue;
							}
							// profiler.EndSample();


							// profiler.BeginSample("backface culling");
							// Vector3 nrm = normals[m_uniqueIndices[i][n]];
							// float dot = Vector3.Dot(camDirLocal, nrm);
							// if(!pref_backfaceSelect && (dot < 0 || pb_HandleUtility.PointIsOccluded(cam, selection[i], v)))
							if( !pref_backfaceSelect && pb_HandleUtility.PointIsOccluded(cam, selection[i], v) )
							{
								// profiler.EndSample();
								continue;
							}
							// profiler.EndSample();

							// Check if index is already selected, and if not add it to the pot
							// profiler.BeginSample("selected triangles contains");
							contains = selectedTriangles.Contains(m_uniqueIndices[i][n]);
							// profiler.EndSample();

							// profiler.BeginSample("add / remove");
							if( contains )
								selectedTriangles.Remove(m_uniqueIndices[i][n]);
							else
								selectedTriangles.Add(m_uniqueIndices[i][n]);
							// profiler.EndSample();
						}
					}

					// profiler.BeginSample("SetSelectedTriangles");
					pb.SetSelectedTriangles(selectedTriangles.ToArray());
					// profiler.EndSample();
				}
				// profiler.EndSample();

				if(!vertexSelectionMask)
					DragObjectCheck(true);

				UpdateSelection(false);
			}
			break;

			case SelectMode.Face:
			{
				if(!shiftKey && !ctrlKey) ClearFaceSelection();

				pb_Object[] pool = limitFaceDragCheckToSelection ? selection : (pb_Object[])FindObjectsOfType(typeof(pb_Object));

				List<pb_Face> selectedFaces;

				for(int i = 0; i < pool.Length; i++)
				{
					pb_Object pb = pool[i];
					selectedFaces = new List<pb_Face>(pb.SelectedFaces);

					if(!pb.isSelectable)
						continue;

					Vector3[] verticesInWorldSpace = m_verticesInWorldSpace[i];
					bool addToSelection = false;

					for(int n = 0; n < pb.faces.Length; n++)
					{
						pb_Face face = pb.faces[n];

						/// face is behind the camera
						if( cam.WorldToScreenPoint(verticesInWorldSpace[face.indices[0]]).z < 0 )//|| (!pref_backfaceSelect && Vector3.Dot(dir, nrm) > 0f))
							continue;

						// only check the first index per quad, and if it checks out, then check every other point
						if(selectionRect.Contains(HandleUtility.WorldToGUIPoint(verticesInWorldSpace[face.indices[0]])))
						{
							bool nope = false;
							for(int q = 1; q < face.distinctIndices.Length; q++)
							{
								if(!selectionRect.Contains(HandleUtility.WorldToGUIPoint(verticesInWorldSpace[face.distinctIndices[q]])))
								{
									nope = true;
									break;
								}
							}

							if(!nope)
							{
								if( pref_backfaceSelect || !pb_HandleUtility.PointIsOccluded(cam, pool[i], pb_Math.Average(pbUtil.ValuesWithIndices(verticesInWorldSpace, face.distinctIndices))) )
								{
									int indx =  selectedFaces.IndexOf(face);

									if( indx > -1 ) {
										selectedFaces.RemoveAt(indx);
									} else {
										addToSelection = true;
										selectedFaces.Add(face);
									}
								}
							}
						}
					}

					pb.SetSelectedFaces(selectedFaces.ToArray());
					if(addToSelection)
						AddToSelection(pb.gameObject);
				}

				DragObjectCheck(true);

				UpdateSelection(false);
			}
			break;

			case SelectMode.Edge:
			{
				if(!shiftKey && !ctrlKey) ClearFaceSelection();

				for(int i = 0; i < selection.Length; i++)
				{
					Vector3 v0 = Vector3.zero, v1 = Vector3.zero, cen = Vector3.zero;
					pb_Object pb = selection[i];
					Vector3[] vertices = m_verticesInWorldSpace[i];
					pb_IntArray[] sharedIndices = pb.sharedIndices;
					HashSet<pb_Edge> inSelection = new HashSet<pb_Edge>();

					for(int n = 0; n < m_universalEdges[i].Length; n++)
					{
						v0 = vertices[sharedIndices[m_universalEdges[i][n].x][0]];
						v1 = vertices[sharedIndices[m_universalEdges[i][n].y][0]];

						cen = (v0+v1)*.5f;

						bool behindCam = cam.WorldToScreenPoint(cen).z < 0;

						if( behindCam )
							continue;

						bool rectContains = selectionRect.Contains( HandleUtility.WorldToGUIPoint(cen) );

						if( rectContains )
						{
							bool occluded = !pref_backfaceSelect && pb_HandleUtility.PointIsOccluded(cam, pb, cen);

							if(!occluded)
							{
								inSelection.Add( new pb_Edge(m_universalEdges[i][n]) );
							}
						}
					}

					pb_Edge[] curSelection = pb_Edge.GetUniversalEdges(pb.SelectedEdges, m_sharedIndicesLookup[i]);
					inSelection.SymmetricExceptWith(curSelection);
					pb_Edge[] selected = inSelection.ToArray();

					for(int n = 0; n < selected.Length; n++)
					{
						selected[n].x = sharedIndices[selected[n].x][0];
						selected[n].y = sharedIndices[selected[n].y][0];
					}

					pb.SetSelectedEdges( selected );
				}

				if(!vertexSelectionMask)
				{
					DragObjectCheck(true);
				}

				UpdateSelection(false);
			}
			break;

		default:
			DragObjectCheck(false);
			break;
		}

		SceneView.RepaintAll();
	}

	// Emulates the usual Unity drag to select objects functionality
	private void DragObjectCheck(bool vertexMode)
	{
		// if we're in vertex selection mode, only add to selection if shift key is held,
		// and don't clear the selection if shift isn't held.
		// if not, behave regularly (clear selection if shift isn't held)
		if(!vertexMode) {
			if(!shiftKey) ClearSelection();
		} else {
			if(!shiftKey && selectedVertexCount > 0) return;
		}

		// scan for new selected objects
		/// if mode based, don't allow selection of non-probuilder objects
		if(!limitFaceDragCheckToSelection)
		{
			foreach(pb_Object g in HandleUtility.PickRectObjects(selectionRect).GetComponents<pb_Object>())
				if(!Selection.Contains(g.gameObject))
					AddToSelection(g.gameObject);
		}
	}
#endregion

#region VERTEX TOOLS

	private bool snapToVertex = false;
	private bool snapToFace = false;
	private Vector3 previousHandleScale = Vector3.one;
	private Vector3 currentHandleScale = Vector3.one;
	private Vector3[][] vertexOrigins;
	private Vector3[] vertexOffset;
	private Quaternion previousHandleRotation = Quaternion.identity;
	private Quaternion currentHandleRotation = Quaternion.identity;

	// Use for delta display
	private Vector3 translateOrigin = Vector3.zero;
	private Vector3 rotateOrigin = Vector3.zero;
	private Vector3 scaleOrigin = Vector3.zero;

	private void VertexMoveTool()
	{
		newPosition = m_handlePivotWorld;
		cachedPosition = newPosition;

		newPosition = Handles.PositionHandle(newPosition, handleRotation);

		if(altClick)
			return;

		bool previouslyMoving = movingVertices;

		if(newPosition != cachedPosition)
		{
			// profiler.BeginSample("VertexMoveTool()");
			Vector3 diff = newPosition-cachedPosition;

			Vector3 mask = diff.ToMask(pb_Math.HANDLE_EPSILON);

			if(snapToVertex)
			{
				Vector3 v;

				if( FindNearestVertex(mousePosition, out v) )
					diff = Vector3.Scale(v-cachedPosition, mask);
			}
			else if(snapToFace)
			{
				pb_Object obj = null;
				pb_RaycastHit hit;

				if( pb_Handle_Utility.FaceRaycast(mousePosition, out obj, out hit) && !(selection.Contains(obj) && SelectedFacesInEditZone.Any(x => x.Contains(obj.faces[hit.face]))))
				{
					if( mask.Sum() == 1 )
					{
						Ray r = new Ray(cachedPosition, -mask);
						Plane plane = new Plane(obj.transform.TransformDirection(hit.normal).normalized, obj.transform.TransformPoint(hit.point));

						float forward, backward;
						plane.Raycast(r, out forward);
						plane.Raycast(r, out backward);
						float planeHit = Mathf.Abs(forward) < Mathf.Abs(backward) ? forward : backward;
						r.direction = -r.direction;
						plane.Raycast(r, out forward);
						plane.Raycast(r, out backward);
						float rev = Mathf.Abs(forward) < Mathf.Abs(backward) ? forward : backward;
						if( Mathf.Abs(rev) > Mathf.Abs(planeHit) )
							planeHit = rev;

						if(Mathf.Abs(planeHit) > Mathf.Epsilon)
							diff = mask * -planeHit;
					}
					else
					{
						diff = Vector3.Scale(obj.transform.TransformPoint(hit.point) - cachedPosition, mask.Abs());
					}
				}
			}
			// else if(snapToEdge && nearestEdge.IsValid())
			// {
			// 	// FINDME

			// }

			movingVertices = true;

			if(previouslyMoving == false)
			{
				translateOrigin = cachedPosition;
				rotateOrigin = currentHandleRotation.eulerAngles;
				scaleOrigin = currentHandleScale;

				OnBeginVertexMovement();

				if(Event.current.modifiers == EventModifiers.Shift)
					ShiftExtrude();

				pb_ProGrids_Interface.OnHandleMove(mask);
			}

			for(int i = 0; i < selection.Length; i++)
			{
				selection[i].TranslateVertices_World(selection[i].SelectedTriangles, diff, pref_snapEnabled ? pref_snapValue : 0f, pref_snapAxisConstraints, m_sharedIndicesLookup[i]);
				selection[i].RefreshUV( SelectedFacesInEditZone[i] );
				selection[i].RefreshNormals();
				// selection[i].RefreshTangents();
				selection[i].msh.RecalculateBounds();
			}

			Internal_UpdateSelectionFast();

			// profiler.EndSample();
		}

	}

	private void VertexScaleTool()
	{
		newPosition = m_handlePivotWorld;

		previousHandleScale = currentHandleScale;

		currentHandleScale = Handles.ScaleHandle(currentHandleScale, newPosition, handleRotation, HandleUtility.GetHandleSize(newPosition));

		if(altClick) return;

		bool previouslyMoving = movingVertices;

		if(previousHandleScale != currentHandleScale)
		{
			movingVertices = true;
			if(previouslyMoving == false)
			{
				translateOrigin = cachedPosition;
				rotateOrigin = currentHandleRotation.eulerAngles;
				scaleOrigin = currentHandleScale;

				OnBeginVertexMovement();

				if(Event.current.modifiers == EventModifiers.Shift)
					ShiftExtrude();

				// cache vertex positions for scaling later
				vertexOrigins = new Vector3[selection.Length][];
				vertexOffset = new Vector3[selection.Length];

				for(int i = 0; i < selection.Length; i++)
				{
					vertexOrigins[i] = selection[i].vertices.ValuesWithIndices(selection[i].SelectedTriangles);
					vertexOffset[i] = pb_Math.Average(vertexOrigins[i]);
				}
			}

			Vector3 ver;	// resulting vertex from modification
			Vector3 over;	// vertex point to modify. different for world, local, and plane

			bool gotoWorld = Selection.transforms.Length > 1 && handleAlignment == HandleAlignment.Plane;
			bool gotoLocal = selectedFaceCount < 1;

			// if(pref_snapEnabled)
			// 	pbUndo.RecordObjects(selection as Object[], "Move Vertices");

			for(int i = 0; i < selection.Length; i++)
			{
				// get the plane rotation in local space
				Vector3 nrm = pb_Math.Normal(vertexOrigins[i]);
				Quaternion localRot = Quaternion.LookRotation(nrm == Vector3.zero ? Vector3.forward : nrm, Vector3.up);

				Vector3[] v = selection[i].vertices;
				pb_IntArray[] sharedIndices = selection[i].sharedIndices;

				for(int n = 0; n < selection[i].SelectedTriangles.Length; n++)
				{
					switch(handleAlignment)
					{
						case HandleAlignment.Plane:
						{
							if(gotoWorld)
								goto case HandleAlignment.World;

							if(gotoLocal)
								goto case HandleAlignment.Local;

							// move center of vertices to 0,0,0 and set rotation as close to identity as possible
							over = Quaternion.Inverse(localRot) * (vertexOrigins[i][n] - vertexOffset[i]);

							// apply scale
							ver = Vector3.Scale(over, currentHandleScale);

							// re-apply original rotation
							if(vertexOrigins[i].Length > 2)
								ver = localRot * ver;

							// re-apply world position offset
							ver += vertexOffset[i];

							int[] array = sharedIndices[m_sharedIndicesLookup[i][selection[i].SelectedTriangles[n]]].array;

							for(int t = 0; t < array.Length; t++)
								v[array[t]] = ver;

							break;
						}

						case HandleAlignment.World:
						case HandleAlignment.Local:
						{
							// move vertex to relative origin from center of selection
							over = vertexOrigins[i][n] - vertexOffset[i];
							// apply scale
							ver = Vector3.Scale(over, currentHandleScale);
							// move vertex back to locally offset position
							ver += vertexOffset[i];
							// set vertex in local space on pb-Object

							int[] array = sharedIndices[m_sharedIndicesLookup[i][selection[i].SelectedTriangles[n]]].array;

							for(int t = 0; t < array.Length; t++)
								v[array[t]] = ver;

							break;
						}
					}
				}

				selection[i].SetVertices(v);
				selection[i].msh.vertices = v;
				selection[i].RefreshUV( SelectedFacesInEditZone[i] );
				selection[i].RefreshNormals();
				selection[i].msh.RecalculateBounds();
			}

			Internal_UpdateSelectionFast();
		}
	}

	Quaternion c_inversePlaneRotation = Quaternion.identity;
	private void VertexRotateTool()
	{
		newPosition = m_handlePivotWorld;

		previousHandleRotation = currentHandleRotation;

		if(altClick)
			Handles.RotationHandle(currentHandleRotation, newPosition);
		else
			currentHandleRotation = Handles.RotationHandle(currentHandleRotation, newPosition);

		bool previouslyMoving = movingVertices;

		if(currentHandleRotation != previousHandleRotation)
		{
			// profiler.BeginSample("Rotate");
			movingVertices = true;
			if(previouslyMoving == false)
			{
				translateOrigin = cachedPosition;
				rotateOrigin = currentHandleRotation.eulerAngles;
				scaleOrigin = currentHandleScale;

				pb_Object pb;
				pb_Face face;
				if(GetFirstSelectedFace(out pb, out face))
				{
					Vector3 nrm, bitan, tan;
					pb_Math.NormalTangentBitangent(pb, face, out nrm, out tan, out bitan);
					c_inversePlaneRotation = Quaternion.Inverse( Quaternion.LookRotation(nrm, bitan) );
				}

				OnBeginVertexMovement();

				if(Event.current.modifiers == EventModifiers.Shift)
					ShiftExtrude();

				// cache vertex positions for modifying later
				vertexOrigins = new Vector3[selection.Length][];
				vertexOffset = new Vector3[selection.Length];

				for(int i = 0; i < selection.Length; i++)
				{
					vertexOrigins[i] = selection[i].vertices.ValuesWithIndices(selection[i].SelectedTriangles).ToArray();
					vertexOffset[i] = pb_Math.BoundsCenter(vertexOrigins[i]);
				}
			}

			// profiler.BeginSample("Calc Matrix");
			Quaternion transformedRotation;
			switch(handleAlignment)
			{
				case HandleAlignment.Plane:

					pb_Object pb;
					pb_Face face;

					if( !GetFirstSelectedFace(out pb, out face) )
						goto case HandleAlignment.Local;	// can't do plane without a plane

					Quaternion inverseLocalRotation = Quaternion.Inverse(pb.transform.localRotation);

					transformedRotation =  inverseLocalRotation * currentHandleRotation * c_inversePlaneRotation;
					break;

				case HandleAlignment.Local:

					if(selection.Length < 1)
						goto default;

					transformedRotation = Quaternion.Inverse(selection[0].transform.localRotation) * currentHandleRotation;
					break;

				default:
					transformedRotation = currentHandleRotation;
					break;
			}
			// profiler.EndSample();

			// profiler.BeginSample("matrix mult");
			Vector3 ver;	// resulting vertex from modification
			for(int i = 0; i < selection.Length; i++)
			{
				Vector3[] v = selection[i].vertices;
				pb_IntArray[] sharedIndices = selection[i].sharedIndices;

				if(selection.Length > 1)	// use world when selection is > 1 objects
				{
					for(int n = 0; n < selection[i].SelectedTriangles.Length; n++)
					{
						// vertex offset relative to object
						ver = vertexOrigins[i][n] - vertexOffset[i];

						// move to world space
						ver = selection[i].transform.localToWorldMatrix * ver;

						// apply handle rotation
						ver = currentHandleRotation * ver;

						// move back to local space
						ver = selection[i].transform.worldToLocalMatrix * ver;

						// and back to vertex position
						ver += vertexOffset[i];

						// now set in the msh.vertices array
						int[] array = sharedIndices[m_sharedIndicesLookup[i][selection[i].SelectedTriangles[n]]].array;

						for(int t = 0; t < array.Length; t++)
							v[array[t]] = ver;
					}
				}
				else
				{
					for(int n = 0; n < selection[i].SelectedTriangles.Length; n++)
					{
						// move vertex to relative origin from center of selection
						ver = vertexOrigins[i][n] - vertexOffset[i];

						ver = transformedRotation * ver;

						// move vertex back to locally offset position
						ver += vertexOffset[i];

						int[] array = sharedIndices[m_sharedIndicesLookup[i][selection[i].SelectedTriangles[n]]].array;

						for(int t = 0; t < array.Length; t++)
							v[array[t]] = ver;
					}

				}

				selection[i].SetVertices(v);
				selection[i].msh.vertices = v;

				// set vertex in local space on pb-Object

				selection[i].RefreshUV( SelectedFacesInEditZone[i] );
				selection[i].RefreshNormals();
				selection[i].msh.RecalculateBounds();
			}
			// profiler.EndSample();

			// don't modify the handle rotation because otherwise rotating with plane coordinates
			// updates the handle rotation with every change, making moving things a changing target
			Quaternion rotateToolHandleRotation = currentHandleRotation;

			Internal_UpdateSelectionFast();

			currentHandleRotation = rotateToolHandleRotation;
			// profiler.EndSample();
		}
	}

	/**
	 * Extrude the current selection with no translation.
	 */
	private void ShiftExtrude()
	{
		int ef = 0;
		foreach(pb_Object pb in selection)
		{
			// @todo - If caching normals, remove this 'ToMesh' and move
			Undo.RegisterCompleteObjectUndo(selection, "Extrude Vertices");

			switch(selectionMode)
			{
				case SelectMode.Edge:
					if(pb.SelectedFaceCount > 0)
						goto default;
#if !PROTOTYPE

					pb_Edge[] newEdges;
					bool success = pb.Extrude(	pb.SelectedEdges,
												0.0001f,
												pb_Preferences_Internal.GetBool(pb_Constant.pbExtrudeAsGroup),
												pb_Preferences_Internal.GetBool(pb_Constant.pbManifoldEdgeExtrusion),
												out newEdges);

					if(success)
					{
						ef += newEdges.Length;
						pb.SetSelectedEdges(newEdges);
					}
#endif
					break;

				default:
					int len = pb.SelectedFaces.Length;

					if(len > 0)
					{
						pb_Face[] append = null;
						pb.Extrude(pb.SelectedFaces, 0.0001f, pb_Preferences_Internal.GetBool(pb_Constant.pbExtrudeAsGroup), out append);
						pb.SetSelectedFaces(pb.SelectedFaces);

						ef += len;
					}
					break;
			}

			pb.ToMesh();
			pb.Refresh();
		}

		if(ef > 0)
		{
			pb_Editor_Utility.ShowNotification("Extrude");
			UpdateSelection(true);
		}
	}

#if !PROTOTYPE
	Vector3 textureHandle = Vector3.zero;
	Vector3 handleOrigin = Vector3.zero;
	bool movingPictures = false;

	private void TextureMoveTool()
	{
		pb_UV_Editor uvEditor = pb_UV_Editor.instance;
		if(!uvEditor) return;

		Vector3 cached = textureHandle;

		textureHandle = Handles.PositionHandle(textureHandle, handleRotation);

		if(altClick) return;

		if(textureHandle != cached)
		{
			if(!movingPictures)
			{
				handleOrigin = cached;
				movingPictures = true;
			}

			cached = Quaternion.Inverse(handleRotation) * textureHandle;
			cached.y = -cached.y;

			Vector3 lossyScale = selection[0].transform.lossyScale;
			uvEditor.SceneMoveTool( cached.DivideBy(lossyScale), handleOrigin.DivideBy(lossyScale) );
			uvEditor.Repaint();
		}
	}

	Quaternion textureRotation = Quaternion.identity;
	private void TextureRotateTool()
	{
		pb_UV_Editor uvEditor = pb_UV_Editor.instance;
		if(!uvEditor) return;

		float size = HandleUtility.GetHandleSize(m_handlePivotWorld);

		if(altClick) return;

		Matrix4x4 prev = Handles.matrix;
		Handles.matrix = handleMatrix;

		Quaternion cached = textureRotation;

		textureRotation = Handles.Disc(textureRotation, Vector3.zero, Vector3.forward, size, false, 0f);

		if(textureRotation != cached)
		{
			if(!movingPictures)
				movingPictures = true;

			uvEditor.SceneRotateTool(-textureRotation.eulerAngles.z);
		}

		Handles.matrix = prev;
	}

	Vector3 textureScale = Vector3.one;

	private void TextureScaleTool()
	{
		pb_UV_Editor uvEditor = pb_UV_Editor.instance;
		if(!uvEditor) return;

		float size = HandleUtility.GetHandleSize(m_handlePivotWorld);

		Matrix4x4 prev = Handles.matrix;
		Handles.matrix = handleMatrix;

		Vector3 cached = textureScale;
		textureScale = Handles.ScaleHandle(textureScale, Vector3.zero, Quaternion.identity, size);

		if(altClick) return;

		if(cached != textureScale)
		{
			if(!movingPictures)
				movingPictures = true;

			uvEditor.SceneScaleTool(textureScale, cached);
		}

		Handles.matrix = prev;
	}
#endif
#endregion

#region HANDLE DRAWING

	public void DrawHandles ()
	{
		Handles.lighting = false;

		/**
		 * Edge wireframe and selected faces are drawn in pb_ElementGraphics, selected edges & vertices
		 * are drawn here.
		 */
		switch(selectionMode)
		{
			case SelectMode.Edge:

				// TODO - figure out how to run UpdateSelection prior to an Undo event.
				// Currently UndoRedoPerformed is called after the action has taken place.
				try
				{
					Handles.color = Color.green;
					for(int i = 0; i < selection.Length; i++)
					{
						for(int j = 0; j < selection[i].SelectedEdges.Length; j++)
						{
							pb_Object pb = selection[i];
							Vector3[] v = m_verticesInWorldSpace[i];

							Handles.DrawLine(v[pb.SelectedEdges[j].x], v[pb.SelectedEdges[j].y]);
						}
					}

					if(nearestEdgeObject != null && nearestEdge.IsValid())
					{
						Handles.color = Color.red;
						Handles.DrawLine( 	nearestEdgeObject.transform.TransformPoint(nearestEdgeObject.vertices[nearestEdge.x]),
											nearestEdgeObject.transform.TransformPoint(nearestEdgeObject.vertices[nearestEdge.y]) );
					}
				} catch {}
				Handles.color = Color.white;

				break;
		}

		Handles.lighting = true;
	}

	Color handleBgColor;
	Rect sceneInfoRect = new Rect(10, 10, 200, 40);

	public void DrawHandleGUI(SceneView sceneView)
	{
		if(sceneView != SceneView.lastActiveSceneView)
			return;

		Handles.BeginGUI();

		if(pref_showToolbar)
		{
			int t_selectionMode = editLevel != EditLevel.Top ? ((int)selectionMode) + 1 : 0;

			switch(pref_sceneToolbarLocation)
			{
				case SceneToolbarLocation.BottomCenter:
					elementModeToolbarRect.x = (Screen.width/2 - 64);
					elementModeToolbarRect.y = Screen.height - elementModeToolbarRect.height * 3;
					break;

				case SceneToolbarLocation.BottomLeft:
					elementModeToolbarRect.x = 12;
					elementModeToolbarRect.y = Screen.height - elementModeToolbarRect.height * 3;
					break;

				case SceneToolbarLocation.BottomRight:
					elementModeToolbarRect.x = Screen.width - (elementModeToolbarRect.width + 12);
					elementModeToolbarRect.y = Screen.height - elementModeToolbarRect.height * 3;
					break;

				case SceneToolbarLocation.UpperLeft:
					elementModeToolbarRect.x = 12;
					elementModeToolbarRect.y = 10;
					break;

				case SceneToolbarLocation.UpperRight:
					elementModeToolbarRect.x = Screen.width - (elementModeToolbarRect.width + 96);
					elementModeToolbarRect.y = 10;
					break;

				default:
				case SceneToolbarLocation.UpperCenter:
					elementModeToolbarRect.x = (Screen.width/2 - 64);
					elementModeToolbarRect.y = 10;
					break;
			}

			EditorGUI.BeginChangeCheck();

			t_selectionMode = GUI.Toolbar(elementModeToolbarRect, (int)t_selectionMode, EditModeIcons, "Command");

			if(EditorGUI.EndChangeCheck())
			{
				if(t_selectionMode == 0)
				{
					SetEditLevel(EditLevel.Top);
				}
				else
				{
					if(editLevel != EditLevel.Geometry)
						SetEditLevel(EditLevel.Geometry);

					SetSelectionMode( (SelectMode)(t_selectionMode - 1));
				}
			}
		}

		handleBgColor = GUI.backgroundColor;

		if(movingVertices && pref_showSceneInfo)
		{
			GUI.backgroundColor = pb_Constant.ProBuilderLightGray;

			GUI.Label(new Rect(Screen.width-200, Screen.height-120, 162, 48),
				"Translate: " + (newPosition-translateOrigin).ToString() +
				"\nRotate: " + (currentHandleRotation.eulerAngles-rotateOrigin).ToString() +
				"\nScale: " + (currentHandleScale-scaleOrigin).ToString()
				, VertexTranslationInfoStyle
				);
		}

		if( pref_showSceneInfo )
		{
			/**
			 * Show the PB cached and Unity mesh element counts if in Debug mode.
			 */
			try
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();

				sb.AppendLine("Faces: " + faceCount);
				sb.AppendLine("Triangles: " + triangleCount);
				sb.AppendLine("Vertices: " + vertexCount + " (" + (selection != null ? selection.Select(x => x.msh.vertexCount).Sum() : 0).ToString() + ")\n");
				sb.AppendLine("Selected Faces: " + selectedFaceCount);
				sb.AppendLine("Selected Edges: " + selectedEdgeCount);
				sb.AppendLine("Selected Vertices: " + selectedVertexCount);

				GUIContent gc = new GUIContent(sb.ToString(), "");

				Vector2 size = EditorStyles.label.CalcSize(gc);

				sceneInfoRect.width = size.x + 8;
				sceneInfoRect.height = size.y - 4;

				pb_GUI_Utility.DrawSolidColor( new Rect(sceneInfoRect.x-4, sceneInfoRect.y-4, sceneInfoRect.width, sceneInfoRect.height), new Color(.1f,.1f,.1f,.55f));

				GUI.Label(sceneInfoRect, gc);
			} catch {}
		}

		// Enables vertex selection with a mouse click
		if(editLevel == EditLevel.Geometry && !dragging && selectionMode == SelectMode.Vertex)
			mouseRect = new Rect(Event.current.mousePosition.x-10, Event.current.mousePosition.y-10, 20, 20);
		else
			mouseRect = pb_Constant.RectZero;

		// Draw selection rect if dragging

		if(dragging)
		{
			GUI.backgroundColor = dragRectColor;

			// Always draw from lowest to largest values
			Vector2 start = Vector2.Min(mousePosition_initial, mousePosition);
			Vector2 end = Vector2.Max(mousePosition_initial, mousePosition);

			selectionRect = new Rect(start.x, start.y,
				end.x - start.x, end.y - start.y);

			GUI.Box(selectionRect, "");

			HandleUtility.Repaint();
		}

		GUI.backgroundColor = handleBgColor;

		Handles.EndGUI();
	}
#endregion

#region SHORTCUT

	public bool ShortcutCheck(Event e)
	{
		List<pb_Shortcut> matches = shortcuts.Where(x => x.Matches(e.keyCode, e.modifiers)).ToList();

		if(matches.Count < 1)
			return false;

		bool used = false;
		pb_Shortcut usedShortcut = null;

		foreach(pb_Shortcut cut in matches)
		{
			if(AllLevelShortcuts(cut))
			{
				used = true;
				usedShortcut = cut;
				break;
			}
		}

		if(!used)
		{
			foreach(pb_Shortcut cut in matches)
			{
				switch(editLevel)
				{
					case EditLevel.Top:
						used = TopLevelShortcuts(cut);
						break;

					case EditLevel.Texture:
						goto case EditLevel.Geometry;

					case EditLevel.Geometry:
						used = GeoLevelShortcuts(cut);
						break;
				}

				if(used)
				{
					usedShortcut = cut;
					break;
				}
			}
		}

		if(used)
		{
			if(	usedShortcut.action != "Delete Face" &&
				usedShortcut.action != "Escape" &&
				usedShortcut.action != "Quick Apply Nodraw" &&
				usedShortcut.action != "Toggle Geometry Mode" &&
				usedShortcut.action != "Toggle Handle Pivot" &&
				usedShortcut.action != "Toggle Selection Mode" )
				pb_Editor_Utility.ShowNotification(usedShortcut.action);

			Event.current.Use();
		}

		return used;
	}

	private bool AllLevelShortcuts(pb_Shortcut shortcut)
	{
		bool uniqueModeShortcuts = pb_Preferences_Internal.GetBool(pb_Constant.pbUniqueModeShortcuts);

		switch(shortcut.action)
		{
			// TODO Remove once a workaround for non-upper-case shortcut chars is found
			case "Toggle Geometry Mode":

				if(editLevel == EditLevel.Geometry)
				{
					pb_Editor_Utility.ShowNotification("Top Level Editing");
					SetEditLevel(EditLevel.Top);
				}
				else if( !uniqueModeShortcuts )
				{
					pb_Editor_Utility.ShowNotification("Geometry Editing");
					SetEditLevel(EditLevel.Geometry);
				}
				return true;

			case "Vertex Mode":
			{
				if(!uniqueModeShortcuts)
					return false;

				if(editLevel == EditLevel.Top)
					SetEditLevel(EditLevel.Geometry);

				SetSelectionMode( SelectMode.Vertex );
				return true;
			}

			case "Edge Mode":
			{
				if(!uniqueModeShortcuts)
					return false;

				if(editLevel == EditLevel.Top)
					SetEditLevel(EditLevel.Geometry);

				SetSelectionMode( SelectMode.Edge );
				return true;
			}

			case "Face Mode":
			{
				if(!uniqueModeShortcuts)
					return false;

				if(editLevel == EditLevel.Top)
					SetEditLevel(EditLevel.Geometry);

				SetSelectionMode( SelectMode.Face );
				return true;
			}

			default:
				return false;
		}
	}

	private bool TopLevelShortcuts(pb_Shortcut shortcut)
	{
		if(selection == null || selection.Length < 1 || editLevel != EditLevel.Top)
			return false;

		switch(shortcut.action)
		{
			/* ENTITY TYPES */
			case "Set Trigger":
					pb_Menu_Commands.MenuSetEntityType(selection, EntityType.Trigger);
				return true;

			case "Set Occluder":
					pb_Menu_Commands.MenuSetEntityType(selection, EntityType.Occluder);
				return true;

			case "Set Collider":
					pb_Menu_Commands.MenuSetEntityType(selection, EntityType.Collider);
				return true;

			case "Set Mover":
					pb_Menu_Commands.MenuSetEntityType(selection, EntityType.Mover);
				return true;

			case "Set Detail":
					pb_Menu_Commands.MenuSetEntityType(selection, EntityType.Detail);
				return true;

			default:
				return true;
		}
	}

	private bool GeoLevelShortcuts(pb_Shortcut shortcut)
	{
		switch(shortcut.action)
		{
			case "Escape":
				ClearFaceSelection();
				pb_Editor_Utility.ShowNotification("Top Level");
				UpdateSelection(false);
				SetEditLevel(EditLevel.Top);
				return true;

			// TODO Remove once a workaround for non-upper-case shortcut chars is found
			case "Toggle Selection Mode":

				if( pb_Preferences_Internal.GetBool(pb_Constant.pbUniqueModeShortcuts) )
					return false;

				ToggleSelectionMode();
				switch(selectionMode)
				{
					case SelectMode.Face:
						pb_Editor_Utility.ShowNotification("Editing Faces");
						break;

					case SelectMode.Vertex:
						pb_Editor_Utility.ShowNotification("Editing Vertices");
						break;

					case SelectMode.Edge:
						pb_Editor_Utility.ShowNotification("Editing Edges");
						break;
				}
				return true;

			case "Delete Face":
				pb_Menu_Commands.MenuDeleteFace(selection);
				return true;

			/* handle alignment */
			case "Toggle Handle Pivot":
				if(selectedVertexCount < 1)
					return false;

				if(editLevel != EditLevel.Texture)
				{
					ToggleHandleAlignment();
					pb_Editor_Utility.ShowNotification("Handle Alignment: " + ((HandleAlignment)handleAlignment).ToString());
				}
				return true;

			case "Set Pivot":

				if (selection.Length > 0)
				{
					foreach (pb_Object pbo in selection)
					{
						pbUndo.RecordObjects(new Object[2] {pbo, pbo.transform}, "Set Pivot");

						if (pbo.SelectedTriangles.Length > 0)
						{
							pbo.CenterPivot(pbo.SelectedTriangles);
						}
						else
						{
							pbo.CenterPivot(null);
						}
					}
				}
				return true;

			default:
				return false;
		}
	}
#endregion

#region VIS GROUPS

	bool show_Detail;
	bool show_Mover;
	bool show_Collider;
	bool show_Trigger;

	public void ToggleEntityVisibility(EntityType entityType, bool isVisible)
	{
		foreach(pb_Entity sel in Object.FindObjectsOfType(typeof(pb_Entity)))
		{
			if(sel.entityType == entityType) {
				sel.GetComponent<MeshRenderer>().enabled = isVisible;
				if(sel.GetComponent<MeshCollider>())
					sel.GetComponent<MeshCollider>().enabled = isVisible;
			}
		}
	}
#endregion

#region TOOL SETTINGS

	/**
	 * Allows another window to tell the Editor what Tool is now in use.
	 * Does *not* update any other windows.
	 */
	public void SetTool(Tool newTool)
	{
		currentHandle = newTool;
	}

	/**
	 * Calls SetTool(), then Updates the UV Editor window if applicable.
	 */
	private void SetTool_Internal(Tool newTool)
	{
		SetTool(newTool);

#if !PROTOTYPE
		if(pb_UV_Editor.instance != null)
			pb_UV_Editor.instance.SetTool(newTool);
#endif
	}

	public void SetHandleAlignment(HandleAlignment ha)
	{
		if(editLevel == EditLevel.Texture)
			ha = HandleAlignment.Plane;
		else
			EditorPrefs.SetInt(pb_Constant.pbHandleAlignment, (int)ha);

		handleAlignment = ha;

		UpdateHandleRotation();

		currentHandleRotation = handleRotation;

		SceneView.RepaintAll();

		// todo
		Repaint();
	}

	public void ToggleHandleAlignment()
	{
		int newHa = (int)handleAlignment+1;
		if( newHa >= System.Enum.GetValues(typeof(HandleAlignment)).Length)
			newHa = 0;
		SetHandleAlignment((HandleAlignment)newHa);
	}

	public void ToggleEditLevel()
	{
		if(editLevel == EditLevel.Geometry)
			SetEditLevel(EditLevel.Top);
		else
			SetEditLevel(EditLevel.Geometry);
	}

	/**
	 * Toggles between the SelectMode values and updates the graphic handles
	 * as necessary.
	 */
	public void ToggleSelectionMode()
	{
		int smode = (int)selectionMode;
		smode++;
		if(smode >= SELECT_MODE_LENGTH)
			smode = 0;
		SetSelectionMode( (SelectMode)smode );
	}

	/**
	 * \brief Sets the current selection mode @SelectMode to the mode value.
	 */
	public void SetSelectionMode(SelectMode mode)
	{
		selectionMode = mode;

		Internal_UpdateSelectionFast();

		EditorPrefs.SetInt(pb_Constant.pbDefaultSelectionMode, (int)selectionMode);

		SceneView.RepaintAll();
	}

	/**
	 * Set the EditLevel back to its last level.
	 */
	public void PopEditLevel()
	{
		SetEditLevel(previousEditLevel);
	}

	/**
	 * Changes the current Editor level - switches between Object, Sub-object, and Texture (hidden).
	 */
	public void SetEditLevel(EditLevel el)
	{
		previousEditLevel = editLevel;
		editLevel = el;

		switch(el)
		{
			case EditLevel.Top:
				ClearFaceSelection();
				UpdateSelection(true);

				SetSelection(Selection.gameObjects);
				break;

			case EditLevel.Geometry:

				Tools.current = Tool.None;

				UpdateSelection(false);
				SceneView.RepaintAll();
				break;

			case EditLevel.Plugin:
				UpdateSelection(false);
				SceneView.RepaintAll();
				break;

#if !PROTOTYPE
			case EditLevel.Texture:

				previousHandleAlignment = handleAlignment;
				previousSelectMode = selectionMode;

				SetHandleAlignment(HandleAlignment.Plane);
				break;
#endif
		}


#if !PROTOTYPE
		if(previousEditLevel == EditLevel.Texture && el != EditLevel.Texture)
		{
			SetSelectionMode(previousSelectMode);
			SetHandleAlignment(previousHandleAlignment);
		}
#endif

		if(editLevel != EditLevel.Texture)
			EditorPrefs.SetInt(pb_Constant.pbDefaultEditLevel, (int)editLevel);

		if( onEditLevelChanged != null )
			onEditLevelChanged( (int) editLevel );
	}
#endregion

#region SELECTION CACHING

	/**
	 *	\brief Updates the arrays used to draw GUI elements (both Window and Scene).
	 *	@selection_vertex should already be populated at this point.  UpdateSelection
	 *	just removes duplicate indices, and populates the gui arrays for displaying
	 *	 things like quad faces and vertex billboards.
	 */

	int[][] 				m_uniqueIndices = new int[0][];
	Vector3[][] 			m_verticesInWorldSpace = new Vector3[0][];
	pb_Edge[][] 			m_universalEdges = new pb_Edge[0][];
	Vector3					m_handlePivotWorld = Vector3.zero;
	Dictionary<int, int>[] 	m_sharedIndicesLookup = new Dictionary<int, int>[0];

	public pb_Edge[][]  SelectedUniversalEdges { get { return m_universalEdges; } }

	// faces that need to be refreshed when moving or modifying the actual selection
	public pb_Face[][] 	SelectedFacesInEditZone { get; private set; }

	// The number of selected distinct indices on the object with the greatest number of selected distinct indices.
	int per_object_vertexCount_distinct = 0;

	int faceCount = 0;
	int vertexCount = 0;
	int triangleCount = 0;

	public void UpdateSelection() { UpdateSelection(true); }
	public void UpdateSelection(bool forceUpdate)
	{
		// profiler.BeginSample("UpdateSelection()");
		per_object_vertexCount_distinct = 0;

		selectedVertexCount = 0;
		selectedFaceCount = 0;
		selectedEdgeCount = 0;

		faceCount = 0;
		vertexCount = 0;
		triangleCount = 0;

		pb_Object[] t_selection = selection;

		selection = pbUtil.GetComponents<pb_Object>(Selection.transforms);


		// If the top level selection has changed, update all the heavy cache things
		// that don't change based on element selction
		if(forceUpdate || !t_selection.SequenceEqual(selection))
		{
			// profiler.BeginSample("Heavy Update");

			forceUpdate = true;	// If updating due to inequal selections, set the forceUpdate to true so some of the functions below know that these values
								// can be trusted.
			m_universalEdges 		= new pb_Edge[selection.Length][];
			m_verticesInWorldSpace 	= new Vector3[selection.Length][];
			m_uniqueIndices			= new int[selection.Length][];
			m_sharedIndicesLookup 	= new Dictionary<int, int>[selection.Length];

			for(int i = 0; i < selection.Length; i++)
			{
				// profiler.BeginSample("Unique Indices");
				m_uniqueIndices[i] = selection[i].faces.SelectMany(x => x != null ? x.distinctIndices : null).ToArray();
				// profiler.EndSample();

				// profiler.BeginSample("sharedIndices.ToDictionary()");
				m_sharedIndicesLookup[i] = selection[i].sharedIndices.ToDictionary();
				// profiler.EndSample();

				// profiler.BeginSample("GetUniversalEdges (dictionary)");
				m_universalEdges[i] = pb_Edge.GetUniversalEdges(pb_Edge.AllEdges(selection[i].faces), m_sharedIndicesLookup[i]);
				// profiler.EndSample();
			}
			// profiler.EndSample();
		}

		SelectedFacesInEditZone = new pb_Face[selection.Length][];

		m_handlePivotWorld = Vector3.zero;

		Vector3 min = Vector3.zero, max = Vector3.zero;
		bool boundsInitialized = false;

		for(int i = 0; i < selection.Length; i++)
		{
			pb_Object pb = selection[i];

			// pb.transform.hasChanged = false;

			// profiler.BeginSample("VerticesInWorldSpace");
			m_verticesInWorldSpace[i] = selection[i].VerticesInWorldSpace();	// to speed this up, could just get uniqueIndices vertiecs
			// profiler.EndSample();

			if(!boundsInitialized && pb.SelectedTriangleCount > 0)
			{
				boundsInitialized = true;
				min = pb.transform.TransformPoint(pb.vertices[pb.SelectedTriangles[0]]);
				max = min;
			}

			if(pb.SelectedTriangles.Length > 0)
			{
				if(forceUpdate)
				{
					foreach(Vector3 v in pbUtil.ValuesWithIndices(m_verticesInWorldSpace[i], pb.SelectedTriangles))
					{
						min = Vector3.Min(min, v);
						max = Vector3.Max(max, v);
					}
				}
				else
				{
					foreach(Vector3 v in pb.VerticesInWorldSpace(pb.SelectedTriangles))
					{
						min = Vector3.Min(min, v);
						max = Vector3.Max(max, v);
					}
				}
			}

			SelectedFacesInEditZone[i] = pbMeshUtils.GetNeighborFaces(pb, pb.SelectedTriangles).ToArray();

			selectedVertexCount += selection[i].SelectedTriangles.Length;
			selectedFaceCount += selection[i].SelectedFaceIndices.Length;
			selectedEdgeCount += selection[i].SelectedEdges.Length;

			int distinctVertexCount = selection[i].sharedIndices.UniqueIndicesWithValues(selection[i].SelectedTriangles).ToList().Count;

			if(distinctVertexCount > per_object_vertexCount_distinct)
				per_object_vertexCount_distinct = distinctVertexCount;

			faceCount += selection[i].faces.Length;
			vertexCount += selection[i].sharedIndices.Length; // vertexCount;
			triangleCount += selection[i].msh.triangles.Length / 3;
		}

		m_handlePivotWorld = (max+min)/2f;

		UpdateGraphics();

		UpdateHandleRotation();

		DrawNormals(drawNormals);

#if !PROTOTYPE
		UpdateTextureHandles();
#endif

		currentHandleRotation = handleRotation;

		if(OnSelectionUpdate != null)
			OnSelectionUpdate(selection);

		// profiler.EndSample();
	}

	// Only updates things that absolutely need to be refreshed, and assumes that no selection changes have occured
	private void Internal_UpdateSelectionFast()
	{
		// profiler.BeginSample("Internal_UpdateSelectionFast");
		selectedVertexCount = 0;
		selectedFaceCount = 0;
		selectedEdgeCount = 0;

		bool boundsInitialized = false;
		Vector3 min = Vector3.zero, max = Vector3.zero;

		for(int i = 0; i < selection.Length; i++)
		{
			pb_Object pb = selection[i];

			if(pb == null) continue;

			// pb.transform.hasChanged = false;

			m_verticesInWorldSpace[i] = pb.VerticesInWorldSpace();	// to speed this up, could just get uniqueIndices vertiecs

			if(selection[i].SelectedTriangleCount > 0)
			{
				if(!boundsInitialized)
				{
					boundsInitialized = true;
					min = m_verticesInWorldSpace[i][selection[i].SelectedTriangles[0]];
					max = min;
				}

				for(int n = 0; n < selection[i].SelectedTriangleCount; n++)
				{
					min = Vector3.Min(min, m_verticesInWorldSpace[i][selection[i].SelectedTriangles[n]]);
					max = Vector3.Max(max, m_verticesInWorldSpace[i][selection[i].SelectedTriangles[n]]);
				}
			}

			selectedVertexCount += selection[i].SelectedTriangleCount;
			selectedFaceCount 	+= selection[i].SelectedFaceIndices.Length;
			selectedEdgeCount 	+= selection[i].SelectedEdges.Length;
		}

		m_handlePivotWorld = (max+min)/2f;

		UpdateGraphics();
		UpdateHandleRotation();
		currentHandleRotation = handleRotation;

		DrawNormals(drawNormals);

		if(OnSelectionUpdate != null)
			OnSelectionUpdate(selection);

		// profiler.EndSample();
	}

	private void UpdateGraphics()
	{
		// profiler.BeginSample("UpdateGraphics");
		graphics.RebuildGraphics(selection, m_universalEdges, editLevel, selectionMode);
		// profiler.EndSample();
	}

	public void AddToSelection(GameObject t)
	{
		if(t == null || Selection.objects.Contains(t))
			return;

		Object[] temp = new Object[Selection.objects.Length + 1];

		temp[0] = t;

		for(int i = 1; i < temp.Length; i++)
			temp[i] = Selection.objects[i-1];

		Selection.objects = temp;
	}

	public void RemoveFromSelection(GameObject t)
	{
		int ind = System.Array.IndexOf(Selection.objects, t);
		if(ind < 0)
			return;

		Object[] temp = new Object[Selection.objects.Length - 1];

		for(int i = 1; i < temp.Length; i++) {
			if(i != ind)
				temp[i] = Selection.objects[i];
		}

		Selection.objects = temp;
	}

	public void SetSelection(GameObject[] newSelection)
	{
		pbUndo.RecordSelection(selection, "Change Selection");

		ClearSelection();

		// if the previous tool was set to none, use Tool.Move
		if(Tools.current == Tool.None)
			Tools.current = Tool.Move;

		if(newSelection != null && newSelection.Length > 0) {
			Selection.activeTransform = newSelection[0].transform;
			Selection.objects = newSelection;
		}
		else
			Selection.activeTransform = null;
	}

	public void SetSelection(GameObject go)
	{
		pbUndo.RecordSelection(selection, "Change Selection");

		ClearSelection();
		AddToSelection(go);
	}

	/**
	 *	Clears all `selected` caches associated with each pb_Object in the current selection.  The means triangles, faces, and edges.
	 */
	public void ClearFaceSelection()
	{
		foreach(pb_Object pb in selection) {
			pb.ClearSelection();
		}

		nearestEdge = null;
		nearestEdgeObject = null;
	}

	public void ClearSelection()
	{
		foreach(pb_Object pb in selection) {
			pb.ClearSelection();
		}

		Selection.objects = new Object[0];
	}
#endregion

#region HANDLE AND GUI CALCULTATIONS

#if !PROTOTYPE

	Matrix4x4 handleMatrix = Matrix4x4.identity;

	private void UpdateTextureHandles()
	{
		if(selection.Length < 1) return;

		// Reset temp vars
		textureHandle = m_handlePivotWorld;
		textureScale = Vector3.one;
		textureRotation = Quaternion.identity;

		pb_Object pb;
		pb_Face face;

		handleMatrix = selection[0].transform.localToWorldMatrix;

		if( GetFirstSelectedFace(out pb, out face) )
		{
			Vector3 nrm, bitan, tan;
			pb_Math.NormalTangentBitangent(pb, face, out nrm, out tan, out bitan);

			if(nrm == Vector3.zero || bitan == Vector3.zero)
			{
				nrm = Vector3.up;
				bitan = Vector3.right;
				tan = Vector3.forward;
			}

			handleMatrix *= Matrix4x4.TRS( pb_Math.BoundsCenter( pb.vertices.ValuesWithIndices(face.distinctIndices) ), Quaternion.LookRotation(nrm, bitan), Vector3.one);
		}
	}
#endif

	Quaternion handleRotation = new Quaternion(0f, 0f, 0f, 1f);
	public void UpdateHandleRotation()
	{
		Quaternion localRot = Selection.activeTransform == null ? Quaternion.identity : Selection.activeTransform.rotation;

		switch(handleAlignment)
		{
			case HandleAlignment.Plane:

				if( Selection.transforms.Length > 1 )
					goto case HandleAlignment.World;

				pb_Object pb;
				pb_Face face;

				if( !GetFirstSelectedFace(out pb, out face) )
					goto case HandleAlignment.Local;

				// use average normal, tangent, and bitangent to calculate rotation relative to local space
				Vector3 nrm, bitan, tan;
				pb_Math.NormalTangentBitangent(pb, face, out nrm, out tan, out bitan);

				if(nrm == Vector3.zero || bitan == Vector3.zero)
				{
					nrm = Vector3.up;
					bitan = Vector3.right;
					tan = Vector3.forward;
				}

				handleRotation = localRot * Quaternion.LookRotation(nrm, bitan);
				break;

			case HandleAlignment.Local:
				handleRotation = localRot;
				break;

			case HandleAlignment.World:
				handleRotation = Quaternion.identity;
				break;
		}
	}

	/**
	 * Find the nearest vertex among all visible objects.
	 */
	private bool FindNearestVertex(Vector2 mousePosition, out Vector3 vertex)
	{
		List<Transform> t = new List<Transform>((Transform[])pbUtil.GetComponents<Transform>(HandleUtility.PickRectObjects(new Rect(0,0,Screen.width,Screen.height))));

		GameObject nearest = HandleUtility.PickGameObject(mousePosition, false);
		if(nearest != null)
			t.Add(nearest.transform);

		object[] parameters = new object[] { (Vector2)mousePosition, t.ToArray(), null };

		if(findNearestVertex == null)
			findNearestVertex = typeof(HandleUtility).GetMethod("findNearestVertex", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);

		object result = findNearestVertex.Invoke(this, parameters);
		vertex = (bool)result ? (Vector3)parameters[2] : Vector3.zero;
		return (bool)result;
	}
#endregion

#region Selection Management and checks

	/**
	 * If dragging a texture aroudn, this method ensures that if it's a member of a texture group it's cronies are also selected
	 */
	private void VerifyTextureGroupSelection()
	{
		foreach(pb_Object pb in selection)
		{
			List<int> alreadyChecked = new List<int>();

			foreach(pb_Face f in pb.SelectedFaces)
			{
				int tg = f.textureGroup;
				if(tg > 0 && !alreadyChecked.Contains(f.textureGroup))
				{
					foreach(pb_Face j in pb.faces)
						if(j != f && j.textureGroup == tg && !pb.SelectedFaces.Contains(j))
						{
							// int i = EditorUtility.DisplayDialogComplex("Mixed Texture Group Selection", "One or more of the faces selected belong to a Texture Group that does not have all it's member faces selected.  To modify, please either add the remaining group faces to the selection, or remove the current face from this smoothing group.", "Add Group to Selection", "Cancel", "Remove From Group");
							int i = 0;
							switch(i)
							{
								case 0:
									List<pb_Face> newFaceSection = new List<pb_Face>();
									foreach(pb_Face jf in pb.faces)
										if(jf.textureGroup == tg)
											newFaceSection.Add(jf);
									pb.SetSelectedFaces(newFaceSection.ToArray());
									UpdateSelection(false);
									break;

								case 1:
									break;

								case 2:
									f.textureGroup = 0;
									break;
							}
							break;
						}
				}
				alreadyChecked.Add(f.textureGroup);
			}
		}
	}
#endregion

#region EVENTS AND LISTENERS

	private void OnSelectionChange()
	{
		nearestEdge = null;
		nearestEdgeObject = null;

		UpdateSelection(false);

		HideSelectedWireframe();
	}

	/**
	 * Hide the default unity wireframe renderer
	 */
	private void HideSelectedWireframe()
	{
		foreach(pb_Object pb in selection)
		{
			Renderer ren = pb.gameObject.GetComponent<Renderer>();
			EditorUtility.SetSelectedWireframeHidden(ren, true);
		}

		SceneView.RepaintAll();
	}

	/**
	 * Registered to EditorApplication.onPlaymodeStateChanged
	 */
	private void OnPlayModeStateChanged()
	{
		if(EditorApplication.isPlaying)
		{
			foreach(pb_Entity entity in FindObjectsOfType(typeof(pb_Entity)))
			{
				switch(entity.entityType)
				{
					case EntityType.Occluder:
					case EntityType.Detail:
						if(!show_Detail)
						{
							entity.transform.GetComponent<MeshRenderer>().enabled = true;
							entity.transform.GetComponent<Collider>().enabled = true;
						}
						break;

					case EntityType.Mover:
						if(!show_Mover)
						{
							entity.transform.GetComponent<MeshRenderer>().enabled = true;
							entity.transform.GetComponent<Collider>().enabled = true;
						}
						break;

					case EntityType.Collider:
						if(!show_Collider)
						{
							entity.transform.GetComponent<Collider>().enabled = true;
						}
						break;

					case EntityType.Trigger:
						if(!show_Trigger)
						{
							entity.transform.GetComponent<Collider>().enabled = true;
						}
						break;
				}
			}

		}
		else
		{
			// Turn stuff back off that's not supposed to be on
			foreach(pb_Entity entity in FindObjectsOfType(typeof(pb_Entity)))
			{
				switch(entity.entityType)
				{
					case EntityType.Occluder:
					case EntityType.Detail:
						if(!show_Detail)
						{
							entity.transform.GetComponent<MeshRenderer>().enabled = false;
							entity.transform.GetComponent<Collider>().enabled = false;
						}
						break;

					case EntityType.Mover:
						if(!show_Mover)
						{
							entity.transform.GetComponent<MeshRenderer>().enabled = false;
							entity.transform.GetComponent<Collider>().enabled = false;
						}
						break;

					case EntityType.Collider:
						if(!show_Collider)
						{
							entity.transform.GetComponent<MeshRenderer>().enabled = false;
							entity.transform.GetComponent<Collider>().enabled = false;
						}
						break;

					case EntityType.Trigger:
						if(!show_Trigger)
						{
							entity.transform.GetComponent<MeshRenderer>().enabled = false;
							entity.transform.GetComponent<Collider>().enabled = false;
						}
						break;
				}
			}
		}
	}

	void UndoRedoPerformed()
	{
		pb_Object[] pbos = pbUtil.GetComponents<pb_Object>(Selection.transforms);

		foreach(pb_Object pb in pbos)
		{
			/**
			 * because undo after subdivide causes verify to fire, the face references aren't the same anymoore - so reset them
			 */
			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();

			if( pb.SelectedFaces.Length > 0 )
				pb.SetSelectedFaces( System.Array.FindAll( pb.faces, x => pbUtil.ContainsMatch(x.distinctIndices, pb_Face.AllTriangles(pb.SelectedFaces)) ) );
		}

		UpdateSelection(true);
		SceneView.RepaintAll();
	}

	/**
	 * Called from ProGrids.
	 */
	private void PushToGrid(float snapVal)
	{
		pbUndo.RecordObjects(selection, "Push elements to Grid");

		if( editLevel == EditLevel.Top )
			return;

		for(int i = 0; i  < selection.Length; i++)
		{
			pb_Object pb = selection[i];

			int[] indices = pb.SelectedTriangleCount > 0 ? pb.sharedIndices.AllIndicesWithValues(pb.SelectedTriangles).ToArray() : pb.msh.triangles;

			pbVertexOps.Quantize(pb, indices, Vector3.one * snapVal);

			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();
		}

		Internal_UpdateSelectionFast();
	}

	private void ProGridsToolbarOpen(bool menuOpen)
	{
		bool active = pb_ProGrids_Interface.ProGridsActive();
		sceneInfoRect.y = active && !menuOpen ? 28 : 10;
		sceneInfoRect.x = active ? (menuOpen ? 64 : 8) : 10;
	}

	/**
	 *	A tool, any tool, has just been engaged while in texture mode
	 */
	public void OnBeginTextureModification()
	{
		VerifyTextureGroupSelection();
	}

	/**
	 * When beginning a vertex modification, nuke the UV2 and rebuild the mesh
	 * using PB data so that triangles match vertices (and no inserted vertices
	 * from the Unwrapping.GenerateSecondaryUVSet() remain).
	 */
	private void OnBeginVertexMovement()
	{
		switch(currentHandle)
		{
			case Tool.Move:
				pbUndo.RegisterCompleteObjectUndo(selection, "Translate Vertices");
				break;

			case Tool.Rotate:
				pbUndo.RegisterCompleteObjectUndo(selection, "Rotate Vertices");
				break;

			case Tool.Scale:
				pbUndo.RegisterCompleteObjectUndo(selection, "Scale Vertices");
				break;

			default:
				pbUndo.RegisterCompleteObjectUndo(selection, "Modify Vertices");
				break;
		}

		pref_snapEnabled = pb_ProGrids_Interface.SnapEnabled();
		pref_snapValue = pb_ProGrids_Interface.SnapValue();
		pref_snapAxisConstraints = pb_ProGrids_Interface.UseAxisConstraints();

		// Disable iterative lightmapping
		// pb_Lightmapping.PushGIWorkflowMode();

		// profiler.BeginSample("ResetMesh");
		foreach(pb_Object pb in selection)
			pb.ResetMesh();

		// profiler.EndSample();
	}

	private void OnFinishVertexModification()
	{
		// pb_Lightmapping.PopGIWorkflowMode();

		currentHandleScale = Vector3.one;
		currentHandleRotation = handleRotation;

#if !PROTOTYPE
		if(movingPictures)
		{
			if(pb_UV_Editor.instance != null)
				pb_UV_Editor.instance.OnFinishUVModification();

			UpdateTextureHandles();

			movingPictures = false;
		}
		else
#endif
		if(movingVertices)
		{
			foreach(pb_Object sel in selection)
			{

				sel.ToMesh();
				sel.Refresh();
				sel.Optimize();
			}

			movingVertices = false;
		}

		if(OnVertexMovementFinished != null)
			OnVertexMovementFinished(selection);

		DrawNormals(drawNormals);
		scaling = false;
	}
#endregion

#region DEBUG

	static readonly Color[] ElementColors = new Color[] {
		new Color(.1f, .9f, .1f, .8f),	// Green (normal)
		// new Color(.1f, .1f, .9f, .3f),	// Blue (bitangent)
		// new Color(.9f, .1f, .1f, .3f),	// Red (tangent)
	};
	float elementLength = 0f;

	/**
	 * Draw vertex normals, tangents, and bitangents.
	 */
	void DrawNormals(float dist)
	{
		if(dist <= Mathf.Epsilon || movingVertices)
		{
			if(elementLength > 0f)
			{
				elementLength = 0f;
				pb_LineRenderer.instance.Clear();
				SceneView.RepaintAll();
			}

			return;
		}

		float elementOffset = .01f;
		elementLength = dist;

		pb_LineRenderer.instance.Clear();

		foreach(pb_Object pb in selection)
		{
			Mesh m = pb.msh;
			int vertexCount = m.vertexCount;

			Vector3[] vertices = m.vertices;
			Vector3[] normals  = m.normals;
			// Vector4[] tangents = m.tangents;

			Matrix4x4 matrix = pb.transform.localToWorldMatrix;

			// Vector3[] segments = new Vector3[vertexCount * 3 * 2];
			Vector3[] segments = new Vector3[vertexCount * 2];

			int n = 0;
			Vector3 pivot = Vector3.zero;

			for(int i = 0; i < vertexCount; i++)
			{
				pivot = vertices[i] + normals[i] * elementOffset;

				segments[n++] = matrix.MultiplyPoint3x4( pivot );
				segments[n++] = matrix.MultiplyPoint3x4( (pivot + normals[i] * elementLength) );

				// segments[n++] = segments[n];
				// segments[n++] = matrix.MultiplyPoint3x4( (pivot + (Vector3)tangents[i] * elementLength) );
				// segments[n++] = segments[n];
				// segments[n++] = matrix.MultiplyPoint3x4( (pivot + (Vector3.Cross(normals[i], (Vector3)tangents[i]) * tangents[i].w) * elementLength) );
			}

			pb_LineRenderer.instance.AddLineSegments(segments, ElementColors);
		}
	}

#endregion

#region CONVENIENCE CALLS

	/**
	 * Returns the first selected pb_Object and pb_Face, or false if not found.
	 */
	public bool GetFirstSelectedFace(out pb_Object pb, out pb_Face face)
	{
		pb = null;
		face = null;

		if(selection.Length < 1) return false;

		pb = selection.FirstOrDefault(x => x.SelectedFaceIndices.Length > 0);

		if(pb == null)
			return false;

		face = pb.faces[pb.SelectedFaceIndices[0]];

		return true;
	}

	/**
	 * Returns the first selected pb_Object and pb_Face, or false if not found.
	 */
	public bool GetFirstSelectedMaterial(ref Material mat)
	{
		for(int i = 0; i < selection.Length; i++)
		{
			for(int n = 0; n < selection[i].SelectedFaceIndices.Length; n++)
			{
				if(selection[i].faces[selection[i].SelectedFaceIndices[n]].material != null)
				{
					mat = selection[i].faces[selection[i].SelectedFaceIndices[n]].material;
					return true;
				}
			}
		}
		return false;
	}

	// Handy calls -- currentEvent must be set, so only call in the OnGUI loop!
	public bool altClick 			{ get { return (currentEvent.alt); } }
	public bool leftClick 			{ get { return (currentEvent.type == EventType.MouseDown); } }
	public bool leftClickUp 		{ get { return (currentEvent.type == EventType.MouseUp); } }
	public bool contextClick 		{ get { return (currentEvent.type == EventType.ContextClick); } }
	public bool mouseDrag 			{ get { return (currentEvent.type == EventType.MouseDrag); } }
	public bool ignore 				{ get { return currentEvent.type == EventType.Ignore; } }
	public Vector2 mousePosition 	{ get { return currentEvent.mousePosition; } }
	public Vector2 eventDelta 		{ get { return currentEvent.delta; } }
	public bool rightClick 			{ get { return (currentEvent.type == EventType.ContextClick); } }
	public bool shiftKey 			{ get { return currentEvent.shift; } }
	public bool ctrlKey 			{ get { return currentEvent.command || currentEvent.control; } }
	public KeyCode getKeyUp			{ get { return currentEvent.type == EventType.KeyUp ? currentEvent.keyCode : KeyCode.None; } }
#endregion

}
}

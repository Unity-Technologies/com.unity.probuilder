// #pragma warning disable 0414
#pragma warning disable 0168	///< Disable unused var (that exception hack)

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ProBuilder2.Math;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;
using System.Reflection;
using ProBuilder2.GUI;

#if PB_DEBUG
using Parabox.Debug;
#endif

/**
 * Space Conversion:
 * 	- UV - The actual UV coordinates.
 * 	- Canvas - uv * uvGridSize
 * 	- GUI - (canvas * uvGraphScale) + GUI offset
 */

#if !PROTOTYPE
public class pb_UV_Editor : EditorWindow
{

#region DEBUG
   
    #if PB_DEBUG
    static pb_Profiler profiler = new pb_Profiler();
    #endif
#endregion

#region CONST & PREF

	pb_Editor editor { get { return pb_Editor.instance; } }

	const int WINDOW_HEADER_OFFSET = 48;

	public static pb_UV_Editor instance;

	const int LEFT_MOUSE_BUTTON = 0;
	const int RIGHT_MOUSE_BUTTON = 1;
	const int MIDDLE_MOUSE_BUTTON = 2;
	const int PAD = 4;
	const float SCROLL_MODIFIER = .5f;
	const float ALT_SCROLL_MODIFIER = .07f;
	const int DOT_SIZE = 6;
	const int HALF_DOT = 3;
	const int HANDLE_SIZE = 128;
	const int MIN_ACTION_WINDOW_SIZE = 128;
	const float MAX_GRAPH_SCALE = 15f;

	const float MAX_PROXIMITY_SNAP_DIST_UV = .15f; 		///< The maximum allowable distance magnitude between coords to be considered for proximity snapping (UV coordinates)
	const float MAX_PROXIMITY_SNAP_DIST_CANVAS = 12f;	///< The maximum allowable distance magnitude between coords to be considered for proximity snapping (Canvas coordinates)
	const float MIN_DIST_MOUSE_EDGE = 8f;

	const int ACTION_WINDOW_WIDTH_MANUAL = 128;
	const int ACTION_WINDOW_WIDTH_AUTO = 200;

	private float pref_gridSnapValue = .0625f;

	Color DRAG_BOX_COLOR_BASIC 	= new Color(0f, .7f, 1f, .2f);
	Color DRAG_BOX_COLOR_PRO 	= new Color(0f, .7f, 1f, 1f);
	Color DRAG_BOX_COLOR;

	Color HOVER_COLOR_MANUAL 	= new Color(1f, .68f, 0f, .23f);
	Color HOVER_COLOR_AUTO 		= new Color(0f, 1f, 1f, .23f);

	Color SELECTED_COLOR_MANUAL = new Color(1f, .68f, 0f, .39f);
	Color SELECTED_COLOR_AUTO	= new Color(0f, .785f, 1f, .39f);

	#if UNITY_STANDALONE_OSX
	public bool ControlKey { get { return Event.current.modifiers == EventModifiers.Command; } }
	#else
	public bool ControlKey { get { return Event.current.modifiers == EventModifiers.Control; } }
	#endif
	public bool ShiftKey { get { return Event.current.modifiers == EventModifiers.Shift; } }

	private bool pref_showMaterial = true;	///< Show a preview texture for the first selected face in UV space 0,1?
#endregion

#region GUI Properties

	Color GridColorPrimary;
	Color UVColorPrimary, UVColorSecondary;
	Texture2D dot;
	Texture2D icon_textureMode_on, icon_textureMode_off;
	Texture2D icon_sceneUV_on, icon_sceneUV_off;

	GUIContent gc_SceneViewUVHandles = new GUIContent("", (Texture2D)null, "Lock the SceneView handle tools to UV manipulation mode.  This allows you to move UV coordinates directly on your 3d object.");
	GUIContent gc_ShowPreviewTexture = new GUIContent("", (Texture2D)null, "When toggled on, a preview image of the first selected face's material will be drawn from coordinates 0,0 - 1,1.\n\nNote that this depends on the Material's shader having a _mainTexture property.");

	GUIContent gc_ConvertToManual = new GUIContent("Convert to Manual", "There are 2 methods of unwrapping UVs in ProBuilder; Automatic unwrapping and Manual.  Auto unwrapped UVs are generated dynamically using a set of parameters, which may be set.  Manual UVs are akin to traditional UV unwrapping, in that once you set them they will not be updated as your mesh changes.");
	GUIContent gc_ConvertToAuto = new GUIContent("Convert to Auto", "There are 2 methods of unwrapping UVs in ProBuilder; Automatic unwrapping and Manual.  Auto unwrapped UVs are generated dynamically using a set of parameters, which may be set.  Manual UVs are akin to traditional UV unwrapping, in that once you set them they will not be updated as your mesh changes.");
#endregion

#region Properties

	private int uvGridSize = 256;	// Full grid size in pixels (-1, 1)
	private float uvGraphScale = 1f;

	private pb_Bounds2D selected_canvas_bounds = new pb_Bounds2D(Vector2.zero, Vector2.zero);
	enum UVMode 
	{
		Auto,
		Manual,
		Mixed
	};
	UVMode mode = UVMode.Auto;

	#if PB_DEBUG
	int[] UV_CHANNELS = new int[] { 0, 1 };
	#endif
	
	int channel = 0;	// Maybe someday Unity will support > 2 UV channels?

	private Vector2 uvCanvasOffset = Vector2.zero;

	// All UV coordinates
	pb_Object[] selection;
	int[][] distinct_indices;
	Vector2[][] uvs_gui_space;	// mirror of each selected pbo's UV coorindates in GUI window coordinates

	int selectedUVCount = 0;
	int selectedFaceCount = 0;
	// int selectedEdgeCount = 0;
	int screenWidth, screenHeight;

	// Modifying state
	bool modifyingUVs = false;

	Material preview_material;		///< The first selected face's material.  Used to draw texture preview in 0,0 - 1,1 space.

	// Tooools
	Tool tool = Tool.Move;
	SelectMode selectionMode { get { return editor != null ? editor.selectionMode : SelectMode.Face; } set { if(editor) editor.SetSelectionMode(value); } }

	GUIContent[] ToolIcons;
	GUIContent[] SelectionIcons;

	struct ObjectElementIndex
	{
		public int objectIndex;
		public int elementIndex;
		public int elementSubIndex;
		public bool valid;

		public void Clear()
		{
			this.objectIndex = -1;
			this.elementIndex = -1;
			this.elementSubIndex = -1;
			this.valid = false;
		}

		public ObjectElementIndex(int obj, int elem, int sub)
		{
			this.objectIndex = obj;
			this.elementIndex = elem;
			this.elementSubIndex = sub;
			this.valid = false;
		}

		public bool Equals(ObjectElementIndex oei)
		{
			return 	this.objectIndex == oei.objectIndex &&
					this.elementIndex == oei.elementIndex &&
					this.elementSubIndex == oei.elementSubIndex && 
					this.valid == oei.valid;
		}

		public override string ToString()
		{
			return valid ? objectIndex + " : " + elementIndex + " -> " + elementSubIndex : "Invalid";
		}
	}

	ObjectElementIndex nearestElement = new ObjectElementIndex(-1, -1, -1);
#endregion

#region Menu

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/UV Editor Window", false, pb_Constant.MENU_WINDOW + 0)]
	public static void MenuOpenUVEditor()
	{
		if(pb_Editor.instance != null && pb_Editor.instance.editLevel == EditLevel.Top)
			pb_Editor.instance.SetEditLevel(EditLevel.Geometry);

		EditorWindow.GetWindow<pb_UV_Editor>(pb_Preferences_Internal.GetBool(pb_Constant.pbUVEditorFloating), "UV Editor", true);		 
	}

	void OpenContextMenu()
	{
		GenericMenu menu = new GenericMenu();

		menu.AddItem (new GUIContent("Selection/Select Island", ""), false, Menu_SelectUVIsland);
		menu.AddItem (new GUIContent("Selection/Select Face", ""), false, Menu_SelectUVFace);

		menu.AddSeparator("");

		menu.AddItem (new GUIContent("Window/Open as Floating Window", ""), false, ContextMenu_OpenFloatingWindow);
		menu.AddItem (new GUIContent("Window/Open as Dockable Window", ""), false, ContextMenu_OpenDockableWindow);

		menu.ShowAsContext ();
	}

	static void ContextMenu_OpenFloatingWindow()
	{
		EditorPrefs.SetBool(pb_Constant.pbUVEditorFloating, true);

		EditorWindow.GetWindow<pb_UV_Editor>().Close();
		EditorWindow.GetWindow<pb_UV_Editor>(true, "UV Editor", true);
	}

	static void ContextMenu_OpenDockableWindow()
	{
		EditorPrefs.SetBool(pb_Constant.pbUVEditorFloating, false);

		EditorWindow.GetWindow<pb_UV_Editor>().Close();
		EditorWindow.GetWindow<pb_UV_Editor>(false, "UV Editor", true);
	}
#endregion

#region Enable

	void OnEnable()
	{
		this.minSize = new Vector2(500f, 300f);
		
		InitGUI();

		/**
		 * Register for delegates
		 */
		pb_Editor.OnSelectionUpdate += OnSelectionUpdate;
		if(editor != null) OnSelectionUpdate(editor.selection);

		instance = this;

		pb_Object_Editor.OnGetFrameBoundsEvent += OnGetFrameBoundsEvent;
		
		nearestElement.Clear();

		// Find preferences
		pref_showMaterial = pb_Preferences_Internal.GetBool(pb_Constant.pbUVMaterialPreview);
		pref_gridSnapValue = pb_Preferences_Internal.GetFloat(pb_Constant.pbUVGridSnapValue);
	}

	void OnDisable()
	{
		instance = null;

		if(editor && editor.editLevel == EditLevel.Texture)
			editor.PopEditLevel();	

		// EditorApplication.delayCall -= this.Close;							// not sure if this is necessary?
		pb_Editor.OnSelectionUpdate -= OnSelectionUpdate;
		pb_Object_Editor.OnGetFrameBoundsEvent -= OnGetFrameBoundsEvent;
	}

	/**
	 * Loads icons, sets default colors using prefs, etc.
	 */
	void InitGUI()
	{
		DRAG_BOX_COLOR = EditorGUIUtility.isProSkin ? DRAG_BOX_COLOR_PRO : DRAG_BOX_COLOR_BASIC;
		GridColorPrimary = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, .2f) : new Color(0f, 0f, 0f, .2f);
		UVColorPrimary = EditorGUIUtility.isProSkin ? Color.green : new Color(0f, .8f, 0f, 1f);
		UVColorSecondary = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, .7f) : Color.blue;
		// UVColorSecondary = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, .7f) : new Color(0f, 0f, 0f, .3f);
		dot = EditorGUIUtility.whiteTexture;


		MethodInfo loadIconMethod = typeof(EditorGUIUtility).GetMethod("LoadIcon", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
		
		Texture2D moveIcon = (Texture2D)loadIconMethod.Invoke(null, new object[] {"MoveTool"} );
		Texture2D rotateIcon = (Texture2D)loadIconMethod.Invoke(null, new object[] {"RotateTool"} );
		Texture2D scaleIcon = (Texture2D)loadIconMethod.Invoke(null, new object[] {"ScaleTool"} );
		Texture2D viewIcon = (Texture2D)loadIconMethod.Invoke(null, new object[] {"ViewToolMove"} );

		Texture2D face_Graphic_off = (Texture2D)(Resources.Load(EditorGUIUtility.isProSkin ? "GUI/ProBuilderGUI_Mode_Face-Off_Small-Pro" : "GUI/ProBuilderGUI_Mode_Face-Off_Small", typeof(Texture2D)));
		Texture2D vertex_Graphic_off = (Texture2D)(Resources.Load(EditorGUIUtility.isProSkin ? "GUI/ProBuilderGUI_Mode_Vertex-Off_Small-Pro" : "GUI/ProBuilderGUI_Mode_Vertex-Off_Small", typeof(Texture2D)));
		Texture2D edge_Graphic_off = (Texture2D)(Resources.Load(EditorGUIUtility.isProSkin ? "GUI/ProBuilderGUI_Mode_Edge-Off_Small-Pro" : "GUI/ProBuilderGUI_Mode_Edge-Off_Small", typeof(Texture2D)));

		icon_textureMode_on		= (Texture2D)(Resources.Load("GUI/ProBuilderGUI_UV_ShowTexture_On", typeof(Texture2D)));
		icon_textureMode_off	= (Texture2D)(Resources.Load("GUI/ProBuilderGUI_UV_ShowTexture_Off", typeof(Texture2D)));

		icon_sceneUV_on			= (Texture2D)(Resources.Load("GUI/ProBuilderGUI_UV_Manip_On", typeof(Texture2D)));
		icon_sceneUV_off		= (Texture2D)(Resources.Load("GUI/ProBuilderGUI_UV_Manip_Off", typeof(Texture2D)));

		ToolIcons = new GUIContent[4]
		{
			new GUIContent(viewIcon, "View Tool"),
			new GUIContent(moveIcon, "Move Tool"),
			new GUIContent(rotateIcon, "Rotate Tool"),
			new GUIContent(scaleIcon, "Scale Tool")
		};

		SelectionIcons = new GUIContent[3]
		{
			new GUIContent(vertex_Graphic_off, "Vertex Selection"),
			new GUIContent(edge_Graphic_off, "Edge Selection"),
			new GUIContent(face_Graphic_off, "Face Selection")
		};
	}
#endregion

#region GUI Loop

	void Update()
	{
		if( selectionMode == SelectMode.Edge ||
			selectionMode == SelectMode.Face ||
			m_rightMouseDrag )
			Repaint();
	}

	Rect 	graphRect,
			toolbarRect, 
			actionWindowRect = new Rect(6, 64, 128, 200);

	#if PB_DEBUG
	Rect buggerRect;
	#endif

	Vector2 mousePosition_initial;

	Rect dragRect = new Rect(0,0,0,0);
	bool m_mouseDragging = false;

	bool needsRepaint = false;

	void OnGUI()
	{
		if(tool == Tool.View || m_draggingCanvas)	
			EditorGUIUtility.AddCursorRect(new Rect(0,toolbarRect.y + toolbarRect.height,screenWidth,screenHeight), MouseCursor.Pan);

		#if PB_DEBUG
		profiler.BeginSample("pb_UV_Editor::OnGUI");
		profiler.BeginSample("GUI Calculations");
		#endif

		if(Screen.width != screenWidth || Screen.height != screenHeight)
			OnScreenResize();

		toolbarRect = new Rect(PAD, PAD, Screen.width-PAD*2, 29);
		graphRect = new Rect(PAD, PAD, Screen.width-PAD*2, Screen.height-PAD*2);

		actionWindowRect.x = (int)Mathf.Clamp(actionWindowRect.x, PAD, Screen.width-PAD-PAD-actionWindowRect.width);
		actionWindowRect.y = (int)Mathf.Clamp(actionWindowRect.y, PAD, Screen.height-MIN_ACTION_WINDOW_SIZE);
		actionWindowRect.height = (int)Mathf.Min(Screen.height - actionWindowRect.y - 24, 350);
		switch(mode)
		{
			case UVMode.Manual:
			case UVMode.Mixed:
				actionWindowRect.width = ACTION_WINDOW_WIDTH_MANUAL;
				break;

			case UVMode.Auto:
				actionWindowRect.width = ACTION_WINDOW_WIDTH_AUTO;
				break;
		}

		#if PB_DEBUG
		profiler.EndSample();
		profiler.BeginSample("HandleInput");
		#endif

		// Mouse drags, canvas movement, etc
		HandleInput();
		
		#if PB_DEBUG
		profiler.EndSample();
		profiler.BeginSample("DrawUVGraph");
		#endif

		DrawUVGraph( graphRect );

		#if PB_DEBUG
		profiler.EndSample();
		profiler.BeginSample("Tools");
		#endif

		// Draw AND update translation handles
		if(selection != null && selectedUVCount > 0)
		{
			switch(tool)
			{
				case Tool.Move:
					MoveTool();
					break;

				case Tool.Rotate:
					RotateTool();
					break;
			
				case Tool.Scale:
					ScaleTool();
					break;
			}
		}

		#if PB_DEBUG
		profiler.EndSample();
		profiler.BeginSample("UpdateNearestElement");
		#endif

		UpdateNearestElement(Event.current.mousePosition);
		
		#if PB_DEBUG
		profiler.EndSample();
		profiler.BeginSample("MouseDrag");
		#endif

		if(m_mouseDragging && !modifyingUVs && !m_draggingCanvas && !m_rightMouseDrag)
		{
			Color oldColor = GUI.backgroundColor;
			GUI.backgroundColor = DRAG_BOX_COLOR;
			GUI.Box(dragRect, "");
			GUI.backgroundColor = oldColor;
		}

		#if PB_DEBUG
		profiler.EndSample();
		profiler.BeginSample("DrawUVTools");
		#endif

		DrawUVTools(toolbarRect);
		
		#if PB_DEBUG
		profiler.EndSample();
		profiler.BeginSample("DrawActionWindow");
		#endif

		BeginWindows();
			actionWindowRect = GUILayout.Window( 1, actionWindowRect, DrawActionWindow, "Actions" );
		EndWindows();

		#if PB_DEBUG
		profiler.EndSample();
		#endif

		if(needsRepaint)
		{
			Repaint();
			needsRepaint = false;
		}

		#if PB_DEBUG
		profiler.EndSample();
		#endif

		#if PB_DEBUG
		buggerRect = new Rect(Screen.width - 226, PAD, 220, 300);
		DrawDebugInfo(buggerRect);
		#endif
	}
#endregion

#region Editor Delegate and Event

	void OnSelectionUpdate(pb_Object[] selection)
	{	
		this.selection = selection;

		SetSelectedUVsWithSceneView();

		RefreshUVCoordinates();

		Repaint();
	}

	/**
	 * Automatically select textureGroup kin, and copy origins of all UVs.
	 * Also resets the mesh to PB data - removing vertices appended by 
	 * UV2 generation.
	 */
	internal void OnBeginUVModification()
	{
		modifyingUVs = true;

		Vector2 handle = handlePosition_canvas;
		bool update = false;

		for(int i = 0; i < selection.Length; i++)
		{
			if(selection[i].SelectedFaceCount > 0)
			{
				int fc = selection[i].SelectedFaceCount;
				selection[i].SetSelectedFaces( SelectTextureGroups(selection[i], selection[i].SelectedFaces) );

				// kinda lame... this will cause setSelectedUVsWithSceneView to be called again.
				if(fc != selection[i].SelectedFaceCount)
					update = true;
			}

			selection[i].ToMesh();	 // Reset the Mesh to PB data only.
			selection[i].Refresh();
		}

		if(update)
		{
			editor.UpdateSelection();
			SetHandlePosition(handle, true);
		}

		CopySelectionUVs(out uv_origins);
		uvOrigin = pb_Handle_Utility.GUIToUVPoint(handle, uvGridSize);
	}

	/**
	 * Internal because pb_Editor needs to call this sometimes.
	 */
	internal void OnFinishUVModification()
	{
		modifyingUVs = false;

		if((tool == Tool.Rotate || tool == Tool.Scale) && userPivot)
		{
			selected_canvas_bounds = CanvasSelectionBounds();
			SetHandlePosition(handlePosition_canvas, true);
		}

		if(mode == UVMode.Mixed || mode == UVMode.Auto)
		{
			pbUndo.RecordObjects(selection, "Apply AutoUV Transform");

			foreach(pb_Object pb in selection)
			{
				if(pb.SelectedFaceCount > 0)
				{
					/**
					 * Sort faces into texture groups for re-projection
					 */
					Dictionary<int, List<pb_Face>> textureGroups = new Dictionary<int, List<pb_Face>>();

					int n = -2;
					foreach(pb_Face face in System.Array.FindAll(pb.SelectedFaces, x => !x.manualUV))
					{
						if(textureGroups.ContainsKey(face.textureGroup))
							textureGroups[face.textureGroup].Add(face);
						else
							textureGroups.Add( face.textureGroup > 0 ? face.textureGroup : n--, new List<pb_Face>() {face} );
					}

					foreach(KeyValuePair<int, List<pb_Face>> kvp in textureGroups)
					{
						/**
						 * Rotation - only applies to rotation tool
						 */
						if(tool == Tool.Rotate)
						{
							foreach(pb_Face face in kvp.Value)
							{
								if((face.uv.flipU ^ face.uv.flipV) ^ face.uv.swapUV) 
									uvRotation = -uvRotation;

								face.uv.rotation += uvRotation;
								if(face.uv.rotation > 360f) face.uv.rotation = face.uv.rotation % 360f;
								if(face.uv.rotation < 0f) face.uv.rotation = 360f + (face.uv.rotation % 360f);
							}
						}

						/**
						 * Scale is applied in real-time
						 */

						/**
						 * Reproject because uv.localPivot needs to be accurate for this to work properly
						 */
						foreach(pb_Face face in kvp.Value)
							face.uv.offset = Vector2.zero;

						Vector3 nrm = Vector3.zero;
						foreach(pb_Face face in kvp.Value)
						{
							nrm += pb_Math.Normal( 	pb.vertices[face.indices[0]],
													pb.vertices[face.indices[1]],
													pb.vertices[face.indices[2]] ); 
						}

						nrm /= (float)kvp.Value.Count;

						int[] tris = pb_Face.AllTriangles(kvp.Value).ToArray();

						if(kvp.Value[0].uv.useWorldSpace)
						{
							pb.transform.TransformDirection(nrm);
							pb_UV_Utility.PlanarMap( pb.transform.ToWorldSpace(pb.GetVertices(tris)), kvp.Value[0].uv, nrm );
						}
						else
						{
							pb_UV_Utility.PlanarMap( pb.GetVertices(tris), kvp.Value[0].uv, nrm );
						}

						foreach(pb_Face face in kvp.Value)
							face.uv.localPivot = kvp.Value[0].uv.localPivot;

						/**
						 * Translation - applies for every tool
						 */
						Vector2 handle = pb_Handle_Utility.GUIToUVPoint(handlePosition_canvas, uvGridSize);
						Vector2 cen = pb_Bounds2D.Center(pb.GetUVs(tris));

						foreach(pb_Face face in kvp.Value)
							face.uv.offset = -((handle - face.uv.localPivot) - (handle-cen));

					}
				}
				else
				{
					FlagSelectedFacesAsManual(pb);
				}
			}
		}
		else if(mode == UVMode.Manual)
		{
			foreach(pb_Object pb in selection)
			{
				if(pb.SelectedFaceIndices.Length > 0)
				{
					foreach(pb_Face face in pb.SelectedFaces)
					{
						face.textureGroup = -1;
						face.manualUV = true;
					}
				}
				else
				{
					FlagSelectedFacesAsManual(pb);
				}
			}

		}

		// Regenerate UV2s
		foreach(pb_Object pb in selection)
		{
			pb.GenerateUV2();
		}
	}

	void SetSelectedUVsWithSceneView()
	{
		if(selection == null)
		{
			distinct_indices = new int[0][];
			return;
		}

		distinct_indices = new int[selection.Length][];

		// Append shared UV indices to SelectedTriangles array (if necessary)
		for(int i = 0; i < selection.Length; i++)
		{
			pb_IntArray[] sharedUVs = selection[i].sharedIndicesUV;
			
			List<int> selectedTris = new List<int>(selection[i].SelectedTriangles);

			/**
			 * Put sewn UVs into the selection if they aren't already.
			 */	
			if(sharedUVs != null)
			{
				foreach(int[] arr in sharedUVs)
				{
					if( System.Array.Exists(arr, element => System.Array.IndexOf(selection[i].SelectedTriangles, element) > -1 ) )
					{
						selectedTris.AddRange( arr );
					}
				}
			}


			distinct_indices[i] = selectedTris.Distinct().ToArray();
		}
	}

	void OnGetFrameBoundsEvent()
	{
		FrameSelection();
		Repaint();
	}

	void OnScreenResize()
	{
		screenWidth = Screen.width;
		screenHeight = Screen.height;
		RefreshUVCoordinates();
		Repaint();
	}

	/**
	 * return true if shortcut should eat the event
	 */
	internal bool ClickShortcutCheck(pb_Object pb, pb_Face selectedFace)
	{
		Event e = Event.current;

		if(e.modifiers == EventModifiers.Control)
		{
			int len = pb.SelectedFaces == null ? 0 : pb.SelectedFaces.Length;

			if(len < 1)
				return false;

			pb_Face anchor = pb.SelectedFaces[len-1];

			if(anchor == selectedFace) return false;

			pbUndo.RecordObject(pb, "AutoStitch");

			pb.ToMesh();

			bool success = pbUVOps.AutoStitch(pb, anchor, selectedFace);
			
			if(success)
			{	
				RefreshElementGroups(pb);

				pb.SetSelectedFaces(new pb_Face[]{selectedFace});

				// // only need to do this for one pb_Object...
				// for(int i = 0; i < selection.Length; i++)
				// 	selection[i].RefreshUV( editor.SelectedFacesInEditZone[i] );

				pb.Refresh();
				pb.GenerateUV2();

				SetSelectedUVsWithSceneView();

				RefreshUVCoordinates();

				pb_Editor_Utility.ShowNotification("Autostitch");

				if(editor != null)
					editor.UpdateSelection(false);

				Repaint();
			}
			else
			{
				pb.Refresh();
				pb.GenerateUV2();
			}

			return success;
		}

		return false;
	}
#endregion

#region Key and Handle Input

	bool m_ignore = false;
	bool m_rightMouseDrag = false;
	bool m_draggingCanvas = false;
	bool m_doubleClick = false;

	void HandleInput()
	{
		Event e = Event.current;

		if(e.isKey)
		{
			HandleKeyInput(e);
			return;
		}

		switch(e.type)
		{
			case EventType.MouseDown:			
				
				#if PB_DEBUG
				if(toolbarRect.Contains(e.mousePosition) || actionWindowRect.Contains(e.mousePosition) || buggerRect.Contains(e.mousePosition))
				#else
				if(toolbarRect.Contains(e.mousePosition) || actionWindowRect.Contains(e.mousePosition))
				#endif
				{
					m_ignore = true;
					return;
				}

				if(e.clickCount > 1)
					m_doubleClick = true;

				mousePosition_initial = e.mousePosition;

				break;

			case EventType.MouseDrag:

				if(m_ignore || (e.mousePosition.y <= toolbarRect.y && !m_mouseDragging))
					break;
				
				m_mouseDragging = true;

				if(e.button == RIGHT_MOUSE_BUTTON || (e.button == LEFT_MOUSE_BUTTON && e.alt))
					m_rightMouseDrag = true;
			
				/* If no handle is selected, do other stuff */
				if(pb_Handle_Utility.CurrentID < 0)
				{
					if( (e.alt && e.button == LEFT_MOUSE_BUTTON) || e.button == MIDDLE_MOUSE_BUTTON || Tools.current == Tool.View)
					{
						m_draggingCanvas = true;
						uvCanvasOffset += e.delta;
						needsRepaint = true;
					}
					else if(e.button == LEFT_MOUSE_BUTTON)
					{
						dragRect.x = mousePosition_initial.x < e.mousePosition.x ? mousePosition_initial.x : e.mousePosition.x;
						dragRect.y = mousePosition_initial.y > e.mousePosition.y ? e.mousePosition.y : mousePosition_initial.y;
						dragRect.width = Mathf.Abs(mousePosition_initial.x-e.mousePosition.x);
						dragRect.height = Mathf.Abs(mousePosition_initial.y-e.mousePosition.y);
						
						needsRepaint = true;
					}
					else if(e.alt && e.button == RIGHT_MOUSE_BUTTON)
					{
						SetCanvasScale(uvGraphScale + (e.delta.x - e.delta.y) * ((uvGraphScale/MAX_GRAPH_SCALE) * ALT_SCROLL_MODIFIER) );
						needsRepaint = true;
					}
				}
				break;

			case EventType.Ignore:
			case EventType.MouseUp:

				modifyingUVs_AutoPanel = false;

				if(m_ignore)
				{
					m_ignore = false;
					m_mouseDragging = false;
					m_draggingCanvas = false;
					m_doubleClick = false;
					needsRepaint = true;
					return;
				}

				if(e.button == LEFT_MOUSE_BUTTON && !m_rightMouseDrag && !modifyingUVs && !m_draggingCanvas)
				{
					Vector2 hp = handlePosition_canvas;

					if(m_mouseDragging)
					{
						OnMouseDrag();
					}
					else
					{
						if(e.modifiers == (EventModifiers)0 && editor)
							editor.ClearFaceSelection();

						OnMouseClick(e.mousePosition);

						if(m_doubleClick)
							SelectUVShell();
					}

					if(!e.shift || !userPivot)
						SetHandlePosition( selected_canvas_bounds.center, false );
					else
						SetHandlePosition( hp, true );
				}

				if(e.button != RIGHT_MOUSE_BUTTON)
					m_rightMouseDrag = false;

				m_mouseDragging = false;
				m_doubleClick = false;
				m_draggingCanvas = false;

				if(modifyingUVs)
					OnFinishUVModification();

				uvRotation = 0f;
				uvScale = Vector2.one;

				needsRepaint = true;
				break;

			case EventType.ScrollWheel:
				
				SetCanvasScale( uvGraphScale - e.delta.y * ((uvGraphScale/MAX_GRAPH_SCALE) * SCROLL_MODIFIER) );
				e.Use();
				
				needsRepaint = true;
				break;

			case EventType.ContextClick:
					
					if(!m_rightMouseDrag)
						OpenContextMenu();
					else
						m_rightMouseDrag = false;
					break;
					
			default:
				return;
		}
	}

	void HandleKeyInput(Event e)
	{
		if(e.type != EventType.KeyUp || !GUI.GetNameOfFocusedControl().Equals(""))
			return;

		switch(e.keyCode)
		{
			// case KeyCode.Keypad0: 
			// case KeyCode.Alpha0:
			// 	ResetCanvas();
			// 	e.Use();
			// 	needsRepaint = true;
			// 	break;

			case KeyCode.W:
				SetTool_Internal(Tool.Move);
				break;
			
			case KeyCode.E:
				SetTool_Internal(Tool.Rotate);
				break;

			case KeyCode.R:
				SetTool_Internal(Tool.Scale);
				break;

			case KeyCode.F:
				FrameSelection();
				break;

			// case KeyCode.J:	// Blech.
			// 	EditorApplication.delayCall += this.Close;
			// 	break;

			case KeyCode.H:
				if(editor)
					editor.ToggleSelectionMode();
				break;
		}
	}

	/**
	 * Finds the nearest edge to the mouse and sets the `nearestEdge` struct with it's info
	 */
	bool UpdateNearestElement(Vector2 mousePosition)
	{
		if(selection == null || m_mouseDragging || modifyingUVs || tool == Tool.View)// || pb_Handle_Utility.CurrentID > -1)
		{
			if(nearestElement.valid)
			{
				nearestElement.valid = false;
				return true;
			}
			else
			{
				return false;
			}
		}

		Vector2 mpos = GUIToCanvasPoint(mousePosition);
		ObjectElementIndex oei = nearestElement;
		nearestElement.valid = false;

		switch(selectionMode)
		{
			case SelectMode.Edge:
				float dist, best = 100f;

				try
				{
					for(int i = 0; i < selection.Length; i++)
					{
						pb_Object pb = selection[i];

						for(int n = 0; n < pb.faces.Length; n++)
						{
							for(int p = 0; p < pb.faces[n].edges.Length; p++)
							{
								Vector2 x = uvs_gui_space[i][pb.faces[n].edges[p].x];
								Vector2 y = uvs_gui_space[i][pb.faces[n].edges[p].y];

								dist = pb_Math.DistancePointLineSegment(mpos, x, y);

								if(dist < best)
								{
									nearestElement.objectIndex = i;
									nearestElement.elementIndex = n;
									nearestElement.elementSubIndex = p;
									best = dist;
								}
							}
						}
					}
				} catch(System.Exception e) {}

				nearestElement.valid = best < MIN_DIST_MOUSE_EDGE;
				break;

			case SelectMode.Face:

				try
				{
					bool superBreak = false;
					for(int i = 0; i < selection.Length; i++)
					{
						for(int n = 0; n < selection[i].faces.Length; n++)
						{
							if( pb_Math.PointInPolygon( pbUtil.ValuesWithIndices(uvs_gui_space[i], selection[i].faces[n].edges.AllTriangles()), mpos) )
							{
								nearestElement.objectIndex = i;
								nearestElement.elementIndex = n;
								nearestElement.elementSubIndex = -1;
								nearestElement.valid = true;
								superBreak = true;
								break;
							}
							if(superBreak) break;
						}
					}
				} catch(System.Exception e) {}
				break;
		}

		return !nearestElement.Equals(oei);
	}

	/**
	 * Allows another window to set the current tool.
	 * Does *not* update any other editor windows.
	 */
	public void SetTool(Tool tool)
	{
		this.tool = tool;
		nearestElement.Clear();
		Repaint();
	}

	/**
	 * Sets the global Tool.current and updates any other windows.
	 */
	private void SetTool_Internal(Tool tool)
	{
		SetTool(tool);

		if(tool == Tool.View)
			Tools.current = Tool.View;
		else
			Tools.current = Tool.None;	

		if(editor)
		{
			editor.SetTool(tool);
			SceneView.RepaintAll();
		}
	}

	/**
	 * Handle click check and updating selection.
	 */
	void OnMouseClick(Vector2 mousePosition)
	{
		if (selection == null) return;

		#if PB_DEBUG
		profiler.BeginSample("OnMouseClick");
		#endif

		pbUndo.RecordObjects(selection, "Change Selection");
		
		switch(selectionMode)
		{
			case SelectMode.Edge:
				if(nearestElement.valid)
				{
					pb_Object pb = selection[nearestElement.objectIndex];

					pb_Edge edge = pb.faces[nearestElement.elementIndex].edges[nearestElement.elementSubIndex];
					int ind = pb.SelectedEdges.IndexOf(edge, pb.sharedIndices);

					if( ind > -1 )
						pb.SetSelectedEdges(pb.SelectedEdges.RemoveAt(ind));
					else
						pb.SetSelectedEdges(pb.SelectedEdges.Add(edge));
				}
				break;

			case SelectMode.Face:

				Vector2 mpos = GUIToCanvasPoint(mousePosition);
				bool superBreak = false;
				for(int i = 0; i < selection.Length; i++)
				{
					// List<int> selectedFaces = new List<int>(selection[i].SelectedFaceIndices);
					List<pb_Face> selectedFaces = new List<pb_Face>(selection[i].SelectedFaces);
					for(int n = 0; n < selection[i].faces.Length; n++)
					{
						if( pb_Math.PointInPolygon( pbUtil.ValuesWithIndices(uvs_gui_space[i], selection[i].faces[n].edges.AllTriangles()), mpos) )
						{
							if( selectedFaces.Contains(selection[i].faces[n]) )
								selectedFaces.Remove(selection[i].faces[n]);
							else
								selectedFaces.Add(selection[i].faces[n]);

							// Only select one face per click
							superBreak = true;
							break;
						}
					}

					selection[i].SetSelectedFaces( selectedFaces.ToArray() );

					if(superBreak) break;
				}
				break;

			case SelectMode.Vertex:
				RefreshUVCoordinates( new Rect(mousePosition.x-8, mousePosition.y-8, 16, 16), true );
				break;
		}

		if(editor)
			editor.UpdateSelection(false);

		RefreshSelectedUVCoordinates();
		
		#if PB_DEBUG
		profiler.EndSample();
		#endif
	}

	void OnMouseDrag()
	{
		Event e = Event.current;
		if(editor && !e.shift && !e.control && !e.command)
			editor.ClearFaceSelection();

		RefreshUVCoordinates(dragRect, false);
		e.Use();
	}
#endregion

#region Tools

	// tool properties
	float uvRotation = 0f;
	Vector2 uvOrigin = Vector2.zero;

	Vector2[][] uv_origins = null;
	Vector2 handlePosition_canvas = Vector2.zero, handlePosition_offset = Vector2.zero;

	/**
	 * Draw an interactive 2d Move tool that affects the current selection of UV coordinates.
	 */
	void MoveTool()
	{
		Event e = Event.current;

		Vector2 t_handlePosition = handlePosition_canvas;

		#if PB_DEBUG
		profiler.BeginSample("Handle");
		#endif

		pb_Handle_Utility.limitToLeftButton = false; // enable right click drag
		t_handlePosition = GUIToCanvasPoint( pb_Handle_Utility.PositionHandle2d(1, CanvasToGUIPoint(handlePosition_canvas), HANDLE_SIZE) );
		pb_Handle_Utility.limitToLeftButton = true;

		#if PB_DEBUG
		profiler.EndSample();
		#endif

		if (!e.isMouse) return;

		/**
		 *	Setting a custom pivot
		 */
		if((e.button == RIGHT_MOUSE_BUTTON || (e.alt && e.button == LEFT_MOUSE_BUTTON)) && !pb_Math.EqualWithError(t_handlePosition, handlePosition_canvas, .9f))
		{
			#if PB_DEBUG
			profiler.BeginSample("Set Custom Pivot");
			#endif

			userPivot = true;	// flag the handle as having been user set.

			if(ControlKey)
			{
				Vector2 uv = pb_Handle_Utility.GUIToUVPoint( t_handlePosition, uvGridSize);
				handlePosition_canvas = pb_Handle_Utility.UVToGUIPoint(pbUtil.SnapValue(uv, handlePosition_canvas-t_handlePosition, pref_gridSnapValue), uvGridSize);
			}
			else
			{		
				handlePosition_canvas = t_handlePosition;

				/**
				 * Attempt vertex proximity snap if shift key isn't held
				 */
				if(!ShiftKey)
				{

					float dist, minDist = MAX_PROXIMITY_SNAP_DIST_CANVAS;
					Vector2 offset = Vector2.zero;
					for(int i = 0; i < selection.Length; i++)
					{
						int index = pb_Handle_Utility.NearestPoint( handlePosition_canvas, uvs_gui_space[i], MAX_PROXIMITY_SNAP_DIST_CANVAS);

						if(index < 0) continue;
						
						dist = Vector2.Distance( uvs_gui_space[i][index], handlePosition_canvas );

						if(dist < minDist)
						{
							minDist = dist;
							offset = uvs_gui_space[i][index] - handlePosition_canvas;
						}
					}

					handlePosition_canvas += offset;
				}
			}

			SetHandlePosition(handlePosition_canvas, true);

			#if PB_DEBUG
			profiler.EndSample();
			#endif
			return;
		}

		/**
		 *	Tool activated - moving some UVs around.
		 * 	Unlike rotate and scale tools, if the selected faces are Auto the pb_UV changes will be applied
		 *	in OnFinishUVModification, not at real time.
		 */
		if( !pb_Math.EqualWithError(t_handlePosition, handlePosition_canvas, .9f) )
		{
			/**
			 * Start of move UV operation
			 */
			if(!modifyingUVs)
			{
				OnBeginUVModification();
			}

			needsRepaint = true;

			Vector2 newUVPosition = pb_Handle_Utility.GUIToUVPoint(t_handlePosition, uvGridSize);

			if(ControlKey)
				newUVPosition = pbUtil.SnapValue(newUVPosition, handlePosition_canvas-t_handlePosition, pref_gridSnapValue);

			pbUndo.RecordObjects(selection, "Move UVs");

			for(int n = 0; n < selection.Length; n++)
			{
				pb_Object pb = selection[n];
				Vector2[] uvs = GetUVs(pb, channel);

				foreach(int i in distinct_indices[n])
				{
					uvs[i] = newUVPosition - (uvOrigin-uv_origins[n][i]);
				}

				// set uv positions before figuring snap dist stuff
				// don't use ApplyUVs() here because we don't actually want to access the msh
				// til we have to.
				if(channel == 0)
					pb.SetUV(uvs);
				else
					pb.msh.uv2 = uvs;

				if( (ShiftKey || ControlKey) && channel == 0)
					pb.msh.uv = uvs;
			}

			if(!ShiftKey && !ControlKey)
			{
				Vector2 nearestDelta = Vector2.one;

				for(int i = 0; i < selection.Length; i++)
				{
					Vector2[] sel = pbUtil.ValuesWithIndices(GetUVs(selection[i], channel), distinct_indices[i]);

					for(int n = 0; n < selection.Length; n++)
					{
						Vector2 offset;
						if( pb_Handle_Utility.NearestPointDelta(sel, GetUVs(selection[n], channel), i == n ? distinct_indices[i] : null, MAX_PROXIMITY_SNAP_DIST_UV, out offset) )
						{
							if( pb_Handle_Utility.CurrentAxisConstraint.Mask(offset).sqrMagnitude < nearestDelta.sqrMagnitude)
								nearestDelta = offset;
						}
					}
				}

				if(nearestDelta.sqrMagnitude < .003f )
				{
					nearestDelta = pb_Handle_Utility.CurrentAxisConstraint.Mask(nearestDelta);

					for(int i = 0; i < selection.Length; i++)
					{
						Vector2[] uvs = GetUVs(selection[i], channel);

						foreach(int n in distinct_indices[i])
							uvs[n] += nearestDelta;

						ApplyUVs(selection[i], uvs, channel);
					}

					handlePosition_canvas = pb_Handle_Utility.UVToGUIPoint(newUVPosition + nearestDelta, uvGridSize);
				}
				else
				{
					if(channel == 0)
					{
						for(int i = 0; i < selection.Length; i++)
						{
							selection[i].msh.uv = selection[i].uv;
						}
					}
				}
			}

			RefreshSelectedUVCoordinates();
		}
	}

	internal void SceneMoveTool(Vector2 t_handlePosition, Vector2 handlePosition)
	{
		t_handlePosition = pb_Handle_Utility.UVToGUIPoint(t_handlePosition, uvGridSize);
		handlePosition = pb_Handle_Utility.UVToGUIPoint(handlePosition, uvGridSize);

		/**
		 *	Tool activated - moving some UVs around.
		 * 	Unlike rotate and scale tools, if the selected faces are Auto the pb_UV changes will be applied
		 *	in OnFinishUVModification, not at real time.
		 */
		if( !pb_Math.EqualWithError(t_handlePosition, handlePosition, .9f) )
		{
			/**
			 * Start of move UV operation
			 */
			if(!modifyingUVs)
			{
				OnBeginUVModification();
				uvOrigin = pb_Handle_Utility.GUIToUVPoint(t_handlePosition, uvGridSize);	// have to set this one special
			}

			Vector2 newUVPosition = pb_Handle_Utility.GUIToUVPoint(t_handlePosition, uvGridSize);

			if(ControlKey)
				newUVPosition = pbUtil.SnapValue(newUVPosition, handlePosition-t_handlePosition, pref_gridSnapValue);

			pbUndo.RecordObjects(selection, "Move UVs");

			for(int n = 0; n < selection.Length; n++)
			{
				pb_Object pb = selection[n];
				Vector2[] uvs = pb.uv;

				foreach(int i in distinct_indices[n])
				{
					uvs[i] = newUVPosition - (uvOrigin-uv_origins[n][i]);
				}

				pb.SetUV(uvs);
				pb.msh.uv = uvs;
			}

			RefreshSelectedUVCoordinates();
		}
	}

	void RotateTool()
	{
		float t_uvRotation = uvRotation;

		uvRotation = pb_Handle_Utility.RotationHandle2d(0, CanvasToGUIPoint(handlePosition_canvas), uvRotation, 128);

		if(uvRotation != t_uvRotation)
		{
			if(!modifyingUVs)
			{
				OnBeginUVModification();
			}

			if(ControlKey)
				uvRotation = pbUtil.SnapValue(uvRotation, 15f);

			pbUndo.RecordObjects(selection, "Rotate UVs");

			for(int n = 0; n < selection.Length; n++)
			{
				pb_Object pb = selection[n];
				Vector2[] uvs = pb.uv;

				foreach(int i in distinct_indices[n])
				{
					uvs[i] = uv_origins[n][i].RotateAroundPoint( uvOrigin, uvRotation );
					uvs_gui_space[n][i] = pb_Handle_Utility.UVToGUIPoint(uvs[i], uvGridSize);
				}

				pb.SetUV(uvs);
				pb.msh.uv = uvs;
			}

			nearestElement.valid = false;
		}

		needsRepaint = true;
	}

	internal void SceneRotateTool(float rotation)
	{
		if(rotation != uvRotation)
		{
			uvRotation = rotation;

			if(!modifyingUVs)
			{
				OnBeginUVModification();
			}

			if(ControlKey)
				rotation = pbUtil.SnapValue(rotation, 15f);

			pbUndo.RecordObjects(selection, "Rotate UVs");

			for(int n = 0; n < selection.Length; n++)
			{
				pb_Object pb = selection[n];
				Vector2[] uvs = pb.uv;

				foreach(int i in distinct_indices[n])
				{
					uvs[i] = uv_origins[n][i].RotateAroundPoint( uvOrigin, rotation );
					uvs_gui_space[n][i] = pb_Handle_Utility.UVToGUIPoint(uvs[i], uvGridSize);
				}

				pb.SetUV(uvs);
				pb.msh.uv = uvs;
			}

			nearestElement.valid = false;
		}
	}

	Vector2 uvScale = Vector2.one;
	void ScaleTool()
	{
		Vector2 t_uvScale = uvScale;
		uvScale = pb_Handle_Utility.ScaleHandle2d(2, CanvasToGUIPoint(handlePosition_canvas), uvScale, 128);

		if(ControlKey)
			uvScale = pbUtil.SnapValue(uvScale, pref_gridSnapValue);

		if(t_uvScale != uvScale)
		{
			if(!modifyingUVs)
			{		
				OnBeginUVModification();
			}

			pbUndo.RecordObjects(selection, "Scale UVs");

			if(mode == UVMode.Mixed || mode == UVMode.Manual)
			{
				for(int n = 0; n < selection.Length; n++)
				{
					pb_Object pb = selection[n];
					Vector2[] uvs = pb.msh.uv;

					foreach(int i in distinct_indices[n])
					{
						uvs[i] = uv_origins[n][i].ScaleAroundPoint(uvOrigin, uvScale);
						uvs_gui_space[n][i] = pb_Handle_Utility.UVToGUIPoint(uvs[i], uvGridSize);
					}
					
					pb.SetUV(uvs);
					pb.msh.uv = uvs;
				}
			}

			/**
			 * Auto mode scales UVs prior to rotation, so we have to do it separately here.
			 */
			if(mode == UVMode.Mixed || mode == UVMode.Auto)
			{
				Vector2 scale = uvScale.DivideBy(t_uvScale);
				for(int n = 0; n < selection.Length; n++)
				{
					pb_Face[] autoFaces = System.Array.FindAll(selection[n].SelectedFaces, x => !x.manualUV);
					foreach(pb_Face face in autoFaces)
					{
						face.uv.scale = Vector2.Scale(face.uv.scale, scale);
					}
					selection[n].RefreshUV(autoFaces);
				}

				RefreshSelectedUVCoordinates();
			}

			nearestElement.valid = false;
			needsRepaint = true;
		}
	}

	/**
	 * New scale, previous scale
	 */
	internal void SceneScaleTool(Vector2 textureScale, Vector2 previousScale)
	{
		textureScale.x = 1f / textureScale.x;
		textureScale.y = 1f / textureScale.y;

		previousScale.x = 1f / previousScale.x;
		previousScale.y = 1f / previousScale.y;

		if(ControlKey)
			textureScale = pbUtil.SnapValue(textureScale, pref_gridSnapValue);

		if(!modifyingUVs)
		{		
			OnBeginUVModification();
		}

		pbUndo.RecordObjects(selection, "Scale UVs");

		if(mode == UVMode.Mixed || mode == UVMode.Manual)
		{
			for(int n = 0; n < selection.Length; n++)
			{
				pb_Object pb = selection[n];
				Vector2[] uvs = pb.msh.uv;

				foreach(int i in distinct_indices[n])
				{
					uvs[i] = uv_origins[n][i].ScaleAroundPoint(uvOrigin, textureScale);
					uvs_gui_space[n][i] = pb_Handle_Utility.UVToGUIPoint(uvs[i], uvGridSize);
				}
				
				pb.SetUV(uvs);
				pb.msh.uv = uvs;
			}
		}

		/**
		 * Auto mode scales UVs prior to rotation, so we have to do it separately here.
		 */
		if(mode == UVMode.Mixed || mode == UVMode.Auto)
		{
			Vector2 delta = textureScale.DivideBy(previousScale);

			for(int n = 0; n < selection.Length; n++)
			{
				pb_Face[] autoFaces = System.Array.FindAll(selection[n].SelectedFaces, x => !x.manualUV);
				foreach(pb_Face face in autoFaces)
				{
					face.uv.scale = Vector2.Scale(face.uv.scale, delta);
				}
				selection[n].RefreshUV(autoFaces);
			}

			RefreshSelectedUVCoordinates();
		}

		nearestElement.valid = false;
		needsRepaint = true;
	}
#endregion

#region UV Graph Drawing

	Vector2 UVGraphCenter = Vector2.zero;
	
	// private class UVGraphCoordinates
	// {
		// Remember that Unity GUI coordinates Y origin is the bottom
	private static Vector2 UpperLeft = new Vector2(  0f, -1f);
	private static Vector2 UpperRight = new Vector2( 1f, -1f);
	private static Vector2 LowerLeft = new Vector2(  0f,  0f);
	private static Vector2 LowerRight = new Vector2( 1f,  0f);

	private Rect UVGraphZeroZero = new Rect(0,0,40,40);
	private Rect UVGraphOneOne = new Rect(0,0,40,40);

	/**
	 * Must be called inside GL immediate mode context
	 */
	internal void DrawUVGrid(Color gridColor)
	{		
		Color col = GUI.color;
		gridColor.a = .1f;

		GL.PushMatrix();
		pb_Handle_Utility.handleMaterial.SetPass(0);
		GL.MultMatrix(Handles.matrix);

		GL.Begin( GL.LINES );
		GL.Color( gridColor );

		// Grid temp vars
		int GridLines = 64;
		float StepSize = pref_gridSnapValue;	// In UV coordinates

		// Exponentially scale grid size
		while(StepSize * uvGridSize * uvGraphScale < uvGridSize/10)
			StepSize *= 2f;

		// Calculate what offset the grid should be (different from uvCanvasOffset in that we always want to render the grid)
		Vector2 gridOffset = uvCanvasOffset;
		gridOffset.x = gridOffset.x % (StepSize * uvGridSize * uvGraphScale); // (uvGridSize * uvGraphScale);
		gridOffset.y = gridOffset.y % (StepSize * uvGridSize * uvGraphScale); // (uvGridSize * uvGraphScale);

		Vector2 p0 = Vector2.zero, p1 = Vector2.zero;

		///==== X axis lines
		p0.x = ( ( StepSize * (GridLines/2) * uvGridSize ) * uvGraphScale) + UVGraphCenter.x + gridOffset.x;
		p1.x = ( ( -StepSize * (GridLines/2) * uvGridSize ) * uvGraphScale) + UVGraphCenter.x + gridOffset.x;

		for(int i = 0; i < GridLines + 1; i++)
		{
			p0.y = (((StepSize * i) - ((GridLines*StepSize)/2)) * uvGridSize) * uvGraphScale + UVGraphCenter.y + gridOffset.y;
			p1.y = p0.y;

			GL.Vertex( p0 );
			GL.Vertex( p1 );
		}

		///==== Y axis lines
		p0.y = ( ( StepSize * (GridLines/2) * uvGridSize ) * uvGraphScale) + UVGraphCenter.y + gridOffset.y;
		p1.y = ( ( -StepSize * (GridLines/2) * uvGridSize ) * uvGraphScale) + UVGraphCenter.y + gridOffset.y;

		for(int i = 0; i < GridLines + 1; i++)
		{
			p0.x = (((StepSize * i) - ((GridLines*StepSize)/2)) * uvGridSize) * uvGraphScale + UVGraphCenter.x + gridOffset.x;
			p1.x = p0.x;

			GL.Vertex( p0 );
			GL.Vertex( p1 );
		}

		// Box
		GL.Color( Color.gray );

		GL.Vertex(UVGraphCenter + (UpperLeft * uvGridSize) * uvGraphScale + uvCanvasOffset );
		GL.Vertex(UVGraphCenter + (UpperRight * uvGridSize) * uvGraphScale + uvCanvasOffset );

		GL.Vertex(UVGraphCenter + (UpperRight * uvGridSize) * uvGraphScale + uvCanvasOffset );
		GL.Vertex(UVGraphCenter + (LowerRight * uvGridSize) * uvGraphScale + uvCanvasOffset );

		GL.Color( pb_Constant.ProBuilderBlue );

		GL.Vertex(UVGraphCenter + (LowerRight * uvGridSize) * uvGraphScale + uvCanvasOffset );
		GL.Vertex(UVGraphCenter + (LowerLeft * uvGridSize) * uvGraphScale + uvCanvasOffset );

		GL.Vertex(UVGraphCenter + (LowerLeft * uvGridSize) * uvGraphScale + uvCanvasOffset );
		GL.Vertex(UVGraphCenter + (UpperLeft * uvGridSize) * uvGraphScale + uvCanvasOffset );
		
		GL.End();
		GL.PopMatrix();	// Pop pop!

		GUI.color = gridColor;

		UVGraphZeroZero.x = UVRectIdentity.x + 4;
		UVGraphZeroZero.y = UVRectIdentity.y + UVRectIdentity.height + 1;

		UVGraphOneOne.x = UVRectIdentity.x + UVRectIdentity.width + 4;
		UVGraphOneOne.y = UVRectIdentity.y;

		Handles.BeginGUI();
			GUI.Label(UVGraphZeroZero, "0, 0" );
			GUI.Label(UVGraphOneOne, "1, 1" );
		Handles.EndGUI();

		GUI.color = col;
	}

	Rect UVRectIdentity = new Rect(0,0,1,1);

	void DrawUVGraph(Rect rect)
	{
		UVGraphCenter = rect.center;

		UVRectIdentity.width = uvGridSize * uvGraphScale;
		UVRectIdentity.height = UVRectIdentity.width;

		UVRectIdentity.x = UVGraphCenter.x + uvCanvasOffset.x;
		UVRectIdentity.y = UVGraphCenter.y + uvCanvasOffset.y - UVRectIdentity.height;

		if(pref_showMaterial && preview_material && preview_material.mainTexture)
			EditorGUI.DrawPreviewTexture(UVRectIdentity, preview_material.mainTexture, null, ScaleMode.StretchToFill, 0);

		#if PB_DEBUG
			profiler.BeginSample("Draw Base Graph");
				DrawUVGrid(GridColorPrimary);
			profiler.EndSample();
		#else
			DrawUVGrid(GridColorPrimary);
		#endif

		if(selection == null || selection.Length < 1)
			return;

		/**
		 * Draw regular old outlines
		 */
	 	#if PB_DEBUG
		profiler.BeginSample("Draw Base Edges + Vertices");
		#endif
	
		/**
		 * Draw all vertices if in vertex mode
		 */
		try 
		{
			Vector2 p = Vector2.zero;
			if(selectionMode == SelectMode.Vertex)
			{
				// GUI.color = UVColorSecondary;

				for(int i = 0; i < uvs_gui_space.Length; i++)
				{
					GUI.color = UVColorSecondary;
					for(int n = 0; n < uvs_gui_space[i].Length; n++)
					{
						p = CanvasToGUIPoint(uvs_gui_space[i][n]);
						GUI.DrawTexture(new Rect(p.x-HALF_DOT, p.y-HALF_DOT, DOT_SIZE, DOT_SIZE), dot, ScaleMode.ScaleToFit);
					}
		
					GUI.color = UVColorPrimary;
					foreach(int index in selection[i].SelectedTriangles)
					{
						p = CanvasToGUIPoint(uvs_gui_space[i][index]);
						GUI.DrawTexture(new Rect(p.x-HALF_DOT, p.y-HALF_DOT, DOT_SIZE, DOT_SIZE), dot, ScaleMode.ScaleToFit);

						#if PB_DEBUG
						GUI.Label( new Rect(p.x, p.y, 220, 120), selection[i].uv[index].ToString("F4") + "\n" + uvs_gui_space[i][index] + " -> " + p );
						#endif
					}
				}
			}
		} catch(System.Exception e) { }

		GL.PushMatrix();
		pb_Handle_Utility.handleMaterial.SetPass(0);
		GL.MultMatrix(Handles.matrix);

		GL.Begin(GL.LINES);
		GL.Color(UVColorSecondary);

		// Here because when you undo a geometry action that involved deleting or adding vertices,
		// the UpdateSelection() delegate doesn't call UV editor's updateselection fast enough,
		// meaning that uvs_gui_space[][] can get some out of bounds values.  this  seemed like the
		// lesser of two evils, the second being an "if (out of bounds) continue"
		try
		{
			Vector2 x = Vector2.zero, y = Vector2.zero;
			for(int i = 0; i < selection.Length; i++)
			{
				pb_Object pb = selection[i];

				for(int n = 0; n < pb.faces.Length; n++)
				{
					pb_Face face = pb.faces[n];

					foreach(pb_Edge edge in face.edges)
					{
						x = CanvasToGUIPoint(uvs_gui_space[i][edge.x]);
						y = CanvasToGUIPoint(uvs_gui_space[i][edge.y]);

						GL.Vertex(x);
						GL.Vertex(y);
					}
				}	
			}
		} catch(System.Exception e) {
			// sshhhh...
		}
		GL.End();

		#if PB_DEBUG
		profiler.EndSample();
		#endif

		/**
		 * Draw selected UVs with shiny green color and dots
		 */
		#if PB_DEBUG
		profiler.BeginSample("Draw Selected Edges + Vertices");
		#endif

		GUI.color = UVColorPrimary;

		GL.Begin(GL.LINES);
		GL.Color(UVColorPrimary);

		for(int i = 0; i < selection.Length; i++)
		{
			pb_Object pb = selection[i];

			try
			{
				if(pb.SelectedEdges.Length > 0)
				{
					foreach(pb_Edge edge in pb.SelectedEdges)
					{
						Vector2 x = CanvasToGUIPoint(uvs_gui_space[i][edge.x]);
						Vector2 y = CanvasToGUIPoint(uvs_gui_space[i][edge.y]);

						GL.Vertex(x);
						GL.Vertex(y);
						
						#if PB_DEBUG
						GUI.Label( new Rect(x.x, x.y, 120, 20), pb.uv[edge.x].ToString() );
						GUI.Label( new Rect(y.x, y.y, 120, 20), pb.uv[edge.y].ToString() );
						#endif
					}
				}
	
			} catch(System.Exception e) {}
		}

		GL.End();

		#if PB_DEBUG
		profiler.EndSample();
		#endif

		switch(selectionMode)
		{
			case SelectMode.Edge:

				#if PB_DEBUG
				profiler.BeginSample("Draw Nearest Edge Highlight");
				#endif

				GL.Begin(GL.LINES);
				GL.Color(Color.red);
				if(nearestElement.valid && nearestElement.elementSubIndex > -1 && !modifyingUVs)
				{
					pb_Edge edge = selection[nearestElement.objectIndex].faces[nearestElement.elementIndex].edges[nearestElement.elementSubIndex];
					GL.Vertex( CanvasToGUIPoint(uvs_gui_space[nearestElement.objectIndex][edge.x]) );
					GL.Vertex( CanvasToGUIPoint(uvs_gui_space[nearestElement.objectIndex][edge.y]) );
				}
				GL.End();
				
				#if PB_DEBUG
				profiler.EndSample();
				#endif

				break;

			case SelectMode.Face:

				#if PB_DEBUG
				profiler.BeginSample("Draw Nearest Face Highlight GL");
				#endif

				if(nearestElement.valid && !m_mouseDragging)
				{
					GL.Begin(GL.TRIANGLES);

					GL.Color( selection[nearestElement.objectIndex].faces[nearestElement.elementIndex].manualUV ? HOVER_COLOR_MANUAL : HOVER_COLOR_AUTO);
					int[] tris = selection[nearestElement.objectIndex].faces[nearestElement.elementIndex].indices;

					for(int i = 0; i < tris.Length; i+=3)
					{
						GL.Vertex( CanvasToGUIPoint(uvs_gui_space[nearestElement.objectIndex][tris[i+0]]) );
						GL.Vertex( CanvasToGUIPoint(uvs_gui_space[nearestElement.objectIndex][tris[i+1]]) );
						GL.Vertex( CanvasToGUIPoint(uvs_gui_space[nearestElement.objectIndex][tris[i+2]]) );
					}

					GL.End();
				}

				#if PB_DEBUG
				profiler.EndSample();
				profiler.BeginSample("Draw Selected Face Highlights GL");
				#endif

				GL.Begin(GL.TRIANGLES);
				try
				{
					for(int i = 0; i < selection.Length; i++)
					{
						foreach(pb_Face face in selection[i].SelectedFaces)
						{
							GL.Color(face.manualUV ? SELECTED_COLOR_MANUAL : SELECTED_COLOR_AUTO);

							int[] tris = face.indices;

							for(int n = 0; n < tris.Length; n+=3)
							{
								GL.Vertex( CanvasToGUIPoint(uvs_gui_space[i][tris[n+0]]) );
								GL.Vertex( CanvasToGUIPoint(uvs_gui_space[i][tris[n+1]]) );
								GL.Vertex( CanvasToGUIPoint(uvs_gui_space[i][tris[n+2]]) );
							}
						}
					}
				} catch(System.Exception e) {}
				GL.End();

				#if PB_DEBUG
				profiler.EndSample();
				#endif
				break;

		}

		GL.PopMatrix();
		GUI.color = Color.white;
	}	

	#if PB_DEBUG
	void DrawDebugInfo(Rect rect)
	{
		GUI.BeginGroup(rect);
		GUILayout.BeginVertical(GUILayout.MaxWidth(rect.width-6));

		GUILayout.Label("Object: " + nearestElement.ToString());

		int t_channel = channel;
		channel = EditorGUILayout.IntPopup(channel, new string[] {"1", "2"}, UV_CHANNELS);
		if(channel != t_channel)
			RefreshUVCoordinates();

		if(GUILayout.Button("Dump Times"))
			Debug.Log( profiler.ToString() );

		if(GUILayout.Button("Clear Profiler"))
			profiler.Reset();
		
		GUILayout.Label("m_mouseDragging: " + m_mouseDragging);
		GUILayout.Label("m_rightMouseDrag: " + m_rightMouseDrag);
		GUILayout.Label("m_draggingCanvas: " + m_draggingCanvas);
		GUILayout.Label("modifyingUVs: " + modifyingUVs);

		if(editor)
			GUILayout.Label("manualUV: " + selection.SelectMany(x => x.SelectedFaces.Select(n => n.manualUV)).ToArray().ToFormattedString(", "), GUILayout.MaxWidth(rect.width-6));

		if(GUILayout.Button("Refresh Auto UVs"))
		{
			foreach(pb_Object pb in selection)
				pb.RefreshUV();

			RefreshUVCoordinates();
		}

		if(GUILayout.Button("Screenshot"))	
			EditorApplication.delayCall += Screenshot;

		GUILayout.Label("Canvas Zoom: " + uvGraphScale, GUILayout.MaxWidth(rect.width-6));
		GUILayout.Label("Canvas Offset: " + uvCanvasOffset, GUILayout.MaxWidth(rect.width-6));

		float tmp = pref_gridSnapValue;
		pref_gridSnapValue = EditorGUILayout.FloatField("Grid Snap", pref_gridSnapValue, GUILayout.MaxWidth(rect.width-6));
		if(tmp != pref_gridSnapValue)
		{
			pref_gridSnapValue = Mathf.Clamp(pref_gridSnapValue, .015625f, 2f);
			EditorPrefs.SetFloat(pb_Constant.pbUVGridSnapValue, pref_gridSnapValue);
		}

		GUI.EndGroup();
	}
	#endif
#endregion

#region UV Canvas Operations

	/**
	 * Zooms in on the current UV selection
	 */
	void FrameSelection()
	{
		needsRepaint = true;

		if(selection == null || selection.Length < 1 || (editor && editor.selectedVertexCount < 1))
		{
			SetCanvasCenter( GUIToCanvasPoint( Event.current.mousePosition ) * uvGraphScale );
			return;
		}

		SetCanvasCenter( selected_canvas_bounds.center * uvGraphScale );

		if(selected_canvas_bounds.size.sqrMagnitude > 0f)
		{
			float x = (float)screenWidth / (selected_canvas_bounds.size.x*2f);
			float y = (float)(screenHeight-96) / (selected_canvas_bounds.size.y*2f);

			SetCanvasScale( Mathf.Min(x, y) );
		}
	}

	/**
	 * Sets the canvas scale.  1 is full size, .1 is super zoomed, and 2 would be 2x out.
	 */
	void SetCanvasScale(float zoom)
	{
		Vector2 center = -(uvCanvasOffset / uvGraphScale);
		uvGraphScale = Mathf.Clamp(zoom, .01f, MAX_GRAPH_SCALE);
		SetCanvasCenter( center * uvGraphScale );
	}

	/**
	 * Center the canvas on this point.  Should be GUI coordinates.
	 */
	void SetCanvasCenter(Vector2 center)
	{
		uvCanvasOffset = center;
		uvCanvasOffset.x = -uvCanvasOffset.x;
		uvCanvasOffset.y = -uvCanvasOffset.y;
	}

	void ResetCanvas()
	{
		uvGraphScale = 1f;
		SetCanvasCenter( new Vector2(.5f, -.5f) * uvGridSize * uvGraphScale );
	}

	/**
	 * Set the handlePosition to this canvas space coordinate
	 */
	bool userPivot = false;
	void SetHandlePosition(Vector2 canvasPoint, bool isUserSet)
	{
		userPivot = isUserSet;
		handlePosition_offset = selected_canvas_bounds.center - canvasPoint;
		handlePosition_canvas = canvasPoint;
	}

	/**
	 * Used by pb_Editor to reset the pivot offset when adding or removing faces in the scenview.
	 */
	public void ResetUserPivot()
	{
		handlePosition_offset = Vector2.zero;
	}

	pb_Bounds2D GetBounds(int i, int f, Vector2[][] array)
	{
		return new pb_Bounds2D( pbUtil.ValuesWithIndices(array[i], selection[i].faces[f].distinctIndices) );
	}

	/**
	 * Convert a point on the UV canvas (0,1 scaled to guisize) to a GUI coordinate.
	 */
	Vector2 CanvasToGUIPoint(Vector2 v)
	{
		return UVGraphCenter + (v * uvGraphScale + uvCanvasOffset);
	}

	/**
	 * Convert a mouse position in GUI space to a canvas relative point
	 */
	Vector2 GUIToCanvasPoint(Vector2 v)
	{
		return ((v-UVGraphCenter)-uvCanvasOffset)/uvGraphScale;
	}

	/**
	 * Returns the bounds of the current selection in canvas space (multiplied by uvGridSize but not scaled or offset).
	 */
	pb_Bounds2D CanvasSelectionBounds()
	{	
		float xMin = 0f, xMax = 0f, yMin = 0f, yMax = 0f;
		bool first = true;
		for(int n = 0; n < selection.Length; n++)
		{
			foreach(int i in distinct_indices[n])
			{
				if(first) { 
					xMin = uvs_gui_space[n][i].x; 
					xMax = xMin; 
					yMin = uvs_gui_space[n][i].y; 
					yMax = yMin; 
					first = false;
				} else {
					xMin = Mathf.Min(xMin, uvs_gui_space[n][i].x);
					yMin = Mathf.Min(yMin, uvs_gui_space[n][i].y);

					xMax = Mathf.Max(xMax, uvs_gui_space[n][i].x);
					yMax = Mathf.Max(yMax, uvs_gui_space[n][i].y);
				}
			}
		}

		return new pb_Bounds2D( new Vector2( (xMin+xMax)/2f, (yMin+yMax)/2f ), new Vector2(xMax-xMin, yMax-yMin) );
	}
#endregion

#region Refresh / Set

	// Doesn't call Repaint for you
	void RefreshUVCoordinates()
	{
		RefreshUVCoordinates(null, false);
	}

	/**
	 * If dragRect is null, the selected UV array will be derived using the selected ProBuilder faces.
	 * If it ain't null, selected UVs will be set to the UV coordinates contained within the drag rect.
	 */
	void RefreshUVCoordinates(Rect? dragRect, bool isClick)
	{	
		if(editor == null || selection == null) return;

		#if PB_DEBUG
		profiler.BeginSample("RefreshUVCoordinates");
		#endif

		// Collect drawables
		uvs_gui_space = new Vector2[selection.Length][];

		// Convert dragrect from Unity GUI space to uv_gui_space
		pb_Bounds2D dragBounds = dragRect != null ? 
			new pb_Bounds2D( GUIToCanvasPoint(((Rect)dragRect).center), new Vector2( ((Rect)dragRect).width, ((Rect)dragRect).height) / uvGraphScale ) :
			new pb_Bounds2D( Vector2.zero, Vector2.zero );

		selectedUVCount   = editor.selectedVertexCount;
		selectedFaceCount = editor.selectedFaceCount;
		// selectedEdgeCount = editor.selectedEdgeCount;

		for(int i = 0; i < selection.Length; i++)
		{
			pb_Object pb = selection[i];

			int len = pb.vertexCount;
			Vector2[] mshUV = GetUVs(pb, channel);

			uvs_gui_space[i] = new Vector2[len];

			for(int j = 0; j < len; j++)
				uvs_gui_space[i][j] = pb_Handle_Utility.UVToGUIPoint(mshUV[j], uvGridSize);

			// this should probably be separate from RefreshUVCoordinates
			if(dragRect != null)
			{	
				switch(selectionMode)
				{
					case SelectMode.Vertex:
						List<int> selectedTris = new List<int>(pb.SelectedTriangles);

						for(int j = 0; j < len; j++)
						{
							if( dragBounds.ContainsPoint( uvs_gui_space[i][j] ) )
							{
								int indx = selectedTris.IndexOf(j);

								if(indx > -1)
									selectedTris.RemoveAt(indx);
								else
									selectedTris.Add(j);

								// if this is a click, only do one thing per-click
								if(isClick)
									break;
							}
						}

						pb.SetSelectedTriangles(selectedTris.ToArray());
						break;

					case SelectMode.Edge:
						List<pb_Edge> selectedEdges = new List<pb_Edge>(pb.SelectedEdges);

						for(int n = 0; n < pb.faces.Length; n++)
						{
							for(int p = 0; p < pb.faces[n].edges.Length; p++)
							{
								pb_Edge edge = pb.faces[n].edges[p];

								if( dragBounds.IntersectsLineSegment( uvs_gui_space[i][edge.x],  uvs_gui_space[i][edge.y]) )	
								{
									if(!selectedEdges.Contains(edge))
										selectedEdges.Add( edge );
									else
										selectedEdges.Remove( edge );
								}
							}
						}

						pb.SetSelectedEdges(selectedEdges.ToArray());
						break;
				
					/**
					 * Check if any of the faces intersect with the mousedrag rect.
					 */
					case SelectMode.Face:

						List<int> selectedFaces = new List<int>(selection[i].SelectedFaceIndices);
						for(int n = 0; n < pb.faces.Length; n++)
						{
							Vector2[] uvs = pbUtil.ValuesWithIndices(uvs_gui_space[i], pb.faces[n].distinctIndices);
							bool allPointsContained = true;

							// if(dragBounds.Intersects(faceBounds))
							for(int t = 0; t < uvs.Length; t++)
							{
								if(!dragBounds.ContainsPoint(uvs[t]))
								{
									allPointsContained = false;
									break;
								}
							}

							if(allPointsContained)
							{
								if( selectedFaces.Contains(n) )
										selectedFaces.Remove(n);
									else
										selectedFaces.Add(n);
							}
						}
						selection[i].SetSelectedFaces(selectedFaces.ToArray());

						break;
				}

				editor.UpdateSelection(false);
			}
		}

		// figure out what the mode of selected faces is
		if(editor.selectedFaceCount > 0)
		{
			// @todo write a more effecient method for this
			List<bool> manual = new List<bool>();
			for(int i = 0; i < selection.Length; i++)
				manual.AddRange( selection[i].SelectedFaces.Select(x => x.manualUV).ToList() );
			int c = manual.Distinct().Count();
			if(c > 1)
				mode = UVMode.Mixed;
			else if (c > 0)
				mode = manual[0] ? UVMode.Manual : UVMode.Auto;
		}
		else
		{
			mode = UVMode.Manual;
		}

		editor.GetFirstSelectedMaterial(ref preview_material);

		selected_canvas_bounds = CanvasSelectionBounds();
		handlePosition_canvas = selected_canvas_bounds.center - handlePosition_offset;

		#if PB_DEBUG
		profiler.EndSample();
		#endif
	}

	/**
	 * Sets an array to the appropriate UV channel, but don't refresh the Mesh.
	 */
	static void ApplyUVs(pb_Object pb, Vector2[] uvs, int channel)
	{
		switch(channel)
		{
			case 0:
				pb.SetUV(uvs);
				pb.msh.uv = uvs;
				break;

			case 1:
				pb.msh.uv2 = uvs;
				break;
		}
	}

	/**
	 * Get a UV channel.
	 */
	static Vector2[] GetUVs(pb_Object pb, int channel)
	{
		switch(channel)
		{
			case 1:
				return pb.msh.uv2;

			default:
				return pb.uv;
		}
	}

	/**
	 * Refresh only the selected UV coordinates.
	 */
	void RefreshSelectedUVCoordinates()
	{	
		for(int n = 0; n < selection.Length; n++)
		{
			Vector2[] uvs = GetUVs(selection[n], channel);
			
			foreach(int i in distinct_indices[n])
				uvs_gui_space[n][i] = pb_Handle_Utility.UVToGUIPoint(uvs[i], uvGridSize);
		}

		selected_canvas_bounds = CanvasSelectionBounds();
		handlePosition_canvas = selected_canvas_bounds.center - handlePosition_offset;
	}
#endregion

#region UV Toolbar

	Rect toolbarRect_tool = new Rect(PAD, PAD, 130f, 24f);
	Rect toolbarRect_select = new Rect(PAD + 130 + PAD, PAD, 130f, 24f);
	void DrawUVTools(Rect rect)
	{
		GUI.BeginGroup(rect);

		/**
		 * Handle toggles and SelectionMode toggles.
		 */
		EditorGUI.BeginChangeCheck();

			tool = (Tool)GUI.Toolbar(toolbarRect_tool, (int)tool < 0 ? 0 : (int)tool, ToolIcons, "Command");

		if(EditorGUI.EndChangeCheck())
			SetTool_Internal(tool);

		int t_selectionMode = (int)selectionMode;
		t_selectionMode = GUI.Toolbar(toolbarRect_select, (int)t_selectionMode, SelectionIcons, "Command");
		if(t_selectionMode != (int)selectionMode)
			selectionMode = (SelectMode)t_selectionMode;

		/**
		 * Begin Editor pref toggles (Show Texture, Lock UV sceneview handle, etc)
		 */

		Rect editor_toggles_rect = new Rect(toolbarRect_select.x + 130, PAD, 36f, 22f);

		if(editor)
		{
			gc_SceneViewUVHandles.image = editor.editLevel == EditLevel.Texture ? icon_sceneUV_on : icon_sceneUV_off;
			if(GUI.Button(editor_toggles_rect, gc_SceneViewUVHandles))
			{
				if(editor.editLevel == EditLevel.Texture)
					editor.PopEditLevel();
				else
					editor.SetEditLevel(EditLevel.Texture);
			}
		}

		editor_toggles_rect.x += editor_toggles_rect.width + PAD;

		gc_ShowPreviewTexture.image =  pref_showMaterial ? icon_textureMode_on : icon_textureMode_off;
		if(GUI.Button(editor_toggles_rect, gc_ShowPreviewTexture))
		{
			pref_showMaterial = !pref_showMaterial;
			EditorPrefs.SetBool(pb_Constant.pbUVMaterialPreview, pref_showMaterial);
		}
		GUI.EndGroup();
	}

	static Rect ActionWindowDragRect = new Rect(0,0,10000,20);
	void DrawActionWindow(int windowIndex)
	{
		GUILayout.Label("UV Mode: " + mode.ToString(), EditorStyles.boldLabel);
	
		switch(mode)
		{
			case UVMode.Auto:
				DrawAutoModeUI();
				break;

			case UVMode.Manual:
				DrawManualModeUI();
				break;

			case UVMode.Mixed:

				if(GUILayout.Button( gc_ConvertToManual, EditorStyles.miniButton))
					Menu_SetManualUV();

				if(GUILayout.Button( gc_ConvertToAuto, EditorStyles.miniButton))
					Menu_SetAutoUV();

				break;
		}

		// Get some draggage up in hurrr
		GUI.DragWindow(ActionWindowDragRect);
	}

	bool modifyingUVs_AutoPanel = false;

	void DrawAutoModeUI()
	{
		if(GUILayout.Button("Convert to Manual", EditorStyles.miniButton))
			Menu_SetManualUV();

		#if PB_DEBUG
		profiler.BeginSample("pb_AutoUV_Editor");
		#endif

		Undo.RecordObjects(selection, "Adjust Auto UV Parameters");

		if( pb_AutoUV_Editor.OnGUI(selection, (int)actionWindowRect.width) )
		{
			if(!modifyingUVs_AutoPanel)
			{				
				modifyingUVs_AutoPanel = true;

				foreach(pb_Object pb in selection)
				{
					pb.ToMesh();
					pb.Refresh();
				}
			}

			for(int i = 0; i < selection.Length; i++)
			{
				selection[i].RefreshUV(editor.SelectedFacesInEditZone[i] );
			}

			RefreshSelectedUVCoordinates();
		}

		#if PB_DEBUG
		profiler.EndSample();
		#endif

		GUI.enabled = selectedFaceCount > 0;
		
	}

	Vector2 scroll = Vector2.zero;
	void DrawManualModeUI()
	{
		if(GUILayout.Button("Convert to Auto", EditorStyles.miniButton))
			Menu_SetAutoUV();

		scroll = EditorGUILayout.BeginScrollView(scroll);

		/**
		 * Projection Methods
		 */
		GUILayout.Label("Project UVs", EditorStyles.miniBoldLabel);

		GUI.enabled = selectedFaceCount > 0;
		GUILayout.BeginHorizontal();
			if(GUILayout.Button("Planar", EditorStyles.miniButton, GUILayout.MaxWidth(actionWindowRect.width)))
				Menu_PlanarProject();

			if(GUILayout.Button("Box", EditorStyles.miniButton, GUILayout.MaxWidth(actionWindowRect.width)))
				Menu_BoxProject();
		GUILayout.EndHorizontal();

		/**
		 * Selection
		 */
		GUI.enabled = selectedUVCount > 0;
		GUILayout.Label("Selection", EditorStyles.miniBoldLabel);

		if(GUILayout.Button("Select Island", EditorStyles.miniButton, GUILayout.MaxWidth(actionWindowRect.width)))
			Menu_SelectUVIsland();

		if(GUILayout.Button("Select Face", EditorStyles.miniButton, GUILayout.MaxWidth(actionWindowRect.width)))
			Menu_SelectUVFace();

		/**
		 * Edit
		 */
		GUILayout.Label("Edit", EditorStyles.miniBoldLabel);

		GUI.enabled = selectedUVCount > 1;
		if(GUILayout.Button("Weld UVs", EditorStyles.miniButton, GUILayout.MaxWidth(actionWindowRect.width)))
			Menu_SewUVs();

		if(GUILayout.Button("Collapse UVs", EditorStyles.miniButton, GUILayout.MaxWidth(actionWindowRect.width)))
			Menu_CollapseUVs();

		GUI.enabled = selectedUVCount > 0;
		if(GUILayout.Button("Split UVs", EditorStyles.miniButton, GUILayout.MaxWidth(actionWindowRect.width)))
			Menu_SplitUVs();

		GUILayout.Space(4);

		if(GUILayout.Button("Flip Horizontal", EditorStyles.miniButton, GUILayout.MaxWidth(actionWindowRect.width)))
			Menu_FlipUVs(Vector2.up);

		if(GUILayout.Button("Flip Vertical", EditorStyles.miniButton, GUILayout.MaxWidth(actionWindowRect.width)))
			Menu_FlipUVs(Vector2.right);

		GUILayout.Space(4);

		if(GUILayout.Button("Fit UVs", EditorStyles.miniButton, GUILayout.MaxWidth(actionWindowRect.width)))
			Menu_FitUVs();

		EditorGUILayout.EndScrollView();

		GUI.enabled = true;
	}
#endregion

#region UV Selection

	/**
	 * Given selected tris, return an array of all indices attached to face
	 */
	private void SelectUVShell()
	{
		if(selection == null || selection.Length < 1) return;

		foreach(pb_Object pb in selection)
		{
			pb_Face[] faces = GetFaces(pb, pb.SelectedTriangles);
			
			List<int> elementGroups = new List<int>();
			
			foreach(pb_Face f in faces)
				elementGroups.Add(f.elementGroup);

			List<pb_Face> facesMatchingElementGroup = new List<pb_Face>(faces);

			foreach(pb_Face f in pb.faces)
			{
				if(f.elementGroup > -1 && elementGroups.Contains(f.elementGroup))
					facesMatchingElementGroup.Add(f);
			}

			pb.SetSelectedFaces( facesMatchingElementGroup.Distinct().ToArray() );
	
			if(editor != null)
				editor.UpdateSelection(false);
		}
	}

	/**
	 * If any of the faces in @selection are AutoUV and in a texture group, this 
	 * augments the texture group buddies to the selection and returns it.
	 */
	private pb_Face[] SelectTextureGroups(pb_Object pb, pb_Face[] selection)
	{	
		List<int> texGroups = selection.Select(x => x.textureGroup).Where(x => x > 0).Distinct().ToList();
		pb_Face[] sel = System.Array.FindAll(pb.faces, x => !x.manualUV && texGroups.Contains(x.textureGroup));

		return selection.Union(sel).ToArray();
	}

	/**
	 * Sets the SceneView and UV selection to include any faces with currently selected indices.
	 */
	private void SelectUVFace()
	{
		if(selection == null || selection.Length < 1) return;

		foreach(pb_Object pb in selection)
		{
			pb_Face[] faces = GetFaces(pb, pb.SelectedTriangles);
			pb.SetSelectedFaces(faces);
	
			if(editor != null)
				editor.UpdateSelection(false);
		}
	}

	/**
	 *	Element Groups are used to associate faces that share UV seams.  In this 
	 *	way, we can easily select UV shells by grouping all elements as opposed
	 *	to iterating through and checking nearby faces every time.
	 */
	private void RefreshElementGroups(pb_Object pb)
	{
		foreach(pb_Face f in pb.faces)
			f.elementGroup = -1;

		pb_IntArray[] sharedUVs = pb.sharedIndicesUV;

		int eg = 0;
		foreach(pb_IntArray pint in sharedUVs)
		{
			if(pint.array.Length < 2) continue;

			pb_Face[] faces = GetFaces(pb, pint);

			int cur = pb.UnusedElementGroup(eg++);

			foreach(pb_Face f in faces)
			{
				if(f.elementGroup > -1)
				{
					int g = f.elementGroup;

					foreach(pb_Face fin in pb.faces)
						if(fin.elementGroup == g)
							fin.elementGroup = cur;
				}
				
				f.elementGroup = cur;
			}
		}
	}

	/**
	 * Get all faces that contain any of the passed vertex indices.
	 */
	private pb_Face[] GetFaces(pb_Object pb, int[] indices)
	{
		#if PB_DEBUG
		profiler.BeginSample("GetFaces");
		#endif

		List<pb_Face> faces = new List<pb_Face>();
		foreach(pb_Face f in pb.faces)
		{
			foreach(int i in f.distinctIndices)
			{
				if(System.Array.IndexOf(indices, i) > -1)
				{
					faces.Add(f);
					break;
				}
			}
		}

		#if PB_DEBUG
		profiler.EndSample();
		#endif

		return faces.Distinct().ToArray();
	}

	/**
	 * Finds all faces attached to the current selection and marks the faces as having been manually modified.
	 */
	private void FlagSelectedFacesAsManual(pb_Object pb)
	{
		// Mark selected UV faces manualUV flag true
		foreach(pb_Face f in GetFaces(pb, pb.SelectedTriangles))
		{
			f.textureGroup = -1;
			f.manualUV = true;
		}
	}

	/**
	 * Creates a copy of each msh.uv array in a jagged array, and stores the average of all points.
	 */
	private void CopySelectionUVs(out Vector2[][] uvCopy)
	{		
		uvCopy = new Vector2[selection.Length][];
		for(int i = 0; i < selection.Length; i++)
		{
			pb_Object pb = selection[i];
			uvCopy[i] = new Vector2[pb.vertexCount];
			System.Array.Copy( GetUVs(pb, channel), uvCopy[i], pb.vertexCount);
		}
	}
#endregion

#region Menu Commands

	public void Menu_PlanarProject()
	{
		pbUndo.RecordObjects(selection, "Planar Project Faces");
		int projected = 0;
	
		for(int i = 0; i < selection.Length; i++)
		{
			if(selection[i].SelectedFaces.Length > 0)
			{
				selection[i].ToMesh();	// Remove UV2 modifications
				selection[i].Refresh();

				pbUVOps.SplitUVs(selection[i], selection[i].SelectedTriangles);

				pbUVOps.ProjectFacesAuto(selection[i], selection[i].SelectedFaces);
				
				foreach(int f in selection[i].SelectedFaceIndices)
					selection[i].faces[f].manualUV = true;

				selection[i].GenerateUV2();
				
				projected++;
			}
		}

		SetSelectedUVsWithSceneView();

		if(projected > 0)
		{
			if(pb_Preferences_Internal.GetBool(pb_Constant.pbNormalizeUVsOnPlanarProjection))
				Menu_FitUVs();

			ResetUserPivot();
		}
		
		pb_Editor_Utility.ShowNotification(this, projected > 0 ? "Planar Project" : "Nothing Selected");
	
		// Special case
		RefreshUVCoordinates();
		needsRepaint = true;
	}

	public void Menu_BoxProject()
	{
		int p = 0;
		pbUndo.RecordObjects(selection, "Box Project Faces");

		for(int i = 0; i < selection.Length; i++)
		{
			if(selection[i].SelectedFaces.Length > 0)
			{
				pbUVOps.ProjectFacesBox(selection[i], selection[i].SelectedFaces);
				
				foreach(int f in selection[i].SelectedFaceIndices)
					selection[i].faces[f].manualUV = true;

				p ++;

				selection[i].GenerateUV2();
			}
		}

		SetSelectedUVsWithSceneView();

		if(p > 0)
		{
			ResetUserPivot();
		}

		pb_Editor_Utility.ShowNotification(this, "Box Project UVs");
		
		// Special case
		RefreshUVCoordinates();
		needsRepaint = true;
	}

	/**
	 * Reset all selected faces to use the default Automatic unwrapping.  Removes
	 * any modifications made by the user.
	 */
	public void Menu_SetAutoUV()
	{
		SetIsManual(false);
	}

	/**
	 * Sets all faces to manual UV mode.
	 */
	public void Menu_SetManualUV()
	{
		SetIsManual(true);
	}

	public void SetIsManual(bool isManual)
	{
		pbUndo.RecordObjects(selection, isManual ? "Set Faces Manual" : "Set Faces Auto");
		
		foreach(pb_Object pb in selection)
		{
			pbUVOps.SetAutoUV(pb, pb.SelectedFaces, !isManual);
			pb.GenerateUV2();
		}

		SetSelectedUVsWithSceneView();
		RefreshUVCoordinates();

		pb_Editor_Utility.ShowNotification(this, "Set " + selectedFaceCount + " Faces " + (isManual ? "Manual" : "Auto"));
	}

	public void Menu_SelectUVIsland()
	{
		pbUndo.RecordObjects(selection, "Select Island");
		
		SelectUVShell(); 
		pb_Editor_Utility.ShowNotification(this, "Select UV Island");
	}

	public void Menu_SelectUVFace()
	{
		pbUndo.RecordObjects(selection, "Select Face");

		SelectUVFace(); 
		pb_Editor_Utility.ShowNotification(this, "Select UV Face");
	}

	public void Menu_CollapseUVs()
	{
		if(channel == 1)
		{
			pb_Editor_Utility.ShowNotification(this, "Invalid UV2 Operation");
			return;
		}

		pbUndo.RecordObjects(selection, "Collapse UVs");

		for(int i = 0; i < selection.Length; i++)
		{			
			selection[i].CollapseUVs(distinct_indices[i]);

			selection[i].ToMesh();
			selection[i].Refresh();
			selection[i].GenerateUV2();
		}

		RefreshSelectedUVCoordinates();

		pb_Editor_Utility.ShowNotification(this, "Collapse UVs");
	}

	public void Menu_SewUVs()
	{
		if(channel == 1)
		{
			pb_Editor_Utility.ShowNotification(this, "Invalid UV2 Operation");
			return;
		}

		pbUndo.RecordObjects(selection, "Sew UV Seams");
		for(int i = 0; i < selection.Length; i++)
		{
			selection[i].ToMesh();
			selection[i].Refresh();

			selection[i].SewUVs(distinct_indices[i], .03f);
			RefreshElementGroups(selection[i]);

			selection[i].GenerateUV2();
		}
		
		RefreshSelectedUVCoordinates();
		pb_Editor_Utility.ShowNotification(this, "Weld UVs");
	}

	public void Menu_SplitUVs()
	{
		if(channel == 1)
		{
			pb_Editor_Utility.ShowNotification(this, "Invalid UV2 Operation");
			return;
		}

		pbUndo.RecordObjects(selection, "Split UV Seams");

		foreach(pb_Object pb in selection)
		{
			pb.ToMesh();
			pb.Refresh();
			
			pb.SplitUVs(pb.SelectedTriangles);
			RefreshElementGroups(pb);

			pb.GenerateUV2();
		}

		SetSelectedUVsWithSceneView();
		RefreshSelectedUVCoordinates();

		pb_Editor_Utility.ShowNotification(this, "Split UVs");
	}

	/**
	 * Flips UVs across the provided direction. The current pivot position is used as origin.  Can be horizontal, vertical, or anything in between.
	 */
	public void Menu_FlipUVs(Vector2 direction)
	{
		pbUndo.RecordObjects(selection, "Flip " + direction);

		Vector2 center = pb_Handle_Utility.GUIToUVPoint( handlePosition_canvas, uvGridSize );

		for(int i = 0; i < selection.Length; i++)
		{
			selection[i].ToMesh();
			selection[i].Refresh();

			selection[i].SplitUVs(selection[i].SelectedTriangles);

			Vector2[] uv = channel == 0 ? selection[i].uv : selection[i].msh.uv2;

			foreach(int n in selection[i].SelectedTriangles.Distinct())
				uv[n] = pb_Math.ReflectPoint(uv[n], center, center + direction);

			ApplyUVs(selection[i], uv, channel);
			
			RefreshElementGroups(selection[i]);

			selection[i].GenerateUV2();
		}

		SetSelectedUVsWithSceneView();
		RefreshSelectedUVCoordinates();

		if( direction == Vector2.right )
		{
			pb_Editor_Utility.ShowNotification(this, "Flip UVs Vertically");
		}
		else if( direction == Vector2.up )
		{
			pb_Editor_Utility.ShowNotification(this, "Flip UVs Horizontally");
		} 
		else
		{
			pb_Editor_Utility.ShowNotification(this, "Flip UVs");
		}
	}

	/**
	 * Fit selected UVs to 0,1 space.
	 */
	public void Menu_FitUVs()
	{
		pbUndo.RecordObjects(selection, "Fit UVs");

		for(int i = 0; i < selection.Length; i++)
		{
			if(selection[i].SelectedTriangleCount < 3) continue;

			Vector2[] uv = selection[i].uv;
			Vector2[] uvs = pbUtil.ValuesWithIndices( uv, distinct_indices[i] );

			uvs = pbUVOps.FitUVs(uvs);

			for(int n = 0; n < uvs.Length; n++)
				uv[ distinct_indices[i][n] ] = uvs[n];

			selection[i].SetUV(uv);

			selection[i].ToMesh();
			selection[i].Refresh();
			selection[i].GenerateUV2();
		}

		RefreshSelectedUVCoordinates();
		pb_Editor_Utility.ShowNotification(this, "Fit UVs");
	}
#endregion

	public void Screenshot()
	{
		Vector2 cen = CanvasToGUIPoint( Vector2.zero );
		cen.y = Screen.height - cen.y;

		int sz = (int)(uvGridSize * uvGraphScale);

		Texture2D tex = new Texture2D(sz, sz);
		tex.ReadPixels(new Rect(cen.x, cen.y - WINDOW_HEADER_OFFSET, sz, sz), 0, 0);
		tex.Apply();

		pb_Editor_Utility.SaveTexture(tex);
	}
}

#endif

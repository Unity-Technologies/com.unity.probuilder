using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using ProBuilder.Core;
using ProBuilder.Interface;
using ProBuilder.MeshOperations;

namespace ProBuilder.EditorCore
{
	public delegate void OnSelectionUpdateEventHandler(pb_Object[] selection);
	public delegate void OnVertexMovementBeginEventHandler(pb_Object[] selection);
	public delegate void OnVertexMovementFinishedEventHandler(pb_Object[] selection);

class pb_Editor : EditorWindow
{
	pb_ElementGraphics graphics { get { return pb_ElementGraphics.instance; } }

#region LOCAL MEMBERS && EDITOR PREFS

	// because editor prefs can change, or shortcuts may be added, certain pb_Preferences_Internal.need to be force reloaded.
	// adding to this const will force update on updating packages.
	const int EDITOR_PREF_VERSION = 2080;
	const int EDITOR_SHORTCUTS_VERSION = 250;
	const int WINDOW_WIDTH_FlOATING = 102;
	const int WINDOW_WIDTH_DOCKABLE = 108;

	// Toggles for Face, Vertex, and Edge mode.
	const int SELECT_MODE_LENGTH = 3;
	GUIContent[] EditModeIcons;
	GUIStyle VertexTranslationInfoStyle;

	public static pb_Editor instance { get { return _instance; } }
	private static pb_Editor _instance;

	public static void Refresh(bool force = true)
	{
		if(instance != null)
			instance.UpdateSelection(force);
	}

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
#if !PROTOTYPE
	private SelectMode previousSelectMode;
#endif

	public DragSelectMode dragSelectMode = DragSelectMode.Difference;

	public HandleAlignment handleAlignment { get; private set; }

	#if !PROTOTYPE
	private HandleAlignment previousHandleAlignment;
	#endif

	private static pb_EditorToolbar editorToolbar = null;

	pb_Shortcut[] shortcuts;

	// If true, in EditMode.ModeBased && SelectionMode.Vertex only vertices will be selected when dragging.
	bool m_VertexSelectionMask = true;
	bool m_ShowSceneInfo = false;
	bool m_HamSelection = false;

	float m_SnapValue = .25f;
	bool m_SnapAxisConstraint = true;
	bool m_SnapEnabled = false;
	bool m_IsIconGui = false;

	bool m_ShowToolbar = true;
	SceneToolbarLocation m_SceneToolbarLocation = SceneToolbarLocation.UpperCenter;

	bool m_LimitFaceDragToSelection = true;
	public bool isFloatingWindow { get; private set; }

	public static bool SelectHiddenEnabled
	{
		get { return pb_PreferencesInternal.GetBool(pb_Constant.pbEnableBackfaceSelection, true); }
		set { pb_PreferencesInternal.SetBool(pb_Constant.pbEnableBackfaceSelection, value); }
	}
#endregion

#region INITIALIZATION AND ONDISABLE

	/// <summary>
	/// Open the pb_Editor window with whatever dockable status is preference-d.
	/// </summary>
	/// <returns></returns>
	public static pb_Editor MenuOpenWindow()
	{
		pb_Editor editor = (pb_Editor)EditorWindow.GetWindow(typeof(pb_Editor), !pb_PreferencesInternal.GetBool(pb_Constant.pbDefaultOpenInDockableWindow), pb_Constant.PRODUCT_NAME, true);			// open as floating window
		// would be nice if editorwindow's showMode was exposed
		editor.isFloatingWindow = !pb_PreferencesInternal.GetBool(pb_Constant.pbDefaultOpenInDockableWindow);
		return editor;
	}

	public void OnEnable()
	{
		_instance = this;

		graphics.LoadPrefs( (Color) pb_PreferencesInternal.GetColor(pb_Constant.pbDefaultFaceColor),
							(Color) pb_PreferencesInternal.GetColor(pb_Constant.pbDefaultEdgeColor),
							(Color) pb_PreferencesInternal.GetColor(pb_Constant.pbDefaultSelectedVertexColor),
							(Color) pb_PreferencesInternal.GetColor(pb_Constant.pbDefaultVertexColor),
							(float) pb_PreferencesInternal.GetFloat(pb_Constant.pbVertexHandleSize) );

		HookDelegates();

		// make sure load prefs is called first, because other methods depend on the preferences set here
		LoadPrefs();

		InitGUI();

		// EditorUtility.UnloadUnusedAssets();

		UpdateSelection(true);

		HideSelectedWireframe();

		findNearestVertex = typeof(HandleUtility).GetMethod("FindNearestVertex", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);

		if( onEditLevelChanged != null )
			onEditLevelChanged( (int) editLevel );
	}

	private void InitGUI()
	{
		if(editorToolbar != null)
			GameObject.DestroyImmediate(editorToolbar);

		editorToolbar = ScriptableObject.CreateInstance<pb_EditorToolbar>();
		editorToolbar.hideFlags = HideFlags.HideAndDontSave;
		editorToolbar.InitWindowProperties(this);

		VertexTranslationInfoStyle = new GUIStyle();
		VertexTranslationInfoStyle.normal.background = EditorGUIUtility.whiteTexture;
		VertexTranslationInfoStyle.normal.textColor = new Color(1f, 1f, 1f, .6f);
		VertexTranslationInfoStyle.padding = new RectOffset(3,3,3,0);

		Texture2D object_Graphic_off 	= pb_IconUtility.GetIcon("Modes/Mode_Object", IconSkin.Pro);
		Texture2D face_Graphic_off 		= pb_IconUtility.GetIcon("Modes/Mode_Face", IconSkin.Pro);
		Texture2D vertex_Graphic_off 	= pb_IconUtility.GetIcon("Modes/Mode_Vertex", IconSkin.Pro);
		Texture2D edge_Graphic_off 		= pb_IconUtility.GetIcon("Modes/Mode_Edge", IconSkin.Pro);

		EditModeIcons = new GUIContent[]
		{
			object_Graphic_off != null ? new GUIContent(object_Graphic_off, "Object Selection") : new GUIContent("OBJ", "Object Selection"),
			vertex_Graphic_off != null ? new GUIContent(vertex_Graphic_off, "Vertex Selection") : new GUIContent("VRT", "Vertex Selection"),
			edge_Graphic_off != null ? new GUIContent(edge_Graphic_off, "Edge Selection") : new GUIContent("EDG", "Edge Selection"),
			face_Graphic_off != null ? new GUIContent(face_Graphic_off, "Face Selection") : new GUIContent("FCE", "Face Selection"),
		};
	}

	public void LoadPrefs()
	{
		// this exists to force update preferences when updating packages
		if(!pb_PreferencesInternal.HasKey(pb_Constant.pbEditorPrefVersion) || pb_PreferencesInternal.GetInt(pb_Constant.pbEditorPrefVersion) != EDITOR_PREF_VERSION )
		{
			pb_PreferencesInternal.SetInt(pb_Constant.pbEditorPrefVersion, EDITOR_PREF_VERSION, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.DeleteKey(pb_Constant.pbVertexHandleSize);
			pb_PreferencesInternal.DeleteKey(pb_Constant.pbDefaultFaceColor);
			pb_PreferencesInternal.DeleteKey(pb_Constant.pbDefaultEdgeColor);
			pb_PreferencesInternal.DeleteKey(pb_Constant.pbDefaultSelectedVertexColor);
			pb_PreferencesInternal.DeleteKey(pb_Constant.pbDefaultVertexColor);
			pb_PreferencesInternal.DeleteKey(pb_Constant.pbDefaultShortcuts);
		}

		if( pb_PreferencesInternal.GetInt(pb_Constant.pbEditorShortcutsVersion, -1) != EDITOR_SHORTCUTS_VERSION )
		{
			pb_PreferencesInternal.SetInt(pb_Constant.pbEditorShortcutsVersion, EDITOR_SHORTCUTS_VERSION, pb_PreferenceLocation.Global);
			pb_PreferencesInternal.DeleteKey(pb_Constant.pbDefaultShortcuts);
			Debug.LogWarning("ProBuilder shortcuts reset. This is either due to a version update that breaks existing shortcuts, or the preferences have been manually reset.");
		}

		editLevel 			= pb_PreferencesInternal.GetEnum<EditLevel>(pb_Constant.pbDefaultEditLevel);
		selectionMode		= pb_PreferencesInternal.GetEnum<SelectMode>(pb_Constant.pbDefaultSelectionMode);
		handleAlignment		= pb_PreferencesInternal.GetEnum<HandleAlignment>(pb_Constant.pbHandleAlignment);
		m_ShowSceneInfo 	= pb_PreferencesInternal.GetBool(pb_Constant.pbShowSceneInfo);
		m_HamSelection	= pb_PreferencesInternal.GetBool(pb_Constant.pbElementSelectIsHamFisted);

		m_SnapEnabled 	= pb_ProGridsInterface.SnapEnabled();
		m_SnapValue		= pb_ProGridsInterface.SnapValue();
		m_SnapAxisConstraint = pb_ProGridsInterface.UseAxisConstraints();

		shortcuts 			= pb_Shortcut.ParseShortcuts(pb_PreferencesInternal.GetString(pb_Constant.pbDefaultShortcuts)).ToArray();
		m_LimitFaceDragToSelection = pb_PreferencesInternal.GetBool(pb_Constant.pbDragCheckLimit);

		// pref_showToolbar = pb_Preferences_Internal.GetBool(pb_Constant.pbShowSceneToolbar);
		m_SceneToolbarLocation = pb_PreferencesInternal.GetEnum<SceneToolbarLocation>(pb_Constant.pbToolbarLocation);
		m_IsIconGui = pb_PreferencesInternal.GetBool(pb_Constant.pbIconGUI);
		dragSelectMode = pb_PreferencesInternal.GetEnum<DragSelectMode>(pb_Constant.pbDragSelectMode);
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

		if(editorToolbar != null)
			GameObject.DestroyImmediate(editorToolbar);

		ClearElementSelection();

		UpdateSelection();

		if( OnSelectionUpdate != null )
			OnSelectionUpdate(null);

		pb_ProGridsInterface.UnsubscribePushToGridEvent(PushToGrid);

		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;

		pb_PreferencesInternal.SetInt(pb_Constant.pbHandleAlignment, (int)handleAlignment);

		if(pb_LineRenderer.Valid())
			pb_LineRenderer.instance.Clear();

		// re-enable unity wireframe
		foreach(pb_Object pb in FindObjectsOfType(typeof(pb_Object)))
			pb_EditorUtility.SetSelectionRenderState(pb.gameObject.GetComponent<Renderer>(), pb_EditorUtility.GetSelectionRenderState());

		SceneView.RepaintAll();
	}
#endregion

#region EVENT HANDLERS

	public static event OnSelectionUpdateEventHandler OnSelectionUpdate;

	// Called when vertex modifications are complete.
	public static event OnVertexMovementFinishedEventHandler OnVertexMovementFinish;

	// Called immediately prior to beginning vertex modifications.  pb_Object will be
	// in un-altered state at this point (meaning ToMesh and Refresh have been called, but not Optimize).
	public static event OnVertexMovementBeginEventHandler OnVertexMovementBegin;

	public void HookDelegates()
	{
		SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
		SceneView.onSceneGUIDelegate += this.OnSceneGUI;

		pb_ProGridsInterface.SubscribePushToGridEvent(PushToGrid);
		pb_ProGridsInterface.SubscribeToolbarEvent(ProGridsToolbarOpen);
	}
#endregion

#region ONGUI

	public void OnInspectorUpdate()
	{
		if(EditorWindow.focusedWindow != this)
			Repaint();
	}

	GUIStyle commandStyle = null;
	Rect elementModeToolbarRect = new Rect(3,6,128,24);

	void OnGUI()
	{
		if(	commandStyle == null )
			commandStyle = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("Command");

		Event e = Event.current;

		switch(e.type)
		{
			case EventType.ContextClick:
				OpenContextMenu();
				break;

			case EventType.KeyDown:
				if (shortcuts.Any(x => x.Matches(e.keyCode, e.modifiers)))
					e.Use();
				break;

			case EventType.KeyUp:
				ShortcutCheck(e);
				break;
		}

		if(editorToolbar != null)
		{
			editorToolbar.OnGUI();
		}
		else
		{
			try
			{
				InitGUI();
			}
			catch(System.Exception exception)
			{
				Debug.LogWarning(string.Format("Failed initializing ProBuilder Toolbar:\n{0}", exception.ToString()));
			}
		}
	}
#endregion

#region CONTEXT MENU

	void OpenContextMenu()
	{
		GenericMenu menu = new GenericMenu();

		menu.AddItem (new GUIContent("Open As Floating Window", ""), !pb_PreferencesInternal.GetBool(pb_Constant.pbDefaultOpenInDockableWindow, true), Menu_OpenAsFloatingWindow);
		menu.AddItem (new GUIContent("Open As Dockable Window", ""), pb_PreferencesInternal.GetBool(pb_Constant.pbDefaultOpenInDockableWindow, true), Menu_OpenAsDockableWindow);

		menu.AddSeparator("");

		menu.AddItem (new GUIContent("Use Icon Mode", ""), pb_PreferencesInternal.GetBool(pb_Constant.pbIconGUI), Menu_ToggleIconMode);
		menu.AddItem (new GUIContent("Use Text Mode", ""), !pb_PreferencesInternal.GetBool(pb_Constant.pbIconGUI), Menu_ToggleIconMode);

		menu.ShowAsContext ();
	}

	void Menu_ToggleIconMode()
	{
		m_IsIconGui = !pb_PreferencesInternal.GetBool(pb_Constant.pbIconGUI);
		pb_PreferencesInternal.SetBool(pb_Constant.pbIconGUI, m_IsIconGui);
		if(editorToolbar != null)
			GameObject.DestroyImmediate(editorToolbar);
		editorToolbar = ScriptableObject.CreateInstance<pb_EditorToolbar>();
		editorToolbar.hideFlags = HideFlags.HideAndDontSave;
		editorToolbar.InitWindowProperties(this);
	}

	void Menu_OpenAsDockableWindow()
	{
		pb_PreferencesInternal.SetBool(pb_Constant.pbDefaultOpenInDockableWindow, true);
		EditorWindow.GetWindow<pb_Editor>().Close();
		pb_Editor.MenuOpenWindow();
	}

	void Menu_OpenAsFloatingWindow()
	{
		pb_PreferencesInternal.SetBool(pb_Constant.pbDefaultOpenInDockableWindow, false);
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
	private bool dragging = false, readyForMouseDrag = false;
	private bool doubleClicked = false;	// prevents leftClickUp from stealing focus after double click

	// vertex handles
	Vector3 newPosition, cachedPosition;
	bool movingVertices = false;

	// top level caching
	bool scaling = false;

	private bool rightMouseDown = false;
	Event currentEvent;

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

		// Snap stuff
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
								pb_Undo.RecordSelection(selection, "Set Face Materials");

								foreach(pb_Object pbs in selection)
									pbs.SetFaceMaterial(pbs.SelectedFaces.Length < 1 ? pbs.faces : pbs.SelectedFaces, mat);

							}
							else
							{
								pb_Undo.RecordObject(pb, "Set Object Material");
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

		if(currentEvent.type == EventType.KeyDown)
		{
			if (shortcuts.Any(x => x.Matches(currentEvent.keyCode, currentEvent.modifiers)))
				currentEvent.Use();
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
		if(currentEvent.type == EventType.MouseMove && editLevel == EditLevel.Geometry)
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
		if( pb_EditorHandleUtility.SceneViewInUse(currentEvent) || currentEvent.isKey || selection == null || selection.Length < 1)
		{
			dragging = false;
			return;
		}

		// This prevents us from selecting other objects in the scene,
		// and allows for the selection of faces / vertices.
		int controlID = GUIUtility.GetControlID(FocusType.Passive);
		HandleUtility.AddDefaultControl(controlID);

		// If selection is made, don't use default handle -- set it to Tools.None
		if(selectedVertexCount > 0)
			Tools.current = Tool.None;

		if(leftClick)
		{
			// double clicking object
			if(currentEvent.clickCount > 1)
			{
				DoubleClick(currentEvent);
			}

			mousePosition_initial = mousePosition;
			// readyForMouseDrag prevents a bug wherein after ending a drag an errant
			// MouseDrag event is sent with no corresponding MouseDown/MouseUp event.
			readyForMouseDrag = true;
		}

		if(mouseDrag && readyForMouseDrag)
		{
			dragging = true;
		}

		if(ignore)
		{
			if(dragging)
			{
				readyForMouseDrag = false;
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
					if(pb_UVEditor.instance)
						pb_UVEditor.instance.ResetUserPivot();
#endif

					RaycastCheck(currentEvent.mousePosition);
				}
				else
				{
					dragging = false;
					readyForMouseDrag = false;
#if !PROTOTYPE
					if(pb_UVEditor.instance)
						pb_UVEditor.instance.ResetUserPivot();
#endif

					DragCheck();
				}
			}
		}
	}

	void DoubleClick(Event e)
	{
		pb_Object pb = RaycastCheck(e.mousePosition, -1);

		if(pb != null)
		{
			if (selectionMode == SelectMode.Edge)
			{
				if (e.shift)
					pb_MenuCommands.MenuRingSelection(selection);
				else
					pb_MenuCommands.MenuLoopSelection(selection);
			}
			else if(selectionMode == SelectMode.Face)
			{
				if((e.modifiers & (EventModifiers.Control | EventModifiers.Shift)) == (EventModifiers.Control | EventModifiers.Shift))
					pb_MenuCommands.MenuRingAndLoopFaces(selection);
				else if(e.control)
					pb_MenuCommands.MenuRingFaces(selection);
				else if(e.shift)
					pb_MenuCommands.MenuLoopFaces(selection);
				else
					pb.SetSelectedFaces(pb.faces);
			}
			else
			{
				pb.SetSelectedFaces(pb.faces);
			}

			UpdateSelection(false);
			SceneView.RepaintAll();
			doubleClicked = true;
		}
	}
#endregion

#region RAYCASTING AND DRAGGING

	public const float MAX_EDGE_SELECT_DISTANCE_HAM = 128;
	public const float MAX_EDGE_SELECT_DISTANCE_CTX = 12;

	pb_Object nearestEdgeObject = null;
	pb_Edge nearestEdge;

	/**
	 * If in Edge mode, finds the nearest Edge to the mouse
	 */
	private void UpdateMouse(Vector3 mousePosition)
	{
		if(selection.Length < 1 || selectionMode != SelectMode.Edge)
			return;

		GameObject go = HandleUtility.PickGameObject(mousePosition, false);

		pb_Edge bestEdge = pb_Edge.Empty;
		pb_Object bestObj = go == null ? null : go.GetComponent<pb_Object>();

		if(bestObj != null && !selection.Contains(bestObj))
			bestObj = null;

		/**
		 * If mouse isn't over a pb object, it still may be near enough to an edge.
		 */
		if(bestObj == null)
		{
			// TODO
			float bestDistance = m_HamSelection ? MAX_EDGE_SELECT_DISTANCE_HAM : MAX_EDGE_SELECT_DISTANCE_CTX;

			try {
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

			if(pb_HandleUtility.FaceRaycast(ray, bestObj, out hits, Mathf.Infinity, pb_Culling.FrontBack))
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

				if(	bestEdge.IsValid() && HandleUtility.DistanceToLine(bestObj.transform.TransformPoint(v[bestEdge.x]), bestObj.transform.TransformPoint(v[bestEdge.y])) > (m_HamSelection ? MAX_EDGE_SELECT_DISTANCE_HAM : MAX_EDGE_SELECT_DISTANCE_CTX))
					bestEdge = pb_Edge.Empty;
			}
		}

		if(bestEdge != nearestEdge || bestObj != nearestEdgeObject)
		{
			nearestEdge = bestEdge;
			nearestEdgeObject = bestObj;

			SceneView.RepaintAll();
		}
	}

	private static int m_DeepSelectionPrevious = 0x0;

	// Returns the pb_Object modified by this action.  If no action taken, or action is eaten by texture window, return null.
	// A pb_Object is returned because double click actions need to know what the last selected pb_Object was.
	// If deepClickOffset is specified, the object + deepClickOffset in the deep select stack will be returned (instead of next).
	private pb_Object RaycastCheck(Vector3 mousePosition, int deepClickOffset = 0)
	{
		pb_Object pb = null;

		// Since Edge or Vertex selection may be valid even if clicking off a gameObject, check them
		// first. If no hits, move on to face selection or object change.
		if( (selectionMode == SelectMode.Edge && EdgeClickCheck(out pb)) ||
			(selectionMode == SelectMode.Vertex && VertexClickCheck(out pb)))
		{
			UpdateSelection(false);
			SceneView.RepaintAll();
			return pb;
		}

		if(!shiftKey && !ctrlKey)
			pb_Selection.SetSelection( (GameObject)null );

		GameObject pickedGo = null;
		pb_Object pickedPb = null;
		pb_Face pickedFace = null;
		int newHash = 0;

		List<GameObject> picked = pb_EditorHandleUtility.GetAllOverlapping(mousePosition);

		EventModifiers em = Event.current.modifiers;

		// If any event modifiers are engaged don't cycle the deep click
		int pickedCount = em != EventModifiers.None ? System.Math.Min(1, picked.Count) : picked.Count;

		for(int i = 0, next = 0; i < pickedCount; i++)
		{
			GameObject go = picked[i];
			pb = go.GetComponent<pb_Object>();
			pb_Face face = null;

			if(pb != null)
			{
				Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
				pb_RaycastHit hit;

				if( pb_HandleUtility.FaceRaycast(ray,
					pb,
					out hit,
					Mathf.Infinity,
					SelectHiddenEnabled ? pb_Culling.FrontBack : pb_Culling.Front) )
				{
					face = pb.faces[hit.face];
				}
			}

			// pb_Face doesn't define GetHashCode, meaning it falls to object.GetHashCode (reference comparison)
			int hash = face == null ? go.GetHashCode() : face.GetHashCode();

			if(m_DeepSelectionPrevious == hash)
				next = (i + (1 + deepClickOffset)) % pickedCount;

			if(next == i)
			{
				pickedGo = go;
				pickedPb = pb;
				pickedFace = face;

				newHash = hash;

				// a prior hash was matched, this is the next. if
				// it's just the first iteration don't break (but do
				// set the default).
				if(next != 0)
					break;
			}
		}

		m_DeepSelectionPrevious = newHash;

		if( pickedGo != null )
		{
			Event.current.Use();

			if( pickedPb != null)
			{
				if(pickedPb.isSelectable)
				{
					pb_Selection.AddToSelection(pickedGo);

#if !PROTOTYPE
					// Check for other editor mouse shortcuts first
					pb_MaterialEditor matEditor = pb_MaterialEditor.instance;
					if( matEditor != null && matEditor.ClickShortcutCheck(Event.current.modifiers, pickedPb, pickedFace) )
						return pickedPb;

					pb_UVEditor uvEditor = pb_UVEditor.instance;
					if(uvEditor != null && uvEditor.ClickShortcutCheck(pickedPb, pickedFace))
						return pickedPb;
#endif

					// Check to see if we've already selected this quad.  If so, remove it from selection cache.
					pb_Undo.RecordSelection(pickedPb, "Change Face Selection");

					int indx = System.Array.IndexOf(pickedPb.SelectedFaces, pickedFace);

					if( indx > -1 ) {
						pickedPb.RemoveFromFaceSelectionAtIndex(indx);
					} else {
						pickedPb.AddToFaceSelection(pickedFace);
					}
				}
				else
				{
					return null;
				}
			}
			else if( !pb_PreferencesInternal.GetBool(pb_Constant.pbPBOSelectionOnly) )
			{
				// If clicked off a pb_Object but onto another gameobject, set the selection
				// and dip out.
				pb_Selection.SetSelection(pickedGo);
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

		// OnSelectionChange will also call UpdateSelection, but this needs to remain
		// because it catches element selection changes.
		UpdateSelection(false);
		SceneView.RepaintAll();

		return pickedPb;
	}

	private bool VertexClickCheck(out pb_Object vpb)
	{
		if(!shiftKey && !ctrlKey)
			ClearElementSelection();

		Camera cam = SceneView.lastActiveSceneView.camera;
		Vector2 m = Event.current.mousePosition;
		List<pb_Tuple<float, Vector3, int, int>> nearest = new List<pb_Tuple<float, Vector3, int, int>>();

		// this could be much faster by raycasting against the mesh and doing a 3d space
		// distance check first

		if(m_HamSelection)
		{
			float best = MAX_EDGE_SELECT_DISTANCE_HAM * MAX_EDGE_SELECT_DISTANCE_HAM;
			int obj = -1, tri = -1;

			for(int i = 0; i < selection.Length; i++)
			{
				pb_Object pb = selection[i];

				if(!pb.isSelectable)
					continue;

				for(int n = 0; n < m_uniqueIndices[i].Length; n++)
				{
					Vector3 v = m_verticesInWorldSpace[i][m_uniqueIndices[i][n]];
					Vector2 p = HandleUtility.WorldToGUIPoint(v);

					float dist = (p - m).sqrMagnitude;

					if(dist < best)
						nearest.Add(new pb_Tuple<float, Vector3, int, int>(dist, v, i, m_uniqueIndices[i][n]));
				}
			}

			nearest.Sort( (x, y) => x.Item1.CompareTo(y.Item1) );

			for(int i = 0; i < nearest.Count; i++)
			{
				obj = nearest[i].Item3;

				if(!pb_HandleUtility.PointIsOccluded(cam, selection[obj], nearest[i].Item2))
				{
					tri = nearest[i].Item4;
					break;
				}
			}

			if(obj > -1 && tri > -1)
			{
				pb_Object pb = selection[obj];

				int indx = System.Array.IndexOf(pb.SelectedTriangles, tri);

				pb_Undo.RecordSelection(pb, "Change Vertex Selection");

				// If we get a match, check to see if it exists in our selection array already, then add / remove
				if( indx > -1 )
					pb.SetSelectedTriangles(pb.SelectedTriangles.RemoveAt(indx));
				else
					pb.SetSelectedTriangles(pb.SelectedTriangles.Add(tri));

				vpb = pb;
				return true;
			}
		}
		else
		{
			for(int i = 0; i < selection.Length; i++)
			{
				pb_Object pb = selection[i];

				if(!pb.isSelectable)
					continue;

				for(int n = 0; n < m_uniqueIndices[i].Length; n++)
				{
					Vector3 v = m_verticesInWorldSpace[i][m_uniqueIndices[i][n]];

					if(mouseRect.Contains(HandleUtility.WorldToGUIPoint(v)))
					{
						if( pb_HandleUtility.PointIsOccluded(cam, pb, v) )
							continue;

						// Check if index is already selected, and if not add it to the pot
						int indx = System.Array.IndexOf(pb.SelectedTriangles, m_uniqueIndices[i][n]);

						pb_Undo.RecordObject(pb, "Change Vertex Selection");

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
		}

		vpb = null;
		return false;
	}

	private bool EdgeClickCheck(out pb_Object pb)
	{
		if(!shiftKey && !ctrlKey)
		{
			// don't call ClearElementSelection b/c that also removes
			// nearestEdge info
			foreach(pb_Object p in selection)
				p.ClearSelection();
		}

		if(nearestEdgeObject != null)
		{
			pb = nearestEdgeObject;

			if(nearestEdge.IsValid())
			{
				pb_Tuple<pb_Face, pb_Edge> edge;

				if( pb_EdgeExtension.ValidateEdge(pb, nearestEdge, out edge) )
					nearestEdge = edge.Item2;

				int ind = pb.SelectedEdges.IndexOf(nearestEdge, pb.sharedIndices.ToDictionary());

				pb_Undo.RecordSelection(pb, "Change Edge Selection");

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
				ClearElementSelection();

			pb = null;

			return false;
		}
	}

	void DragCheck()
	{
		SceneView sceneView = SceneView.lastActiveSceneView;
		Camera cam = sceneView.camera;

		pb_Undo.RecordSelection(selection, "Drag Select");

		m_LimitFaceDragToSelection = pb_PreferencesInternal.GetBool(pb_Constant.pbDragCheckLimit);
		bool selectWholeElement = pb_PreferencesInternal.GetBool(pb_Constant.pbDragSelectWholeElement);
		bool selectHidden = SelectHiddenEnabled;

		var pickingOptions = new pb_PickerOptions()
		{
			depthTest = !selectHidden,
			rectSelectMode = selectWholeElement ? pb_RectSelectMode.Complete : pb_RectSelectMode.Partial
		};

		switch(selectionMode)
		{
			case SelectMode.Vertex:
			{
				if(!shiftKey && !ctrlKey)
					ClearElementSelection();

				Dictionary<pb_Object, HashSet<int>> selected = pb_Picking.PickVerticesInRect(
					SceneView.lastActiveSceneView.camera,
					selectionRect,
					selection,
					pickingOptions,
					EditorGUIUtility.pixelsPerPoint );

				foreach(var kvp in selected)
				{
					pb_IntArray[] sharedIndices = kvp.Key.sharedIndices;
					HashSet<int> common;

					if(shiftKey || ctrlKey)
					{
						common = sharedIndices.GetCommonIndices(kvp.Key.SelectedTriangles);

						if(dragSelectMode == DragSelectMode.Add)
							common.UnionWith(kvp.Value);
						else if(dragSelectMode == DragSelectMode.Subtract)
							common.RemoveWhere(x => kvp.Value.Contains(x));
						else if(dragSelectMode == DragSelectMode.Difference)
							common.SymmetricExceptWith(kvp.Value);
					}
					else
					{
						common = kvp.Value;
					}

					kvp.Key.SetSelectedTriangles( sharedIndices.GetIndicesWithCommon(common).ToArray() );
				}

				if(!m_VertexSelectionMask)
					DragObjectCheck(true);

				UpdateSelection(false);
			}
			break;

			case SelectMode.Face:
			{
				if(!shiftKey && !ctrlKey)
					ClearElementSelection();

				Dictionary<pb_Object, HashSet<pb_Face>> selected = pb_Picking.PickFacesInRect(
					SceneView.lastActiveSceneView.camera,
					selectionRect,
					selection,
					pickingOptions,
					EditorGUIUtility.pixelsPerPoint);

				foreach(var kvp in selected)
				{
					HashSet<pb_Face> current;

					if(shiftKey || ctrlKey)
					{
						current = new HashSet<pb_Face>(kvp.Key.SelectedFaces);

						if(dragSelectMode == DragSelectMode.Add)
							current.UnionWith(kvp.Value);
						else if(dragSelectMode == DragSelectMode.Subtract)
							current.RemoveWhere(x => kvp.Value.Contains(x));
						else if(dragSelectMode == DragSelectMode.Difference)
							current.SymmetricExceptWith(kvp.Value);
					}
					else
					{
						current = kvp.Value;
					}

					kvp.Key.SetSelectedFaces(current);
				}


				DragObjectCheck(true);
				UpdateSelection(false);
			}
			break;

			case SelectMode.Edge:
			{
				if(!shiftKey && !ctrlKey)
					ClearElementSelection();

				var selected = pb_Picking.PickEdgesInRect(
					SceneView.lastActiveSceneView.camera,
					selectionRect,
					selection,
					pickingOptions,
					EditorGUIUtility.pixelsPerPoint);

				foreach(var kvp in selected)
				{
					pb_Object pb = kvp.Key;
					Dictionary<int, int> commonIndices = pb.sharedIndices.ToDictionary();
					HashSet<pb_EdgeLookup> selectedEdges = pb_EdgeLookup.GetEdgeLookupHashSet(kvp.Value, commonIndices);

					HashSet<pb_EdgeLookup> current;

					if(shiftKey || ctrlKey)
					{
						current = pb_EdgeLookup.GetEdgeLookupHashSet(pb.SelectedEdges, commonIndices);

						if(dragSelectMode == DragSelectMode.Add)
							current.UnionWith(selectedEdges);
						else if(dragSelectMode == DragSelectMode.Subtract)
							current.RemoveWhere(x => selectedEdges.Contains(x));
						else if(dragSelectMode == DragSelectMode.Difference)
							current.SymmetricExceptWith(selectedEdges);
					}
					else
					{
						current = selectedEdges;
					}

					pb.SetSelectedEdges(current.Select(x => x.local));
				}

				if(!m_VertexSelectionMask)
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
			if(!shiftKey) pb_Selection.ClearElementAndObjectSelection();
		} else {
			if(!shiftKey && selectedVertexCount > 0) return;
		}

		// scan for new selected objects
		/// if mode based, don't allow selection of non-probuilder objects
		if(!m_LimitFaceDragToSelection)
		{
			foreach(pb_Object g in HandleUtility.PickRectObjects(selectionRect).GetComponents<pb_Object>())
				if(!Selection.Contains(g.gameObject))
					pb_Selection.AddToSelection(g.gameObject);
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
				Dictionary<pb_Object, HashSet<pb_Face>> ignore = new Dictionary<pb_Object, HashSet<pb_Face>>();
				foreach(pb_Object pb in selection)
					ignore.Add(pb, new HashSet<pb_Face>(pb.SelectedFaces));

				if( pb_EditorHandleUtility.FaceRaycast(mousePosition, out obj, out hit, ignore) )
				{
					if( mask.IntSum() == 1 )
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

				pb_ProGridsInterface.OnHandleMove(mask);
			}

			for(int i = 0; i < selection.Length; i++)
			{
				selection[i].TranslateVertices_World(selection[i].SelectedTriangles, diff, m_SnapEnabled ? m_SnapValue : 0f, m_SnapAxisConstraint, m_sharedIndicesLookup[i]);
				selection[i].RefreshUV( SelectedFacesInEditZone[selection[i]] );
				selection[i].Refresh(RefreshMask.Normals);
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
			// 	pbUndo.RecordSelection(selection as Object[], "Move Vertices");

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
				selection[i].RefreshUV( SelectedFacesInEditZone[selection[i]] );
				selection[i].Refresh(RefreshMask.Normals);
				selection[i].msh.RecalculateBounds();
			}

			Internal_UpdateSelectionFast();
		}
	}

	Quaternion m_HandleRotation = Quaternion.identity;
	Quaternion m_InverseRotation = Quaternion.identity;

	private void VertexRotateTool()
	{
		if(!movingVertices)
			newPosition = m_handlePivotWorld;

		previousHandleRotation = currentHandleRotation;

		if(altClick)
			Handles.RotationHandle(currentHandleRotation, newPosition);
		else
			currentHandleRotation = Handles.RotationHandle(currentHandleRotation, newPosition);

		if(currentHandleRotation != previousHandleRotation)
		{
			// profiler.BeginSample("Rotate");
			if(!movingVertices)
			{
				movingVertices = true;

				translateOrigin = cachedPosition;
				rotateOrigin = currentHandleRotation.eulerAngles;
				scaleOrigin = currentHandleScale;

				m_HandleRotation = previousHandleRotation;
				m_InverseRotation = Quaternion.Inverse(previousHandleRotation);

				OnBeginVertexMovement();

				if(Event.current.modifiers == EventModifiers.Shift)
					ShiftExtrude();

				// cache vertex positions for modifying later
				vertexOrigins = new Vector3[selection.Length][];
				vertexOffset = new Vector3[selection.Length];

				for(int i = 0; i < selection.Length; i++)
				{
					Vector3[] vertices = selection[i].vertices;
					int[] triangles = selection[i].SelectedTriangles;
					vertexOrigins[i] = new Vector3[triangles.Length];

					for(int nn = 0; nn < triangles.Length; nn++)
						vertexOrigins[i][nn] = selection[i].transform.TransformPoint(vertices[triangles[nn]]);

					if(handleAlignment == HandleAlignment.World)
						vertexOffset[i] = newPosition;
					else
						vertexOffset[i] = pb_Math.BoundsCenter(vertexOrigins[i]);
				}
			}

			// profiler.BeginSample("Calc Matrix");
			Quaternion transformedRotation = m_InverseRotation * currentHandleRotation;

			// profiler.BeginSample("matrix mult");
			Vector3 ver;	// resulting vertex from modification
			for(int i = 0; i < selection.Length; i++)
			{
				Vector3[] v = selection[i].vertices;
				pb_IntArray[] sharedIndices = selection[i].sharedIndices;

				Quaternion lr = m_HandleRotation;// selection[0].transform.localRotation;
				Quaternion ilr = m_InverseRotation;// Quaternion.Inverse(lr);

				for(int n = 0; n < selection[i].SelectedTriangles.Length; n++)
				{
					// move vertex to relative origin from center of selection
					ver = ilr * (vertexOrigins[i][n] - vertexOffset[i]);

					// rotate
					ver = transformedRotation * ver;

					// move vertex back to locally offset position
					ver = (lr * ver) + vertexOffset[i];

					int[] array = sharedIndices[m_sharedIndicesLookup[i][selection[i].SelectedTriangles[n]]].array;

					for(int t = 0; t < array.Length; t++)
						v[array[t]] = selection[i].transform.InverseTransformPoint(ver);
				}

				selection[i].SetVertices(v);
				selection[i].msh.vertices = v;
				selection[i].RefreshUV( SelectedFacesInEditZone[selection[i]] );
				selection[i].Refresh(RefreshMask.Normals);
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
												pb_PreferencesInternal.GetBool(pb_Constant.pbExtrudeAsGroup),
												pb_PreferencesInternal.GetBool(pb_Constant.pbManifoldEdgeExtrusion),
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
						pb.Extrude(pb.SelectedFaces, pb_PreferencesInternal.GetEnum<ExtrudeMethod>(pb_Constant.pbExtrudeMethod), 0.0001f);
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
			pb_EditorUtility.ShowNotification("Extrude");
			UpdateSelection(true);
		}
	}

#if !PROTOTYPE
	Vector3 textureHandle = Vector3.zero;
	Vector3 previousTextureHandle = Vector3.zero;
	bool movingPictures = false;

	private void TextureMoveTool()
	{
		pb_UVEditor uvEditor = pb_UVEditor.instance;
		if(!uvEditor) return;

		Vector3 cached = textureHandle;

		textureHandle = Handles.PositionHandle(textureHandle, handleRotation);

		if(altClick) return;

		if(textureHandle != cached)
		{
			cached = Quaternion.Inverse(handleRotation) * textureHandle;
			cached.y = -cached.y;

			Vector3 lossyScale = selection[0].transform.lossyScale;
			Vector3 position = cached.DivideBy(lossyScale);

			if(!movingPictures)
			{
				previousTextureHandle = position;
				movingPictures = true;
			}

			uvEditor.SceneMoveTool( position - previousTextureHandle );
			previousTextureHandle = position;

			uvEditor.Repaint();
		}
	}

	Quaternion textureRotation = Quaternion.identity;
	private void TextureRotateTool()
	{
		pb_UVEditor uvEditor = pb_UVEditor.instance;
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
		pb_UVEditor uvEditor = pb_UVEditor.instance;
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

		if(m_ShowToolbar)
		{
			int screenWidth = (int) sceneView.position.width;
			int screenHeight = (int) sceneView.position.height;

			int t_selectionMode = (editLevel != EditLevel.Top && editLevel != EditLevel.Plugin) ? ((int)selectionMode) + 1 : 0;

			switch(m_SceneToolbarLocation)
			{
				case SceneToolbarLocation.BottomCenter:
					elementModeToolbarRect.x = (screenWidth/2 - 64);
					elementModeToolbarRect.y = screenHeight - elementModeToolbarRect.height * 3;
					break;

				case SceneToolbarLocation.BottomLeft:
					elementModeToolbarRect.x = 12;
					elementModeToolbarRect.y = screenHeight - elementModeToolbarRect.height * 3;
					break;

				case SceneToolbarLocation.BottomRight:
					elementModeToolbarRect.x = screenWidth - (elementModeToolbarRect.width + 12);
					elementModeToolbarRect.y = screenHeight - elementModeToolbarRect.height * 3;
					break;

				case SceneToolbarLocation.UpperLeft:
					elementModeToolbarRect.x = 12;
					elementModeToolbarRect.y = 10;
					break;

				case SceneToolbarLocation.UpperRight:
					elementModeToolbarRect.x = screenWidth - (elementModeToolbarRect.width + 96);
					elementModeToolbarRect.y = 10;
					break;

				default:
				case SceneToolbarLocation.UpperCenter:
					elementModeToolbarRect.x = (screenWidth/2 - 64);
					elementModeToolbarRect.y = 10;
					break;
			}

			EditorGUI.BeginChangeCheck();

			t_selectionMode = GUI.Toolbar(elementModeToolbarRect, (int)t_selectionMode, EditModeIcons, commandStyle);

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

		if(movingVertices && m_ShowSceneInfo)
		{
			GUI.backgroundColor = pb_Constant.ProBuilderLightGray;

			GUI.Label(new Rect(Screen.width-200, Screen.height-120, 162, 48),
				"Translate: " + (newPosition-translateOrigin).ToString() +
				"\nRotate: " + (currentHandleRotation.eulerAngles-rotateOrigin).ToString() +
				"\nScale: " + (currentHandleScale-scaleOrigin).ToString()
				, VertexTranslationInfoStyle
				);
		}

		if( m_ShowSceneInfo )
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

				pb_EditorGUIUtility.DrawSolidColor( new Rect(sceneInfoRect.x-4, sceneInfoRect.y-4, sceneInfoRect.width, sceneInfoRect.height), new Color(.1f,.1f,.1f,.55f));

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
				pb_EditorUtility.ShowNotification(usedShortcut.action);

			Event.current.Use();
		}

		return used;
	}

	private bool AllLevelShortcuts(pb_Shortcut shortcut)
	{
		bool uniqueModeShortcuts = pb_PreferencesInternal.GetBool(pb_Constant.pbUniqueModeShortcuts);

		switch(shortcut.action)
		{
			// TODO Remove once a workaround for non-upper-case shortcut chars is found
			case "Toggle Geometry Mode":

				if(editLevel == EditLevel.Geometry)
				{
					pb_EditorUtility.ShowNotification("Top Level Editing");
					SetEditLevel(EditLevel.Top);
				}
				else if( !uniqueModeShortcuts )
				{
					pb_EditorUtility.ShowNotification("Geometry Editing");
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

	private bool GeoLevelShortcuts(pb_Shortcut shortcut)
	{
		switch(shortcut.action)
		{
			case "Escape":
				ClearElementSelection();
				pb_EditorUtility.ShowNotification("Top Level");
				UpdateSelection(false);
				SetEditLevel(EditLevel.Top);
				return true;

			// TODO Remove once a workaround for non-upper-case shortcut chars is found
			case "Toggle Selection Mode":

				if( pb_PreferencesInternal.GetBool(pb_Constant.pbUniqueModeShortcuts) )
					return false;

				ToggleSelectionMode();
				switch(selectionMode)
				{
					case SelectMode.Face:
						pb_EditorUtility.ShowNotification("Editing Faces");
						break;

					case SelectMode.Vertex:
						pb_EditorUtility.ShowNotification("Editing Vertices");
						break;

					case SelectMode.Edge:
						pb_EditorUtility.ShowNotification("Editing Edges");
						break;
				}
				return true;

			case "Delete Face":
				pb_EditorUtility.ShowNotification(pb_MenuCommands.MenuDeleteFace(selection).notification);
				return true;

			/* handle alignment */
			case "Toggle Handle Pivot":
				if(selectedVertexCount < 1)
					return false;

				if(editLevel != EditLevel.Texture)
				{
					ToggleHandleAlignment();
					pb_EditorUtility.ShowNotification("Handle Alignment: " + ((HandleAlignment)handleAlignment).ToString());
				}
				return true;

			case "Set Pivot":

				if (selection.Length > 0)
				{
					foreach (pb_Object pbo in selection)
					{
						pb_Undo.RecordObjects(new Object[2] { pbo, pbo.transform }, "Set Pivot");

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
		if(pb_UVEditor.instance != null)
			pb_UVEditor.instance.SetTool(newTool);
#endif
	}

	public void SetHandleAlignment(HandleAlignment ha)
	{
		if(editLevel == EditLevel.Texture)
			ha = HandleAlignment.Plane;
		else
			pb_PreferencesInternal.SetInt(pb_Constant.pbHandleAlignment, (int)ha);

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

		pb_PreferencesInternal.SetInt(pb_Constant.pbDefaultSelectionMode, (int)selectionMode);

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
				ClearElementSelection();
				UpdateSelection(true);

				pb_Selection.SetSelection(Selection.gameObjects);
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
			pb_PreferencesInternal.SetInt(pb_Constant.pbDefaultEditLevel, (int)editLevel);

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
	// public pb_Face[][] 	SelectedFacesInEditZone { get; private set; }
	public Dictionary<pb_Object, List<pb_Face>> SelectedFacesInEditZone { get; private set; }

	// The number of selected distinct indices on the object with the greatest number of selected distinct indices.
	int per_object_vertexCount_distinct = 0;

	int faceCount = 0;
	int vertexCount = 0;
	int triangleCount = 0;

	// todo remove this manual selection caching junk
	public void UpdateSelection(bool forceUpdate = true)
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

		selection = pb_Util.GetComponents<pb_Object>(Selection.transforms);

		if(SelectedFacesInEditZone != null)
			SelectedFacesInEditZone.Clear();
		else
			SelectedFacesInEditZone = new Dictionary<pb_Object, List<pb_Face>>();

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
				m_universalEdges[i] = pb_EdgeExtension.GetUniversalEdges(pb_EdgeExtension.AllEdges(selection[i].faces), m_sharedIndicesLookup[i]);
				// profiler.EndSample();
			}
			// profiler.EndSample();
		}

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
					foreach(Vector3 v in pb_Util.ValuesWithIndices(m_verticesInWorldSpace[i], pb.SelectedTriangles))
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

			SelectedFacesInEditZone.Add(pb, pb_MeshUtils.GetNeighborFaces(pb, pb.SelectedTriangles).ToList() );

			selectedVertexCount += selection[i].SelectedTriangles.Length;
			selectedFaceCount += selection[i].SelectedFaceCount;
			selectedEdgeCount += selection[i].SelectedEdges.Length;

			int distinctVertexCount = selection[i].sharedIndices.UniqueIndicesWithValues(selection[i].SelectedTriangles).ToList().Count;

			if(distinctVertexCount > per_object_vertexCount_distinct)
				per_object_vertexCount_distinct = distinctVertexCount;

			faceCount += selection[i].faces.Length;
			vertexCount += selection[i].sharedIndices.Length; // vertexCount;
			triangleCount += selection[i].msh != null ? selection[i].msh.triangles.Length / 3 : selection[i].faces.Length;
		}

		m_handlePivotWorld = (max+min)/2f;

		UpdateGraphics();

		UpdateHandleRotation();

#if !PROTOTYPE
		UpdateTextureHandles();
#endif

		currentHandleRotation = handleRotation;

		if(OnSelectionUpdate != null)
			OnSelectionUpdate(selection);

		// profiler.EndSample();
	}

	// Only updates things that absolutely need to be refreshed, and assumes that no selection changes have occured
	internal void Internal_UpdateSelectionFast()
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
			selectedFaceCount 	+= selection[i].SelectedFaceCount;
			selectedEdgeCount 	+= selection[i].SelectedEdges.Length;
		}

		m_handlePivotWorld = (max+min)/2f;

		UpdateGraphics();
		UpdateHandleRotation();
		currentHandleRotation = handleRotation;

		if(OnSelectionUpdate != null)
			OnSelectionUpdate(selection);

		// profiler.EndSample();
	}

	void UpdateGraphics()
	{
		graphics.RebuildGraphics(selection, editLevel, selectionMode);
	}

	public void ClearElementSelection()
	{
		foreach(pb_Object pb in selection)
			pb.ClearSelection();

		nearestEdge = pb_Edge.Empty;
		nearestEdgeObject = null;
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
		List<Transform> t = new List<Transform>((Transform[])pb_Util.GetComponents<Transform>(HandleUtility.PickRectObjects(new Rect(0,0,Screen.width,Screen.height))));

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
		nearestEdge = pb_Edge.Empty;
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
			pb_EditorUtility.SetSelectionRenderState(pb.gameObject.GetComponent<Renderer>(), pb_EditorUtility.GetSelectionRenderState() & SelectionRenderState.Outline);

		SceneView.RepaintAll();
	}

	/**
	 * Called from ProGrids.
	 */
	private void PushToGrid(float snapVal)
	{
		pb_Undo.RecordSelection(selection, "Push elements to Grid");

		if( editLevel == EditLevel.Top )
			return;

		for(int i = 0; i  < selection.Length; i++)
		{
			pb_Object pb = selection[i];

			int[] indices = pb.SelectedTriangleCount > 0 ? pb.sharedIndices.AllIndicesWithValues(pb.SelectedTriangles).ToArray() : pb.msh.triangles;

			pb_VertexOps.Quantize(pb, indices, Vector3.one * snapVal);

			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();
		}

		Internal_UpdateSelectionFast();
	}

	private void ProGridsToolbarOpen(bool menuOpen)
	{
		bool active = pb_ProGridsInterface.ProGridsActive();
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
				pb_Undo.RegisterCompleteObjectUndo(selection, "Translate Vertices");
				break;

			case Tool.Rotate:
				pb_Undo.RegisterCompleteObjectUndo(selection, "Rotate Vertices");
				break;

			case Tool.Scale:
				pb_Undo.RegisterCompleteObjectUndo(selection, "Scale Vertices");
				break;

			default:
				pb_Undo.RegisterCompleteObjectUndo(selection, "Modify Vertices");
				break;
		}

		m_SnapEnabled = pb_ProGridsInterface.SnapEnabled();
		m_SnapValue = pb_ProGridsInterface.SnapValue();
		m_SnapAxisConstraint = pb_ProGridsInterface.UseAxisConstraints();

		// Disable iterative lightmapping
		pb_Lightmapping.PushGIWorkflowMode();

		// profiler.BeginSample("ResetMesh");
		foreach(pb_Object pb in selection)
		{
			pb.ToMesh();
			pb.Refresh();
		}

		// profiler.EndSample();

		if(OnVertexMovementBegin != null)
			OnVertexMovementBegin(selection);
	}

	private void OnFinishVertexModification()
	{
		pb_Lightmapping.PopGIWorkflowMode();

		currentHandleScale = Vector3.one;
		currentHandleRotation = handleRotation;

#if !PROTOTYPE
		if(movingPictures)
		{
			if(pb_UVEditor.instance != null)
				pb_UVEditor.instance.OnFinishUVModification();

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

		if(OnVertexMovementFinish != null)
			OnVertexMovementFinish(selection);

		scaling = false;
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

		pb = selection.FirstOrDefault(x => x.SelectedFaceCount > 0);

		if(pb == null)
			return false;

		face = pb.SelectedFaces[0];

		return true;
	}

	/**
	 * Returns the first selected pb_Object and pb_Face, or false if not found.
	 */
	public bool GetFirstSelectedMaterial(ref Material mat)
	{
		for(int i = 0; i < selection.Length; i++)
		{
			for(int n = 0; n < selection[i].SelectedFaceCount; n++)
			{
				mat = selection[i].SelectedFaces[i].material;

				if(mat != null)
					return true;
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

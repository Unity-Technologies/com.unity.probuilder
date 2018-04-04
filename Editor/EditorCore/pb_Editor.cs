using System;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using ProBuilder.Core;
using ProBuilder.Interface;
using ProBuilder.MeshOperations;
using Object = UnityEngine.Object;

namespace ProBuilder.EditorCore
{
	public delegate void OnSelectionUpdateEventHandler(pb_Object[] selection);

	public delegate void OnVertexMovementBeginEventHandler(pb_Object[] selection);

	public delegate void OnVertexMovementFinishedEventHandler(pb_Object[] selection);

	public class pb_Editor : EditorWindow
	{
		public static event OnSelectionUpdateEventHandler onSelectionUpdate;

		// Called when vertex modifications are complete.
		public static event OnVertexMovementFinishedEventHandler onVertexMovementFinish;

		// Called immediately prior to beginning vertex modifications. pb_Object will be in un-altered state at this point (meaning ToMesh and Refresh have been called, but not Optimize).
		public static event OnVertexMovementBeginEventHandler onVertexMovementBegin;

		// Toggles for Face, Vertex, and Edge mode.
		const int k_SelectModeLength = 3;

		GUIContent[] m_EditModeIcons;
		GUIStyle VertexTranslationInfoStyle;
		public static Action<int> onEditLevelChanged;

		bool m_ShowSceneInfo = false;
		bool m_HamSelection = false;

		float m_SnapValue = .25f;
		bool m_SnapAxisConstraint = true;
		bool m_SnapEnabled = false;
		bool m_IsIconGui = false;
		// Needs to be initialized from an instance, not a static class. Don't move to HandleUtility, you tried that already.
		MethodInfo findNearestVertex;
		EditLevel m_PreviousEditLevel;
		SelectMode m_PreviousSelectMode;
		HandleAlignment m_PreviousHandleAlignment;
		public DragSelectMode dragSelectMode = DragSelectMode.Difference;
		static pb_EditorToolbar s_EditorToolbar = null;
		pb_Shortcut[] m_Shortcuts;
		static pb_Editor s_Instance;
		SceneToolbarLocation m_SceneToolbarLocation = SceneToolbarLocation.UpperCenter;
		GUIStyle commandStyle = null;
		Rect elementModeToolbarRect = new Rect(3, 6, 128, 24);
		bool m_SelectHiddenEnabled;

		const float k_MaxEdgeSelectDistanceHam = 128;
		const float k_MaxEdgeSelectDistanceCtx = 12;

		pb_Object nearestEdgeObject = null;
		pb_Edge nearestEdge;

		// the mouse vertex selection box
		Rect m_MouseClickRect = new Rect(0, 0, 0, 0);
		Tool currentHandle = Tool.Move;
		Vector2 mousePosition_initial;
		Rect m_MouseDragRect;
		bool m_IsDragging = false, readyForMouseDrag = false;
		// prevents leftClickUp from stealing focus after double click
		bool doubleClicked = false;
		// vertex handles
		Vector3 newPosition, cachedPosition;
		bool movingVertices = false;
		// top level caching
		bool scaling = false;
		bool rightMouseDown = false;
		static int s_DeepSelectionPrevious = 0x0;

		bool snapToVertex = false;
		bool snapToFace = false;
		Vector3 previousHandleScale = Vector3.one;
		Vector3 currentHandleScale = Vector3.one;
		Vector3[][] vertexOrigins;
		Vector3[] vertexOffset;
		Quaternion previousHandleRotation = Quaternion.identity;
		Quaternion currentHandleRotation = Quaternion.identity;

		GUIContent m_SceneInfo = new GUIContent();

		// Use for delta display
		Vector3 translateOrigin = Vector3.zero;
		Vector3 rotateOrigin = Vector3.zero;
		Vector3 scaleOrigin = Vector3.zero;

		Quaternion m_HandleRotation = Quaternion.identity;
		Quaternion m_InverseRotation = Quaternion.identity;

		Vector3 textureHandle = Vector3.zero;
		Vector3 previousTextureHandle = Vector3.zero;
		bool movingPictures = false;
		Quaternion textureRotation = Quaternion.identity;
		Vector3 textureScale = Vector3.one;
		Rect sceneInfoRect = new Rect(10, 10, 200, 40);

		pb_Edge[][] m_UniversalEdges = new pb_Edge[0][];
		Vector3 m_HandlePivotWorld = Vector3.zero;
		Dictionary<int, int>[] m_SharedIndicesDictionary = new Dictionary<int, int>[0];

		public pb_Edge[][] SelectedUniversalEdges
		{
			get { return m_UniversalEdges; }
		}

		// faces that need to be refreshed when moving or modifying the actual selection
		// public pb_Face[][] 	SelectedFacesInEditZone { get; private set; }
		public Dictionary<pb_Object, List<pb_Face>> SelectedFacesInEditZone { get; private set; }

		Matrix4x4 handleMatrix = Matrix4x4.identity;
		Quaternion handleRotation = new Quaternion(0f, 0f, 0f, 1f);

#if !UNITY_2018_2_OR_NEWER
		static MethodInfo s_ResetOnSceneGUIState = null;
#endif

		internal pb_Object[] selection = new pb_Object[0]; // All selected pb_Objects

		// Sum of all vertices selected
		int m_SelectedVertexCount;

		// Sum of all vertices selected, not counting duplicates on common positions
		int m_SelectedVerticesCommon;

		// Sum of all faces selected
		int m_SelectedFaceCount;

		// Sum of all edges sleected
		int m_SelectedEdgeCount;

		public int selectedVertexCount { get { return m_SelectedVertexCount; } }
		public int selectedVertexCommonCount { get { return m_SelectedVerticesCommon; } }
		public int selectedFaceCount { get { return m_SelectedFaceCount; } }
		public int selectedEdgeCount { get { return m_SelectedEdgeCount; } }

		Event m_CurrentEvent;

		public bool isFloatingWindow { get; private set; }
		public EditLevel editLevel { get; private set; }
		public SelectMode selectionMode { get; private set; }
		public HandleAlignment handleAlignment { get; private set; }
		public bool selectHiddenEnabled { get { return m_SelectHiddenEnabled; } }

		static class SceneStyles
		{
			static bool m_Init = false;
			static GUIStyle m_SelectionRect;

			public static GUIStyle selectionRect
			{
				get { return m_SelectionRect; }
			}

			public static void Init()
			{
				if (m_Init)
					return;

				m_Init = true;

				m_SelectionRect = new GUIStyle()
				{
					normal = new GUIStyleState()
					{
						background = pb_IconUtility.GetIcon("Scene/SelectionRect")
					},
					border = new RectOffset(1,1,1,1),
					margin = new RectOffset(0,0,0,0),
					padding = new RectOffset(0,0,0,0)
				};
			}
		}

		/// <summary>
		/// Subscribe to notifications of edit level changes.
		/// </summary>
		/// <param name="listener"></param>
		public static void AddOnEditLevelChangedListener(System.Action<int> listener)
		{
			onEditLevelChanged += listener;
		}

		public static void RemoveOnEditLevelChangedListener(System.Action<int> listener)
		{
			onEditLevelChanged -= listener;
		}

		public static pb_Editor instance
		{
			get { return s_Instance; }
		}

		/// <summary>
		/// Open the pb_Editor window with whatever dockable status is preference-d.
		/// </summary>
		/// <returns></returns>
		public static pb_Editor MenuOpenWindow()
		{
			pb_Editor editor = (pb_Editor) EditorWindow.GetWindow(typeof(pb_Editor),
				!pb_PreferencesInternal.GetBool(pb_Constant.pbDefaultOpenInDockableWindow), pb_Constant.PRODUCT_NAME,
				true); // open as floating window
			// would be nice if editorwindow's showMode was exposed
			editor.isFloatingWindow = !pb_PreferencesInternal.GetBool(pb_Constant.pbDefaultOpenInDockableWindow);
			return editor;
		}

		internal void OnEnable()
		{
			s_Instance = this;

			pb_MeshHandles.Initialize();

			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			SceneView.onSceneGUIDelegate += this.OnSceneGUI;

			pb_ProGridsInterface.SubscribePushToGridEvent(PushToGrid);
			pb_ProGridsInterface.SubscribeToolbarEvent(ProGridsToolbarOpen);

			ProGridsToolbarOpen(pb_ProGridsInterface.SceneToolbarIsExtended());

			pb_Selection.onObjectSelectionChanged += OnObjectSelectionChanged;

#if !UNITY_2018_2_OR_NEWER
			s_ResetOnSceneGUIState = typeof(SceneView).GetMethod("ResetOnSceneGUIState", BindingFlags.Instance | BindingFlags.NonPublic);
#endif

			// make sure load prefs is called first, because other methods depend on the preferences set here
			LoadPrefs();
			InitGUI();
			UpdateSelection(true);
			HideSelectedWireframe();

			findNearestVertex = typeof(HandleUtility).GetMethod("FindNearestVertex",
				BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);

			if (onEditLevelChanged != null)
				onEditLevelChanged((int) editLevel);
		}

		void OnDisable()
		{
			s_Instance = null;

			if (s_EditorToolbar != null)
				DestroyImmediate(s_EditorToolbar);

			ClearElementSelection();

			UpdateSelection();

			pb_MeshHandles.Destroy();

			if (onSelectionUpdate != null)
				onSelectionUpdate(null);

			pb_ProGridsInterface.UnsubscribePushToGridEvent(PushToGrid);
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			pb_PreferencesInternal.SetInt(pb_Constant.pbHandleAlignment, (int) handleAlignment);
			pb_Selection.onObjectSelectionChanged -= OnObjectSelectionChanged;

			if (pb_LineRenderer.Valid())
				pb_LineRenderer.instance.Clear();

			// re-enable unity wireframe
			// todo set wireframe override in pb_Selection, no pb_Editor
			foreach (var pb in FindObjectsOfType<pb_Object>())
				pb_EditorUtility.SetSelectionRenderState(pb.gameObject.GetComponent<Renderer>(),
					pb_EditorUtility.GetSelectionRenderState());

			SceneView.RepaintAll();
		}

		void OnDestroy()
		{
			if (pb_LineRenderer.nullableInstance != null)
				DestroyImmediate(pb_LineRenderer.nullableInstance.gameObject);
		}

		internal void LoadPrefs()
		{
			pb_PreferencesUpdater.CheckEditorPrefsVersion();

			editLevel = pb_PreferencesInternal.GetEnum<EditLevel>(pb_Constant.pbDefaultEditLevel);
			selectionMode = pb_PreferencesInternal.GetEnum<SelectMode>(pb_Constant.pbDefaultSelectionMode);
			handleAlignment = pb_PreferencesInternal.GetEnum<HandleAlignment>(pb_Constant.pbHandleAlignment);
			m_ShowSceneInfo = pb_PreferencesInternal.GetBool(pb_Constant.pbShowSceneInfo);
			m_HamSelection = pb_PreferencesInternal.GetBool(pb_Constant.pbElementSelectIsHamFisted);
			m_SelectHiddenEnabled = pb_PreferencesInternal.GetBool(pb_Constant.pbEnableBackfaceSelection);

			m_SnapEnabled = pb_ProGridsInterface.SnapEnabled();
			m_SnapValue = pb_ProGridsInterface.SnapValue();
			m_SnapAxisConstraint = pb_ProGridsInterface.UseAxisConstraints();

			m_Shortcuts = pb_Shortcut.ParseShortcuts(pb_PreferencesInternal.GetString(pb_Constant.pbDefaultShortcuts)).ToArray();

			m_SceneToolbarLocation = pb_PreferencesInternal.GetEnum<SceneToolbarLocation>(pb_Constant.pbToolbarLocation);
			m_IsIconGui = pb_PreferencesInternal.GetBool(pb_Constant.pbIconGUI);
			dragSelectMode = pb_PreferencesInternal.GetEnum<DragSelectMode>(pb_Constant.pbDragSelectMode);
		}

		void InitGUI()
		{
			if (s_EditorToolbar != null)
				Object.DestroyImmediate(s_EditorToolbar);

			s_EditorToolbar = ScriptableObject.CreateInstance<pb_EditorToolbar>();
			s_EditorToolbar.hideFlags = HideFlags.HideAndDontSave;
			s_EditorToolbar.InitWindowProperties(this);

			VertexTranslationInfoStyle = new GUIStyle();
			VertexTranslationInfoStyle.normal.background = EditorGUIUtility.whiteTexture;
			VertexTranslationInfoStyle.normal.textColor = new Color(1f, 1f, 1f, .6f);
			VertexTranslationInfoStyle.padding = new RectOffset(3, 3, 3, 0);

			var object_Graphic_off = pb_IconUtility.GetIcon("Modes/Mode_Object");
			var face_Graphic_off = pb_IconUtility.GetIcon("Modes/Mode_Face");
			var vertex_Graphic_off = pb_IconUtility.GetIcon("Modes/Mode_Vertex");
			var edge_Graphic_off = pb_IconUtility.GetIcon("Modes/Mode_Edge");

			m_EditModeIcons = new GUIContent[]
			{
				object_Graphic_off != null
					? new GUIContent(object_Graphic_off, "Object Selection")
					: new GUIContent("OBJ", "Object Selection"),
				vertex_Graphic_off != null
					? new GUIContent(vertex_Graphic_off, "Vertex Selection")
					: new GUIContent("VRT", "Vertex Selection"),
				edge_Graphic_off != null
					? new GUIContent(edge_Graphic_off, "Edge Selection")
					: new GUIContent("EDG", "Edge Selection"),
				face_Graphic_off != null
					? new GUIContent(face_Graphic_off, "Face Selection")
					: new GUIContent("FCE", "Face Selection"),
			};
		}

		public static void Refresh(bool force = true)
		{
			if (instance != null)
				instance.UpdateSelection(force);
		}

		void OnGUI()
		{
			if (commandStyle == null)
				commandStyle = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("Command");

			Event e = Event.current;

			switch (e.type)
			{
				case EventType.ContextClick:
					OpenContextMenu();
					break;

				case EventType.KeyDown:
					if (m_Shortcuts.Any(x => x.Matches(e.keyCode, e.modifiers)))
						e.Use();
					break;

				case EventType.KeyUp:
					ShortcutCheck(e);
					break;
			}

			if (s_EditorToolbar != null)
			{
				s_EditorToolbar.OnGUI();
			}
			else
			{
				try
				{
					InitGUI();
				}
				catch (System.Exception exception)
				{
					Debug.LogWarning(string.Format("Failed initializing ProBuilder Toolbar:\n{0}", exception.ToString()));
				}
			}
		}

		void OpenContextMenu()
		{
			GenericMenu menu = new GenericMenu();

			menu.AddItem(new GUIContent("Open As Floating Window", ""),
				!pb_PreferencesInternal.GetBool(pb_Constant.pbDefaultOpenInDockableWindow, true), Menu_OpenAsFloatingWindow);
			menu.AddItem(new GUIContent("Open As Dockable Window", ""),
				pb_PreferencesInternal.GetBool(pb_Constant.pbDefaultOpenInDockableWindow, true), Menu_OpenAsDockableWindow);

			menu.AddSeparator("");

			menu.AddItem(new GUIContent("Use Icon Mode", ""), pb_PreferencesInternal.GetBool(pb_Constant.pbIconGUI),
				Menu_ToggleIconMode);
			menu.AddItem(new GUIContent("Use Text Mode", ""), !pb_PreferencesInternal.GetBool(pb_Constant.pbIconGUI),
				Menu_ToggleIconMode);

			menu.ShowAsContext();
		}

		void Menu_ToggleIconMode()
		{
			m_IsIconGui = !pb_PreferencesInternal.GetBool(pb_Constant.pbIconGUI);
			pb_PreferencesInternal.SetBool(pb_Constant.pbIconGUI, m_IsIconGui);
			if (s_EditorToolbar != null)
				Object.DestroyImmediate(s_EditorToolbar);
			s_EditorToolbar = ScriptableObject.CreateInstance<pb_EditorToolbar>();
			s_EditorToolbar.hideFlags = HideFlags.HideAndDontSave;
			s_EditorToolbar.InitWindowProperties(this);
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

		void OnSceneGUI(SceneView sceneView)
		{
#if !UNITY_2018_2_OR_NEWER
			if(s_ResetOnSceneGUIState != null)
				s_ResetOnSceneGUIState.Invoke(sceneView, null);
#endif

			SceneStyles.Init();

			m_CurrentEvent = Event.current;

			if (editLevel == EditLevel.Geometry)
			{
				if (m_CurrentEvent.Equals(Event.KeyboardEvent("v")))
					snapToVertex = true;
				else if (m_CurrentEvent.Equals(Event.KeyboardEvent("c")))
					snapToFace = true;
			}

			// Snap stuff
			if (m_CurrentEvent.type == EventType.KeyUp)
			{
				snapToFace = false;
				snapToVertex = false;
			}

			if (m_CurrentEvent.type == EventType.MouseDown && m_CurrentEvent.button == 1)
				rightMouseDown = true;

			if (m_CurrentEvent.type == EventType.MouseUp && m_CurrentEvent.button == 1 || m_CurrentEvent.type == EventType.Ignore)
				rightMouseDown = false;

			pb_MeshHandles.DoGUI(editLevel, selectionMode);

			DrawHandleGUI(sceneView);

			if (!rightMouseDown && getKeyUp != KeyCode.None)
			{
				if (ShortcutCheck(m_CurrentEvent))
				{
					m_CurrentEvent.Use();
					return;
				}
			}

			if (m_CurrentEvent.type == EventType.KeyDown)
			{
				if (m_Shortcuts.Any(x => x.Matches(m_CurrentEvent.keyCode, m_CurrentEvent.modifiers)))
					m_CurrentEvent.Use();
			}

			// Finished moving vertices, scaling, or adjusting uvs
			if ((movingVertices || movingPictures || scaling) && GUIUtility.hotControl < 1)
			{
				OnFinishVertexModification();
				UpdateHandleRotation();
				UpdateTextureHandles();
			}

			// Check mouse position in scene and determine if we should highlight something
			if (m_CurrentEvent.type == EventType.MouseMove && editLevel == EditLevel.Geometry)
				UpdateMouse(m_CurrentEvent.mousePosition);

			if (Tools.current != Tool.None && Tools.current != currentHandle)
				SetTool_Internal(Tools.current);

			if ((editLevel == EditLevel.Geometry || editLevel == EditLevel.Texture) && Tools.current != Tool.View)
			{
				if (m_SelectedVertexCount > 0)
				{
					if (editLevel == EditLevel.Geometry)
					{
						switch (currentHandle)
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
					else if (editLevel == EditLevel.Texture && m_SelectedVertexCount > 0)
					{
						switch (currentHandle)
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
				}
			}
			else
			{
				return;
			}

			// altClick || Tools.current == Tool.View || GUIUtility.hotControl > 0 || middleClick
			// Tools.viewTool == ViewTool.FPS || Tools.viewTool == ViewTool.Orbit
			if (pb_EditorHandleUtility.SceneViewInUse(m_CurrentEvent) || m_CurrentEvent.isKey || selection == null ||
			    selection.Length < 1)
			{
				m_IsDragging = false;
				return;
			}

			// This prevents us from selecting other objects in the scene,
			// and allows for the selection of faces / vertices.
			int controlID = GUIUtility.GetControlID(FocusType.Passive);
			HandleUtility.AddDefaultControl(controlID);

			// If selection is made, don't use default handle -- set it to Tools.None
			if (m_SelectedVertexCount > 0)
				Tools.current = Tool.None;

			if (leftClick)
			{
				// double clicking object
				if (m_CurrentEvent.clickCount > 1)
				{
					DoubleClick(m_CurrentEvent);
				}

				mousePosition_initial = m_CurrentEvent.mousePosition;
				// readyForMouseDrag prevents a bug wherein after ending a drag an errant
				// MouseDrag event is sent with no corresponding MouseDown/MouseUp event.
				readyForMouseDrag = true;
			}

			if (mouseDrag && readyForMouseDrag)
			{
				if(!m_IsDragging)
					sceneView.Repaint();

				m_IsDragging = true;
			}

			if (ignore)
			{
				if (m_IsDragging)
				{
					readyForMouseDrag = false;
					m_IsDragging = false;
					DragCheck();
				}

				if (doubleClicked)
					doubleClicked = false;
			}

			if (leftClickUp)
			{
				if (doubleClicked)
				{
					doubleClicked = false;
				}
				else
				{
					if (!m_IsDragging)
					{
						if (pb_UVEditor.instance)
							pb_UVEditor.instance.ResetUserPivot();

						RaycastCheck(m_CurrentEvent.mousePosition);
					}
					else
					{
						m_IsDragging = false;
						readyForMouseDrag = false;

						if (pb_UVEditor.instance)
							pb_UVEditor.instance.ResetUserPivot();

						DragCheck();
					}
				}
			}
		}

		void DoubleClick(Event e)
		{
			pb_Object pb = RaycastCheck(e.mousePosition, -1);

			if (pb != null)
			{
				if (selectionMode == SelectMode.Edge)
				{
					if (e.shift)
						pb_MenuCommands.MenuRingSelection(selection);
					else
						pb_MenuCommands.MenuLoopSelection(selection);
				}
				else if (selectionMode == SelectMode.Face)
				{
					if ((e.modifiers & (EventModifiers.Control | EventModifiers.Shift)) ==
					    (EventModifiers.Control | EventModifiers.Shift))
						pb_MenuCommands.MenuRingAndLoopFaces(selection);
					else if (e.control)
						pb_MenuCommands.MenuRingFaces(selection);
					else if (e.shift)
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

		/// <summary>
		/// If in Edge mode, finds the nearest Edge to the mouse
		/// </summary>
		/// <param name="mousePosition"></param>
		void UpdateMouse(Vector3 mousePosition)
		{
			if (selection.Length < 1 || selectionMode != SelectMode.Edge)
				return;

			GameObject go = HandleUtility.PickGameObject(mousePosition, false);

			pb_Edge bestEdge = pb_Edge.Empty;
			pb_Object bestObj = go == null ? null : go.GetComponent<pb_Object>();

			if (bestObj != null && !selection.Contains(bestObj))
				bestObj = null;

			// If mouse isn't over a pb object, it still may be near enough to an edge.
			if (bestObj == null)
			{
				float bestDistance = m_HamSelection ? k_MaxEdgeSelectDistanceHam : k_MaxEdgeSelectDistanceCtx;

				for (int i = 0; i < m_UniversalEdges.Length; i++)
				{
					var pb = selection[i];
					var edges = m_UniversalEdges[i];

					for (int j = 0; j < edges.Length; j++)
					{
						int x = selection[i].sharedIndices[edges[j].x][0];
						int y = selection[i].sharedIndices[edges[j].y][0];

						float d = HandleUtility.DistanceToLine(
							pb.transform.TransformPoint(pb.vertices[x]),
							pb.transform.TransformPoint(pb.vertices[y]));

						if (d < bestDistance)
						{
							bestObj = selection[i];
							bestEdge = new pb_Edge(x, y);
							bestDistance = d;
						}
					}
				}
			}
			else
			{
				// Test culling
				List<pb_RaycastHit> hits;
				Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

				if (pb_HandleUtility.FaceRaycast(ray, bestObj, out hits, Mathf.Infinity, pb_Culling.FrontBack))
				{
					Camera cam = SceneView.lastActiveSceneView.camera;

					// Sort from nearest hit to farthest
					hits.Sort((x, y) => x.distance.CompareTo(y.distance));

					// Find the nearest edge in the hit faces

					float bestDistance = Mathf.Infinity;
					Vector3[] v = bestObj.vertices;

					for (int i = 0; i < hits.Count; i++)
					{
						if (pb_HandleUtility.PointIsOccluded(cam, bestObj, bestObj.transform.TransformPoint(hits[i].point)))
							continue;

						foreach (pb_Edge edge in bestObj.faces[hits[i].face].edges)
						{
							float d = HandleUtility.DistancePointLine(hits[i].point, v[edge.x], v[edge.y]);

							if (d < bestDistance)
							{
								bestDistance = d;
								bestEdge = edge;
							}
						}

						if (Vector3.Dot(ray.direction, bestObj.transform.TransformDirection(hits[i].normal)) < 0f)
							break;
					}

					if (bestEdge.IsValid() &&
					    HandleUtility.DistanceToLine(bestObj.transform.TransformPoint(v[bestEdge.x]),
						    bestObj.transform.TransformPoint(v[bestEdge.y])) >
					    (m_HamSelection ? k_MaxEdgeSelectDistanceHam : k_MaxEdgeSelectDistanceCtx))
						bestEdge = pb_Edge.Empty;
				}
			}

			if (bestEdge != nearestEdge || bestObj != nearestEdgeObject)
			{
				nearestEdge = bestEdge;
				nearestEdgeObject = bestObj;

				SceneView.RepaintAll();
			}
		}

		// Returns the pb_Object modified by this action.  If no action taken, or action is eaten by texture window, return null.
		// A pb_Object is returned because double click actions need to know what the last selected pb_Object was.
		// If deepClickOffset is specified, the object + deepClickOffset in the deep select stack will be returned (instead of next).
		pb_Object RaycastCheck(Vector3 mousePosition, int deepClickOffset = 0)
		{
			pb_Object pb = null;

			// Since Edge or Vertex selection may be valid even if clicking off a gameObject, check them
			// first. If no hits, move on to face selection or object change.
			if ((selectionMode == SelectMode.Edge && EdgeClickCheck(out pb)) ||
			    (selectionMode == SelectMode.Vertex && VertexClickCheck(out pb)))
			{
				UpdateSelection(false);
				SceneView.RepaintAll();
				return pb;
			}

			if (!shiftKey && !ctrlKey)
				pb_Selection.SetSelection((GameObject) null);

			GameObject pickedGo = null;
			pb_Object pickedPb = null;
			pb_Face pickedFace = null;
			int newHash = 0;

			List<GameObject> picked = pb_EditorHandleUtility.GetAllOverlapping(mousePosition);

			EventModifiers em = Event.current.modifiers;

			// If any event modifiers are engaged don't cycle the deep click
			int pickedCount = em != EventModifiers.None ? System.Math.Min(1, picked.Count) : picked.Count;

			for (int i = 0, next = 0; i < pickedCount; i++)
			{
				GameObject go = picked[i];
				pb = go.GetComponent<pb_Object>();
				pb_Face face = null;

				if (pb != null)
				{
					Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
					pb_RaycastHit hit;

					if (pb_HandleUtility.FaceRaycast(ray,
						pb,
						out hit,
						Mathf.Infinity,
						selectHiddenEnabled ? pb_Culling.FrontBack : pb_Culling.Front))
					{
						face = pb.faces[hit.face];
					}
				}

				// pb_Face doesn't define GetHashCode, meaning it falls to object.GetHashCode (reference comparison)
				int hash = face == null ? go.GetHashCode() : face.GetHashCode();

				if (s_DeepSelectionPrevious == hash)
					next = (i + (1 + deepClickOffset)) % pickedCount;

				if (next == i)
				{
					pickedGo = go;
					pickedPb = pb;
					pickedFace = face;

					newHash = hash;

					// a prior hash was matched, this is the next. if
					// it's just the first iteration don't break (but do
					// set the default).
					if (next != 0)
						break;
				}
			}

			s_DeepSelectionPrevious = newHash;

			if (pickedGo != null)
			{
				Event.current.Use();

				if (pickedPb != null)
				{
					if (pickedPb.isSelectable)
					{
						pb_Selection.AddToSelection(pickedGo);

#if !PROTOTYPE
						// Check for other editor mouse shortcuts first
						pb_MaterialEditor matEditor = pb_MaterialEditor.instance;
						if (matEditor != null && matEditor.ClickShortcutCheck(Event.current.modifiers, pickedPb, pickedFace))
							return pickedPb;

						pb_UVEditor uvEditor = pb_UVEditor.instance;
						if (uvEditor != null && uvEditor.ClickShortcutCheck(pickedPb, pickedFace))
							return pickedPb;
#endif

						// Check to see if we've already selected this quad.  If so, remove it from selection cache.
						pb_Undo.RecordSelection(pickedPb, "Change Face Selection");

						int indx = System.Array.IndexOf(pickedPb.SelectedFaces, pickedFace);

						if (indx > -1)
						{
							pickedPb.RemoveFromFaceSelectionAtIndex(indx);
						}
						else
						{
							pickedPb.AddToFaceSelection(pickedFace);
						}
					}
					else
					{
						return null;
					}
				}
				else if (!pb_PreferencesInternal.GetBool(pb_Constant.pbPBOSelectionOnly))
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

		bool VertexClickCheck(out pb_Object vpb)
		{
			if (!shiftKey && !ctrlKey)
				ClearElementSelection();

			Camera cam = SceneView.lastActiveSceneView.camera;
			List<pb_Tuple<float, Vector3, int, int>> nearest = new List<pb_Tuple<float, Vector3, int, int>>();

			// this could be much faster by raycasting against the mesh and doing a 3d space
			// distance check first

			if (m_HamSelection)
			{
				const float minAllowableDistance = k_MaxEdgeSelectDistanceHam * k_MaxEdgeSelectDistanceHam;
				int obj = -1, tri = -1;
				Vector2 mousePosition = m_CurrentEvent.mousePosition;

				for (int i = 0; i < selection.Length; i++)
				{
					pb_Object pb = selection[i];

					if (!pb.isSelectable)
						continue;

					for (int n = 0, c = pb.sharedIndices.Length; n < c; n++)
					{
						int index = pb.sharedIndices[n][0];
						Vector3 v = pb.transform.TransformPoint(pb.vertices[index]);
						Vector2 p = HandleUtility.WorldToGUIPoint(v);

						float dist = (p - mousePosition).sqrMagnitude;

						if (dist < minAllowableDistance)
							nearest.Add(new pb_Tuple<float, Vector3, int, int>(dist, v, i, index));
					}
				}

				nearest.Sort((x, y) => x.Item1.CompareTo(y.Item1));

				for (int i = 0; i < nearest.Count; i++)
				{
					obj = nearest[i].Item3;

					if (!pb_HandleUtility.PointIsOccluded(cam, selection[obj], nearest[i].Item2))
					{
						tri = nearest[i].Item4;
						break;
					}
				}

				if (obj > -1 && tri > -1)
				{
					pb_Object pb = selection[obj];

					int indx = System.Array.IndexOf(pb.SelectedTriangles, tri);

					pb_Undo.RecordSelection(pb, "Change Vertex Selection");

					// If we get a match, check to see if it exists in our selection array already, then add / remove
					if (indx > -1)
						pb.SetSelectedTriangles(pb.SelectedTriangles.RemoveAt(indx));
					else
						pb.SetSelectedTriangles(pb.SelectedTriangles.Add(tri));

					vpb = pb;
					return true;
				}
			}
			else
			{
				for (int i = 0; i < selection.Length; i++)
				{
					pb_Object pb = selection[i];

					if (!pb.isSelectable)
						continue;

					for (int n = 0, c = pb.sharedIndices.Length; n < c; n++)
					{
						int index = pb.sharedIndices[n][0];
						Vector3 v = pb.transform.TransformPoint(pb.vertices[index]);

						if (m_MouseClickRect.Contains(HandleUtility.WorldToGUIPoint(v)))
						{
							if (pb_HandleUtility.PointIsOccluded(cam, pb, v))
								continue;

							// Check if index is already selected, and if not add it to the pot
							int indx = System.Array.IndexOf(pb.SelectedTriangles, index);

							pb_Undo.RecordObject(pb, "Change Vertex Selection");

							// If we get a match, check to see if it exists in our selection array already, then add / remove
							if (indx > -1)
								pb.SetSelectedTriangles(pb.SelectedTriangles.RemoveAt(indx));
							else
								pb.SetSelectedTriangles(pb.SelectedTriangles.Add(index));

							vpb = pb;
							return true;
						}
					}
				}
			}

			vpb = null;
			return false;
		}

		bool EdgeClickCheck(out pb_Object pb)
		{
			if (!shiftKey && !ctrlKey)
			{
				// don't call ClearElementSelection b/c that also removes
				// nearestEdge info
				foreach (pb_Object p in selection)
					p.ClearSelection();
			}

			if (nearestEdgeObject != null)
			{
				pb = nearestEdgeObject;

				if (nearestEdge.IsValid())
				{
					pb_Tuple<pb_Face, pb_Edge> edge;

					if (pb_EdgeExtension.ValidateEdge(pb, nearestEdge, out edge))
						nearestEdge = edge.Item2;

					int ind = pb.SelectedEdges.IndexOf(nearestEdge, pb.sharedIndices.ToDictionary());

					pb_Undo.RecordSelection(pb, "Change Edge Selection");

					if (ind > -1)
						pb.SetSelectedEdges(pb.SelectedEdges.RemoveAt(ind));
					else
						pb.SetSelectedEdges(pb.SelectedEdges.Add(nearestEdge));

					return true;
				}

				return false;
			}
			else
			{
				if (!shiftKey && !ctrlKey)
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
			bool selectHidden = selectHiddenEnabled;

			var pickingOptions = new pb_PickerOptions()
			{
				depthTest = !selectHidden,
				rectSelectMode = pb_PreferencesInternal.GetEnum<pb_RectSelectMode>(pb_Constant.pbRectSelectMode)
			};

			switch (selectionMode)
			{
				case SelectMode.Vertex:
				{
					if (!shiftKey && !ctrlKey)
						ClearElementSelection();

					Dictionary<pb_Object, HashSet<int>> selected = pb_Picking.PickVerticesInRect(
						SceneView.lastActiveSceneView.camera,
						m_MouseDragRect,
						selection,
						pickingOptions,
						EditorGUIUtility.pixelsPerPoint);

					foreach (var kvp in selected)
					{
						pb_IntArray[] sharedIndices = kvp.Key.sharedIndices;
						HashSet<int> common;

						if (shiftKey || ctrlKey)
						{
							common = sharedIndices.GetCommonIndices(kvp.Key.SelectedTriangles);

							if (dragSelectMode == DragSelectMode.Add)
								common.UnionWith(kvp.Value);
							else if (dragSelectMode == DragSelectMode.Subtract)
								common.RemoveWhere(x => kvp.Value.Contains(x));
							else if (dragSelectMode == DragSelectMode.Difference)
								common.SymmetricExceptWith(kvp.Value);
						}
						else
						{
							common = kvp.Value;
						}

						kvp.Key.SetSelectedTriangles(common.SelectMany(x => sharedIndices[x].array).ToArray());
					}

					UpdateSelection(false);
				}
					break;

				case SelectMode.Face:
				{
					if (!shiftKey && !ctrlKey)
						ClearElementSelection();

					Dictionary<pb_Object, HashSet<pb_Face>> selected = pb_Picking.PickFacesInRect(
						SceneView.lastActiveSceneView.camera,
						m_MouseDragRect,
						selection,
						pickingOptions,
						EditorGUIUtility.pixelsPerPoint);

					foreach (var kvp in selected)
					{
						HashSet<pb_Face> current;

						if (shiftKey || ctrlKey)
						{
							current = new HashSet<pb_Face>(kvp.Key.SelectedFaces);

							if (dragSelectMode == DragSelectMode.Add)
								current.UnionWith(kvp.Value);
							else if (dragSelectMode == DragSelectMode.Subtract)
								current.RemoveWhere(x => kvp.Value.Contains(x));
							else if (dragSelectMode == DragSelectMode.Difference)
								current.SymmetricExceptWith(kvp.Value);
						}
						else
						{
							current = kvp.Value;
						}

						kvp.Key.SetSelectedFaces(current);
					}

					UpdateSelection(false);
				}
					break;

				case SelectMode.Edge:
				{
					if (!shiftKey && !ctrlKey)
						ClearElementSelection();

					var selected = pb_Picking.PickEdgesInRect(
						SceneView.lastActiveSceneView.camera,
						m_MouseDragRect,
						selection,
						pickingOptions,
						EditorGUIUtility.pixelsPerPoint);

					foreach (var kvp in selected)
					{
						pb_Object pb = kvp.Key;
						Dictionary<int, int> commonIndices = pb.sharedIndices.ToDictionary();
						HashSet<pb_EdgeLookup> selectedEdges = pb_EdgeLookup.GetEdgeLookupHashSet(kvp.Value, commonIndices);

						HashSet<pb_EdgeLookup> current;

						if (shiftKey || ctrlKey)
						{
							current = pb_EdgeLookup.GetEdgeLookupHashSet(pb.SelectedEdges, commonIndices);

							if (dragSelectMode == DragSelectMode.Add)
								current.UnionWith(selectedEdges);
							else if (dragSelectMode == DragSelectMode.Subtract)
								current.RemoveWhere(x => selectedEdges.Contains(x));
							else if (dragSelectMode == DragSelectMode.Difference)
								current.SymmetricExceptWith(selectedEdges);
						}
						else
						{
							current = selectedEdges;
						}

						pb.SetSelectedEdges(current.Select(x => x.local));
					}

					UpdateSelection(false);
				}
					break;

				default:
					DragObjectCheck();
					break;
			}

			SceneView.RepaintAll();
		}

		// Emulates the usual Unity drag to select objects functionality
		void DragObjectCheck()
		{
			// if we're in vertex selection mode, only add to selection if shift key is held,
			// and don't clear the selection if shift isn't held.
			// if not, behave regularly (clear selection if shift isn't held)
			if (editLevel == EditLevel.Geometry && selectionMode == SelectMode.Vertex)
			{
				if (!shiftKey && m_SelectedVertexCount > 0) return;
			}
			else
			{
				if (!shiftKey) pb_Selection.ClearElementAndObjectSelection();
			}

			// scan for new selected objects
			// if mode based, don't allow selection of non-probuilder objects
			foreach (pb_Object g in HandleUtility.PickRectObjects(m_MouseDragRect).GetComponents<pb_Object>())
				if (!Selection.Contains(g.gameObject))
					pb_Selection.AddToSelection(g.gameObject);
		}

		void VertexMoveTool()
		{
			newPosition = m_HandlePivotWorld;
			cachedPosition = newPosition;

			newPosition = Handles.PositionHandle(newPosition, handleRotation);

			if (altClick)
				return;

			bool previouslyMoving = movingVertices;

			if (newPosition != cachedPosition)
			{
				// profiler.BeginSample("VertexMoveTool()");
				Vector3 diff = newPosition - cachedPosition;

				Vector3 mask = diff.ToMask(pb_Math.HANDLE_EPSILON);

				if (snapToVertex)
				{
					Vector3 v;

					if (FindNearestVertex(m_CurrentEvent.mousePosition, out v))
						diff = Vector3.Scale(v - cachedPosition, mask);
				}
				else if (snapToFace)
				{
					pb_Object obj = null;
					pb_RaycastHit hit;
					Dictionary<pb_Object, HashSet<pb_Face>> ignore = new Dictionary<pb_Object, HashSet<pb_Face>>();
					foreach (pb_Object pb in selection)
						ignore.Add(pb, new HashSet<pb_Face>(pb.SelectedFaces));

					if (pb_EditorHandleUtility.FaceRaycast(m_CurrentEvent.mousePosition, out obj, out hit, ignore))
					{
						if (mask.IntSum() == 1)
						{
							Ray r = new Ray(cachedPosition, -mask);
							Plane plane = new Plane(obj.transform.TransformDirection(hit.normal).normalized,
								obj.transform.TransformPoint(hit.point));

							float forward, backward;
							plane.Raycast(r, out forward);
							plane.Raycast(r, out backward);
							float planeHit = Mathf.Abs(forward) < Mathf.Abs(backward) ? forward : backward;
							r.direction = -r.direction;
							plane.Raycast(r, out forward);
							plane.Raycast(r, out backward);
							float rev = Mathf.Abs(forward) < Mathf.Abs(backward) ? forward : backward;
							if (Mathf.Abs(rev) > Mathf.Abs(planeHit))
								planeHit = rev;

							if (Mathf.Abs(planeHit) > Mathf.Epsilon)
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

				if (previouslyMoving == false)
				{
					translateOrigin = cachedPosition;
					rotateOrigin = currentHandleRotation.eulerAngles;
					scaleOrigin = currentHandleScale;

					OnBeginVertexMovement();

					if (Event.current.modifiers == EventModifiers.Shift)
						ShiftExtrude();

					pb_ProGridsInterface.OnHandleMove(mask);
				}

				for (int i = 0; i < selection.Length; i++)
				{
					selection[i].TranslateVertices_World(selection[i].SelectedTriangles, diff, m_SnapEnabled ? m_SnapValue : 0f,
						m_SnapAxisConstraint, m_SharedIndicesDictionary[i]);
					selection[i].RefreshUV(SelectedFacesInEditZone[selection[i]]);
					selection[i].Refresh(RefreshMask.Normals);
					selection[i].msh.RecalculateBounds();
				}

				Internal_UpdateSelectionFast();

				// profiler.EndSample();
			}
		}

		void VertexScaleTool()
		{
			newPosition = m_HandlePivotWorld;

			previousHandleScale = currentHandleScale;

			currentHandleScale = Handles.ScaleHandle(currentHandleScale, newPosition, handleRotation,
				HandleUtility.GetHandleSize(newPosition));

			if (altClick) return;

			bool previouslyMoving = movingVertices;

			if (previousHandleScale != currentHandleScale)
			{
				movingVertices = true;
				if (previouslyMoving == false)
				{
					translateOrigin = cachedPosition;
					rotateOrigin = currentHandleRotation.eulerAngles;
					scaleOrigin = currentHandleScale;

					OnBeginVertexMovement();

					if (Event.current.modifiers == EventModifiers.Shift)
						ShiftExtrude();

					// cache vertex positions for scaling later
					vertexOrigins = new Vector3[selection.Length][];
					vertexOffset = new Vector3[selection.Length];

					for (int i = 0; i < selection.Length; i++)
					{
						vertexOrigins[i] = selection[i].vertices.ValuesWithIndices(selection[i].SelectedTriangles);
						vertexOffset[i] = pb_Math.Average(vertexOrigins[i]);
					}
				}

				Vector3 ver; // resulting vertex from modification
				Vector3 over; // vertex point to modify. different for world, local, and plane

				bool gotoWorld = Selection.transforms.Length > 1 && handleAlignment == HandleAlignment.Plane;
				bool gotoLocal = m_SelectedFaceCount < 1;

				// if(pref_snapEnabled)
				// 	pbUndo.RecordSelection(selection as Object[], "Move Vertices");

				for (int i = 0; i < selection.Length; i++)
				{
					// get the plane rotation in local space
					Vector3 nrm = pb_Math.Normal(vertexOrigins[i]);
					Quaternion localRot = Quaternion.LookRotation(nrm == Vector3.zero ? Vector3.forward : nrm, Vector3.up);

					Vector3[] v = selection[i].vertices;
					pb_IntArray[] sharedIndices = selection[i].sharedIndices;

					for (int n = 0; n < selection[i].SelectedTriangles.Length; n++)
					{
						switch (handleAlignment)
						{
							case HandleAlignment.Plane:
							{
								if (gotoWorld)
									goto case HandleAlignment.World;

								if (gotoLocal)
									goto case HandleAlignment.Local;

								// move center of vertices to 0,0,0 and set rotation as close to identity as possible
								over = Quaternion.Inverse(localRot) * (vertexOrigins[i][n] - vertexOffset[i]);

								// apply scale
								ver = Vector3.Scale(over, currentHandleScale);

								// re-apply original rotation
								if (vertexOrigins[i].Length > 2)
									ver = localRot * ver;

								// re-apply world position offset
								ver += vertexOffset[i];

								int[] array = sharedIndices[m_SharedIndicesDictionary[i][selection[i].SelectedTriangles[n]]].array;

								for (int t = 0; t < array.Length; t++)
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

								int[] array = sharedIndices[m_SharedIndicesDictionary[i][selection[i].SelectedTriangles[n]]].array;

								for (int t = 0; t < array.Length; t++)
									v[array[t]] = ver;

								break;
							}
						}
					}

					selection[i].SetVertices(v);
					selection[i].msh.vertices = v;
					selection[i].RefreshUV(SelectedFacesInEditZone[selection[i]]);
					selection[i].Refresh(RefreshMask.Normals);
					selection[i].msh.RecalculateBounds();
				}

				Internal_UpdateSelectionFast();
			}
		}

		void VertexRotateTool()
		{
			if (!movingVertices)
				newPosition = m_HandlePivotWorld;

			previousHandleRotation = currentHandleRotation;

			if (altClick)
				Handles.RotationHandle(currentHandleRotation, newPosition);
			else
				currentHandleRotation = Handles.RotationHandle(currentHandleRotation, newPosition);

			if (currentHandleRotation != previousHandleRotation)
			{
				// profiler.BeginSample("Rotate");
				if (!movingVertices)
				{
					movingVertices = true;

					translateOrigin = cachedPosition;
					rotateOrigin = currentHandleRotation.eulerAngles;
					scaleOrigin = currentHandleScale;

					m_HandleRotation = previousHandleRotation;
					m_InverseRotation = Quaternion.Inverse(previousHandleRotation);

					OnBeginVertexMovement();

					if (Event.current.modifiers == EventModifiers.Shift)
						ShiftExtrude();

					// cache vertex positions for modifying later
					vertexOrigins = new Vector3[selection.Length][];
					vertexOffset = new Vector3[selection.Length];

					for (int i = 0; i < selection.Length; i++)
					{
						Vector3[] vertices = selection[i].vertices;
						int[] triangles = selection[i].SelectedTriangles;
						vertexOrigins[i] = new Vector3[triangles.Length];

						for (int nn = 0; nn < triangles.Length; nn++)
							vertexOrigins[i][nn] = selection[i].transform.TransformPoint(vertices[triangles[nn]]);

						if (handleAlignment == HandleAlignment.World)
							vertexOffset[i] = newPosition;
						else
							vertexOffset[i] = pb_Math.BoundsCenter(vertexOrigins[i]);
					}
				}

				// profiler.BeginSample("Calc Matrix");
				Quaternion transformedRotation = m_InverseRotation * currentHandleRotation;

				// profiler.BeginSample("matrix mult");
				Vector3 ver; // resulting vertex from modification
				for (int i = 0; i < selection.Length; i++)
				{
					Vector3[] v = selection[i].vertices;
					pb_IntArray[] sharedIndices = selection[i].sharedIndices;

					Quaternion lr = m_HandleRotation; // selection[0].transform.localRotation;
					Quaternion ilr = m_InverseRotation; // Quaternion.Inverse(lr);

					for (int n = 0; n < selection[i].SelectedTriangles.Length; n++)
					{
						// move vertex to relative origin from center of selection
						ver = ilr * (vertexOrigins[i][n] - vertexOffset[i]);

						// rotate
						ver = transformedRotation * ver;

						// move vertex back to locally offset position
						ver = (lr * ver) + vertexOffset[i];

						int[] array = sharedIndices[m_SharedIndicesDictionary[i][selection[i].SelectedTriangles[n]]].array;

						for (int t = 0; t < array.Length; t++)
							v[array[t]] = selection[i].transform.InverseTransformPoint(ver);
					}

					selection[i].SetVertices(v);
					selection[i].msh.vertices = v;
					selection[i].RefreshUV(SelectedFacesInEditZone[selection[i]]);
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

		/// <summary>
		/// Extrude the current selection with no translation.
		/// </summary>
		void ShiftExtrude()
		{
			int ef = 0;
			foreach (pb_Object pb in selection)
			{
				// @todo - If caching normals, remove this 'ToMesh' and move
				Undo.RegisterCompleteObjectUndo(selection, "Extrude Vertices");

				switch (selectionMode)
				{
					case SelectMode.Edge:
						if (pb.SelectedFaceCount > 0)
							goto default;

						pb_Edge[] newEdges;
						bool success = pb.Extrude(pb.SelectedEdges,
							0.0001f,
							pb_PreferencesInternal.GetBool(pb_Constant.pbExtrudeAsGroup),
							pb_PreferencesInternal.GetBool(pb_Constant.pbManifoldEdgeExtrusion),
							out newEdges);

						if (success)
						{
							ef += newEdges.Length;
							pb.SetSelectedEdges(newEdges);
						}
						break;

					default:
						int len = pb.SelectedFaces.Length;

						if (len > 0)
						{
							pb.Extrude(pb.SelectedFaces, pb_PreferencesInternal.GetEnum<ExtrudeMethod>(pb_Constant.pbExtrudeMethod),
								0.0001f);
							pb.SetSelectedFaces(pb.SelectedFaces);
							ef += len;
						}

						break;
				}

				pb.ToMesh();
				pb.Refresh();
			}

			if (ef > 0)
			{
				pb_EditorUtility.ShowNotification("Extrude");
				UpdateSelection(true);
			}
		}

		void TextureMoveTool()
		{
			pb_UVEditor uvEditor = pb_UVEditor.instance;
			if (!uvEditor) return;

			Vector3 cached = textureHandle;

			textureHandle = Handles.PositionHandle(textureHandle, handleRotation);

			if (altClick) return;

			if (textureHandle != cached)
			{
				cached = Quaternion.Inverse(handleRotation) * textureHandle;
				cached.y = -cached.y;

				Vector3 lossyScale = selection[0].transform.lossyScale;
				Vector3 position = cached.DivideBy(lossyScale);

				if (!movingPictures)
				{
					previousTextureHandle = position;
					movingPictures = true;
				}

				uvEditor.SceneMoveTool(position - previousTextureHandle);
				previousTextureHandle = position;

				uvEditor.Repaint();
			}
		}

		void TextureRotateTool()
		{
			pb_UVEditor uvEditor = pb_UVEditor.instance;
			if (!uvEditor) return;

			float size = HandleUtility.GetHandleSize(m_HandlePivotWorld);

			if (altClick) return;

			Matrix4x4 prev = Handles.matrix;
			Handles.matrix = handleMatrix;

			Quaternion cached = textureRotation;

			textureRotation = Handles.Disc(textureRotation, Vector3.zero, Vector3.forward, size, false, 0f);

			if (textureRotation != cached)
			{
				if (!movingPictures)
					movingPictures = true;

				uvEditor.SceneRotateTool(-textureRotation.eulerAngles.z);
			}

			Handles.matrix = prev;
		}

		void TextureScaleTool()
		{
			pb_UVEditor uvEditor = pb_UVEditor.instance;
			if (!uvEditor) return;

			float size = HandleUtility.GetHandleSize(m_HandlePivotWorld);

			Matrix4x4 prev = Handles.matrix;
			Handles.matrix = handleMatrix;

			Vector3 cached = textureScale;
			textureScale = Handles.ScaleHandle(textureScale, Vector3.zero, Quaternion.identity, size);

			if (altClick) return;

			if (cached != textureScale)
			{
				if (!movingPictures)
					movingPictures = true;

				uvEditor.SceneScaleTool(textureScale, cached);
			}

			Handles.matrix = prev;
		}

		void DrawHandleGUI(SceneView sceneView)
		{
			if (sceneView != SceneView.lastActiveSceneView)
				return;

			// Draw nearest edge
			if (m_CurrentEvent.type == EventType.Repaint &&
			    editLevel != EditLevel.Top &&
			    editLevel != EditLevel.Plugin)
			{
				if (nearestEdgeObject != null && nearestEdge.IsValid())
				{
					if (pb_EditorHandleUtility.BeginDrawingLines(Handles.zTest))
					{
						pb_MeshHandles.lineMaterial.SetColor("_Color", Color.white);
						GL.Color(pb_MeshHandles.preselectionColor);

						GL.MultMatrix(nearestEdgeObject.transform.localToWorldMatrix);

						GL.Vertex(nearestEdgeObject.vertices[nearestEdge.x]);
						GL.Vertex(nearestEdgeObject.vertices[nearestEdge.y]);

						pb_EditorHandleUtility.EndDrawingLines();
					}
				}
			}

			using (new pb_HandleGUI())
			{
				int screenWidth = (int) sceneView.position.width;
				int screenHeight = (int) sceneView.position.height;

				int currentSelectionMode =
					(editLevel != EditLevel.Top && editLevel != EditLevel.Plugin) ? ((int) selectionMode) + 1 : 0;

				switch (m_SceneToolbarLocation)
				{
					case SceneToolbarLocation.BottomCenter:
						elementModeToolbarRect.x = (screenWidth / 2 - 64);
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
						elementModeToolbarRect.x = (screenWidth / 2 - 64);
						elementModeToolbarRect.y = 10;
						break;
				}

				EditorGUI.BeginChangeCheck();

				currentSelectionMode =
					GUI.Toolbar(elementModeToolbarRect, (int) currentSelectionMode, m_EditModeIcons, commandStyle);

				if (EditorGUI.EndChangeCheck())
				{
					if (currentSelectionMode == 0)
					{
						SetEditLevel(EditLevel.Top);
					}
					else
					{
						if (editLevel != EditLevel.Geometry)
							SetEditLevel(EditLevel.Geometry);

						SetSelectionMode((SelectMode) (currentSelectionMode - 1));
					}
				}

				if (movingVertices && m_ShowSceneInfo)
				{
					string handleTransformInfo = string.Format(
						"translate: <b>{0}</b>\nrotate: <b>{1}</b>\nscale: <b>{2}</b>",
						(newPosition - translateOrigin).ToString(),
						(currentHandleRotation.eulerAngles - rotateOrigin).ToString(),
						(currentHandleScale - scaleOrigin).ToString());

					var gc = pb_EditorGUIUtility.TempGUIContent(handleTransformInfo);
					// sceneview screen.height includes the tab and toolbar
					var toolbarHeight = EditorStyles.toolbar.CalcHeight(gc, Screen.width);
					var size = pb_EditorStyles.sceneTextBox.CalcSize(gc);

					Rect handleTransformInfoRect = new Rect(
						sceneView.position.width - (size.x + 8), sceneView.position.height - (size.y + 8 + toolbarHeight),
						size.x,
						size.y);

					GUI.Label(handleTransformInfoRect, gc, pb_EditorStyles.sceneTextBox);
				}

				if (m_ShowSceneInfo)
				{
					Vector2 size = pb_EditorStyles.sceneTextBox.CalcSize(m_SceneInfo);
					sceneInfoRect.width = size.x;
					sceneInfoRect.height = size.y;
					GUI.Label(sceneInfoRect, m_SceneInfo, pb_EditorStyles.sceneTextBox);
				}

				// Enables vertex selection with a mouse click
				if (editLevel == EditLevel.Geometry && !m_IsDragging && selectionMode == SelectMode.Vertex)
					m_MouseClickRect = new Rect(m_CurrentEvent.mousePosition.x - 10, m_CurrentEvent.mousePosition.y - 10, 20, 20);
				else
					m_MouseClickRect = pb_Constant.RectZero;

				if (m_IsDragging)
				{
					if (m_CurrentEvent.type == EventType.Repaint)
					{
						// Always draw from lowest to largest values
						var start = Vector2.Min(mousePosition_initial, m_CurrentEvent.mousePosition);
						var end = Vector2.Max(mousePosition_initial, m_CurrentEvent.mousePosition);

						m_MouseDragRect = new Rect(start.x, start.y, end.x - start.x, end.y - start.y);

						SceneStyles.selectionRect.Draw(m_MouseDragRect, false, false, false, false);
					}
					else if (m_CurrentEvent.isMouse)
					{
						HandleUtility.Repaint();
					}
				}
			}
		}

		internal bool ShortcutCheck(Event e)
		{
			List<pb_Shortcut> matches = m_Shortcuts.Where(x => x.Matches(e.keyCode, e.modifiers)).ToList();

			if (matches.Count < 1)
				return false;

			bool used = false;
			pb_Shortcut usedShortcut = null;

			foreach (pb_Shortcut cut in matches)
			{
				if (AllLevelShortcuts(cut))
				{
					used = true;
					usedShortcut = cut;
					break;
				}
			}

			if (!used)
			{
				foreach (pb_Shortcut cut in matches)
				{
					switch (editLevel)
					{
						case EditLevel.Top:
							break;

						case EditLevel.Texture:
							goto case EditLevel.Geometry;

						case EditLevel.Geometry:
							used = GeoLevelShortcuts(cut);
							break;
					}

					if (used)
					{
						usedShortcut = cut;
						break;
					}
				}
			}

			if (used)
			{
				if (usedShortcut.action != "Delete Face" &&
				    usedShortcut.action != "Escape" &&
				    usedShortcut.action != "Quick Apply Nodraw" &&
				    usedShortcut.action != "Toggle Geometry Mode" &&
				    usedShortcut.action != "Toggle Handle Pivot" &&
				    usedShortcut.action != "Toggle Selection Mode")
					pb_EditorUtility.ShowNotification(usedShortcut.action);

				Event.current.Use();
			}

			return used;
		}

		bool AllLevelShortcuts(pb_Shortcut shortcut)
		{
			bool uniqueModeShortcuts = pb_PreferencesInternal.GetBool(pb_Constant.pbUniqueModeShortcuts);

			switch (shortcut.action)
			{
				// TODO Remove once a workaround for non-upper-case shortcut chars is found
				case "Toggle Geometry Mode":

					if (editLevel == EditLevel.Geometry)
					{
						pb_EditorUtility.ShowNotification("Top Level Editing");
						SetEditLevel(EditLevel.Top);
					}
					else if (!uniqueModeShortcuts)
					{
						pb_EditorUtility.ShowNotification("Geometry Editing");
						SetEditLevel(EditLevel.Geometry);
					}

					return true;

				case "Vertex Mode":
				{
					if (!uniqueModeShortcuts)
						return false;

					if (editLevel == EditLevel.Top)
						SetEditLevel(EditLevel.Geometry);

					SetSelectionMode(SelectMode.Vertex);
					return true;
				}

				case "Edge Mode":
				{
					if (!uniqueModeShortcuts)
						return false;

					if (editLevel == EditLevel.Top)
						SetEditLevel(EditLevel.Geometry);

					SetSelectionMode(SelectMode.Edge);
					return true;
				}

				case "Face Mode":
				{
					if (!uniqueModeShortcuts)
						return false;

					if (editLevel == EditLevel.Top)
						SetEditLevel(EditLevel.Geometry);

					SetSelectionMode(SelectMode.Face);
					return true;
				}

				default:
					return false;
			}
		}

		bool GeoLevelShortcuts(pb_Shortcut shortcut)
		{
			switch (shortcut.action)
			{
				case "Escape":
					ClearElementSelection();
					pb_EditorUtility.ShowNotification("Top Level");
					UpdateSelection(false);
					SetEditLevel(EditLevel.Top);
					return true;

				// TODO Remove once a workaround for non-upper-case shortcut chars is found
				case "Toggle Selection Mode":

					if (pb_PreferencesInternal.GetBool(pb_Constant.pbUniqueModeShortcuts))
						return false;

					ToggleSelectionMode();
					switch (selectionMode)
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
					if (m_SelectedVertexCount < 1)
						return false;

					if (editLevel != EditLevel.Texture)
					{
						ToggleHandleAlignment();
						pb_EditorUtility.ShowNotification("Handle Alignment: " + ((HandleAlignment) handleAlignment).ToString());
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

		/// <summary>
		/// Allows another window to tell the Editor what Tool is now in use. Does *not* update any other windows.
		/// </summary>
		/// <param name="newTool"></param>
		internal void SetTool(Tool newTool)
		{
			currentHandle = newTool;
		}

		/// <summary>
		/// Calls SetTool(), then Updates the UV Editor window if applicable.
		/// </summary>
		/// <param name="newTool"></param>
		void SetTool_Internal(Tool newTool)
		{
			SetTool(newTool);

			if (pb_UVEditor.instance != null)
				pb_UVEditor.instance.SetTool(newTool);
		}

		internal void SetHandleAlignment(HandleAlignment ha)
		{
			if (editLevel == EditLevel.Texture)
				ha = HandleAlignment.Plane;
			else
				pb_PreferencesInternal.SetInt(pb_Constant.pbHandleAlignment, (int) ha);

			handleAlignment = ha;

			UpdateHandleRotation();

			currentHandleRotation = handleRotation;

			SceneView.RepaintAll();

			// todo
			Repaint();
		}

		internal void ToggleHandleAlignment()
		{
			int newHa = (int) handleAlignment + 1;
			if (newHa >= System.Enum.GetValues(typeof(HandleAlignment)).Length)
				newHa = 0;
			SetHandleAlignment((HandleAlignment) newHa);
		}

		/// <summary>
		/// Toggles between the SelectMode values and updates the graphic handles as necessary.
		/// </summary>
		internal void ToggleSelectionMode()
		{
			int smode = (int) selectionMode;
			smode++;
			if (smode >= k_SelectModeLength)
				smode = 0;
			SetSelectionMode((SelectMode) smode);
		}

		/// <summary>
		/// Sets the current selection mode @SelectMode to the mode value.
		/// </summary>
		/// <param name="mode"></param>
		public void SetSelectionMode(SelectMode mode)
		{
			selectionMode = mode;

			Internal_UpdateSelectionFast();

			pb_PreferencesInternal.SetInt(pb_Constant.pbDefaultSelectionMode, (int) selectionMode);

			SceneView.RepaintAll();
		}

		/// <summary>
		/// Set the EditLevel back to its last level.
		/// </summary>
		internal void PopEditLevel()
		{
			SetEditLevel(m_PreviousEditLevel);
		}

		/// <summary>
		/// Changes the current Editor level - switches between Object, Sub-object, and Texture (hidden).
		/// </summary>
		/// <param name="el"></param>
		public void SetEditLevel(EditLevel el)
		{
			m_PreviousEditLevel = editLevel;
			editLevel = el;

			switch (el)
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

					m_PreviousHandleAlignment = handleAlignment;
					m_PreviousSelectMode = selectionMode;

					SetHandleAlignment(HandleAlignment.Plane);
					break;
#endif
			}


#if !PROTOTYPE
			if (m_PreviousEditLevel == EditLevel.Texture && el != EditLevel.Texture)
			{
				SetSelectionMode(m_PreviousSelectMode);
				SetHandleAlignment(m_PreviousHandleAlignment);
			}
#endif

			if (editLevel != EditLevel.Texture)
				pb_PreferencesInternal.SetInt(pb_Constant.pbDefaultEditLevel, (int) editLevel);

			if (onEditLevelChanged != null)
				onEditLevelChanged((int) editLevel);
		}

		/**
		 *	\brief Updates the arrays used to draw GUI elements (both Window and Scene).
		 *	@selection_vertex should already be populated at this point.  UpdateSelection
		 *	just removes duplicate indices, and populates the gui arrays for displaying
		 *	 things like quad faces and vertex billboards.
		 */

		/// <summary>
		/// Rebuild the selection caches that help pb_Editor work.
		/// </summary>
		/// <param name="forceUpdate">Force update if elements have been added or removed, or the indices have been altered.</param>
		public void UpdateSelection(bool forceUpdate = true)
		{
////			profiler.BeginSample("UpdateSelection()");

//			profiler.BeginSample("CompareSequence");
			m_SelectedVertexCount = 0;
			m_SelectedFaceCount = 0;
			m_SelectedEdgeCount = 0;
			m_SelectedVerticesCommon = 0;

			pb_Object[] t_selection = selection;

			selection = pb_Util.GetComponents<pb_Object>(Selection.transforms);

			if (SelectedFacesInEditZone != null)
				SelectedFacesInEditZone.Clear();
			else
				SelectedFacesInEditZone = new Dictionary<pb_Object, List<pb_Face>>();

			bool selectionEqual = t_selection.SequenceEqual(selection);

//			profiler.EndSample();
//			profiler.BeginSample("forceUpdate");

			// If the top level selection has changed, update all the heavy cache things
			// that don't change based on element selction
			if (forceUpdate || !selectionEqual)
			{

				// If updating due to inequal selections, set the forceUpdate to true so some of the functions below
				// know that these values can be trusted.
				forceUpdate = true;

//				profiler.BeginSample("alloc pb_Edge[]");
				m_UniversalEdges = new pb_Edge[selection.Length][];
//				profiler.EndSample();

//				profiler.BeginSample("alloc dictionary[]");
				m_SharedIndicesDictionary = new Dictionary<int, int>[selection.Length];
//				profiler.EndSample();

//				profiler.BeginSample("get caches");
				for (int i = 0; i < selection.Length; i++)
				{
//					profiler.BeginSample("sharedIndices.ToDictionary()");
					m_SharedIndicesDictionary[i] = selection[i].sharedIndices.ToDictionary();
//					profiler.EndSample();

//					profiler.BeginSample("GetUniversalEdges (dictionary)");
					m_UniversalEdges[i] = pb_EdgeExtension.GetUniversalEdges(pb_EdgeExtension.AllEdges(selection[i].faces), m_SharedIndicesDictionary[i]);
//					profiler.EndSample();
				}
//				profiler.EndSample();
			}

//			profiler.EndSample();
//			profiler.BeginSample("get bounds");

			m_HandlePivotWorld = Vector3.zero;

			Vector3 min = Vector3.zero, max = Vector3.zero;
			var boundsInitialized = false;
			HashSet<int> used = new HashSet<int>();

			for (var i = 0; i < selection.Length; i++)
			{
				var lookup = m_SharedIndicesDictionary[i];
				used.Clear();

//				profiler.Begin("bounds");
				pb_Object pb = selection[i];

				if (!boundsInitialized && pb.SelectedTriangleCount > 0)
				{
					boundsInitialized = true;
					min = pb.transform.TransformPoint(pb.vertices[pb.SelectedTriangles[0]]);
					max = min;
				}

				if (pb.SelectedTriangleCount > 0)
				{
					var indices = pb.SelectedTriangles;

					for (int n = 0, c = pb.SelectedTriangleCount; n < c; n++)
					{
						if (used.Add(lookup[indices[n]]))
						{
							Vector3 v = pb.transform.TransformPoint(pb.vertices[indices[n]]);
							min = Vector3.Min(min, v);
							max = Vector3.Max(max, v);
						}
					}

					m_SelectedVerticesCommon += used.Count;
				}

//				profiler.End();
//				profiler.Begin("selected faces in edit zone");
				SelectedFacesInEditZone.Add(pb, pb_MeshUtils.GetNeighborFaces(pb, pb.SelectedTriangles, m_SharedIndicesDictionary[i]));

				m_SelectedVertexCount += selection[i].SelectedTriangles.Length;
				m_SelectedFaceCount += selection[i].SelectedFaceCount;
				m_SelectedEdgeCount += selection[i].SelectedEdges.Length;
//				profiler.End();
			}

			m_HandlePivotWorld = (max + min) * .5f;

//			profiler.EndSample();
//			profiler.BeginSample("update graphics");

			pb_MeshHandles.RebuildGraphics(selection, m_SharedIndicesDictionary, editLevel, selectionMode);

//			profiler.EndSample();
//			profiler.BeginSample("update handlerotation");

			UpdateHandleRotation();

//			profiler.EndSample();
//			profiler.BeginSample("update texture hadnles");

			UpdateTextureHandles();

//			profiler.EndSample();
//			profiler.BeginSample("onSelectionUpdate");

			currentHandleRotation = handleRotation;

			if (onSelectionUpdate != null)
				onSelectionUpdate(selection);
//			profiler.EndSample();

			UpdateSceneInfo();

//			profiler.EndSample();
		}

		void UpdateSceneInfo()
		{
			m_SceneInfo.text = string.Format(
				"Faces: <b>{0}</b>\nTriangles: <b>{1}</b>\nVertices: <b>{2} ({3})</b>\n\nSelected Faces: <b>{4}</b>\nSelected Edges: <b>{5}</b>\nSelected Vertices: <b>{6} ({7})</b>",
				pb_Selection.totalFaceCount,
				pb_Selection.totalTriangleCountCompiled,
				pb_Selection.totalCommonVertexCount,
				pb_Selection.totalVertexCountCompiled,
				m_SelectedFaceCount,
				m_SelectedEdgeCount,
				m_SelectedVerticesCommon,
				m_SelectedVertexCount);
		}

		// Only updates things that absolutely need to be refreshed, and assumes that no selection changes have occured
		internal void Internal_UpdateSelectionFast()
		{
			// profiler.BeginSample("Internal_UpdateSelectionFast");
			m_SelectedVertexCount = 0;
			m_SelectedFaceCount = 0;
			m_SelectedEdgeCount = 0;

			bool boundsInitialized = false;
			Vector3 min = Vector3.zero, max = Vector3.zero;

			for (int i = 0; i < selection.Length; i++)
			{
				pb_Object pb = selection[i];
				Vector3[] vertices = pb.vertices;
				int[] indices = pb.SelectedTriangles;

				if (pb == null) continue;

				if (selection[i].SelectedTriangleCount > 0)
				{
					if (!boundsInitialized)
					{
						boundsInitialized = true;
						min = pb.transform.TransformPoint(vertices[indices[0]]);
						max = min;
					}

					for (int n = 0; n < selection[i].SelectedTriangleCount; n++)
					{
						min = Vector3.Min(min, pb.transform.TransformPoint(vertices[indices[n]]));
						max = Vector3.Max(max, pb.transform.TransformPoint(vertices[indices[n]]));
					}
				}

				m_SelectedVertexCount += selection[i].SelectedTriangleCount;
				m_SelectedFaceCount += selection[i].SelectedFaceCount;
				m_SelectedEdgeCount += selection[i].SelectedEdges.Length;
			}

			m_HandlePivotWorld = (max + min) / 2f;

			pb_MeshHandles.RebuildGraphics(selection, m_SharedIndicesDictionary, editLevel, selectionMode);

			UpdateHandleRotation();
			currentHandleRotation = handleRotation;

			if (onSelectionUpdate != null)
				onSelectionUpdate(selection);

			UpdateSceneInfo();

			// profiler.EndSample();
		}

		public void ClearElementSelection()
		{
			foreach (pb_Object pb in selection)
				pb.ClearSelection();

			nearestEdge = pb_Edge.Empty;
			nearestEdgeObject = null;
		}

		void UpdateTextureHandles()
		{
			if (selection.Length < 1) return;

			// Reset temp vars
			textureHandle = m_HandlePivotWorld;
			textureScale = Vector3.one;
			textureRotation = Quaternion.identity;

			pb_Object pb;
			pb_Face face;

			handleMatrix = selection[0].transform.localToWorldMatrix;

			if (GetFirstSelectedFace(out pb, out face))
			{
				Vector3 nrm, bitan, tan;
				pb_Math.NormalTangentBitangent(pb, face, out nrm, out tan, out bitan);

				if (nrm == Vector3.zero || bitan == Vector3.zero)
				{
					nrm = Vector3.up;
					bitan = Vector3.right;
					tan = Vector3.forward;
				}

				handleMatrix *= Matrix4x4.TRS(pb_Math.BoundsCenter(pb.vertices.ValuesWithIndices(face.distinctIndices)),
					Quaternion.LookRotation(nrm, bitan), Vector3.one);
			}
		}

		internal void UpdateHandleRotation()
		{
			Quaternion localRot = Selection.activeTransform == null ? Quaternion.identity : Selection.activeTransform.rotation;

			switch (handleAlignment)
			{
				case HandleAlignment.Plane:

					if (Selection.transforms.Length > 1)
						goto case HandleAlignment.World;

					pb_Object pb;
					pb_Face face;

					if (!GetFirstSelectedFace(out pb, out face))
						goto case HandleAlignment.Local;

					// use average normal, tangent, and bitangent to calculate rotation relative to local space
					Vector3 nrm, bitan, tan;
					pb_Math.NormalTangentBitangent(pb, face, out nrm, out tan, out bitan);

					if (nrm == Vector3.zero || bitan == Vector3.zero)
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

		/// <summary>
		/// Find the nearest vertex among all visible objects.
		/// </summary>
		/// <param name="mousePosition"></param>
		/// <param name="vertex"></param>
		/// <returns></returns>
		bool FindNearestVertex(Vector2 mousePosition, out Vector3 vertex)
		{
			List<Transform> t =
				new List<Transform>(
					(Transform[]) pb_Util.GetComponents<Transform>(
						HandleUtility.PickRectObjects(new Rect(0, 0, Screen.width, Screen.height))));

			GameObject nearest = HandleUtility.PickGameObject(mousePosition, false);

			if (nearest != null)
				t.Add(nearest.transform);

			object[] parameters = new object[] { (Vector2) mousePosition, t.ToArray(), null };

			if (findNearestVertex == null)
				findNearestVertex = typeof(HandleUtility).GetMethod("findNearestVertex",
					BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);

			object result = findNearestVertex.Invoke(this, parameters);
			vertex = (bool) result ? (Vector3) parameters[2] : Vector3.zero;
			return (bool) result;
		}

		/// <summary>
		/// If dragging a texture aroudn, this method ensures that if it's a member of a texture group it's cronies are also selected
		/// </summary>
		void VerifyTextureGroupSelection()
		{
			foreach (pb_Object pb in selection)
			{
				List<int> alreadyChecked = new List<int>();

				foreach (pb_Face f in pb.SelectedFaces)
				{
					int tg = f.textureGroup;
					if (tg > 0 && !alreadyChecked.Contains(f.textureGroup))
					{
						foreach (pb_Face j in pb.faces)
							if (j != f && j.textureGroup == tg && !pb.SelectedFaces.Contains(j))
							{
								// int i = EditorUtility.DisplayDialogComplex("Mixed Texture Group Selection", "One or more of the faces selected belong to a Texture Group that does not have all it's member faces selected.  To modify, please either add the remaining group faces to the selection, or remove the current face from this smoothing group.", "Add Group to Selection", "Cancel", "Remove From Group");
								int i = 0;
								switch (i)
								{
									case 0:
										List<pb_Face> newFaceSection = new List<pb_Face>();
										foreach (pb_Face jf in pb.faces)
											if (jf.textureGroup == tg)
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

		void OnObjectSelectionChanged()
		{
			nearestEdge = pb_Edge.Empty;
			nearestEdgeObject = null;

			UpdateSelection(false);
			HideSelectedWireframe();
		}

		/// <summary>
		/// Hide the default unity wireframe renderer
		/// </summary>
		void HideSelectedWireframe()
		{
			foreach (pb_Object pb in selection)
				pb_EditorUtility.SetSelectionRenderState(pb.gameObject.GetComponent<Renderer>(),
					pb_EditorUtility.GetSelectionRenderState() & SelectionRenderState.Outline);

			SceneView.RepaintAll();
		}

		/// <summary>
		/// Called from ProGrids.
		/// </summary>
		/// <param name="snapVal"></param>
		void PushToGrid(float snapVal)
		{
			pb_Undo.RecordSelection(selection, "Push elements to Grid");

			if (editLevel == EditLevel.Top)
				return;

			for (int i = 0; i < selection.Length; i++)
			{
				pb_Object pb = selection[i];

				int[] indices = pb.SelectedTriangleCount > 0
					? pb.sharedIndices.AllIndicesWithValues(pb.SelectedTriangles).ToArray()
					: pb.msh.triangles;

				pb_VertexOps.Quantize(pb, indices, Vector3.one * snapVal);

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			Internal_UpdateSelectionFast();
		}

		void ProGridsToolbarOpen(bool menuOpen)
		{
			bool active = pb_ProGridsInterface.ProGridsActive();
			sceneInfoRect.y = active && !menuOpen ? 28 : 10;
			sceneInfoRect.x = active ? (menuOpen ? 64 : 8) : 10;
		}

		/// <summary>
		/// A tool, any tool, has just been engaged while in texture mode
		/// </summary>
		internal void OnBeginTextureModification()
		{
			VerifyTextureGroupSelection();
		}

		/// <summary>
		/// When beginning a vertex modification, nuke the UV2 and rebuild the mesh using PB data so that triangles
		/// match vertices (and no inserted vertices from the Unwrapping.GenerateSecondaryUVSet() remain).
		/// </summary>
		void OnBeginVertexMovement()
		{
			switch (currentHandle)
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

			foreach (pb_Object pb in selection)
			{
				pb.ToMesh();
				pb.Refresh();
			}

			if (onVertexMovementBegin != null)
				onVertexMovementBegin(selection);
		}

		void OnFinishVertexModification()
		{
			pb_Lightmapping.PopGIWorkflowMode();

			currentHandleScale = Vector3.one;
			currentHandleRotation = handleRotation;

			if (movingPictures)
			{
				if (pb_UVEditor.instance != null)
					pb_UVEditor.instance.OnFinishUVModification();

				UpdateTextureHandles();

				movingPictures = false;
			}
			else if (movingVertices)
			{
				foreach (pb_Object sel in selection)
				{
					sel.ToMesh();
					sel.Refresh();
					sel.Optimize();
				}

				movingVertices = false;
			}

			if (onVertexMovementFinish != null)
				onVertexMovementFinish(selection);

			scaling = false;
		}

		/// <summary>
		/// Returns the first selected pb_Object and pb_Face, or false if not found.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="face"></param>
		/// <returns></returns>
		internal bool GetFirstSelectedFace(out pb_Object pb, out pb_Face face)
		{
			pb = null;
			face = null;

			if (selection.Length < 1) return false;

			pb = selection.FirstOrDefault(x => x.SelectedFaceCount > 0);

			if (pb == null)
				return false;

			face = pb.SelectedFaces[0];

			return true;
		}

		/// <summary>
		/// Returns the first selected pb_Object and pb_Face, or false if not found.
		/// </summary>
		/// <param name="mat"></param>
		/// <returns></returns>
		internal bool GetFirstSelectedMaterial(ref Material mat)
		{
			for (int i = 0; i < selection.Length; i++)
			{
				for (int n = 0; n < selection[i].SelectedFaceCount; n++)
				{
					mat = selection[i].SelectedFaces[i].material;

					if (mat != null)
						return true;
				}
			}

			return false;
		}

		// Handy calls -- currentEvent must be set, so only call in the OnGUI loop!
		bool altClick
		{
			get { return (m_CurrentEvent.alt); }
		}

		bool leftClick
		{
			get { return (m_CurrentEvent.type == EventType.MouseDown); }
		}

		bool leftClickUp
		{
			get { return (m_CurrentEvent.type == EventType.MouseUp); }
		}

		bool contextClick
		{
			get { return (m_CurrentEvent.type == EventType.ContextClick); }
		}

		bool mouseDrag
		{
			get { return (m_CurrentEvent.type == EventType.MouseDrag); }
		}

		bool ignore
		{
			get { return m_CurrentEvent.type == EventType.Ignore; }
		}

		bool rightClick
		{
			get { return (m_CurrentEvent.type == EventType.ContextClick); }
		}

		bool shiftKey
		{
			get { return m_CurrentEvent.shift; }
		}

		bool ctrlKey
		{
			get { return m_CurrentEvent.command || m_CurrentEvent.control; }
		}

		KeyCode getKeyUp
		{
			get { return m_CurrentEvent.type == EventType.KeyUp ? m_CurrentEvent.keyCode : KeyCode.None; }
		}
	}
}

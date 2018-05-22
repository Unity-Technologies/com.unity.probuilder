using System;
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder.UI;
using PMesh = UnityEngine.ProBuilder.ProBuilderMesh;
using UnityEngine.ProBuilder.MeshOperations;
using Math = UnityEngine.ProBuilder.Math;
using Object = UnityEngine.Object;
using RaycastHit = UnityEngine.ProBuilder.RaycastHit;

namespace UnityEditor.ProBuilder
{
	/// <summary>
	/// Manages the ProBuilder toolbar window and tool mode.
	/// </summary>
	public sealed class ProBuilderEditor : EditorWindow
	{
		/// <value>
		/// Raised any time the ProBuilder editor refreshes the selection. This is called every frame when interacting with mesh elements, and after any mesh operation.
		/// </value>
		public static event Action<ProBuilderMesh[]> onSelectionUpdate;

        /// <value>
        /// Called when vertex modifications are complete.
        /// </value>
        public static event Action<ProBuilderMesh[]> onVertexMovementFinish;

        /// <value>
        /// Called immediately prior to beginning vertex modifications. The ProBuilderMesh will be in un-altered state at this point (meaning ProBuilderMesh.ToMesh and ProBuilderMesh.Refresh have been called, but not Optimize).
        /// </value>
        public static event Action<ProBuilderMesh[]> onVertexMovementBegin;

		/// <value>
		/// Raised when the EditLevel is changed.
		/// </value>
        public static event Action<int> onEditLevelChanged;

        // Toggles for Face, Vertex, and Edge mode.
        const int k_SelectModeLength = 3;
		const float k_MaxEdgeSelectDistanceHam = 128;
		const float k_MaxEdgeSelectDistanceCtx = 12;

		static EditorToolbar s_EditorToolbar;
		static ProBuilderEditor s_Instance;
		static int s_DeepSelectionPrevious = 0x0;

		GUIContent[] m_EditModeIcons;
		GUIStyle VertexTranslationInfoStyle;

		bool m_ShowSceneInfo;
		bool m_HamSelection;

		float m_SnapValue = .25f;
		bool m_SnapAxisConstraint = true;
		bool m_SnapEnabled;
		bool m_IsIconGui;
		MethodInfo m_FindNearestVertex;
		EditLevel m_PreviousEditLevel;
		SelectMode m_PreviousSelectMode;
		HandleAlignment m_PreviousHandleAlignment;
        DragSelectMode m_DragSelectMode;
		Shortcut[] m_Shortcuts;
		SceneToolbarLocation m_SceneToolbarLocation;
		GUIStyle m_CommandStyle;
		Rect m_ElementModeToolbarRect = new Rect(3, 6, 128, 24);
		bool m_SelectHiddenEnabled;
		SimpleTuple<PMesh, Edge> m_NearestEdge = new SimpleTuple<ProBuilderMesh, Edge>();

		// the mouse vertex selection box
		Rect m_MouseClickRect = new Rect(0, 0, 0, 0);
		Tool m_CurrentTool = Tool.Move;
		Vector2 m_InitialMousePosition;
		Rect m_MouseDragRect;
		bool m_IsDragging;
		bool m_IsReadyForMouseDrag;
		// prevents leftClickUp from stealing focus after double click
		bool m_WasDoubleClick;
		// vertex handles
		Vector3 m_ElementHandlePosition;
		Vector3 m_ElementHandleCachedPosition;
		bool m_IsMovingElements;
		bool m_IsRightMouseDown;

		bool m_DoSnapToVertex;
		bool m_DoSnapToFace;
		Vector3 m_HandleScalePrevious = Vector3.one;
		Vector3 m_HandleScale = Vector3.one;
		Vector3[][] m_VertexPositions;
		Vector3[] m_VertexOffset;
		Quaternion m_HandleRotationPrevious = Quaternion.identity;
		Quaternion m_HandleRotation = Quaternion.identity;
		Quaternion m_RotationInitial;
		Quaternion m_RotationInitialInverse;

		GUIContent m_SceneInfo = new GUIContent();

		// Use for delta display
		Vector3 m_TranslateOrigin = Vector3.zero;
		Vector3 m_RotateOrigin = Vector3.zero;
		Vector3 m_ScaleOrigin = Vector3.zero;

		Vector3 m_TextureHandlePosition = Vector3.zero;
		Vector3 m_TextureHandlePositionPrevious = Vector3.zero;
		bool m_IsMovingTextures;
		Quaternion m_TextureRotation = Quaternion.identity;
		Vector3 m_TextureScale = Vector3.one;
		Rect m_SceneInfoRect = new Rect(10, 10, 200, 40);

		Edge[][] m_UniversalEdges = new Edge[0][];
		Vector3 m_HandlePivotWorld = Vector3.zero;
		Dictionary<int, int>[] m_SharedIndicesDictionary = new Dictionary<int, int>[0];

		internal Edge[][] selectedUniversalEdges
		{
			get { return m_UniversalEdges; }
		}

		/// <summary>
		/// Faces that need to be refreshed when moving or modifying the actual selection
		/// </summary>
		public Dictionary<ProBuilderMesh, List<Face>> selectedFacesInEditZone { get; private set; }

		Matrix4x4 handleMatrix = Matrix4x4.identity;
		Quaternion handleRotation = new Quaternion(0f, 0f, 0f, 1f);

#if !UNITY_2018_2_OR_NEWER
		static MethodInfo s_ResetOnSceneGUIState = null;
#endif

		// All selected pb_Objects
		internal ProBuilderMesh[] selection = new ProBuilderMesh[0];

		// Sum of all vertices selected
		int m_SelectedVertexCount;

		// Sum of all vertices selected, not counting duplicates on common positions
		int m_SelectedVerticesCommon;

		// Sum of all faces selected
		int m_SelectedFaceCount;

		// Sum of all edges sleected
		int m_SelectedEdgeCount;

		internal int selectedVertexCount { get { return m_SelectedVertexCount; } }
		internal int selectedVertexCommonCount { get { return m_SelectedVerticesCommon; } }
		internal int selectedFaceCount { get { return m_SelectedFaceCount; } }
		internal int selectedEdgeCount { get { return m_SelectedEdgeCount; } }

		Event m_CurrentEvent;

		internal bool isFloatingWindow { get; private set; }
		internal bool selectHiddenEnabled { get { return m_SelectHiddenEnabled; } }

		/// <value>
		/// Get the current @"UnityEngine.ProBuilder.EditLevel".
		/// </value>
		public EditLevel editLevel { get; private set; }

		/// <summary>
		/// Get the current @"UnityEngine.ProBuilder.SelectMode".
		/// </summary>
		/// <value>The SelectMode currently set.</value>
		public SelectMode selectionMode { get; private set; }

		/// <summary>
		/// Get the alignment of the ProBuilder transform gizmo.
		/// </summary>
		/// <seealso cref="HandleAlignment"/>
		public HandleAlignment handleAlignment { get; private set; }

		static class SceneStyles
		{
			static bool s_Init = false;
			static GUIStyle s_SelectionRect;

			public static GUIStyle selectionRect
			{
				get { return s_SelectionRect; }
			}

			public static void Init()
			{
				if (s_Init)
					return;

				s_Init = true;

				s_SelectionRect = new GUIStyle()
				{
					normal = new GUIStyleState()
					{
						background = IconUtility.GetIcon("Scene/SelectionRect")
					},
					border = new RectOffset(1,1,1,1),
					margin = new RectOffset(0,0,0,0),
					padding = new RectOffset(0,0,0,0)
				};
			}
		}

		/// <value>
		/// Get the active ProBuilderEditor window. Null if no instance is open.
		/// </value>
		public static ProBuilderEditor instance
		{
			get { return s_Instance; }
		}

		internal static void MenuOpenWindow()
		{
			ProBuilderEditor editor = (ProBuilderEditor) EditorWindow.GetWindow(typeof(ProBuilderEditor),
				!PreferencesInternal.GetBool(PreferenceKeys.pbDefaultOpenInDockableWindow), PreferenceKeys.pluginTitle,
				true); // open as floating window
			// would be nice if editorwindow's showMode was exposed
			editor.isFloatingWindow = !PreferencesInternal.GetBool(PreferenceKeys.pbDefaultOpenInDockableWindow);
		}

		internal void OnEnable()
		{
			s_Instance = this;

			MeshHandles.Initialize();

			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			SceneView.onSceneGUIDelegate += this.OnSceneGUI;

			ProGridsInterface.SubscribePushToGridEvent(PushToGrid);
			ProGridsInterface.SubscribeToolbarEvent(ProGridsToolbarOpen);

			ProGridsToolbarOpen(ProGridsInterface.SceneToolbarIsExtended());

			MeshSelection.onObjectSelectionChanged += OnObjectSelectionChanged;

#if !UNITY_2018_2_OR_NEWER
			s_ResetOnSceneGUIState = typeof(SceneView).GetMethod("ResetOnSceneGUIState", BindingFlags.Instance | BindingFlags.NonPublic);
#endif

			// make sure load prefs is called first, because other methods depend on the preferences set here
			LoadPrefs();
			InitGUI();
			UpdateSelection(true);
			HideSelectedWireframe();

			m_FindNearestVertex = typeof(HandleUtility).GetMethod("FindNearestVertex",
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

			MeshHandles.Destroy();

			if (onSelectionUpdate != null)
				onSelectionUpdate(null);

			ProGridsInterface.UnsubscribePushToGridEvent(PushToGrid);
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			PreferencesInternal.SetInt(PreferenceKeys.pbHandleAlignment, (int) handleAlignment);
			MeshSelection.onObjectSelectionChanged -= OnObjectSelectionChanged;

			// re-enable unity wireframe
			// todo set wireframe override in pb_Selection, no pb_Editor
			foreach (var pb in FindObjectsOfType<ProBuilderMesh>())
				EditorUtility.SetSelectionRenderState(pb.gameObject.GetComponent<Renderer>(),
					EditorUtility.GetSelectionRenderState());

			SceneView.RepaintAll();
		}

		void OnDestroy()
		{
		}

		internal void LoadPrefs()
		{
			PreferencesUpdater.CheckEditorPrefsVersion();

			editLevel = PreferencesInternal.GetEnum<EditLevel>(PreferenceKeys.pbDefaultEditLevel);
			selectionMode = PreferencesInternal.GetEnum<SelectMode>(PreferenceKeys.pbDefaultSelectionMode);
			handleAlignment = PreferencesInternal.GetEnum<HandleAlignment>(PreferenceKeys.pbHandleAlignment);
			m_ShowSceneInfo = PreferencesInternal.GetBool(PreferenceKeys.pbShowSceneInfo);
			m_HamSelection = PreferencesInternal.GetBool(PreferenceKeys.pbElementSelectIsHamFisted);
			m_SelectHiddenEnabled = PreferencesInternal.GetBool(PreferenceKeys.pbEnableBackfaceSelection);

			m_SnapEnabled = ProGridsInterface.SnapEnabled();
			m_SnapValue = ProGridsInterface.SnapValue();
			m_SnapAxisConstraint = ProGridsInterface.UseAxisConstraints();

			m_Shortcuts = Shortcut.ParseShortcuts(PreferencesInternal.GetString(PreferenceKeys.pbDefaultShortcuts)).ToArray();

			m_SceneToolbarLocation = PreferencesInternal.GetEnum<SceneToolbarLocation>(PreferenceKeys.pbToolbarLocation);
			m_IsIconGui = PreferencesInternal.GetBool(PreferenceKeys.pbIconGUI);
			m_DragSelectMode = PreferencesInternal.GetEnum<DragSelectMode>(PreferenceKeys.pbDragSelectMode);
		}

		void InitGUI()
		{
			if (s_EditorToolbar != null)
				Object.DestroyImmediate(s_EditorToolbar);

			s_EditorToolbar = ScriptableObject.CreateInstance<EditorToolbar>();
			s_EditorToolbar.hideFlags = HideFlags.HideAndDontSave;
			s_EditorToolbar.InitWindowProperties(this);

			VertexTranslationInfoStyle = new GUIStyle();
			VertexTranslationInfoStyle.normal.background = EditorGUIUtility.whiteTexture;
			VertexTranslationInfoStyle.normal.textColor = new Color(1f, 1f, 1f, .6f);
			VertexTranslationInfoStyle.padding = new RectOffset(3, 3, 3, 0);

			var object_Graphic_off = IconUtility.GetIcon("Modes/Mode_Object");
			var face_Graphic_off = IconUtility.GetIcon("Modes/Mode_Face");
			var vertex_Graphic_off = IconUtility.GetIcon("Modes/Mode_Vertex");
			var edge_Graphic_off = IconUtility.GetIcon("Modes/Mode_Edge");

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

		/// <summary>
		/// Rebuild the mesh wireframe and selection caches.
		/// </summary>
		/// <param name="vertexCountChanged">An optional parameter that allows Refresh to skip some more expensive calculations when rebuilding caches if the vertex count and face layout has not changed.</param>
		public static void Refresh(bool vertexCountChanged = true)
		{
			if (instance != null)
				instance.UpdateSelection(vertexCountChanged);
		}

		void OnGUI()
		{
			if (m_CommandStyle == null)
				m_CommandStyle = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("Command");

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
				!PreferencesInternal.GetBool(PreferenceKeys.pbDefaultOpenInDockableWindow, true), Menu_OpenAsFloatingWindow);
			menu.AddItem(new GUIContent("Open As Dockable Window", ""),
				PreferencesInternal.GetBool(PreferenceKeys.pbDefaultOpenInDockableWindow, true), Menu_OpenAsDockableWindow);

			menu.AddSeparator("");

			menu.AddItem(new GUIContent("Use Icon Mode", ""), PreferencesInternal.GetBool(PreferenceKeys.pbIconGUI),
				Menu_ToggleIconMode);
			menu.AddItem(new GUIContent("Use Text Mode", ""), !PreferencesInternal.GetBool(PreferenceKeys.pbIconGUI),
				Menu_ToggleIconMode);

			menu.ShowAsContext();
		}

		void Menu_ToggleIconMode()
		{
			m_IsIconGui = !PreferencesInternal.GetBool(PreferenceKeys.pbIconGUI);
			PreferencesInternal.SetBool(PreferenceKeys.pbIconGUI, m_IsIconGui);
			if (s_EditorToolbar != null)
				Object.DestroyImmediate(s_EditorToolbar);
			s_EditorToolbar = ScriptableObject.CreateInstance<EditorToolbar>();
			s_EditorToolbar.hideFlags = HideFlags.HideAndDontSave;
			s_EditorToolbar.InitWindowProperties(this);
		}

		void Menu_OpenAsDockableWindow()
		{
			PreferencesInternal.SetBool(PreferenceKeys.pbDefaultOpenInDockableWindow, true);
			EditorWindow.GetWindow<ProBuilderEditor>().Close();
			ProBuilderEditor.MenuOpenWindow();
		}

		void Menu_OpenAsFloatingWindow()
		{
			PreferencesInternal.SetBool(PreferenceKeys.pbDefaultOpenInDockableWindow, false);
			EditorWindow.GetWindow<ProBuilderEditor>().Close();
			ProBuilderEditor.MenuOpenWindow();
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
					m_DoSnapToVertex = true;
				else if (m_CurrentEvent.Equals(Event.KeyboardEvent("c")))
					m_DoSnapToFace = true;
			}

			// Snap stuff
			if (m_CurrentEvent.type == EventType.KeyUp)
			{
				m_DoSnapToFace = false;
				m_DoSnapToVertex = false;
			}

			if (m_CurrentEvent.type == EventType.MouseDown && m_CurrentEvent.button == 1)
				m_IsRightMouseDown = true;

			if (m_CurrentEvent.type == EventType.MouseUp && m_CurrentEvent.button == 1 || m_CurrentEvent.type == EventType.Ignore)
				m_IsRightMouseDown = false;

			MeshHandles.DoGUI(selectionMode);

			DrawHandleGUI(sceneView);

			if (!m_IsRightMouseDown && (m_CurrentEvent.type == EventType.KeyUp ? m_CurrentEvent.keyCode : KeyCode.None) != KeyCode.None)
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
			if ((m_IsMovingElements || m_IsMovingTextures) && GUIUtility.hotControl < 1)
			{
				OnFinishVertexModification();
				UpdateHandleRotation();
				UpdateTextureHandles();
			}

			// Check mouse position in scene and determine if we should highlight something
			if (m_CurrentEvent.type == EventType.MouseMove && editLevel == EditLevel.Geometry)
				UpdateMouse(m_CurrentEvent.mousePosition);

			if (Tools.current != Tool.None && Tools.current != m_CurrentTool)
				SetTool_Internal(Tools.current);

			if ((editLevel == EditLevel.Geometry || editLevel == EditLevel.Texture) && Tools.current != Tool.View)
			{
				if (m_SelectedVertexCount > 0)
				{
					if (editLevel == EditLevel.Geometry)
					{
						switch (m_CurrentTool)
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
						switch (m_CurrentTool)
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

			// m_CurrentEvent.alt || Tools.current == Tool.View || GUIUtility.hotControl > 0 || middleClick
			// Tools.viewTool == ViewTool.FPS || Tools.viewTool == ViewTool.Orbit
			if (EditorHandleUtility.SceneViewInUse(m_CurrentEvent) || m_CurrentEvent.isKey || selection == null ||
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

			if (m_CurrentEvent.type == EventType.MouseDown)
			{
				// double clicking object
				if (m_CurrentEvent.clickCount > 1)
				{
					DoubleClick(m_CurrentEvent);
				}

				m_InitialMousePosition = m_CurrentEvent.mousePosition;
				// readyForMouseDrag prevents a bug wherein after ending a drag an errant
				// MouseDrag event is sent with no corresponding MouseDown/MouseUp event.
				m_IsReadyForMouseDrag = true;
			}

			if (m_CurrentEvent.type == EventType.MouseDrag && m_IsReadyForMouseDrag)
			{
				if(!m_IsDragging)
					sceneView.Repaint();

				m_IsDragging = true;
			}

			if (m_CurrentEvent.type == EventType.Ignore)
			{
				if (m_IsDragging)
				{
					m_IsReadyForMouseDrag = false;
					m_IsDragging = false;
					DragCheck();
				}

				if (m_WasDoubleClick)
					m_WasDoubleClick = false;
			}

			if (m_CurrentEvent.type == EventType.MouseUp)
			{
				if (m_WasDoubleClick)
				{
					m_WasDoubleClick = false;
				}
				else
				{
					if (!m_IsDragging)
					{
						if (UVEditor.instance)
							UVEditor.instance.ResetUserPivot();

						RaycastCheck(m_CurrentEvent.mousePosition);
					}
					else
					{
						m_IsDragging = false;
						m_IsReadyForMouseDrag = false;

						if (UVEditor.instance)
							UVEditor.instance.ResetUserPivot();

						DragCheck();
					}
				}
			}
		}

		void DoubleClick(Event e)
		{
			ProBuilderMesh pb = RaycastCheck(e.mousePosition, -1);

			if (pb != null)
			{
				if (selectionMode == SelectMode.Edge)
				{
					if (e.shift)
						MenuCommands.MenuRingSelection(selection);
					else
						MenuCommands.MenuLoopSelection(selection);
				}
				else if (selectionMode == SelectMode.Face)
				{
					if ((e.modifiers & (EventModifiers.Control | EventModifiers.Shift)) ==
					    (EventModifiers.Control | EventModifiers.Shift))
						MenuCommands.MenuRingAndLoopFaces(selection);
					else if (e.control)
						MenuCommands.MenuRingFaces(selection);
					else if (e.shift)
						MenuCommands.MenuLoopFaces(selection);
					else
						pb.SetSelectedFaces(pb.facesInternal);
				}
				else
				{
					pb.SetSelectedFaces(pb.facesInternal);
				}

				UpdateSelection(false);
				SceneView.RepaintAll();
				m_WasDoubleClick = true;
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

			Edge bestEdge = Edge.Empty;
			ProBuilderMesh bestObj = go == null ? null : go.GetComponent<ProBuilderMesh>();

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
						int x = selection[i].sharedIndicesInternal[edges[j].x][0];
						int y = selection[i].sharedIndicesInternal[edges[j].y][0];

						float d = HandleUtility.DistanceToLine(
							pb.transform.TransformPoint(pb.positionsInternal[x]),
							pb.transform.TransformPoint(pb.positionsInternal[y]));

						if (d < bestDistance)
						{
							bestObj = selection[i];
							bestEdge = new Edge(x, y);
							bestDistance = d;
						}
					}
				}
			}
			else
			{
				// Test culling
				List<RaycastHit> hits;
				Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);

				if (UnityEngine.ProBuilder.HandleUtility.FaceRaycast(ray, bestObj, out hits, CullingMode.Front))
				{
					Camera cam = SceneView.lastActiveSceneView.camera;

					// Sort from nearest hit to farthest
					hits.Sort((x, y) => x.distance.CompareTo(y.distance));

					// Find the nearest edge in the hit faces

					float bestDistance = Mathf.Infinity;
					Vector3[] v = bestObj.positionsInternal;

					for (int i = 0; i < hits.Count; i++)
					{
						if (UnityEngine.ProBuilder.HandleUtility.PointIsOccluded(cam, bestObj, bestObj.transform.TransformPoint(hits[i].point)))
							continue;

						foreach (Edge edge in bestObj.facesInternal[hits[i].face].edgesInternal)
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
						bestEdge = Edge.Empty;
				}
			}

			if (bestEdge != m_NearestEdge.item2 || bestObj != m_NearestEdge.item1)
			{
				m_NearestEdge.item2 = bestEdge;
				m_NearestEdge.item1 = bestObj;

				SceneView.RepaintAll();
			}
		}

		// Returns the pb_Object modified by this action.  If no action taken, or action is eaten by texture window, return null.
		// A pb_Object is returned because double click actions need to know what the last selected pb_Object was.
		// If deepClickOffset is specified, the object + deepClickOffset in the deep select stack will be returned (instead of next).
		ProBuilderMesh RaycastCheck(Vector3 mousePosition, int deepClickOffset = 0)
		{
			ProBuilderMesh pb = null;

			// Since Edge or Vertex selection may be valid even if clicking off a gameObject, check them
			// first. If no hits, move on to face selection or object change.
			if ((selectionMode == SelectMode.Edge && EdgeClickCheck(out pb)) ||
			    (selectionMode == SelectMode.Vertex && VertexClickCheck(out pb)))
			{
				UpdateSelection(false);
				SceneView.RepaintAll();
				return pb;
			}

			if (!m_CurrentEvent.shift && !(m_CurrentEvent.command || m_CurrentEvent.control))
				MeshSelection.SetSelection((GameObject) null);

			GameObject pickedGo = null;
			ProBuilderMesh pickedPb = null;
			Face pickedFace = null;
			int newHash = 0;

			List<GameObject> picked = EditorHandleUtility.GetAllOverlapping(mousePosition);

			EventModifiers em = Event.current.modifiers;

			// If any event modifiers are engaged don't cycle the deep click
			int pickedCount = em != EventModifiers.None ? System.Math.Min(1, picked.Count) : picked.Count;

			for (int i = 0, next = 0; i < pickedCount; i++)
			{
				GameObject go = picked[i];
				pb = go.GetComponent<ProBuilderMesh>();
				Face face = null;

				if (pb != null)
				{
					Ray ray = HandleUtility.GUIPointToWorldRay(mousePosition);
					RaycastHit hit;

					if (UnityEngine.ProBuilder.HandleUtility.FaceRaycast(ray,
						pb,
						out hit,
						Mathf.Infinity,
						selectHiddenEnabled ? CullingMode.FrontBack : CullingMode.Front))
					{
						face = pb.facesInternal[hit.face];
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
						MeshSelection.AddToSelection(pickedGo);

#if !PROTOTYPE
						// Check for other editor mouse shortcuts first
						MaterialEditor matEditor = MaterialEditor.instance;
						if (matEditor != null && matEditor.ClickShortcutCheck(Event.current.modifiers, pickedPb, pickedFace))
							return pickedPb;

						UVEditor uvEditor = UVEditor.instance;
						if (uvEditor != null && uvEditor.ClickShortcutCheck(pickedPb, pickedFace))
							return pickedPb;
#endif

						// Check to see if we've already selected this quad.  If so, remove it from selection cache.
						UndoUtility.RecordSelection(pickedPb, "Change Face Selection");

						int indx = System.Array.IndexOf(pickedPb.selectedFacesInternal, pickedFace);

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
				else if (!PreferencesInternal.GetBool(PreferenceKeys.pbPBOSelectionOnly))
				{
					// If clicked off a pb_Object but onto another gameobject, set the selection
					// and dip out.
					MeshSelection.SetSelection(pickedGo);
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

		bool VertexClickCheck(out ProBuilderMesh vpb)
		{
			if (!m_CurrentEvent.shift && !(m_CurrentEvent.command || m_CurrentEvent.control))
				ClearElementSelection();

			Camera cam = SceneView.lastActiveSceneView.camera;
			List<SimpleTuple<float, Vector3, int, int>> nearest = new List<SimpleTuple<float, Vector3, int, int>>();

			// this could be much faster by raycasting against the mesh and doing a 3d space
			// distance check first

			if (m_HamSelection)
			{
				const float minAllowableDistance = k_MaxEdgeSelectDistanceHam * k_MaxEdgeSelectDistanceHam;
				int obj = -1, tri = -1;
				Vector2 mousePosition = m_CurrentEvent.mousePosition;

				for (int i = 0; i < selection.Length; i++)
				{
					ProBuilderMesh pb = selection[i];

					if (!pb.isSelectable)
						continue;

					for (int n = 0, c = pb.sharedIndicesInternal.Length; n < c; n++)
					{
						int index = pb.sharedIndicesInternal[n][0];
						Vector3 v = pb.transform.TransformPoint(pb.positionsInternal[index]);
						Vector2 p = HandleUtility.WorldToGUIPoint(v);

						float dist = (p - mousePosition).sqrMagnitude;

						if (dist < minAllowableDistance)
							nearest.Add(new SimpleTuple<float, Vector3, int, int>(dist, v, i, index));
					}
				}

				nearest.Sort((x, y) => x.item1.CompareTo(y.item1));

				for (int i = 0; i < nearest.Count; i++)
				{
					obj = nearest[i].item3;

					if (!UnityEngine.ProBuilder.HandleUtility.PointIsOccluded(cam, selection[obj], nearest[i].item2))
					{
						tri = nearest[i].item4;
						break;
					}
				}

				if (obj > -1 && tri > -1)
				{
					ProBuilderMesh pb = selection[obj];

					int indx = System.Array.IndexOf(pb.selectedIndicesInternal, tri);

					UndoUtility.RecordSelection(pb, "Change Vertex Selection");

					// If we get a match, check to see if it exists in our selection array already, then add / remove
					if (indx > -1)
						pb.SetSelectedVertices(pb.selectedIndicesInternal.RemoveAt(indx));
					else
						pb.SetSelectedVertices(pb.selectedIndicesInternal.Add(tri));

					vpb = pb;
					return true;
				}
			}
			else
			{
				for (int i = 0; i < selection.Length; i++)
				{
					ProBuilderMesh pb = selection[i];

					if (!pb.isSelectable)
						continue;

					for (int n = 0, c = pb.sharedIndicesInternal.Length; n < c; n++)
					{
						int index = pb.sharedIndicesInternal[n][0];
						Vector3 v = pb.transform.TransformPoint(pb.positionsInternal[index]);

						if (m_MouseClickRect.Contains(HandleUtility.WorldToGUIPoint(v)))
						{
							if (UnityEngine.ProBuilder.HandleUtility.PointIsOccluded(cam, pb, v))
								continue;

							// Check if index is already selected, and if not add it to the pot
							int indx = System.Array.IndexOf(pb.selectedIndicesInternal, index);

							UndoUtility.RecordObject(pb, "Change Vertex Selection");

							// If we get a match, check to see if it exists in our selection array already, then add / remove
							if (indx > -1)
								pb.SetSelectedVertices(pb.selectedIndicesInternal.RemoveAt(indx));
							else
								pb.SetSelectedVertices(pb.selectedIndicesInternal.Add(index));

							vpb = pb;
							return true;
						}
					}
				}
			}

			vpb = null;
			return false;
		}

		bool EdgeClickCheck(out ProBuilderMesh pb)
		{
			if (!m_CurrentEvent.shift && !(m_CurrentEvent.command || m_CurrentEvent.control))
			{
				// don't call ClearElementSelection b/c that also removes
				// nearestEdge info
				foreach (ProBuilderMesh p in selection)
					p.ClearSelection();
			}

			if (m_NearestEdge.item1 != null)
			{
				pb = m_NearestEdge.item1;

				if (m_NearestEdge.item2.IsValid())
				{
					SimpleTuple<Face, Edge> edge;

					if (EdgeExtension.ValidateEdge(pb, m_NearestEdge.item2, out edge))
						m_NearestEdge.item2 = edge.item2;

					int ind = pb.selectedEdges.IndexOf(m_NearestEdge.item2, pb.sharedIndicesInternal.ToDictionary());

					UndoUtility.RecordSelection(pb, "Change Edge Selection");

					if (ind > -1)
						pb.SetSelectedEdges(pb.selectedEdges.ToArray().RemoveAt(ind));
					else
						pb.SetSelectedEdges(pb.selectedEdges.ToArray().Add(m_NearestEdge.item2));

					return true;
				}

				return false;
			}
			else
			{
				if (!m_CurrentEvent.shift && !(m_CurrentEvent.command || m_CurrentEvent.control))
					ClearElementSelection();

				pb = null;

				return false;
			}
		}

		void DragCheck()
		{
			SceneView sceneView = SceneView.lastActiveSceneView;
			Camera cam = sceneView.camera;

			UndoUtility.RecordSelection(selection, "Drag Select");
			bool selectHidden = selectHiddenEnabled;

			var pickingOptions = new PickerOptions()
			{
				depthTest = !selectHidden,
				rectSelectMode = PreferencesInternal.GetEnum<RectSelectMode>(PreferenceKeys.pbRectSelectMode)
			};

			switch (selectionMode)
			{
				case SelectMode.Vertex:
				{
					if (!m_CurrentEvent.shift && !(m_CurrentEvent.command || m_CurrentEvent.control))
						ClearElementSelection();

					Dictionary<ProBuilderMesh, HashSet<int>> selected = Picking.PickVerticesInRect(
						SceneView.lastActiveSceneView.camera,
						m_MouseDragRect,
						selection,
						pickingOptions,
						EditorGUIUtility.pixelsPerPoint);

					foreach (var kvp in selected)
					{
						IntArray[] sharedIndices = kvp.Key.sharedIndicesInternal;
						HashSet<int> common;

						if (m_CurrentEvent.shift || (m_CurrentEvent.command || m_CurrentEvent.control))
						{
							common = sharedIndices.GetCommonIndices(kvp.Key.selectedIndicesInternal);

							if (m_DragSelectMode == DragSelectMode.Add)
								common.UnionWith(kvp.Value);
							else if (m_DragSelectMode == DragSelectMode.Subtract)
								common.RemoveWhere(x => kvp.Value.Contains(x));
							else if (m_DragSelectMode == DragSelectMode.Difference)
								common.SymmetricExceptWith(kvp.Value);
						}
						else
						{
							common = kvp.Value;
						}

						kvp.Key.SetSelectedVertices(common.SelectMany(x => sharedIndices[x].array).ToArray());
					}

					UpdateSelection(false);
				}
					break;

				case SelectMode.Face:
				{
					if (!m_CurrentEvent.shift && !(m_CurrentEvent.command || m_CurrentEvent.control))
						ClearElementSelection();

					Dictionary<ProBuilderMesh, HashSet<Face>> selected = Picking.PickFacesInRect(
						SceneView.lastActiveSceneView.camera,
						m_MouseDragRect,
						selection,
						pickingOptions,
						EditorGUIUtility.pixelsPerPoint);

					foreach (var kvp in selected)
					{
						HashSet<Face> current;

						if (m_CurrentEvent.shift || (m_CurrentEvent.command || m_CurrentEvent.control))
						{
							current = new HashSet<Face>(kvp.Key.selectedFacesInternal);

							if (m_DragSelectMode == DragSelectMode.Add)
								current.UnionWith(kvp.Value);
							else if (m_DragSelectMode == DragSelectMode.Subtract)
								current.RemoveWhere(x => kvp.Value.Contains(x));
							else if (m_DragSelectMode == DragSelectMode.Difference)
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
					if (!m_CurrentEvent.shift && !(m_CurrentEvent.command || m_CurrentEvent.control))
						ClearElementSelection();

					var selected = Picking.PickEdgesInRect(
						SceneView.lastActiveSceneView.camera,
						m_MouseDragRect,
						selection,
						pickingOptions,
						EditorGUIUtility.pixelsPerPoint);

					foreach (var kvp in selected)
					{
						ProBuilderMesh pb = kvp.Key;
						Dictionary<int, int> commonIndices = pb.sharedIndicesInternal.ToDictionary();
						HashSet<EdgeLookup> selectedEdges = EdgeLookup.GetEdgeLookupHashSet(kvp.Value, commonIndices);

						HashSet<EdgeLookup> current;

						if (m_CurrentEvent.shift || (m_CurrentEvent.command || m_CurrentEvent.control))
						{
							current = EdgeLookup.GetEdgeLookupHashSet(pb.selectedEdges, commonIndices);

							if (m_DragSelectMode == DragSelectMode.Add)
								current.UnionWith(selectedEdges);
							else if (m_DragSelectMode == DragSelectMode.Subtract)
								current.RemoveWhere(x => selectedEdges.Contains(x));
							else if (m_DragSelectMode == DragSelectMode.Difference)
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
				if (!m_CurrentEvent.shift && m_SelectedVertexCount > 0) return;
			}
			else
			{
				if (!m_CurrentEvent.shift) MeshSelection.ClearElementAndObjectSelection();
			}

			// scan for new selected objects
			// if mode based, don't allow selection of non-probuilder objects
			foreach (ProBuilderMesh g in HandleUtility.PickRectObjects(m_MouseDragRect).GetComponents<ProBuilderMesh>())
				if (!Selection.Contains(g.gameObject))
					MeshSelection.AddToSelection(g.gameObject);
		}

		void VertexMoveTool()
		{
			m_ElementHandlePosition = m_HandlePivotWorld;
			m_ElementHandleCachedPosition = m_ElementHandlePosition;

			m_ElementHandlePosition = Handles.PositionHandle(m_ElementHandlePosition, handleRotation);

			if (m_CurrentEvent.alt)
				return;

			bool previouslyMoving = m_IsMovingElements;

			if (m_ElementHandlePosition != m_ElementHandleCachedPosition)
			{
				// profiler.BeginSample("VertexMoveTool()");
				Vector3 diff = m_ElementHandlePosition - m_ElementHandleCachedPosition;

				Vector3 mask = diff.ToMask(Math.handleEpsilon);

				if (m_DoSnapToVertex)
				{
					Vector3 v;

					if (FindNearestVertex(m_CurrentEvent.mousePosition, out v))
						diff = Vector3.Scale(v - m_ElementHandleCachedPosition, mask);
				}
				else if (m_DoSnapToFace)
				{
					ProBuilderMesh obj = null;
					RaycastHit hit;
					Dictionary<ProBuilderMesh, HashSet<Face>> ignore = new Dictionary<ProBuilderMesh, HashSet<Face>>();
					foreach (ProBuilderMesh pb in selection)
						ignore.Add(pb, new HashSet<Face>(pb.selectedFacesInternal));

					if (EditorHandleUtility.FaceRaycast(m_CurrentEvent.mousePosition, out obj, out hit, ignore))
					{
						if (mask.IntSum() == 1)
						{
							Ray r = new Ray(m_ElementHandleCachedPosition, -mask);
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
							diff = Vector3.Scale(obj.transform.TransformPoint(hit.point) - m_ElementHandleCachedPosition, mask.Abs());
						}
					}
				}
				// else if(snapToEdge && nearestEdge.IsValid())
				// {
				// 	// FINDME

				// }

				m_IsMovingElements = true;

				if (previouslyMoving == false)
				{
					m_TranslateOrigin = m_ElementHandleCachedPosition;
					m_RotateOrigin = m_HandleRotation.eulerAngles;
					m_ScaleOrigin = m_HandleScale;

					OnBeginVertexMovement();

					if (Event.current.modifiers == EventModifiers.Shift)
						ShiftExtrude();

					ProGridsInterface.OnHandleMove(mask);
				}

				for (int i = 0; i < selection.Length; i++)
				{
					selection[i].TranslateVerticesInWorldSpace(selection[i].selectedIndicesInternal, diff, m_SnapEnabled ? m_SnapValue : 0f,
						m_SnapAxisConstraint, m_SharedIndicesDictionary[i]);
					selection[i].RefreshUV(selectedFacesInEditZone[selection[i]]);
					selection[i].Refresh(RefreshMask.Normals);
					selection[i].mesh.RecalculateBounds();
				}

				Internal_UpdateSelectionFast();

				// profiler.EndSample();
			}
		}

		void VertexScaleTool()
		{
			m_ElementHandlePosition = m_HandlePivotWorld;

			m_HandleScalePrevious = m_HandleScale;

			m_HandleScale = Handles.ScaleHandle(m_HandleScale, m_ElementHandlePosition, handleRotation,
				HandleUtility.GetHandleSize(m_ElementHandlePosition));

			if (m_CurrentEvent.alt) return;

			bool previouslyMoving = m_IsMovingElements;

			if (m_HandleScalePrevious != m_HandleScale)
			{
				m_IsMovingElements = true;
				if (previouslyMoving == false)
				{
					m_TranslateOrigin = m_ElementHandleCachedPosition;
					m_RotateOrigin = m_HandleRotation.eulerAngles;
					m_ScaleOrigin = m_HandleScale;

					OnBeginVertexMovement();

					if (Event.current.modifiers == EventModifiers.Shift)
						ShiftExtrude();

					// cache vertex positions for scaling later
					m_VertexPositions = new Vector3[selection.Length][];
					m_VertexOffset = new Vector3[selection.Length];

					for (int i = 0; i < selection.Length; i++)
					{
						m_VertexPositions[i] = selection[i].positionsInternal.ValuesWithIndices(selection[i].selectedIndicesInternal);
						m_VertexOffset[i] = Math.Average(m_VertexPositions[i]);
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
					Vector3 nrm = Math.Normal(m_VertexPositions[i]);
					Quaternion localRot = Quaternion.LookRotation(nrm == Vector3.zero ? Vector3.forward : nrm, Vector3.up);

					Vector3[] v = selection[i].positionsInternal;
					IntArray[] sharedIndices = selection[i].sharedIndicesInternal;

					for (int n = 0; n < selection[i].selectedIndicesInternal.Length; n++)
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
								over = Quaternion.Inverse(localRot) * (m_VertexPositions[i][n] - m_VertexOffset[i]);

								// apply scale
								ver = Vector3.Scale(over, m_HandleScale);

								// re-apply original rotation
								if (m_VertexPositions[i].Length > 2)
									ver = localRot * ver;

								// re-apply world position offset
								ver += m_VertexOffset[i];

								int[] array = sharedIndices[m_SharedIndicesDictionary[i][selection[i].selectedIndicesInternal[n]]].array;

								for (int t = 0; t < array.Length; t++)
									v[array[t]] = ver;

								break;
							}

							case HandleAlignment.World:
							case HandleAlignment.Local:
							{
								// move vertex to relative origin from center of selection
								over = m_VertexPositions[i][n] - m_VertexOffset[i];
								// apply scale
								ver = Vector3.Scale(over, m_HandleScale);
								// move vertex back to locally offset position
								ver += m_VertexOffset[i];
								// set vertex in local space on pb-Object

								int[] array = sharedIndices[m_SharedIndicesDictionary[i][selection[i].selectedIndicesInternal[n]]].array;

								for (int t = 0; t < array.Length; t++)
									v[array[t]] = ver;

								break;
							}
						}
					}

					selection[i].mesh.vertices = v;
					selection[i].RefreshUV(selectedFacesInEditZone[selection[i]]);
					selection[i].Refresh(RefreshMask.Normals);
					selection[i].mesh.RecalculateBounds();
				}

				Internal_UpdateSelectionFast();
			}
		}

		void VertexRotateTool()
		{
			if (!m_IsMovingElements)
				m_ElementHandlePosition = m_HandlePivotWorld;

			m_HandleRotationPrevious = m_HandleRotation;

			if (m_CurrentEvent.alt)
				Handles.RotationHandle(m_HandleRotation, m_ElementHandlePosition);
			else
				m_HandleRotation = Handles.RotationHandle(m_HandleRotation, m_ElementHandlePosition);

			if (m_HandleRotation != m_HandleRotationPrevious)
			{
				// profiler.BeginSample("Rotate");
				if (!m_IsMovingElements)
				{
					m_IsMovingElements = true;

					m_TranslateOrigin = m_ElementHandleCachedPosition;
					m_RotateOrigin = m_HandleRotation.eulerAngles;
					m_ScaleOrigin = m_HandleScale;

					m_RotationInitial = m_HandleRotationPrevious;
					m_RotationInitialInverse = Quaternion.Inverse(m_HandleRotationPrevious);

					OnBeginVertexMovement();

					if (Event.current.modifiers == EventModifiers.Shift)
						ShiftExtrude();

					// cache vertex positions for modifying later
					m_VertexPositions = new Vector3[selection.Length][];
					m_VertexOffset = new Vector3[selection.Length];

					for (int i = 0; i < selection.Length; i++)
					{
						Vector3[] vertices = selection[i].positionsInternal;
						int[] triangles = selection[i].selectedIndicesInternal;
						m_VertexPositions[i] = new Vector3[triangles.Length];

						for (int nn = 0; nn < triangles.Length; nn++)
							m_VertexPositions[i][nn] = selection[i].transform.TransformPoint(vertices[triangles[nn]]);

						if (handleAlignment == HandleAlignment.World)
							m_VertexOffset[i] = m_ElementHandlePosition;
						else
							m_VertexOffset[i] = Math.GetBounds(m_VertexPositions[i]).center;
					}
				}

				// profiler.BeginSample("Calc Matrix");
				Quaternion transformedRotation = m_RotationInitialInverse * m_HandleRotation;

				// profiler.BeginSample("matrix mult");
				Vector3 ver; // resulting vertex from modification
				for (int i = 0; i < selection.Length; i++)
				{
					Vector3[] v = selection[i].positionsInternal;
					IntArray[] sharedIndices = selection[i].sharedIndicesInternal;

					Quaternion lr = m_RotationInitial; // selection[0].transform.localRotation;
					Quaternion ilr = m_RotationInitialInverse; // Quaternion.Inverse(lr);

					for (int n = 0; n < selection[i].selectedIndicesInternal.Length; n++)
					{
						// move vertex to relative origin from center of selection
						ver = ilr * (m_VertexPositions[i][n] - m_VertexOffset[i]);

						// rotate
						ver = transformedRotation * ver;

						// move vertex back to locally offset position
						ver = (lr * ver) + m_VertexOffset[i];

						int[] array = sharedIndices[m_SharedIndicesDictionary[i][selection[i].selectedIndicesInternal[n]]].array;

						for (int t = 0; t < array.Length; t++)
							v[array[t]] = selection[i].transform.InverseTransformPoint(ver);
					}

					selection[i].mesh.vertices = v;
					selection[i].RefreshUV(selectedFacesInEditZone[selection[i]]);
					selection[i].Refresh(RefreshMask.Normals);
					selection[i].mesh.RecalculateBounds();
				}
				// profiler.EndSample();

				// don't modify the handle rotation because otherwise rotating with plane coordinates
				// updates the handle rotation with every change, making moving things a changing target
				Quaternion rotateToolHandleRotation = m_HandleRotation;

				Internal_UpdateSelectionFast();

				m_HandleRotation = rotateToolHandleRotation;
				// profiler.EndSample();
			}
		}

		/// <summary>
		/// Extrude the current selection with no translation.
		/// </summary>
		void ShiftExtrude()
		{
			int ef = 0;
			foreach (ProBuilderMesh pb in selection)
			{
				// @todo - If caching normals, remove this 'ToMesh' and move
				Undo.RegisterCompleteObjectUndo(selection, "Extrude Vertices");

				switch (selectionMode)
				{
					case SelectMode.Edge:
						if (pb.selectedFaceCount > 0)
							goto default;

						Edge[] newEdges = pb.Extrude(pb.selectedEdges,
							0.0001f,
							PreferencesInternal.GetBool(PreferenceKeys.pbExtrudeAsGroup),
							PreferencesInternal.GetBool(PreferenceKeys.pbManifoldEdgeExtrusion));

						if (newEdges != null)
						{
							ef += newEdges.Length;
							pb.SetSelectedEdges(newEdges);
						}
						break;

					default:
						int len = pb.selectedFacesInternal.Length;

						if (len > 0)
						{
							pb.Extrude(pb.selectedFacesInternal, PreferencesInternal.GetEnum<ExtrudeMethod>(PreferenceKeys.pbExtrudeMethod),
								0.0001f);
							pb.SetSelectedFaces(pb.selectedFacesInternal);
							ef += len;
						}

						break;
				}

				pb.ToMesh();
				pb.Refresh();
			}

			if (ef > 0)
			{
				EditorUtility.ShowNotification("Extrude");
				UpdateSelection(true);
			}
		}

		void TextureMoveTool()
		{
			UVEditor uvEditor = UVEditor.instance;
			if (!uvEditor) return;

			Vector3 cached = m_TextureHandlePosition;

			m_TextureHandlePosition = Handles.PositionHandle(m_TextureHandlePosition, handleRotation);

			if (m_CurrentEvent.alt) return;

			if (m_TextureHandlePosition != cached)
			{
				cached = Quaternion.Inverse(handleRotation) * m_TextureHandlePosition;
				cached.y = -cached.y;

				Vector3 lossyScale = selection[0].transform.lossyScale;
				Vector3 position = cached.DivideBy(lossyScale);

				if (!m_IsMovingTextures)
				{
					m_TextureHandlePositionPrevious = position;
					m_IsMovingTextures = true;
				}

				uvEditor.SceneMoveTool(position - m_TextureHandlePositionPrevious);
				m_TextureHandlePositionPrevious = position;

				uvEditor.Repaint();
			}
		}

		void TextureRotateTool()
		{
			UVEditor uvEditor = UVEditor.instance;
			if (!uvEditor) return;

			float size = HandleUtility.GetHandleSize(m_HandlePivotWorld);

			if (m_CurrentEvent.alt) return;

			Matrix4x4 prev = Handles.matrix;
			Handles.matrix = handleMatrix;

			Quaternion cached = m_TextureRotation;

			m_TextureRotation = Handles.Disc(m_TextureRotation, Vector3.zero, Vector3.forward, size, false, 0f);

			if (m_TextureRotation != cached)
			{
				if (!m_IsMovingTextures)
					m_IsMovingTextures = true;

				uvEditor.SceneRotateTool(-m_TextureRotation.eulerAngles.z);
			}

			Handles.matrix = prev;
		}

		void TextureScaleTool()
		{
			UVEditor uvEditor = UVEditor.instance;
			if (!uvEditor) return;

			float size = HandleUtility.GetHandleSize(m_HandlePivotWorld);

			Matrix4x4 prev = Handles.matrix;
			Handles.matrix = handleMatrix;

			Vector3 cached = m_TextureScale;
			m_TextureScale = Handles.ScaleHandle(m_TextureScale, Vector3.zero, Quaternion.identity, size);

			if (m_CurrentEvent.alt) return;

			if (cached != m_TextureScale)
			{
				if (!m_IsMovingTextures)
					m_IsMovingTextures = true;

				uvEditor.SceneScaleTool(m_TextureScale, cached);
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
				if (m_NearestEdge.item1 != null && m_NearestEdge.item2.IsValid())
				{
					if (EditorHandleUtility.BeginDrawingLines(Handles.zTest))
					{
						MeshHandles.lineMaterial.SetColor("_Color", Color.white);
						GL.Color(MeshHandles.preselectionColor);

						GL.MultMatrix(m_NearestEdge.item1.transform.localToWorldMatrix);

						GL.Vertex(m_NearestEdge.item1.positionsInternal[m_NearestEdge.item2.x]);
						GL.Vertex(m_NearestEdge.item1.positionsInternal[m_NearestEdge.item2.y]);

						EditorHandleUtility.EndDrawingLines();
					}
				}
			}

			using (new HandleGUI())
			{
				int screenWidth = (int) sceneView.position.width;
				int screenHeight = (int) sceneView.position.height;

				int currentSelectionMode =
					(editLevel != EditLevel.Top && editLevel != EditLevel.Plugin) ? ((int) selectionMode) + 1 : 0;

				switch (m_SceneToolbarLocation)
				{
					case SceneToolbarLocation.BottomCenter:
						m_ElementModeToolbarRect.x = (screenWidth / 2 - 64);
						m_ElementModeToolbarRect.y = screenHeight - m_ElementModeToolbarRect.height * 3;
						break;

					case SceneToolbarLocation.BottomLeft:
						m_ElementModeToolbarRect.x = 12;
						m_ElementModeToolbarRect.y = screenHeight - m_ElementModeToolbarRect.height * 3;
						break;

					case SceneToolbarLocation.BottomRight:
						m_ElementModeToolbarRect.x = screenWidth - (m_ElementModeToolbarRect.width + 12);
						m_ElementModeToolbarRect.y = screenHeight - m_ElementModeToolbarRect.height * 3;
						break;

					case SceneToolbarLocation.UpperLeft:
						m_ElementModeToolbarRect.x = 12;
						m_ElementModeToolbarRect.y = 10;
						break;

					case SceneToolbarLocation.UpperRight:
						m_ElementModeToolbarRect.x = screenWidth - (m_ElementModeToolbarRect.width + 96);
						m_ElementModeToolbarRect.y = 10;
						break;

					default:
					case SceneToolbarLocation.UpperCenter:
						m_ElementModeToolbarRect.x = (screenWidth / 2 - 64);
						m_ElementModeToolbarRect.y = 10;
						break;
				}

				EditorGUI.BeginChangeCheck();

				currentSelectionMode =
					GUI.Toolbar(m_ElementModeToolbarRect, (int) currentSelectionMode, m_EditModeIcons, m_CommandStyle);

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

				if (m_IsMovingElements && m_ShowSceneInfo)
				{
					string handleTransformInfo = string.Format(
						"translate: <b>{0}</b>\nrotate: <b>{1}</b>\nscale: <b>{2}</b>",
						(m_ElementHandlePosition - m_TranslateOrigin).ToString(),
						(m_HandleRotation.eulerAngles - m_RotateOrigin).ToString(),
						(m_HandleScale - m_ScaleOrigin).ToString());

					var gc = UI.EditorGUIUtility.TempGUIContent(handleTransformInfo);
					// sceneview screen.height includes the tab and toolbar
					var toolbarHeight = EditorStyles.toolbar.CalcHeight(gc, Screen.width);
					var size = UI.EditorStyles.sceneTextBox.CalcSize(gc);

					Rect handleTransformInfoRect = new Rect(
						sceneView.position.width - (size.x + 8), sceneView.position.height - (size.y + 8 + toolbarHeight),
						size.x,
						size.y);

					GUI.Label(handleTransformInfoRect, gc, UI.EditorStyles.sceneTextBox);
				}

				if (m_ShowSceneInfo)
				{
					Vector2 size = UI.EditorStyles.sceneTextBox.CalcSize(m_SceneInfo);
					m_SceneInfoRect.width = size.x;
					m_SceneInfoRect.height = size.y;
					GUI.Label(m_SceneInfoRect, m_SceneInfo, UI.EditorStyles.sceneTextBox);
				}

				// Enables vertex selection with a mouse click
				if (editLevel == EditLevel.Geometry && !m_IsDragging && selectionMode == SelectMode.Vertex)
					m_MouseClickRect = new Rect(m_CurrentEvent.mousePosition.x - 10, m_CurrentEvent.mousePosition.y - 10, 20, 20);
				else
					m_MouseClickRect = PreferenceKeys.RectZero;

				if (m_IsDragging)
				{
					if (m_CurrentEvent.type == EventType.Repaint)
					{
						// Always draw from lowest to largest values
						var start = Vector2.Min(m_InitialMousePosition, m_CurrentEvent.mousePosition);
						var end = Vector2.Max(m_InitialMousePosition, m_CurrentEvent.mousePosition);

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
			List<Shortcut> matches = m_Shortcuts.Where(x => x.Matches(e.keyCode, e.modifiers)).ToList();

			if (matches.Count < 1)
				return false;

			bool used = false;
			Shortcut usedShortcut = null;

			foreach (Shortcut cut in matches)
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
				foreach (Shortcut cut in matches)
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
					EditorUtility.ShowNotification(usedShortcut.action);

				Event.current.Use();
			}

			return used;
		}

		bool AllLevelShortcuts(Shortcut shortcut)
		{
			bool uniqueModeShortcuts = PreferencesInternal.GetBool(PreferenceKeys.pbUniqueModeShortcuts);

			switch (shortcut.action)
			{
				// TODO Remove once a workaround for non-upper-case shortcut chars is found
				case "Toggle Geometry Mode":

					if (editLevel == EditLevel.Geometry)
					{
						EditorUtility.ShowNotification("Top Level Editing");
						SetEditLevel(EditLevel.Top);
					}
					else if (!uniqueModeShortcuts)
					{
						EditorUtility.ShowNotification("Geometry Editing");
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

		bool GeoLevelShortcuts(Shortcut shortcut)
		{
			switch (shortcut.action)
			{
				case "Escape":
					ClearElementSelection();
					EditorUtility.ShowNotification("Top Level");
					UpdateSelection(false);
					SetEditLevel(EditLevel.Top);
					return true;

				// TODO Remove once a workaround for non-upper-case shortcut chars is found
				case "Toggle Selection Mode":

					if (PreferencesInternal.GetBool(PreferenceKeys.pbUniqueModeShortcuts))
						return false;

					ToggleSelectionMode();
					switch (selectionMode)
					{
						case SelectMode.Face:
							EditorUtility.ShowNotification("Editing Faces");
							break;

						case SelectMode.Vertex:
							EditorUtility.ShowNotification("Editing Vertices");
							break;

						case SelectMode.Edge:
							EditorUtility.ShowNotification("Editing Edges");
							break;
					}

					return true;

				case "Delete Face":
					EditorUtility.ShowNotification(MenuCommands.MenuDeleteFace(selection).notification);
					return true;

				/* handle alignment */
				case "Toggle Handle Pivot":
					if (m_SelectedVertexCount < 1)
						return false;

					if (editLevel != EditLevel.Texture)
					{
						ToggleHandleAlignment();
						EditorUtility.ShowNotification("Handle Alignment: " + ((HandleAlignment) handleAlignment).ToString());
					}

					return true;

				case "Set Pivot":

					if (selection.Length > 0)
					{
						foreach (ProBuilderMesh pbo in selection)
						{
							UndoUtility.RecordObjects(new Object[2] { pbo, pbo.transform }, "Set Pivot");

							if (pbo.selectedIndicesInternal.Length > 0)
							{
								pbo.CenterPivot(pbo.selectedIndicesInternal);
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
			m_CurrentTool = newTool;
		}

		/// <summary>
		/// Calls SetTool(), then Updates the UV Editor window if applicable.
		/// </summary>
		/// <param name="newTool"></param>
		void SetTool_Internal(Tool newTool)
		{
			SetTool(newTool);

			if (UVEditor.instance != null)
				UVEditor.instance.SetTool(newTool);
		}

		internal void SetHandleAlignment(HandleAlignment ha)
		{
			if (editLevel == EditLevel.Texture)
				ha = HandleAlignment.Plane;
			else
				PreferencesInternal.SetInt(PreferenceKeys.pbHandleAlignment, (int) ha);

			handleAlignment = ha;

			UpdateHandleRotation();

			m_HandleRotation = handleRotation;

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
		/// Sets what mesh attributes are editable in the scene.
		/// </summary>
		/// <seealso cref="UnityEngine.ProBuilder.SelectMode"/>
		/// <param name="mode">The @"UnityEngine.ProBuilder.SelectMode" to engage.</param>
		public void SetSelectionMode(SelectMode mode)
		{
			selectionMode = mode;

			Internal_UpdateSelectionFast();

			PreferencesInternal.SetInt(PreferenceKeys.pbDefaultSelectionMode, (int) selectionMode);

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
		/// Set the @"UnityEngine.ProBuilder.EditLevel".
		/// </summary>
		/// <param name="editMode">The new EditLevel to engage.</param>
		public void SetEditLevel(EditLevel editMode)
		{
			m_PreviousEditLevel = editLevel;
			editLevel = editMode;

			switch (editMode)
			{
				case EditLevel.Top:
					ClearElementSelection();
					UpdateSelection(true);

					MeshSelection.SetSelection(Selection.gameObjects);
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


			if (m_PreviousEditLevel == EditLevel.Texture && editMode != EditLevel.Texture)
			{
				SetSelectionMode(m_PreviousSelectMode);
				SetHandleAlignment(m_PreviousHandleAlignment);
			}

			if (editLevel != EditLevel.Texture)
				PreferencesInternal.SetInt(PreferenceKeys.pbDefaultEditLevel, (int) editLevel);

			if (onEditLevelChanged != null)
				onEditLevelChanged((int) editLevel);
		}

		/// <summary>
		/// Rebuild the wireframe selection caches.
		/// </summary>
		/// <param name="forceUpdate">Force update if mesh attributes have been added or removed, or the face indices have been altered.</param>
		void UpdateSelection(bool forceUpdate = true)
		{
			m_SelectedVertexCount = 0;
			m_SelectedFaceCount = 0;
			m_SelectedEdgeCount = 0;
			m_SelectedVerticesCommon = 0;
			ProBuilderMesh[] t_selection = selection;
			selection = InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms);

			if (selectedFacesInEditZone != null)
				selectedFacesInEditZone.Clear();
			else
				selectedFacesInEditZone = new Dictionary<ProBuilderMesh, List<Face>>();

			bool selectionEqual = t_selection.SequenceEqual(selection);

			// If the top level selection has changed, update all the heavy cache things
			// that don't change based on element selction
			if (forceUpdate || !selectionEqual)
			{

				// If updating due to inequal selections, set the forceUpdate to true so some of the functions below
				// know that these values can be trusted.
				forceUpdate = true;

				m_UniversalEdges = new Edge[selection.Length][];
				m_SharedIndicesDictionary = new Dictionary<int, int>[selection.Length];
				for (int i = 0; i < selection.Length; i++)
				{
					m_SharedIndicesDictionary[i] = selection[i].sharedIndicesInternal.ToDictionary();
					m_UniversalEdges[i] = EdgeExtension.GetUniversalEdges(EdgeExtension.AllEdges(selection[i].facesInternal), m_SharedIndicesDictionary[i]);
				}
			}

			m_HandlePivotWorld = Vector3.zero;

			Vector3 min = Vector3.zero, max = Vector3.zero;
			var boundsInitialized = false;
			HashSet<int> used = new HashSet<int>();

			for (var i = 0; i < selection.Length; i++)
			{
				var lookup = m_SharedIndicesDictionary[i];
				used.Clear();

				ProBuilderMesh pb = selection[i];

				if (!boundsInitialized && pb.selectedVertexCount > 0)
				{
					boundsInitialized = true;
					min = pb.transform.TransformPoint(pb.positionsInternal[pb.selectedIndicesInternal[0]]);
					max = min;
				}

				if (pb.selectedVertexCount > 0)
				{
					var indices = pb.selectedIndicesInternal;

					for (int n = 0, c = pb.selectedVertexCount; n < c; n++)
					{
						if (used.Add(lookup[indices[n]]))
						{
							Vector3 v = pb.transform.TransformPoint(pb.positionsInternal[indices[n]]);
							min = Vector3.Min(min, v);
							max = Vector3.Max(max, v);
						}
					}

					m_SelectedVerticesCommon += used.Count;
				}

				selectedFacesInEditZone.Add(pb, ElementSelection.GetNeighborFaces(pb, pb.selectedIndicesInternal, m_SharedIndicesDictionary[i]));

				m_SelectedVertexCount += selection[i].selectedIndicesInternal.Length;
				m_SelectedFaceCount += selection[i].selectedFaceCount;
				m_SelectedEdgeCount += selection[i].selectedEdgeCount;
			}

			m_HandlePivotWorld = (max + min) * .5f;
			MeshHandles.RebuildGraphics(selection, m_SharedIndicesDictionary, editLevel, selectionMode);
			UpdateHandleRotation();
			UpdateTextureHandles();
			m_HandleRotation = handleRotation;

			if (onSelectionUpdate != null)
				onSelectionUpdate(selection);

			UpdateSceneInfo();
		}

		void UpdateSceneInfo()
		{
			m_SceneInfo.text = string.Format(
				"Faces: <b>{0}</b>\nTriangles: <b>{1}</b>\nVertices: <b>{2} ({3})</b>\n\nSelected Faces: <b>{4}</b>\nSelected Edges: <b>{5}</b>\nSelected Vertices: <b>{6} ({7})</b>",
				MeshSelection.totalFaceCount,
				MeshSelection.totalTriangleCountCompiled,
				MeshSelection.totalCommonVertexCount,
				MeshSelection.totalVertexCountOptimized,
				m_SelectedFaceCount,
				m_SelectedEdgeCount,
				m_SelectedVerticesCommon,
				m_SelectedVertexCount);
		}

		// Only updates things that absolutely need to be refreshed, and assumes that no selection changes have occured
		internal void Internal_UpdateSelectionFast()
		{
			m_SelectedVertexCount = 0;
			m_SelectedFaceCount = 0;
			m_SelectedEdgeCount = 0;

			bool boundsInitialized = false;
			Vector3 min = Vector3.zero, max = Vector3.zero;

			for (int i = 0; i < selection.Length; i++)
			{
				ProBuilderMesh pb = selection[i];
				Vector3[] vertices = pb.positionsInternal;
				int[] indices = pb.selectedIndicesInternal;

				if (pb == null) continue;

				if (selection[i].selectedVertexCount > 0)
				{
					if (!boundsInitialized)
					{
						boundsInitialized = true;
						min = pb.transform.TransformPoint(vertices[indices[0]]);
						max = min;
					}

					for (int n = 0; n < selection[i].selectedVertexCount; n++)
					{
						min = Vector3.Min(min, pb.transform.TransformPoint(vertices[indices[n]]));
						max = Vector3.Max(max, pb.transform.TransformPoint(vertices[indices[n]]));
					}
				}

				m_SelectedVertexCount += selection[i].selectedVertexCount;
				m_SelectedFaceCount += selection[i].selectedFaceCount;
				m_SelectedEdgeCount += selection[i].selectedEdgeCount;
			}

			m_HandlePivotWorld = (max + min) / 2f;

			MeshHandles.RebuildGraphics(selection, m_SharedIndicesDictionary, editLevel, selectionMode);

			UpdateHandleRotation();
			m_HandleRotation = handleRotation;

			if (onSelectionUpdate != null)
				onSelectionUpdate(selection);

			UpdateSceneInfo();
		}

		internal void ClearElementSelection()
		{
			foreach (ProBuilderMesh pb in selection)
				pb.ClearSelection();

			m_NearestEdge.item2 = Edge.Empty;
			m_NearestEdge.item1 = null;
		}

		void UpdateTextureHandles()
		{
			if (selection.Length < 1) return;

			// Reset temp vars
			m_TextureHandlePosition = m_HandlePivotWorld;
			m_TextureScale = Vector3.one;
			m_TextureRotation = Quaternion.identity;

			ProBuilderMesh pb;
			Face face;

			handleMatrix = selection[0].transform.localToWorldMatrix;

			if (GetFirstSelectedFace(out pb, out face))
			{
				var normals = Math.NormalTangentBitangent(pb, face);
				var nrm = normals.normal;
				var bitan = normals.bitangent;

				if (nrm == Vector3.zero || bitan == Vector3.zero)
				{
					nrm = Vector3.up;
					bitan = Vector3.right;
				}

				handleMatrix *= Matrix4x4.TRS(Math.GetBounds(pb.positionsInternal.ValuesWithIndices(face.distinctIndices)).center,
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

					ProBuilderMesh pb;
					Face face;

					if (!GetFirstSelectedFace(out pb, out face))
						goto case HandleAlignment.Local;

					// use average normal, tangent, and bitangent to calculate rotation relative to local space
					var tup = Math.NormalTangentBitangent(pb, face);
					Vector3 nrm = tup.normal, bitan = tup.bitangent;

					if (nrm == Vector3.zero || bitan == Vector3.zero)
					{
						nrm = Vector3.up;
						bitan = Vector3.right;
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
					(Transform[]) InternalUtility.GetComponents<Transform>(
						HandleUtility.PickRectObjects(new Rect(0, 0, Screen.width, Screen.height))));

			GameObject nearest = HandleUtility.PickGameObject(mousePosition, false);

			if (nearest != null)
				t.Add(nearest.transform);

			object[] parameters = new object[] { (Vector2) mousePosition, t.ToArray(), null };

			if (m_FindNearestVertex == null)
				m_FindNearestVertex = typeof(HandleUtility).GetMethod("findNearestVertex",
					BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);

			object result = m_FindNearestVertex.Invoke(this, parameters);
			vertex = (bool) result ? (Vector3) parameters[2] : Vector3.zero;
			return (bool) result;
		}

		/// <summary>
		/// If dragging a texture aroudn, this method ensures that if it's a member of a texture group it's cronies are also selected
		/// </summary>
		void VerifyTextureGroupSelection()
		{
			foreach (ProBuilderMesh pb in selection)
			{
				List<int> alreadyChecked = new List<int>();

				foreach (Face f in pb.selectedFacesInternal)
				{
					int tg = f.textureGroup;
					if (tg > 0 && !alreadyChecked.Contains(f.textureGroup))
					{
						foreach (Face j in pb.facesInternal)
							if (j != f && j.textureGroup == tg && !pb.selectedFacesInternal.Contains(j))
							{
								// int i = EditorUtility.DisplayDialogComplex("Mixed Texture Group Selection", "One or more of the faces selected belong to a Texture Group that does not have all it's member faces selected.  To modify, please either add the remaining group faces to the selection, or remove the current face from this smoothing group.", "Add Group to Selection", "Cancel", "Remove From Group");
								int i = 0;
								switch (i)
								{
									case 0:
										List<Face> newFaceSection = new List<Face>();
										foreach (Face jf in pb.facesInternal)
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
			m_NearestEdge.item2 = Edge.Empty;
			m_NearestEdge.item1 = null;

			UpdateSelection(false);
			HideSelectedWireframe();
		}

		/// <summary>
		/// Hide the default unity wireframe renderer
		/// </summary>
		void HideSelectedWireframe()
		{
			foreach (ProBuilderMesh pb in selection)
				EditorUtility.SetSelectionRenderState(pb.gameObject.GetComponent<Renderer>(),
					EditorUtility.GetSelectionRenderState() & SelectionRenderState.Outline);

			SceneView.RepaintAll();
		}

		/// <summary>
		/// Called from ProGrids.
		/// </summary>
		/// <param name="snapVal"></param>
		void PushToGrid(float snapVal)
		{
			UndoUtility.RecordSelection(selection, "Push elements to Grid");

			if (editLevel == EditLevel.Top)
				return;

			for (int i = 0; i < selection.Length; i++)
			{
				ProBuilderMesh pb = selection[i];

				int[] indices = pb.selectedVertexCount > 0
					? pb.sharedIndicesInternal.AllIndexesWithValues(pb.selectedIndicesInternal).ToArray()
					: pb.mesh.triangles;

				Snapping.SnapVertices(pb, indices, Vector3.one * snapVal);

				pb.ToMesh();
				pb.Refresh();
				pb.Optimize();
			}

			Internal_UpdateSelectionFast();
		}

		void ProGridsToolbarOpen(bool menuOpen)
		{
			bool active = ProGridsInterface.ProGridsActive();
			m_SceneInfoRect.y = active && !menuOpen ? 28 : 10;
			m_SceneInfoRect.x = active ? (menuOpen ? 64 : 8) : 10;
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
			switch (m_CurrentTool)
			{
				case Tool.Move:
					UndoUtility.RegisterCompleteObjectUndo(selection, "Translate Vertices");
					break;

				case Tool.Rotate:
					UndoUtility.RegisterCompleteObjectUndo(selection, "Rotate Vertices");
					break;

				case Tool.Scale:
					UndoUtility.RegisterCompleteObjectUndo(selection, "Scale Vertices");
					break;

				default:
					UndoUtility.RegisterCompleteObjectUndo(selection, "Modify Vertices");
					break;
			}

			m_SnapEnabled = ProGridsInterface.SnapEnabled();
			m_SnapValue = ProGridsInterface.SnapValue();
			m_SnapAxisConstraint = ProGridsInterface.UseAxisConstraints();

			// Disable iterative lightmapping
			Lightmapping.PushGIWorkflowMode();

			foreach (ProBuilderMesh pb in selection)
			{
				pb.ToMesh();
				pb.Refresh();
			}

			if (onVertexMovementBegin != null)
				onVertexMovementBegin(selection);
		}

		void OnFinishVertexModification()
		{
			Lightmapping.PopGIWorkflowMode();

			m_HandleScale = Vector3.one;
			m_HandleRotation = handleRotation;

			if (m_IsMovingTextures)
			{
				if (UVEditor.instance != null)
					UVEditor.instance.OnFinishUVModification();

				UpdateTextureHandles();

				m_IsMovingTextures = false;
			}
			else if (m_IsMovingElements)
			{
				foreach (ProBuilderMesh sel in selection)
				{
					sel.ToMesh();
					sel.Refresh();
					sel.Optimize();
				}

				m_IsMovingElements = false;
			}

			if (onVertexMovementFinish != null)
				onVertexMovementFinish(selection);
		}

		/// <summary>
		/// Returns the first selected pb_Object and pb_Face, or false if not found.
		/// </summary>
		/// <param name="pb"></param>
		/// <param name="face"></param>
		/// <returns></returns>
		internal bool GetFirstSelectedFace(out ProBuilderMesh pb, out Face face)
		{
			pb = null;
			face = null;

			if (selection.Length < 1) return false;

			pb = selection.FirstOrDefault(x => x.selectedFaceCount > 0);

			if (pb == null)
				return false;

			face = pb.selectedFacesInternal[0];

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
				for (int n = 0; n < selection[i].selectedFaceCount; n++)
				{
					mat = selection[i].selectedFacesInternal[i].material;

					if (mat != null)
						return true;
				}
			}

			return false;
		}
	}
}

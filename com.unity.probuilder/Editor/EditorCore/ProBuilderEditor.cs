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
using UnityEngine.SceneManagement;
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
		public static event Action<ProBuilderMesh[]> selectionUpdated;

        /// <value>
        /// Called when vertex modifications are complete.
        /// </value>
        public static event Action<ProBuilderMesh[]> afterMeshModification;

        /// <value>
        /// Called immediately prior to beginning vertex modifications. The ProBuilderMesh will be in un-altered state at this point (meaning ProBuilderMesh.ToMesh and ProBuilderMesh.Refresh have been called, but not Optimize).
        /// </value>
        public static event Action<ProBuilderMesh[]> beforeMeshModification;

		/// <value>
		/// Raised when the EditLevel is changed.
		/// </value>
        public static event Action<SelectMode> selectModeChanged;

        // Toggles for Face, Vertex, and Edge mode.
        const int k_SelectModeLength = 3;

		static EditorToolbar s_EditorToolbar;
		static ProBuilderEditor s_Instance;
		EditorMeshHandles m_EditorMeshHandles;

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
		ComponentMode m_PreviousComponentMode;
		HandleAlignment m_PreviousHandleAlignment;
		Shortcut[] m_Shortcuts;
		SceneToolbarLocation m_SceneToolbarLocation;
		GUIStyle m_CommandStyle;
		Rect m_ElementModeToolbarRect = new Rect(3, 6, 128, 24);

		SceneSelection m_Hovering = new SceneSelection();
		SceneSelection m_HoveringPrevious = new SceneSelection();
		ScenePickerPreferences m_ScenePickerPreferences;
		bool m_ShowPreselectionHighlight;

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

		Vector3 m_HandlePivotWorld = Vector3.zero;

		/// <summary>
		/// Faces that need to be refreshed when moving or modifying the actual selection
		/// </summary>
		internal Dictionary<ProBuilderMesh, List<Face>> selectedFacesInEditZone { get; private set; }

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
		internal bool selectHiddenEnabled { get { return m_ScenePickerPreferences.cullMode == CullingMode.None; } }

		/// <value>
		/// Get the current @"UnityEngine.ProBuilder.EditLevel".
		/// </value>
		internal static EditLevel editLevel { get; private set; }

		/// <summary>
		/// Get the current @"UnityEngine.ProBuilder.SelectMode".
		/// </summary>
		/// <value>The SelectMode currently set.</value>
		internal static ComponentMode componentMode { get; private set; }

		/// <value>
		/// Get and set the current SelectMode.
		/// </value>
		public static SelectMode selectMode
		{
			get
			{
				if (s_Instance != null)
					return EditorUtility.GetSelectMode(editLevel, componentMode);

				// for backwards compatibility reasons `Object` is returned when editor is closed
				return SelectMode.Object;
			}

			set
			{
				if (s_Instance == null)
					return;

				switch (value)
				{
					case SelectMode.None:
						s_Instance.SetEditLevel(EditLevel.Plugin);
						break;
					case SelectMode.Object:
						s_Instance.SetEditLevel(EditLevel.Top);
						break;
					case SelectMode.Vertex:
						s_Instance.SetEditLevel(EditLevel.Geometry);
						s_Instance.SetSelectionMode(ComponentMode.Vertex);
						break;
					case SelectMode.Edge:
						s_Instance.SetEditLevel(EditLevel.Geometry);
						s_Instance.SetSelectionMode(ComponentMode.Edge);
						break;
					case SelectMode.Face:
						s_Instance.SetEditLevel(EditLevel.Geometry);
						s_Instance.SetSelectionMode(ComponentMode.Face);
						break;
					case SelectMode.Texture:
						s_Instance.SetEditLevel(EditLevel.Texture);
						break;
				}
			}
		}

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

		void OnBecameVisible()
		{
			// fixes maximizing/unmaximizing
			s_Instance = this;
		}

		internal void OnEnable()
		{
			s_Instance = this;

			if (m_EditorMeshHandles != null)
				m_EditorMeshHandles.Dispose();

			m_EditorMeshHandles = new EditorMeshHandles();

			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			SceneView.onSceneGUIDelegate += this.OnSceneGUI;

			ProGridsInterface.SubscribePushToGridEvent(PushToGrid);
			ProGridsInterface.SubscribeToolbarEvent(ProGridsToolbarOpen);

			ProGridsToolbarOpen(ProGridsInterface.SceneToolbarIsExtended());

			MeshSelection.objectSelectionChanged += OnObjectSelectionChanged;

#if !UNITY_2018_2_OR_NEWER
			s_ResetOnSceneGUIState = typeof(SceneView).GetMethod("ResetOnSceneGUIState", BindingFlags.Instance | BindingFlags.NonPublic);
#endif

			// make sure load prefs is called first, because other methods depend on the preferences set here
			LoadPrefs();
			InitGUI();
			UpdateSelection();
			HideSelectedWireframe();

			m_FindNearestVertex = typeof(HandleUtility).GetMethod("FindNearestVertex",
				BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Instance);

			if (selectModeChanged != null)
				selectModeChanged(EditorUtility.GetSelectMode(editLevel, componentMode));
		}

		void OnDisable()
		{
			s_Instance = null;

			if (s_EditorToolbar != null)
				DestroyImmediate(s_EditorToolbar);

			ClearElementSelection();

			UpdateSelection();

			m_EditorMeshHandles.Dispose();

			if (selectionUpdated != null)
				selectionUpdated(null);

			ProGridsInterface.UnsubscribePushToGridEvent(PushToGrid);
			SceneView.onSceneGUIDelegate -= this.OnSceneGUI;
			PreferencesInternal.SetInt(PreferenceKeys.pbHandleAlignment, (int) handleAlignment);
			MeshSelection.objectSelectionChanged -= OnObjectSelectionChanged;

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
			componentMode = PreferencesInternal.GetEnum<ComponentMode>(PreferenceKeys.pbDefaultSelectionMode);
			handleAlignment = PreferencesInternal.GetEnum<HandleAlignment>(PreferenceKeys.pbHandleAlignment);
			m_ShowSceneInfo = PreferencesInternal.GetBool(PreferenceKeys.pbShowSceneInfo);
			m_ShowPreselectionHighlight = PreferencesInternal.GetBool(PreferenceKeys.pbShowPreselectionHighlight);

			// ---
			m_HamSelection = PreferencesInternal.GetBool(PreferenceKeys.pbElementSelectIsHamFisted);
			bool selectHiddenFaces = PreferencesInternal.GetBool(PreferenceKeys.pbEnableBackfaceSelection);
			SelectionModifierBehavior selectModifierBehavior = PreferencesInternal.GetEnum<SelectionModifierBehavior>(PreferenceKeys.pbDragSelectMode);

			m_ScenePickerPreferences = new ScenePickerPreferences()
			{
				maxPointerDistance = m_HamSelection ? ScenePickerPreferences.maxPointerDistanceFuzzy : ScenePickerPreferences.maxPointerDistancePrecise,
				cullMode = selectHiddenFaces ? CullingMode.None : CullingMode.Back,
				selectionModifierBehavior = selectModifierBehavior,
				rectSelectMode = PreferencesInternal.GetEnum<RectSelectMode>(PreferenceKeys.pbRectSelectMode)
			};
			// ---

			m_SnapEnabled = ProGridsInterface.SnapEnabled();
			m_SnapValue = ProGridsInterface.SnapValue();
			m_SnapAxisConstraint = ProGridsInterface.UseAxisConstraints();

			m_Shortcuts = Shortcut.ParseShortcuts(PreferencesInternal.GetString(PreferenceKeys.pbDefaultShortcuts)).ToArray();

			m_SceneToolbarLocation = PreferencesInternal.GetEnum<SceneToolbarLocation>(PreferenceKeys.pbToolbarLocation);
			m_IsIconGui = PreferencesInternal.GetBool(PreferenceKeys.pbIconGUI);
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
		public static void Refresh()
		{
			if (instance != null)
				instance.UpdateSelection();
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

			m_EditorMeshHandles.DrawSceneHandles(selectMode);

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
			if (m_ShowPreselectionHighlight
				&& m_CurrentEvent.type == EventType.MouseMove
				&& editLevel == EditLevel.Geometry)
			{
				m_Hovering.CopyTo(m_HoveringPrevious);
				EditorSceneViewPicker.MouseRayHitTest(m_CurrentEvent.mousePosition, componentMode, m_ScenePickerPreferences, m_Hovering);
				if (!m_Hovering.Equals(m_HoveringPrevious))
					SceneView.RepaintAll();
			}

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
					EditorSceneViewPicker.DoMouseDrag(m_MouseDragRect, componentMode, m_ScenePickerPreferences);
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

						EditorSceneViewPicker.DoMouseClick(m_CurrentEvent, componentMode, m_ScenePickerPreferences);
						UpdateSelection();
						SceneView.RepaintAll();
					}
					else
					{
						m_IsDragging = false;
						m_IsReadyForMouseDrag = false;

						if (UVEditor.instance)
							UVEditor.instance.ResetUserPivot();

						EditorSceneViewPicker.DoMouseDrag(m_MouseDragRect, componentMode, m_ScenePickerPreferences);
					}
				}
			}
		}

		void DoubleClick(Event e)
		{
			var mesh = EditorSceneViewPicker.DoMouseClick(m_CurrentEvent, componentMode, m_ScenePickerPreferences);

			if (mesh != null)
			{
				if (componentMode == ComponentMode.Edge)
				{
					if (e.shift)
						MenuCommands.MenuRingSelection(selection);
					else
						MenuCommands.MenuLoopSelection(selection);
				}
				else if (componentMode == ComponentMode.Face)
				{
					if ((e.modifiers & (EventModifiers.Control | EventModifiers.Shift)) ==
					    (EventModifiers.Control | EventModifiers.Shift))
						MenuCommands.MenuRingAndLoopFaces(selection);
					else if (e.control)
						MenuCommands.MenuRingFaces(selection);
					else if (e.shift)
						MenuCommands.MenuLoopFaces(selection);
					else
						mesh.SetSelectedFaces(mesh.facesInternal);
				}
				else
				{
					mesh.SetSelectedFaces(mesh.facesInternal);
				}

				UpdateSelection();
				SceneView.RepaintAll();
				m_WasDoubleClick = true;
			}
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
					var mesh = selection[i];

					mesh.TranslateVerticesInWorldSpace(mesh.selectedIndexesInternal,
						diff,
						m_SnapEnabled ? m_SnapValue : 0f,
						m_SnapAxisConstraint);

					mesh.RefreshUV(selectedFacesInEditZone[mesh]);
					mesh.Refresh(RefreshMask.Normals);
					mesh.mesh.RecalculateBounds();
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
						m_VertexPositions[i] = selection[i].positionsInternal.ValuesWithIndexes(selection[i].selectedIndexesInternal);
						m_VertexOffset[i] = Math.Average(m_VertexPositions[i]);
					}
				}

				Vector3 ver; // resulting vertex from modification
				Vector3 over; // vertex point to modify. different for world, local, and plane

				bool gotoWorld = Selection.transforms.Length > 1 && handleAlignment == HandleAlignment.Plane;
				bool gotoLocal = m_SelectedFaceCount < 1;

				// if(pref_snapEnabled)
				// 	pbUndo.RecordSelection(selection as Object[], "Move vertices");

				for (int i = 0; i < selection.Length; i++)
				{
					// get the plane rotation in local space
					var mesh = selection[i];
					Vector3 nrm = Math.Normal(m_VertexPositions[i]);
					Quaternion localRot = Quaternion.LookRotation(nrm == Vector3.zero ? Vector3.forward : nrm, Vector3.up);

					Vector3[] v = mesh.positionsInternal;
					List<int> coincident = new List<int>();

					for (int n = 0; n < mesh.selectedIndexesInternal.Length; n++)
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

								mesh.GetCoincidentVertices(mesh.selectedIndexesInternal[n], coincident);

								for (int t = 0, c = coincident.Count; t < c; t++)
									v[coincident[t]] = ver;

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

								mesh.GetCoincidentVertices(mesh.selectedIndexesInternal[n], coincident);

								for (int t = 0, c = coincident.Count; t < c; t++)
									v[coincident[t]] = ver;

								break;
							}
						}
					}

					mesh.mesh.vertices = v;
					mesh.RefreshUV(selectedFacesInEditZone[selection[i]]);
					mesh.Refresh(RefreshMask.Normals);
					mesh.mesh.RecalculateBounds();
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
						int[] triangles = selection[i].selectedIndexesInternal;
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
				List<int> coincident = new List<int>();

				// profiler.BeginSample("matrix mult");
				for (int i = 0; i < selection.Length; i++)
				{
					Vector3[] v = selection[i].positionsInternal;
					SharedVertex[] sharedIndexes = selection[i].sharedVerticesInternal;

					Quaternion lr = m_RotationInitial; // selection[0].transform.localRotation;
					Quaternion ilr = m_RotationInitialInverse; // Quaternion.Inverse(lr);

					for (int n = 0; n < selection[i].selectedIndexesInternal.Length; n++)
					{
						// move vertex to relative origin from center of selection
						Vector3 ver = ilr * (m_VertexPositions[i][n] - m_VertexOffset[i]);

						// rotate
						ver = transformedRotation * ver;

						// move vertex back to locally offset position
						ver = (lr * ver) + m_VertexOffset[i];

						selection[i].GetCoincidentVertices(selection[i].selectedIndexesInternal[n], coincident);

						for (int t = 0, c = coincident.Count; t < c; t++)
							v[coincident[t]] = selection[i].transform.InverseTransformPoint(ver);
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

				switch (componentMode)
				{
					case ComponentMode.Edge:
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
				UpdateSelection();
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
				Vector3 pos = cached.DivideBy(lossyScale);

				if (!m_IsMovingTextures)
				{
					m_TextureHandlePositionPrevious = pos;
					m_IsMovingTextures = true;
				}

				uvEditor.SceneMoveTool(pos - m_TextureHandlePositionPrevious);
				m_TextureHandlePositionPrevious = pos;

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

			if (m_CurrentEvent.type == EventType.Repaint
				&& m_Hovering != null
				&& editLevel == EditLevel.Geometry)
			{
				try
				{
					m_EditorMeshHandles.DrawSceneSelection(m_Hovering);
				}
				catch
				{
					; // this happens on undo, when c++ object is destroyed but c# side thinks it's still alive
				}
			}

			using (new HandleGUI())
			{
				int screenWidth = (int) sceneView.position.width;
				int screenHeight = (int) sceneView.position.height;

				int currentSelectionMode =
					(editLevel != EditLevel.Top && editLevel != EditLevel.Plugin) ? ((int) componentMode) + 1 : 0;

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

						SetSelectionMode((ComponentMode) (currentSelectionMode - 1));
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

					SetSelectionMode(ComponentMode.Vertex);
					return true;
				}

				case "Edge Mode":
				{
					if (!uniqueModeShortcuts)
						return false;

					if (editLevel == EditLevel.Top)
						SetEditLevel(EditLevel.Geometry);

					SetSelectionMode(ComponentMode.Edge);
					return true;
				}

				case "Face Mode":
				{
					if (!uniqueModeShortcuts)
						return false;

					if (editLevel == EditLevel.Top)
						SetEditLevel(EditLevel.Geometry);

					SetSelectionMode(ComponentMode.Face);
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
					UpdateSelection();
					SetEditLevel(EditLevel.Top);
					return true;

				// TODO Remove once a workaround for non-upper-case shortcut chars is found
				case "Toggle Selection Mode":

					if (PreferencesInternal.GetBool(PreferenceKeys.pbUniqueModeShortcuts))
						return false;

					ToggleSelectionMode();
					switch (componentMode)
					{
						case ComponentMode.Face:
							EditorUtility.ShowNotification("Editing Faces");
							break;

						case ComponentMode.Vertex:
							EditorUtility.ShowNotification("Editing Vertices");
							break;

						case ComponentMode.Edge:
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

							if (pbo.selectedIndexesInternal.Length > 0)
							{
								pbo.CenterPivot(pbo.selectedIndexesInternal);
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
			int smode = (int) componentMode;
			smode++;
			if (smode >= k_SelectModeLength)
				smode = 0;
			SetSelectionMode((ComponentMode) smode);
		}

		/// <summary>
		/// Sets what mesh attributes are editable in the scene.
		/// </summary>
		/// <seealso cref="ComponentMode"/>
		/// <param name="mode">The @"UnityEngine.ProBuilder.SelectMode" to engage.</param>
		internal void SetSelectionMode(ComponentMode mode)
		{
			componentMode = mode;

			Internal_UpdateSelectionFast();

			PreferencesInternal.SetInt(PreferenceKeys.pbDefaultSelectionMode, (int) componentMode);

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
		internal void SetEditLevel(EditLevel editMode)
		{
			m_PreviousEditLevel = editLevel;
			editLevel = editMode;

			switch (editMode)
			{
				case EditLevel.Top:
					ClearElementSelection();
					UpdateSelection();

					MeshSelection.SetSelection(Selection.gameObjects);
					break;

				case EditLevel.Geometry:

					Tools.current = Tool.None;

					UpdateSelection();
					SceneView.RepaintAll();
					break;

				case EditLevel.Plugin:
					UpdateSelection();
					SceneView.RepaintAll();
					break;

#if !PROTOTYPE
				case EditLevel.Texture:

					m_PreviousHandleAlignment = handleAlignment;
					m_PreviousComponentMode = componentMode;

					SetHandleAlignment(HandleAlignment.Plane);
					break;
#endif
			}


			if (m_PreviousEditLevel == EditLevel.Texture && editMode != EditLevel.Texture)
			{
				SetSelectionMode(m_PreviousComponentMode);
				SetHandleAlignment(m_PreviousHandleAlignment);
			}

			if (editLevel != EditLevel.Texture)
				PreferencesInternal.SetInt(PreferenceKeys.pbDefaultEditLevel, (int) editLevel);

			if (selectModeChanged != null)
				selectModeChanged(EditorUtility.GetSelectMode(editLevel, componentMode));
		}

		/// <summary>
		/// Rebuild the wireframe selection caches.
		/// </summary>
		void UpdateSelection()
		{
			m_SelectedVertexCount = 0;
			m_SelectedFaceCount = 0;
			m_SelectedEdgeCount = 0;
			m_SelectedVerticesCommon = 0;
			selection = InternalUtility.GetComponents<ProBuilderMesh>(Selection.transforms);

			if (selectedFacesInEditZone != null)
				selectedFacesInEditZone.Clear();
			else
				selectedFacesInEditZone = new Dictionary<ProBuilderMesh, List<Face>>();

			m_HandlePivotWorld = Vector3.zero;

			Vector3 min = Vector3.zero, max = Vector3.zero;
			var boundsInitialized = false;
			HashSet<int> used = new HashSet<int>();

			for (var i = 0; i < selection.Length; i++)
			{
				ProBuilderMesh mesh = selection[i];

				used.Clear();
				var lookup = mesh.sharedVertexLookup;

				if (!boundsInitialized && mesh.selectedVertexCount > 0)
				{
					boundsInitialized = true;
					min = mesh.transform.TransformPoint(mesh.positionsInternal[mesh.selectedIndexesInternal[0]]);
					max = min;
				}

				if (mesh.selectedVertexCount > 0)
				{
					var indexes = mesh.selectedIndexesInternal;

					for (int n = 0, c = mesh.selectedVertexCount; n < c; n++)
					{
						if (used.Add(lookup[indexes[n]]))
						{
							Vector3 v = mesh.transform.TransformPoint(mesh.positionsInternal[indexes[n]]);
							min = Vector3.Min(min, v);
							max = Vector3.Max(max, v);
						}
					}

					m_SelectedVerticesCommon += used.Count;
				}

				selectedFacesInEditZone.Add(mesh, ElementSelection.GetNeighborFaces(mesh, mesh.selectedIndexesInternal));

				m_SelectedVertexCount += mesh.selectedIndexesInternal.Length;
				m_SelectedFaceCount += mesh.selectedFaceCount;
				m_SelectedEdgeCount += mesh.selectedEdgeCount;
			}

			m_HandlePivotWorld = (max + min) * .5f;

			UpdateHandleRotation();
			UpdateTextureHandles();
			m_HandleRotation = handleRotation;

			if (selectionUpdated != null)
				selectionUpdated(selection);

			UpdateSceneInfo();

			try
			{
				m_EditorMeshHandles.RebuildSelectedHandles(MeshSelection.Top(), componentMode);
			}
			catch
			{
				// happens on undo when c++ object is gone but c# isn't in the know
				m_EditorMeshHandles.ClearHandles();
			}
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
				Vector3[] positions = pb.positionsInternal;
				int[] indexes = pb.selectedIndexesInternal;

				if (pb == null) continue;

				if (selection[i].selectedVertexCount > 0)
				{
					if (!boundsInitialized)
					{
						boundsInitialized = true;
						min = pb.transform.TransformPoint(positions[indexes[0]]);
						max = min;
					}

					for (int n = 0; n < selection[i].selectedVertexCount; n++)
					{
						min = Vector3.Min(min, pb.transform.TransformPoint(positions[indexes[n]]));
						max = Vector3.Max(max, pb.transform.TransformPoint(positions[indexes[n]]));
					}
				}

				m_SelectedVertexCount += selection[i].selectedVertexCount;
				m_SelectedFaceCount += selection[i].selectedFaceCount;
				m_SelectedEdgeCount += selection[i].selectedEdgeCount;
			}

			m_HandlePivotWorld = (max + min) / 2f;

			UpdateHandleRotation();
			m_HandleRotation = handleRotation;

			if (selectionUpdated != null)
				selectionUpdated(selection);

			UpdateSceneInfo();

			// todo
			m_EditorMeshHandles.RebuildSelectedHandles(MeshSelection.Top(), componentMode);
		}

		internal void ClearElementSelection()
		{
			foreach (ProBuilderMesh pb in selection)
				pb.ClearSelection();

			m_Hovering.Clear();
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

				handleMatrix *= Matrix4x4.TRS(Math.GetBounds(pb.positionsInternal.ValuesWithIndexes(face.distinctIndexesInternal)).center,
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
										UpdateSelection();
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
			m_Hovering.Clear();
			UpdateSelection();
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
				ProBuilderMesh mesh = selection[i];
				if (mesh.selectedVertexCount < 1)
					continue;

				var indexes = mesh.GetCoincidentVertices(mesh.selectedIndexesInternal);
				Snapping.SnapVertices(mesh, indexes, Vector3.one * snapVal);

				mesh.ToMesh();
				mesh.Refresh();
				mesh.Optimize();
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

			if (beforeMeshModification != null)
				beforeMeshModification(selection);
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

			if (afterMeshModification != null)
				afterMeshModification(selection);
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

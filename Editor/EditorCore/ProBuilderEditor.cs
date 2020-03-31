#if UNITY_2019_1_OR_NEWER
#define SHORTCUT_MANAGER
#endif

using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.ProBuilder.Actions;
using UnityEngine.ProBuilder;
using PMesh = UnityEngine.ProBuilder.ProBuilderMesh;
using UObject = UnityEngine.Object;
using UnityEditor.SettingsManagement;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Manages the ProBuilder toolbar window and tool mode.
    /// </summary>
    public sealed class ProBuilderEditor : EditorWindow, IHasCustomMenu
    {
        // Match the value set in RectSelection.cs
        const float k_MouseDragThreshold = 6f;

        /// <value>
        /// Raised any time the ProBuilder editor refreshes the selection. This is called every frame when interacting with mesh elements, and after any mesh operation.
        /// </value>
        public static event Action<IEnumerable<ProBuilderMesh>> selectionUpdated;

        /// <value>
        /// Raised when the EditLevel is changed.
        /// </value>
        public static event Action<SelectMode> selectModeChanged;

        /// <value>
        /// Called when vertex modifications are complete.
        /// </value>
        public static event Action<IEnumerable<ProBuilderMesh>> afterMeshModification;

        /// <value>
        /// Called immediately prior to beginning vertex modifications. The ProBuilderMesh will be in un-altered state at this point (meaning ProBuilderMesh.ToMesh and ProBuilderMesh.Refresh have been called, but not Optimize).
        /// </value>
        public static event Action<IEnumerable<ProBuilderMesh>> beforeMeshModification;

        internal static EditorToolbar s_EditorToolbar;
        static ProBuilderEditor s_Instance;

        GUIContent[] m_EditModeIcons;
        GUIStyle VertexTranslationInfoStyle;

        [UserSetting("General", "Show Scene Info", "Toggle the display of information about selected meshes in the Scene View.")]
        static Pref<bool> s_ShowSceneInfo = new Pref<bool>("editor.showSceneInfo", false);

        [UserSetting("Toolbar", "Icon GUI", "Toggles the ProBuilder window interface between text and icon versions.")]
        internal static Pref<bool> s_IsIconGui = new Pref<bool>("editor.toolbarIconGUI", false);

#if !SHORTCUT_MANAGER
        [UserSetting("Toolbar", "Unique Mode Shortcuts", "When off, the G key toggles between Object and Element modes and H enumerates the element modes.  If on, G, H, J, and K are shortcuts to Object, Vertex, Edge, and Face modes respectively.")]
        internal static Pref<bool> s_UniqueModeShortcuts = new Pref<bool>("editor.uniqueModeShortcuts", false, SettingsScope.User);
#endif

        [UserSetting("Mesh Editing", "Allow non-manifold actions", "Enables advanced mesh editing techniques that may create non-manifold geometry.")]
        internal static Pref<bool> s_AllowNonManifoldActions = new Pref<bool>("editor.allowNonManifoldActions", false, SettingsScope.User);

        [UserSetting("Toolbar", "Toolbar Location", "Where the Object, Face, Edge, and Vertex toolbar will be shown in the Scene View.")]
        static Pref<SceneToolbarLocation> s_SceneToolbarLocation = new Pref<SceneToolbarLocation>("editor.sceneToolbarLocation", SceneToolbarLocation.UpperCenter, SettingsScope.User);

        static Pref<bool> s_WindowIsFloating = new Pref<bool>("UnityEngine.ProBuilder.ProBuilderEditor-isUtilityWindow", false, SettingsScope.Project);
        static Pref<bool> m_BackfaceSelectEnabled = new Pref<bool>("editor.backFaceSelectEnabled", false);
        static Pref<RectSelectMode> m_DragSelectRectMode = new Pref<RectSelectMode>("editor.dragSelectRectMode", RectSelectMode.Partial);
        static Pref<SelectionModifierBehavior> m_SelectModifierBehavior = new Pref<SelectionModifierBehavior>("editor.rectSelectModifier", SelectionModifierBehavior.Difference);
        static Pref<SelectMode> s_SelectMode = new Pref<SelectMode>("editor.selectMode", SelectMode.Object);

        internal static RectSelectMode rectSelectMode
        {
            get { return m_DragSelectRectMode.value; }

            set
            {
                if (m_DragSelectRectMode.value == value)
                    return;

                m_DragSelectRectMode.SetValue(value, true);

                if (s_Instance != null)
                    s_Instance.m_ScenePickerPreferences.rectSelectMode = value;
            }
        }

        internal static SelectionModifierBehavior selectionModifierBehavior
        {
            get { return m_SelectModifierBehavior.value; }

            set
            {
                if (s_Instance == null || m_SelectModifierBehavior.value == value)
                    return;

                m_SelectModifierBehavior.SetValue(value, true);

                if (s_Instance != null)
                    s_Instance.m_ScenePickerPreferences.selectionModifierBehavior = value;
            }
        }

        internal static bool backfaceSelectionEnabled
        {
            get { return m_BackfaceSelectEnabled.value; }

            set
            {
                if (value == m_BackfaceSelectEnabled.value)
                    return;

                m_BackfaceSelectEnabled.SetValue(value, true);

                if(s_Instance != null)
                    s_Instance.m_ScenePickerPreferences.cullMode = value ? CullingMode.None : CullingMode.Back;
            }
        }

        // used for 'g' key shortcut to swap between object/vef modes
        SelectMode m_LastComponentMode;

#if !SHORTCUT_MANAGER
        [UserSetting]
        internal static Pref<Shortcut[]> s_Shortcuts = new Pref<Shortcut[]>("editor.sceneViewShortcuts", Shortcut.DefaultShortcuts().ToArray());
#endif
        GUIStyle m_CommandStyle;
        Rect m_ElementModeToolbarRect = new Rect(3, 6, 128, 24);

        int m_DefaultControl;
        SceneSelection m_Hovering = new SceneSelection();
        SceneSelection m_HoveringPrevious = new SceneSelection();
        ScenePickerPreferences m_ScenePickerPreferences;

        [UserSetting("Graphics", "Show Hover Highlight", "Highlight the mesh element nearest to the mouse cursor.")]
        static Pref<bool> s_ShowHoverHighlight = new Pref<bool>("editor.showPreselectionHighlight", true, SettingsScope.User);

        Tool m_CurrentTool = Tool.Move;
        Vector2 m_InitialMousePosition;
        Rect m_MouseDragRect;
        bool m_IsDragging;
        bool m_IsReadyForMouseDrag;
        // prevents leftClickUp from stealing focus after double click
        bool m_WasDoubleClick;
        // vertex handles
#if !SHORTCUT_MANAGER
        bool m_IsRightMouseDown;
#endif
        static Dictionary<Type, VertexManipulationTool> s_EditorTools = new Dictionary<Type, VertexManipulationTool>();

        Vector3[][] m_VertexPositions;
        Vector3[] m_VertexOffset;

        GUIContent m_SceneInfo = new GUIContent();

        Rect m_SceneInfoRect = new Rect(10, 10, 200, 40);

#if !UNITY_2018_2_OR_NEWER
        static MethodInfo s_ResetOnSceneGUIState = null;
#endif

        // All selected pb_Objects
        internal List<ProBuilderMesh> selection
        {
            get { return MeshSelection.topInternal; }
        }

        Event m_CurrentEvent;

        internal bool isFloatingWindow { get; private set; }

        /// <value>
        /// Get the current @"UnityEngine.ProBuilder.EditLevel".
        /// </value>
        [Obsolete]
        internal static EditLevel editLevel
        {
            get { return s_Instance != null ? EditorUtility.GetEditLevel(s_SelectMode) : EditLevel.Top; }
        }

        /// <summary>
        /// Get the current @"UnityEngine.ProBuilder.SelectMode".
        /// </summary>
        /// <value>The ComponentMode currently set.</value>
        [Obsolete]
        internal static ComponentMode componentMode
        {
            get { return s_Instance != null ? EditorUtility.GetComponentMode(s_SelectMode) : ComponentMode.Face; }
        }

        /// <value>
        /// Get and set the current SelectMode.
        /// </value>
        public static SelectMode selectMode
        {
            get
            {
                if (s_Instance != null)
                    return s_SelectMode;

                // for backwards compatibility reasons `Object` is returned when editor is closed
                return SelectMode.Object;
            }

            set
            {
                if (s_Instance == null)
                    return;

                var previous = s_SelectMode.value;

                if (previous == value)
                    return;

                s_SelectMode.SetValue(value, true);

                if (previous == SelectMode.Edge || previous == SelectMode.Vertex || previous == SelectMode.Face)
                    s_Instance.m_LastComponentMode = previous;

                if (value == SelectMode.Object)
                    Tools.current = s_Instance.m_CurrentTool;

                if (selectModeChanged != null)
                    selectModeChanged(value);

                Refresh();
            }
        }

        Stack<SelectMode> m_SelectModeHistory = new Stack<SelectMode>();

        internal static void PushSelectMode(SelectMode mode)
        {
            s_Instance.m_SelectModeHistory.Push(selectMode);
            selectMode = mode;
        }

        internal static void PopSelectMode()
        {
            if (s_Instance.m_SelectModeHistory.Count < 1)
                return;
            selectMode = s_Instance.m_SelectModeHistory.Pop();
        }

        internal static void ResetToLastSelectMode()
        {
            if (s_Instance != null)
                selectMode = s_Instance.m_LastComponentMode;
        }

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
                    border = new RectOffset(1, 1, 1, 1),
                    margin = new RectOffset(0, 0, 0, 0),
                    padding = new RectOffset(0, 0, 0, 0)
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
            ProBuilderEditor editor = (ProBuilderEditor)EditorWindow.GetWindow(typeof(ProBuilderEditor),
                    s_WindowIsFloating, PreferenceKeys.pluginTitle,
                    true); // open as floating window
            editor.isFloatingWindow = s_WindowIsFloating;
        }

        void OnBecameVisible()
        {
            // fixes maximizing/unmaximizing
            s_Instance = this;
        }

        void OnEnable()
        {
            s_Instance = this;

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
            ProGridsInterface.SubscribePushToGridEvent(PushToGrid);
            ProGridsInterface.SubscribeToolbarEvent(ProGridsToolbarOpen);
            MeshSelection.objectSelectionChanged += OnObjectSelectionChanged;

            ProGridsToolbarOpen(ProGridsInterface.SceneToolbarIsExtended());

#if !UNITY_2018_2_OR_NEWER
            s_ResetOnSceneGUIState = typeof(SceneView).GetMethod("ResetOnSceneGUIState", BindingFlags.Instance | BindingFlags.NonPublic);
#endif

            VertexManipulationTool.beforeMeshModification += BeforeMeshModification;
            VertexManipulationTool.afterMeshModification += AfterMeshModification;

            LoadSettings();
            InitGUI();
            UpdateSelection();
            SetOverrideWireframe(true);

            if (selectModeChanged != null)
                selectModeChanged(selectMode);
        }

        void OnDisable()
        {
            s_Instance = null;

            VertexManipulationTool.beforeMeshModification -= BeforeMeshModification;
            VertexManipulationTool.afterMeshModification -= AfterMeshModification;

            if (s_EditorToolbar != null)
                DestroyImmediate(s_EditorToolbar);

            ClearElementSelection();

            UpdateSelection();

            if (selectionUpdated != null)
                selectionUpdated(null);

#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif
            ProGridsInterface.UnsubscribePushToGridEvent(PushToGrid);
            ProGridsInterface.UnsubscribeToolbarEvent(ProGridsToolbarOpen);
            MeshSelection.objectSelectionChanged -= OnObjectSelectionChanged;

            SetOverrideWireframe(false);

            if (selectModeChanged != null)
                selectModeChanged(SelectMode.Object);

            SceneView.RepaintAll();
        }

        void BeforeMeshModification(IEnumerable<ProBuilderMesh> meshes)
        {
            if(beforeMeshModification != null)
                beforeMeshModification(meshes);
        }

        void AfterMeshModification(IEnumerable<ProBuilderMesh> meshes)
        {
            if(afterMeshModification != null)
                afterMeshModification(meshes);
        }

        internal static void ReloadSettings()
        {
            if (s_Instance != null)
                s_Instance.LoadSettings();
            SceneView.RepaintAll();
        }

        void LoadSettings()
        {
            EditorMeshHandles.ResetPreferences();

            m_ScenePickerPreferences = new ScenePickerPreferences()
            {
                cullMode = m_BackfaceSelectEnabled ? CullingMode.None : CullingMode.Back,
                selectionModifierBehavior = m_SelectModifierBehavior,
                rectSelectMode = m_DragSelectRectMode
            };

#if !SHORTCUT_MANAGER
            // workaround for old single-key shortcuts
            if (s_Shortcuts.value == null || s_Shortcuts.value.Length < 1)
                s_Shortcuts.SetValue(Shortcut.DefaultShortcuts().ToArray(), true);
#endif
        }

        void InitGUI()
        {
            OpenEditorToolbar();
            VertexTranslationInfoStyle = new GUIStyle();
            VertexTranslationInfoStyle.normal.background = EditorGUIUtility.whiteTexture;
            VertexTranslationInfoStyle.normal.textColor = new Color(1f, 1f, 1f, .6f);
            VertexTranslationInfoStyle.padding = new RectOffset(3, 3, 3, 0);
        }

        /// <summary>
        /// Rebuild the mesh wireframe and selection caches.
        /// </summary>
        public static void Refresh(bool vertexCountChanged = true)
        {
            if (instance != null)
            {
                instance.UpdateSelection(vertexCountChanged);
                SceneView.RepaintAll();
            }
        }

        void OnGUI()
        {
            if (s_EditorToolbar != null && s_EditorToolbar.isIconMode != s_IsIconGui.value)
                IconModeChanged();

            if (m_CommandStyle == null)
                m_CommandStyle = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("Command");

            Event e = Event.current;

            switch (e.type)
            {
                case EventType.ContextClick:
                    var menu = new GenericMenu();
                    AddItemsToMenu(menu);
                    menu.ShowAsContext();
                    break;

#if !SHORTCUT_MANAGER
                case EventType.KeyDown:
                    if (s_Shortcuts.value.Any(x => x.Matches(e.keyCode, e.modifiers)))
                        e.Use();
                    break;

                case EventType.KeyUp:
                    ShortcutCheck(e);
                    break;
#else
                case EventType.KeyUp:
                    if (e.keyCode == KeyCode.Escape)
                    {
                        selectMode = SelectMode.Object;
                        e.Use();
                    }
                    break;
#endif
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

        void OpenEditorToolbar()
        {
            if (s_EditorToolbar != null)
                DestroyImmediate(s_EditorToolbar);

            s_EditorToolbar = ScriptableObject.CreateInstance<EditorToolbar>();
            s_EditorToolbar.hideFlags = HideFlags.HideAndDontSave;
            s_EditorToolbar.InitWindowProperties(this);
        }

        void IconModeChanged()
        {
            OpenEditorToolbar();
        }

        void Menu_ToggleIconMode()
        {
            s_IsIconGui.value = !s_IsIconGui.value;
            IconModeChanged();
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            bool floating = s_WindowIsFloating;

            menu.AddItem(new GUIContent("Window/Open as Floating Window", ""), floating, () => SetIsUtilityWindow(true));
            menu.AddItem(new GUIContent("Window/Open as Dockable Window", ""), !floating, () => SetIsUtilityWindow(false));
            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Use Icon Mode", ""), s_IsIconGui,
                Menu_ToggleIconMode);
            menu.AddItem(new GUIContent("Use Text Mode", ""), !s_IsIconGui,
                Menu_ToggleIconMode);
        }

        void SetIsUtilityWindow(bool isUtilityWindow)
        {
            s_WindowIsFloating.value = isUtilityWindow;
            var windowTitle = titleContent;
            Close();
            var res = GetWindow(GetType(), isUtilityWindow);
            res.titleContent = windowTitle;
        }

        internal static VertexManipulationTool activeTool
        {
            get
            {
                return s_Instance == null
                    ? null
                    : s_Instance.GetToolForSelectMode(s_Instance.m_CurrentTool, s_SelectMode);
            }
        }

        VertexManipulationTool GetTool<T>() where T : VertexManipulationTool, new()
        {
            VertexManipulationTool tool;

            if (s_EditorTools.TryGetValue(typeof(T), out tool))
                return tool;
            tool = new T();
            s_EditorTools.Add(typeof(T), tool);
            return tool;
        }

        VertexManipulationTool GetToolForSelectMode(Tool tool, SelectMode mode)
        {
            switch (tool)
            {
                case Tool.Move:
                    return mode.IsTextureMode()
                        ? GetTool<TextureMoveTool>()
                        : GetTool<PositionMoveTool>();
                case Tool.Rotate:
                    return mode.IsTextureMode()
                        ? GetTool<TextureRotateTool>()
                        : GetTool<PositionRotateTool>();
                case Tool.Scale:
                    return mode.IsTextureMode()
                        ? GetTool<TextureScaleTool>()
                        : GetTool<PositionScaleTool>();
                default:
                    return null;
            }
        }

        void OnSceneGUI(SceneView sceneView)
        {
#if !UNITY_2018_2_OR_NEWER
            if (s_ResetOnSceneGUIState != null)
                s_ResetOnSceneGUIState.Invoke(sceneView, null);
#endif

            SceneStyles.Init();

            m_CurrentEvent = Event.current;

            EditorMeshHandles.DrawSceneHandles(SceneDragAndDropListener.isDragging ? SelectMode.None : selectMode);

            DrawHandleGUI(sceneView);

#if SHORTCUT_MANAGER
            // Escape isn't assignable as a shortcut
            if (m_CurrentEvent.type == EventType.KeyDown)
            {
                if (m_CurrentEvent.keyCode == KeyCode.Escape && selectMode != SelectMode.Object)
                {
                    selectMode = SelectMode.Object;
                    m_CurrentEvent.Use();
                }
            }
#else
            if (m_CurrentEvent.type == EventType.MouseDown && m_CurrentEvent.button == 1)
                m_IsRightMouseDown = true;

            if (m_CurrentEvent.type == EventType.MouseUp && m_CurrentEvent.button == 1 || m_CurrentEvent.type == EventType.Ignore)
                m_IsRightMouseDown = false;

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
                if (s_Shortcuts.value.Any(x => x.Matches(m_CurrentEvent.keyCode, m_CurrentEvent.modifiers)))
                    m_CurrentEvent.Use();
            }
#endif

            if (selectMode == SelectMode.Object)
                return;

            // Check mouse position in scene and determine if we should highlight something
            if (s_ShowHoverHighlight
                && m_CurrentEvent.type == EventType.MouseMove
                && selectMode.IsMeshElementMode())
            {
                m_Hovering.CopyTo(m_HoveringPrevious);

                if (GUIUtility.hotControl != 0 ||
                    EditorSceneViewPicker.MouseRayHitTest(m_CurrentEvent.mousePosition, selectMode, m_ScenePickerPreferences, m_Hovering) > ScenePickerPreferences.maxPointerDistance)
                    m_Hovering.Clear();

                if (!m_Hovering.Equals(m_HoveringPrevious))
                    SceneView.RepaintAll();
            }

            if (Tools.current == Tool.View)
                return;

            switch (m_CurrentEvent.type)
            {
                case EventType.ValidateCommand:
                case EventType.ExecuteCommand:
                    bool execute = m_CurrentEvent.type == EventType.ExecuteCommand;
                    switch (m_CurrentEvent.commandName)
                    {
                        case "SelectAll":
                            if (execute)
                            {
                                SelectAll();
                            }
                            m_CurrentEvent.Use();
                            break;
                        case "DeselectAll":
                            if (execute)
                            {
                                DeselectAll();
                            }
                            m_CurrentEvent.Use();
                            break;
                        case "InvertSelection":
                            if (execute)
                            {
                                InvertSelection();
                            }
                            m_CurrentEvent.Use();
                            break;
                    }
                    break;
            }

            // Overrides the toolbar transform tools
            if (Tools.current != Tool.None && Tools.current != m_CurrentTool)
                SetTool_Internal(Tools.current);

            Tools.current = Tool.None;

            if (selectMode.IsMeshElementMode() && MeshSelection.selectedVertexCount > 0)
            {
                var tool = GetToolForSelectMode(m_CurrentTool, s_SelectMode);

                if (tool != null)
                    tool.OnSceneGUI(m_CurrentEvent);
            }

            if (EditorHandleUtility.SceneViewInUse(m_CurrentEvent) || m_CurrentEvent.isKey)
            {
                m_IsDragging = false;

                if (GUIUtility.hotControl == m_DefaultControl)
                    GUIUtility.hotControl = 0;

                return;
            }

            // This prevents us from selecting other objects in the scene,
            // and allows for the selection of faces / vertices.
            m_DefaultControl = GUIUtility.GetControlID(FocusType.Passive);
            if (Event.current.type == EventType.Layout)
                HandleUtility.AddDefaultControl(m_DefaultControl);

            if (m_CurrentEvent.type == EventType.MouseDown && HandleUtility.nearestControl == m_DefaultControl)
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

                GUIUtility.hotControl = m_DefaultControl;
            }

            if (m_CurrentEvent.type == EventType.MouseDrag && m_IsReadyForMouseDrag && GUIUtility.hotControl == m_DefaultControl)
            {
                if (!m_IsDragging && Vector2.Distance(m_CurrentEvent.mousePosition, m_InitialMousePosition) > k_MouseDragThreshold)
                {
                    sceneView.Repaint();
                    m_IsDragging = true;
                }
            }

            if (m_CurrentEvent.type == EventType.Ignore)
            {
                if (m_IsDragging)
                {
                    m_IsReadyForMouseDrag = false;
                    m_IsDragging = false;
                    EditorSceneViewPicker.DoMouseDrag(m_MouseDragRect, selectMode, m_ScenePickerPreferences);
                }

                if (m_WasDoubleClick)
                    m_WasDoubleClick = false;

                if (GUIUtility.hotControl == m_DefaultControl)
                    GUIUtility.hotControl = 0;
            }

            if (m_CurrentEvent.type == EventType.MouseUp && GUIUtility.hotControl == m_DefaultControl)
            {
                GUIUtility.hotControl = 0;

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

                        EditorSceneViewPicker.DoMouseClick(m_CurrentEvent, selectMode, m_ScenePickerPreferences);
                        UpdateSelection();
                        SceneView.RepaintAll();
                    }
                    else
                    {
                        m_IsDragging = false;
                        m_IsReadyForMouseDrag = false;

                        if (UVEditor.instance)
                            UVEditor.instance.ResetUserPivot();

                        EditorSceneViewPicker.DoMouseDrag(m_MouseDragRect, selectMode, m_ScenePickerPreferences);

                        if (GUIUtility.hotControl == m_DefaultControl)
                            GUIUtility.hotControl = 0;
                    }
                }
            }
        }

        void SelectAll()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return;

            UndoUtility.RecordSelection("Select all");

            switch (selectMode)
            {
                case SelectMode.Vertex:
                    foreach (var mesh in MeshSelection.topInternal)
                    {
                        var sharedIndexes = mesh.sharedVerticesInternal;
                        var all = new List<int>();

                        for (var i = 0; i < sharedIndexes.Length; i++)
                        {
                            all.Add(sharedIndexes[i][0]);
                        }

                        mesh.SetSelectedVertices(all);
                    }
                    break;

                case SelectMode.Face:
                case SelectMode.TextureFace:
                    foreach (var mesh in MeshSelection.topInternal)
                    {
                        mesh.SetSelectedFaces(mesh.facesInternal);
                    }
                    break;

                case SelectMode.Edge:

                    foreach (var mesh in MeshSelection.topInternal)
                    {
                        var universalEdges = mesh.GetSharedVertexHandleEdges(mesh.facesInternal.SelectMany(x => x.edges)).ToArray();
                        var all = new Edge[universalEdges.Length];

                        for (var n = 0; n < universalEdges.Length; n++)
                            all[n] = new Edge(mesh.sharedVerticesInternal[universalEdges[n].a][0], mesh.sharedVerticesInternal[universalEdges[n].b][0]);

                        mesh.SetSelectedEdges(all);
                    }
                    break;
            }

            Refresh();
            SceneView.RepaintAll();
        }

        void DeselectAll()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return;

            UndoUtility.RecordSelection("Deselect All");

            switch (selectMode)
            {
                case SelectMode.Vertex:
                    foreach (var mesh in MeshSelection.topInternal)
                    {
                        mesh.SetSelectedVertices(null);
                    }
                    break;

                case SelectMode.Face:
                case SelectMode.TextureFace:
                    foreach (var mesh in MeshSelection.topInternal)
                    {
                        mesh.SetSelectedFaces((IEnumerable<Face>) null);
                    }
                    break;

                case SelectMode.Edge:

                    foreach (var mesh in MeshSelection.topInternal)
                    {
                        mesh.SetSelectedEdges(null);
                    }
                    break;
            }

            Refresh();
            SceneView.RepaintAll();
        }

        void InvertSelection()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return;

            UndoUtility.RecordSelection("Invert Selection");

            switch (selectMode)
            {
                case SelectMode.Vertex:
                    foreach (var mesh in MeshSelection.topInternal)
                    {
                        var sharedIndexes = mesh.sharedVerticesInternal;
                        var selectedSharedIndexes = new List<int>();

                        foreach (int i in mesh.selectedIndexesInternal)
                            selectedSharedIndexes.Add(mesh.GetSharedVertexHandle(i));

                        var inverse = new List<int>();

                        for (int i = 0; i < sharedIndexes.Length; i++)
                        {
                            if (!selectedSharedIndexes.Contains(i))
                                inverse.Add(sharedIndexes[i][0]);
                        }

                        mesh.SetSelectedVertices(inverse.ToArray());
                    }

                    break;

                case SelectMode.Face:
                case SelectMode.TextureFace:
                    foreach (var mesh in MeshSelection.topInternal)
                    {
                        var inverse = mesh.facesInternal.Where(x => !mesh.selectedFacesInternal.Contains(x));
                        mesh.SetSelectedFaces(inverse.ToArray());
                    }

                    break;

                case SelectMode.Edge:

                    foreach (var mesh in MeshSelection.topInternal)
                    {
                        var universalEdges =
                            mesh.GetSharedVertexHandleEdges(mesh.facesInternal.SelectMany(x => x.edges)).ToArray();
                        var universalSelectedEdges =
                            EdgeUtility.GetSharedVertexHandleEdges(mesh, mesh.selectedEdges).Distinct();
                        var inverseUniversal =
                            System.Array.FindAll(universalEdges, x => !universalSelectedEdges.Contains(x));
                        var inverse = new Edge[inverseUniversal.Length];

                        for (var n = 0; n < inverseUniversal.Length; n++)
                            inverse[n] = new Edge(mesh.sharedVerticesInternal[inverseUniversal[n].a][0],
                                mesh.sharedVerticesInternal[inverseUniversal[n].b][0]);

                        mesh.SetSelectedEdges(inverse);
                    }

                    break;
            }

            Refresh();
            SceneView.RepaintAll();
        }

        void DoubleClick(Event e)
        {
            var mesh = EditorSceneViewPicker.DoMouseClick(m_CurrentEvent, selectMode, m_ScenePickerPreferences);

            if (mesh != null)
            {
                if (selectMode.ContainsFlag(SelectMode.Edge | SelectMode.TextureEdge))
                {
                    if (e.shift)
                        EditorUtility.ShowNotification(EditorToolbarLoader.GetInstance<Actions.SelectEdgeRing>().DoAction());
                    else
                        EditorUtility.ShowNotification(EditorToolbarLoader.GetInstance<Actions.SelectEdgeLoop>().DoAction());
                }
                else if (selectMode.ContainsFlag(SelectMode.Face | SelectMode.TextureFace))
                {
                    if ((e.modifiers & (EventModifiers.Control | EventModifiers.Shift)) ==
                        (EventModifiers.Control | EventModifiers.Shift))
                        Actions.SelectFaceRing.MenuRingAndLoopFaces(MeshSelection.topInternal);
                    else if (e.control)
                        EditorUtility.ShowNotification(EditorToolbarLoader.GetInstance<Actions.SelectFaceRing>().DoAction());
                    else if (e.shift)
                        EditorUtility.ShowNotification(EditorToolbarLoader.GetInstance<Actions.SelectFaceLoop>().DoAction());
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

        void DrawHandleGUI(SceneView sceneView)
        {
            if (sceneView != SceneView.lastActiveSceneView)
                return;

            if (m_CurrentEvent.type == EventType.Repaint
                && !SceneDragAndDropListener.isDragging
                && m_Hovering != null
                && GUIUtility.hotControl == 0
                && HandleUtility.nearestControl == m_DefaultControl
                && selectMode.IsMeshElementMode())
            {
                try
                {
                    EditorMeshHandles.DrawSceneSelection(m_Hovering);
                }
                catch
                {
                    // this happens on undo, when c++ object is destroyed but c# side thinks it's still alive
                }
            }

            using (new HandleGUI())
            {
                int screenWidth = (int)sceneView.position.width;
                int screenHeight = (int)sceneView.position.height;

                switch ((SceneToolbarLocation)s_SceneToolbarLocation)
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

                selectMode = UI.EditorGUIUtility.DoElementModeToolbar(m_ElementModeToolbarRect, selectMode);

                if (s_ShowSceneInfo)
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

#if !SHORTCUT_MANAGER
        internal bool ShortcutCheck(Event e)
        {
            List<Shortcut> matches = s_Shortcuts.value.Where(x => x.Matches(e.keyCode, e.modifiers)).ToList();

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
                    used |= GeoLevelShortcuts(cut);
            }

            if (used)
                Event.current.Use();

            if (usedShortcut != null)
                EditorUtility.ShowNotification(usedShortcut.action);

            return used;
        }

        bool AllLevelShortcuts(Shortcut shortcut)
        {
            switch (shortcut.action)
            {
                // TODO Remove once a workaround for non-upper-case shortcut chars is found
                case "Toggle Geometry Mode":

                    if (selectMode == SelectMode.Object)
                        selectMode = m_LastComponentMode;
                    else
                        selectMode = SelectMode.Object;
                    EditorUtility.ShowNotification(selectMode.ToString() + " Editing");
                    return true;

                case "Vertex Mode":
                {
                    if (!s_UniqueModeShortcuts)
                        return false;
                    selectMode = SelectMode.Vertex;
                    return true;
                }

                case "Edge Mode":
                {
                    if (!s_UniqueModeShortcuts)
                        return false;
                    selectMode = SelectMode.Edge;
                    return true;
                }

                case "Face Mode":
                {
                    if (!s_UniqueModeShortcuts)
                        return false;
                    selectMode = SelectMode.Face;
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
                    selectMode = SelectMode.Object;
                    return true;

                // Used to be (incorrectly) named handle pivot, and since shortcuts are serialized this value is still valid
                case "Toggle Handle Pivot":
                case "Toggle Handle Orientation":
                    VertexManipulationTool.handleOrientation = InternalUtility.NextEnumValue(VertexManipulationTool.handleOrientation);
                    return true;

                // TODO Remove once a workaround for non-upper-case shortcut chars is found
                case "Toggle Selection Mode":
                    if (s_UniqueModeShortcuts)
                        return false;
                    ToggleSelectionMode();
                    EditorUtility.ShowNotification(selectMode.ToString());
                    return true;

                case "Delete Face":
                    EditorUtility.ShowNotification(EditorToolbarLoader.GetInstance<DeleteFaces>().DoAction().notification);
                    return true;

                case "Set Pivot":

                    if (selection.Count > 0)
                    {
                        foreach (ProBuilderMesh pbo in selection)
                        {
                            UndoUtility.RecordObjects(new UObject[2] { pbo, pbo.transform }, "Set Pivot");

                            if (pbo.selectedIndexesInternal.Length > 0)
                            {
                                pbo.CenterPivot(pbo.selectedIndexesInternal);
                            }
                            else
                            {
                                pbo.CenterPivot(null);
                            }
                        }

                        EditorUtility.ShowNotification("Set Pivot");
                    }

                    return true;

                default:
                    return false;
            }
        }
#endif

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

        /// <summary>
        /// Toggles between the SelectMode values and updates the graphic handles as necessary.
        /// </summary>
        internal void ToggleSelectionMode()
        {
            if (s_SelectMode == SelectMode.Vertex)
                selectMode = SelectMode.Edge;
            else if (s_SelectMode == SelectMode.Edge)
                selectMode = SelectMode.Face;
            else if (s_SelectMode == SelectMode.Face)
                selectMode = SelectMode.Vertex;
        }

        void UpdateSelection(bool selectionChanged = true)
        {
            UpdateMeshHandles(selectionChanged);

            if (selectionChanged)
            {
                MeshSelection.OnComponentSelectionChanged();
                UpdateSceneInfo();
            }

            if (selectionUpdated != null)
                selectionUpdated(selection);
        }

        internal static void UpdateMeshHandles(bool selectionOrVertexCountChanged = true)
        {
            if (!s_Instance)
                return;

            try
            {
                EditorMeshHandles.RebuildSelectedHandles(MeshSelection.topInternal, selectMode);
            }
            catch
            {
                // happens on undo when c++ object is gone but c# isn't in the know
                EditorMeshHandles.ClearHandles();
            }
        }

        void UpdateSceneInfo()
        {
            m_SceneInfo.text = string.Format(
                    "Faces: <b>{0}</b>\nTriangles: <b>{1}</b>\nVertices: <b>{2} ({3})</b>\n\nSelected Faces: <b>{4}</b>\nSelected Edges: <b>{5}</b>\nSelected Vertices: <b>{6} ({7})</b>",
                    MeshSelection.totalFaceCount.ToString(),
                    MeshSelection.totalTriangleCountCompiled.ToString(),
                    MeshSelection.totalCommonVertexCount.ToString(),
                    MeshSelection.totalVertexCountOptimized.ToString(),
                    MeshSelection.selectedFaceCount.ToString(),
                    MeshSelection.selectedEdgeCount.ToString(),
                    MeshSelection.selectedSharedVertexCount.ToString(),
                    MeshSelection.selectedVertexCount.ToString());
        }

        internal void ClearElementSelection()
        {
            foreach (ProBuilderMesh pb in selection)
                pb.ClearSelection();

            m_Hovering.Clear();
        }

        /// <summary>
        /// If dragging a texture aroudn, this method ensures that if it's a member of a texture group it's cronies are also selected
        /// </summary>
        void VerifyTextureGroupSelection()
        {
            bool selectionModified = false;

            foreach (ProBuilderMesh mesh in selection)
            {
                List<int> alreadyChecked = new List<int>();

                foreach (Face f in mesh.selectedFacesInternal)
                {
                    int tg = f.textureGroup;

                    if (tg > 0 && !alreadyChecked.Contains(f.textureGroup))
                    {
                        foreach (Face j in mesh.facesInternal)
                        {
                            if (j != f && j.textureGroup == tg && !mesh.selectedFacesInternal.Contains(j))
                            {
                                List<Face> newFaceSection = new List<Face>();
                                foreach (Face jf in mesh.facesInternal)
                                    if (jf.textureGroup == tg)
                                        newFaceSection.Add(jf);
                                mesh.SetSelectedFaces(newFaceSection.ToArray());
                                selectionModified = true;
                                break;
                            }
                        }
                    }

                    alreadyChecked.Add(f.textureGroup);
                }
            }

            if (selectionModified)
                UpdateSelection(true);
        }

        void OnObjectSelectionChanged()
        {
            m_Hovering.Clear();
            UpdateSelection();
            SetOverrideWireframe(true);
        }

        /// <summary>
        /// Hide the default unity wireframe renderer
        /// </summary>
        void SetOverrideWireframe(bool overrideWireframe)
        {
            const EditorSelectedRenderState k_DefaultSelectedRenderState = EditorSelectedRenderState.Highlight | EditorSelectedRenderState.Wireframe;

            foreach (var mesh in Selection.transforms.GetComponents<ProBuilderMesh>())
            {
                // Disable Wireframe for meshes when ProBuilder is active
                EditorUtility.SetSelectionRenderState(
                    mesh.renderer,
                    overrideWireframe
                        ? k_DefaultSelectedRenderState & ~(EditorSelectedRenderState.Wireframe)
                        : k_DefaultSelectedRenderState);

                EditorUtility.SynchronizeWithMeshFilter(mesh);
            }

            SceneView.RepaintAll();
        }

        /// <summary>
        /// Called from ProGrids.
        /// </summary>
        /// <param name="snapVal"></param>
        void PushToGrid(float snapVal)
        {
            UndoUtility.RecordSelection(selection.ToArray(), "Push elements to Grid");

            if (selectMode == SelectMode.Object || selectMode == SelectMode.None)
                return;

            for (int i = 0, c = MeshSelection.selectedObjectCount; i < c; i++)
            {
                ProBuilderMesh mesh = selection[i];
                if (mesh.selectedVertexCount < 1)
                    continue;

                var indexes = mesh.GetCoincidentVertices(mesh.selectedIndexesInternal);
                ProBuilderSnapping.SnapVertices(mesh, indexes, Vector3.one * snapVal);

                mesh.ToMesh();
                mesh.Refresh();
                mesh.Optimize();
            }

            UpdateSelection();
        }

        void ProGridsToolbarOpen(bool menuOpen)
        {
            bool active = ProGridsInterface.IsActive();
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
        /// Returns the first selected pb_Object and pb_Face, or false if not found.
        /// </summary>
        /// <param name="pb"></param>
        /// <param name="face"></param>
        /// <returns></returns>
        internal bool GetFirstSelectedFace(out ProBuilderMesh pb, out Face face)
        {
            pb = null;
            face = null;

            if (selection.Count < 1)
                return false;

            pb = selection.FirstOrDefault(x => x.selectedFaceCount > 0);

            if (pb == null)
                return false;

            face = pb.selectedFacesInternal[0];

            return true;
        }
    }
}

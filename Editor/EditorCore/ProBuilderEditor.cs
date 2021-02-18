using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using PMesh = UnityEngine.ProBuilder.ProBuilderMesh;
using UObject = UnityEngine.Object;
using UnityEditor.SettingsManagement;
#if UNITY_2020_2_OR_NEWER
using EditorToolManager = UnityEditor.EditorTools.EditorToolManager;
using ToolManager = UnityEditor.EditorTools.ToolManager;
#else
using EditorToolManager = UnityEditor.EditorTools.EditorToolContext;
using ToolManager = UnityEditor.EditorTools.EditorTools;
#endif

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

        EditorToolbar m_Toolbar;
        ProBuilderToolManager m_ToolManager; // never use this directly! use toolManager getter to avoid problems with multiple editor instances
        static ProBuilderToolManager toolManager => s_Instance != null ? s_Instance.m_ToolManager : null;
        internal EditorToolbar toolbar => m_Toolbar; // used by unit tests
        static ProBuilderEditor s_Instance;

        GUIContent[] m_EditModeIcons;
        GUIStyle VertexTranslationInfoStyle;

        [UserSetting("General", "Show Scene Info",
            "Toggle the display of information about selected meshes in the Scene View.")]
        static Pref<bool> s_ShowSceneInfo = new Pref<bool>("editor.showSceneInfo", false);

        [UserSetting("Toolbar", "Icon GUI", "Toggles the ProBuilder window interface between text and icon versions.")]
        internal static Pref<bool> s_IsIconGui = new Pref<bool>("editor.toolbarIconGUI", false);

        [UserSetting("Mesh Editing", "Allow non-manifold actions",
            "Enables advanced mesh editing techniques that may create non-manifold geometry.")]
        internal static Pref<bool> s_AllowNonManifoldActions =
            new Pref<bool>("editor.allowNonManifoldActions", false, SettingsScope.User);

        [UserSetting("Toolbar", "Toolbar Location",
            "Where the Object, Face, Edge, and Vertex toolbar will be shown in the Scene View.")]
        static Pref<SceneToolbarLocation> s_SceneToolbarLocation =
            new Pref<SceneToolbarLocation>("editor.sceneToolbarLocation", SceneToolbarLocation.UpperCenter,
                SettingsScope.User);

        static Pref<bool> s_WindowIsFloating = new Pref<bool>("UnityEngine.ProBuilder.ProBuilderEditor-isUtilityWindow",
            false, SettingsScope.Project);

        static Pref<bool> m_BackfaceSelectEnabled = new Pref<bool>("editor.backFaceSelectEnabled", false);

        static Pref<RectSelectMode> m_DragSelectRectMode =
            new Pref<RectSelectMode>("editor.dragSelectRectMode", RectSelectMode.Partial);

        static Pref<SelectionModifierBehavior> m_SelectModifierBehavior =
            new Pref<SelectionModifierBehavior>("editor.rectSelectModifier", SelectionModifierBehavior.Difference);

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

                if (s_Instance != null)
                    s_Instance.m_ScenePickerPreferences.cullMode = value ? CullingMode.None : CullingMode.Back;
            }
        }

        // used for 'g' key shortcut to swap between object/vef modes
        SelectMode m_LastComponentMode;

        GUIStyle m_CommandStyle;
        Rect m_ElementModeToolbarRect = new Rect(3, 6, 128, 24);

        int m_DefaultControl;
        SceneSelection m_Hovering = new SceneSelection();
        SceneSelection m_HoveringPrevious = new SceneSelection();
        ScenePickerPreferences m_ScenePickerPreferences;

        [UserSetting("Graphics", "Show Hover Highlight", "Highlight the mesh element nearest to the mouse cursor.")]
        static Pref<bool> s_ShowHoverHighlight =
            new Pref<bool>("editor.showPreselectionHighlight", true, SettingsScope.User);

        Vector2 m_InitialMousePosition;
        Rect m_MouseDragRect;
        bool m_IsDragging;

        bool m_IsReadyForMouseDrag;

        // prevents leftClickUp from stealing focus after double click
        bool m_WasDoubleClick;
        // vertex handles

        Vector3[][] m_VertexPositions;
        Vector3[] m_VertexOffset;

        GUIContent m_SceneInfo = new GUIContent();

        Rect m_SceneInfoRect = new Rect(10, 10, 200, 40);

        bool m_wasSelectingPath;

        // All selected pb_Objects
        internal List<ProBuilderMesh> selection
        {
            get { return MeshSelection.topInternal; }
        }

        Event m_CurrentEvent;

        internal bool isFloatingWindow { get; private set; }

        /// <value>
        /// Get and set the current SelectMode.
        /// </value>
        public static SelectMode selectMode
        {
            get
            {
                // for backwards compatibility reasons `Object` is returned when editor is closed
                if (s_Instance != null)
                    return ProBuilderToolManager.selectMode;
                return SelectMode.Object;
            }

            set
            {
                toolManager?.SetSelectMode(value);
            }
        }

        /// <summary>
        /// Set the <see cref="SelectMode"/> to the last used mesh element mode.
        /// </summary>
        public static void ResetToLastSelectMode()
        {
            toolManager?.ResetToLastSelectMode();
            Refresh();
        }

        // used by tests for pre-override tools
        internal static void SyncEditorToolSelectMode()
        {
            toolManager?.ForwardBuiltinToolCheck();
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
            ProBuilderEditor editor = (ProBuilderEditor) GetWindow(typeof(ProBuilderEditor),
                    s_WindowIsFloating, PreferenceKeys.pluginTitle,
                    true); // open as floating window
            editor.isFloatingWindow = s_WindowIsFloating;
        }

        void OnEnable()
        {
            // maximize does this weird crap where it doesn't disable or enable windows in the current layout when
            // entering or exiting maximized mode, but _does_ Enable/Disable the new maximized window instance. when
            // that happens the ProBuilderEditor loses the s_Instance due to that maximized instance taking over.
            // so in order to prevent the problems that occur when multiple instances of ProBuilderEditor, instead
            // ensure that there is always one true instance. we'll also skip initializing what are basically singleton
            // managers as well (ex, tool manager)
            if(s_Instance == null)
                s_Instance = this;

            ProBuilderToolManager.selectModeChanged += OnSelectModeChanged;

            m_Toolbar = new EditorToolbar(this);
            m_ToolManager = s_Instance == this ? new ProBuilderToolManager() : null;

            SceneView.duringSceneGui += OnSceneGUI;
            ProGridsInterface.SubscribePushToGridEvent(PushToGrid);
            ProGridsInterface.SubscribeToolbarEvent(ProGridsToolbarOpen);
            MeshSelection.objectSelectionChanged += OnObjectSelectionChanged;

            ProGridsToolbarOpen(ProGridsInterface.SceneToolbarIsExtended());

            VertexManipulationTool.beforeMeshModification += BeforeMeshModification;
            VertexManipulationTool.afterMeshModification += AfterMeshModification;

            LoadSettings();
            InitGUI();
            EditorApplication.delayCall += () => UpdateSelection();
            SetOverrideWireframe(true);
        }

        void OnDisable()
        {
            VertexManipulationTool.beforeMeshModification -= BeforeMeshModification;
            VertexManipulationTool.afterMeshModification -= AfterMeshModification;

            ClearElementSelection();

            UpdateSelection();

            if (selectionUpdated != null)
                selectionUpdated(null);

            SceneView.duringSceneGui -= OnSceneGUI;
            ProGridsInterface.UnsubscribePushToGridEvent(PushToGrid);
            ProGridsInterface.UnsubscribeToolbarEvent(ProGridsToolbarOpen);
            MeshSelection.objectSelectionChanged -= OnObjectSelectionChanged;

            SetOverrideWireframe(false);
            m_Toolbar.Dispose();
            if(m_ToolManager != null)
                m_ToolManager.Dispose();
            OnSelectModeChanged();
            ProBuilderToolManager.selectModeChanged -= OnSelectModeChanged;

            SceneView.RepaintAll();

            if(s_Instance == this)
                s_Instance = null;
        }

        void OnSelectModeChanged()
        {
            Refresh();
            if (selectModeChanged != null)
                selectModeChanged(ProBuilderToolManager.selectMode);
            Repaint();
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
            EditorApplication.delayCall += EditorHandleDrawing.ResetPreferences;

            m_ScenePickerPreferences = new ScenePickerPreferences()
            {
                cullMode = m_BackfaceSelectEnabled ? CullingMode.None : CullingMode.Back,
                selectionModifierBehavior = m_SelectModifierBehavior,
                rectSelectMode = m_DragSelectRectMode
            };
        }

        void InitGUI()
        {
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
            if(instance != null)
                instance.UpdateSelection(vertexCountChanged);
        }

        void OnGUI()
        {
            if (m_Toolbar.isIconMode != s_IsIconGui.value)
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

                case EventType.KeyUp:
                    if (e.keyCode == KeyCode.Escape)
                    {
                        selectMode = SelectMode.Object;
                        e.Use();
                    }
                    break;
            }

            m_Toolbar.OnGUI();
        }

        void IconModeChanged()
        {
            m_Toolbar.Dispose();
            m_Toolbar = new EditorToolbar(this);
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
                    : (VertexManipulationTool) EditorToolManager.activeTool;
            }
        }

        void OnSceneGUI(SceneView sceneView)
        {
            SceneStyles.Init();

            m_CurrentEvent = Event.current;

            EditorHandleDrawing.DrawSceneHandles(SceneDragAndDropListener.isDragging ? SelectMode.None : selectMode);

            DrawHandleGUI(sceneView);

            if (m_CurrentEvent.type == EventType.KeyDown)
            {
                // Escape isn't assignable as a shortcut
                if (m_CurrentEvent.keyCode == KeyCode.Escape && selectMode != SelectMode.Object)
                {
                    selectMode = SelectMode.Object;

                    m_IsDragging = false;
                    m_IsReadyForMouseDrag = false;

                    m_CurrentEvent.Use();
                }
            }

            if (selectMode == SelectMode.Object)
                return;

            bool pathSelectionModifier = EditorHandleUtility.IsSelectionPathModifier(m_CurrentEvent.modifiers);

            // Check mouse position in scene and determine if we should highlight something
            if (s_ShowHoverHighlight
                && selectMode.IsMeshElementMode()
                && (m_CurrentEvent.type == EventType.MouseMove
                || (m_wasSelectingPath != pathSelectionModifier && m_CurrentEvent.isKey)))
            {
                m_Hovering.CopyTo(m_HoveringPrevious);
                if (GUIUtility.hotControl != 0 ||
                    EditorSceneViewPicker.MouseRayHitTest(m_CurrentEvent.mousePosition, selectMode, m_ScenePickerPreferences, m_Hovering) > ScenePickerPreferences.maxPointerDistance)
                    m_Hovering.Clear();

                if (!m_Hovering.Equals(m_HoveringPrevious))
                {
                    if (pathSelectionModifier)
                        EditorSceneViewPicker.DoMouseHover(m_Hovering);

                    SceneView.RepaintAll();
                }
            }

            m_wasSelectingPath = pathSelectionModifier;

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

             if (EditorHandleUtility.SceneViewInUse(m_CurrentEvent))
             {
                 if(m_IsDragging)
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

            HandleMouseEvent(sceneView, m_DefaultControl);
        }

        internal void HandleMouseEvent(SceneView sceneView, int controlID)
        {
            if(m_CurrentEvent.type == EventType.MouseDown && HandleUtility.nearestControl == controlID)
            {
                // double clicking object
                if(m_CurrentEvent.clickCount > 1)
                {
                    DoubleClick(m_CurrentEvent);
                }

                m_InitialMousePosition = m_CurrentEvent.mousePosition;
                // readyForMouseDrag prevents a bug wherein after ending a drag an errant
                // MouseDrag event is sent with no corresponding MouseDown/MouseUp event.
                m_IsReadyForMouseDrag = true;

                GUIUtility.hotControl = controlID;
            }

            if(m_CurrentEvent.type == EventType.MouseDrag && m_IsReadyForMouseDrag && GUIUtility.hotControl == controlID)
            {
                if(!m_IsDragging && Vector2.Distance(m_CurrentEvent.mousePosition, m_InitialMousePosition) >
                    k_MouseDragThreshold)
                {
                    sceneView.Repaint();
                    m_IsDragging = true;
                }
            }

            if(m_CurrentEvent.type == EventType.Ignore)
            {
                if(m_IsDragging)
                {
                    m_IsReadyForMouseDrag = false;
                    m_IsDragging = false;
                    EditorSceneViewPicker.DoMouseDrag(m_MouseDragRect, selectMode, m_ScenePickerPreferences);
                }

                if(m_WasDoubleClick)
                    m_WasDoubleClick = false;

                if(GUIUtility.hotControl == controlID)
                    GUIUtility.hotControl = 0;
            }

            if(m_CurrentEvent.type == EventType.MouseUp && GUIUtility.hotControl == controlID)
            {
                GUIUtility.hotControl = 0;

                if(m_WasDoubleClick)
                {
                    m_WasDoubleClick = false;
                }
                else
                {
                    if(!m_IsDragging)
                    {
                        if(UVEditor.instance)
                            UVEditor.instance.ResetUserPivot();

                        EditorSceneViewPicker.DoMouseClick(m_CurrentEvent, selectMode, m_ScenePickerPreferences);
                        UpdateSelection();
                    }
                    else
                    {
                        m_IsDragging = false;
                        m_IsReadyForMouseDrag = false;

                        if(UVEditor.instance)
                            UVEditor.instance.ResetUserPivot();

                        EditorSceneViewPicker.DoMouseDrag(m_MouseDragRect, selectMode, m_ScenePickerPreferences);

                        if(GUIUtility.hotControl == controlID)
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
        }

        void DoubleClick(Event e)
        {
            var mesh = EditorSceneViewPicker.DoMouseClick(m_CurrentEvent, selectMode, m_ScenePickerPreferences);

            if (mesh != null)
            {
                if (selectMode.ContainsFlag(SelectMode.Edge | SelectMode.TextureEdge))
                {
                    if (e.shift)
                        EditorUtility.ShowNotification(EditorToolbarLoader.GetInstance<Actions.SelectEdgeRing>().PerformAction());
                    else
                        EditorUtility.ShowNotification(EditorToolbarLoader.GetInstance<Actions.SelectEdgeLoop>().PerformAction());
                }
                else if (selectMode.ContainsFlag(SelectMode.Face | SelectMode.TextureFace))
                {
                    if ((e.modifiers & (EventModifiers.Control | EventModifiers.Shift)) ==
                        (EventModifiers.Control | EventModifiers.Shift))
                        Actions.SelectFaceRing.MenuRingAndLoopFaces(MeshSelection.topInternal);
                    else if (e.control)
                        EditorUtility.ShowNotification(EditorToolbarLoader.GetInstance<Actions.SelectFaceRing>().PerformAction());
                    else if (e.shift)
                        EditorUtility.ShowNotification(EditorToolbarLoader.GetInstance<Actions.SelectFaceLoop>().PerformAction());
                    else
                        mesh.SetSelectedFaces(mesh.facesInternal);
                }
                else
                {
                    mesh.SetSelectedFaces(mesh.facesInternal);
                }

                UpdateSelection();
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
                    EditorHandleDrawing.DrawSceneSelection(m_Hovering);
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

        /// <summary>
        /// Toggles between the SelectMode values and updates the graphic handles as necessary.
        /// </summary>
        internal void ToggleSelectionMode()
        {
            ProBuilderToolManager.NextMeshSelectMode();
            Refresh();
        }

        void UpdateSelection(bool selectionChanged = true)
        {
            UpdateMeshHandles(selectionChanged);

            if (selectionChanged)
                UpdateSceneInfo();

            if (selectionUpdated != null)
                selectionUpdated(selection);

            SceneView.RepaintAll();
        }

        internal static void UpdateMeshHandles(bool selectionOrVertexCountChanged = true)
        {
            if (!s_Instance)
                return;

            try
            {
                EditorHandleDrawing.RebuildSelectedHandles(MeshSelection.topInternal, selectMode);
            }
            catch
            {
                // happens on undo when c++ object is gone but c# isn't in the know
                EditorHandleDrawing.ClearHandles();
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

        void OnObjectSelectionChanged()
        {
            m_Hovering.Clear();
            UpdateSelection();
            SetOverrideWireframe(true);
            Repaint();
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
    }
}

using System;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.EditorTools;
using UnityEngine.ProBuilder;
using UnityEditor.SettingsManagement;
using EditorToolManager = UnityEditor.EditorTools.EditorToolManager;
using ToolManager = UnityEditor.EditorTools.ToolManager;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Manages the [ProBuilder toolbar and tool mode](../manual/toolbar.html).
    /// </summary>
    public sealed class ProBuilderEditor : IDisposable
    {
        // Match the value set in RectSelection.cs
        const float k_MouseDragThreshold = 6f;

        /// <summary>
        /// Raised any time the ProBuilder editor refreshes the selection. This is called every frame when interacting with mesh elements, and after any mesh operation.
        /// </summary>
        public static event Action<IEnumerable<ProBuilderMesh>> selectionUpdated;

        /// <summary>
        /// Raised when the EditLevel changes.
        /// </summary>
        public static event Action<SelectMode> selectModeChanged;

        /// <summary>
        /// Raised when vertex modifications are complete.
        /// </summary>
        public static event Action<IEnumerable<ProBuilderMesh>> afterMeshModification;

        /// <summary>
        /// Raised immediately prior to beginning vertex modifications, when the ProBuilderMesh is in un-altered state. This is after
        /// <see cref="ProBuilderMesh.ToMesh"/> and <see cref="ProBuilderMesh.Refresh"/> have been called, but before <see cref="EditorMeshUtility.Optimize"/>.
        /// </summary>
        public static event Action<IEnumerable<ProBuilderMesh>> beforeMeshModification;

        static ProBuilderEditor s_Instance;

        GUIStyle VertexTranslationInfoStyle;

        [UserSetting("General", "Show Scene Info",
            "Toggle the display of information about selected meshes in the Scene View.")]
        internal static Pref<bool> s_ShowSceneInfo = new Pref<bool>("editor.showSceneInfo", false);

        [UserSetting("Mesh Editing", "Allow non-manifold actions",
            "Enables advanced mesh editing techniques that may create non-manifold geometry.")]
        internal static Pref<bool> s_AllowNonManifoldActions =
            new Pref<bool>("editor.allowNonManifoldActions", false, SettingsScope.User);

        static Pref<bool> s_BackfaceSelectEnabled = new Pref<bool>("editor.backFaceSelectEnabled", false);

        static Pref<RectSelectMode> s_DragSelectRectMode =
            new Pref<RectSelectMode>("editor.dragSelectRectMode", RectSelectMode.Partial);

        internal static event Action rectSelectModeChanged;
        internal static RectSelectMode rectSelectMode
        {
            get { return s_DragSelectRectMode.value; }

            set
            {
                if (s_DragSelectRectMode.value == value)
                    return;

                s_DragSelectRectMode.SetValue(value, true);
                if(rectSelectModeChanged != null)
                    rectSelectModeChanged();

                if (s_Instance != null)
                    s_Instance.m_ScenePickerPreferences.rectSelectMode = value;
            }
        }

        internal static event Action backfaceSelectionEnabledChanged;
        internal static bool backfaceSelectionEnabled
        {
            get { return s_BackfaceSelectEnabled.value; }

            set
            {
                if (value == s_BackfaceSelectEnabled.value)
                    return;

                s_BackfaceSelectEnabled.SetValue(value, true);
                if(backfaceSelectionEnabledChanged != null)
                    backfaceSelectionEnabledChanged();

                if (s_Instance != null)
                    s_Instance.m_ScenePickerPreferences.cullMode = value ? CullingMode.None : CullingMode.Back;
            }
        }

        // used for 'g' key shortcut to swap between object/vef modes
        SelectMode m_LastComponentMode;

        GUIStyle m_CommandStyle;

        int m_DefaultControl;
        SceneSelection m_Hovering = new SceneSelection();
        internal SceneSelection hovering => m_Hovering;
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

        bool m_wasSelectingPath;

        // All selected pb_Objects
        internal List<ProBuilderMesh> selection
        {
            get { return MeshSelection.topInternal; }
        }

        Event m_CurrentEvent;

        static Pref<SelectMode> s_SelectMode = new Pref<SelectMode>(nameof(s_SelectMode), SelectMode.Face);
        /// <summary>
        /// Gets and sets the current <see cref="SelectMode"/> value.
        /// </summary>
        public static SelectMode selectMode
        {
            get => s_SelectMode;

            set
            {
                if (s_SelectMode != value)
                {
                    s_SelectMode.SetValue(value);
                    selectModeChanged?.Invoke(value);
                }
            }
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
        
        /// <summary>
        /// Static getter for the ProBuilderEditor instance.
        /// </summary>
        /// <value>
        /// Static instance of ProBuilderEditor.
        /// </value>
        public static ProBuilderEditor instance => s_Instance;

        internal ProBuilderEditor()
        {
            if(s_Instance != null)
                s_Instance.Dispose();

            s_Instance = this;

            SceneView.duringSceneGui += OnSceneGUI;
            ProGridsInterface.SubscribePushToGridEvent(PushToGrid);
            MeshSelection.objectSelectionChanged += OnObjectSelectionChanged;
            selectModeChanged += OnSelectModeChanged;

            VertexManipulationTool.beforeMeshModification += BeforeMeshModification;
            VertexManipulationTool.afterMeshModification += AfterMeshModification;

            LoadSettings();
            InitGUI();
            UpdateMeshHandles();
            SetOverrideWireframe(true);
            EditorApplication.delayCall += () => UpdateSelection();
        }

        /// <summary>
        /// Clears static state and unsubscribes from events.
        /// </summary>
        public void Dispose()
        {
            VertexManipulationTool.beforeMeshModification -= BeforeMeshModification;
            VertexManipulationTool.afterMeshModification -= AfterMeshModification;
            selectModeChanged -= OnSelectModeChanged;

            SceneView.duringSceneGui -= OnSceneGUI;
            ProGridsInterface.UnsubscribePushToGridEvent(PushToGrid);
            MeshSelection.objectSelectionChanged -= OnObjectSelectionChanged;

            SetOverrideWireframe(false);
            OnSelectModeChanged(SelectMode.None);
            SceneView.RepaintAll();

            if(s_Instance == this)
                s_Instance = null;
        }

        void OnSelectModeChanged(SelectMode obj)
        {
            Refresh();
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
                cullMode = s_BackfaceSelectEnabled ? CullingMode.None : CullingMode.Back,
                rectSelectMode = s_DragSelectRectMode
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
        /// Rebuilds the mesh wireframe and selection caches.
        /// </summary>
        /// <param name="vertexCountChanged">True if the number of vertices changed, which is the default value.</param>
        public static void Refresh(bool vertexCountChanged = true)
        {
            instance?.UpdateSelection(vertexCountChanged);
        }
        
        /// <summary>
        /// Called when handling events in Scene view.
        /// </summary>
        /// <param name="sceneView">SceneView for which OnSceneGUI method is called.</param>
        public void OnSceneGUI(SceneView sceneView)
        {
            if (!EditorToolUtility.IsBuiltinOverride(EditorToolManager.activeTool))
                return;

            SceneStyles.Init();

            m_CurrentEvent = Event.current;

            EditorHandleDrawing.DrawSceneHandles(SceneDragAndDropListener.isDragging ? SelectMode.None : selectMode);

            DrawHandleGUI(sceneView);

            // escape isn't assignable as a shortcut, and we want it to exit the tool context
            if (m_CurrentEvent.type == EventType.KeyDown)
            {
                if (m_CurrentEvent.keyCode == KeyCode.Escape
                    && EditorToolManager.activeOverride == null
                    && ToolManager.activeContextType == typeof(PositionToolContext))
                {
                    ToolManager.SetActiveContext<GameObjectToolContext>();

                    m_IsDragging = false;
                    m_IsReadyForMouseDrag = false;

                    m_CurrentEvent.Use();
                }
            }

            bool pathSelectionModifier = EditorPathSelectionUtility.IsSelectionPathModifier(m_CurrentEvent.modifiers);

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

                if (pathSelectionModifier)
                    EditorSceneViewPicker.DoMouseHover(m_Hovering);

                SceneView.RepaintAll();
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

        internal void ResetSceneGUIEvent()
        {
            if(GUIUtility.hotControl == m_DefaultControl)
            {
                GUIUtility.hotControl = 0;

                m_WasDoubleClick = false;
                m_IsDragging = false;
                m_IsReadyForMouseDrag = false;
            }
            Refresh();
        }

        internal void HandleMouseEvent(SceneView sceneView, int controlID)
        {
            if ((Event.current.modifiers & EventModifiers.Alt) == EventModifiers.Alt && !m_IsDragging)
                return;

            bool isLeftMouseClick = m_CurrentEvent.type == EventType.MouseDown && m_CurrentEvent.button == 0 && HandleUtility.nearestControl == controlID;
            if (isLeftMouseClick)
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
            if (sceneView != SceneView.lastActiveSceneView || instance == null)
                return;

            if (m_CurrentEvent.type == EventType.Repaint
                && !SceneDragAndDropListener.isDragging
                && m_Hovering != null
                && GUIUtility.hotControl == 0
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
                        HandleUtility.Repaint();
                }
            }
        }

        /// <summary>
        /// Toggles between the SelectMode values and updates the graphic handles as necessary.
        /// </summary>
        internal void ToggleSelectionMode()
        {
            if (selectMode == SelectMode.None)
                return;

            switch (selectMode)
            {
                case SelectMode.Vertex:
                    selectMode = SelectMode.Edge;
                    break;
                case SelectMode.Edge:
                    selectMode = SelectMode.Face;
                    break;
                case SelectMode.Face:
                    selectMode = SelectMode.Vertex;
                    break;
            }
        }

        void UpdateSelection(bool selectionChanged = true)
        {
            UpdateMeshHandles();

            if (selectionUpdated != null)
                selectionUpdated(selection);

            SceneView.RepaintAll();
        }

        internal static void UpdateMeshHandles()
        {
            if (s_Instance == null)
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
    }
}

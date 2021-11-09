#if UNITY_2021_2_OR_NEWER
#define OVERLAYS_AVAILABLE
#endif
using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
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
    class SelectionGUI
    {
#if OVERLAYS_AVAILABLE
        /// <value>
        /// Raised any time the ProBuilder editor refreshes the selection. This is called every frame when interacting with mesh elements, and after any mesh operation.
        /// </value>
        public static event Action<IEnumerable<ProBuilderMesh>> selectionUpdated;
#endif

        ProBuilderToolManager m_ToolManager; // never use this directly! use toolManager getter to avoid problems with multiple editor instances
        internal static ProBuilderToolManager toolManager => s_Instance != null ? s_Instance.m_ToolManager : null;

        // Match the value set in RectSelection.cs
        const float k_MouseDragThreshold = 6f;

        static SelectionGUI s_Instance;
        public static SelectionGUI instance
        {
            get { return s_Instance; }
        }

        Event m_CurrentEvent;

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

        GUIContent m_SceneInfo = new GUIContent();
        Rect m_SceneInfoRect = new Rect(10, 10, 200, 40);

        bool m_wasSelectingPath;

        // All selected pb_Objects
        internal List<ProBuilderMesh> selection
        {
            get { return MeshSelection.topInternal; }
        }

        int m_DefaultControl;
        SceneSelection m_Hovering = new SceneSelection();
        SceneSelection m_HoveringPrevious = new SceneSelection();
        ScenePickerPreferences m_ScenePickerPreferences;

        [UserSetting("General", "Show Scene Info",
            "Toggle the display of information about selected meshes in the Scene View.")]
        static Pref<bool> s_ShowSceneInfo = new Pref<bool>("editor.showSceneInfo", false);

        [UserSetting("Graphics", "Show Hover Highlight", "Highlight the mesh element nearest to the mouse cursor.")]
        static Pref<bool> s_ShowHoverHighlight =
            new Pref<bool>("editor.showPreselectionHighlight", true, SettingsScope.User);

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

        Vector2 m_InitialMousePosition;
        Rect m_MouseDragRect;
        bool m_IsDragging;
        bool m_IsReadyForMouseDrag;

        // prevents leftClickUp from stealing focus after double click
        bool m_WasDoubleClick;

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

        public SelectionGUI(PositionToolContext toolContext)
        {
            if(s_Instance == null)
                s_Instance = this;

            toolContext.objectSelectionChanged += OnObjectSelectionChanged;
            LoadSettings();
        }

        public void OnSceneGUI(SceneView sceneView)
        {
            SceneStyles.Init();

            m_CurrentEvent = Event.current;

            // REMOVE: Draws top wireframe
            EditorHandleDrawing.DrawSceneHandles(SceneDragAndDropListener.isDragging ? SelectMode.None : selectMode);

            // REMOVE: Draws selection highlights/gizmos (vertices, edges, faces).
            DrawHandleGUI(sceneView);

            // REMOVE: switches back to Object mode on ESC hit
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

            // REMOVE: early exit if we're in object select mode
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
                                MeshSelection.SelectAll(selectMode);
                            }
                            m_CurrentEvent.Use();
                            break;
                        case "DeselectAll":
                            if (execute)
                            {
                                MeshSelection.DeselectAll(selectMode);
                            }
                            m_CurrentEvent.Use();
                            break;
                        case "InvertSelection":
                            if (execute)
                            {
                                MeshSelection.InvertSelection(selectMode);
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

        void DrawHandleGUI(SceneView sceneView)
        {
            if (sceneView != SceneView.lastActiveSceneView)
                return;

            Debug.Log($"DrawHandleGUI: selectMode.IsMeshElementMode{selectMode.IsMeshElementMode()} " +
                      $"HandleUtility.nearestControl {HandleUtility.nearestControl}");

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

        /// <summary>
        /// Rebuild the mesh wireframe and selection caches.
        /// </summary>
        public static void Refresh(bool vertexCountChanged = true)
        {
            if(instance != null)
                instance.UpdateSelection(vertexCountChanged);
        }

        internal void UpdateSelection(bool selectionChanged = true)
        {
            UpdateMeshHandles(selectionChanged);

            if (selectionChanged)
                UpdateSceneInfo();

            if (selectionUpdated != null)
                selectionUpdated(selection);

            SceneView.RepaintAll();
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
        }

        internal static void UpdateMeshHandles(bool selectionOrVertexCountChanged = true)
        {
            if (s_Instance == null)
                return;

            try
            {
#if OVERLAYS_AVAILABLE
                EditorHandleDrawing.RebuildSelectedHandles(MeshSelection.topInternal);
#else
                EditorHandleDrawing.RebuildSelectedHandles(MeshSelection.topInternal, selectMode);
#endif
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
    }
}


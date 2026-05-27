using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
using Edge = UnityEngine.ProBuilder.Edge;
using Math = UnityEngine.ProBuilder.Math;
using UObject = UnityEngine.Object;
using RaycastHit = UnityEngine.ProBuilder.RaycastHit;
using UHandleUtility = UnityEditor.HandleUtility;

using ToolManager = UnityEditor.EditorTools.ToolManager;
using Vertex = UnityEngine.ProBuilder.Vertex;

namespace UnityEditor.ProBuilder
{
    [EditorTool("Cut Tool", typeof(ProBuilderMesh), typeof(PositionToolContext))]
    partial class CutTool : EditorTool
    {
        ProBuilderMesh m_Mesh;

        Texture2D m_CutCursorTexture;
        Texture2D m_CutAddCursorTexture;
        Texture2D m_CurrentCutCursor = null;

        /// <summary>
        /// Describes the different vertex types on the path.
        /// </summary>
        [Flags]
        public enum VertexTypes
        {
            None = 0x0,
            NewVertex = 1 << 0,
            AddedOnEdge = 1 << 1,
            ExistingVertex = 1 << 2,
            VertexInShape = 1 << 3,
        }

        [Serializable]
        internal struct CutVertexData
        {
            [SerializeField] Vector3 m_Position;
            [SerializeField] Vector3 m_Normal;
            [SerializeField] VertexTypes m_Types;

            public Vector3 position
            {
                get => m_Position;
                set => m_Position = value;
            }

            public Vector3 normal
            {
                get => m_Normal;
            }

            public VertexTypes types
            {
                get => m_Types;
            }

            public CutVertexData(Vector3 position, Vector3 normal, VertexTypes types = VertexTypes.None)
            {
                m_Position = position;
                m_Normal = normal;
                m_Types = types;
            }
        }

        static readonly Color k_HandleColor = new Color(.8f, .8f, .8f, 1f);
        static readonly Color k_HandleColorAddNewVertex = new Color(.01f, .9f, .3f, 1f);
        static Color s_HandleColorAddVertexOnEdge = new Color(.3f, .01f, .9f, 1f);
        static Color s_HandleColorUseExistingVertex = new Color(.01f, .5f, 1f, 1f);
        static readonly Color k_HandleColorModifyVertex = new Color(1f, .75f, .0f, 1f);
        const float k_HandleSize = .05f;

        static readonly Color k_LineColor = new Color(0f, 55f / 255f, 1f, 1f);
        static readonly Color k_InvalidLineColor = Color.red;
        static readonly Color k_ConnectionsLineColor = new Color(0f, 200f / 255f, 170f / 200f, 1f);
        static readonly Color k_DrawingLineColor = new Color(0.01f, .9f, 0.3f, 1f);

        Color m_CurrentHandleColor = k_HandleColor;

        GUIContent m_IconContent;
        public override GUIContent toolbarIcon
        {
            get { return m_IconContent; }
        }

        //Handles and point placement
        int m_ControlId;
        bool m_PlacingPoint;
        internal bool m_SnappingPoint;
        bool m_ModifyingPoint; // State machine instead? //Status
        int m_SelectedIndex = -2;
        bool m_IsCutValid = true;
        bool m_Dirty = false;

        //Cut tool elements
        internal Face m_TargetFace;
        internal Face m_CurrentFace;
        internal Vector3 m_CurrentPosition = Vector3.positiveInfinity;
        internal Vector3 m_CurrentPositionNormal = Vector3.up;
        internal VertexTypes m_CurrentVertexTypes = VertexTypes.None;
        IList<Edge> m_SelectedEdges = null;
        IList<int> m_SelectedVertices = null;

        //Path composed of position that define a cut in the face
        [SerializeField]
        internal List<CutVertexData> m_CutPath = new List<CutVertexData>();
        //Connection between the path and the mesh vertices to get a 'safe' cut
        internal List<SimpleTuple<int, int>> m_MeshConnections = new List<SimpleTuple<int, int>>();

        //Snapping
        int m_SnapedVertexId = -1;
        Edge m_SnapedEdge = Edge.Empty;

        bool m_SnapToGeometry;
        bool m_SnapToGrid;
        float m_SnappingDistance;

        //Overlay fields
        GUIContent m_OverlayTitle;
        const string k_SnapToGeometryPrefKey = "VertexInsertion.snapToGeometry";
        const string k_SnappingDistancePrefKey = "VertexInsertion.snappingDistance";

        //Rectangle mode fields
        bool m_RectangleMode;
        Vector3 m_RectStartPoint = Vector3.positiveInfinity;
        Vector3 m_RectEndPoint = Vector3.positiveInfinity;
        bool m_RectDragging;
        const string k_SnapToGridPrefKey = "VertexInsertion.snapToGrid";
        const string k_RectangleModePrefKey = "VertexInsertion.rectangleMode";
        static readonly Color k_RectPreviewColor = new Color(1f, 1f, 0f, 0.4f);
        static readonly Color k_RectOutlineColor = new Color(1f, 1f, 0f, 1f);
        readonly Vector3[] m_RectConvexPolygon = new Vector3[4];
        readonly Vector3[] m_RectPreviewPath = new Vector3[5];

        public bool isALoop
        {
            get
            {
                if (m_CutPath.Count < 3)
                    return false;

                return Math.Approx3(m_CutPath[0].position, m_CutPath[m_CutPath.Count - 1].position);
            }
        }

        public int connectionsToBordersCount
        {
            get
            {
                return m_CutPath.Count(data => (data.types & (VertexTypes.AddedOnEdge | VertexTypes.ExistingVertex)) != 0
                                                    && (data.types & VertexTypes.VertexInShape) == 0);
            }
        }

        /// <summary>
        /// Update the mouse cursor depending on the tool status
        /// </summary>
        /// <returns>the texture to use as a cursor</returns>
        Texture2D cursorTexture
        {
            get
            {
                if(m_CutPath.Count > 0)
                    return m_CutAddCursorTexture;

                return m_CutCursorTexture;
            }
        }

        public override bool gridSnapEnabled => true;

        public override bool IsAvailable()
        {
            return MeshSelection.selectedObjectCount == 1;
        }

        void OnEnable()
        {
            m_IconContent = new GUIContent()
            {
                image = IconUtility.GetIcon("Toolbar/CutTool"),
                text = "Cut Tool",
                tooltip = "Cut Tool"
            };

            s_HandleColorUseExistingVertex = Handles.selectedColor;
            s_HandleColorAddVertexOnEdge = Handles.selectedColor;

            m_OverlayTitle = new GUIContent("Cut Settings");
            m_SnapToGeometry = EditorPrefs.GetBool( k_SnapToGeometryPrefKey, false );
            m_SnapToGrid = EditorPrefs.GetBool( k_SnapToGridPrefKey, false );
            m_SnappingDistance = EditorPrefs.GetFloat( k_SnappingDistancePrefKey, 0.1f );
            m_RectangleMode = EditorPrefs.GetBool( k_RectangleModePrefKey, false );

            m_CutCursorTexture = IconUtility.GetIcon("Cursors/cutCursor");
            m_CutAddCursorTexture = IconUtility.GetIcon("Cursors/cutCursor-add");

            m_Mesh = null;
        }

        public override void OnActivated()
        {
            if(MeshSelection.selectedObjectCount == 1)
            {
                m_Mesh = MeshSelection.activeMesh;
                m_Mesh.ClearSelection();
                m_SelectedVertices = m_Mesh.sharedVertexLookup.Keys.ToArray();
                m_SelectedEdges = m_Mesh.faces.SelectMany(f => f.edges).Distinct().ToArray();
            }
            Undo.undoRedoPerformed += UndoRedoPerformed;
            MeshSelection.objectSelectionChanged += UpdateTarget;
            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;
        }

        public override void OnWillBeDeactivated()
        {
            if(!m_RectangleMode && m_TargetFace != null && m_CutPath.Count > 1)
                ExecuteCut(false);

            Undo.undoRedoPerformed -= UndoRedoPerformed;
            MeshSelection.objectSelectionChanged -= UpdateTarget;
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
        }

        /// <summary>
        /// Clear all data from the cut tool
        /// </summary>
        void Clear()
        {
            ResetToolState(false);
        }

        void ResetToolState(bool keepTarget)
        {
            if (!keepTarget)
                m_Mesh = null;

            m_TargetFace = null;
            m_CurrentFace = null;
            m_PlacingPoint = false;
            m_SnappingPoint = false;
            m_ModifyingPoint = false;
            m_CurrentCutCursor = null;
            m_CurrentPosition = Vector3.positiveInfinity;
            m_CurrentPositionNormal = Vector3.up;
            m_CurrentVertexTypes = VertexTypes.None;
            m_CutPath.Clear();
            m_MeshConnections.Clear();
            m_SnapedVertexId = -1;
            m_SnapedEdge = Edge.Empty;
            m_SelectedIndex = -2;
            m_Dirty = false;

            m_RectDragging = false;
            m_RectStartPoint = Vector3.positiveInfinity;
            m_RectEndPoint = Vector3.positiveInfinity;

            if (keepTarget && m_Mesh != null)
            {
                m_SelectedVertices = m_Mesh.sharedVertexLookup.Keys.ToArray();
                m_SelectedEdges = m_Mesh.faces.SelectMany(f => f.edges).Distinct().ToArray();
            }
            else
            {
                m_SelectedVertices = null;
                m_SelectedEdges = null;
            }

            EditorHandleDrawing.ClearHandles();

            ProBuilderEditor.Refresh();
        }

        /// <summary>
        /// Exit the tool and restore the previous tool
        /// </summary>
        void ExitTool()
        {
            Clear();
            ToolManager.RestorePreviousTool();
        }

        /// <summary>
        /// Undo/Redo callback: Reset and recompute lines, and update the targeted face if needed
        /// </summary>
        void UndoRedoPerformed()
        {
            if(m_CutPath.Count == 0 && m_Mesh != null)
            {
                m_TargetFace = null;

                m_SelectedVertices = m_Mesh.sharedVertexLookup.Keys.ToArray();
                m_SelectedEdges = m_Mesh.faces.SelectMany(f => f.edges).Distinct().ToArray();
            }

            m_SelectedIndex = -1;
            m_MeshConnections.Clear();

            m_Dirty = true;
        }

        void OnSelectModeChanged(SelectMode mode)
        {
            ToolManager.RestorePreviousPersistentTool();
        }

        /// <summary>
        /// Update the mesh targeted by the tool.
        /// This is used when the mesh selection is changed to refresh the tool.
        /// </summary>
        internal void UpdateTarget()
        {
            if(MeshSelection.activeMesh != m_Mesh)
            {
                Clear();
                if(MeshSelection.selectedObjectCount == 1)
                {
                    m_Mesh = MeshSelection.activeMesh;
                    m_Mesh.ClearSelection();

                    m_SelectedVertices = m_Mesh.sharedVertexLookup.Keys.ToArray();
                    m_SelectedEdges = m_Mesh.faces.SelectMany(f => f.edges).Distinct().ToArray();

                    if(m_CutPath.Count > 0)
                    {
                        m_CutPath.Clear();
                        m_MeshConnections.Clear();
                    }
                }
            }
        }

        /// <summary>
        /// Main GUI update for the tool, calls every secondary methods to place points, update lines and compute the cut
        /// </summary>
        /// <param name="window">current window calling the tool : SceneView</param>
        public override void OnToolGUI( EditorWindow window )
        {
        // todo refactor overlays to use `Overlay` class
#pragma warning disable 618
            SceneViewOverlay.Window( m_OverlayTitle, OnOverlayGUI, 0, SceneViewOverlay.WindowDisplayOption.OneWindowPerTitle );
#pragma warning restore 618

            var currentEvent = Event.current;
            if (currentEvent.type == EventType.KeyDown)
                HandleKeyEvent(currentEvent);

            if(currentEvent.type == EventType.Repaint && m_Mesh != null)
            {
                DoExistingLinesGUI();
                DoExistingPointsGUI();
            }

            if (EditorHandleUtility.SceneViewInUse(currentEvent))
                return;

            if(m_Mesh != null)
            {
                m_ControlId = GUIUtility.GetControlID(FocusType.Passive);
                if(currentEvent.type == EventType.Layout)
                    HandleUtility.AddDefaultControl(m_ControlId);

                if(m_RectangleMode)
                    DoRectanglePlacement(window);
                else
                    DoPointPlacement(window);

                //Refresh the cut shape if points have been added to it or removed.
                if(m_Dirty)
                    RebuildCutShape();

                if(currentEvent.type == EventType.Repaint)
                {
                    if(m_RectangleMode)
                        DoRectanglePreview();
                    else
                        DrawGuideLine();

                    DoCurrentPointsGUI();
                    DoVisualCues();

                    Cursor.SetCursor(m_CurrentCutCursor, Vector2.zero, CursorMode.Auto);
                    if(m_CurrentCutCursor != null)
                    {
                        Rect sceneViewRect = window.position;
                        sceneViewRect.x = 0;
                        sceneViewRect.y = 0;
                        SceneView.AddCursorRect(sceneViewRect, MouseCursor.CustomCursor);
                    }
                }
            }
        }

        bool IsCursorInSceneView(EditorWindow window)
        {
            if (!(window is SceneView sceneView))
                return false;

            var mousePos = Event.current.mousePosition;
            mousePos = VisualElementExtensions.LocalToWorld(sceneView.cameraViewVisualElement, mousePos);

            var ve = window.rootVisualElement.panel.Pick(mousePos);
            if (ve == null)
                return false;

            return (ve == sceneView.cameraViewVisualElement);
        }


        /// <summary>
        /// Handle key events
        /// </summary>
        /// <param name="evt">the current event to check</param>
        void HandleKeyEvent(Event evt)
        {
            KeyCode key = evt.keyCode;

            switch (key)
            {
                case KeyCode.Backspace:
                {
                    if (m_CutPath.Count > 0)
                    {
                        UndoUtility.RecordObject(m_Mesh, "Delete Selected Points");
                        m_CutPath.RemoveAt(m_CutPath.Count - 1);
                        m_Dirty = true;
                    }

                    evt.Use();
                    break;
                }

                case KeyCode.Escape:
                    evt.Use();
                    ExitTool();
                    break;

                case KeyCode.KeypadEnter:
                case KeyCode.Return:
                case KeyCode.Space:
                    evt.Use();
                    ExecuteCut();
                    ExitTool();
                    break;
            }
        }

        /// <summary>
        /// Compute the placement of the designated position, this method takes into account the snapping option
        /// And check as well if the user is moving existing positions of the cut path.
        /// The method is also in charge to add the new positions to the CutPath on user clicks
        /// </summary>
        void DoPointPlacement(EditorWindow window)
        {
            Event evt = Event.current;
            EventType evtType = evt.type;

            m_SnappingPoint = m_SnapToGeometry || (evt.modifiers & EventModifiers.Control) != 0;
            m_ModifyingPoint = evt.shift;

            bool hasHitPosition = UpdateHitPosition();

            //Updating visual helpers to get the right position and color to help in the placement
            if (evtType == EventType.Repaint)
            {
                if(!m_SnappingPoint &&
                   !m_ModifyingPoint &&
                   !m_PlacingPoint)
                    m_SelectedIndex = -1;

                if (hasHitPosition && IsCursorInSceneView(window))
                {
                    m_CurrentCutCursor = cursorTexture;
                    if( (m_CurrentVertexTypes & (VertexTypes.ExistingVertex | VertexTypes.VertexInShape)) != 0)
                        m_CurrentHandleColor = m_ModifyingPoint ? k_HandleColorModifyVertex : s_HandleColorUseExistingVertex;
                    else if ((m_CurrentVertexTypes & VertexTypes.AddedOnEdge) != 0)
                        m_CurrentHandleColor = s_HandleColorAddVertexOnEdge;
                    else
                    {
                        m_CurrentHandleColor = k_HandleColorAddNewVertex;
                        if(!m_PlacingPoint)
                            m_SelectedIndex = -1;
                    }
                }
                else
                {
                    m_CurrentCutCursor = null;
                    m_CurrentPosition = Vector3.positiveInfinity;
                    m_CurrentVertexTypes = VertexTypes.None;
                    m_CurrentHandleColor = k_HandleColor;
                }
            }

            //If the user is moving an existing point
            if (m_PlacingPoint || m_ModifyingPoint)
            {
                if( evtType == EventType.MouseDown && evt.button == 0
                    && HandleUtility.nearestControl == m_ControlId )
                {
                    m_PlacingPoint = true;
                }

                if (evtType == EventType.MouseDrag)
                {
                    if (hasHitPosition && m_SelectedIndex >= 0)
                    {
                        evt.Use();
                        CutVertexData data = m_CutPath[m_SelectedIndex];
                        data.position = m_CurrentPosition;
                        m_CutPath[m_SelectedIndex] = data;
                        m_Dirty = true;
                    }
                }

                if (evtType == EventType.MouseUp ||
                    evtType == EventType.Ignore ||
                    evtType == EventType.KeyDown ||
                    evtType == EventType.KeyUp)
                {
                    evt.Use();
                    m_PlacingPoint = false;
                    m_SelectedIndex = -1;
                }
            }
            //If the user is adding the current position to the cut.
            else if (hasHitPosition
                     && evtType == EventType.MouseDown && evt.button == 0
                     && HandleUtility.nearestControl == m_ControlId)
            {
                if(CanAppendCurrentPointToPath())
                {
                    AddCurrentPositionToPath();
                    evt.Use();
                }
            }

            //If nothing in the current cut, then pass the mouse event to ProBuilder Editor to handle selection.
            //This might disable the tool depending on the new selection.
            if (TryPassThroughSelection(window, hasHitPosition))
                return;
        }

        internal void AddCurrentPositionToPath(bool optimize = true)
        {
            UndoUtility.RecordObject(this, "Add Vertex On Path");

            if(m_TargetFace == null)
            {
                m_TargetFace = m_CurrentFace;

                var edges = m_TargetFace.edges;
                m_SelectedVertices = edges.Select(e => e.a).ToArray();
                m_SelectedEdges = edges.ToArray();
            }

            m_CutPath.Add(new CutVertexData(m_CurrentPosition, m_CurrentPositionNormal, m_CurrentVertexTypes));
            m_PlacingPoint = true;
            m_SelectedIndex = m_CutPath.Count - 1;

            RebuildCutShape(optimize);
        }
    }
}

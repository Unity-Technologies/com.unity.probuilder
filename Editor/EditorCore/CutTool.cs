using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Edge = UnityEngine.ProBuilder.Edge;
using Math = UnityEngine.ProBuilder.Math;
using UObject = UnityEngine.Object;
using RaycastHit = UnityEngine.ProBuilder.RaycastHit;
using UHandleUtility = UnityEditor.HandleUtility;

#if UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.ToolManager;
#else
using ToolManager = UnityEditor.EditorTools.EditorTools;
#endif

namespace UnityEditor.ProBuilder
{
    internal class CutTool : EditorTool
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
            None = 0 << 0,
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
        float m_SnappingDistance;

        //Overlay fields
        GUIContent m_OverlayTitle;
        const string k_SnapToGeometryPrefKey = "VertexInsertion.snapToGeometry";
        const string k_SnappingDistancePrefKey = "VertexInsertion.snappingDistance";

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
            m_SnappingDistance = EditorPrefs.GetFloat( k_SnappingDistancePrefKey, 0.1f );

            m_CutCursorTexture = IconUtility.GetIcon("Cursors/cutCursor");
            m_CutAddCursorTexture = IconUtility.GetIcon("Cursors/cutCursor-add");

            Undo.undoRedoPerformed += UndoRedoPerformed;
            if(MeshSelection.selectedObjectCount == 1)
            {
                m_Mesh = MeshSelection.activeMesh;
                m_Mesh.ClearSelection();

                m_SelectedVertices = m_Mesh.sharedVertexLookup.Keys.ToArray();
                m_SelectedEdges = m_Mesh.faces.SelectMany(f => f.edges).Distinct().ToArray();
            }
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedoPerformed;

            ExecuteCut(false);
            Clear();
        }

        /// <summary>
        /// Clear all data from the cut tool
        /// </summary>
        void Clear()
        {
            m_Mesh = null;
            m_TargetFace = null;
            m_CurrentFace = null;
            m_PlacingPoint = false;
            m_CurrentCutCursor = null;
            m_CutPath.Clear();
            m_MeshConnections.Clear();

            m_SelectedVertices = null;
            m_SelectedEdges = null;

            EditorHandleDrawing.ClearHandles();

            ProBuilderEditor.Refresh();
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
        /// Creates a toggle for cut tool overlays.
        /// </summary>
        /// <param name="label">toggle title</param>
        /// <param name="val">starting value for the toggle</param>
        /// <returns>new toggle value</returns>
        bool DoOverlayToggle(string label, bool val)
        {
            using(new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(225));
                GUILayout.FlexibleSpace();
                return EditorGUILayout.Toggle(val);
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

                DoPointPlacement();

                //Refresh the cut shape if points have been added to it or removed.
                if(m_Dirty)
                    RebuildCutShape();

                if(currentEvent.type == EventType.Repaint)
                {
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

        /// <summary>
        /// Overlay GUI
        /// </summary>
        /// <param name="target">the target of this overlay</param>
        /// <param name="view">the current SceneView where to display the overlay</param>
        void OnOverlayGUI(UObject target, SceneView view)
        {
            if(MeshSelection.selectedObjectCount != 1)
            {
                var rect = EditorGUILayout.GetControlRect(false, 45, GUILayout.Width(250));
                EditorGUI.HelpBox(rect, L10n.Tr("One and only one ProBuilder mesh must be selected."), MessageType.Warning);
            }

            m_SnapToGeometry = DoOverlayToggle(L10n.Tr("Snap to existing edges and vertices"), m_SnapToGeometry);
            EditorPrefs.SetBool(k_SnapToGeometryPrefKey, m_SnapToGeometry);

            if(!m_SnapToGeometry)
                GUI.enabled = false;
            EditorGUI.indentLevel++;
            using(new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(L10n.Tr("Snapping distance"), GUILayout.Width(200));
                m_SnappingDistance = EditorGUILayout.FloatField(m_SnappingDistance);
                EditorPrefs.SetFloat( k_SnappingDistancePrefKey, m_SnappingDistance);
            }
            EditorGUI.indentLevel--;

            GUI.enabled = true;

            if(MeshSelection.selectedObjectCount != 1)
                GUI.enabled = false;

            using(new GUILayout.HorizontalScope())
            {
                if(m_Mesh == null)
                {
                    if(GUILayout.Button(EditorGUIUtility.TrTextContent("Start")))
                        UpdateTarget();

                    if(GUILayout.Button(EditorGUIUtility.TrTextContent("Quit")))
                    {
                        Clear();
                        ToolManager.RestorePreviousTool();
                    }
                }
                else
                {
                    if(GUILayout.Button(EditorGUIUtility.TrTextContent("Complete")))
                        ExecuteCut();

                    if(GUILayout.Button(EditorGUIUtility.TrTextContent("Cancel")))
                    {
                        Clear();
                        ToolManager.RestorePreviousTool();
                    }
                }
            }

            GUI.enabled = true;
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
                    Clear();
                    break;

                case KeyCode.KeypadEnter:
                case KeyCode.Return:
                case KeyCode.Space:
                    evt.Use();
                    ExecuteCut();
                    ToolManager.RestorePreviousTool();
                    break;
            }
        }

        /// <summary>
        /// Compute the placement of the designated position, this method takes into account the snapping option
        /// And check as well if the user is moving existing positions of the cut path.
        /// The method is also in charge to add the new positions to the CutPath on user clicks
        /// </summary>
        void DoPointPlacement()
        {
            Event evt = Event.current;
            EventType evtType = evt.type;

            bool hasHitPosition = UpdateHitPosition();

            //Updating visual helpers to get the right position and color to help in the placement
            if (evtType== EventType.Repaint)
            {
                m_SnappingPoint = m_SnapToGeometry || (evt.modifiers & EventModifiers.Control) != 0;
                m_ModifyingPoint = evt.shift;

                if(!m_SnappingPoint &&
                   !m_ModifyingPoint &&
                   !m_PlacingPoint)
                    m_SelectedIndex = -1;

                if (hasHitPosition)
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
                if( evtType == EventType.MouseDown
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
                     && evtType == EventType.MouseDown
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
            if(m_CutPath.Count == 0
                && !hasHitPosition
                && HandleUtility.nearestControl == m_ControlId)
            {
                ProBuilderEditor.instance.HandleMouseEvent(SceneView.lastActiveSceneView, m_ControlId);
            }
        }

        bool CanAppendCurrentPointToPath()
        {
            int polyCount = m_CutPath.Count;

            if (!Math.IsNumber(m_CurrentPosition))
                return false;

            if (!(polyCount == 0 || m_SelectedIndex != polyCount - 1))
                return false;

            // duplicate points are not permitted, except the special case where placing a final point on the starting
            // point finishes the cut operation.
            for(int i = 1; i < polyCount; i++)
                if (Math.Approx3(m_CutPath[i].position, m_CurrentPosition))
                    return false;

            // when the existing vertex count is less than 3, don't allow the special duplicate first vertex position
            return polyCount < 2 || !(polyCount < 3 && Math.Approx3(m_CutPath[0].position, m_CurrentPosition));
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

        /// <summary>
        /// Compute the position designated by the user in the current mesh/face taking into account snapping
        /// </summary>
        /// <returns>true is a valid position is computed in the mesh</returns>
        bool UpdateHitPosition()
        {
            Event evt = Event.current;

            Ray ray = UHandleUtility.GUIPointToWorldRay(evt.mousePosition);
            RaycastHit pbHit;

            m_CurrentFace = null;

            if (UnityEngine.ProBuilder.HandleUtility.FaceRaycast(ray, m_Mesh, out pbHit))
            {
                UpdateCurrentPosition(m_Mesh.faces[pbHit.face], pbHit.point ,pbHit.normal);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Add the position in the face to the cut taking into account snapping
        /// </summary>
        internal void UpdateCurrentPosition(Face face, Vector3 position, Vector3 normal)
        {
            m_CurrentPosition = position;
            m_CurrentPositionNormal = normal;
            m_CurrentFace = face;
            m_CurrentVertexTypes = VertexTypes.None;

            CheckPointInCutPath();

            if (m_CurrentVertexTypes == VertexTypes.None && !m_ModifyingPoint)
                CheckPointInMesh();
        }

        /// <summary>
        /// Updates the connections between the cut path and the mesh vertices
        /// </summary>
        internal void UpdateMeshConnections()
        {
            m_MeshConnections.Clear();
            if(m_CutPath.Count < 2)
                return;

            List<Vector3> existingVerticesInCut =
                m_CutPath.Where(v => ( v.types | VertexTypes.ExistingVertex ) != 0)
                         .Select(v => v.position).ToList();

            Vector3[] verticesPositions = m_Mesh.positionsInternal;
            if(!isALoop)
            {
                //Connects to start and the end of the path to create a loop
                float minDistToStart = Single.PositiveInfinity, minDistToStart2 = Single.PositiveInfinity;
                float minDistToEnd = Single.PositiveInfinity, minDistToEnd2 = Single.PositiveInfinity;
                int bestVertexIndexToStart = -1, bestVertexIndexToStart2 = -1, bestVertexIndexToEnd = -1,  bestVertexIndexToEnd2 = -1;
                float dist;
                foreach(var vertexIndex in m_TargetFace.distinctIndexes)
                {
                    if(existingVerticesInCut.Count > 0)
                    {
                        if(existingVerticesInCut.Exists(vert => Math.Approx3(verticesPositions[vertexIndex], vert)))
                            continue;
                    }

                    if(( m_CutPath[0].types & VertexTypes.NewVertex ) != 0)
                    {
                        dist = Vector3.Distance(verticesPositions[vertexIndex], m_CutPath[0].position);
                        if(dist < minDistToStart)
                        {
                            minDistToStart2 = minDistToStart;
                            bestVertexIndexToStart2 = bestVertexIndexToStart;
                            minDistToStart = dist;
                            bestVertexIndexToStart = vertexIndex;
                        }else if(dist < minDistToStart2)
                        {
                            minDistToStart2 = dist;
                            bestVertexIndexToStart2 = vertexIndex;
                        }
                    }
                    if(m_CutPath.Count > 1 && ( m_CutPath[m_CutPath.Count - 1].types & VertexTypes.NewVertex ) != 0)
                    {
                        dist = Vector3.Distance(verticesPositions[vertexIndex], m_CutPath[m_CutPath.Count - 1].position);
                        if(dist < minDistToEnd)
                        {
                            minDistToEnd2 = minDistToEnd;
                            bestVertexIndexToEnd2 = bestVertexIndexToEnd;
                            minDistToEnd = dist;
                            bestVertexIndexToEnd = vertexIndex;
                        }
                        else if(dist < minDistToEnd2)
                        {
                            minDistToEnd2 = dist;
                            bestVertexIndexToEnd2 = vertexIndex;
                        }
                    }
                }

                //Do not connect the 2 extremities to the same point
                if(bestVertexIndexToStart == bestVertexIndexToEnd)
                {
                    if(minDistToStart2 < minDistToEnd2)
                        bestVertexIndexToStart = bestVertexIndexToStart2;
                    else
                        bestVertexIndexToEnd = bestVertexIndexToEnd2;
                }

                if(bestVertexIndexToStart >= 0)
                    m_MeshConnections.Add(new SimpleTuple<int, int>(0,bestVertexIndexToStart));

                if(bestVertexIndexToEnd >= 0)
                    m_MeshConnections.Add(new SimpleTuple<int, int>(m_CutPath.Count - 1,bestVertexIndexToEnd));
            }
            else if(isALoop && connectionsToBordersCount < 2)
            {
                //The path must have minimum connections with the face borders, find the closest vertices
                foreach(var vertexIndex in m_TargetFace.distinctIndexes)
                {
                    if(existingVerticesInCut.Count > 0)
                    {
                        if(existingVerticesInCut.Exists(vert => Math.Approx3(verticesPositions[vertexIndex], vert)))
                            continue;
                    }

                    int pathIndex = -1;
                    float minDistance = Single.MaxValue;
                    for(int i = 0; i < m_CutPath.Count; i++)
                    {
                        if(( m_CutPath[i].types & (VertexTypes.AddedOnEdge | VertexTypes.ExistingVertex) ) == 0)
                        {
                            float dist = Vector3.Distance(verticesPositions[vertexIndex], m_CutPath[i].position);
                            if(dist < minDistance)
                            {
                                minDistance = dist;
                                pathIndex = i;
                            }
                        }
                    }

                    if(pathIndex >= 0)
                    {
                        if(m_MeshConnections.Exists(tup => tup.item1 == pathIndex))
                        {
                            var tuple = m_MeshConnections.Find(tup => tup.item1 == pathIndex);
                            if(Vector3.Distance(m_CutPath[tuple.item1].position, verticesPositions[tuple.item2])
                               > Vector3.Distance(m_CutPath[pathIndex].position, verticesPositions[vertexIndex]))
                            {
                                m_MeshConnections.Remove(tuple);
                                m_MeshConnections.Add(new SimpleTuple<int, int>(pathIndex, vertexIndex));
                            }
                        }
                        else
                            m_MeshConnections.Add(new SimpleTuple<int, int>(pathIndex, vertexIndex));
                    }
                }

                m_MeshConnections.Sort((a,b) =>
                    (int)Mathf.Sign(Vector3.Distance(m_CutPath[a.item1].position, verticesPositions[a.item2])
                                    - Vector3.Distance(m_CutPath[b.item1].position, verticesPositions[b.item2])));

                int connectionsCount = 2 - connectionsToBordersCount;
                m_MeshConnections.RemoveRange(connectionsCount,m_MeshConnections.Count - connectionsCount);
            }
        }

        /// <summary>
        /// Compute the cut result and display a notification
        /// </summary>
        void ExecuteCut(bool restorePrevious = true)
        {
            ActionResult result = DoCut();
            EditorUtility.ShowNotification(result.notification);

            if(restorePrevious)
                ToolManager.RestorePreviousTool();
        }

        /// <summary>
        /// Compute the faces resulting from the cut:
        /// - First inserts points defining the cut as vertices in the face
        /// - Compute the central polygon is the cut is creating a closed polygon in the face
        /// - Update the rest of the face accordingly to the cut and the central polygon
        /// </summary>
        /// <returns>ActionResult success if it was possible to create the cut</returns>
        internal ActionResult DoCut()
        {
            if (m_TargetFace == null || m_CutPath.Count < 2)
            {
                return new ActionResult(ActionResult.Status.Canceled, L10n.Tr("Not enough elements selected for a cut"));
            }

            if(!m_IsCutValid)
            {
                return new ActionResult(ActionResult.Status.Failure, L10n.Tr("The current cut overlaps itself"));
            }

            UndoUtility.RecordObject(m_Mesh, "Execute Cut");

            List<Vertex> meshVertices = new List<Vertex>();
            m_Mesh.GetVerticesInList(meshVertices);
            Vertex[] formerVertices = new Vertex[m_MeshConnections.Count];
            for(int i = 0; i < m_MeshConnections.Count; i++)
            {
                formerVertices[i] = meshVertices[m_MeshConnections[i].item2];
            }

            //Insert cut vertices in the mesh
            List<Vertex> cutVertices = InsertVertices();
            m_Mesh.GetVerticesInList(meshVertices);
            //Retrieve indexes of the cut points in the mesh vertices
            int[] cutIndexes = cutVertices.Select(vert => meshVertices.IndexOf(vert)).ToArray();

            //Update mesh connections with new indexes
            for(int i = 0; i<m_MeshConnections.Count; i++)
            {
                SimpleTuple<int, int> connection = m_MeshConnections[i];
                connection.item1 = meshVertices.IndexOf(cutVertices[connection.item1]);
                connection.item2 = meshVertices.IndexOf(formerVertices[i]);
                m_MeshConnections[i] = connection;
            }

            List<Face> newFaces = new List<Face>();
            // If the cut defines a loop in the face, create the polygon corresponding to that loop
            if (isALoop)
            {
                Face f = m_Mesh.CreatePolygon(cutIndexes, false);
                f.submeshIndex = m_TargetFace.submeshIndex;

                if(f == null)
                    return new ActionResult(ActionResult.Status.Failure, L10n.Tr("Cut Shape is not valid"));

                Vector3 nrm = Math.Normal(m_Mesh, f);
                Vector3 targetNrm = Math.Normal(m_Mesh, m_TargetFace);
                // If the shape is define in the wrong orientation compared to the former face, reverse it
                if(Vector3.Dot(nrm,targetNrm) < 0f)
                    f.Reverse();

                newFaces.Add(f);
            }

            //Compute the rest of the new faces (faces outside of the loop or division of the original face)
            List<Face> faces = ComputeNewFaces(m_TargetFace, cutIndexes);
            if(!isALoop)
                newFaces.AddRange(faces);

            //Remove inserted vertices only if they were inserted for the process
            List<int> verticesIndexesToDelete = new List<int>();
            for(int i = 0; i < m_CutPath.Count; i++)
            {
                if(( m_CutPath[i].types & VertexTypes.NewVertex ) != 0
                && ( m_CutPath[i].types & VertexTypes.VertexInShape ) == 0)
                    verticesIndexesToDelete.Add(cutIndexes[i]);
            }
            m_Mesh.DeleteVertices(verticesIndexesToDelete);

            //Delete former face
            m_Mesh.DeleteFace(m_TargetFace);

            m_Mesh.ToMesh();
            m_Mesh.Refresh();
            m_Mesh.Optimize();

            //Update mesh selection after the cut has been performed
            MeshSelection.ClearElementSelection();
            m_Mesh.SetSelectedFaces(newFaces);
            ProBuilderEditor.Refresh();

            Clear();

            return new ActionResult(ActionResult.Status.Success, L10n.Tr("Cut executed"));
        }


        /// <summary>
        /// Based on the new vertices inserted in the face, this method computes the different faces
        /// created between the cut and the original face (external to the cut if it makes a loop)
        ///
        /// The faces are created by parsing the edges that defines the border of the original face. Is an edge ends on a
        /// vertex that is part of the cut, or belongs to a connection between the cut and the face,
        /// we close the defined polygon using the cut (though ComputeFaceClosure method) and create a face out of this polygon
        /// </summary>
        /// <param name="face">Original face to modify</param>
        /// <param name="cutVertexIndexes">Indexes of the new vertices inserted in the face</param>
        /// <returns>The list of polygons to create (defined by their vertices indexes)</returns>
        List<Face> ComputeNewFaces(Face face, IList<int> cutVertexIndexes)
        {
            List<Face> newFaces = new List<Face>();

            //Get Vertices from the mesh
            Dictionary<int, int> sharedToUnique = m_Mesh.sharedVertexLookup;
            var cutVertexSharedIndexes = cutVertexIndexes.Select(ind => sharedToUnique[ind]).ToList();

            //Parse peripheral edges to unique id and find a common point between the peripheral edges and the cut
            var peripheralEdges = WingedEdge.SortEdgesByAdjacency(face);
            var peripheralEdgesUnique = new List<Edge>();
            int startIndex = -1;
            for (int i = 0; i < peripheralEdges.Count; i++)
            {
                Edge eShared = peripheralEdges[i];
                Edge eUnique = new Edge(sharedToUnique[eShared.a], sharedToUnique[eShared.b]);
                peripheralEdgesUnique.Add(eUnique);

                if (startIndex == -1 && ( cutVertexSharedIndexes.Contains(eUnique.a)
                                          || m_MeshConnections.Exists(tup => sharedToUnique[tup.item2] == eUnique.a)))
                    startIndex = i;
            }

            //Create a polygon for each cut reaching the mesh edges
            List<Face> facesToDelete = new List<Face>();
            List<int> polygon = new List<int>();
            for (int i = startIndex; i <= peripheralEdgesUnique.Count + startIndex; i++)
            {
                 polygon.Add(peripheralEdges[i % peripheralEdgesUnique.Count].a);
                 Edge e = peripheralEdgesUnique[i % peripheralEdgesUnique.Count];

                 if(polygon.Count > 1)
                 {
                     int index = -1;
                     if(cutVertexSharedIndexes.Contains(e.a)) // get next vertex
                     {
                         index = e.a;
                     }
                     else if(m_MeshConnections.Exists(tup => sharedToUnique[tup.item2] == e.a))
                     {
                         SimpleTuple<int, int> connection = m_MeshConnections.Find(tup => sharedToUnique[tup.item2] == e.a);
                         polygon.Add(connection.item1);
                         index = sharedToUnique[connection.item1];
                     }

                     if(index >= 0)
                     {
                         List<Face> toDelete;
                         Face newFace = ComputeFaceClosure(polygon, index, cutVertexSharedIndexes, out toDelete);
                         newFace.submeshIndex = m_TargetFace.submeshIndex;

                         newFaces.Add(newFace);
                         facesToDelete.AddRange(toDelete);

                         //Start a new polygon
                         polygon = new List<int>();
                         polygon.Add(peripheralEdges[i % peripheralEdgesUnique.Count].a);
                     }
                 }
            }
            polygon.Clear();

            m_Mesh.DeleteFaces(facesToDelete);
            return newFaces;
        }



        /// <summary>
        ///    The method computes all the possible faces that can be made starting by the vertices in polygonStart and ending with the cut
        /// This method creates faces that are not the final one and that must be deleted at the end. These invalid faces are returned in facesToDelete
        /// The only valid face is returned from this method. From all defined faces, the valid face is the one with the smaller area
        /// (otherwise it means it covers another face of the mesh).
        /// </summary>
        /// <param name="polygonStart">Indexes of the first vertices of the new Face to define, these vertices are coming from the original face only</param>
        /// <param name="currentIndex">Current vertex index in the cut</param>
        /// <param name="cutIndexes">Indexes of the vertices defining the cut</param>
        /// <param name="cutIndexes">out : extra faces created by this method that will need to be deleted after
        /// (these faces cannot be deleted directly as it will break the m_MeshConnections by deleting some indexes before the end of the algorithm)
        /// <returns>the valid face that need to be kept in the resulting mesh</returns>
        Face ComputeFaceClosure( List<int> polygonStart, int currentIndex, List<int> cutIndexes, out List<Face> facesToDelete)
        {
            List<Vertex> meshVertices = new List<Vertex>();
            IList<SharedVertex> uniqueIdToVertexIndex = m_Mesh.sharedVertices;
            Dictionary<int, int> sharedToUnique = m_Mesh.sharedVertexLookup;

            int polygonFirstVertex = polygonStart[0];
            int startIndex = cutIndexes.IndexOf(currentIndex);

            SimpleTuple<int,int> connection = m_MeshConnections.Find(tup => sharedToUnique[tup.item2] == sharedToUnique[polygonFirstVertex]);

            List<List<int>> closureCandidates = new List<List<int>>();

            //Go through the cut in reverse direction
            int index;
            int finalIndex = isALoop ?(startIndex - cutIndexes.Count) : 0;
            bool connected = false;
            List<int> candidate = new List<int>();
            for(index = startIndex - 1; index >= finalIndex; index--)
            {
                int vertexIndex = uniqueIdToVertexIndex[cutIndexes[(index + cutIndexes.Count) % cutIndexes.Count]][0];
                candidate.Add(vertexIndex);
                if(sharedToUnique[vertexIndex] == sharedToUnique[polygonFirstVertex] ||
                   sharedToUnique[vertexIndex] == sharedToUnique[connection.item1])
                {
                    connected = true;
                    break;
                }
            }

            //If we find a valid candidate for the connection, add it to the list
            if(connected)
                closureCandidates.Add(candidate);

            //Go through the cut in forward direction
            finalIndex = isALoop ? (startIndex + cutIndexes.Count) : cutIndexes.Count;
            connected = false;
            candidate = new List<int>();
            for(index = startIndex + 1; index < finalIndex; index++)
            {
                int vertexIndex = uniqueIdToVertexIndex[cutIndexes[index % cutIndexes.Count]][0];
                candidate.Add(vertexIndex);
                if(sharedToUnique[vertexIndex] == sharedToUnique[polygonFirstVertex] ||
                   sharedToUnique[vertexIndex] == sharedToUnique[connection.item1])
                {
                    connected = true;
                    break;
                }
            }

            //If we find a valid candidate for the connection, add it to the list
            if(connected)
                closureCandidates.Add(candidate);

            //Go through the different candidate and keep the best one
            facesToDelete = new List<Face>();
            Face bestFace = null;
            float bestArea = 0f;
            foreach(var closure in closureCandidates)
            {
                closure.AddRange(polygonStart);

                Face face = m_Mesh.CreatePolygon(closure, false);
                m_Mesh.GetVerticesInList(meshVertices);
                uniqueIdToVertexIndex = m_Mesh.sharedVertices;
                sharedToUnique = m_Mesh.sharedVertexLookup;

                if(bestFace != null)
                {
                    Vector3[] vertices = meshVertices.Select(vertex => vertex.position).ToArray();
                    int[] indexes = face.indexesInternal.Select(i => uniqueIdToVertexIndex[sharedToUnique[i]][0]).ToArray();
                    float area = Math.PolygonArea(vertices, indexes);
                    if(area < bestArea)
                    {
                        facesToDelete.Add(bestFace);
                        bestArea = area;
                        bestFace = face;
                    }
                    else
                        facesToDelete.Add(face);
                }
                else
                {
                    bestFace = face;
                    Vector3[] vertices = meshVertices.Select(vertex => vertex.position).ToArray();
                    int[] indexes = face.indexesInternal.Select(i => uniqueIdToVertexIndex[sharedToUnique[i]][0]).ToArray();
                    bestArea = Math.PolygonArea(vertices, indexes);
                }
            }

            return bestFace;
        }

        /// <summary>
        /// Check whether the current position (m_CurrentPosition) can be associated/snapped to an existing position of the path
        /// </summary>
        void CheckPointInCutPath()
        {
            //Check if trying to reach the start point
            if(!m_ModifyingPoint && m_CutPath.Count > 1)
            {
                float snapDistance = 0.1f;
                var vertexData = m_CutPath[0];
                if(Math.Approx3(vertexData.position, m_CurrentPosition, snapDistance))
                {
                    m_CurrentPosition = vertexData.position;
                    m_CurrentVertexTypes = vertexData.types | VertexTypes.VertexInShape;
                    m_SelectedIndex = 0;
                }
            }
            else if (m_SnappingPoint || m_ModifyingPoint)
            {
                float snapDistance = m_SnappingDistance;
                for(int i = 0; i < m_CutPath.Count; i++)
                {
                    var vertexData = m_CutPath[i];
                    if(Math.Approx3(vertexData.position,
                        m_CurrentPosition,
                        snapDistance))
                    {
                        snapDistance = Vector3.Distance(vertexData.position, m_CurrentPosition);
                        if(!m_ModifyingPoint)
                            m_CurrentPosition = vertexData.position;
                        m_CurrentVertexTypes = vertexData.types | VertexTypes.VertexInShape;
                        m_SelectedIndex = i;
                    }
                }
            }
        }

        /// <summary>
        /// Check whether the current position (m_CurrentPosition) can be associated/snapped to an existing
        /// edge or vertex of the current face
        /// </summary>
        void CheckPointInMesh()
        {
            m_CurrentVertexTypes = VertexTypes.NewVertex;
            bool snapedOnVertex = false;
            float snapDistance = m_SnappingDistance;
            int bestIndex = -1;
            float bestDistance = Mathf.Infinity;

            m_SnapedVertexId = -1;
            m_SnapedEdge = Edge.Empty;

            Vertex[] vertices = m_Mesh.GetVertices();
            List<Edge> peripheralEdges = WingedEdge.SortEdgesByAdjacency(m_CurrentFace);
            if (m_TargetFace != null && m_CurrentFace != m_TargetFace)
                peripheralEdges = WingedEdge.SortEdgesByAdjacency(m_TargetFace);
            for (int i = 0; i < peripheralEdges.Count; i++)
            {
                if ((m_TargetFace == null || m_TargetFace == m_CurrentFace) && m_SnappingPoint)
                {
                    if (Math.Approx3(vertices[peripheralEdges[i].a].position,
                        m_CurrentPosition,
                        snapDistance))
                    {
                        bestIndex = i;
                        snapedOnVertex = true;
                        break;
                    }
                    else
                    {
                        float dist = Math.DistancePointLineSegment(
                            m_CurrentPosition,
                            vertices[peripheralEdges[i].a].position,
                            vertices[peripheralEdges[i].b].position);

                        if (dist < Mathf.Min(snapDistance, bestDistance))
                        {
                            bestIndex = i;
                            bestDistance = dist;
                        }
                    }
                }
                //Even with no snapping, try to detect if the first point is on a existing geometry
                else if(m_TargetFace == null && !m_SnappingPoint)
                {
                    if (Math.Approx3(vertices[peripheralEdges[i].a].position,
                        m_CurrentPosition,
                        0.01f))
                    {
                        bestIndex = i;
                        snapedOnVertex = true;
                        break;
                    }
                    else
                    {
                        float dist = Math.DistancePointLineSegment(
                            m_CurrentPosition,
                            vertices[peripheralEdges[i].a].position,
                            vertices[peripheralEdges[i].b].position);

                        if (dist < Mathf.Min(0.01f, bestDistance))
                        {
                            bestIndex = i;
                            bestDistance = dist;
                        }
                    }
                }
                else if(m_CurrentFace != m_TargetFace && m_TargetFace != null )
                {
                    float edgeDist = Math.DistancePointLineSegment(m_CurrentPosition,
                        vertices[peripheralEdges[i].a].position,
                        vertices[peripheralEdges[i].b].position);

                    float vertexDist = Vector3.Distance(m_CurrentPosition,
                        vertices[peripheralEdges[i].a].position);

                    if (edgeDist < vertexDist && edgeDist < bestDistance)
                    {
                        bestIndex = i;
                        bestDistance = edgeDist;
                        snapedOnVertex = false;
                    }
                    //always prioritize vertex snap on edge snap
                    else if (vertexDist <= bestDistance)
                    {
                        bestIndex = i;
                        bestDistance = vertexDist;
                        snapedOnVertex = true;
                    }
                }
            }

            //We found a close vertex
            if (snapedOnVertex)
            {
                m_CurrentPosition = vertices[peripheralEdges[bestIndex].a].position;
                m_CurrentVertexTypes = VertexTypes.ExistingVertex;
                m_SelectedIndex = -1;

                m_SnapedVertexId = peripheralEdges[bestIndex].a;
                CheckPointInCutPath();
            }
            //If not, did we found a close edge?
            else if (bestIndex >= 0)
            {
                if (m_TargetFace == null || m_TargetFace == m_CurrentFace)
                {
                    Vector3 left = vertices[peripheralEdges[bestIndex].a].position,
                        right = vertices[peripheralEdges[bestIndex].b].position;

                    float x = (m_CurrentPosition - left).magnitude;
                    float y = (m_CurrentPosition - right).magnitude;

                    m_CurrentPosition = left + (x / (x + y)) * (right - left);
                }
                else //if(m_CurrentFace != m_TargetFace)
                {
                    Vector3 a = m_CurrentPosition -
                                vertices[peripheralEdges[bestIndex].a].position;
                    Vector3 b = vertices[peripheralEdges[bestIndex].b].position -
                                vertices[peripheralEdges[bestIndex].a].position;

                    float angle = Vector3.Angle(b, a);
                    m_CurrentPosition = Vector3.Magnitude(a) * Mathf.Cos(angle * Mathf.Deg2Rad) * b / Vector3.Magnitude(b);
                    m_CurrentPosition += vertices[peripheralEdges[bestIndex].a].position;
                }

                m_SnapedEdge = peripheralEdges[bestIndex];

                m_CurrentVertexTypes = VertexTypes.AddedOnEdge;
                m_SelectedIndex = -1;
            }
        }

        /// <summary>
        /// Insert all position from the cut path to the current faces as new vertices
        /// </summary>
        /// <returns>The list of Vertex inserted in the face</returns>
        List<Vertex> InsertVertices()
        {
            List<Vertex> newVertices = new List<Vertex>();

            foreach (var vertexData in m_CutPath)
            {
                switch (vertexData.types)
                {
                    case VertexTypes.ExistingVertex:
                    case VertexTypes.VertexInShape:
                        newVertices.Add(InsertVertexOnExistingVertex(vertexData.position));
                        break;
                    case VertexTypes.AddedOnEdge:
                        newVertices.Add(InsertVertexOnExistingEdge(vertexData.position));
                        break;
                    case VertexTypes.NewVertex:
                        newVertices.Add(m_Mesh.InsertVertexInMesh(vertexData.position,vertexData.normal));
                        break;
                    default:
                        break;
                }
            }

            return newVertices;
        }

        /// <summary>
        /// Method to retrieve a vertex already existing in the face to avoid duplicated
        /// </summary>
        /// <param name="vertexPosition">The vertex position</param>
        /// <returns>The retrieved vertex</returns>
        Vertex InsertVertexOnExistingVertex(Vector3 vertexPosition)
        {
            Vertex vertex = null;

            List<Vertex> vertices = m_Mesh.GetVertices().ToList();
            for (int vertIndex = 0; vertIndex < vertices.Count; vertIndex++)
            {
                if (Math.Approx3(vertices[vertIndex].position, vertexPosition)
                    && !float.IsNaN(vertices[vertIndex].normal.x) )
                {
                    vertex = vertices[vertIndex];
                    break;
                }
            }

            return vertex;
        }

        /// <summary>
        /// Insert the vertex in an exiting edge
        /// </summary>
        /// <param name="vertexPosition">The position of the vertex to insert</param>
        /// <returns>The inew vertex inserted</returns>
        Vertex InsertVertexOnExistingEdge(Vector3 vertexPosition)
        {
            List<Vertex> vertices = m_Mesh.GetVertices().ToList();
            List<Edge> peripheralEdges = WingedEdge.SortEdgesByAdjacency(m_TargetFace);

            int bestIndex = -1;
            float bestDistance = Mathf.Infinity;
            for (int i = 0; i < peripheralEdges.Count; i++)
            {
                float dist = UnityEngine.ProBuilder.Math.DistancePointLineSegment(vertexPosition,
                        vertices[peripheralEdges[i].a].position,
                        vertices[peripheralEdges[i].b].position);

                if (dist < bestDistance)
                {
                    bestIndex = i;
                    bestDistance = dist;
                }
            }

            Vertex v = m_Mesh.InsertVertexOnEdge(peripheralEdges[bestIndex], vertexPosition);
            return v;
        }

        /// <summary>
        /// Display existing points of the cut
        /// </summary>
        void DoExistingPointsGUI()
        {
            Transform trs = m_Mesh.transform;
            int len = m_CutPath.Count;

            Event evt = Event.current;

            if (evt.type == EventType.Repaint)
            {
                for (int index = 0; index < len; index++)
                {
                    Vector3 point = trs.TransformPoint(m_CutPath[index].position);
                    float size = HandleUtility.GetHandleSize(point) * k_HandleSize;

                    Handles.color = k_HandleColor;
                    Handles.DotHandleCap(-1, point, Quaternion.identity, size, evt.type);
                }

                Handles.color = Color.white;
            }
        }

        /// <summary>
        /// Display current position to potentially add to the cut
        /// </summary>
        void DoCurrentPointsGUI()
        {
            Transform trs = m_Mesh.transform;
            int len = m_CutPath.Count;

            Event evt = Event.current;

            if (evt.type == EventType.Repaint)
            {
                if (!m_CurrentPosition.Equals(Vector3.positiveInfinity))
                {
                    Vector3 point = trs.TransformPoint(m_CurrentPosition);
                    if(m_SelectedIndex >= 0 && m_SelectedIndex < m_CutPath.Count)
                        point = trs.TransformPoint(m_CutPath[m_SelectedIndex].position);

                    float size = HandleUtility.GetHandleSize(point) * k_HandleSize;
                    Handles.color = m_CurrentHandleColor;
                    Handles.DotHandleCap(-1, point, Quaternion.identity, size, evt.type);
                }
                Handles.color = Color.white;
            }
        }

        /// <summary>
        /// Visual indications to help the user: highlighting faces, edges and vertices when snapping on them
        /// </summary>
        void DoVisualCues()
        {
            if(m_Mesh != null)
            {
                if(m_TargetFace == null && m_CurrentFace != null)
                    EditorHandleDrawing.HighlightFaces(m_Mesh, new Face[]{m_CurrentFace}, Color.Lerp(Color.blue, Color.cyan, 0.5f));

                EditorHandleDrawing.HighlightVertices(m_Mesh, m_SelectedVertices, false);
                EditorHandleDrawing.HighlightEdges(m_Mesh, m_SelectedEdges,false);

                if(m_TargetFace != null)
                {
                    if(m_SnapedVertexId != -1)
                        EditorHandleDrawing.HighlightVertices(m_Mesh, new int[] { m_SnapedVertexId });

                    if(m_SnapedEdge != Edge.Empty)
                        EditorHandleDrawing.HighlightEdges(m_Mesh, new Edge[] { m_SnapedEdge });
                }
            }
        }

        /// <summary>
        /// Rebuild the line mesh when updated
        /// </summary>
        void RebuildCutShape(bool optimize = true)
        {
            // If Undo is called immediately after creation this situation can occur
            if (m_Mesh == null)
                return;

            UpdateMeshConnections();
            ValidateCutShape();

            // While the vertex count may not change, the triangle winding might. So unfortunately we can't take
            // advantage of the `vertexCountChanged = false` optimization here.
            ProBuilderEditor.Refresh();
            SceneView.RepaintAll();

            if(optimize && isALoop && m_IsCutValid)
                ExecuteCut();

            m_Dirty = false;
        }

        void ValidateCutShape()
        {
            Vector3[] verticesPositions = m_Mesh.positionsInternal;

            m_IsCutValid = true;

            //For all segments of the current cut
            for(int i = 0; i < m_CutPath.Count-1 && m_IsCutValid; i++)
            {
                Vector2 segment1Start2D = HandleUtility.WorldToGUIPoint(m_Mesh.transform.TransformPoint(m_CutPath[i].position));
                Vector2 segment1End2D = HandleUtility.WorldToGUIPoint(m_Mesh.transform.TransformPoint(m_CutPath[i+1].position));

                int lastVertexIndex = (isALoop && i == 0) ? m_CutPath.Count-2 : m_CutPath.Count-1;
                //Test intersections with the rest of the cut path
                for(int j = i + 2; j < lastVertexIndex && m_IsCutValid; j++)
                {
                    if(((m_CutPath[j].types | m_CutPath[j+1].types) & VertexTypes.VertexInShape) == 0)
                    {
                        Vector2 segment2Start2D =
                            HandleUtility.WorldToGUIPoint(m_Mesh.transform.TransformPoint(m_CutPath[j].position));
                        Vector2 segment2End2D =
                            HandleUtility.WorldToGUIPoint(m_Mesh.transform.TransformPoint(m_CutPath[j + 1].position));

                        m_IsCutValid = !Math.GetLineSegmentIntersect(segment1Start2D, segment1End2D, segment2Start2D,
                            segment2End2D);
                    }
                }

                if(( (m_CutPath[i].types| m_CutPath[i+1].types) & VertexTypes.VertexInShape ) == 0)
                {
                    //Test intersections with the connections to the face vertices
                    for(int j = 0; j < m_MeshConnections.Count && m_IsCutValid; j++)
                    {
                        SimpleTuple<int, int> connection = m_MeshConnections[j];

                        if(connection.item1 != i && connection.item1 != i + 1)
                        {
                            Vector2 segment2Start2D =
                                HandleUtility.WorldToGUIPoint(
                                    m_Mesh.transform.TransformPoint(m_CutPath[connection.item1].position));
                            Vector2 segment2End2D =
                                HandleUtility.WorldToGUIPoint(
                                    m_Mesh.transform.TransformPoint(verticesPositions[connection.item2]));

                            m_IsCutValid = !Math.GetLineSegmentIntersect(segment1Start2D, segment1End2D,
                                segment2Start2D,
                                segment2End2D);
                        }
                    }
                }
            }

            //For all connections to the face vertices
            for(int i = 0; i <  m_MeshConnections.Count-1 && m_IsCutValid; i++)
            {
                SimpleTuple<int,int> connection1 = m_MeshConnections[i];
                Vector2 segment1Start2D =
                    HandleUtility.WorldToGUIPoint(
                        m_Mesh.transform.TransformPoint(m_CutPath[connection1.item1].position) );
                Vector2 segment1End2D =
                    HandleUtility.WorldToGUIPoint(
                        m_Mesh.transform.TransformPoint(verticesPositions[connection1.item2]));

                //Test intersection with the other connections to the face vertices
                for(int j = i+1; j < m_MeshConnections.Count && m_IsCutValid; j++)
                {
                    SimpleTuple<int,int> connection2 = m_MeshConnections[j];

                    Vector2 segment2Start2D =
                        HandleUtility.WorldToGUIPoint(
                            m_Mesh.transform.TransformPoint(m_CutPath[connection2.item1].position));
                    Vector2 segment2End2D =
                        HandleUtility.WorldToGUIPoint(
                            m_Mesh.transform.TransformPoint(verticesPositions[connection2.item2]));

                    m_IsCutValid = !Math.GetLineSegmentIntersect(segment1Start2D, segment1End2D, segment2Start2D, segment2End2D);
                }
            }

        }

        /// <summary>
        /// Display lines of the cut shape
        /// </summary>
        void DoExistingLinesGUI()
        {
            DrawCutLine();
            DrawMeshConnectionsHandles();
        }


        /// <summary>
        /// Draw the line corresponding to the current cut path in the face
        /// </summary>
        void DrawCutLine()
        {
            Handles.color = m_IsCutValid ? k_LineColor : k_InvalidLineColor;
            Handles.DrawPolyLine(m_CutPath.Select(tup => m_Mesh.transform.TransformPoint(tup.position)).ToArray());
            Handles.color = Color.white;
        }

        /// <summary>
        /// Draw a helper line between the last point of the cut and the current position of the mouse cursor
        /// </summary>
        void DrawGuideLine()
        {
            if(m_CurrentPosition.Equals(Vector3.positiveInfinity) || m_ModifyingPoint)
                return ;

            if(m_CutPath.Count > 0)
            {
                Handles.color = k_DrawingLineColor;
                Handles.DrawDottedLine(m_Mesh.transform.TransformPoint(m_CutPath[m_CutPath.Count - 1].position),
                                        m_Mesh.transform.TransformPoint(m_CurrentPosition), 5f);
                Handles.color = Color.white;
            }
        }

        /// <summary>
        /// Draw a helper line to show which vertices of the face are connected to points of the cut shape
        /// </summary>
        void DrawMeshConnectionsHandles()
        {
            if(m_MeshConnections.Count > 0)
            {
                Vertex[] vertices = m_Mesh.GetVertices();
                foreach(var connection in m_MeshConnections)
                {
                    Handles.color = k_ConnectionsLineColor;
                    Handles.DrawDottedLine(m_Mesh.transform.TransformPoint(m_CutPath[connection.item1].position),
                                            m_Mesh.transform.TransformPoint(vertices[connection.item2].position), 5f);
                    Handles.color = Color.white;
                }
            }
        }

    }
}

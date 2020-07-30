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


namespace UnityEditor.ProBuilder
{
    public class CutTool : EditorTool
    {
        ProBuilderMesh m_Mesh;

        Texture2D m_CutCursorTexture;
        Texture2D m_CutAddCursorTexture;
        Texture2D m_CutMagnetCursorTexture;
        Texture2D m_CutAddMagnetCursorTexture;
        Texture2D m_CurrentCutCursor = null;

        /// <summary>
        /// Describes the different vertex types on the path.
        /// </summary>
        [System.Flags]
        public enum VertexTypes
        {
            None = 0 << 0,
            NewVertex = 1 << 0,
            AddedOnEdge = 1 << 1,
            ExistingVertex = 1 << 2,
            VertexInShape = 1 << 3,
        }

        [Serializable]
        public struct CutVertexData
        {
            [SerializeField]
            Vector3 m_Position;
            [SerializeField]
            Vector3 m_Normal;
            [SerializeField]
            VertexTypes m_Types;

            public Vector3 position
            {
                get => m_Position;
                set => m_Position = value;
            }

            public Vector3 normal
            {
                get => m_Normal;
                set => m_Normal = value;
            }

            public VertexTypes types
            {
                get => m_Types;
                set => m_Types = value;
            }

            public CutVertexData(Vector3 position, VertexTypes types = VertexTypes.None)
            {
                m_Position = position;
                m_Normal = Vector3.up;
                m_Types = types;
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
        static Color k_HandleColorAddVertexOnEdge = new Color(.3f, .01f, .9f, 1f);
        static Color k_HandleColorUseExistingVertex = new Color(.01f, .5f, 1f, 1f);
        static readonly Color k_HandleColorModifyVertex = new Color(1f, .75f, .0f, 1f);
        const float k_HandleSize = .05f;

        // Line renderer for the current cut path
        Material m_LineMaterial;
        Mesh m_LineMesh = null;
        static readonly Color k_LineMaterialBaseColor = new Color(0f, 136f / 255f, 1f, 1f);
        static readonly Color k_LineMaterialHighlightColor = new Color(0f, 200f / 255f, 170f / 200f, 1f);

        // Line renderer between the last point of the cut and the first one to close the shape if the option is activated
        Material m_ConnectionsLineMaterial;
        Mesh m_ConnectionsLineMesh = null;
        static readonly Color k_ConnectionsLineMaterialBaseColor = new Color(0f, 200f / 255f, 170f / 200f, 1f);

        // Line renderer to provide a preview to the user of the next cut section
        Material m_DrawingLineMaterial;
        Mesh m_DrawingLineMesh = null;
        static readonly Color k_DrawingLineMaterialBaseColor = new Color(0.01f, .9f, 0.3f, 1f);

        Color m_CurrentHandleColor = k_HandleColor;

        internal Face m_TargetFace;
        internal Face m_CurrentFace;
        internal Vector3 m_CurrentPosition = Vector3.positiveInfinity;
        internal Vector3 m_CurrentPositionNormal = Vector3.up;
        internal VertexTypes m_CurrentVertexTypes = VertexTypes.None;

        int m_ControlId;
        bool m_PlacingPoint;
        internal bool m_SnappingPoint;
        bool m_ModifyingPoint;
        int m_SelectedIndex = -2;

        int m_SnapedVertexId = -1;
        Edge m_SnapedEdge = Edge.Empty;

        [SerializeField]
        internal List<CutVertexData> m_CutPath = new List<CutVertexData>();
        internal List<SimpleTuple<int, int>> m_MeshConnections = new List<SimpleTuple<int, int>>();

        GUIContent m_OverlayTitle;
        const string k_SnapToGeometryPrefKey = "VertexInsertion.snapToGeometry";
        const string k_SnappingDistancePrefKey = "VertexInsertion.snappingDistance";

        bool m_SnapToGeometry;
        float m_SnappingDistance;

        bool m_Ended = false;

        public bool IsALoop
        {
            get
            {
                if (m_CutPath.Count < 3)
                    return false;
                else
                    return Math.Approx3(m_CutPath[0].position,
                        m_CutPath[m_CutPath.Count - 1].position);
            }
        }

        public int ConnectionsToBordersCount
        {
            get
            {
                return m_CutPath.Count(data => (data.types & (VertexTypes.AddedOnEdge | VertexTypes.ExistingVertex)) != 0
                                                    && (data.types & VertexTypes.VertexInShape) == 0);
            }
        }

        void OnEnable()
        {
            k_HandleColorUseExistingVertex = Handles.selectedColor;
            k_HandleColorAddVertexOnEdge = Handles.selectedColor;

            m_OverlayTitle = new GUIContent("Cut Tool");
            m_SnapToGeometry = EditorPrefs.GetBool( k_SnapToGeometryPrefKey, false );
            m_SnappingDistance = EditorPrefs.GetFloat( k_SnappingDistancePrefKey, 0.1f );

            m_CutCursorTexture = Resources.Load<Texture2D>("Cursors/cutCursor");
            m_CutAddCursorTexture = Resources.Load<Texture2D>("Cursors/cutCursor-add");
            m_CutMagnetCursorTexture = Resources.Load<Texture2D>("Cursors/cutCursor-magnet");
            m_CutAddMagnetCursorTexture = Resources.Load<Texture2D>("Cursors/cutCursor-add-magnet");

            EditorApplication.update += Update;
            Undo.undoRedoPerformed += UndoRedoPerformed;
            if(MeshSelection.selectedObjectCount == 1)
                m_Mesh = MeshSelection.activeMesh;

            InitLineRenderers();
        }

        void OnDisable()
        {
            EditorApplication.update -= Update;
            Undo.undoRedoPerformed -= UndoRedoPerformed;

            ExecuteCut();
            Clear();
        }


        /// <summary>
        /// Create line renderers for the current cut
        /// </summary>
        void InitLineRenderers()
        {
            m_LineMesh = new Mesh();
            m_LineMaterial = CreateLineMaterial(k_LineMaterialBaseColor, k_LineMaterialHighlightColor);

            m_ConnectionsLineMesh = new Mesh();
            m_ConnectionsLineMaterial =
                CreateLineMaterial(k_ConnectionsLineMaterialBaseColor, k_ConnectionsLineMaterialBaseColor);

            m_DrawingLineMesh = new Mesh();
            m_DrawingLineMaterial = CreateLineMaterial(k_DrawingLineMaterialBaseColor, k_DrawingLineMaterialBaseColor);
        }

        /// <summary>
        /// Clear all line renderers
        /// </summary>
        void ClearLineRenderers()
        {
            if(m_LineMesh)
                DestroyImmediate(m_LineMesh);
            if(m_LineMaterial)
                DestroyImmediate(m_LineMaterial);

            if(m_ConnectionsLineMesh)
                DestroyImmediate(m_ConnectionsLineMesh);
            if(m_ConnectionsLineMaterial)
                DestroyImmediate(m_ConnectionsLineMaterial);

            if(m_DrawingLineMesh != null)
                DestroyImmediate(m_DrawingLineMesh);
            if(m_DrawingLineMaterial != null)
                DestroyImmediate(m_DrawingLineMaterial);
        }

        /// <summary>
        /// Clear all data from the cut tool
        /// </summary>
        void Clear()
        {
            ClearLineRenderers();

            m_Mesh = null;
            m_TargetFace = null;
            m_CurrentFace = null;
            m_PlacingPoint = false;
            m_CurrentCutCursor = null;
            m_Ended = false;
            m_CutPath.Clear();
            m_MeshConnections.Clear();
        }

        /// <summary>
        /// Reset tool data and line renderers
        /// </summary>
        void Reset()
        {
            Clear();
            InitLineRenderers();
        }

        /// <summary>
        /// Instantiate Line Materials, all are based on the same base Material with different colors
        /// </summary>
        /// <param name="baseColor">base color to apply to the line</param>
        /// <param name="highlightColor">highlight color to apply to the line</param>
        /// <returns></returns>
        static Material CreateLineMaterial(Color baseColor, Color highlightColor)
        {
            Material mat = new Material(Shader.Find("Hidden/ProBuilder/ScrollHighlight"));
            mat.SetColor("_Base", baseColor);
            mat.SetColor("_Highlight", highlightColor);
            return mat;
        }

        /// <summary>
        /// Update method that handles the update of line renderers
        /// </summary>
        void Update()
        {
            if(m_Mesh != null)
            {
                if(m_LineMaterial != null)
                    m_LineMaterial.SetFloat("_EditorTime", (float) EditorApplication.timeSinceStartup);
            }
        }

        /// <summary>
        /// Undo/Redo callback: Reset and recompute lines, and update the targeted face if needed
        /// </summary>
        private void UndoRedoPerformed()
        {
            ClearLineRenderers();
            InitLineRenderers();

            if(m_CutPath.Count == 0)
                m_TargetFace = null;

            m_SelectedIndex = -1;

            EditorApplication.delayCall = () => RebuildCutShape();
        }

        /// <summary>
        /// Main GUI update for the tool, calls every secondary methods to place points, update lines and compute the cut
        /// </summary>
        /// <param name="window">current window calling the tool : SceneView</param>
        public override void OnToolGUI( EditorWindow window )
        {
            SceneViewOverlay.Window( m_OverlayTitle, OnOverlayGUI, 0, SceneViewOverlay.WindowDisplayOption.OneWindowPerTitle );

            var currentEvent = Event.current;
            if (currentEvent.type == EventType.KeyDown)
                HandleKeyEvent(currentEvent);

            if(m_Mesh != null)
            {
                m_ControlId = GUIUtility.GetControlID(FocusType.Passive);
                if(currentEvent.type == EventType.Layout)
                    HandleUtility.AddDefaultControl(m_ControlId);

                DoPointPlacement();

                if(currentEvent.type == EventType.Repaint)
                {
                    DoExistingLinesGUI();
                    DoExistingPointsGUI();
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
                EditorGUI.HelpBox(rect, "One and only one ProBuilder mesh must be selected.", MessageType.Warning);
            }

            m_SnapToGeometry = DoOverlayToggle("Snap to existing edges and vertices", m_SnapToGeometry);
            EditorPrefs.SetBool(k_SnapToGeometryPrefKey, m_SnapToGeometry);

            if(!m_SnapToGeometry)
                GUI.enabled = false;
            EditorGUI.indentLevel++;
            using(new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Snapping distance", GUILayout.Width(200));
                m_SnappingDistance = EditorGUILayout.FloatField(m_SnappingDistance);
                EditorPrefs.SetFloat( k_SnappingDistancePrefKey, m_SnappingDistance);
            }
            EditorGUI.indentLevel--;

            GUI.enabled = true;

            if(MeshSelection.selectedObjectCount != 1)
                GUI.enabled = false;

            if(m_Mesh == null)
            {
                if(GUILayout.Button("Start"))
                {
                    m_Mesh = MeshSelection.activeMesh;
                    if(m_CutPath.Count > 0)
                    {
                        m_CutPath.Clear();
                        m_MeshConnections.Clear();
                    }
                }
            }
            else
            {
                if(GUILayout.Button("Cut"))
                    DoCut();
            }
            GUI.enabled = true;
        }

        /// <summary>
        /// Creates a toggle for cut tool overlays
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
        /// Update the mouse cursor depending on the tool status
        /// </summary>
        /// <returns>the texture to use as a cursor</returns>
        Texture2D GetCursorTexture()
        {
            Texture2D texture = m_CutCursorTexture;
            if(m_ModifyingPoint)
                return texture;

            if(m_CutPath.Count > 0)
                texture = m_SnapToGeometry ? m_CutAddMagnetCursorTexture : m_CutAddCursorTexture;
            else if(m_SnapToGeometry)
                texture = m_CutMagnetCursorTexture;

            return texture;
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

            if (evtType== EventType.Repaint)
            {
                m_SnappingPoint = m_SnapToGeometry || (evt.modifiers & EventModifiers.Control) != 0;
                m_ModifyingPoint = evt.shift;

                if(!m_SnappingPoint && !m_ModifyingPoint && !m_PlacingPoint)
                    m_SelectedIndex = -1;

                if (hasHitPosition)
                {
                    m_CurrentCutCursor = GetCursorTexture();
                    if( (m_CurrentVertexTypes & (VertexTypes.ExistingVertex | VertexTypes.VertexInShape)) != 0)
                        m_CurrentHandleColor = m_ModifyingPoint ? k_HandleColorModifyVertex : k_HandleColorUseExistingVertex;
                    else if ((m_CurrentVertexTypes & VertexTypes.AddedOnEdge) != 0)
                        m_CurrentHandleColor = k_HandleColorAddVertexOnEdge;
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
                        RebuildCutShape();
                        SceneView.RepaintAll();
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
                    SceneView.RepaintAll();
                }
            }
            else
            if (evtType == EventType.MouseDown
                && HandleUtility.nearestControl == m_ControlId)
            {
                int polyCount = m_CutPath.Count;
                if (!m_CurrentPosition.Equals(Vector3.positiveInfinity)
                    && (polyCount == 0 || m_SelectedIndex != polyCount - 1))
                {
                    AddCurrentPositionToPath();

                    evt.Use();
                }
            }
        }

        internal void AddCurrentPositionToPath()
        {
            UndoUtility.RecordObject(this, "Add Vertex On Path");

            if(m_TargetFace == null)
                m_TargetFace = m_CurrentFace;

            m_CutPath.Add(new CutVertexData(m_CurrentPosition, m_CurrentPositionNormal, m_CurrentVertexTypes));
            UpdateMeshConnections();

            m_PlacingPoint = true;
            m_SelectedIndex = m_CutPath.Count - 1;

            RebuildCutShape();
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
            Vertex[] vertices = m_Mesh.GetVertices();
            if(!IsALoop)
            {
                float minDistToStart = Single.PositiveInfinity;
                float minDistToEnd = Single.PositiveInfinity;
                int bestVertexToStart = -1, bestVertexToEnd = -1;
                float dist;
                foreach(var vertexIndex in m_TargetFace.distinctIndexes)
                {
                    Vertex v = vertices[vertexIndex];
                    if(( m_CutPath[0].types & VertexTypes.NewVertex ) != 0)
                    {
                        dist = Vector3.Distance(v.position, m_CutPath[0].position);
                        if(dist < minDistToStart)
                        {
                            minDistToStart = dist;
                            bestVertexToStart = vertexIndex;
                        }
                    }
                    if(m_CutPath.Count > 1 && ( m_CutPath[m_CutPath.Count - 1].types & VertexTypes.NewVertex ) != 0)
                    {
                        dist = Vector3.Distance(v.position, m_CutPath[m_CutPath.Count - 1].position);
                        if(dist < minDistToEnd)
                        {
                            minDistToEnd = dist;
                            bestVertexToEnd = vertexIndex;
                        }
                    }
                }

                if(bestVertexToStart >= 0)
                    m_MeshConnections.Add(new SimpleTuple<int, int>(0,bestVertexToStart));

                if(bestVertexToEnd >= 0)
                    m_MeshConnections.Add(new SimpleTuple<int, int>(m_CutPath.Count - 1,bestVertexToEnd));
            }
            else if(IsALoop && ConnectionsToBordersCount < 2)
            {
                foreach(var vertexIndex in m_TargetFace.distinctIndexes)
                {
                    Vertex v = vertices[vertexIndex];
                    int pathIndex = -1;
                    float minDistance = Single.MaxValue;
                    for(int i = 0; i < m_CutPath.Count; i++)
                    {
                        if(( m_CutPath[i].types & VertexTypes.AddedOnEdge ) == 0)
                        {
                            float dist = Vector3.Distance(v.position, m_CutPath[i].position);
                            if(dist < minDistance)
                            {
                                minDistance = dist;
                                pathIndex = i;
                            }
                        }
                    }
                    if(pathIndex >= 0)
                        m_MeshConnections.Add(new SimpleTuple<int, int>(pathIndex, vertexIndex));
                }

                m_MeshConnections.Sort((a,b) =>
                    (int)Mathf.Sign(Vector3.Distance(m_CutPath[a.item1].position, vertices[a.item2].position)
                                    - Vector3.Distance(m_CutPath[b.item1].position, vertices[b.item2].position)));

                int connectionsCount = 2 - ConnectionsToBordersCount;
                m_MeshConnections.RemoveRange(connectionsCount,m_MeshConnections.Count - connectionsCount);
            }

            UpdateMeshConnectionsLines();
        }

        /// <summary>
        /// Compute the cut result and display a notification
        /// </summary>
        void ExecuteCut()
        {
            ActionResult result = DoCut();
            EditorUtility.ShowNotification(result.notification);
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
                return new ActionResult(ActionResult.Status.Canceled, "Not enough elements selected for a cut");
            }

            UndoUtility.RecordObject(m_Mesh, "Add Face To Mesh");

            List<Vertex> meshVertices = m_Mesh.GetVertices().ToList();
            Vertex[] formerVertices = new Vertex[m_MeshConnections.Count];
            for(int i = 0; i < m_MeshConnections.Count; i++)
            {
                formerVertices[i] = meshVertices[m_MeshConnections[i].item2];
            }

            List<Vertex> cutVertices = InsertVertices();
            meshVertices = m_Mesh.GetVertices().ToList();
            int[] cutIndexes = cutVertices.Select(vert => meshVertices.IndexOf(vert)).ToArray();

            for(int i = 0; i<m_MeshConnections.Count; i++)
            {
                SimpleTuple<int, int> connection = m_MeshConnections[i];
                connection.item1 = meshVertices.IndexOf(cutVertices[connection.item1]);
                connection.item2 = meshVertices.IndexOf(formerVertices[i]);
                m_MeshConnections[i] = connection;
            }

            List<Face> newFaces = new List<Face>();
            // If the cut defines a loop in the face, create the polygon corresponding to that loop
            if (IsALoop)
            {
                Face f = m_Mesh.CreatePolygon(cutIndexes, false);

                if(f == null)
                    return new ActionResult(ActionResult.Status.Failure, "Cut Shape is not valid");

                Vector3 nrm = Math.Normal(m_Mesh, f);
                Vector3 targetNrm = Math.Normal(m_Mesh, m_TargetFace);
                // If the shape is define in the wrong orientation compared to the former face, reverse it
                if(Vector3.Dot(nrm,targetNrm) < 0f)
                    f.Reverse();

                newFaces.Add(f);
            }

            //Compute the polygons defined in the face
            var verticesIndexes = ComputePolygonsIndexes(m_TargetFace, cutIndexes);
            //Create these new polygonal faces
            foreach(var polygon in verticesIndexes)
            {
                Face newFace = m_Mesh.CreatePolygon(polygon, false);
                if(!IsALoop)
                    newFaces.Add(newFace);
            }

            //Delete former face
            m_Mesh.DeleteFace(m_TargetFace);

            m_Mesh.ToMesh();
            m_Mesh.Refresh();
            m_Mesh.Optimize();

            MeshSelection.ClearElementSelection();
            m_Mesh.SetSelectedFaces(newFaces);

            Reset();

            return ActionResult.Success;
        }


        /// <summary>
        /// Based on the new vertices inserted in the face, this method computes the different polygons
        /// created between the cut and the original face (external to the cut if it makes a loop)
        ///
        /// The polygons are created by parsing the edges that defines the border of the face. Is an edge ends on a
        /// vertex that is part of the cut, we close this polygon using the cut (though ClosePolygonalCut method)
        /// </summary>
        /// <param name="face">Original face to modify</param>
        /// <param name="cutVertexIndexes">Indexes of the new vertices inserted in the face</param>
        /// <returns>The list of polygons to create (defined by their vertices indexes)</returns>
        List<int[]> ComputePolygonsIndexes(Face face, IList<int> cutVertexIndexes)
        {
            var polygons =new List<int[]>();
            var vertices = m_Mesh.GetVertices();

            //Get Vertices from the mesh
            Dictionary<int, int> sharedToUnique = m_Mesh.sharedVertexLookup;
            IList<SharedVertex> uniqueIdToVertexIndex = m_Mesh.sharedVertices;
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
            List<int> polygon = new List<int>();
            Edge previousEdge = Edge.Empty;
            for (int i = startIndex; i <= peripheralEdgesUnique.Count + startIndex; i++)
            {
                 Edge e = peripheralEdgesUnique[i % peripheralEdgesUnique.Count];

                 polygon.Add(peripheralEdges[i % peripheralEdgesUnique.Count].a);

                 if(polygon.Count > 1 && m_MeshConnections.Exists(tup => sharedToUnique[tup.item2] == e.a))
                 {
                     SimpleTuple<int, int> connection = m_MeshConnections.Find(tup => sharedToUnique[tup.item2] == e.a);

                     Vector3 connectionDirection = vertices[connection.item1].position - vertices[uniqueIdToVertexIndex[e.a][0]].position;
                     Vector3 previousEdgeDirection =
                         vertices[uniqueIdToVertexIndex[previousEdge.b][0]].position - vertices[uniqueIdToVertexIndex[previousEdge.a][0]].position;
                     Vector3 normal = Vector3.Cross(previousEdgeDirection, connectionDirection);
                     List<int> closure = ClosePolygonalCut(polygon[0], connectionDirection, sharedToUnique[connection.item1], cutVertexSharedIndexes, normal);

                     polygon.Add(connection.item1);
                     polygon.AddRange(closure);
                     polygons.Add(polygon.ToArray());

                     //Start a new polygon
                     polygon = new List<int>();
                     polygon.Add(peripheralEdges[i % peripheralEdgesUnique.Count].a);
                 }

                 if(polygon.Count > 1 && cutVertexSharedIndexes.Contains(e.a)) // get next vertex
                 {
                     Vector3 previousEdgeDirection =
                         vertices[uniqueIdToVertexIndex[previousEdge.b][0]].position - vertices[uniqueIdToVertexIndex[previousEdge.a][0]].position;
                     List<int> closure = ClosePolygonalCut(polygon[0], previousEdgeDirection, e.a, cutVertexSharedIndexes, vertices[uniqueIdToVertexIndex[previousEdge.a][0]].normal);
                     polygon.AddRange(closure);
                     polygons.Add(polygon.ToArray());

                     //Start a new polygon
                     polygon = new List<int>();
                     polygon.Add(peripheralEdges[i % peripheralEdgesUnique.Count].a);
                 }

                 previousEdge = e;
            }
            polygon.Clear();

            return polygons;
        }

        /// <summary>
        ///    The method compute which vertices of the cut are defining the end of the current polygon
        /// </summary>
        /// <param name="polygonFirstVertex">Index of the first vertex to know when to end the cut</param>
        /// <param name="previousEdge">Previous edge in the face</param>
        /// <param name="currentIndex">Current vertex index</param>
        /// <param name="cutIndexes">Indexes of the vertices defining the cut</param>
        /// <returns>the indexes of the vertices ending the designated polygon</returns>
        List<int> ClosePolygonalCut(int polygonFirstVertex , Vector3 previousEdgeDirection, int currentIndex, List<int> cutIndexes, Vector3 normal)
        {
            List<int> closure = new List<int>();
            List<Vertex> meshVertices = m_Mesh.GetVertices().ToList();
            IList<SharedVertex> uniqueIdToVertexIndex = m_Mesh.sharedVertices;

            // Vector3 previousEdgeDir = meshVertices[uniqueIdToVertexIndex[previousEdge.b][0]].position -
            //                           meshVertices[uniqueIdToVertexIndex[previousEdge.a][0]].position;
            previousEdgeDirection.Normalize();

            int bestSuccessorIndex = -1;
            int successorDirection = 0;
            float bestCandidate = Mathf.Infinity;
            for (int i = 0; i < cutIndexes.Count; i++)
            {
                //Find the current point in the polygon
                if (cutIndexes[i] == currentIndex)
                {
                    if (i > 0 || IsALoop)
                    {
                        int previousIndex = (i > 0) ? i - 1 : (cutIndexes.Count - 1);
                        int previousVertexIndex = cutIndexes[previousIndex];
                        Vector3 previousVertexDir = meshVertices[uniqueIdToVertexIndex[previousVertexIndex][0]].position -
                                                    meshVertices[uniqueIdToVertexIndex[currentIndex][0]].position;
                        previousVertexDir.Normalize();

                        //float similarityToPrevious = Vector3.Dot(previousEdgeDirection, previousVertexDir);
                        float angle = Vector3.SignedAngle(previousVertexDir, previousEdgeDirection, normal);
                        if (angle < bestCandidate) //Go to previous
                        {
                            bestCandidate = angle;
                            bestSuccessorIndex = previousIndex;
                            successorDirection = -1;
                        }
                    }

                    if (i < cutIndexes.Count - 1 || IsALoop)
                    {
                        int nextIndex = (i < cutIndexes.Count - 1) ? i + 1 : 0;
                        int nextVertexIndex = cutIndexes[nextIndex];
                        Vector3 nextVertexDir = meshVertices[uniqueIdToVertexIndex[nextVertexIndex][0]].position -
                                                meshVertices[uniqueIdToVertexIndex[currentIndex][0]].position;
                        nextVertexDir.Normalize();

                        //float similarityToNext = Vector3.Dot(previousEdgeDirection, nextVertexDir);
                        float angle = Vector3.SignedAngle(nextVertexDir, previousEdgeDirection, normal);
                        if (angle < bestCandidate) // Go to next
                        {
                            bestCandidate = angle;
                            bestSuccessorIndex = nextIndex;
                            successorDirection = 1;
                        }
                    }

                }
            }

            Dictionary<int, int> sharedToUnique = m_Mesh.sharedVertexLookup;
            if (successorDirection == -1)
            {
                for (int i = bestSuccessorIndex; i > (bestSuccessorIndex - cutIndexes.Count); i--)
                {
                    int vertexIndex = uniqueIdToVertexIndex[cutIndexes[(i + cutIndexes.Count) % cutIndexes.Count]][0];
                    closure.Add(vertexIndex);
                    if(sharedToUnique[vertexIndex] == sharedToUnique[polygonFirstVertex]
                    || m_MeshConnections.Exists(tup => sharedToUnique[tup.item1] == sharedToUnique[vertexIndex]
                                                       && sharedToUnique[tup.item2] ==  sharedToUnique[polygonFirstVertex]))
                        break;
                }
            }
            else if (successorDirection == 1)
            {
                for (int i = bestSuccessorIndex; i < (bestSuccessorIndex + cutIndexes.Count); i++)
                {
                    int vertexIndex = uniqueIdToVertexIndex[cutIndexes[i % cutIndexes.Count]][0];
                    closure.Add(vertexIndex);
                    if(sharedToUnique[vertexIndex] == sharedToUnique[polygonFirstVertex]
                       || m_MeshConnections.Exists(tup => sharedToUnique[tup.item1] == sharedToUnique[vertexIndex]
                                                          && sharedToUnique[tup.item2] ==  sharedToUnique[polygonFirstVertex]))
                        break;
                }
            }

            return closure;
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

                    m_Ended = true;
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

            List<Vertex> vertices = m_Mesh.GetVertices().ToList();
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
                if (Math.Approx3(vertices[vertIndex].position, vertexPosition))
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
                    UndoUtility.RecordObject(m_Mesh, "Delete Selected Points");
                    m_CutPath.RemoveAt(m_CutPath.Count-1);
                    UpdateMeshConnections();
                    RebuildCutShape();
                    evt.Use();
                    break;
                }

                case KeyCode.Escape:
                    evt.Use();
                    Clear();
                    //Leave the current tool
                    Tools.current = Tool.None;
                    break;

                case KeyCode.KeypadEnter:
                case KeyCode.Return:
                case KeyCode.Space:
                    evt.Use();
                    ExecuteCut();
                    //Leave the current tool
                    Tools.current = Tool.None;
                    break;
            }
        }

        /// <summary>
        /// Display existing points of the cut
        /// </summary>
        void DoExistingPointsGUI()
        {
            Transform trs = m_Mesh.transform;
            int len = m_CutPath.Count;

            Event evt = Event.current;

            bool used = evt.type == EventType.Used;

            if (!used &&
                (evt.type == EventType.MouseDown &&
                 evt.button == 0 &&
                 !EditorHandleUtility.IsAppendModifier(evt.modifiers)))
            {
                m_SelectedIndex = -1;
            }

            if (evt.type == EventType.Repaint)
            {
                for (int index = 0; index < len; index++)
                {
                    Vector3 point = trs.TransformPoint(m_CutPath[index].position);
                    float size = HandleUtility.GetHandleSize(point) * k_HandleSize;

                    Handles.color = k_HandleColor;
                    Handles.DotHandleCap(-1, point, Quaternion.identity, size, evt.type);

                    // "clicked" a button
                    if (!used && evt.type == EventType.Used)
                        used = true;
                }

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
        /// Display lines of the cut shape
        /// </summary>
        void DoExistingLinesGUI()
        {
            if(m_LineMaterial != null)
            {
                m_LineMaterial.SetPass(0);
                Graphics.DrawMeshNow(m_LineMesh, m_Mesh.transform.localToWorldMatrix, 0);
            }

            if(m_ConnectionsLineMesh != null)
            {
                m_ConnectionsLineMaterial.SetPass(0);
                Graphics.DrawMeshNow(m_ConnectionsLineMesh, m_Mesh.transform.localToWorldMatrix, 0);
            }

            if(m_DrawingLineMaterial != null)
            {
                if(DrawGuideLine())
                {
                    m_DrawingLineMaterial.SetPass(0);
                    Graphics.DrawMeshNow(m_DrawingLineMesh, m_Mesh.transform.localToWorldMatrix, 0);
                }
            }
        }

        /// <summary>
        /// Visual indications to help the user: highlighting faces, edges and vertices when snapping on them
        /// </summary>
        void DoVisualCues()
        {
            if(m_Mesh != null)
            {
                if(m_TargetFace == null)
                {
                    EditorHandleDrawing.HighlightVertices(m_Mesh, m_Mesh.sharedVertexLookup.Keys.ToArray(), false);
                    EditorHandleDrawing.HighlightEdges(m_Mesh, m_Mesh.faces.SelectMany(f => f.edges).Distinct().ToArray(),
                        false);

                    if(m_CurrentFace != null)
                        EditorHandleDrawing.HighlightFaces(m_Mesh, new Face[]{m_CurrentFace}, Color.Lerp(Color.blue, Color.cyan, 0.5f));
                }
                else
                {
                    var edges = m_TargetFace.edges;
                    EditorHandleDrawing.HighlightVertices(m_Mesh, edges.Select(e => e.a).ToArray(), false);
                    EditorHandleDrawing.HighlightEdges(m_Mesh, edges.ToArray(), false);

                    if(m_SnapedVertexId != -1)
                        EditorHandleDrawing.HighlightVertices(m_Mesh, new int[]{m_SnapedVertexId});

                    if(m_SnapedEdge != Edge.Empty)
                        EditorHandleDrawing.HighlightEdges(m_Mesh, new Edge[]{m_SnapedEdge});
                }
            }
            else if(MeshSelection.activeMesh != null)
            {
                ProBuilderMesh mesh = MeshSelection.activeMesh;
                EditorHandleDrawing.HighlightVertices(mesh, mesh.sharedVertexLookup.Keys.ToArray(), false);
                EditorHandleDrawing.HighlightEdges(mesh, mesh.faces.SelectMany(f => f.edges).ToArray(), false);
            }
        }

        /// <summary>
        /// Rebuild the line mesh when updated
        /// </summary>
        void RebuildCutShape()
        {
            // If Undo is called immediately after creation this situation can occur
            if (m_Mesh == null)
                return;

            DrawPolyLine(m_CutPath.Select(tup => tup.position).ToList());

            // While the vertex count may not change, the triangle winding might. So unfortunately we can't take
            // advantage of the `vertexCountChanged = false` optimization here.
            ProBuilderEditor.Refresh();
        }

        /// <summary>
        /// Draw the line corresponding to the cut and the closure if needed
        /// </summary>
        /// <param name="points">Positions of the cut points</param>
        void DrawPolyLine(List<Vector3> points)
        {
            if(m_LineMesh)
                m_LineMesh.Clear();

            if (points.Count < 2)
                return;

            int vc = points.Count;

            Vector3[] ver = new Vector3[vc];
            Vector2[] uvs = new Vector2[vc];
            int[] indexes = new int[vc];
            int cnt = points.Count;
            float distance = 0f;

            for (int i = 0; i < vc; i++)
            {
                Vector3 a = points[i % cnt];
                Vector3 b = points[i < 1 ? 0 : i - 1];

                float d = Vector3.Distance(a, b);
                distance += d;

                ver[i] = points[i % cnt];
                uvs[i] = new Vector2(distance, 1f);
                indexes[i] = i;
            }

            m_LineMesh.name = "Cut Guide";
            m_LineMesh.vertices = ver;
            m_LineMesh.uv = uvs;
            m_LineMesh.SetIndices(indexes, MeshTopology.LineStrip, 0);
            m_LineMaterial.SetFloat("_LineDistance", distance);
        }

        void UpdateDashedLine(Mesh lineMesh, Vector3 fromPoint, Vector3 toPoint)
        {
            float lineLength = 0.1f, spaceLength = 0.05f;
            List<Vector3> ver = lineMesh.vertices.ToList();
            List<Vector2> uvs = lineMesh.uv.ToList();

            List<int> indexes = new List<int>();
            lineMesh.GetIndices(indexes,0);

            float d = Vector3.Distance(fromPoint, toPoint);
            Vector3 dir = ( toPoint - fromPoint ).normalized;
            int sections = (int)(d / (lineLength + spaceLength));

            int offset = ver.Count;
            for(int i = 0; i < sections; i++)
            {
                ver.Add(fromPoint + i * (lineLength + spaceLength) * dir);
                ver.Add(fromPoint + (i * (lineLength + spaceLength) + lineLength) * dir);

                uvs.Add(new Vector2( 1f, 1f));
                uvs.Add(new Vector2( 1f, 1f));

                indexes.Add(2*i + offset);
                indexes.Add(2*i+1 + offset);
            }

            ver.Add(fromPoint + sections * (lineLength + spaceLength) * dir);
            uvs.Add(new Vector2( 1f, 1f));
            indexes.Add(2 * sections + offset);


            if(d - (sections * ( lineLength + spaceLength )) > lineLength)
                ver.Add(fromPoint + ( sections * ( lineLength + spaceLength ) + lineLength ) * dir);
            else
                ver.Add(toPoint);
            uvs.Add(new Vector2( 1f, 1f));
            indexes.Add(2 * sections + 1 + offset);

            lineMesh.name = "DashedLine";
            lineMesh.vertices = ver.ToArray();
            lineMesh.uv = uvs.ToArray();
            lineMesh.SetIndices(indexes, MeshTopology.Lines, 0);
        }


        /// <summary>
        /// Draw a helper line between the last point of the cut and the current position of the mouse cursor
        /// </summary>
        /// <returns>true if the line can be traced (the position of the cursor must be valid and the cut have one point minimum)</returns>
        bool DrawGuideLine()
        {
            if(m_DrawingLineMesh)
                m_DrawingLineMesh.Clear();

            if(m_CurrentPosition.Equals(Vector3.positiveInfinity)
               || m_ModifyingPoint)
                return false;

            if(m_CutPath.Count > 0)
            {
                Vector3 lastPosition = m_CutPath[m_CutPath.Count - 1].position;
                Vector3 currentPosition = m_CurrentPosition;

                UpdateDashedLine(m_DrawingLineMesh, lastPosition, currentPosition);
                m_DrawingLineMaterial.SetFloat("_LineDistance", 1f);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Draw a helper line to show which vertices of the face are connected to points of the cut shape
        /// </summary>
        void UpdateMeshConnectionsLines()
        {
            if(m_ConnectionsLineMesh)
                m_ConnectionsLineMesh.Clear();

            if(m_MeshConnections.Count > 0)
            {
                Vertex[] vertices = m_Mesh.GetVertices();
                foreach(var connection in m_MeshConnections)
                {
                    UpdateDashedLine(m_ConnectionsLineMesh, m_CutPath[connection.item1].position, vertices[connection.item2].position);
                }
                m_ConnectionsLineMaterial.SetFloat("_LineDistance", 1f);
            }
        }

    }
}

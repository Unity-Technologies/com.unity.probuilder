using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.EditorTools;
using UnityEditor.Graphs;
using UnityEditor.ProBuilder.Actions;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Edge = UnityEngine.ProBuilder.Edge;
using Math = UnityEngine.ProBuilder.Math;
using UObject = UnityEngine.Object;
using RaycastHit = UnityEngine.ProBuilder.RaycastHit;
using UHandleUtility = UnityEditor.HandleUtility;


#if !UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.EditorTools;
#endif

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

        Material m_LineMaterial;
        Mesh m_LineMesh = null;
        static readonly Color k_LineMaterialBaseColor = new Color(0f, 136f / 255f, 1f, 1f);
        static readonly Color k_LineMaterialHighlightColor = new Color(0f, 200f / 255f, 170f / 200f, 1f);

        Material m_ClosingLineMaterial;
        Mesh m_ClosingLineMesh = null;
        static readonly Color k_ClosingLineMaterialBaseColor = new Color(1f, 170/200f, 0f, 1f);
        static readonly Color k_ClosingLineMaterialHighlightColor = new Color(1f, 100f / 200f, 0f, 1f);

        Material m_DrawingLineMaterial;
        Mesh m_DrawingLineMesh = null;
        static readonly Color k_DrawingLineMaterialBaseColor = new Color(0.01f, .9f, 0.3f, 1f);
        static readonly Color k_DrawingLineMaterialHighlightColor = new Color(0f, 1f, 0f, 1f);

        Color m_CurrentHandleColor = k_HandleColor;

        Face m_TargetFace;
        Face m_CurrentFace;
        Vector3 m_CurrentPosition = Vector3.positiveInfinity;
        Vector3 m_CurrentPositionNormal = Vector3.up;
        VertexTypes m_CurrentVertexTypes = VertexTypes.None;

        int m_ControlId;
        bool m_PlacingPoint;
        bool m_SnappingPoint;
        bool m_ModifyingPoint;
        int m_SelectedIndex = -2;

        int m_SnapedVertexId = -1;
        Edge m_SnapedEdge = Edge.Empty;

        //bool m_ToolInUse;

        [SerializeField]
        internal List<CutVertexData> m_cutPath = new List<CutVertexData>();
        GUIContent m_OverlayTitle;

        const string k_EdgeToEdgePrefKey = "VertexInsertion.edgeToEdge";
        const string k_ConnectToStartPrefKey = "VertexInsertion.connectToStart";
        const string k_EndOnClicToStartPrefKey = "VertexInsertion.endOnClicToStart";
        const string k_SnapToGeometryPrefKey = "VertexInsertion.snapToGeometry";
        const string k_SnappingDistancePrefKey = "VertexInsertion.snappingDistance";

        bool m_EdgeToEdge;
        bool m_ConnectToStart;
        bool m_EndOnClicToStart;
        bool m_SnapToGeometry;
        float m_SnappingDistance;

        public bool IsALoop
        {
            get
            {
                if (m_cutPath.Count < 3)
                    return false;
                else
                    return Math.Approx3(m_cutPath[0].position,
                        m_cutPath[m_cutPath.Count - 1].position);
            }
        }

        public int ConnectionsToFaceBordersCount
        {
            get
            {
                return m_cutPath.Count(data => (data.types & (VertexTypes.AddedOnEdge | VertexTypes.ExistingVertex)) != 0 );
            }
        }


        // void ActiveToolChanged()
        // {
        //     if(ToolManager.IsActiveTool(this))
        //         InitTool();
        //     else
        //         CloseTool();
        // }

        void OnEnable()
        {
            k_HandleColorUseExistingVertex = Handles.selectedColor;
            k_HandleColorAddVertexOnEdge = Handles.selectedColor;

            m_OverlayTitle = new GUIContent("Cut Tool");
            m_EdgeToEdge = EditorPrefs.GetBool( k_EdgeToEdgePrefKey, false );
            m_ConnectToStart = EditorPrefs.GetBool( k_ConnectToStartPrefKey, false );
            m_EndOnClicToStart = EditorPrefs.GetBool( k_EndOnClicToStartPrefKey, false );
            m_SnapToGeometry = EditorPrefs.GetBool( k_SnapToGeometryPrefKey, false );
            m_SnappingDistance = EditorPrefs.GetFloat( k_SnappingDistancePrefKey, 0.1f );

            m_CutCursorTexture = Resources.Load<Texture2D>("Cursors/cutCursor");
            m_CutAddCursorTexture = Resources.Load<Texture2D>("Cursors/cutCursor-add");
            m_CutMagnetCursorTexture = Resources.Load<Texture2D>("Cursors/cutCursor-magnet");
            m_CutAddMagnetCursorTexture = Resources.Load<Texture2D>("Cursors/cutCursor-add-magnet");

            //ToolManager.activeToolChanged += ActiveToolChanged;
            EditorApplication.update += Update;
            Undo.undoRedoPerformed += UndoRedoPerformed;
            //ProBuilderEditor.selectModeChanged += OnSelectModeChanged;

            if(MeshSelection.selectedObjectCount == 1)
                m_Mesh = MeshSelection.activeMesh;

            InitLineRenderers();
            //InitTool();
        }

        // void InitTool()
        // {
        //     InitLineRenderers();
        //     m_ToolInUse = true;
        // }

        void OnDisable()
        {
            //ToolManager.activeToolChanged -= ActiveToolChanged;

            EditorApplication.update -= Update;
            Undo.undoRedoPerformed -= UndoRedoPerformed;
            //ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;

            CloseTool();
        }

        void CloseTool()
        {
            //if(m_ToolInUse)
                ExecuteCut();

            Clear();

            //m_ToolInUse = false;
        }

        void InitLineRenderers()
        {
            m_LineMesh = new Mesh();
            m_LineMaterial = CreateLineMaterial(k_LineMaterialBaseColor, k_LineMaterialHighlightColor);

            if(m_ConnectToStart)
            {
                m_ClosingLineMesh = new Mesh();
                m_ClosingLineMaterial =
                    CreateLineMaterial(k_ClosingLineMaterialBaseColor, k_ClosingLineMaterialHighlightColor);
            }

            m_DrawingLineMesh = new Mesh();
            m_DrawingLineMaterial = CreateLineMaterial(k_DrawingLineMaterialBaseColor, k_DrawingLineMaterialHighlightColor);
        }

        void ClearLineRenderers()
        {
            if(m_LineMesh)
                DestroyImmediate(m_LineMesh);
            if(m_LineMaterial)
                DestroyImmediate(m_LineMaterial);

            if(m_ClosingLineMesh)
                DestroyImmediate(m_ClosingLineMesh);
            if(m_ClosingLineMaterial)
                DestroyImmediate(m_ClosingLineMaterial);

            if(m_DrawingLineMesh != null)
                DestroyImmediate(m_DrawingLineMesh);
            if(m_DrawingLineMaterial != null)
                DestroyImmediate(m_DrawingLineMaterial);
        }

        void Clear()
        {
            ClearLineRenderers();

            m_Mesh = null;
            m_TargetFace = null;
            m_CurrentFace = null;
            m_PlacingPoint = false;
            m_CurrentCutCursor = null;
        }

        void Reset()
        {
            Clear();
            InitLineRenderers();
        }

        static Material CreateLineMaterial(Color baseColor, Color highlightColor)
        {
            Material mat = new Material(Shader.Find("Hidden/ProBuilder/ScrollHighlight"));
            mat.SetColor("_Base", baseColor);
            mat.SetColor("_Highlight", highlightColor);
            return mat;
        }

        void Update()
        {
            //if(!m_ToolInUse)
            //    return;

            //Cursor.SetCursor(m_CurrentCutCursor, Vector2.zero, CursorMode.ForceSoftware);
            //Cursor.SetCursor(m_CurrentCutCursor, Vector2.zero, CursorMode.Auto);

            if (m_Mesh != null && m_LineMaterial != null)
                m_LineMaterial.SetFloat("_EditorTime", (float) EditorApplication.timeSinceStartup);
            if (m_Mesh != null && m_ClosingLineMaterial != null)
                m_ClosingLineMaterial.SetFloat("_EditorTime", (float)EditorApplication.timeSinceStartup);
            if (m_Mesh != null && m_DrawingLineMaterial != null)
                m_DrawingLineMaterial.SetFloat("_EditorTime", (float)EditorApplication.timeSinceStartup);
        }

        private void UndoRedoPerformed()
        {
            ClearLineRenderers();
            InitLineRenderers();

            if(m_cutPath.Count == 0)
                m_TargetFace = null;

            m_SelectedIndex = -1;

            EditorApplication.delayCall = () => RebuildCutShape();
        }

        // private void OnSelectModeChanged(SelectMode mode)
        // {
        //     if(!m_ToolInUse)
        //         return;
        //
        //     //ActionResult result = DoCut();
        // }

        public override void OnToolGUI( EditorWindow window )
        {
            if(Event.current.type == EventType.Repaint)
            {
                Cursor.SetCursor(m_CurrentCutCursor, Vector2.zero, CursorMode.Auto);
                if(m_CurrentCutCursor != null)
                {
                    Rect sceneViewRect = window.position;
                    sceneViewRect.x = 0;
                    sceneViewRect.y = 0;
                    SceneView.AddCursorRect(sceneViewRect, MouseCursor.CustomCursor);
                }
            }

            SceneViewOverlay.Window( m_OverlayTitle, OnOverlayGUI, 0, SceneViewOverlay.WindowDisplayOption.OneWindowPerTitle );

            if(m_Mesh != null)
            {
                if(m_LineMaterial != null)
                {
                    m_LineMaterial.SetPass(0);
                    Graphics.DrawMeshNow(m_LineMesh, m_Mesh.transform.localToWorldMatrix, 0);
                }

                if(m_DrawingLineMaterial != null)
                {
                    bool drawline = DrawGuideLine();
                    if(drawline)
                    {
                    m_DrawingLineMaterial.SetPass(0);
                    Graphics.DrawMeshNow(m_DrawingLineMesh, m_Mesh.transform.localToWorldMatrix, 0);
                    }
                }

                if(!m_ConnectToStart && m_ClosingLineMaterial)
                    DestroyImmediate(m_ClosingLineMaterial);

                if(m_ClosingLineMaterial == null && m_ConnectToStart)
                {
                    m_ClosingLineMaterial = CreateLineMaterial(k_LineMaterialBaseColor, k_ClosingLineMaterialHighlightColor);
                    RebuildCutShape();
                }

                if(m_ClosingLineMaterial != null && m_ConnectToStart)
                {
                    m_ClosingLineMaterial.SetPass(0);
                    Graphics.DrawMeshNow(m_ClosingLineMesh, m_Mesh.transform.localToWorldMatrix, 0);
                }

                DoExistingPointsGUI();
            }

            var currentEvent = Event.current;

            if (currentEvent.type == EventType.KeyDown)
                HandleKeyEvent(currentEvent);

            if(m_Mesh != null)
            {
                 m_ControlId = GUIUtility.GetControlID(FocusType.Passive);
                 if(currentEvent.type == EventType.Layout)
                     HandleUtility.AddDefaultControl(m_ControlId);

                 DoPointPlacement();
                 DoVisualCues();
            }
        }



        void OnOverlayGUI(UObject target, SceneView view)
        {
            var rect = EditorGUILayout.GetControlRect(false, 45, GUILayout.Width(250));
            if (MeshSelection.selectedObjectCount < 1)
                EditorGUI.HelpBox(rect, "A ProBuilderMesh must be selected to start a cut.", MessageType.Info);
            else if (MeshSelection.selectedObjectCount > 1)
                EditorGUI.HelpBox(rect, "Only one ProBuilder mesh must be selected.", MessageType.Warning);
            else
                EditorGUI.HelpBox(rect, "Click to start inserting new vertices in the shape.", MessageType.Info);

            using(new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Cut From Edge To Edge", GUILayout.Width(225));
                GUILayout.FlexibleSpace();
                m_EdgeToEdge = EditorGUILayout.Toggle(m_EdgeToEdge);
                EditorPrefs.SetBool( k_EdgeToEdgePrefKey,m_EdgeToEdge );
            }

            using(new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Connect End to Start Point", GUILayout.Width(225));
                GUILayout.FlexibleSpace();
                m_ConnectToStart = EditorGUILayout.Toggle(m_ConnectToStart);
                EditorPrefs.SetBool(k_ConnectToStartPrefKey, m_ConnectToStart);
            }

            using(new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Selecting Start Point is ending cut", GUILayout.Width(225));
                GUILayout.FlexibleSpace();
                m_EndOnClicToStart = EditorGUILayout.Toggle(m_EndOnClicToStart);
                EditorPrefs.SetBool(k_EndOnClicToStartPrefKey, m_EndOnClicToStart);
            }

            using(new GUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Snap to existing edges and vertices", GUILayout.Width(225));
                GUILayout.FlexibleSpace();
                m_SnapToGeometry = EditorGUILayout.Toggle(m_SnapToGeometry);
                EditorPrefs.SetBool(k_SnapToGeometryPrefKey, m_SnapToGeometry);
            }

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
                if(GUILayout.Button("Start Cut"))
                {
                    m_Mesh = MeshSelection.activeMesh;
                    if(m_cutPath.Count > 0)
                        m_cutPath.Clear();
                }
            }
            else
            {
                if(GUILayout.Button("Compute Cut"))
                {
                    DoCut();
                }
                if(GUILayout.Button("Cancel"))
                {
                    Reset();
                }
            }


            GUI.enabled = true;
        }

        Texture2D GetCursorTexture()
        {
            Texture2D texture = m_CutCursorTexture;
            if(m_ModifyingPoint)
                return texture;

            if(m_cutPath.Count > 0)
                texture = m_SnapToGeometry ? m_CutAddMagnetCursorTexture : m_CutAddCursorTexture;
            else if(m_SnapToGeometry)
                texture = m_CutMagnetCursorTexture;

            return texture;
        }

        private void DoPointPlacement()
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
                        CutVertexData data = m_cutPath[m_SelectedIndex];
                        data.position = m_CurrentPosition;
                        m_cutPath[m_SelectedIndex] = data;
                        RebuildCutShape(false);
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
                int polyCount = m_cutPath.Count;
                if (!m_CurrentPosition.Equals(Vector3.positiveInfinity)
                    && (polyCount == 0 || m_SelectedIndex != polyCount - 1))
                {
                    UndoUtility.RecordObject(this, "Add Vertex On Path");

                    if (m_TargetFace == null)
                        m_TargetFace = m_CurrentFace;

                    m_cutPath.Add(new CutVertexData(m_CurrentPosition,m_CurrentPositionNormal, m_CurrentVertexTypes));

                    m_PlacingPoint = true;
                    m_SelectedIndex = m_cutPath.Count - 1;

                    RebuildCutShape();

                    if (CheckForEditionEnd())
                        ExecuteCut();

                    evt.Use();
                }
            }
        }

        private bool UpdateHitPosition()
        {
            Event evt = Event.current;

            Ray ray = UHandleUtility.GUIPointToWorldRay(evt.mousePosition);
            RaycastHit pbHit;

            m_CurrentFace = null;

            if (UnityEngine.ProBuilder.HandleUtility.FaceRaycast(ray, m_Mesh, out pbHit))
            {
                m_CurrentPosition = pbHit.point;
                m_CurrentPositionNormal = pbHit.normal;
                m_CurrentFace = m_Mesh.faces[pbHit.face];
                m_CurrentVertexTypes = VertexTypes.None;

                if(m_SnappingPoint || m_ModifyingPoint)
                    CheckPointInCutPath();

                if (m_CurrentVertexTypes == VertexTypes.None && !m_ModifyingPoint)
                    CheckPointInMesh();

                return true;
            }

            return false;
        }

        private void ExecuteCut()
        {
            ActionResult result = DoCut();
            EditorUtility.ShowNotification(result.notification);
        }

        private ActionResult DoCut()
        {
            if (m_TargetFace == null || m_cutPath.Count < 2)
            {
                return new ActionResult(ActionResult.Status.Canceled, "Not enough elements selected for a cut");
            }

            if (!IsALoop)
            {
                if (m_ConnectToStart && m_cutPath.Count > 2)
                {
                    m_cutPath.Add(new CutVertexData(m_cutPath[0].position, VertexTypes.VertexInShape));
                }
                else
                {
    //                PruneOrphans(); ?
    //                while ( (polygonalCut.m_verticesToAdd[0].m_Type & (VertexType.ExistingVertex | VertexType.AddedOnEdge)) == 0)
    //                {
    //                    remove and destroy polygonalCut.m_verticesToAdd[0]
    //                }
                }
            }

            UndoUtility.RecordObject(m_Mesh, "Add Face To Mesh");

            List<Vertex> cutVertices = InsertVertices();
            List<Vertex> meshVertices = m_Mesh.GetVertices().ToList();
            int[] cutIndexes = cutVertices.Select(vert => meshVertices.IndexOf(vert)).ToArray();

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

            }

            switch (ConnectionsToFaceBordersCount)
            {
                case 0:
                    //If the cut does not touches the face edges, it will create a hole in the face
                    //The creation of this shape is specific and different from others
                    DoFaceWithHole(m_TargetFace, cutIndexes);
                    break;
                case 1:
                    //If only one vertex touches the edge of the face, it means the outter shape
                    //will have a singularity point, then the face creation is split in 2 phases
                    List<int[]> indexes = ComputePolygonsIndexes(m_TargetFace, cutIndexes);
                    foreach (var polygon in indexes)
                    {
                        //Create the face with a missing triangle
                        Face face = m_Mesh.CreatePolygon(polygon,false);

                        //Compute the missing triangle/polygon
                        int[] complementaryPolygon = GetComplementaryPolygons(polygon);
                        if (complementaryPolygon != null)
                        {
                            //Create the missing triangle
                            Face compFace = m_Mesh.CreatePolygon(complementaryPolygon, false);
                            //Merge the face plus the missing triangle that define together the full face
                            //Face mergedFace = MergeElements.Merge(polygonalCut.mesh, new[] {face, compFace});

                            //For safety, triangulate the new surface and make quad geometry from there
                            var triangulatedFaces = m_Mesh.ToTriangles(new Face[]{face,compFace});
                            m_Mesh.ToQuad(triangulatedFaces);
                        }
                    }
                    break;
                default:
                    //Compute the polygons defined in the face
                    List<int[]> verticesIndexes = ComputePolygonsIndexes(m_TargetFace, cutIndexes);

                    //Create these new polygonal faces
                    foreach (var polygon in verticesIndexes)
                    {
                        m_Mesh.CreatePolygon(polygon,false);
                    }
                break;
            }

            //Delete former face
            m_Mesh.DeleteFace(m_TargetFace);

            m_Mesh.ToMesh();
            m_Mesh.Refresh();
            m_Mesh.Optimize();

            m_cutPath.Clear();
            RebuildCutShape(true);

            Reset();

            return ActionResult.Success;
        }

        private Face DoFaceWithHole(Face face, IList<int> cutVertexIndexes)
        {
            List<Edge> peripheralEdges = WingedEdge.SortEdgesByAdjacency(face);
            List<int> borderIndexes = peripheralEdges.Select(edge => edge.a).ToList();

            IList<IList<int>> holes = new List<IList<int>>();
            holes.Add(cutVertexIndexes);

            Face newFace = m_Mesh.CreatePolygonWithHole(borderIndexes, holes);

            //For safety, triangulate the new surface and make quad geometry from there
            var triangulatedFaces = m_Mesh.ToTriangles(new Face[]{newFace});
            m_Mesh.ToQuad(triangulatedFaces);

            return newFace;
        }


        private List<int[]> ComputePolygonsIndexes(Face face, IList<int> cutVertexIndexes)
        {
            List<int[]> polygons =new List<int[]>();

            //Get Vertices from the mesh
            Dictionary<int, int> sharedToUnique = m_Mesh.sharedVertexLookup;

            List<int> cutVertexSharedIndexes = cutVertexIndexes.Select(ind => sharedToUnique[ind]).ToList();

            //Parse peripheral edges to unique id and find a common point between the mesh and the cut
            List<Edge> peripheralEdges = WingedEdge.SortEdgesByAdjacency(face);
            List<Edge> peripheralEdgesUnique = new List<Edge>();
            int startIndex = -1;
            for (int i = 0; i < peripheralEdges.Count; i++)
            {
                Edge e = peripheralEdges[i];
                Edge eShared = new Edge();
                eShared.a = sharedToUnique[e.a];
                eShared.b = sharedToUnique[e.b];

                peripheralEdgesUnique.Add(eShared);

                if (cutVertexSharedIndexes.Contains(eShared.a) && startIndex == -1)
                    startIndex = i;
            }

            //Create a polygon for each cut reaching the mesh edges
            List<int> polygon = new List<int>();
            Edge previousEdge = new Edge(-1,-1);
            for (int i = startIndex; i <= peripheralEdgesUnique.Count + startIndex; i++)
            {
                 Edge e = peripheralEdgesUnique[i % peripheralEdgesUnique.Count];

                 polygon.Add(peripheralEdges[i % peripheralEdgesUnique.Count].a);
                 if(polygon.Count > 1 && cutVertexSharedIndexes.Contains(e.a)) // get next vertex
                 {
                     List<int> closure = ClosePolygonalCut(polygon[0], previousEdge, e.a, cutVertexSharedIndexes);
                     polygon.AddRange(closure);
                     polygons.Add(polygon.ToArray());

                     //Start a new polygon
                     polygon = new List<int>();
                     polygon.Add(peripheralEdges[i % peripheralEdgesUnique.Count].a);
                 }

                 previousEdge = e;
            }

            return polygons;
        }

        private List<int> ClosePolygonalCut(int polygonFirstVertex ,Edge previousEdge, int currentIndex, List<int> cutIndexes)
        {
            List<int> closure = new List<int>();
            List<Vertex> meshVertices = m_Mesh.GetVertices().ToList();
            IList<SharedVertex> uniqueIdToVertexIndex = m_Mesh.sharedVertices;

            Vector3 previousEdgeDir = meshVertices[uniqueIdToVertexIndex[previousEdge.b][0]].position -
                                      meshVertices[uniqueIdToVertexIndex[previousEdge.a][0]].position;
            previousEdgeDir.Normalize();

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

                        float similarityToPrevious = Vector3.Dot(previousEdgeDir, previousVertexDir);
                        if (similarityToPrevious < bestCandidate) //Go to previous
                        {
                            bestCandidate = similarityToPrevious;
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

                        float similarityToNext = Vector3.Dot(previousEdgeDir, nextVertexDir);
                        if (similarityToNext < bestCandidate) // Go to next
                        {
                            bestCandidate = similarityToNext;
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
                    if(sharedToUnique[vertexIndex] == sharedToUnique[polygonFirstVertex])
                        break;
                }
            }
            else if (successorDirection == 1)
            {
                for (int i = bestSuccessorIndex; i < (bestSuccessorIndex + cutIndexes.Count); i++)
                {
                    int vertexIndex = uniqueIdToVertexIndex[cutIndexes[i % cutIndexes.Count]][0];
                    closure.Add(vertexIndex);
                    if(sharedToUnique[vertexIndex] == sharedToUnique[polygonFirstVertex])
                        break;
                }
            }

            return closure;
        }


        private int[] GetComplementaryPolygons(int[] indexes)
        {
            for (int i = 0; i < indexes.Length; i++)
            {
                for (int j = i+1; j < indexes.Length; j++)
                {
                    //Is it the vertex to duplicate?
                    if (indexes[i] == indexes[j])
                    {
                        int[] complementaryPoly = new int[3];
                        complementaryPoly[0] = indexes[j - 1];
                        complementaryPoly[1] = indexes[j];
                        complementaryPoly[2] = indexes[j + 1];
                        return complementaryPoly;
                    }
                }
            }
            return null;
        }

        private void CheckPointInCutPath()
        {
            //For now the method is only used to moved points of the cut Path
            //The idea is to extend it to be able to create a cut that crosses a
            //single vertex multiple times
            if(!m_ModifyingPoint)
                return;

            float snapDistance = m_SnappingDistance;
            for (int i = 0; i < m_cutPath.Count; i++)
            {
                var vertexData = m_cutPath[i];
                if( Math.Approx3( vertexData.position,
                    m_CurrentPosition,
                    snapDistance ) )
                {
                    snapDistance = Vector3.Distance( vertexData.position, m_CurrentPosition );
                    if( !m_ModifyingPoint )
                        m_CurrentPosition = vertexData.position;
                    m_CurrentVertexTypes = vertexData.types | VertexTypes.VertexInShape;
                    m_SelectedIndex = i;
                }
            }
        }

        private void CheckPointInMesh()
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

        private List<Vertex> InsertVertices()
        {
            List<Vertex> newVertices = new List<Vertex>();

            foreach (var vertexData in m_cutPath)
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
                        newVertices.Add(m_Mesh.InsertVertexInMeshSimple(vertexData.position,vertexData.normal));
                        break;
                    default:
                        break;
                }
            }

            return newVertices;
        }

        private Vertex InsertVertexOnExistingVertex(Vector3 vertexPosition)
        {
            Vertex vertex = null;

            List<Vertex> vertices = m_Mesh.GetVertices().ToList();
            for (int vertIndex = 0; vertIndex < vertices.Count; vertIndex++)
            {
                if (UnityEngine.ProBuilder.Math.Approx3(vertices[vertIndex].position, vertexPosition))
                {
                    vertex = vertices[vertIndex];
                    break;
                }
            }

            return vertex;
        }

        private Vertex InsertVertexOnExistingEdge(Vector3 vertexPosition)
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

        private bool CheckForEditionEnd()
        {
            if (m_TargetFace == null || m_cutPath.Count < 2)
                return false;

            if (m_EndOnClicToStart)
            {
                return Math.Approx3(m_cutPath[0].position,m_cutPath[m_cutPath.Count - 1].position);
            }

            if (m_EdgeToEdge)
            {
                return (m_cutPath[0].types
                            & (VertexTypes.AddedOnEdge | VertexTypes.ExistingVertex)) != 0
                       && (m_cutPath[m_cutPath.Count - 1].types
                           & (VertexTypes.AddedOnEdge | VertexTypes.ExistingVertex)) != 0;
            }

            return false;
        }

        void HandleKeyEvent(Event evt)
        {
            KeyCode key = evt.keyCode;

            switch (key)
            {
                case KeyCode.Backspace:
                {
                    UndoUtility.RecordObject(m_Mesh, "Delete Selected Points");
                    m_cutPath.RemoveAt(m_cutPath.Count-1);
                    RebuildCutShape(true);
                    evt.Use();
                    break;
                }

                case KeyCode.Escape:
                    evt.Use();
                    ExecuteCut();
                    //Leave the current tool
                    Tools.current = Tool.None;
                    break;
            }
        }

        void DoExistingPointsGUI()
        {
            Transform trs = m_Mesh.transform;
            int len = m_cutPath.Count;

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
                    Vector3 point = trs.TransformPoint(m_cutPath[index].position);
                    float size = HandleUtility.GetHandleSize(point) * k_HandleSize;

                    Handles.color = k_HandleColor;
                    Handles.DotHandleCap(-1, point, Quaternion.identity, size, evt.type);

                    // "clicked" a button
                    if (!used && evt.type == EventType.Used)
                    {
                        used = true;
                    }
                }

                if (!m_CurrentPosition.Equals(Vector3.positiveInfinity))
                {
                    Vector3 point = trs.TransformPoint(m_CurrentPosition);
                    if(m_SelectedIndex >= 0 && m_SelectedIndex < m_cutPath.Count)
                        point = trs.TransformPoint(m_cutPath[m_SelectedIndex].position);

                    float size = HandleUtility.GetHandleSize(point) * k_HandleSize;

                    Handles.color = m_CurrentHandleColor;

                    Handles.DotHandleCap(-1, point, Quaternion.identity, size, evt.type);
                }
                Handles.color = Color.white;
            }
        }

        private void DoVisualCues()
        {
            if(m_Mesh != null)
            {
                if(m_TargetFace == null)
                {
                    EditorMeshHandles.HighlightVertices(m_Mesh, m_Mesh.sharedVertexLookup.Keys.ToArray(), false);
                    EditorMeshHandles.HighlightEdges(m_Mesh, m_Mesh.faces.SelectMany(f => f.edges).Distinct().ToArray(),
                        false);

                    if(m_CurrentFace != null)
                        EditorMeshHandles.HighlightFaces(m_Mesh, new Face[]{m_CurrentFace}, Color.Lerp(Color.blue, Color.cyan, 0.5f));
                }
                else
                {
                    var edges = m_TargetFace.edges;
                    EditorMeshHandles.HighlightVertices(m_Mesh, edges.Select(e => e.a).ToArray(), false);
                    EditorMeshHandles.HighlightEdges(m_Mesh, edges.ToArray(), false);

                    if(m_SnapedVertexId != -1)
                        EditorMeshHandles.HighlightVertices(m_Mesh, new int[]{m_SnapedVertexId});

                    if(m_SnapedEdge != Edge.Empty)
                        EditorMeshHandles.HighlightEdges(m_Mesh, new Edge[]{m_SnapedEdge});
                }
            }
            else if(MeshSelection.activeMesh != null)
            {
                ProBuilderMesh mesh = MeshSelection.activeMesh;
                EditorMeshHandles.HighlightVertices(mesh, mesh.sharedVertexLookup.Keys.ToArray(), false);
                EditorMeshHandles.HighlightEdges(mesh, mesh.faces.SelectMany(f => f.edges).ToArray(), false);
            }
        }

        public void RebuildCutShape(bool vertexCountChanged = false)
        {
            // If Undo is called immediately after creation this situation can occur
            if (m_Mesh == null)
                return;

            DrawPolyLine(m_cutPath.Select(tup => tup.position).ToList());

            // While the vertex count may not change, the triangle winding might. So unfortunately we can't take
            // advantage of the `vertexCountChanged = false` optimization here.
            ProBuilderEditor.Refresh();
        }

        void DrawPolyLine(List<Vector3> points)
        {
            if(m_LineMesh)
                m_LineMesh.Clear();
            if(m_ClosingLineMesh)
                m_ClosingLineMesh.Clear();

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

            if (m_ConnectToStart && points.Count > 2)
            {
                Vector3 a = points[vc - 1], b = points[0];

                m_ClosingLineMesh.name = "Cut Closure";
                m_ClosingLineMesh.vertices = new Vector3[]{ a , b };
                m_ClosingLineMesh.uv = new Vector2[]{new Vector2(0,1), Vector2.one };;
                m_ClosingLineMesh.SetIndices(new int[]{0,1}, MeshTopology.LineStrip, 0);
                m_ClosingLineMaterial.SetFloat("_LineDistance", Vector3.Distance(a, b));
            }
            else
            {
                if(m_ClosingLineMesh != null)
                {
                    m_ClosingLineMesh.Clear();
                    m_ClosingLineMesh.name = "Poly Shape End";
                }
            }

        }

        bool DrawGuideLine()
        {
            if(m_DrawingLineMesh)
                m_DrawingLineMesh.Clear();

            if(m_CurrentPosition.Equals(Vector3.positiveInfinity)
            || m_ModifyingPoint)
                return false;

            Vector3[] ver;
            Vector2[] uvs;
            int[] indexes;
            float lineLength = 0.1f, spaceLength = 0.05f;
            if(m_cutPath.Count > 0)
            {
                Vector3 lastPosition = m_cutPath[m_cutPath.Count - 1].position;
                Vector3 currentPosition = m_CurrentPosition;

                float d = Vector3.Distance(lastPosition, currentPosition);
                Vector3 dir = ( currentPosition - lastPosition ).normalized;
                int sections = (int)(d / (lineLength + spaceLength));

                ver = new Vector3[2 * sections + 2];
                uvs = new Vector2[2 * sections + 2];
                indexes = new int[2 * sections + 2];
                for(int i = 0; i < sections; i++)
                {
                    ver[2*i] = lastPosition + i * (lineLength + spaceLength) * dir;
                    ver[2*i+1] = lastPosition + (i * (lineLength + spaceLength) + lineLength) * dir;

                    uvs[2*i] = new Vector2( ver[i].magnitude/ d, 1f);
                    uvs[2*i+1] = new Vector2( ver[i+1].magnitude/ d, 1f);

                    indexes[2*i] = 2*i;
                    indexes[2*i+1] = 2*i+1;
                }

                int len = ver.Length;
                ver[len - 2] = lastPosition + sections * (lineLength + spaceLength) * dir;
                uvs[len - 2] = new Vector2( ver[len - 2].magnitude/ d, 1f);
                indexes[len - 2] = len - 2;


                if(d - (sections * ( lineLength + spaceLength )) > lineLength)
                    ver[len - 1] = lastPosition + ( sections * ( lineLength + spaceLength ) + lineLength ) * dir;
                else
                    ver[len - 1] = currentPosition;
                uvs[len - 1] = new Vector2( 1f, 1f);
                indexes[len - 1] = len - 1;

                m_DrawingLineMesh.name = "Drawing Guide";
                m_DrawingLineMesh.vertices = ver;
                m_DrawingLineMesh.uv = uvs;
                m_DrawingLineMesh.SetIndices(indexes, MeshTopology.Lines, 0);
                m_DrawingLineMaterial.SetFloat("_LineDistance", 1f);

                return true;
            }

            return false;
        }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Math = System.Math;
using RaycastHit = UnityEngine.ProBuilder.RaycastHit;
using UHandleUtility = UnityEditor.HandleUtility;

namespace UnityEditor.ProBuilder
{
    [CustomEditor(typeof(PolygonalCut))]
    public class PolygonalCutEditor : Editor
    {
#region Variables

        static Color k_HandleColor = new Color(.8f, .8f, .8f, 1f);
        static Color k_HandleColorAddNewVertex = new Color(.01f, .9f, .3f, 1f);
        static Color k_HandleColorAddVertexOnEdge = new Color(.3f, .01f, .9f, 1f);
        static Color k_HandleColorUseExistingVertex = new Color(.9f, .3f, .01f, 1f);
        static Color k_HandleSelectedColor = new Color(.01f, .8f, .98f, 1f);

        const float k_HandleSize = .05f;

        static Color k_LineMaterialBaseColor = new Color(0f, 136f / 255f, 1f, 1f);
        static Color k_LineMaterialHighlightColor = new Color(0f, 200f / 255f, 170f / 200f, 1f);

        static Color k_InvalidLineMaterialColor = Color.red;

        Material m_LineMaterial;
        Mesh m_LineMesh = null;

        static Material CreateHighlightLineMaterial()
        {
            Material mat = new Material(Shader.Find("Hidden/ProBuilder/ScrollHighlight"));
            mat.SetColor("_Highlight", k_LineMaterialHighlightColor);
            mat.SetColor("_Base", k_LineMaterialBaseColor);
            return mat;
        }

        PolygonalCut polygonalCut
        {
            get { return target as PolygonalCut; }
        }

        private Face m_TargetFace = null;
        private Face m_CurrentFace;
        private Vector3 m_CurrentPositionToAdd = Vector3.positiveInfinity;
        public PolygonalCut.VertexType m_VertexType = PolygonalCut.VertexType.None;

        private int m_ControlId;
        bool m_PlacingPoint = false;
        int m_SelectedIndex = -2;

        #endregion

#region Unity Callbacks

        void OnEnable()
        {
            if (polygonalCut == null)
            {
                DestroyImmediate(this);
                return;
            }

            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;

            m_LineMesh = new Mesh();
            m_LineMaterial = CreateHighlightLineMaterial();

            Undo.undoRedoPerformed += UndoRedoPerformed;
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += DuringSceneGUI;
#else
            SceneView.onSceneGUIDelegate += DuringSceneGUI;
#endif
            EditorApplication.update += Update;
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        void OnDisable()
        {
            // Quit Edit mode when the object gets de-selected.
            if (polygonalCut != null)
                polygonalCut.polygonEditMode = PolygonalCut.PolygonEditMode.None;

            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= DuringSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= DuringSceneGUI;
#endif
            Undo.undoRedoPerformed -= UndoRedoPerformed;
            EditorApplication.update -= Update;

            DestroyImmediate(m_LineMesh);
            DestroyImmediate(m_LineMaterial);

            //Removing the script from the object
            DestroyImmediate(polygonalCut);
        }

        void Update()
        {
            if (polygonalCut.mesh != null && polygonalCut.polygonEditMode == PolygonalCut.PolygonEditMode.Add && m_LineMaterial != null)
                m_LineMaterial.SetFloat("_EditorTime", (float)EditorApplication.timeSinceStartup);
        }

#endregion

#region Callbacks

        private void DuringSceneGUI(SceneView obj)
        {
            if (polygonalCut.polygonEditMode == PolygonalCut.PolygonEditMode.None)
                return;

            if (m_LineMaterial != null)
            {
                m_LineMaterial.SetPass(0);
                Graphics.DrawMeshNow(m_LineMesh, polygonalCut.transform.localToWorldMatrix, 0);
            }

            Event currentEvent = Event.current;

            DoExistingPointsGUI();

            if (currentEvent.type == EventType.KeyDown)
                HandleKeyEvent(currentEvent);

            if (EditorHandleUtility.SceneViewInUse(currentEvent))
                return;

            m_ControlId = GUIUtility.GetControlID(FocusType.Passive);
            if (currentEvent.type == EventType.Layout)
                HandleUtility.AddDefaultControl(m_ControlId);

            DoPointPrePlacement();
            DoPointPlacement();
        }

        private void UndoRedoPerformed()
        {
            if (m_LineMesh != null)
                DestroyImmediate(m_LineMesh);

            if (m_LineMaterial != null)
                DestroyImmediate(m_LineMaterial);

            m_LineMesh = new Mesh();
            m_LineMaterial = CreateHighlightLineMaterial();

            RebuildPolygonalShape(polygonalCut);
        }

        private void OnSelectModeChanged(SelectMode obj)
        {
            //throw new System.NotImplementedException();
        }

#endregion

#region Point Placement

        void DoPointPrePlacement()
        {
            Event evt = Event.current;
            EventType evtType = evt.type;

            if (evtType== EventType.Repaint
                && polygonalCut.polygonEditMode == PolygonalCut.PolygonEditMode.Add)
            {
                Ray ray = UHandleUtility.GUIPointToWorldRay(evt.mousePosition);
                RaycastHit pbHit;

                m_CurrentFace = null;

                if (UnityEngine.ProBuilder.HandleUtility.FaceRaycast(ray, polygonalCut.mesh, out pbHit))
                {
                    m_CurrentPositionToAdd = pbHit.point;
                    m_CurrentFace = polygonalCut.mesh.faces[pbHit.face];
                    m_VertexType = PolygonalCut.VertexType.None;

                    CheckPointProjectionInPolyshape();

                    if(m_VertexType == PolygonalCut.VertexType.None)
                        CheckPointProjectionInMesh();

                    Vector3 point = polygonalCut.transform.TransformPoint(m_CurrentPositionToAdd);
                    float size = HandleUtility.GetHandleSize(point) * k_HandleSize;

                    if (!m_PlacingPoint)
                    {
                        if(m_VertexType == PolygonalCut.VertexType.ExistingVertex)
                            Handles.color = k_HandleColorUseExistingVertex;
                        else if(m_VertexType == PolygonalCut.VertexType.AddedOnEdge)
                            Handles.color = k_HandleColorUseExistingVertex;
                        else
                            Handles.color = k_HandleColorAddNewVertex;
                    }

                    Handles.DotHandleCap(-1, point, Quaternion.identity, size, evtType);
                }
                else
                {
                    m_CurrentPositionToAdd = Vector3.positiveInfinity;
                    m_VertexType = PolygonalCut.VertexType.None;
                }
            }
        }

        private void CheckPointProjectionInPolyshape()
        {
            foreach (PolygonalCut.InsertedVertexData vertexData in polygonalCut.m_verticesToAdd)
            {
                float snapDistance = 0.1f;
                if (UnityEngine.ProBuilder.Math.Approx3(vertexData.Position,
                    m_CurrentPositionToAdd,
                    snapDistance))
                {
                    m_CurrentPositionToAdd = vertexData.Position;
                    m_VertexType = PolygonalCut.VertexType.ExistingVertex;
                }
            }
        }

        private void CheckPointProjectionInMesh()
        {
            m_VertexType = PolygonalCut.VertexType.NewVertex;
            bool snapedOnVertex = false;
            int bestIndex = -1;
            float snapDistance = 0.1f;
            float bestDistance = Mathf.Infinity;

            List<Vertex> vertices = polygonalCut.mesh.GetVertices().ToList();
            List<Edge> peripheralEdges = WingedEdge.SortEdgesByAdjacency(m_CurrentFace);
            //if (m_TargetFace == null || m_CurrentFace == m_TargetFace)
            if(m_TargetFace != null && m_CurrentFace != m_TargetFace)
                peripheralEdges = WingedEdge.SortEdgesByAdjacency(m_TargetFace);

            for (int i = 0; i < peripheralEdges.Count; i++)
            {
                if (m_TargetFace == null || m_TargetFace == m_CurrentFace)
                {
                    if (UnityEngine.ProBuilder.Math.Approx3(vertices[peripheralEdges[i].a].position,
                        m_CurrentPositionToAdd,
                        snapDistance))
                    {
                        bestIndex = i;
                        snapedOnVertex = true;
                        break;
                    }
                    else
                    {
                        float dist = UnityEngine.ProBuilder.Math.DistancePointLineSegment(
                            m_CurrentPositionToAdd,
                            vertices[peripheralEdges[i].a].position,
                            vertices[peripheralEdges[i].b].position);

                        if (dist < Mathf.Min(snapDistance, bestDistance))
                        {
                            bestIndex = i;
                            bestDistance = dist;
                        }
                    }
                }
                else //if(m_CurrentFace != m_TargetFace)
                {
                    float edgeDist = UnityEngine.ProBuilder.Math.DistancePointLineSegment(m_CurrentPositionToAdd,
                        vertices[peripheralEdges[i].a].position,
                        vertices[peripheralEdges[i].b].position);

                    float vertexDist = Vector3.Distance(m_CurrentPositionToAdd,
                        vertices[peripheralEdges[i].a].position);

                    if (edgeDist < vertexDist && edgeDist < bestDistance)
                    {
                        bestIndex = i;
                        bestDistance = edgeDist;
                        snapedOnVertex = false;
                    }
                    //always prioritize vertex on edge
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
                m_CurrentPositionToAdd = vertices[peripheralEdges[bestIndex].a].position;
                m_VertexType = PolygonalCut.VertexType.ExistingVertex;
            }
            //If not, did we found a close edge?
            else if (bestIndex >= 0)
            {
                if (m_TargetFace == null || m_TargetFace == m_CurrentFace)
                {
                    Vector3 left = vertices[peripheralEdges[bestIndex].a].position,
                        right = vertices[peripheralEdges[bestIndex].b].position;

                    float x = (m_CurrentPositionToAdd - left).magnitude;
                    float y = (m_CurrentPositionToAdd - right).magnitude;

                    m_CurrentPositionToAdd = left + (x / (x + y)) * (right - left);
                }
                else //if(m_CurrentFace != m_TargetFace)
                {
                    Vector3 a = m_CurrentPositionToAdd -
                                vertices[peripheralEdges[bestIndex].a].position;
                    Vector3 b = vertices[peripheralEdges[bestIndex].b].position -
                                vertices[peripheralEdges[bestIndex].a].position;

                    float angle = Vector3.Angle(b, a);
                    m_CurrentPositionToAdd = Vector3.Magnitude(a) * Mathf.Cos(angle * Mathf.Deg2Rad) * b / Vector3.Magnitude(b);
                    m_CurrentPositionToAdd += vertices[peripheralEdges[bestIndex].a].position;
                }

                m_VertexType = PolygonalCut.VertexType.AddedOnEdge;
            }
        }

        private void DoPointPlacement()
        {
            Event evt = Event.current;
            EventType evtType = evt.type;

            if (m_PlacingPoint)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);

                if (evtType == EventType.MouseDrag)
                {
                    // float hitDistance = Mathf.Infinity;
                    //
                    // if (m_Plane.Raycast(ray, out hitDistance))
                    // {
                    //     evt.Use();
                    //     //polygonalCut.m_Points[m_SelectedIndex] = GetPointInLocalSpace(ray.GetPoint(hitDistance));
                    //     RebuildPolygonalShape(false);
                    //     SceneView.RepaintAll();
                    // }
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
            if (polygonalCut.polygonEditMode == PolygonalCut.PolygonEditMode.Add)
            {
                if (evtType == EventType.MouseDown && HandleUtility.nearestControl == m_ControlId)
                {
                    if (m_CurrentPositionToAdd != Vector3.positiveInfinity)
                    {
                        UndoUtility.RecordObject(polygonalCut, "Add Vertex On Face");
                        UndoUtility.RecordObject(polygonalCut.mesh, "Add Vertex In Mesh");

                        if (m_TargetFace == null)
                            m_TargetFace = m_CurrentFace;

                        if (m_VertexType == PolygonalCut.VertexType.NewVertex)
                        {
                            Vertex vertex = polygonalCut.mesh.InsertVertexInMeshSimple(m_CurrentPositionToAdd);

                            polygonalCut.m_verticesToAdd.Add(new PolygonalCut.InsertedVertexData(vertex,m_VertexType));
                        }
                        else if (m_VertexType == PolygonalCut.VertexType.ExistingVertex)
                        {
                            List<Vertex> vertices = polygonalCut.mesh.GetVertices().ToList();
                            for (int vertIndex = 0; vertIndex < vertices.Count; vertIndex++)
                            {
                                if (UnityEngine.ProBuilder.Math.Approx3(vertices[vertIndex].position, m_CurrentPositionToAdd))
                                {
                                    polygonalCut.m_verticesToAdd.Add(new PolygonalCut.InsertedVertexData(vertices[vertIndex],m_VertexType));
                                    break;
                                }
                            }
                        }
                        else if (m_VertexType == PolygonalCut.VertexType.AddedOnEdge)
                        {
                            List<Vertex> vertices = polygonalCut.mesh.GetVertices().ToList();
                            List<Edge> peripheralEdges = WingedEdge.SortEdgesByAdjacency(m_TargetFace);

                            int bestIndex = -1;
                            float bestDistance = Mathf.Infinity;
                            for (int i = 0; i < peripheralEdges.Count; i++)
                            {
                                float dist = UnityEngine.ProBuilder.Math.DistancePointLineSegment(m_CurrentPositionToAdd,
                                        vertices[peripheralEdges[i].a].position,
                                        vertices[peripheralEdges[i].b].position);

                                if (dist < bestDistance)
                                {
                                    bestIndex = i;
                                    bestDistance = dist;
                                }
                            }

                            Vertex vertex = polygonalCut.mesh.InsertVertexOnEdge(peripheralEdges[bestIndex],
                                m_CurrentPositionToAdd);

                            polygonalCut.m_verticesToAdd.Add(new PolygonalCut.InsertedVertexData(vertex,m_VertexType));
                        }

                        RebuildPolygonalShape(m_VertexType != PolygonalCut.VertexType.ExistingVertex);
                    }
                }
            }
        }

        // Returns a local space point,
        Vector3 GetPointInLocalSpace(Vector3 point)
        {
            var trs = polygonalCut.transform;
            return trs.InverseTransformPoint(point);
        }

#endregion

#region Utility functions

        void HandleKeyEvent(Event evt)
        {
            KeyCode key = evt.keyCode;

            switch (key)
            {
                case KeyCode.Backspace:
                {
                    UndoUtility.RecordObject(polygonalCut, "Delete Selected Points");
                    polygonalCut.m_verticesToAdd.RemoveAt(m_SelectedIndex);
                    RebuildPolygonalShape(true);
                    evt.Use();
                    break;
                }

                case KeyCode.Escape:
                {
                    evt.Use();
                    DestroyImmediate(polygonalCut);
                    break;
                }

            }
        }

        // public void UpdateProBuilderMesh()
        // {
        //     UndoUtility.RecordObject(polygonalCut.mesh, "Add Vertices to ProBuilder Mesh");
        //
        //     //polygonalCut.mesh.AppendVerticesToFace(m_targetFace, ,false);
        //
        //     UndoUtility.RecordObject(polygonalCut, "Removing Script from ProBuilder Object");
        //     DestroyImmediate(polygonalCut);
        //
        //     Debug.Log("Insertion Done");
        // }

#endregion

#region GUI Drawing

        void DoExistingPointsGUI()
        {
            Transform trs = polygonalCut.transform;
            int len = polygonalCut.m_verticesToAdd.Count;

            Vector3 up = polygonalCut.transform.up;
            Vector3 right = polygonalCut.transform.right;
            Vector3 forward = polygonalCut.transform.forward;
            Vector3 center = Vector3.zero;

            Event evt = Event.current;

            bool used = evt.type == EventType.Used;

            if (!used &&
                (evt.type == EventType.MouseDown &&
                 evt.button == 0 &&
                 !EditorHandleUtility.IsAppendModifier(evt.modifiers)))
            {
                Repaint();
            }

            if (polygonalCut.polygonEditMode == PolygonalCut.PolygonEditMode.Add)
            {
                for (int index = 0; index < len; index++)
                {
                    Vector3 point = trs.TransformPoint(polygonalCut.m_verticesToAdd[index].Position);

                    center.x += point.x;
                    center.y += point.y;
                    center.z += point.z;

                    float size = HandleUtility.GetHandleSize(point) * k_HandleSize;

                    Handles.color = k_HandleSelectedColor;

                    EditorGUI.BeginChangeCheck();

                    Handles.DotHandleCap(-1, point, Quaternion.identity, size, evt.type);
                    //point = Handles.Slider2D(point, up, right, forward, size, Handles.DotHandleCap, Vector2.zero, true);

                    if (EditorGUI.EndChangeCheck())
                    {
                        UndoUtility.RecordObject(polygonalCut, "Move Polygon Vertex On Face");
                        polygonalCut.m_verticesToAdd[index].Position = GetPointInLocalSpace(point);

                        RebuildPolygonalShape(false);
                    }

                    // "clicked" a button
                    if (!used && evt.type == EventType.Used)
                    {
                        used = true;
                    }

                }

                Handles.color = Color.white;
            }
        }

        public void RebuildPolygonalShape(bool vertexCountChanged = false)
        {
            // If Undo is called immediately after creation this situation can occur
            if (polygonalCut == null)
                return;

            DrawPolyLine(polygonalCut.m_verticesToAdd.Select(tup => tup.Position).ToList());

            // While the vertex count may not change, the triangle winding might. So unfortunately we can't take
            // advantage of the `vertexCountChanged = false` optimization here.
            ProBuilderEditor.Refresh();
        }

        void DrawPolyLine(List<Vector3> points)
        {
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

            m_LineMesh.Clear();
            m_LineMesh.name = "Poly Shape Guide";
            m_LineMesh.vertices = ver;
            m_LineMesh.uv = uvs;
            m_LineMesh.SetIndices(indexes, MeshTopology.LineStrip, 0);
            m_LineMaterial.SetFloat("_LineDistance", distance);
        }

#endregion

    }
}

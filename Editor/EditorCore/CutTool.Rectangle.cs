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
    partial class CutTool
    {
        /// <summary>
        /// Create a local coordinate system on the face plane.
        /// </summary>
        static void GetFacePlaneAxes(Vector3 faceNormal, out Vector3 faceRight, out Vector3 faceUp)
        {
            if (Mathf.Abs(Vector3.Dot(faceNormal, Vector3.up)) > 0.99f)
                faceRight = Vector3.Cross(faceNormal, Vector3.forward).normalized;
            else
                faceRight = Vector3.Cross(faceNormal, Vector3.up).normalized;
            faceUp = Vector3.Cross(faceNormal, faceRight).normalized;
        }

        /// <summary>
        /// Rectangle mode: click and drag to define a rectangular cut on the face.
        /// On mouse up, auto-places the 4 corners and executes the cut.
        /// </summary>
        void DoRectanglePlacement(EditorWindow window)
        {
            Event evt = Event.current;
            EventType evtType = evt.type;

            m_SnappingPoint = m_SnapToGeometry || (evt.modifiers & EventModifiers.Control) != 0;
            m_ModifyingPoint = false;

            bool hasHitPosition = UpdateHitPosition();

            // Visual helpers
            if (evtType == EventType.Repaint)
            {
                if (hasHitPosition && IsCursorInSceneView(window))
                {
                    m_CurrentCutCursor = m_CutCursorTexture;
                    m_CurrentHandleColor = k_HandleColorAddNewVertex;
                }
                else
                {
                    m_CurrentCutCursor = null;
                    m_CurrentPosition = Vector3.positiveInfinity;
                }
            }

            // Mouse down: start rectangle drag
            if (hasHitPosition
                && evtType == EventType.MouseDown && evt.button == 0
                && HandleUtility.nearestControl == m_ControlId
                && !m_RectDragging)
            {
                m_RectDragging = true;
                m_RectStartPoint = m_CurrentPosition;
                m_RectEndPoint = m_CurrentPosition;
                m_TargetFace = m_CurrentFace;

                var edges = m_TargetFace.edges;
                m_SelectedVertices = edges.Select(e => e.a).ToArray();
                m_SelectedEdges = edges.ToArray();

                m_CutPath.Clear();
                m_MeshConnections.Clear();
                evt.Use();
            }

            // Mouse drag: update rectangle end point
            if (m_RectDragging && evtType == EventType.MouseDrag && evt.button == 0)
            {
                if (hasHitPosition && m_CurrentFace == m_TargetFace)
                {
                    m_RectEndPoint = m_CurrentPosition;
                }
                evt.Use();
            }

            // Mouse up: finalize the rectangle and execute cut
            if (m_RectDragging
                && (evtType == EventType.MouseUp && evt.button == 0))
            {
                m_RectDragging = false;

                // Project start/end onto the face plane to compute the other 2 corners
                Vector3 start = m_RectStartPoint;
                Vector3 end = m_RectEndPoint;

                // Compute face normal for projection
                Vector3 faceNormal = Math.Normal(m_Mesh, m_TargetFace);

                Vector3 faceRight, faceUp;
                GetFacePlaneAxes(faceNormal, out faceRight, out faceUp);

                // Decompose rect diagonals in face space
                Vector3 diagonal = end - start;
                float rightDot = Vector3.Dot(diagonal, faceRight);
                float upDot = Vector3.Dot(diagonal, faceUp);

                // Compute the 4 rectangle corners in local space
                Vector3 corner0 = start;
                Vector3 corner1 = start + faceRight * rightDot;
                Vector3 corner2 = end;
                Vector3 corner3 = start + faceUp * upDot;

                // Snap all corners to grid if enabled
                if (m_SnapToGrid)
                {
                    corner0 = ProBuilderSnapping.Snap(corner0, EditorSnapping.activeMoveSnapValue);
                    corner1 = ProBuilderSnapping.Snap(corner1, EditorSnapping.activeMoveSnapValue);
                    corner2 = ProBuilderSnapping.Snap(corner2, EditorSnapping.activeMoveSnapValue);
                    corner3 = ProBuilderSnapping.Snap(corner3, EditorSnapping.activeMoveSnapValue);
                }

                if (HasSignificantRectangle(corner0, corner2))
                {
                    // Build cut path: 4 corners + close back to start to form a loop
                    UndoUtility.RecordObject(this, "Rectangle Cut");

                    m_CurrentPosition = corner0;
                    m_CurrentPositionNormal = faceNormal;
                    m_CurrentVertexTypes = VertexTypes.NewVertex;
                    m_CurrentFace = m_TargetFace;
                    AddCurrentPositionToPath(false);

                    m_CurrentPosition = corner1;
                    m_CurrentPositionNormal = faceNormal;
                    m_CurrentVertexTypes = VertexTypes.NewVertex;
                    AddCurrentPositionToPath(false);

                    m_CurrentPosition = corner2;
                    m_CurrentPositionNormal = faceNormal;
                    m_CurrentVertexTypes = VertexTypes.NewVertex;
                    AddCurrentPositionToPath(false);

                    m_CurrentPosition = corner3;
                    m_CurrentPositionNormal = faceNormal;
                    m_CurrentVertexTypes = VertexTypes.NewVertex;
                    AddCurrentPositionToPath(false);

                    // Close the loop by returning to the start corner
                    m_CurrentPosition = corner0;
                    m_CurrentPositionNormal = faceNormal;
                    m_CurrentVertexTypes = VertexTypes.VertexInShape;
                    m_CurrentFace = m_TargetFace;
                    AddCurrentPositionToPath(false);

                    // Don't auto-execute—let user click Complete button like point mode
                    RebuildCutShape(false);
                }

                m_RectStartPoint = Vector3.positiveInfinity;
                m_RectEndPoint = Vector3.positiveInfinity;
                evt.Use();
            }

            if (TryPassThroughSelection(window, hasHitPosition))
                return;
        }

        bool HasSignificantRectangle(Vector3 start, Vector3 end)
        {
            return Vector3.Distance(start, end) > 0.001f;
        }

        /// <summary>
        /// Draw the rectangle preview during a drag operation.
        /// </summary>
        void DoRectanglePreview()
        {
            if (!m_RectDragging || m_Mesh == null || m_TargetFace == null)
                return;

            Transform trs = m_Mesh.transform;

            // Compute the 4 corners in local space
            Vector3 faceNormal = Math.Normal(m_Mesh, m_TargetFace);

            Vector3 faceRight, faceUp;
            GetFacePlaneAxes(faceNormal, out faceRight, out faceUp);

            Vector3 diagonal = m_RectEndPoint - m_RectStartPoint;
            float rightDot = Vector3.Dot(diagonal, faceRight);
            float upDot = Vector3.Dot(diagonal, faceUp);

            Vector3 c0 = trs.TransformPoint(m_RectStartPoint);
            Vector3 c1 = trs.TransformPoint(m_RectStartPoint + faceRight * rightDot);
            Vector3 c2 = trs.TransformPoint(m_RectEndPoint);
            Vector3 c3 = trs.TransformPoint(m_RectStartPoint + faceUp * upDot);

            // Draw filled rectangle
            Handles.color = k_RectPreviewColor;
            Handles.DrawAAConvexPolygon(new Vector3[] { c0, c1, c2, c3 });

            // Draw outline
            Handles.color = k_RectOutlineColor;
            Handles.DrawAAPolyLine(2f, new Vector3[] { c0, c1, c2, c3, c0 });
        }

        bool TryPassThroughSelection(EditorWindow window, bool hasHitPosition)
        {
            if (m_CutPath.Count != 0
                || hasHitPosition
                || HandleUtility.nearestControl != m_ControlId)
            {
                return false;
            }

            SceneView sceneView = window as SceneView;
            if (sceneView == null)
                sceneView = SceneView.lastActiveSceneView;

            if (sceneView == null || ProBuilderEditor.instance == null)
                return false;

            ProBuilderEditor.instance.HandleMouseEvent(sceneView, m_ControlId);
            return true;
        }
    }
}
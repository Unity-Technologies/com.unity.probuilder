using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;
using Edge = UnityEngine.ProBuilder.Edge;
using UObject = UnityEngine.Object;

namespace UnityEditor.ProBuilder
{
    partial class CutTool
    {
        /// <summary>
        /// Overlay GUI
        /// </summary>
        /// <param name="target">the target of this overlay</param>
        /// <param name="view">the current SceneView where to display the overlay</param>
        void OnOverlayGUI(UObject target, SceneView view)
        {
            if(MeshSelection.selectedObjectCount != 1)
            {
                var rect = EditorGUILayout.GetControlRect(false, 45);
                EditorGUI.HelpBox(rect, L10n.Tr("One and only one ProBuilder mesh must be selected."), MessageType.Warning);
            }

            GUI.enabled = MeshSelection.selectedObjectCount == 1;

            m_RectangleMode = DoOverlayToggle(L10n.Tr("Rectangle Mode"), m_RectangleMode);
            EditorPrefs.SetBool(k_RectangleModePrefKey, m_RectangleMode);

            m_SnapToGrid = DoOverlayToggle(L10n.Tr("Snap to Grid"), m_SnapToGrid);
            EditorPrefs.SetBool(k_SnapToGridPrefKey, m_SnapToGrid);

            m_SnapToGeometry = DoOverlayToggle(L10n.Tr("Snap to existing edges and vertices"), m_SnapToGeometry);
            EditorPrefs.SetBool(k_SnapToGeometryPrefKey, m_SnapToGeometry);

            if(m_RectangleMode)
            {
                EditorGUI.indentLevel++;
                using(new GUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(L10n.Tr("Click & drag to draw a rectangle cut"), GUILayout.Width(250));
                }
                EditorGUI.indentLevel--;
            }

            if(!m_RectangleMode && !m_SnapToGeometry)
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
                        ExitTool();
                }
                else
                {
                    if(!m_RectangleMode && m_CutPath.Count > 1)
                    {
                        if(GUILayout.Button(EditorGUIUtility.TrTextContent("Complete")))
                            ExecuteCut();
                    }

                    if(GUILayout.Button(EditorGUIUtility.TrTextContent("Cancel")))
                        ExitTool();
                }
            }

            GUI.enabled = true;
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
            if(m_MeshConnections.Count > 0 && m_Mesh != null)
            {
                Vertex[] vertices = m_Mesh.GetVertices();
                for (int i = m_MeshConnections.Count - 1; i >= 0; i--)
                {
                    var connection = m_MeshConnections[i];
                    if (connection.item1 < 0 || connection.item1 >= m_CutPath.Count
                        || connection.item2 < 0 || connection.item2 >= vertices.Length)
                    {
                        m_MeshConnections.RemoveAt(i);
                        continue;
                    }
                    Handles.color = k_ConnectionsLineColor;
                    Handles.DrawDottedLine(m_Mesh.transform.TransformPoint(m_CutPath[connection.item1].position),
                                            m_Mesh.transform.TransformPoint(vertices[connection.item2].position), 5f);
                    Handles.color = Color.white;
                }
            }
        }
    }
}

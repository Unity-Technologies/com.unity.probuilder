using System.Collections;
using System.Collections.Generic;
using Codice.Client.GameUI.Checkin;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Rendering;

// 1) Learn how to target a specific component when writing a custom editor tool
[EditorTool("Edge Move", typeof(MeshFilter))]
public class EdgeMoveTool : EditorTool
{
    static readonly float k_EdgeHandleThicknessScreen = 3f;
    static readonly float k_EdgeHandlePickDistanceScreen = 3f;
    static readonly Color k_EdgeHandleColor = new Color(0.3f, 0.6f, 1f);

    (int, int) m_SelectedEdgeIndices = (-1, -1);
    MeshFilter m_SelectedMeshFilter = null;

    public override void OnToolGUI(EditorWindow window)
    {
        foreach (MeshFilter meshFilter in targets)
        {
            var mesh = meshFilter.sharedMesh;
            var meshTransform = meshFilter.transform;
            var vertices = mesh.vertices;
            var indices = mesh.GetIndices(0);
            List< (int, int)> edgeIndexPairs = new List<(int, int)>();
            
            for (int indicesIndex = 0; indicesIndex < indices.Length; indicesIndex += 6)
            {
                edgeIndexPairs = GetEdgeIndices(indices, indicesIndex);

                for (int i = 0; i < edgeIndexPairs.Count; i++)
                {
                    var edgeIndexPair = edgeIndexPairs[i];
                    var vertexA = meshTransform.TransformPoint(vertices[edgeIndexPair.Item1]);
                    var vertexB = meshTransform.TransformPoint(vertices[edgeIndexPair.Item2]);
                    
                    var selected = false;
                
                    var handleColor = (m_SelectedMeshFilter == meshFilter && m_SelectedEdgeIndices == edgeIndexPair) ? Handles.selectedColor : k_EdgeHandleColor;
                    using (new Handles.DrawingScope(handleColor, Matrix4x4.identity))
                    {
                        // 2) Learn to write a custom button handle
                        selected = LineButtonHandle(vertexA, vertexB, k_EdgeHandleThicknessScreen, k_EdgeHandlePickDistanceScreen);
                    }

                    if (selected)
                    {
                        m_SelectedEdgeIndices = edgeIndexPair;
                        m_SelectedMeshFilter = meshFilter;
                    }
                }
            }
        }

        if (m_SelectedMeshFilter != null && m_SelectedEdgeIndices != (-1, -1))
        {
            var vertices = m_SelectedMeshFilter.sharedMesh.vertices;
            var meshTransform = m_SelectedMeshFilter.transform;
            var vertexA = vertices[m_SelectedEdgeIndices.Item1];
            var vertexB = vertices[m_SelectedEdgeIndices.Item2];
            var originalPos = vertexA + (vertexB - vertexA) * 0.5f;
            var originalPosWorld = meshTransform.TransformPoint(originalPos);
            
            EditorGUI.BeginChangeCheck();
            var newPos = meshTransform.InverseTransformPoint(Handles.PositionHandle(originalPosWorld, Tools.handleRotation));
            var delta = newPos - originalPos;
            if (EditorGUI.EndChangeCheck())
            {
                // Bruteforce handling of shared vertex positions
                for (int i = 0; i < vertices.Length; i++)
                {
                    var vertexPos = vertices[i];
                    if (Vector3.Magnitude(vertexPos - vertexA) < float.Epsilon || Vector3.Magnitude(vertexPos - vertexB) < float.Epsilon )
                        vertices[i] += delta;
                }
                
                m_SelectedMeshFilter.sharedMesh.SetVertices(vertices);
            }
        }
    }

    List<(int, int)> GetEdgeIndices(int[] indices, int startingIndex)
    {
        var edgeToSkip = new List<int>();
        for (int i = startingIndex; i < startingIndex + 3; i++)
        {
            for (int j = startingIndex + 3; j < startingIndex + 6; j++)
            {
                if (indices[i] == indices[j])
                    edgeToSkip.Add(indices[i]);
            }
        }

        var result = new List<(int, int)>();
        for (int i = 0; i < 6; i++)
        {
            var index = startingIndex + i;
            var nextIndex = i == 5 ? startingIndex : index + 1;
            
            if (edgeToSkip.Contains(indices[index]) && edgeToSkip.Contains(indices[nextIndex]))
                continue;

            result.Add((indices[index], indices[nextIndex]));
        }

        return result;
    }
    
    // 2) Learn to write a custom button handle
    bool LineButtonHandle(Vector3 pointA, Vector3 pointB, float thickness, float pickSizeScreen)
    {
        var evt = Event.current;
        var controlID = GUIUtility.GetControlID(FocusType.Passive);

        switch (evt.GetTypeForControl(controlID))
        {
            case EventType.Layout:
                // 3) Learn how to create a control of a specific shape/size
                HandleUtility.AddControl(controlID, Mathf.Max(0f, HandleUtility.DistanceToLine(pointA, pointB) - pickSizeScreen));
                break;

            case EventType.MouseDown:
                if (HandleUtility.nearestControl == controlID && evt.button == 0 && !evt.alt)
                {
                    GUIUtility.hotControl = controlID;
                    evt.Use();
                }
                break;
                    
            case EventType.MouseUp:
                if (GUIUtility.hotControl == controlID && evt.button == 0 && !evt.alt)
                {
                    GUIUtility.hotControl = 0;
                    evt.Use();
                    return true;
                }
                break; 
                
            case EventType.MouseMove:
                HandleUtility.AddControl(controlID, Mathf.Max(0f, HandleUtility.DistanceToLine(pointA, pointB) - pickSizeScreen));
                if (HandleUtility.nearestControl == controlID)
                    HandleUtility.Repaint();

                break;

            case EventType.Repaint:
                var handleColor = Handles.color;
                if (GUIUtility.hotControl == controlID) 
                    handleColor = Handles.selectedColor;
                else if (HandleUtility.nearestControl == controlID)
                    handleColor = Handles.preselectionColor;

                using (new Handles.DrawingScope(handleColor, Matrix4x4.identity))
                {
                    Handles.zTest = CompareFunction.LessEqual;
                    Handles.DrawLine(pointA, pointB, thickness);
                }

                break;
        }

        return false;
    }
}

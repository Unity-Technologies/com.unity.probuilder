using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;

[EditorTool("Mesh Element Move", typeof(MeshFilter), typeof(MeshEditContext))]
public class MeshElementMoveTool : EditorTool
{
    static readonly float k_VertexHandleWorldSize = 0.1f;
    static readonly float k_VertexHandleWorldPickSize = k_VertexHandleWorldSize * 2.0f;
    static readonly Color k_VertexHandleColor = new Color(0.3f, 0.6f, 1f);
    
    static readonly float k_EdgeHandleThicknessScreen = 3f;
    static readonly float k_EdgeHandlePickDistanceScreen = 3f;
    static readonly Color k_EdgeHandleColor = new Color(0.3f, 0.6f, 1f);
    
    int m_SelectedVertexIndex = -1;
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

            switch (MeshEditContext.EditMode)
            {
                case MeshEditContext.ElementEditMode.Vertex:
                    for (int vertexIndex = 0; vertexIndex < vertices.Length; ++vertexIndex)
                    {
                        var vertexPosWorld = meshTransform.TransformPoint(vertices[vertexIndex]);
                        var selected = false;

                        var handleColor = (m_SelectedMeshFilter == meshFilter && m_SelectedVertexIndex == vertexIndex) ? Handles.selectedColor : k_VertexHandleColor;
                        using (new Handles.DrawingScope(handleColor, Matrix4x4.identity))
                        {
                            var sceneViewCam = SceneView.lastActiveSceneView.camera;
                            var handleVisualSize = k_VertexHandleWorldSize * HandleUtility.GetHandleSize(vertexPosWorld);
                            var handlePickingSize = k_VertexHandleWorldPickSize * HandleUtility.GetHandleSize(vertexPosWorld);

                            // 2) Learn to write a custom button handle
                            selected = CustomHandles.VertexButtonHandle(vertexPosWorld, Quaternion.LookRotation(-sceneViewCam.transform.forward), handleVisualSize, handlePickingSize);
                        }

                        if (selected)
                        {
                            m_SelectedVertexIndex = vertexIndex;
                            m_SelectedMeshFilter = meshFilter;
                        }
                    }
                    
                    if (m_SelectedMeshFilter != null && m_SelectedVertexIndex >= 0)
                    { 
                        vertices = m_SelectedMeshFilter.sharedMesh.vertices;
                        meshTransform = m_SelectedMeshFilter.transform;
                        var originalPos = vertices[m_SelectedVertexIndex];
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
                                if (Vector3.Magnitude(vertexPos - originalPos) < float.Epsilon)
                                    vertices[i] += delta;
                            }
                
                            m_SelectedMeshFilter.sharedMesh.SetVertices(vertices);
                        }
                    }
                    break;
                
                case MeshEditContext.ElementEditMode.Edge:
                    List< (int, int)> edgeIndexPairs = new List<(int, int)>();
                    for (int indicesIndex = 0; indicesIndex < indices.Length; indicesIndex += 6)
                    {
                        edgeIndexPairs = Utils.GetEdgeIndices(indices, indicesIndex);

                        for (int i = 0; i < edgeIndexPairs.Count; i++)
                        {
                            var edgeIndexPair = edgeIndexPairs[i];
                            var vertexA = meshTransform.TransformPoint(vertices[edgeIndexPair.Item1]);
                            var vertexB = meshTransform.TransformPoint(vertices[edgeIndexPair.Item2]);
                    
                            var selected = false;
                
                            var handleColor = (m_SelectedMeshFilter == meshFilter && m_SelectedEdgeIndices == edgeIndexPair) ? Handles.selectedColor : k_EdgeHandleColor;
                            using (new Handles.DrawingScope(handleColor, Matrix4x4.identity))
                            {
                                var sceneViewCam = SceneView.lastActiveSceneView.camera;

                                // 2) Learn to write a custom button handle
                                selected = CustomHandles.LineButtonHandle(vertexA, vertexB, k_EdgeHandleThicknessScreen, k_EdgeHandlePickDistanceScreen);
                            }

                            if (selected)
                            {
                                m_SelectedEdgeIndices = edgeIndexPair;
                                m_SelectedMeshFilter = meshFilter;
                            }
                        }
                    }
                    
                    if (m_SelectedMeshFilter != null && m_SelectedEdgeIndices != (-1, -1))
                    {
                        vertices = m_SelectedMeshFilter.sharedMesh.vertices;
                        meshTransform = m_SelectedMeshFilter.transform;
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
                    break;
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Rendering;

// 1) Learn how to target a specific component when writing a custom editor tool
[EditorTool("Vertex Move", typeof(MeshFilter))]
public class VertexMoveTool : EditorTool
{
    static readonly float k_VertexHandleWorldSize = 0.1f;
    static readonly float k_VertexHandleWorldPickSize = k_VertexHandleWorldSize * 2.0f;
    static readonly Color k_VertexHandleColor = new Color(0.3f, 0.6f, 1f);

    int m_SelectedVertexIndex = -1;
    MeshFilter m_SelectedMeshFilter = null;

    public override void OnToolGUI(EditorWindow window)
    {
        foreach (MeshFilter meshFilter in targets)
        {
            var mesh = meshFilter.sharedMesh;
            var meshTransform = meshFilter.transform;
            var vertices = mesh.vertices;
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
                    selected = VertexButtonHandle(vertexPosWorld, Quaternion.LookRotation(-sceneViewCam.transform.forward), handleVisualSize, handlePickingSize);
                }

                if (selected)
                {
                    m_SelectedVertexIndex = vertexIndex;
                    m_SelectedMeshFilter = meshFilter;
                }
            }
        }

        if (m_SelectedMeshFilter != null && m_SelectedVertexIndex >= 0)
        {
            var vertices = m_SelectedMeshFilter.sharedMesh.vertices;
            var meshTransform = m_SelectedMeshFilter.transform;
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
                    if (Mathf.Approximately(Vector3.Magnitude(vertexPos - originalPos), float.Epsilon))
                        vertices[i] += delta;
                }
                
                m_SelectedMeshFilter.sharedMesh.SetVertices(vertices);
            }
        }
    }

    // 2) Learn to write a custom button handle
    bool VertexButtonHandle(Vector3 position, Quaternion direction, float size, float pickSize)
    {
        var evt = Event.current;
        var controlID = GUIUtility.GetControlID(FocusType.Passive);

        switch (evt.GetTypeForControl(controlID))
        {
            case EventType.Layout:
                // 3) Learn how to create a control of a specific shape/size
                HandleUtility.AddControl(controlID,  HandleUtility.DistanceToCircle(position, pickSize));
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
                HandleUtility.AddControl(controlID,  HandleUtility.DistanceToCircle(position, pickSize));
                if (HandleUtility.nearestControl == controlID)
                    HandleUtility.Repaint();

                break;

            case EventType.Repaint:
                var handleColor = Handles.color;
                if (GUIUtility.hotControl == controlID) 
                    handleColor = Handles.selectedColor;
                else if (HandleUtility.nearestControl == controlID)
                    handleColor = Handles.preselectionColor;
                
                var sceneViewCam = SceneView.lastActiveSceneView.camera;

                using (new Handles.DrawingScope(handleColor, Matrix4x4.identity))
                {
                    Handles.zTest = CompareFunction.LessEqual;
                    Handles.DrawSolidDisc(position, direction * Vector3.forward, size);
                }

                break;
        }

        return false;
    }
}

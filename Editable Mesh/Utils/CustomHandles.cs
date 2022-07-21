using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class CustomHandles 
{
    public static bool VertexButtonHandle(Vector3 position, Quaternion direction, float size, float pickSize)
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

    public static bool LineButtonHandle(Vector3 pointA, Vector3 pointB, float thickness, float pickSizeScreen)
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

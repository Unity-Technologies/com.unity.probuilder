using System;
using UnityEngine;
using Math = UnityEngine.ProBuilder.Math;
using Plane = UnityEngine.ProBuilder.Shapes.Plane;

namespace UnityEditor.ProBuilder
{
    internal class ShapeState_DrawHeightShape : ShapeState
    {
        protected override void EndState()
        {
            tool.RebuildShape();
            tool.m_LastShapeCreated = tool.m_ShapeComponent;
            tool.m_ShapeComponent = null;
        }

        ShapeState ValidateShape()
        {
            tool.RebuildShape();
            tool.m_ShapeComponent.SetPivotPosition(tool.m_BB_Origin);
            DrawShapeTool.s_ActiveShapeIndex.value = Array.IndexOf(EditorShapeUtility.availableShapeTypes,tool.m_ShapeComponent.shape.GetType());

            EditorShapeUtility.SaveParams(tool.m_ShapeComponent.shape);

            return NextState();
        }

        public override ShapeState DoState(Event evt)
        {
            if((tool.m_ShapeComponent.shape is Plane)
                || (tool.m_ShapeComponent.shape is UnityEngine.ProBuilder.Shapes.Sprite))
            {
                //Skip Height definition for plane
                return NextState();
            }

            tool.DrawBoundingBox();

            if(evt.type == EventType.KeyDown)
            {
                switch(evt.keyCode)
                {
                    case KeyCode.Space:
                    case KeyCode.Return:
                    case KeyCode.Escape:
                        return ValidateShape();
                }
            }

            if(evt.isMouse)
            {
                switch(evt.type)
                {
                    case EventType.MouseMove:
                    case EventType.MouseDrag:
                        Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                        Vector3 heightPoint = Math.GetNearestPointRayRay(tool.m_BB_OppositeCorner, tool.m_Plane.normal,
                            ray.origin, ray.direction);

                        var deltaPoint = heightPoint - tool.m_BB_OppositeCorner;
                        deltaPoint = Quaternion.Inverse(tool.m_PlaneRotation) * deltaPoint;
                        deltaPoint = tool.GetPoint(deltaPoint, evt.control);
                        tool.m_BB_HeightCorner = tool.m_PlaneRotation * deltaPoint + tool.m_BB_OppositeCorner;
                        tool.RebuildShape();
                        break;

                    case EventType.MouseUp:
                        return ValidateShape();
                }
            }

            return this;
        }
    }
}

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
            tool.handleSelectionChange = false;

            // Get the current undo group
            var group = Undo.GetCurrentGroup();
            var shape = tool.currentShapeInOverlay.gameObject;
            UndoUtility.RegisterCreatedObjectUndo(shape, "Draw Shape");

            //Actually generate the shape
            tool.m_ProBuilderShape.pivotGlobalPosition = tool.m_BB_Origin;
            tool.m_ProBuilderShape.gameObject.hideFlags = HideFlags.None;

            EditorUtility.InitObject(tool.m_ProBuilderShape.mesh);

            tool.RebuildShape();
            Selection.activeObject = shape;
            // make sure that the whole shape creation process is a single undo group
            Undo.CollapseUndoOperations(group);

            //Update tool
            DrawShapeTool.s_ActiveShapeIndex.value = Array.IndexOf(EditorShapeUtility.availableShapeTypes, tool.m_ProBuilderShape.shape.GetType());
            DrawShapeTool.SaveShapeParams(tool.m_ProBuilderShape);
            tool.m_LastShapeCreated = tool.m_ProBuilderShape;
            tool.m_ProBuilderShape = null;
        }

        ShapeState ValidateShape()
        {
            return NextState();
        }

        public override ShapeState DoState(Event evt)
        {
            if((tool.m_ProBuilderShape.shape is Plane)
                || (tool.m_ProBuilderShape.shape is UnityEngine.ProBuilder.Shapes.Sprite))
            {
                //Skip Height definition for plane
                return ValidateShape();
            }

            if(evt.type == EventType.KeyDown)
            {
                switch(evt.keyCode)
                {
                    case KeyCode.Space:
                    case KeyCode.Return:
                    case KeyCode.Escape:
                        return ValidateShape();

                    case KeyCode.Delete:
                        return ResetState();
                }
            }

            if(evt.type == EventType.Repaint)
                tool.DrawBoundingBox();

            if(evt.isMouse)
            {
                switch(evt.type)
                {
                    case EventType.MouseMove:
                    case EventType.MouseDrag:
                        Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                        Vector3 heightPoint = Math.GetNearestPointRayRay(tool.m_BB_OppositeCorner, tool.m_Plane.normal, ray.origin, ray.direction);
                        var dot = Vector3.Dot(tool.m_Plane.normal, ray.direction);
                        var isHeightPointNaN = float.IsNaN(heightPoint.x) || float.IsNaN(heightPoint.y) || float.IsNaN(heightPoint.z);
                        if (!isHeightPointNaN)
                        {
                            var deltaPoint = (dot == 0 ? ray.origin - tool.m_Plane.ClosestPointOnPlane(ray.origin) : heightPoint - tool.m_BB_OppositeCorner);
                            deltaPoint = Quaternion.Inverse(tool.m_PlaneRotation) * deltaPoint;
                            deltaPoint = tool.GetPoint(deltaPoint, evt.control);
                            tool.m_BB_HeightCorner = tool.m_PlaneRotation * deltaPoint + tool.m_BB_OppositeCorner;
                            tool.RebuildShape();
                        }

                        break;

                    case EventType.MouseUp:
                        return ValidateShape();
                }
            }

            return this;
        }
    }
}

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
            UndoUtility.RegisterCreatedObjectUndo(tool.currentShapeInOverlay.gameObject, "Draw Shape");
            tool.m_LastShapeCreated = tool.m_ProBuilderShape;
            tool.m_ProBuilderShape = null;
        }

        ShapeState ValidateShape()
        {
            tool.handleSelectionChange = false;

            tool.RebuildShape();
            tool.m_ProBuilderShape.pivotGlobalPosition = tool.m_BB_Origin;
            tool.m_ProBuilderShape.gameObject.hideFlags = HideFlags.None;

            EditorUtility.InitObject(tool.m_ProBuilderShape.mesh);

            DrawShapeTool.s_ActiveShapeIndex.value = Array.IndexOf(EditorShapeUtility.availableShapeTypes, tool.m_ProBuilderShape.shape.GetType());
            DrawShapeTool.SaveShapeParams(tool.m_ProBuilderShape);

            // make sure that the whole shape creation process is a single undo group
            var group = Undo.GetCurrentGroup() - 1;
            Selection.activeObject = tool.m_ProBuilderShape.gameObject;
            Undo.CollapseUndoOperations(group);

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

            tool.DrawBoundingBox();

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

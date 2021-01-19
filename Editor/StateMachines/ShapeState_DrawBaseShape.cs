using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;

namespace UnityEditor.ProBuilder
{
    internal class ShapeState_DrawBaseShape : ShapeState
    {
        bool m_IsDragging = false;

        Quaternion m_currentShapeRotation = Quaternion.identity;

        protected override void InitState()
        {
            m_IsDragging = false;
        }

        public override ShapeState DoState(Event evt)
        {
            if(evt.type == EventType.Repaint && m_IsDragging)
                tool.DrawBoundingBox();

            if(evt.type == EventType.KeyDown)
            {
                switch(evt.keyCode)
                {
                    case KeyCode.Escape:
                        return ResetState();
                }
            }

            if(evt.isMouse)
            {
                switch(evt.type)
                {
                    case EventType.MouseDrag:
                    {
                        m_IsDragging = true;
                        Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                        float distance;

                        if(tool.m_Plane.Raycast(ray, out distance))
                            UpdateShapeBase(ray, distance);

                        break;
                    }

                    case EventType.MouseUp:
                    {
                        if(!m_IsDragging && evt.shift)
                        {
                            CreateLastShape();
                            return ResetState();
                        }

                        if(Vector3.Distance(tool.m_BB_OppositeCorner, tool.m_BB_Origin) < .1f)
                            return ResetState();

                        return NextState();

                        break;
                    }
                }
            }

            return this;
        }

        void UpdateShapeBase(Ray ray, float distance)
        {
            var deltaPoint = ray.GetPoint(distance) - tool.m_BB_Origin;
            deltaPoint = Quaternion.Inverse(tool.m_PlaneRotation) * deltaPoint;
            deltaPoint = tool.GetPoint(deltaPoint, Event.current.control);
            tool.m_BB_OppositeCorner = tool.m_PlaneRotation * deltaPoint + tool.m_BB_Origin;
            tool.m_BB_HeightCorner = tool.m_BB_OppositeCorner;

            tool.RebuildShape();
        }

        public void CreateLastShape()
        {
            var lastShape = tool.m_LastShapeCreated != null ? tool.m_LastShapeCreated : tool.currentShapeInOverlay;
            var shape = ShapeFactory.Instantiate(DrawShapeTool.activeShapeType, lastShape.pivotLocation).GetComponent<ShapeComponent>();
            shape.gameObject.name = shape.gameObject.name + "-Copy";
            shape.CopyComponent(lastShape);

            UndoUtility.RegisterCreatedObjectUndo(shape.gameObject, "Create Shape Copy");

            EditorShapeUtility.CopyLastParams(shape.shape, shape.shape.GetType());
            shape.Rebuild(tool.m_Bounds, tool.m_PlaneRotation);
            ProBuilderEditor.Refresh(false);

            tool.m_ShapeComponent = null;
            tool.m_LastShapeCreated = shape;

            if(tool.m_DuplicateGO != null)
                GameObject.DestroyImmediate(tool.m_DuplicateGO);

            MeshSelection.SetSelection(shape.gameObject);
        }
    }
}

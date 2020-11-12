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
                        if(!m_IsDragging)
                        {
                            Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                            float distance;

                            if(tool.m_Plane.Raycast(ray, out distance))
                            {
                                var pos = ray.GetPoint(distance);
                                CreateLastShape(pos);

                                return ResetState();
                            }
                        }
                        else if(Vector3.Distance(tool.m_BB_OppositeCorner, tool.m_BB_Origin) < .1f)
                            return ResetState();
                        else
                        {
                            DrawShapeTool.s_LastShapeRotation = m_currentShapeRotation;
                            return NextState();
                        }

                        break;
                    }
                }
            }

            return this;
        }

        void UpdateShapeBase(Ray ray, float distance)
        {
            tool.m_BB_OppositeCorner = tool.GetPoint(ray.GetPoint(distance));
            tool.m_BB_HeightCorner = tool.m_BB_OppositeCorner;

            var dragDirection = tool.m_BB_OppositeCorner - tool.m_BB_Origin;
            float dragDotForward = Vector3.Dot(dragDirection, tool.m_PlaneForward);
            float dragDotRight = Vector3.Dot(dragDirection, tool.m_PlaneRight);
            m_currentShapeRotation = Quaternion.identity;
            if(dragDotForward < 0 && dragDotRight > 0)
                m_currentShapeRotation = Quaternion.Euler(0, 180, 0);
            else if(dragDotForward < 0 && dragDotRight < 0)
                m_currentShapeRotation = Quaternion.Euler(0, -90, 0);
            else if(dragDotForward > 0 && dragDotRight > 0)
                m_currentShapeRotation = Quaternion.Euler(0, 90, 0);

            tool.m_ShapeComponent.SetInnerBoundsRotation(m_currentShapeRotation);
            tool.RebuildShape();
        }

        public void CreateLastShape(Vector3 position)
        {
            var shape = ShapeFactory.Instantiate(DrawShapeTool.activeShapeType).GetComponent<ShapeComponent>();
            shape.shape = EditorShapeUtility.GetLastParams(shape.shape.GetType());;
            UndoUtility.RegisterCreatedObjectUndo(shape.gameObject, "Create Shape Copy");

            shape.SetInnerBoundsRotation(DrawShapeTool.s_LastShapeRotation);
            shape.Rebuild(tool.m_Bounds, tool.m_PlaneRotation);
            ProBuilderEditor.Refresh(false);

            var res = shape.GetComponent<ProBuilderMesh>();
            EditorUtility.InitObject(res);

            var cornerPosition = position - tool.m_Bounds.extents;
            cornerPosition.y = position.y;
            cornerPosition = tool.GetPoint(cornerPosition);
            res.transform.position = cornerPosition + new Vector3(tool.m_Bounds.extents.x,0, tool.m_Bounds.extents.z) + tool.m_Bounds.extents.y * tool.m_Plane.normal;

            tool.m_LastShapeCreated = shape;
        }
    }
}

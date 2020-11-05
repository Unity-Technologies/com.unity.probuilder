using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;

namespace UnityEditor.ProBuilder
{
    internal class ShapeState_DrawBaseShape : ShapeState
    {
        bool m_IsDragging = false;

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
                        {
                            tool.m_BB_OppositeCorner = tool.GetPoint(ray.GetPoint(distance));
                            tool.m_BB_HeightCorner = tool.m_BB_OppositeCorner;
                            tool.RebuildShape();
                        }

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
                                var shape = CreateLastShape(pos);

                                return ResetState();
                            }
                        }
                        else if(Vector3.Distance(tool.m_BB_OppositeCorner, tool.m_BB_Origin) < .1f)
                            return ResetState();
                        else
                            return NextState();

                        break;
                    }
                }
            }

            return this;
        }

        public ProBuilderMesh CreateLastShape(Vector3 position)
        {
            var shape = ShapeGenerator.CreateShape(DrawShapeTool.activeShapeType).GetComponent<ShapeComponent>();
            shape.shape = EditorShapeUtility.GetLastParams(shape.shape.GetType());
            UndoUtility.RegisterCreatedObjectUndo(shape.gameObject, "Create Shape Copy");

            shape.Rebuild(tool.m_Bounds, tool.m_Rotation);
            ProBuilderEditor.Refresh(false);

            var res = shape.GetComponent<ProBuilderMesh>();
            EditorUtility.InitObject(res);

            var cornerPosition = position - tool.m_Bounds.extents;
            cornerPosition.y = position.y;
            cornerPosition = tool.GetPoint(cornerPosition);
            res.transform.position = cornerPosition + new Vector3(tool.m_Bounds.extents.x,0, tool.m_Bounds.extents.z) + tool.m_Bounds.extents.y * tool.m_Plane.normal;

            return res;
        }
    }
}

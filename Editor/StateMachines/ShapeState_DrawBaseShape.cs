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

            switch (evt.type)
            {
                case EventType.MouseDrag:
                {
                    m_IsDragging = true;
                    Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                    float distance;

                    if (tool.m_Plane.Raycast(ray, out distance))
                    {
                        tool.m_BB_OppositeCorner = tool.GetPoint(ray.GetPoint(distance));
                        tool.m_BB_HeightCorner = tool.m_BB_OppositeCorner;
                        tool.RebuildShape();
                    }
                    break;
                }

                case EventType.MouseUp:
                {
                    if (!m_IsDragging)
                    {
                        Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                        float distance;

                        if (tool.m_Plane.Raycast(ray, out distance))
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

            return this;
        }

        public ProBuilderMesh CreateLastShape(Vector3 position)
        {
            var shape = ShapeGenerator.CreateShape(DrawShapeTool.activeShapeType).GetComponent<ShapeComponent>();
            shape.shape = EditorShapeUtility.GetLastParams(shape.shape.GetType());
            UndoUtility.RegisterCreatedObjectUndo(shape.gameObject, "Create Shape Copy");

            Bounds bounds = new Bounds(Vector3.zero, DrawShapeTool.s_Size);
            shape.Rebuild(bounds, Quaternion.LookRotation(tool.m_PlaneForward,tool.m_Plane.normal));
            ProBuilderEditor.Refresh(false);

            var res = shape.GetComponent<ProBuilderMesh>();
            EditorUtility.InitObject(res);
            res.transform.position = position + tool.m_Plane.normal * bounds.extents.y;

            return res;
        }
    }
}

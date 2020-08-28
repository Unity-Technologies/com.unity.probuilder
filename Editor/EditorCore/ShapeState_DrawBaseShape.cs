using UnityEngine;

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
                        tool.m_OppositeCorner = ray.GetPoint(distance);
                        tool.m_HeightCorner = tool.m_OppositeCorner;
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
                            var shape = tool.CreateLastShape(pos);

                            ResetState();
                        }
                    }
                    else if(Vector3.Distance(tool.m_OppositeCorner, tool.m_Origin) < .1f)
                        return ResetState();
                    else
                        return NextState();
                    break;
                }
            }

            return this;
        }
    }
}

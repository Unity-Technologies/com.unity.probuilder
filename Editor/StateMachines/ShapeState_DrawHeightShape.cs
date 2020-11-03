using UnityEngine;
using UnityEngine.ProBuilder;
using Plane = UnityEngine.ProBuilder.Shapes.Plane;

namespace UnityEditor.ProBuilder
{
    internal class ShapeState_DrawHeightShape : ShapeState
    {
        protected override void EndState()
        {
            DrawShapeTool.s_Size.value = tool.m_Shape.size;
            DrawShapeTool.s_ActiveShapeIndex.value = DrawShapeTool.s_AvailableShapeTypes.IndexOf(tool.m_Shape.shape.GetType());
            tool.m_Shape = null;
        }

        public override ShapeState DoState(Event evt)
        {
            if(tool.m_Shape.shape is Plane)
            {
                //Skip Height definition for plane
                return NextState();
            }

            tool.DrawBoundingBox();

            switch (evt.type)
            {
                case EventType.MouseMove:
                case EventType.MouseDrag:
                    Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                    Vector3 heightPoint = Math.GetNearestPointRayRay(tool.m_BB_OppositeCorner, tool.m_Plane.normal,
                        ray.origin, ray.direction);
                    tool.m_BB_HeightCorner = EditorSnapping.MoveSnap(heightPoint);
                    tool.RebuildShape();
                    break;

                case EventType.MouseUp:
                    tool.RebuildShape();
                    return NextState();
            }
            return this;
        }
    }
}

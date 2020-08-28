using UnityEngine;
using UnityEngine.ProBuilder;

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
            tool.DrawBoundingBox();

            switch (evt.type)
            {
                case EventType.MouseMove:
                case EventType.MouseDrag:
                    Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                    tool.m_HeightCorner = Math.GetNearestPointRayRay(tool.m_OppositeCorner, tool.m_Plane.normal, ray.origin, ray.direction);

                    var diff = tool.m_HeightCorner - tool.m_Origin;
                    if (tool.m_Shape != null)
                        tool.m_Shape.SetRotation(ToRotationAngles(diff));
                    tool.RebuildShape();
                    break;

                case EventType.MouseUp:
                    tool.RebuildShape();
                    return NextState();
            }
            return this;
        }

        /// <summary>
        /// Calculates the rotation angles to give a shape depending on the orientation we started drawing it
        /// </summary>
        /// <param name="diff">Difference between point A and point B</param>
        /// <returns></returns>
        Quaternion ToRotationAngles(Vector3 diff)
        {
            Vector3 angles = Vector3.zero;
            if (diff.y < 0)
                angles.z = -180f;
            if (diff.z < 0)
                angles.y = -180f;

            return Quaternion.Euler(angles);
        }
    }
}

using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

namespace UnityEditor.ProBuilder
{
    internal class ShapeState_InitShape : ShapeState
    {
        protected override void InitState()
        {
            tool.m_IsShapeInit = false;
            tool.m_Shape = new GameObject("Shape", typeof(ShapeComponent)).GetComponent<ShapeComponent>();
            tool.m_Shape.gameObject.hideFlags = HideFlags.HideAndDontSave;
            tool.m_Shape.hideFlags = HideFlags.None;
            tool.m_Shape.SetShape(EditorShapeUtility.CreateShape(DrawShapeTool.activeShapeType));
        }

        public override ShapeState DoState(Event evt)
        {
            if (evt.type == EventType.MouseDown)
            {
                var res = EditorHandleUtility.FindBestPlaneAndBitangent(evt.mousePosition);

                Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                float hit;

                //Click has been done => Define a plane for the tool
                if (res.item1.Raycast(ray, out hit))
                {
                    tool.m_Plane = res.item1;
                    tool.m_PlaneForward = res.item2;
                    tool.m_PlaneRight = Vector3.Cross(tool.m_Plane.normal, tool.m_PlaneForward);
                    tool.m_BB_Origin = ray.GetPoint(hit);
                    tool.m_BB_HeightCorner = tool.m_BB_Origin;
                    tool.m_BB_OppositeCorner = tool.m_BB_Origin;
                    return NextState();
                }
            }
            return this;
        }
    }
}

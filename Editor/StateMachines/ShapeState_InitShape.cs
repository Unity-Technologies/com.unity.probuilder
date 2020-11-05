using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
#if UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.ToolManager;
#else
using ToolManager = UnityEditor.EditorTools.EditorTools;
#endif

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
            tool.DrawBoundingBox();

            if(evt.type == EventType.KeyDown)
            {
                switch(evt.keyCode)
                {
                    case KeyCode.Escape:
                        ToolManager.RestorePreviousTool();
                        break;
                }
            }

            if (evt.isMouse)
            {
                var res = EditorHandleUtility.FindBestPlaneAndBitangent(evt.mousePosition);

                Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                float hit;

                //Click has been done => Define a plane for the tool
                if (res.item1.Raycast(ray, out hit))
                {
                    //Plane init
                    tool.m_Plane = res.item1;
                    tool.m_PlaneForward = res.item2;
                    tool.m_PlaneRight = Vector3.Cross(tool.m_Plane.normal, tool.m_PlaneForward);

                    var planeNormal = tool.m_Plane.normal;
                    var planeCenter = tool.m_Plane.normal * -tool.m_Plane.distance;
                    // if hit point on plane is cardinal axis and on grid, snap to grid.
                    if (Math.IsCardinalAxis(planeNormal))
                    {
                        const float epsilon = .00001f;
                        bool offGrid = false;
                        Vector3 snapVal = EditorSnapping.activeMoveSnapValue;
                        Vector3 center = Vector3.Scale(ProBuilderSnapping.GetSnappingMaskBasedOnNormalVector(planeNormal), planeCenter);
                        for (int i = 0; i < 3; i++)
                            offGrid |= Mathf.Abs(snapVal[i] % center[i]) > epsilon;
                        tool.m_IsOnGrid = !offGrid;
                    }
                    else
                    {
                        tool.m_IsOnGrid = false;
                    }

                    if(evt.type == EventType.MouseDown)
                    {
                        //BB init
                        tool.m_BB_Origin = tool.GetPoint(ray.GetPoint(hit));
                        tool.m_BB_HeightCorner = tool.m_BB_Origin;
                        tool.m_BB_OppositeCorner = tool.m_BB_Origin;

                        return NextState();
                    }
                    else
                    {
                        tool.SetBoundsOrigin(ray.GetPoint(hit));
                    }
                }
            }
            return this;
        }
    }
}

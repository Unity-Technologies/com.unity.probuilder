using UnityEngine;
using UnityEngine.ProBuilder;
using Math = UnityEngine.ProBuilder.Math;
#if UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.ToolManager;
#else
using ToolManager = UnityEditor.EditorTools.EditorTools;
#endif

namespace UnityEditor.ProBuilder
{
    internal class ShapeState_InitShape : ShapeState
    {
        //NOTE: All class attributes are used for handle display
        EditorShapeUtility.FaceData[] m_Faces;

        Vector3 m_HitPosition = Vector3.positiveInfinity;

        protected override void InitState()
        {
            tool.m_IsShapeInit = false;

            m_Faces = new EditorShapeUtility.FaceData[6];
            for (int i = 0; i < m_Faces.Length; i++)
                m_Faces[i] = new EditorShapeUtility.FaceData();

            m_HitPosition = Vector3.positiveInfinity;

            if(tool.m_DuplicateGO != null)
                GameObject.DestroyImmediate(tool.m_DuplicateGO);
        }

        public override ShapeState DoState(Event evt)
        {
            if(evt.type == EventType.KeyDown)
            {
                switch(evt.keyCode)
                {
                    case KeyCode.Escape:
                        ToolManager.RestorePreviousTool();
                        break;
                }
            }

            if(tool.m_LastShapeCreated != null)
                EditShapeTool.DoEditingHandles(tool.m_LastShapeCreated, true);

            if(evt.isMouse && HandleUtility.nearestControl == tool.controlID)
            {
                var res = EditorHandleUtility.FindBestPlaneAndBitangent(evt.mousePosition);

                Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                float hit;

                if(res.item1.Raycast(ray, out hit))
                {
                    //Plane init
                    tool.m_Plane = res.item1;
                    tool.m_PlaneForward = res.item2;
                    tool.m_PlaneRight = Vector3.Cross(tool.m_Plane.normal, tool.m_PlaneForward);

                    var planeNormal = tool.m_Plane.normal;
                    var planeCenter = tool.m_Plane.normal * -tool.m_Plane.distance;
                    // if hit point on plane is cardinal axis and on grid, snap to grid.
                    if(Math.IsCardinalAxis(planeNormal))
                    {
                        const float epsilon = .00001f;
                        bool offGrid = false;
                        Vector3 snapVal = EditorSnapping.activeMoveSnapValue;
                        Vector3 center =
                            Vector3.Scale(ProBuilderSnapping.GetSnappingMaskBasedOnNormalVector(planeNormal),
                                planeCenter);
                        for(int i = 0; i < 3; i++)
                            offGrid |= Mathf.Abs(snapVal[i] % center[i]) > epsilon;
                        tool.m_IsOnGrid = !offGrid;
                    }
                    else
                    {
                        tool.m_IsOnGrid = false;
                    }

                    m_HitPosition = tool.GetPoint(ray.GetPoint(hit));

                    //Click has been done => Define a plane for the tool
                    if(evt.type == EventType.MouseDown)
                    {
                        //BB init
                        tool.m_BB_Origin = m_HitPosition;
                        tool.m_BB_HeightCorner = tool.m_BB_Origin;
                        tool.m_BB_OppositeCorner = tool.m_BB_Origin;

                        return NextState();
                    }
                }
                else
                {
                    m_HitPosition = Vector3.negativeInfinity;
                }
            }

            if (!Math.IsNumber(m_HitPosition))
                return this;

            tool.DoDuplicateShapePreviewHandle(m_HitPosition);

            // Repaint to visualize the placement preview dot
            if (evt.type == EventType.MouseMove && HandleUtility.nearestControl == tool.controlID)
                HandleUtility.Repaint();

            if(evt.type == EventType.Repaint)
            {
                if(GUIUtility.hotControl == 0 && HandleUtility.nearestControl == tool.controlID)
                {
                    using(new Handles.DrawingScope(EditorHandleDrawing.vertexSelectedColor))
                    {
                        Handles.DotHandleCap(-1, m_HitPosition, Quaternion.identity,
                            HandleUtility.GetHandleSize(m_HitPosition) * 0.05f, EventType.Repaint);
                    }
                }
            }

            return this;
        }
    }
}

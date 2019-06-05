using System;
using UnityEditor.EditorTools;
using UnityEngine;
using Math = UnityEngine.ProBuilder.Math;

namespace UnityEditor.ProBuilder
{
    class ShapeTool : EditorTool
    {
        enum InputState
        {
            SelectPlane,
            SetWidthAndDepth,
            SetHeight
        }

        [SerializeField]
        InputState m_InputState;

        // plane of interaction
        Plane m_Plane;
        Vector3 m_Forward;
        Vector3 m_Right;
        Vector3 m_Origin, m_OppositeCorner, m_HeightCorner;

        Quaternion m_Rotation;
        Bounds m_Bounds;

        void OnEnable()
        {
            EditorTools.EditorTools.activeToolChanged += ActiveToolChanged;
        }

        void ActiveToolChanged()
        {
            if (EditorTools.EditorTools.IsActiveTool(this))
                m_InputState = InputState.SelectPlane;
        }

        void AdvanceInputState()
        {
            if (m_InputState == InputState.SelectPlane)
                m_InputState = InputState.SetWidthAndDepth;
            else if (m_InputState == InputState.SetWidthAndDepth)
                m_InputState = InputState.SetHeight;
            else
                FinishShapeCreate();

            SceneView.RepaintAll();
        }

        void FinishShapeCreate()
        {
            m_InputState = InputState.SelectPlane;
        }

        public override void OnToolGUI(EditorWindow window)
        {
            Handles.BeginGUI();
            GUILayout.Label("state: " + m_InputState);
            Handles.EndGUI();

            var evt = Event.current;

            if (evt.type == EventType.Repaint && m_InputState != InputState.SelectPlane)
            {
                Handles.DrawLine(m_Origin, m_OppositeCorner);
                Handles.DrawLine(m_OppositeCorner, m_HeightCorner);

                // draw handle orientation
                var nrm = m_Plane.normal;
                var bit = m_Forward;
                var tan = Vector3.Cross(nrm, bit);

                nrm *= .1f;
                bit *= .1f;
                tan *= .1f;

                Handles.color = Color.blue;
                Handles.DrawLine(m_Origin - bit, m_Origin + bit);
                Handles.color = Color.red;
                Handles.DrawLine(m_Origin - tan, m_Origin + tan);
                Handles.color = Color.green;
                Handles.DrawLine(m_Origin - nrm, m_Origin + nrm);
                Handles.color = Color.white;

                // draw bounds
                RecalculateBounds();
            }

            if (EditorHandleUtility.SceneViewInUse(evt))
                return;

            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);

            switch (m_InputState)
            {
                case InputState.SelectPlane:
                    SelectPlane(evt);
                    break;
                case InputState.SetWidthAndDepth:
                    SetWidthAndDepth(evt);
                    break;
                case InputState.SetHeight:
                    SetHeight(evt);
                    break;
            }
        }

        void SelectPlane(Event evt)
        {
            if (evt.type == EventType.MouseDown)
            {
                var res = EditorHandleUtility.FindBestPlaneAndBitangent(evt.mousePosition);

                Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                float hit;

                if (res.item1.Raycast(ray, out hit))
                {
                    m_Plane = res.item1;
                    m_Forward = res.item2;
                    m_Right = Vector3.Cross(m_Plane.normal, m_Forward);
                    m_Origin = ray.GetPoint(hit);
                    m_HeightCorner = m_Origin;
                    m_OppositeCorner = m_Origin;
                    AdvanceInputState();
                }
            }
        }

        void SetWidthAndDepth(Event evt)
        {
            switch (evt.type)
            {
                case EventType.MouseDrag:
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                    float distance;

                    if (m_Plane.Raycast(ray, out distance))
                    {
                        m_OppositeCorner = ray.GetPoint(distance);
                        m_HeightCorner = m_OppositeCorner;
                        SceneView.RepaintAll();
                    }
                    break;
                }

                case EventType.MouseUp:
                {
                    AdvanceInputState();
                    break;
                }
            }
        }

        void SetHeight(Event evt)
        {
            switch (evt.type)
            {
                case EventType.MouseMove:
                case EventType.MouseDrag:
                {
                    Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                    m_HeightCorner = Math.GetNearestPointRayRay(m_OppositeCorner, m_Plane.normal, ray.origin, ray.direction);
                    SceneView.RepaintAll();
                    break;
                }

                case EventType.MouseUp:
                {
                    AdvanceInputState();
                    break;
                }
            }
        }

        void RecalculateBounds()
        {
//            Handles.DrawLine(m_Origin, m_OppositeCorner);
//            Handles.DrawLine(m_OppositeCorner, m_HeightCorner);

            var fo = HandleUtility.PointOnLineParameter(m_OppositeCorner, m_Origin, m_Forward);
            var ri = HandleUtility.PointOnLineParameter(m_OppositeCorner, m_Origin, m_Right);
            Handles.DrawLine(m_Origin, m_Origin + m_Forward * fo);
            Handles.DrawLine(m_Origin, m_Origin + m_Right * ri);


            var height = (m_HeightCorner - m_OppositeCorner).magnitude;
            m_Bounds.center = ((m_OppositeCorner + m_Origin) * .5f) + m_Plane.normal * (height * .5f);
            m_Bounds.size = new Vector3(ri, height, fo);
            m_Rotation = Quaternion.LookRotation(m_Forward, m_Plane.normal);

            EditorHandleUtility.PushMatrix();
            Handles.matrix = Matrix4x4.TRS(m_Bounds.center, m_Rotation, Vector3.one);
            Handles.DrawWireCube(Vector3.zero, m_Bounds.size);
            EditorHandleUtility.PopMatrix();
        }
    }
}

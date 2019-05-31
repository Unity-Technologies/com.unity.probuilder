using System;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Math = UnityEngine.ProBuilder.Math;
using UObject = UnityEngine.Object;

namespace UnityEditor.ProBuilder
{
    class DrawShapeTool : EditorTool
    {
        enum InputState
        {
            SelectPlane,
            SetWidthAndDepth,
            SetHeight
        }

        [SerializeField]
        InputState m_InputState;

        Shape m_Shape;

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
                FinishShape();

            SceneView.RepaintAll();
        }

        void RebuildShape()
        {
            RecalculateBounds();

            if (m_Bounds.size.sqrMagnitude < .01f)
                return;

            if (m_Shape == null)
            {
                m_Shape = new GameObject("Shape").AddComponent<Cube>();
                UndoUtility.RegisterCreatedObjectUndo(m_Shape.gameObject, "Draw Shape");
                EditorUtility.InitObject(m_Shape.mesh, false);
            }

            m_Shape.Rebuild(m_Bounds, m_Rotation);
            m_Shape.mesh.SetPivot(EditorUtility.newShapePivotLocation);
            ProBuilderEditor.Refresh(false);
        }

        void FinishShape()
        {
            m_Shape = null;
            m_InputState = InputState.SelectPlane;
        }

        void CancelShape()
        {
            if(m_Shape != null)
                DestroyImmediate(m_Shape.gameObject);
            m_InputState = InputState.SelectPlane;
        }

        public override void OnToolGUI(EditorWindow window)
        {
//            Handles.BeginGUI();
//            GUILayout.Label("state: " + m_InputState);
//            Handles.EndGUI();

            var evt = Event.current;

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
                        RebuildShape();
                        SceneView.RepaintAll();
                    }
                    break;
                }

                case EventType.MouseUp:
                {
                    if (Vector3.Distance(m_OppositeCorner, m_Origin) < .1f)
                        CancelShape();
                    else
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
                    RebuildShape();
                    SceneView.RepaintAll();
                    break;
                }

                case EventType.MouseUp:
                {
                    RebuildShape();
                    AdvanceInputState();
                    break;
                }
            }
        }

        void RecalculateBounds()
        {
            var fo = HandleUtility.PointOnLineParameter(m_OppositeCorner, m_Origin, m_Forward);
            var ri = HandleUtility.PointOnLineParameter(m_OppositeCorner, m_Origin, m_Right);

            var direction = m_HeightCorner - m_OppositeCorner;
            var height = direction.magnitude * Mathf.Sign(Vector3.Dot(m_Plane.normal, direction));

            m_Bounds.center = ((m_OppositeCorner + m_Origin) * .5f) + m_Plane.normal * (height * .5f);
            m_Bounds.size = new Vector3(ri, height, fo);
            m_Rotation = Quaternion.LookRotation(m_Forward, m_Plane.normal);

            Handles.color = new Color(.2f, .4f, .8f, 1f);
            EditorHandleUtility.PushMatrix();
            Handles.matrix = Matrix4x4.TRS(m_Bounds.center, m_Rotation, Vector3.one);
            Handles.DrawWireCube(Vector3.zero, m_Bounds.size);
            EditorHandleUtility.PopMatrix();
            Handles.color = Color.white;
        }
    }
}

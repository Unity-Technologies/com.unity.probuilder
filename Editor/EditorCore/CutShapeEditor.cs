using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;
using RaycastHit = UnityEngine.RaycastHit;

namespace UnityEditor.ProBuilder
{
    [CustomEditor(typeof(CutShape))]
    public class CutShapeEditor : Editor
    {

        static Color k_HandleColor = new Color(.8f, .8f, .8f, 1f);
        static Color k_HandleColorGreen = new Color(.01f, .9f, .3f, 1f);
        static Color k_HandleSelectedColor = new Color(.01f, .8f, .98f, 1f);

        const float k_HandleSize = .05f;

        CutShape cutShape
        {
            get { return target as CutShape; }
        }

        private int m_ControlId;
        bool m_PlacingPoint = false;
        int m_SelectedIndex = -2;

        void OnEnable()
        {
            if (cutShape == null)
            {
                DestroyImmediate(this);
                return;
            }

            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;

            Undo.undoRedoPerformed += UndoRedoPerformed;
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += DuringSceneGUI;
#else
            SceneView.onSceneGUIDelegate += DuringSceneGUI;
#endif
            EditorApplication.update += Update;
        }

        void OnDisable()
        {
            // Quit Edit mode when the object gets de-selected.
            if (cutShape != null && cutShape.cutEditMode == CutShape.CutEditMode.Edit)
                cutShape.cutEditMode = CutShape.CutEditMode.None;

            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= DuringSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= DuringSceneGUI;
#endif
            EditorApplication.update -= Update;
            Undo.undoRedoPerformed -= UndoRedoPerformed;

            //Removing the script from the object
            DestroyImmediate(cutShape);
        }


        private void DuringSceneGUI(SceneView obj)
        {
            if (cutShape.cutEditMode == CutShape.CutEditMode.None)
                return;

            Event currentEvent = Event.current;

            DoExistingPointsGUI();

            if (currentEvent.type == EventType.KeyDown)
                HandleKeyEvent(currentEvent);

            if (EditorHandleUtility.SceneViewInUse(currentEvent))
                return;

            m_ControlId = GUIUtility.GetControlID(FocusType.Passive);
            if (currentEvent.type == EventType.Layout)
                HandleUtility.AddDefaultControl(m_ControlId);

            DoPointPlacement();
        }

        private void Update()
        {
            //throw new NotImplementedException();
        }

        private void UndoRedoPerformed()
        {
            //throw new System.NotImplementedException();
        }

        private void OnSelectModeChanged(SelectMode obj)
        {
            //throw new System.NotImplementedException();
        }


        void DoExistingPointsGUI()
        {
            Transform trs = cutShape.transform;
            int len = cutShape.m_Points.Count;

            Vector3 up = cutShape.transform.up;
            Vector3 right = cutShape.transform.right;
            Vector3 forward = cutShape.transform.forward;
            Vector3 center = Vector3.zero;

            Event evt = Event.current;

            bool used = evt.type == EventType.Used;

            if (!used &&
                (evt.type == EventType.MouseDown &&
                 evt.button == 0 &&
                 !EditorHandleUtility.IsAppendModifier(evt.modifiers)))
            {
                Repaint();
            }

            if (cutShape.cutEditMode == CutShape.CutEditMode.Edit || cutShape.cutEditMode == CutShape.CutEditMode.Path)
            {
                // vertex dots
                for (int ii = 0; ii < len; ii++)
                {
                    Vector3 point = trs.TransformPoint(cutShape.m_Points[ii]);

                    center.x += point.x;
                    center.y += point.y;
                    center.z += point.z;

                    float size = HandleUtility.GetHandleSize(point) * k_HandleSize;

                    Handles.color = ii == m_SelectedIndex ? k_HandleSelectedColor : k_HandleColor;

                    EditorGUI.BeginChangeCheck();

                    point = Handles.Slider2D(point, up, right, forward, size, Handles.DotHandleCap, Vector2.zero, true);

                    if (EditorGUI.EndChangeCheck())
                    {
                        UndoUtility.RecordObject(cutShape, "Move Cut Shape Point");
                        cutShape.m_Points[ii] = GetPointInLocalSpace(point);
                    }

                    // "clicked" a button
                    if (!used && evt.type == EventType.Used)
                    {
                            used = true;
                            m_SelectedIndex = ii;
                    }
                }

                Handles.color = Color.white;

            }
        }

        private void DoPointPlacement()
        {
            Event evt = Event.current;
            EventType evtType = evt.type;

            if (cutShape.cutEditMode == CutShape.CutEditMode.Path)
            {
                if (evtType == EventType.MouseDown && HandleUtility.nearestControl == m_ControlId)
                {
                    float hitDistance = Mathf.Infinity;

                    Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                    RaycastHit hit = new RaycastHit();

                    if (cutShape.GetComponent<Collider>().Raycast(ray, out hit, hitDistance))
                    {
                        UndoUtility.RecordObject(cutShape, "Add Cut Point");

                        Vector3 point = GetPointInLocalSpace(hit.point);

                        cutShape.m_Points.Add(point);
                        evt.Use();

                        m_PlacingPoint = true;
                        m_SelectedIndex = cutShape.m_Points.Count - 1;
                    }
                }
            }
        }

        // Returns a local space point,
        Vector3 GetPointInLocalSpace(Vector3 point)
        {
            var trs = cutShape.transform;
            return trs.InverseTransformPoint(point);
        }

        void HandleKeyEvent(Event evt)
        {
            KeyCode key = evt.keyCode;

            switch (key)
            {
                case KeyCode.Backspace:
                {
                    if (m_SelectedIndex > -1)
                    {
                        UndoUtility.RecordObject(cutShape, "Delete Selected Points");
                        cutShape.m_Points.RemoveAt(m_SelectedIndex);
                        m_SelectedIndex = -1;
                        evt.Use();
                    }
                    break;
                }

                case KeyCode.Escape:
                {
                    evt.Use();
                    DestroyImmediate(cutShape);
                    break;
                }
            }
        }

    }
}

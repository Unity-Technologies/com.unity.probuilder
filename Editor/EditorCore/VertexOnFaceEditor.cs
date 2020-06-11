using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using RaycastHit = UnityEngine.ProBuilder.RaycastHit;
using UHandleUtility = UnityEditor.HandleUtility;

namespace UnityEditor.ProBuilder
{
    [CustomEditor(typeof(VertexOnFace))]
    public class VertexOnFaceEditor : Editor
    {

        static Color k_HandleColor = new Color(.8f, .8f, .8f, 1f);
        static Color k_HandleColorGreen = new Color(.01f, .9f, .3f, 1f);
        static Color k_HandleSelectedColor = new Color(.01f, .8f, .98f, 1f);

        const float k_HandleSize = .05f;

        VertexOnFace vertexOnFace
        {
            get { return target as VertexOnFace; }
        }

        private int m_ControlId;
        bool m_PlacingPoint = false;
        int m_SelectedIndex = -2;

        void OnEnable()
        {
            if (vertexOnFace == null)
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
        }

        void OnDisable()
        {
            // Quit Edit mode when the object gets de-selected.
            if (vertexOnFace != null && vertexOnFace.vertexEditMode == VertexOnFace.VertexEditMode.Edit)
                vertexOnFace.vertexEditMode = VertexOnFace.VertexEditMode.None;

            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= DuringSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= DuringSceneGUI;
#endif
            Undo.undoRedoPerformed -= UndoRedoPerformed;

            //Removing the script from the object
            DestroyImmediate(vertexOnFace);
        }


        private void DuringSceneGUI(SceneView obj)
        {
            if (vertexOnFace.vertexEditMode == VertexOnFace.VertexEditMode.None)
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
            Transform trs = vertexOnFace.transform;
            int len = vertexOnFace.m_verticesToAdd.Count;

            Vector3 up = vertexOnFace.transform.up;
            Vector3 right = vertexOnFace.transform.right;
            Vector3 forward = vertexOnFace.transform.forward;
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

            if (vertexOnFace.vertexEditMode == VertexOnFace.VertexEditMode.Edit || vertexOnFace.vertexEditMode == VertexOnFace.VertexEditMode.Add)
            {
                for (int faceIndex = 0; faceIndex < len; faceIndex++)
                {
                    int vertLen = vertexOnFace.m_verticesToAdd[faceIndex].Item2.Count;

                    for ( int vertexIndex = 0; vertexIndex < vertLen; vertexIndex++)
                    {
                        Vector3 point = trs.TransformPoint(vertexOnFace.m_verticesToAdd[faceIndex].Item2[vertexIndex]);

                        center.x += point.x;
                        center.y += point.y;
                        center.z += point.z;

                        float size = HandleUtility.GetHandleSize(point) * k_HandleSize;

                        Handles.color = k_HandleSelectedColor;

                        EditorGUI.BeginChangeCheck();

                        point = Handles.Slider2D(point, up, right, forward, size, Handles.DotHandleCap, Vector2.zero, true);

                        if (EditorGUI.EndChangeCheck())
                        {
                            UndoUtility.RecordObject(vertexOnFace, "Move Vertex On Face");
                            vertexOnFace.m_verticesToAdd[faceIndex].Item2[vertexIndex] = GetPointInLocalSpace(point);
                        }

                        // "clicked" a button
                        if (!used && evt.type == EventType.Used)
                        {
                            used = true;
                        }

                    }
                }

                Handles.color = Color.white;
            }
        }

        // Returns a local space point,
        Vector3 GetPointInLocalSpace(Vector3 point)
        {
            var trs = vertexOnFace.transform;
            return trs.InverseTransformPoint(point);
        }

        private void DoPointPlacement()
        {
            Event evt = Event.current;
            EventType evtType = evt.type;

            if (vertexOnFace.vertexEditMode == VertexOnFace.VertexEditMode.Add)
            {
                if (evtType == EventType.MouseDown && HandleUtility.nearestControl == m_ControlId)
                {
                    float hitDistance = Mathf.Infinity;

                    Ray ray = UHandleUtility.GUIPointToWorldRay(evt.mousePosition);
                    RaycastHit pbHit;

                    if (UnityEngine.ProBuilder.HandleUtility.FaceRaycast(ray, vertexOnFace.mesh, out pbHit))
                    {
                        UndoUtility.RecordObject(vertexOnFace, "Add Vertex On Face");

                        Face hitFace = vertexOnFace.mesh.faces[pbHit.face];
                        if (vertexOnFace.m_verticesToAdd.Exists(tup => tup.Item1 == hitFace))
                        {
                            vertexOnFace.m_verticesToAdd.Find(tup => tup.Item1 == hitFace).Item2.Add(pbHit.point);
                        }
                        else
                        {
                            List<Vector3> vertexList = new List<Vector3>();
                            vertexList.Add(pbHit.point);
                            vertexOnFace.m_verticesToAdd.Add(new System.Tuple<Face, List<Vector3>>(hitFace,vertexList));
                        }


                        m_SelectedIndex = vertexOnFace.m_verticesToAdd.Count - 1;
                        evt.Use();

                        m_PlacingPoint = true;
                    }
                }
            }
        }


        void HandleKeyEvent(Event evt)
        {
            KeyCode key = evt.keyCode;

            switch (key)
            {
                case KeyCode.Backspace:
                {
                    UndoUtility.RecordObject(vertexOnFace, "Delete Selected Points");
                    vertexOnFace.m_verticesToAdd.RemoveAt(m_SelectedIndex);
                    evt.Use();
                    break;
                }

                case KeyCode.Escape:
                {
                    evt.Use();
                    DestroyImmediate(vertexOnFace);
                    break;
                }

                case KeyCode.Return:
                {
                    UpdateProBuilderMesh();
                    evt.Use();
                    break;
                }
            }
        }

        public void UpdateProBuilderMesh()
        {
            UndoUtility.RecordObject(vertexOnFace.mesh, "Add Vertices to ProBuilder Mesh");
            int impactedFacedCount = vertexOnFace.m_verticesToAdd.Count;

            for (int faceIndex = 0; faceIndex < impactedFacedCount; faceIndex++)
            {
                System.Tuple<Face, List<Vector3>> impactedFace = vertexOnFace.m_verticesToAdd[faceIndex];
                vertexOnFace.mesh.AppendVerticesToFace(impactedFace.Item1, impactedFace.Item2.ToArray(),false);
            }

            UndoUtility.RecordObject(vertexOnFace, "Removing Script from ProBuilder Object");
            DestroyImmediate(vertexOnFace);

            Debug.Log("Insertion Done");
        }

    }
}

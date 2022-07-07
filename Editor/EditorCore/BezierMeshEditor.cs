using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.ProBuilder;
using UnityEngine.Splines;

namespace UnityEditor.ProBuilder
{
    [CustomEditor(typeof(BezierMesh))]
    sealed class BezierMeshEditor : Editor
    {
        static GUIContent[] s_TangentModeIcons = new GUIContent[3];

        const float k_HandleSize = .05f;

        static float3 Float3_Zero = new float3(0f, 0f, 0f);
        static float3 Float3_Forward = new float3(0f, 0f, 1f);
        static float3 Float3_Backward = new float3(0f, 0f, -1f);

        static Color bezierPositionHandleColor = new Color(.01f, .8f, .99f, 1f);
        static Color bezierTangentHandleColor = new Color(.6f, .6f, .6f, .8f);

        [SerializeField] BezierHandle m_currentHandle = new BezierHandle(-1, false);

        [SerializeField] BezierTangentMode m_TangentMode = BezierTangentMode.Mirrored;

        BezierMesh m_Target;
        bool m_IsMoving;
        List<Vector3> m_ControlPoints;

        ProBuilderMesh m_CurrentObject
        {
            get
            {
                if (m_Target.mesh == null)
                {
                    m_Target.mesh = m_Target.gameObject.AddComponent<ProBuilderMesh>();
                    EditorUtility.InitObject(m_Target.mesh);
                }

                return m_Target.mesh;
            }
        }

        List<BezierKnot> m_Knots
        {
            get { return m_Target.m_Spline.Knots as List<BezierKnot>; }
            set
            {
                foreach (var val in value)
                {
                    m_Target.m_Spline.Add(val);
                }
            }
        }

        bool m_IsEditing
        {
            get { return m_Target.isEditing; }
            set { m_Target.isEditing = value; }
        }

        float m_Radius
        {
            get { return m_Target.m_Radius; }

            set
            {
                if (m_Target.m_Radius != value)
                    UndoUtility.RecordObject(m_Target, "Set Bezier Shape Radius");
                m_Target.m_Radius = value;
            }
        }

        int m_FaceCountPerSegment
        {
            get { return m_Target.m_FaceCountPerSegment; }

            set
            {
                if (m_Target.m_FaceCountPerSegment != value)
                    UndoUtility.RecordObject(m_Target, "Set Bezier Shape Face Count for each segment");
                m_Target.m_FaceCountPerSegment = value;
            }
        }

        int m_SegmentsPerUnit
        {
            get { return m_Target.m_SegmentsPerUnit; }

            set
            {
                if (m_Target.m_FaceCountPerSegment != value)
                    UndoUtility.RecordObject(m_Target, "Set Bezier Shape Segments Per Unit Count");
                m_Target.m_SegmentsPerUnit = value;
            }
        }

        [System.Serializable]
        struct BezierHandle
        {
            public int index;
            public bool isTangent;
            public BezierTangentDirection tangent;

            public BezierHandle(int index, bool isTangent, BezierTangentDirection tangent = BezierTangentDirection.In)
            {
                this.index = index;
                this.isTangent = isTangent;
                this.tangent = tangent;
            }

            public static implicit operator int(BezierHandle handle)
            {
                return handle.index;
            }

            public static explicit operator BezierHandle(int index)
            {
                return new BezierHandle(index, false);
            }

            public static implicit operator BezierTangentDirection(BezierHandle handle)
            {
                return handle.tangent;
            }

            public void SetIndex(int index)
            {
                this.index = index;
                this.isTangent = false;
            }

            public void SetIndexAndTangent(int index, BezierTangentDirection dir)
            {
                this.index = index;
                this.isTangent = true;
                this.tangent = dir;
            }
        }

        private GUIStyle _commandStyle = null;

        public GUIStyle commandStyle
        {
            get
            {
                if (_commandStyle == null)
                {
                    _commandStyle =
                        new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("Command"));
                    _commandStyle.alignment = TextAnchor.MiddleCenter;
                }

                return _commandStyle;
            }
        }

        void OnEnable()
        {
            m_Target = target as BezierMesh;

            Undo.undoRedoPerformed += this.UndoRedoPerformed;

            s_TangentModeIcons[0] = new GUIContent(IconUtility.GetIcon("Toolbar/Bezier_Free"), "Tangent Mode: Free");
            s_TangentModeIcons[1] =
                new GUIContent(IconUtility.GetIcon("Toolbar/Bezier_Aligned"), "Tangent Mode: Aligned");
            s_TangentModeIcons[2] =
                new GUIContent(IconUtility.GetIcon("Toolbar/Bezier_Mirrored"), "Tangent Mode: Mirrored");

            if (m_Target != null)
                SetIsEditing(m_Target.isEditing);

            ProBuilderEditor.selectModeChanged += SelectModeChanged;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= this.UndoRedoPerformed;
            ProBuilderEditor.selectModeChanged -= SelectModeChanged;
        }

        void SelectModeChanged(SelectMode mode)
        {
            if (!mode.ContainsFlag(SelectMode.InputTool) && m_IsEditing)
            {
                SetIsEditing(false);
                Repaint();
            }
        }

        BezierKnot DoBezierKnotGUI(BezierKnot knot)
        {
            Vector3 pos = knot.Position, tin = knot.TangentIn, tout = knot.TangentOut;

            bool wasInWideMode = EditorGUIUtility.wideMode;
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.wideMode = true;
            EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth / 3f;

            EditorGUI.BeginChangeCheck();
            pos = EditorGUILayout.Vector3Field("Position", pos);
            if (EditorGUI.EndChangeCheck())
                knot.Position = pos;

            EditorGUI.BeginChangeCheck();
            tin = EditorGUILayout.Vector3Field("Tan. In", tin);
            if (EditorGUI.EndChangeCheck())
                knot.TangentIn = tin;

            Rect r = GUILayoutUtility.GetLastRect();
            r.x += EditorGUIUtility.labelWidth - 12;
            GUI.color = Color.blue;
            GUI.Label(r, "\u2022");
            GUI.color = Color.white;

            EditorGUI.BeginChangeCheck();
            tout = EditorGUILayout.Vector3Field("Tan. Out", tout);
            if (EditorGUI.EndChangeCheck())
                knot.TangentOut = tout;

            r = GUILayoutUtility.GetLastRect();
            r.x += EditorGUIUtility.labelWidth - 12;
            GUI.color = Color.red;
            GUI.Label(r, "\u2022");
            GUI.color = Color.white;

            /*
             * check for conversion of quaternion to euler
             */
            Vector4 euler = knot.Rotation.value;
            euler = EditorGUILayout.Vector4Field("Rotation", euler);
            knot.Rotation = Quaternion.Euler(euler);

            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUIUtility.wideMode = wasInWideMode;

            return knot;
        }

        void SetIsEditing(bool isEditing)
        {
            GUIUtility.hotControl = 0;

            if (isEditing && !m_IsEditing)
            {
                if (ProBuilderEditor.instance != null)
                    ProBuilderEditor.instance.ClearElementSelection();

                UndoUtility.RecordObject(m_Target, "Edit Bezier Spline");
                UndoUtility.RecordObject(m_Target.mesh, "Edit Bezier Spline");

                UpdateMesh(true);
            }

            m_Target.isEditing = isEditing;

            if (m_Target.isEditing)
                ProBuilderEditor.selectMode |= SelectMode.InputTool;
            else
                ProBuilderEditor.selectMode &= ~SelectMode.InputTool;

            if (m_Target.isEditing)
            {
                Tools.current = Tool.None;
                UpdateControlPoints();
            }
        }

        public override void OnInspectorGUI()
        {
            if (!m_IsEditing)
            {
                if (GUILayout.Button("Edit Bezier Shape"))
                    SetIsEditing(true);

                EditorGUILayout.HelpBox(
                    "Editing a Bezier Shape will erase any modifications made to the mesh!\n\nIf you accidentally enter Edit Mode you can Undo to get your changes back.",
                    MessageType.Warning);

                return;
            }

            if (GUILayout.Button("Editing Bezier Shape", UI.EditorGUIUtility.GetActiveStyle("Button")))
                SetIsEditing(false);

            Event e = Event.current;

            if (m_IsMoving)
            {
                if (e.type == EventType.Ignore ||
                    e.type == EventType.MouseUp)
                    OnFinishVertexModification();
            }

            EditorGUI.BeginChangeCheck();

            bool handleIsValid = (m_currentHandle > -1 && m_currentHandle < m_Knots.Count());

            BezierKnot inspectorPoint = handleIsValid
                ? m_Knots[m_currentHandle]
                : new BezierKnot(Float3_Zero, Float3_Backward, Float3_Forward, Quaternion.identity);

            inspectorPoint = DoBezierKnotGUI(inspectorPoint);

            if (handleIsValid && EditorGUI.EndChangeCheck())
            {
                if (!m_IsMoving)
                    OnBeginVertexModification();

                m_Knots[m_currentHandle] = inspectorPoint;
                UpdateMesh(false);
            }

            EditorGUI.BeginChangeCheck();

            if (GUILayout.Button("Clear Points"))
            {
                UndoUtility.RecordObject(m_Target, "Clear Bezier Spline Points");
                m_Knots.Clear();
                UpdateMesh(true);
            }

            if (GUILayout.Button("Add Point"))
            {
                UndoUtility.RecordObject(m_Target, "Add Bezier Spline Point");

                if (m_Knots.Count > 0)
                {
                    m_Knots.Add(new BezierKnot(m_Knots[m_Knots.Count - 1].Position,
                        m_Knots[m_Knots.Count - 1].TangentIn,
                        m_Knots[m_Knots.Count - 1].TangentOut,
                        Quaternion.identity));
                    UpdateMesh(true);
                }
                else
                {
                    m_Target.Init();
                }

                m_currentHandle = (BezierHandle)(m_Knots.Count - 1);

                SceneView.RepaintAll();
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            m_TangentMode = (BezierTangentMode)GUILayout.Toolbar((int)m_TangentMode, s_TangentModeIcons, commandStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            m_Radius = Mathf.Max(.001f, EditorGUILayout.FloatField("Radius", m_Radius));
            m_SegmentsPerUnit = Math.Clamp(EditorGUILayout.IntField("Segments Per Unit", m_SegmentsPerUnit), 1, 512);
            m_FaceCountPerSegment =
                Math.Clamp(EditorGUILayout.IntField("Faces Per Segment", m_FaceCountPerSegment), 3, 512);

            if (EditorGUI.EndChangeCheck())
            {
                UpdateMesh(true);
            }
        }

        void UpdateMesh(bool vertexCountChanged)
        {
            if (m_Target != null)
            {
                m_Target.Refresh();
                UpdateControlPoints();
                ProBuilderEditor.Refresh(vertexCountChanged);
            }
        }

        void UpdateControlPoints()
        {
            //m_ControlPoints = Spline.GetControlPoints(m_Points, m_SegmentsPerUnit, m_CloseLoop, null);
        }

        void OnSceneGUI()
        {
            Event e = Event.current;

            bool eventHasBeenUsed = false;

            if (m_IsMoving)
            {
                if (e.type == EventType.Ignore ||
                    e.type == EventType.MouseUp)
                {
                    eventHasBeenUsed = true;
                    OnFinishVertexModification();
                }
            }

            bool sceneViewInUse = EditorHandleUtility.SceneViewInUse(e);

            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Backspace && m_currentHandle > -1 && m_currentHandle < m_Knots.Count)
                {
                    UndoUtility.RecordObject(m_Target, "Delete Bezier Knot");
                    m_Target.m_Spline.RemoveAt(m_currentHandle);
                    UpdateMesh(true);
                }
                else if (e.keyCode == KeyCode.Escape)
                {
                    SetIsEditing(false);
                }
            }

            int count = m_Knots.Count;

            Matrix4x4 handleMatrix = Handles.matrix;
            Handles.matrix = m_Target.transform.localToWorldMatrix;

            EditorGUI.BeginChangeCheck();

            for (int index = 0; index < count; index++)
            {
                // if (index < count - 1)
                // {
                //     Handles.DrawBezier(m_Knots[index].Position,
                //         m_Knots[(index + 1) % count].Position,
                //         m_Knots[index].TangentOut,
                //         m_Knots[(index + 1) % count].TangentIn,
                //         Color.green,
                //         EditorGUIUtility.whiteTexture,
                //         1f);
                // }

                if (!m_IsEditing)
                    continue;

                // If the index is selected show the full transform gizmo, otherwise use free move handles
                if (m_currentHandle == index)
                {
                    BezierKnot knot = m_Knots[index];

                    if (!m_currentHandle.isTangent)
                    {
                        Vector3 prev = knot.Position;

                        prev = Handles.PositionHandle(prev, Quaternion.identity);

                        if (!Math.Approx3(prev, knot.Position))
                        {
                            if (!m_IsMoving)
                                OnBeginVertexModification();

                            prev = EditorSnapping.MoveSnap(prev);

                            Vector3 dir = prev - (Vector3)knot.Position;
                            knot.Position = prev;
                            knot.TangentIn += (float3)dir;
                            knot.TangentOut += (float3)dir;
                        }

                        // rotation
                        int prev_index = index > 0 ? index - 1 : (-1);
                        int next_index = index < count - 1 ? index + 1 : (-1);
                        Vector3 rd = SplineUtility.EvaluateTangent(m_Target.m_Spline, ((float)index) / m_Knots.Count);

                        Quaternion look = Quaternion.LookRotation(rd);
                        float size = HandleUtility.GetHandleSize(knot.Position);
                        Matrix4x4 pm = Handles.matrix;
                        Handles.matrix = pm * Matrix4x4.TRS(knot.Position, look, Vector3.one);
                        knot.Rotation = Handles.Disc(knot.Rotation, Vector3.zero, Vector3.forward, size, false, 0f);
                        Handles.matrix = pm;
                    }
                    else
                    {
                        Handles.color = bezierTangentHandleColor;

                        if (m_currentHandle.tangent == BezierTangentDirection.In && (index > 0))
                        {
                            EditorGUI.BeginChangeCheck();
                            knot.TangentIn = Handles.PositionHandle(knot.TangentIn, Quaternion.identity);
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (!m_IsMoving)
                                    OnBeginVertexModification();

                                knot.TangentIn = EditorSnapping.MoveSnap(knot.TangentIn);
                                // knot.EnforceTangentMode(BezierTangentDirection.In, m_TangentMode);
                            }

                            Handles.color = Color.blue;
                            Handles.DrawLine(m_Knots[index].Position, m_Knots[index].TangentIn);
                        }

                        if (m_currentHandle.tangent == BezierTangentDirection.Out && (index < count - 1))
                        {
                            EditorGUI.BeginChangeCheck();
                            knot.TangentOut = Handles.PositionHandle(knot.TangentOut, Quaternion.identity);
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (!m_IsMoving)
                                    OnBeginVertexModification();

                                knot.TangentOut = EditorSnapping.MoveSnap(knot.TangentOut);
                                //knot.EnforceTangentMode(BezierTangentDirection.Out, m_TangentMode);
                            }

                            Handles.color = Color.red;
                            Handles.DrawLine(m_Knots[index].Position, m_Knots[index].TangentOut);
                        }
                    }

                    m_Knots[index] = knot;
                }
            }

            if (!m_IsEditing)
                return;

            EventType eventType = e.type;

            if (!eventHasBeenUsed)
                eventHasBeenUsed = eventType == EventType.Used;

            for (int index = 0; index < count; index++)
            {
                Vector3 prev;
                BezierKnot knot = m_Knots[index];

                // Position Handle
                float size = HandleUtility.GetHandleSize(knot.Position) * k_HandleSize;
                Handles.color = bezierPositionHandleColor;

                if (m_currentHandle == index && !m_currentHandle.isTangent)
                {
                    Handles.DotHandleCap(0, knot.Position, Quaternion.identity, size, e.type);
                }
                else
                {
                    prev = knot.Position;
                    prev = FreeMoveHandle(prev, size, Vector3.zero, Handles.DotHandleCap);
                    if (!eventHasBeenUsed && eventType == EventType.MouseUp && e.type == EventType.Used)
                    {
                        eventHasBeenUsed = true;
                        m_currentHandle = (BezierHandle)index;
                        Repaint();
                        SceneView.RepaintAll();
                    }
                    else if (!Math.Approx3(prev, knot.Position))
                    {
                        if (!m_IsMoving)
                            OnBeginVertexModification();

                        knot.Position = (EditorSnapping.MoveSnap(prev));
                    }
                }

                // Tangent handles
                Handles.color = bezierTangentHandleColor;

                // Tangent In Handle
                if (index > 0)
                {
                    size = HandleUtility.GetHandleSize(knot.TangentIn) * k_HandleSize;
                    Handles.DrawLine(knot.Position, knot.TangentIn);

                    if (index == m_currentHandle && m_currentHandle.isTangent &&
                        m_currentHandle.tangent == BezierTangentDirection.In)
                    {
                        Handles.DotHandleCap(0, knot.TangentIn, Quaternion.identity, size, e.type);
                    }
                    else
                    {
                        prev = knot.TangentIn;
                        prev = FreeMoveHandle(prev, size, Vector3.zero, Handles.DotHandleCap);

                        if (!eventHasBeenUsed && eventType == EventType.MouseUp && e.type == EventType.Used)
                        {
                            eventHasBeenUsed = true;
                            m_currentHandle.SetIndexAndTangent(index, BezierTangentDirection.In);
                            Repaint();
                            SceneView.RepaintAll();
                        }
                        else if (!Math.Approx3(prev, knot.TangentIn))
                        {
                            if (!m_IsMoving)
                                OnBeginVertexModification();
                            knot.TangentIn = EditorSnapping.MoveSnap(prev);
                            // knot.EnforceTangentMode(BezierTangentDirection.In, m_TangentMode);
                        }
                    }
                }

                // Tangent Out
                if (index < count - 1)
                {
                    size = HandleUtility.GetHandleSize(knot.TangentOut) * k_HandleSize;
                    Handles.DrawLine(knot.Position, knot.TangentOut);

                    if (index == m_currentHandle && m_currentHandle.isTangent &&
                        m_currentHandle.tangent == BezierTangentDirection.Out)
                    {
                        Handles.DotHandleCap(0, knot.TangentOut, Quaternion.identity, size, e.type);
                    }
                    else
                    {
                        prev = knot.TangentOut;
                        prev = FreeMoveHandle(prev, size, Vector3.zero, Handles.DotHandleCap);

                        if (!eventHasBeenUsed && eventType == EventType.MouseUp && e.type == EventType.Used)
                        {
                            eventHasBeenUsed = true;
                            m_currentHandle.SetIndexAndTangent(index, BezierTangentDirection.Out);
                            Repaint();
                            SceneView.RepaintAll();
                        }
                        else if (!Math.Approx3(prev, knot.TangentOut))
                        {
                            if (!m_IsMoving)
                                OnBeginVertexModification();
                            knot.TangentOut = EditorSnapping.MoveSnap(prev);
                            // knot.EnforceTangentMode(BezierTangentDirection.Out, m_TangentMode);
                        }
                    }
                }

                m_Knots[index] = knot;
            }

            // Do control point insertion
            if (!eventHasBeenUsed && m_ControlPoints != null && m_ControlPoints.Count > 1)
            {
                int index = -1;
                float distanceToLine;

                Vector3 p = EditorHandleUtility.ClosestPointToPolyLine(m_ControlPoints, out index, out distanceToLine,
                    false, null);

                if (!IsHoveringHandlePoint(e.mousePosition) &&
                    distanceToLine < PreferenceKeys.k_MaxPointDistanceFromControl)
                {
                    Handles.color = Color.green;
                    Handles.DotHandleCap(-1, p, Quaternion.identity, HandleUtility.GetHandleSize(p) * .05f, e.type);
                    Handles.color = Color.white;

                    if (!eventHasBeenUsed && eventType == EventType.MouseDown && e.button == 0)
                    {
                        UndoUtility.RecordObject(m_Target, "Add Point");
                        Vector3 dir = m_ControlPoints[(index + 1) % m_ControlPoints.Count] - m_ControlPoints[index];
                        m_Knots.Insert((index / m_SegmentsPerUnit) + 1,
                            new BezierKnot(p, p - dir, p + dir, Quaternion.identity));
                        UpdateMesh(true);
                        e.Use();
                    }

                    SceneView.RepaintAll();
                }
            }

            if (e.type == EventType.MouseUp && !sceneViewInUse)
                m_currentHandle.SetIndex(-1);

            Handles.matrix = handleMatrix;

            if (EditorGUI.EndChangeCheck())
                UpdateMesh(false);
        }

        Vector3 FreeMoveHandle(Vector3 position, float size, Vector3 snap, Handles.CapFunction capFunction)
        {
#if UNITY_2022
            return Handles.FreeMoveHandle(position, size, snap, capFunction);
#else
            return Handles.FreeMoveHandle(position, Quaternion.identity, size, snap, capFunction);
#endif
        }

        bool IsHoveringHandlePoint(Vector2 mpos)
        {
            if (m_Target == null)
                return false;

            int count = m_Knots.Count;

            for (int i = 0; i < count; i++)
            {
                BezierKnot knot = m_Knots[i];

                bool ti = i > 0;
                bool to = i < (count - 1);

                if (Vector2.Distance(mpos, HandleUtility.WorldToGUIPoint(knot.Position)) <
                    PreferenceKeys.k_MaxPointDistanceFromControl ||
                    (ti && Vector2.Distance(mpos, HandleUtility.WorldToGUIPoint(knot.TangentIn)) <
                        PreferenceKeys.k_MaxPointDistanceFromControl) ||
                    (to && Vector2.Distance(mpos, HandleUtility.WorldToGUIPoint(knot.TangentOut)) <
                        PreferenceKeys.k_MaxPointDistanceFromControl))
                    return true;
            }

            return false;
        }

        void UndoRedoPerformed()
        {
            if (m_Target && m_IsEditing)
            {
                UpdateControlPoints();
                UpdateMesh(true);
            }
        }

        void OnBeginVertexModification()
        {
            m_IsMoving = true;
            UndoUtility.RecordObject(m_Target, "Modify Bezier Spline");
            Lightmapping.PushGIWorkflowMode();
        }

        void OnFinishVertexModification()
        {
            m_IsMoving = false;
            Lightmapping.PopGIWorkflowMode();
            m_CurrentObject.Optimize();
            ProBuilderEditor.Refresh();
        }
    }
}

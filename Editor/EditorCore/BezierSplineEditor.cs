using UnityEngine;
using System.Collections.Generic;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    [CustomEditor(typeof(BezierShape))]
    sealed class BezierShapeEditor : Editor
    {
        static readonly GUIContent[] s_TangentModeIcons = new GUIContent[3];

        const float k_HandleSize = .05f;

        static Vector3 Vector3_Zero = new Vector3(0f, 0f, 0f);
        static Vector3 Vector3_Forward = new Vector3(0f, 0f, 1f);
        static Vector3 Vector3_Backward = new Vector3(0f, 0f, -1f);

        static Color bezierPositionHandleColor = new Color(.01f, .8f, .99f, 1f);
        static Color bezierTangentHandleColor = new Color(.6f, .6f, .6f, .8f);

        [SerializeField]
        BezierHandle m_currentHandle = new BezierHandle(-1, false);

        [SerializeField]
        BezierTangentMode m_TangentMode = BezierTangentMode.Mirrored;

        BezierShape m_Target = null;
        bool m_IsMoving = false;
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

        List<BezierPoint> m_Points
        {
            get { return m_Target.points; }
            set { m_Target.points = value; }
        }

        bool m_IsEditing
        {
            get { return m_Target.isEditing; }
            set { m_Target.isEditing = value; }
        }

        bool m_CloseLoop
        {
            get { return m_Target.closeLoop; }

            set
            {
                if (m_Target.closeLoop != value)
                    UndoUtility.RecordObject(m_Target, "Set Bezier Shape Close Loop");
                m_Target.closeLoop = value;
            }
        }

        float m_Radius
        {
            get { return m_Target.radius; }

            set
            {
                if (m_Target.radius != value)
                    UndoUtility.RecordObject(m_Target, "Set Bezier Shape Radius");
                m_Target.radius = value;
            }
        }

        int m_Rows
        {
            get { return m_Target.rows; }

            set
            {
                if (m_Target.rows != value)
                    UndoUtility.RecordObject(m_Target, "Set Bezier Shape Rows");
                m_Target.rows = value;
            }
        }

        int m_Columns
        {
            get { return m_Target.columns; }

            set
            {
                if (m_Target.columns != value)
                    UndoUtility.RecordObject(m_Target, "Set Bezier Shape Columns");
                m_Target.columns = value;
            }
        }

        bool m_Smooth
        {
            get { return m_Target.smooth; }

            set
            {
                if (m_Target.smooth != value)
                    UndoUtility.RecordObject(m_Target, "Set Bezier Shape Smooth");
                m_Target.smooth = value;
            }
        }

        private GUIStyle _commandStyle = null;

        public GUIStyle commandStyle
        {
            get
            {
                if (_commandStyle == null)
                {
                    _commandStyle = new GUIStyle(EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).FindStyle("Command"));
                    _commandStyle.alignment = TextAnchor.MiddleCenter;
                }

                return _commandStyle;
            }
        }

        void OnEnable()
        {
            m_Target = target as BezierShape;

            Undo.undoRedoPerformed += this.UndoRedoPerformed;

            s_TangentModeIcons[0] = new GUIContent(IconUtility.GetIcon("Toolbar/Bezier_Free"), "Tangent Mode: Free");
            s_TangentModeIcons[1] = new GUIContent(IconUtility.GetIcon("Toolbar/Bezier_Aligned"), "Tangent Mode: Aligned");
            s_TangentModeIcons[2] = new GUIContent(IconUtility.GetIcon("Toolbar/Bezier_Mirrored"), "Tangent Mode: Mirrored");

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

        BezierPoint DoBezierPointGUI(BezierPoint point)
        {
            Vector3 pos = point.position, tin = point.tangentIn, tout = point.tangentOut;

            bool wasInWideMode = EditorGUIUtility.wideMode;
            float labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.wideMode = true;
            EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth / 3f;

            EditorGUI.BeginChangeCheck();
            pos = EditorGUILayout.Vector3Field("Position", pos);
            if (EditorGUI.EndChangeCheck())
                point.SetPosition(pos);

            EditorGUI.BeginChangeCheck();
            tin = EditorGUILayout.Vector3Field("Tan. In", tin);
            if (EditorGUI.EndChangeCheck())
                point.SetTangentIn(tin, m_TangentMode);
            Rect r = GUILayoutUtility.GetLastRect();
            r.x += EditorGUIUtility.labelWidth - 12;
            GUI.color = Color.blue;
            GUI.Label(r, "\u2022");
            GUI.color = Color.white;

            EditorGUI.BeginChangeCheck();
            tout = EditorGUILayout.Vector3Field("Tan. Out", tout);
            if (EditorGUI.EndChangeCheck())
                point.SetTangentOut(tout, m_TangentMode);
            r = GUILayoutUtility.GetLastRect();
            r.x += EditorGUIUtility.labelWidth - 12;
            GUI.color = Color.red;
            GUI.Label(r, "\u2022");
            GUI.color = Color.white;

            Vector3 euler = point.rotation.eulerAngles;
            euler = EditorGUILayout.Vector3Field("Rotation", euler);
            point.rotation = Quaternion.Euler(euler);

            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUIUtility.wideMode = wasInWideMode;

            return point;
        }

        void SetIsEditing(bool isEditing)
        {
            GUIUtility.hotControl = 0;

            if (isEditing && !m_IsEditing)
            {
                if (ProBuilderEditor.instance != null)
                    ProBuilderEditor.instance.ClearElementSelection();

                UndoUtility.RecordObject(m_Target, "Edit Bezier Shape");
                UndoUtility.RecordObject(m_Target.mesh, "Edit Bezier Shape");

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

                EditorGUILayout.HelpBox("Editing a Bezier Shape will erase any modifications made to the mesh!\n\nIf you accidentally enter Edit Mode you can Undo to get your changes back.", MessageType.Warning);

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

            bool handleIsValid = (m_currentHandle > -1 && m_currentHandle < m_Points.Count);

            BezierPoint inspectorPoint = handleIsValid ?
                m_Points[m_currentHandle] :
                new BezierPoint(Vector3_Zero, Vector3_Backward, Vector3_Forward, Quaternion.identity);

            inspectorPoint = DoBezierPointGUI(inspectorPoint);

            if (handleIsValid && EditorGUI.EndChangeCheck())
            {
                if (!m_IsMoving)
                    OnBeginVertexModification();

                m_Points[m_currentHandle] = inspectorPoint;
                UpdateMesh(false);
            }

            EditorGUI.BeginChangeCheck();

            if (GUILayout.Button("Clear Points"))
            {
                UndoUtility.RecordObject(m_Target, "Clear Bezier Spline Points");
                m_Points.Clear();
                UpdateMesh(true);
            }

            if (GUILayout.Button("Add Point"))
            {
                UndoUtility.RecordObject(m_Target, "Add Bezier Spline Point");

                if (m_Points.Count > 0)
                {
                    m_Points.Add(new BezierPoint(m_Points[m_Points.Count - 1].position,
                            m_Points[m_Points.Count - 1].tangentIn,
                            m_Points[m_Points.Count - 1].tangentOut,
                            Quaternion.identity));
                    UpdateMesh(true);
                }
                else
                {
                    m_Target.Init();
                }

                m_currentHandle = (BezierHandle)(m_Points.Count - 1);

                SceneView.RepaintAll();
            }

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            m_TangentMode = (BezierTangentMode)GUILayout.Toolbar((int)m_TangentMode, s_TangentModeIcons, commandStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            m_CloseLoop = EditorGUILayout.Toggle("Close Loop", m_CloseLoop);
            m_Smooth = EditorGUILayout.Toggle("Smooth", m_Smooth);
            m_Radius = Mathf.Max(.001f, EditorGUILayout.FloatField("Radius", m_Radius));
            m_Rows = Math.Clamp(EditorGUILayout.IntField("Rows", m_Rows), 3, 512);
            m_Columns = Math.Clamp(EditorGUILayout.IntField("Columns", m_Columns), 3, 512);

            if (EditorGUI.EndChangeCheck())
                UpdateMesh(true);
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
            m_ControlPoints = Spline.GetControlPoints(m_Points, m_Columns, m_CloseLoop, null);
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
                if (e.keyCode == KeyCode.Backspace && m_currentHandle > -1 && m_currentHandle < m_Points.Count)
                {
                    UndoUtility.RecordObject(m_Target, "Delete Bezier Point");
                    m_Points.RemoveAt(m_currentHandle);
                    UpdateMesh(true);
                }
                else if (e.keyCode == KeyCode.Escape)
                {
                    SetIsEditing(false);
                }
            }

            int count = m_Points.Count;

            Matrix4x4 handleMatrix = Handles.matrix;
            Handles.matrix = m_Target.transform.localToWorldMatrix;

            EditorGUI.BeginChangeCheck();

            for (int index = 0; index < count; index++)
            {
                if (index < count - 1 || m_CloseLoop)
                {
                    Handles.DrawBezier(m_Points[index].position,
                        m_Points[(index + 1) % count].position,
                        m_Points[index].tangentOut,
                        m_Points[(index + 1) % count].tangentIn,
                        Color.green,
                        EditorGUIUtility.whiteTexture,
                        1f);
                }

                if (!m_IsEditing)
                    continue;

                // If the index is selected show the full transform gizmo, otherwise use free move handles
                if (m_currentHandle == index)
                {
                    BezierPoint point = m_Points[index];

                    if (!m_currentHandle.isTangent)
                    {
                        Vector3 prev = point.position;

                        prev = Handles.PositionHandle(prev, Quaternion.identity);

                        if (!Math.Approx3(prev, point.position))
                        {
                            if (!m_IsMoving)
                                OnBeginVertexModification();

                            prev = EditorSnapping.MoveSnap(prev);

                            Vector3 dir = prev - point.position;
                            point.position = prev;
                            point.tangentIn += dir;
                            point.tangentOut += dir;
                        }

                        // rotation
                        int prev_index = index > 0 ? index - 1 : (m_CloseLoop ? count - 1 : -1);
                        int next_index = index < count - 1 ? index + 1 : (m_CloseLoop ? 0 : -1);
                        Vector3 rd = BezierPoint.GetLookDirection(m_Points, index, prev_index, next_index);

                        Quaternion look = Quaternion.LookRotation(rd);
                        float size = HandleUtility.GetHandleSize(point.position);
                        Matrix4x4 pm = Handles.matrix;
                        Handles.matrix = pm * Matrix4x4.TRS(point.position, look, Vector3.one);
                        point.rotation = Handles.Disc(point.rotation, Vector3.zero, Vector3.forward, size, false, 0f);
                        Handles.matrix = pm;
                    }
                    else
                    {
                        Handles.color = bezierTangentHandleColor;

                        if (m_currentHandle.tangent == BezierTangentDirection.In && (m_CloseLoop || index > 0))
                        {
                            EditorGUI.BeginChangeCheck();
                            point.tangentIn = Handles.PositionHandle(point.tangentIn, Quaternion.identity);
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (!m_IsMoving)
                                    OnBeginVertexModification();

                                point.tangentIn = EditorSnapping.MoveSnap(point.tangentIn);
                                point.EnforceTangentMode(BezierTangentDirection.In, m_TangentMode);
                            }
                            Handles.color = Color.blue;
                            Handles.DrawLine(m_Points[index].position, m_Points[index].tangentIn);
                        }

                        if (m_currentHandle.tangent == BezierTangentDirection.Out && (m_CloseLoop || index < count - 1))
                        {
                            EditorGUI.BeginChangeCheck();
                            point.tangentOut = Handles.PositionHandle(point.tangentOut, Quaternion.identity);
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (!m_IsMoving)
                                    OnBeginVertexModification();

                                point.tangentOut = EditorSnapping.MoveSnap(point.tangentOut);
                                point.EnforceTangentMode(BezierTangentDirection.Out, m_TangentMode);
                            }
                            Handles.color = Color.red;
                            Handles.DrawLine(m_Points[index].position, m_Points[index].tangentOut);
                        }
                    }

                    m_Points[index] = point;
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
                BezierPoint point = m_Points[index];

                // Position Handle
                float size = HandleUtility.GetHandleSize(point.position) * k_HandleSize;
                Handles.color = bezierPositionHandleColor;

                if (m_currentHandle == index && !m_currentHandle.isTangent)
                {
                    Handles.DotHandleCap(0, point.position, Quaternion.identity, size, e.type);
                }
                else
                {
                    prev = point.position;
                    prev = FreeMoveHandle(prev, size, Vector3.zero, Handles.DotHandleCap);
                    if (!eventHasBeenUsed && eventType == EventType.MouseUp && e.type == EventType.Used)
                    {
                        eventHasBeenUsed = true;
                        m_currentHandle = (BezierHandle)index;
                        Repaint();
                        SceneView.RepaintAll();
                    }
                    else if (!Math.Approx3(prev, point.position))
                    {
                        if (!m_IsMoving)
                            OnBeginVertexModification();

                        point.SetPosition(EditorSnapping.MoveSnap(prev));
                    }
                }

                // Tangent handles
                Handles.color = bezierTangentHandleColor;

                // Tangent In Handle
                if (m_CloseLoop || index > 0)
                {
                    size = HandleUtility.GetHandleSize(point.tangentIn) * k_HandleSize;
                    Handles.DrawLine(point.position, point.tangentIn);

                    if (index == m_currentHandle && m_currentHandle.isTangent && m_currentHandle.tangent == BezierTangentDirection.In)
                    {
                        Handles.DotHandleCap(0, point.tangentIn, Quaternion.identity, size, e.type);
                    }
                    else
                    {
                        prev = point.tangentIn;
                        prev = FreeMoveHandle(prev, size, Vector3.zero, Handles.DotHandleCap);

                        if (!eventHasBeenUsed && eventType == EventType.MouseUp && e.type == EventType.Used)
                        {
                            eventHasBeenUsed = true;
                            m_currentHandle.SetIndexAndTangent(index, BezierTangentDirection.In);
                            Repaint();
                            SceneView.RepaintAll();
                        }
                        else if (!Math.Approx3(prev, point.tangentIn))
                        {
                            if (!m_IsMoving)
                                OnBeginVertexModification();
                            point.tangentIn = EditorSnapping.MoveSnap(prev);
                            point.EnforceTangentMode(BezierTangentDirection.In, m_TangentMode);
                        }
                    }
                }

                // Tangent Out
                if (m_CloseLoop || index < count - 1)
                {
                    size = HandleUtility.GetHandleSize(point.tangentOut) * k_HandleSize;
                    Handles.DrawLine(point.position, point.tangentOut);

                    if (index == m_currentHandle && m_currentHandle.isTangent && m_currentHandle.tangent == BezierTangentDirection.Out)
                    {
                        Handles.DotHandleCap(0, point.tangentOut, Quaternion.identity, size, e.type);
                    }
                    else
                    {
                        prev = point.tangentOut;
                        prev = FreeMoveHandle(prev, size, Vector3.zero, Handles.DotHandleCap);

                        if (!eventHasBeenUsed && eventType == EventType.MouseUp && e.type == EventType.Used)
                        {
                            eventHasBeenUsed = true;
                            m_currentHandle.SetIndexAndTangent(index, BezierTangentDirection.Out);
                            Repaint();
                            SceneView.RepaintAll();
                        }
                        else if (!Math.Approx3(prev, point.tangentOut))
                        {
                            if (!m_IsMoving)
                                OnBeginVertexModification();
                            point.tangentOut = EditorSnapping.MoveSnap(prev);
                            point.EnforceTangentMode(BezierTangentDirection.Out, m_TangentMode);
                        }
                    }
                }

                m_Points[index] = point;
            }

            // Do control point insertion
            if (!eventHasBeenUsed && m_ControlPoints != null && m_ControlPoints.Count > 1)
            {
                int index = -1;
                float distanceToLine;

                Vector3 p = EditorHandleUtility.ClosestPointToPolyLine(m_ControlPoints, out index, out distanceToLine, false, null);

                if (!IsHoveringHandlePoint(e.mousePosition) && distanceToLine < PreferenceKeys.k_MaxPointDistanceFromControl)
                {
                    Handles.color = Color.green;
                    Handles.DotHandleCap(-1, p, Quaternion.identity, HandleUtility.GetHandleSize(p) * .05f, e.type);
                    Handles.color = Color.white;

                    if (!eventHasBeenUsed && eventType == EventType.MouseDown && e.button == 0)
                    {
                        UndoUtility.RecordObject(m_Target, "Add Point");
                        Vector3 dir = m_ControlPoints[(index + 1) % m_ControlPoints.Count] - m_ControlPoints[index];
                        m_Points.Insert((index / m_Columns) + 1, new BezierPoint(p, p - dir, p + dir, Quaternion.identity));
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
            return Handles.FreeMoveHandle(position, size, snap, capFunction);
        }

        bool IsHoveringHandlePoint(Vector2 mpos)
        {
            if (m_Target == null)
                return false;

            int count = m_Points.Count;

            for (int i = 0; i < count; i++)
            {
                BezierPoint p = m_Points[i];

                bool ti = m_CloseLoop || i > 0;
                bool to = m_CloseLoop || i < (count - 1);

                if (Vector2.Distance(mpos, HandleUtility.WorldToGUIPoint(p.position)) < PreferenceKeys.k_MaxPointDistanceFromControl ||
                    (ti && Vector2.Distance(mpos, HandleUtility.WorldToGUIPoint(p.tangentIn)) < PreferenceKeys.k_MaxPointDistanceFromControl) ||
                    (to && Vector2.Distance(mpos, HandleUtility.WorldToGUIPoint(p.tangentOut)) < PreferenceKeys.k_MaxPointDistanceFromControl))
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

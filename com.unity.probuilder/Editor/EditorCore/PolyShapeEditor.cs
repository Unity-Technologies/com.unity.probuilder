using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using System.Collections.Generic;
using UnityEngine.ProBuilder;
using MeshTopology = UnityEngine.MeshTopology;

namespace UnityEditor.ProBuilder
{
    [CustomEditor(typeof(PolyShape))]
    sealed class PolyShapeEditor : Editor
    {
        static Color k_HandleColor = new Color(.8f, .8f, .8f, 1f);
        static Color k_HandleColorGreen = new Color(.01f, .9f, .3f, 1f);
        static Color k_HandleSelectedColor = new Color(.01f, .8f, .98f, 1f);

        const float k_HandleSize = .05f;

        Material m_LineMaterial;
        Mesh m_LineMesh = null;

        Plane m_Plane = new Plane(Vector3.up, Vector3.zero);

        bool m_PlacingPoint = false;
        int m_SelectedIndex = -2;
        float m_DistanceFromHeightHandle;
        static float s_HeightMouseOffset;
        bool m_NextMouseUpAdvancesMode = false;
        bool m_IsModifyingVertices = false;

        PolyShape polygon
        {
            get { return target as PolyShape; }
        }

        Material CreateHighlightLineMaterial()
        {
            Material mat = new Material(Shader.Find("Hidden/ProBuilder/ScrollHighlight"));
            mat.SetColor("_Highlight", new Color(0f, 200f / 255f, 170f / 200f, 1f));
            mat.SetColor("_Base", new Color(0f, 136f / 255f, 1f, 1f));
            return mat;
        }

        void OnEnable()
        {
            if (polygon == null)
            {
                DestroyImmediate(this);
                return;
            }

            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;
            m_LineMesh = new Mesh();
            m_LineMaterial = CreateHighlightLineMaterial();
            Undo.undoRedoPerformed += UndoRedoPerformed;
            DrawPolyLine(polygon.m_Points);
            EditorApplication.update += Update;

            PolyShape.PolyEditMode mode = polygon.polyEditMode;
            polygon.polyEditMode = PolyShape.PolyEditMode.None;
            SetPolyEditMode(mode);
        }

        void OnDisable()
        {
            // Quit Edit mode when the object gets de-selected.
            if (polygon != null && polygon.polyEditMode == PolyShape.PolyEditMode.Edit)
                SetPolyEditMode(PolyShape.PolyEditMode.None);

            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
            DestroyImmediate(m_LineMesh);
            DestroyImmediate(m_LineMaterial);
            EditorApplication.update -= Update;
            Undo.undoRedoPerformed -= UndoRedoPerformed;

            // Delete the created Polyshape if path is empty.
            if (polygon != null && polygon.polyEditMode == PolyShape.PolyEditMode.Path && polygon.m_Points.Count == 0)
                DiscardIncompleteShape();
        }

        void DiscardIncompleteShape()
        {
            if(polygon != null && polygon.gameObject != null)
                Undo.DestroyObjectImmediate(polygon.gameObject);
        }

        public override void OnInspectorGUI()
        {
            switch (polygon.polyEditMode)
            {
                case PolyShape.PolyEditMode.None:
                {
                    if (GUILayout.Button("Edit Poly Shape"))
                        SetPolyEditMode(PolyShape.PolyEditMode.Edit);

                    EditorGUILayout.HelpBox(
                        "Editing a poly shape will erase any modifications made to the mesh!\n\nIf you accidentally enter Edit Mode you can Undo to get your changes back.",
                        MessageType.Warning);

                    break;
                }

                case PolyShape.PolyEditMode.Path:
                {
                    EditorGUILayout.HelpBox("\nClick To Add Points\n\nPress 'Enter' or 'Space' to Set Height\n", MessageType.Info);
                    break;
                }

                case PolyShape.PolyEditMode.Height:
                {
                    EditorGUILayout.HelpBox("\nMove Mouse to Set Height\n\nPress 'Enter' or 'Space' to Finalize\n", MessageType.Info);
                    break;
                }

                case PolyShape.PolyEditMode.Edit:
                {
                    if (GUILayout.Button("Editing Poly Shape", UI.EditorGUIUtility.GetActiveStyle("Button")))
                        SetPolyEditMode(PolyShape.PolyEditMode.None);
                    break;
                }
            }

            EditorGUI.BeginChangeCheck();

            float extrude = polygon.extrude;
            extrude = EditorGUILayout.FloatField("Extrusion", extrude);

            bool flipNormals = polygon.flipNormals;
            flipNormals = EditorGUILayout.Toggle("Flip Normals", flipNormals);

            if (EditorGUI.EndChangeCheck())
            {
                if (polygon.polyEditMode == PolyShape.PolyEditMode.None)
                {
                    if (ProBuilderEditor.instance != null)
                        ProBuilderEditor.instance.ClearElementSelection();

                    UndoUtility.RecordObject(polygon, "Change Polygon Shape Settings");
                    UndoUtility.RecordObject(polygon.mesh, "Change Polygon Shape Settings");
                }
                else
                {
                    UndoUtility.RecordObject(polygon, "Change Polygon Shape Settings");
                }

                polygon.extrude = extrude;
                polygon.flipNormals = flipNormals;

                RebuildPolyShapeMesh(polygon);
            }

            // GUILayout.Label("selected : " + m_SelectedIndex);
        }

        void Update()
        {
            if (polygon != null && polygon.polyEditMode == PolyShape.PolyEditMode.Path && m_LineMaterial != null)
                m_LineMaterial.SetFloat("_EditorTime", (float)EditorApplication.timeSinceStartup);
        }

        void SetPolyEditMode(PolyShape.PolyEditMode mode)
        {
            PolyShape.PolyEditMode old = polygon.polyEditMode;

            if (mode != old)
            {
                // Clear the control always
                GUIUtility.hotControl = 0;

                // Entering edit mode after the shape has been finalized once before, which means
                // possibly reverting manual changes.  Store undo state so that if this was
                // not intentional user can revert.
                if (polygon.polyEditMode == PolyShape.PolyEditMode.None && polygon.m_Points.Count > 2)
                {
                    if (ProBuilderEditor.instance != null)
                        ProBuilderEditor.instance.ClearElementSelection();

                    UndoUtility.RecordObject(polygon, "Edit Polygon Shape");
                    UndoUtility.RecordObject(polygon.mesh, "Edit Polygon Shape");
                }

                polygon.polyEditMode = mode;

                if (polygon.polyEditMode == PolyShape.PolyEditMode.None)
                    ProBuilderEditor.selectMode = ProBuilderEditor.selectMode & ~(SelectMode.InputTool);
                else
                    ProBuilderEditor.selectMode = ProBuilderEditor.selectMode | SelectMode.InputTool;

                if (polygon.polyEditMode != PolyShape.PolyEditMode.None)
                    Tools.current = Tool.None;

                // If coming from Path -> Height set the mouse / origin offset
                if (old == PolyShape.PolyEditMode.Path && mode == PolyShape.PolyEditMode.Height && Event.current != null)
                {
                    Vector3 up = polygon.transform.up;
                    Vector3 origin = polygon.transform.TransformPoint(Math.Average(polygon.m_Points));
                    Ray r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    Vector3 p = Math.GetNearestPointRayRay(origin, up, r.origin, r.direction);
                    s_HeightMouseOffset = polygon.extrude -
                        ProGridsInterface.ProGridsSnap(
                            Vector3.Distance(origin, p) * Mathf.Sign(Vector3.Dot(p - origin, up)));
                }

                RebuildPolyShapeMesh(polygon);
            }
        }

        void RebuildPolyShapeMesh(bool vertexCountChanged = false)
        {
            // If Undo is called immediately after creation this situation can occur
            if (polygon == null)
                return;

            DrawPolyLine(polygon.m_Points);

            polygon.CreateShapeFromPolygon();

            // While the vertex count may not change, the triangle winding might. So unfortunately we can't take
            // advantage of the `vertexCountChanged = false` optimization here.
            ProBuilderEditor.Refresh();
        }

        void OnSceneGUI()
        {
            if (polygon.polyEditMode == PolyShape.PolyEditMode.None)
                return;

            if (polygon == null || Tools.current != Tool.None)
            {
                SetPolyEditMode(PolyShape.PolyEditMode.None);
                return;
            }

            if (m_LineMaterial != null)
            {
                m_LineMaterial.SetPass(0);
                Graphics.DrawMeshNow(m_LineMesh, polygon.transform.localToWorldMatrix, 0);
            }

            Event evt = Event.current;

            // used when finishing a loop by clicking the first created point
            if (m_NextMouseUpAdvancesMode && evt.type == EventType.MouseUp)
            {
                evt.Use();

                m_NextMouseUpAdvancesMode = false;

                if (SceneCameraIsAlignedWithPolyUp())
                    SetPolyEditMode(PolyShape.PolyEditMode.Edit);
                else
                    SetPolyEditMode(PolyShape.PolyEditMode.Height);
            }

            if (m_IsModifyingVertices && (
                    evt.type == EventType.MouseUp ||
                    evt.type == EventType.Ignore ||
                    evt.type == EventType.KeyDown ||
                    evt.type == EventType.KeyUp))
            {
                OnFinishVertexMovement();
            }

            DoExistingPointsGUI();

            if (evt.type == EventType.KeyDown)
                HandleKeyEvent(evt);

            if (EditorHandleUtility.SceneViewInUse(evt))
                return;

            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);

            DoPointPlacement();
        }

        void DoPointPlacement()
        {
            Event evt = Event.current;
            EventType eventType = evt.type;

            if (m_PlacingPoint)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);

                if (eventType == EventType.MouseDrag)
                {
                    float hitDistance = Mathf.Infinity;

                    if (m_Plane.Raycast(ray, out hitDistance))
                    {
                        evt.Use();

                        polygon.m_Points[m_SelectedIndex] = ProGridsInterface.ProGridsSnap(polygon.transform.InverseTransformPoint(ray.GetPoint(hitDistance)), Vector3.one);
                        RebuildPolyShapeMesh(false);
                        SceneView.RepaintAll();
                    }
                }

                if (eventType == EventType.MouseUp ||
                    eventType == EventType.Ignore ||
                    eventType == EventType.KeyDown ||
                    eventType == EventType.KeyUp)
                {
                    evt.Use();
                    m_PlacingPoint = false;
                    m_SelectedIndex = -1;
                    SceneView.RepaintAll();
                }
            }
            else if (polygon.polyEditMode == PolyShape.PolyEditMode.Path)
            {
                if (eventType == EventType.MouseDown)
                {
                    if (polygon.m_Points.Count < 1)
                        SetupInputPlane(evt.mousePosition);

                    float hitDistance = Mathf.Infinity;

                    Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);

                    if (m_Plane.Raycast(ray, out hitDistance))
                    {
                        UndoUtility.RecordObject(polygon, "Add Polygon Shape Point");

                        Vector3 hit = ray.GetPoint(hitDistance);

                        if (polygon.m_Points.Count < 1)
                        {
                            polygon.transform.position = polygon.isOnGrid ? ProGridsInterface.ProGridsSnap(hit) : hit;
                            polygon.transform.rotation = Quaternion.LookRotation(m_Plane.normal) * Quaternion.Euler(new Vector3(90f, 0f, 0f));
                        }

                        Vector3 point = ProGridsInterface.ProGridsSnap(polygon.transform.InverseTransformPoint(hit), Vector3.one);

                        if (polygon.m_Points.Count > 2 && Math.Approx3(polygon.m_Points[0], point))
                        {
                            m_NextMouseUpAdvancesMode = true;
                            return;
                        }

                        polygon.m_Points.Add(point);

                        m_PlacingPoint = true;
                        m_SelectedIndex = polygon.m_Points.Count - 1;
                        RebuildPolyShapeMesh(polygon);

                        evt.Use();
                    }
                }
            }
            else if (polygon.polyEditMode == PolyShape.PolyEditMode.Edit)
            {
                if (polygon.m_Points.Count < 3)
                {
                    SetPolyEditMode(PolyShape.PolyEditMode.Path);
                    return;
                }

                if (m_DistanceFromHeightHandle > PreferenceKeys.k_MaxPointDistanceFromControl)
                {
                    // point insertion
                    int index;
                    float distanceToLine;

                    Vector3 p = EditorHandleUtility.ClosestPointToPolyLine(polygon.m_Points, out index, out distanceToLine, true, polygon.transform);
                    Vector3 wp = polygon.transform.TransformPoint(p);

                    Vector2 ga = HandleUtility.WorldToGUIPoint(polygon.transform.TransformPoint(polygon.m_Points[index % polygon.m_Points.Count]));
                    Vector2 gb = HandleUtility.WorldToGUIPoint(polygon.transform.TransformPoint(polygon.m_Points[(index - 1)]));

                    Vector2 mouse = evt.mousePosition;

                    float distanceToVertex = Mathf.Min(Vector2.Distance(mouse, ga), Vector2.Distance(mouse, gb));

                    if (distanceToVertex > PreferenceKeys.k_MaxPointDistanceFromControl && distanceToLine < PreferenceKeys.k_MaxPointDistanceFromControl)
                    {
                        Handles.color = Color.green;

                        Handles.DotHandleCap(-1, wp, Quaternion.identity, HandleUtility.GetHandleSize(wp) * k_HandleSize, evt.type);

                        if (evt.type == EventType.MouseDown)
                        {
                            evt.Use();

                            UndoUtility.RecordObject(polygon, "Insert Point");
                            polygon.m_Points.Insert(index, p);
                            m_SelectedIndex = index;
                            m_PlacingPoint = true;
                            RebuildPolyShapeMesh(true);
                            OnBeginVertexMovement();
                        }

                        Handles.color = Color.white;
                    }
                }
            }
        }

        void SetupInputPlane(Vector2 mousePosition)
        {
            m_Plane = EditorHandleUtility.FindBestPlane(mousePosition);

            if (ProGridsInterface.SnapEnabled())
            {
                m_Plane.SetNormalAndPosition(
                    m_Plane.normal,
                    ProGridsInterface.ProGridsSnap(m_Plane.normal * -m_Plane.distance));
            }

            var planeNormal = m_Plane.normal;
            var planeCenter = m_Plane.normal * -m_Plane.distance;

            // if hit point on plane is cardinal axis and on grid, snap to grid.
            if (Math.IsCardinalAxis(planeNormal))
            {
                const float epsilon = .00001f;
                float snapVal = Mathf.Abs(ProGridsInterface.SnapValue());
                float rem = Mathf.Abs(snapVal - (Vector3.Scale(planeNormal, planeCenter).magnitude % snapVal));
                polygon.isOnGrid = (rem < epsilon || Mathf.Abs(snapVal - rem) < epsilon);
            }
            else
            {
                polygon.isOnGrid = false;
            }
        }

        void DoExistingPointsGUI()
        {
            Transform trs = polygon.transform;
            int len = polygon.m_Points.Count;

            Vector3 up = polygon.transform.up;
            Vector3 right = polygon.transform.right;
            Vector3 forward = polygon.transform.forward;
            Vector3 center = Vector3.zero;

            Event evt = Event.current;

            bool used = evt.type == EventType.Used;

            if (!used &&
                (evt.type == EventType.MouseDown &&
                 evt.button == 0 &&
                 !EditorHandleUtility.IsAppendModifier(evt.modifiers)))
            {
                m_SelectedIndex = -1;
                Repaint();
            }

            if (polygon.polyEditMode == PolyShape.PolyEditMode.Height)
            {
                if (!used && evt.type == EventType.MouseUp && evt.button == 0 && !EditorHandleUtility.IsAppendModifier(evt.modifiers))
                {
                    evt.Use();
                    SetPolyEditMode(PolyShape.PolyEditMode.Edit);
                }

                bool sceneInUse = EditorHandleUtility.SceneViewInUse(evt);
                Ray r = HandleUtility.GUIPointToWorldRay(evt.mousePosition);

                Vector3 origin = polygon.transform.TransformPoint(Math.Average(polygon.m_Points));
                float extrude = polygon.extrude;

                if (evt.type == EventType.MouseMove && !sceneInUse)
                {
                    Vector3 p = Math.GetNearestPointRayRay(origin, up, r.origin, r.direction);
                    extrude = ProGridsInterface.ProGridsSnap(s_HeightMouseOffset + Vector3.Distance(origin, p) * Mathf.Sign(Vector3.Dot(p - origin, up)));
                }

                Vector3 extrudePoint = origin + (extrude * up);

                Handles.color = k_HandleColor;
                Handles.DotHandleCap(-1, origin, Quaternion.identity, HandleUtility.GetHandleSize(origin) * k_HandleSize, evt.type);
                Handles.color = k_HandleColorGreen;
                Handles.DrawLine(origin, extrudePoint);
                Handles.DotHandleCap(-1, extrudePoint, Quaternion.identity, HandleUtility.GetHandleSize(extrudePoint) * k_HandleSize, evt.type);
                Handles.color = Color.white;

                if (!sceneInUse && polygon.extrude != extrude)
                {
                    OnBeginVertexMovement();
                    polygon.extrude = extrude;
                    RebuildPolyShapeMesh(false);
                }
            }
            else
            {
                // vertex dots
                for (int ii = 0; ii < len; ii++)
                {
                    Vector3 point = trs.TransformPoint(polygon.m_Points[ii]);

                    center.x += point.x;
                    center.y += point.y;
                    center.z += point.z;

                    float size = HandleUtility.GetHandleSize(point) * k_HandleSize;

                    Handles.color = ii == m_SelectedIndex ? k_HandleSelectedColor : k_HandleColor;

                    EditorGUI.BeginChangeCheck();

                    point = Handles.Slider2D(point, up, right, forward, size, Handles.DotHandleCap, Vector2.zero, true);

                    if (EditorGUI.EndChangeCheck())
                    {
                        UndoUtility.RecordObject(polygon, "Move Polygon Shape Point");

                        Vector3 snapMask = Snapping.GetSnappingMaskBasedOnNormalVector(m_Plane.normal);
                        polygon.m_Points[ii] = ProGridsInterface.ProGridsSnap(trs.InverseTransformPoint(point), snapMask);
                        OnBeginVertexMovement();
                        RebuildPolyShapeMesh(false);
                    }

                    // "clicked" a button
                    if (!used && evt.type == EventType.Used)
                    {
                        if (ii == 0 && polygon.m_Points.Count > 2 && polygon.polyEditMode == PolyShape.PolyEditMode.Path)
                        {
                            m_NextMouseUpAdvancesMode = true;
                            return;
                        }
                        else
                        {
                            used = true;
                            m_SelectedIndex = ii;
                        }
                    }
                }

                Handles.color = Color.white;

                // height setting
                if (polygon.polyEditMode != PolyShape.PolyEditMode.Path && polygon.m_Points.Count > 2)
                {
                    center.x /= (float)len;
                    center.y /= (float)len;
                    center.z /= (float)len;

                    Vector3 extrude = center + (up * polygon.extrude);
                    m_DistanceFromHeightHandle = Vector2.Distance(HandleUtility.WorldToGUIPoint(extrude), evt.mousePosition);

                    EditorGUI.BeginChangeCheck();

                    Handles.color = k_HandleColor;
                    Handles.DotHandleCap(-1, center, Quaternion.identity, HandleUtility.GetHandleSize(center) * k_HandleSize, evt.type);
                    Handles.DrawLine(center, extrude);
                    Handles.color = k_HandleColorGreen;
                    extrude = Handles.Slider(extrude, up, HandleUtility.GetHandleSize(extrude) * k_HandleSize, Handles.DotHandleCap, 0f);
                    Handles.color = Color.white;

                    if (EditorGUI.EndChangeCheck())
                    {
                        UndoUtility.RecordObject(polygon, "Set Polygon Shape Height");
                        polygon.extrude = ProGridsInterface.ProGridsSnap(Vector3.Distance(extrude, center) * Mathf.Sign(Vector3.Dot(up, extrude - center)));
                        OnBeginVertexMovement();
                        RebuildPolyShapeMesh(false);
                    }
                }
            }
        }

        void HandleKeyEvent(Event evt)
        {
            KeyCode key = evt.keyCode;

            switch (key)
            {
                case KeyCode.Space:
                case KeyCode.Return:
                {
                    if (polygon.polyEditMode == PolyShape.PolyEditMode.Path)
                    {
                        if (SceneCameraIsAlignedWithPolyUp())
                            SetPolyEditMode(PolyShape.PolyEditMode.Edit);
                        else
                            SetPolyEditMode(PolyShape.PolyEditMode.Height);

                        evt.Use();
                    }
                    else if (polygon.polyEditMode == PolyShape.PolyEditMode.Height)
                    {
                        SetPolyEditMode(PolyShape.PolyEditMode.Edit);
                        evt.Use();
                    }
                    else if (polygon.polyEditMode == PolyShape.PolyEditMode.Edit)
                    {
                        SetPolyEditMode(PolyShape.PolyEditMode.None);
                        evt.Use();
                    }

                    break;
                }

                case KeyCode.Backspace:
                {
                    if (m_SelectedIndex > -1)
                    {
                        UndoUtility.RecordObject(polygon, "Delete Selected Points");
                        polygon.m_Points.RemoveAt(m_SelectedIndex);
                        m_SelectedIndex = -1;
                        RebuildPolyShapeMesh(polygon);
                        evt.Use();
                    }
                    break;
                }

                case KeyCode.Escape:
                {
                    if (polygon.polyEditMode == PolyShape.PolyEditMode.Path ||
                        polygon.polyEditMode == PolyShape.PolyEditMode.Height)
                    {
                        DiscardIncompleteShape();
                        evt.Use();
                    }
                    else if (polygon.polyEditMode == PolyShape.PolyEditMode.Edit)
                    {
                        SetPolyEditMode(PolyShape.PolyEditMode.None);
                        evt.Use();
                    }

                    break;
                }
            }
        }

        void DrawPolyLine(List<Vector3> points)
        {
            if (points.Count < 2)
                return;

            int vc = polygon.polyEditMode == PolyShape.PolyEditMode.Path ? points.Count : points.Count + 1;

            Vector3[] ver = new Vector3[vc];
            Vector2[] uvs = new Vector2[vc];
            int[] indexes = new int[vc];
            int cnt = points.Count;
            float distance = 0f;

            for (int i = 0; i < vc; i++)
            {
                Vector3 a = points[i % cnt];
                Vector3 b = points[i < 1 ? 0 : i - 1];

                float d = Vector3.Distance(a, b);
                distance += d;

                ver[i] = points[i % cnt];
                uvs[i] = new Vector2(distance, 1f);
                indexes[i] = i;
            }

            m_LineMesh.Clear();
            m_LineMesh.name = "Poly Shape Guide";
            m_LineMesh.vertices = ver;
            m_LineMesh.uv = uvs;
            m_LineMesh.SetIndices(indexes, MeshTopology.LineStrip, 0);
            m_LineMaterial.SetFloat("_LineDistance", distance);
        }

        /**
         *  Is the scene camera looking directly at the up vector of the current polygon?
         *  Prevents a situation where the height tool is rendered useless by coplanar
         *  ray tracking.
         */
        bool SceneCameraIsAlignedWithPolyUp()
        {
            float dot = Vector3.Dot(SceneView.lastActiveSceneView.camera.transform.forward, polygon.transform.up);
            return Mathf.Abs(Mathf.Abs(dot) - 1f) < .01f;
        }

        void OnSelectModeChanged(SelectMode selectMode)
        {
            // User changed select mode manually, remove InputTool flag
            if (polygon != null
                && polygon.polyEditMode != PolyShape.PolyEditMode.None
                && !selectMode.ContainsFlag(SelectMode.InputTool))
            {
                SetPolyEditMode(PolyShape.PolyEditMode.None);
            }
        }

        void OnBeginVertexMovement()
        {
            if (!m_IsModifyingVertices)
                m_IsModifyingVertices = true;
        }

        void OnFinishVertexMovement()
        {
            m_IsModifyingVertices = false;
            RebuildPolyShapeMesh(polygon);
        }

        void UndoRedoPerformed()
        {
            if (m_LineMesh != null)
                DestroyImmediate(m_LineMesh);

            if (m_LineMaterial != null)
                DestroyImmediate(m_LineMaterial);

            m_LineMesh = new Mesh();
            m_LineMaterial = CreateHighlightLineMaterial();

            if (polygon.polyEditMode != PolyShape.PolyEditMode.None)
                RebuildPolyShapeMesh(polygon);
        }
    }
}

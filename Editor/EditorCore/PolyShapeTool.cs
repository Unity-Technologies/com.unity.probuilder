using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.EditorTools;
using UnityEditor.ProBuilder.UI;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Math = UnityEngine.ProBuilder.Math;
using UObject = UnityEngine.Object;

#if !UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.EditorTools;
#else
using ToolManager = UnityEditor.EditorTools.ToolManager;
#endif


namespace UnityEditor.ProBuilder
{
    public class PolyShapeTool : EditorTool
    {
        static Color k_HandleColor = new Color(.8f, .8f, .8f, 1f);
        static Color k_HandleColorGreen = new Color(.01f, .9f, .3f, 1f);
        static Color k_HandleSelectedColor = new Color(.01f, .8f, .98f, 1f);

        static Color k_LineMaterialBaseColor = new Color(0f, 136f / 255f, 1f, 1f);
        static Color k_LineMaterialHighlightColor = new Color(0f, 200f / 255f, 170f / 200f, 1f);

        static Color k_InvalidLineMaterialColor = Color.red;

        // Line renderer to provide a preview to the user of the next cut section
        Material m_DrawingLineMaterial;
        Mesh m_DrawingLineMesh = null;
        static readonly Color k_DrawingLineMaterialBaseColor = new Color(0.01f, .9f, 0.3f, 1f);

        const float k_HandleSize = .05f;

        GUIContent m_OverlayTitle;

        Material m_LineMaterial;
        Mesh m_LineMesh = null;

        Plane m_Plane = new Plane(Vector3.up, Vector3.zero);

        int m_ControlId;
        int m_SelectedIndex = -2;
        bool m_IsModifyingVertices = false;
        bool m_NextMouseUpAdvancesMode = false;

        bool m_PlacingPoint = false;
        float m_DistanceFromHeightHandle;

        MouseCursor m_MouseCursor;

        static float s_HeightMouseOffset;
        // should the height change handles be visible?
        bool m_DrawHeightHandles = true;

        Vector3 m_CurrentPosition = Vector3.positiveInfinity;

        PolyShape m_Polygon;
        public PolyShape polygon
        {
            set
            {
                m_Polygon = value;
                DrawPolyLine(m_Polygon.m_Points);

                PolyShape.PolyEditMode mode = m_Polygon.polyEditMode;
                m_Polygon.polyEditMode = PolyShape.PolyEditMode.None;
                SetPolyEditMode(mode);
            }
        }

        /// <summary>
        /// Instantiate Line Materials, all are based on the same base Material with different colors
        /// </summary>
        /// <param name="baseColor">base color to apply to the line</param>
        /// <param name="highlightColor">highlight color to apply to the line</param>
        /// <returns></returns>
        static Material CreateLineMaterial(Color baseColor, Color highlightColor)
        {
            Material mat = new Material(Shader.Find("Hidden/ProBuilder/ScrollHighlight"));
            mat.SetColor("_Base", baseColor);
            mat.SetColor("_Highlight", highlightColor);
            return mat;
        }


        void OnEnable()
        {
            m_OverlayTitle = new GUIContent("Poly Shape Tool");

            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;
            Undo.undoRedoPerformed += UndoRedoPerformed;
            EditorApplication.update += Update;

            InitLineRenderers();
        }

        void OnDisable()
        {
            ClearLineRenderers();

            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
            EditorApplication.update -= Update;
            Undo.undoRedoPerformed -= UndoRedoPerformed;
        }

        /// <summary>
        /// Create line renderers for the current cut
        /// </summary>
        void InitLineRenderers()
        {
            m_LineMesh = new Mesh();
            m_LineMaterial = CreateLineMaterial(k_LineMaterialBaseColor, k_LineMaterialHighlightColor);

            m_DrawingLineMesh = new Mesh();
            m_DrawingLineMaterial = CreateLineMaterial(k_DrawingLineMaterialBaseColor, k_DrawingLineMaterialBaseColor);
        }

        /// <summary>
        /// Clear all line renderers
        /// </summary>
        void ClearLineRenderers()
        {
            if(m_LineMesh)
                DestroyImmediate(m_LineMesh);
            if(m_LineMaterial)
                DestroyImmediate(m_LineMaterial);

            if(m_DrawingLineMesh)
                DestroyImmediate(m_DrawingLineMesh);
            if(m_DrawingLineMaterial)
                DestroyImmediate(m_DrawingLineMaterial);
        }

        /// <summary>
        /// Update method that handles the update of line renderers
        /// </summary>
        void Update()
        {
//            if (m_Polygon != null && m_Polygon.polyEditMode == PolyShape.PolyEditMode.Path && m_LineMaterial != null)
//                m_LineMaterial.SetFloat("_EditorTime", (float)EditorApplication.timeSinceStartup);
        }

        // void OnActiveToolChanged()
        // {
        //     if (!ToolManager.IsActiveTool(this) && m_Polygon!=null)
        //     {
        //         SetPolyEditMode(PolyShape.PolyEditMode.None);
        //     }
        // }

        /// <summary>
        /// Main GUI update for the tool, calls every secondary methods to place points, update lines and compute the cut
        /// </summary>
        /// <param name="window">current window calling the tool : SceneView</param>
        public override void OnToolGUI(EditorWindow window)
        {
            Event evt = Event.current;
            SceneViewOverlay.Window( m_OverlayTitle, OnOverlayGUI, 0, SceneViewOverlay.WindowDisplayOption.OneWindowPerTitle );

            if (m_Polygon.polyEditMode == PolyShape.PolyEditMode.None)
                return;

            if (m_Polygon == null)
            {
                SetPolyEditMode(PolyShape.PolyEditMode.None);
                return;
            }

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

            if (evt.type == EventType.KeyDown)
                HandleKeyEvent(evt);

             if (EditorHandleUtility.SceneViewInUse(evt))
                 return;

            m_ControlId = GUIUtility.GetControlID(FocusType.Passive);

            if(evt.type == EventType.Layout)
                HandleUtility.AddDefaultControl(m_ControlId);

            if(m_Polygon.polyEditMode == PolyShape.PolyEditMode.Path && !m_PlacingPoint)
                m_MouseCursor = MouseCursor.ArrowPlus;
            else if((GUIUtility.hotControl != 0) || m_PlacingPoint)
                m_MouseCursor = MouseCursor.MoveArrow;
            else
                m_MouseCursor = MouseCursor.Arrow;

            DoPointPlacement();
            DoExistingPointsGUI();
            DoExistingLinesGUI();

            if(evt.type == EventType.Repaint)
            {
                Rect sceneViewRect = window.position;
                sceneViewRect.x = 0;
                sceneViewRect.y = 0;
                SceneView.AddCursorRect(sceneViewRect, m_MouseCursor);
            }
        }

        /// <summary>
        /// Overlay GUI
        /// </summary>
        /// <param name="target">the target of this overlay</param>
        /// <param name="view">the current SceneView where to display the overlay</param>
        void OnOverlayGUI(UObject target, SceneView view)
        {
            switch (m_Polygon.polyEditMode)
            {
                case PolyShape.PolyEditMode.Path:
                {
                    EditorGUILayout.HelpBox("Click To Add Points\nPress 'Enter' or 'Space' to Set Height", MessageType.Info);
                    break;
                }

                case PolyShape.PolyEditMode.Height:
                {
                    EditorGUILayout.HelpBox("Move Mouse to Set Height\nPress 'Enter' or 'Space' to Finalize", MessageType.Info);
                    break;
                }

                case PolyShape.PolyEditMode.Edit:
                {
                    if (GUILayout.Button("Quit Editing", UI.EditorGUIUtility.GetActiveStyle("Button")))
                        SetPolyEditMode(PolyShape.PolyEditMode.None);
                    EditorGUILayout.HelpBox("Move Poly Shape points to update the shape\nPress 'Enter' or 'Space' to Finalize", MessageType.Info);
                    break;
                }
            }

            EditorGUI.BeginChangeCheck();

            float extrude = m_Polygon.extrude;
            extrude = EditorGUILayout.FloatField("Extrusion", extrude);

            bool flipNormals = m_Polygon.flipNormals;
            flipNormals = EditorGUILayout.Toggle("Flip Normals", flipNormals);

            if (EditorGUI.EndChangeCheck())
            {
                if (m_Polygon.polyEditMode == PolyShape.PolyEditMode.None)
                {
                    if (ProBuilderEditor.instance != null)
                        ProBuilderEditor.instance.ClearElementSelection();

                    UndoUtility.RecordObject(m_Polygon, "Change Polygon Shape Settings");
                    UndoUtility.RecordObject(m_Polygon.mesh, "Change Polygon Shape Settings");
                }
                else
                {
                    UndoUtility.RecordObject(m_Polygon, "Change Polygon Shape Settings");
                }

                m_Polygon.extrude = extrude;
                m_Polygon.flipNormals = flipNormals;

                RebuildPolyShapeMesh(m_Polygon);
            }
        }

        void SetPolyEditMode(PolyShape.PolyEditMode mode)
        {
            PolyShape.PolyEditMode old = m_Polygon.polyEditMode;

            if (mode != old)
            {
                GUIUtility.hotControl = 0;

                // Entering edit mode after the shape has been finalized once before, which means
                // possibly reverting manual changes.  Store undo state so that if this was
                // not intentional user can revert.
                if (m_Polygon.polyEditMode == PolyShape.PolyEditMode.None && m_Polygon.m_Points.Count > 2)
                {
                    if (ProBuilderEditor.instance != null)
                        ProBuilderEditor.instance.ClearElementSelection();

                    UndoUtility.RecordObject(m_Polygon, "Edit Polygon Shape");
                    UndoUtility.RecordObject(m_Polygon.mesh, "Edit Polygon Shape");
                }

                m_Polygon.polyEditMode = mode;

                // If coming from Path -> Height set the mouse / origin offset
                if (old == PolyShape.PolyEditMode.Path && mode == PolyShape.PolyEditMode.Height && Event.current != null)
                {
                    Vector3 up = m_Polygon.transform.up;
                    Vector3 origin = m_Polygon.transform.TransformPoint(Math.Average(m_Polygon.m_Points));
                    Ray r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                    Vector3 p = Math.GetNearestPointRayRay(origin, up, r.origin, r.direction);
                    s_HeightMouseOffset = m_Polygon.extrude -
                                          ProGridsInterface.ProGridsSnap(
                                              Vector3.Distance(origin, p) * Mathf.Sign(Vector3.Dot(p - origin, up)));
                }

                RebuildPolyShapeMesh(m_Polygon);

                if(mode == PolyShape.PolyEditMode.None)
                {
                    DestroyImmediate(this);
                }
            }
        }

        public void RebuildPolyShapeMesh(bool vertexCountChanged = false)
        {
            // If Undo is called immediately after creation this situation can occur
            if (m_Polygon == null)
                return;

            DrawPolyLine(m_Polygon.m_Points);

            if (m_Polygon.polyEditMode != PolyShape.PolyEditMode.Path)
            {
                var result = m_Polygon.CreateShapeFromPolygon();
                if(result.status == ActionResult.Status.Failure)
                {
                    m_LineMaterial.SetColor("_Highlight", k_InvalidLineMaterialColor);
                    m_LineMaterial.SetColor("_Base", k_InvalidLineMaterialColor);

                    // hide the handle to change the height of the invalid mesh
                    m_DrawHeightHandles = false;

                    // skip height edit mode if the mesh is invalid
                    if(m_Polygon.polyEditMode == PolyShape.PolyEditMode.Height)
                    {
                        SetPolyEditMode(PolyShape.PolyEditMode.Edit);
                    }
                }
                else
                {
                    // make sure everything set to normal if polygon creation succeeded
                    m_LineMaterial.SetColor("_Highlight", k_LineMaterialHighlightColor);
                    m_LineMaterial.SetColor("_Base", k_LineMaterialBaseColor);
                    m_DrawHeightHandles = true;
                }
            }

            // While the vertex count may not change, the triangle winding might. So unfortunately we can't take
            // advantage of the `vertexCountChanged = false` optimization here.
            ProBuilderEditor.Refresh();
        }

        // Returns a local space point,
        Vector3 GetPointInLocalSpace(Vector3 point)
        {
            var trs = m_Polygon.transform;

            if (m_Polygon.isOnGrid)
            {
                Vector3 snapMask = ProBuilderSnapping.GetSnappingMaskBasedOnNormalVector(m_Plane.normal);
                return trs.InverseTransformPoint(ProGridsInterface.ProGridsSnap(point, snapMask));
            }

            return trs.InverseTransformPoint(point);
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
                        m_Polygon.m_Points[m_SelectedIndex] = GetPointInLocalSpace(ray.GetPoint(hitDistance));
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
            else if (m_Polygon.polyEditMode == PolyShape.PolyEditMode.Path)
            {
                if (eventType == EventType.MouseDown && HandleUtility.nearestControl == m_ControlId)
                {
                    if (m_Polygon.m_Points.Count < 1)
                        SetupInputPlane(evt.mousePosition);

                    float hitDistance = Mathf.Infinity;

                    Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);

                    if (m_Plane.Raycast(ray, out hitDistance))
                    {
                        UndoUtility.RecordObject(m_Polygon, "Add Polygon Shape Point");

                        Vector3 hit = ray.GetPoint(hitDistance);

                        if (m_Polygon.m_Points.Count < 1)
                        {
                            m_Polygon.transform.position = m_Polygon.isOnGrid ? ProGridsInterface.ProGridsSnap(hit) : hit;

                            Vector3 cameraFacingPlaneNormal = m_Plane.normal;
                            if (Vector3.Dot(cameraFacingPlaneNormal, SceneView.lastActiveSceneView.camera.transform.forward) > 0f)
                                cameraFacingPlaneNormal *= -1;

                            m_Polygon.transform.rotation = Quaternion.LookRotation(cameraFacingPlaneNormal) * Quaternion.Euler(new Vector3(90f, 0f, 0f));
                        }

                        Vector3 point = GetPointInLocalSpace(hit);

                        if (m_Polygon.m_Points.Count > 2 && Math.Approx3(m_Polygon.m_Points[0], point))
                        {
                            m_NextMouseUpAdvancesMode = true;
                            return;
                        }

                        m_Polygon.m_Points.Add(point);

                        m_PlacingPoint = true;
                        m_SelectedIndex = m_Polygon.m_Points.Count - 1;
                        RebuildPolyShapeMesh(m_Polygon);

                        evt.Use();
                    }
                }
                else
                {
                    float hitDistance = Mathf.Infinity;
                    Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);

                    if(m_Plane.Raycast(ray, out hitDistance))
                    {
                        Vector3 hit = ray.GetPoint(hitDistance);
                        m_CurrentPosition = GetPointInLocalSpace(hit);
                    }
                }
            }
            else if (m_Polygon.polyEditMode == PolyShape.PolyEditMode.Edit)
            {
                if (m_Polygon.m_Points.Count < 3)
                {
                    SetPolyEditMode(PolyShape.PolyEditMode.Path);
                    return;
                }

                if (m_DistanceFromHeightHandle > PreferenceKeys.k_MaxPointDistanceFromControl)
                {
                    // point insertion
                    Vector2 mouse = evt.mousePosition;
                    Ray ray = HandleUtility.GUIPointToWorldRay(mouse);
                    float hitDistance = Mathf.Infinity;
                    if(m_Plane.Raycast(ray, out hitDistance))
                    {
                        Vector3 hit = ray.GetPoint(hitDistance);
                        Vector3 point = GetPointInLocalSpace(hit);

                        int polyCount = m_Polygon.m_Points.Count;

                        float distToLineInGUI;
                        int index;
                        Vector3 pInGUI = EditorHandleUtility.ClosestPointToPolyLine(m_Polygon.m_Points, out index, out distToLineInGUI, true, m_Polygon.transform);

                        Vector3 aToPoint = point - m_Polygon.m_Points[index - 1];
                        Vector3 aToB = m_Polygon.m_Points[index % polyCount] - m_Polygon.m_Points[index - 1];

                        float ratio = Vector3.Dot(aToPoint, aToB.normalized) / aToB.magnitude;
                        Vector3 wp =  Vector3.Lerp(m_Polygon.m_Points[index - 1], m_Polygon.m_Points[index % polyCount], ratio);
                        wp = m_Polygon.transform.TransformPoint(wp);

                        Vector2 aInGUI = HandleUtility.WorldToGUIPoint(m_Polygon.transform.TransformPoint(m_Polygon.m_Points[index - 1]));
                        Vector2 bInGUI = HandleUtility.WorldToGUIPoint(m_Polygon.transform.TransformPoint(m_Polygon.m_Points[index % polyCount]));
                        float distanceToVertex = Mathf.Min(Vector2.Distance(mouse, aInGUI), Vector2.Distance(mouse, bInGUI));

                        if (distanceToVertex > PreferenceKeys.k_MaxPointDistanceFromControl && distToLineInGUI < PreferenceKeys.k_MaxPointDistanceFromControl)
                        {
                            m_MouseCursor = MouseCursor.ArrowPlus;

                            if(evt.type == EventType.Repaint)
                            {
                                Handles.color = Color.green;
                                Handles.DotHandleCap(-1, wp, Quaternion.identity,
                                    HandleUtility.GetHandleSize(wp) * k_HandleSize, evt.type);
                            }

                            if (evt.type == EventType.MouseDown && HandleUtility.nearestControl == m_ControlId)
                            {
                                evt.Use();

                                UndoUtility.RecordObject(m_Polygon, "Insert Point");
                                m_Polygon.m_Points.Insert(index, point);
                                m_SelectedIndex = index;
                                m_PlacingPoint = true;
                                RebuildPolyShapeMesh(true);
                                OnBeginVertexMovement();
                            }

                            Handles.color = Color.white;
                        }

                        if(evt.type != EventType.Repaint)
                            SceneView.RepaintAll();
                    }
                }
            }
        }

        void SetupInputPlane(Vector2 mousePosition)
        {
            m_Plane = EditorHandleUtility.FindBestPlane(mousePosition);

            var planeNormal = m_Plane.normal;
            var planeCenter = m_Plane.normal * -m_Plane.distance;

            // if hit point on plane is cardinal axis and on grid, snap to grid.
            if (Math.IsCardinalAxis(planeNormal))
            {
                const float epsilon = .00001f;
                float snapVal = Mathf.Abs(ProGridsInterface.SnapValue());
                float rem = Mathf.Abs(snapVal - (Vector3.Scale(planeNormal, planeCenter).magnitude % snapVal));
                m_Polygon.isOnGrid = (rem < epsilon || Mathf.Abs(snapVal - rem) < epsilon);
            }
            else
            {
                m_Polygon.isOnGrid = false;
            }
        }

        void DoExistingPointsGUI()
        {
            Transform trs = m_Polygon.transform;
            int len = m_Polygon.m_Points.Count;

            Vector3 up = trs.up;
            Vector3 right = trs.right;
            Vector3 forward = trs.forward;
            Vector3 center = Vector3.zero;

            Event evt = Event.current;

            bool used = evt.type == EventType.Used;

            if (!used &&
                (evt.type == EventType.MouseDown &&
                 evt.button == 0 &&
                 !EditorHandleUtility.IsAppendModifier(evt.modifiers)))
            {
                m_SelectedIndex = -1;
            }

            if (m_Polygon.polyEditMode == PolyShape.PolyEditMode.Height)
            {
                if (!used && evt.type == EventType.MouseUp && evt.button == 0 && !EditorHandleUtility.IsAppendModifier(evt.modifiers))
                {
                    evt.Use();
                    SetPolyEditMode(PolyShape.PolyEditMode.Edit);
                }

                bool sceneInUse = EditorHandleUtility.SceneViewInUse(evt);
                Ray r = HandleUtility.GUIPointToWorldRay(evt.mousePosition);

                Vector3 origin = m_Polygon.transform.TransformPoint(Math.Average(m_Polygon.m_Points));
                float extrude = m_Polygon.extrude;

                if (evt.type == EventType.MouseMove && !sceneInUse)
                {
                    Vector3 p = Math.GetNearestPointRayRay(origin, up, r.origin, r.direction);
                    extrude = ProGridsInterface.ProGridsSnap(s_HeightMouseOffset + Vector3.Distance(origin, p) * Mathf.Sign(Vector3.Dot(p - origin, up)));
                }

                Vector3 extrudePoint = origin + (extrude * up);

                if (m_DrawHeightHandles)
                {
                    Handles.color = k_HandleColor;
                    Handles.DotHandleCap(-1, origin, Quaternion.identity, HandleUtility.GetHandleSize(origin) * k_HandleSize, evt.type);
                    Handles.color = k_HandleColorGreen;
                    Handles.DrawLine(origin, extrudePoint);
                    Handles.DotHandleCap(-1, extrudePoint, Quaternion.identity, HandleUtility.GetHandleSize(extrudePoint) * k_HandleSize, evt.type);
                    Handles.color = Color.white;
                }

                if (!sceneInUse && m_Polygon.extrude != extrude)
                {
                    OnBeginVertexMovement();
                    m_Polygon.extrude = extrude;
                    RebuildPolyShapeMesh(false);
                }
            }
            else
            {
                // vertex dots
                for (int ii = 0; ii < len; ii++)
                {
                    Vector3 point = trs.TransformPoint(m_Polygon.m_Points[ii]);

                    center.x += point.x;
                    center.y += point.y;
                    center.z += point.z;

                    float size = HandleUtility.GetHandleSize(point) * k_HandleSize;

                    Handles.color = ii == m_SelectedIndex ? k_HandleSelectedColor : k_HandleColor;

                    EditorGUI.BeginChangeCheck();

                    point = Handles.Slider2D(point, up, right, forward, size, Handles.DotHandleCap, Vector2.zero, true);

                    if (EditorGUI.EndChangeCheck())
                    {
                        UndoUtility.RecordObject(m_Polygon, "Move Polygon Shape Point");
                        m_Polygon.m_Points[ii] = GetPointInLocalSpace(point);
                        OnBeginVertexMovement();
                        RebuildPolyShapeMesh(false);
                    }

                    // "clicked" a button
                    if (!used && evt.type == EventType.Used)
                    {
                        if (ii == 0 && m_Polygon.m_Points.Count > 2 && m_Polygon.polyEditMode == PolyShape.PolyEditMode.Path)
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
                if (m_Polygon.polyEditMode != PolyShape.PolyEditMode.Path && m_Polygon.m_Points.Count > 2)
                {
                    center.x /= (float)len;
                    center.y /= (float)len;
                    center.z /= (float)len;

                    Vector3 extrude = center + (up * m_Polygon.extrude);
                    m_DistanceFromHeightHandle = Vector2.Distance(HandleUtility.WorldToGUIPoint(extrude), evt.mousePosition);

                    EditorGUI.BeginChangeCheck();

                    if (m_DrawHeightHandles)
                    {
                        Handles.color = k_HandleColor;
                        Handles.DotHandleCap(-1, center, Quaternion.identity, HandleUtility.GetHandleSize(center) * k_HandleSize, evt.type);
                        Handles.DrawLine(center, extrude);
                        Handles.color = k_HandleColorGreen;
                        extrude = Handles.Slider(extrude, up, HandleUtility.GetHandleSize(extrude) * k_HandleSize, Handles.DotHandleCap, 0f);
                        Handles.color = Color.white;
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        UndoUtility.RecordObject(m_Polygon, "Set Polygon Shape Height");
                        m_Polygon.extrude = ProGridsInterface.ProGridsSnap(Vector3.Distance(extrude, center) * Mathf.Sign(Vector3.Dot(up, extrude - center)));
                        OnBeginVertexMovement();
                        RebuildPolyShapeMesh(false);
                    }
                }
            }
        }



        /// <summary>
        /// Display lines of the poly shape
        /// </summary>
        void DoExistingLinesGUI()
        {
            if(m_LineMaterial != null)
            {
                m_LineMaterial.SetPass(0);
                Graphics.DrawMeshNow(m_LineMesh, m_Polygon.transform.localToWorldMatrix, 0);
            }

            if(m_DrawingLineMaterial != null && m_Polygon.polyEditMode == PolyShape.PolyEditMode.Path)
            {
                if(DrawGuideLine())
                {
                    m_DrawingLineMaterial.SetPass(0);
                    Graphics.DrawMeshNow(m_DrawingLineMesh, m_Polygon.transform.localToWorldMatrix, 0);
                }
            }
        }

        void DrawPolyLine(List<Vector3> points)
        {
            if (points.Count < 2)
                return;

            int vc = m_Polygon.polyEditMode == PolyShape.PolyEditMode.Path ? points.Count : points.Count + 1;

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

        void UpdateDashedLine(Mesh lineMesh, Vector3 fromPoint, Vector3 toPoint)
        {
            float lineLength = 0.1f, spaceLength = 0.05f;
            List<Vector3> ver = lineMesh.vertices.ToList();
            List<Vector2> uvs = lineMesh.uv.ToList();

            List<int> indexes = new List<int>();
            lineMesh.GetIndices(indexes,0);

            float d = Vector3.Distance(fromPoint, toPoint);
            Vector3 dir = ( toPoint - fromPoint ).normalized;
            int sections = (int)(d / (lineLength + spaceLength));

            int offset = ver.Count;
            for(int i = 0; i < sections; i++)
            {
                ver.Add(fromPoint + i * (lineLength + spaceLength) * dir);
                ver.Add(fromPoint + (i * (lineLength + spaceLength) + lineLength) * dir);

                uvs.Add(new Vector2( 1f, 1f));
                uvs.Add(new Vector2( 1f, 1f));

                indexes.Add(2*i + offset);
                indexes.Add(2*i+1 + offset);
            }

            ver.Add(fromPoint + sections * (lineLength + spaceLength) * dir);
            uvs.Add(new Vector2( 1f, 1f));
            indexes.Add(2 * sections + offset);


            if(d - (sections * ( lineLength + spaceLength )) > lineLength)
                ver.Add(fromPoint + ( sections * ( lineLength + spaceLength ) + lineLength ) * dir);
            else
                ver.Add(toPoint);
            uvs.Add(new Vector2( 1f, 1f));
            indexes.Add(2 * sections + 1 + offset);

            lineMesh.name = "DashedLine";
            lineMesh.vertices = ver.ToArray();
            lineMesh.uv = uvs.ToArray();
            lineMesh.SetIndices(indexes, MeshTopology.Lines, 0);
        }

        /// <summary>
        /// Draw a helper line between the last point of the cut and the current position of the mouse cursor
        /// </summary>
        /// <returns>true if the line can be traced (the position of the cursor must be valid and the cut have one point minimum)</returns>
        bool DrawGuideLine()
        {
            if(m_DrawingLineMesh)
                m_DrawingLineMesh.Clear();

            if(m_CurrentPosition.Equals(Vector3.positiveInfinity) || m_PlacingPoint)
                return false;

            if(m_Polygon.controlPoints.Count > 0)
            {
                Vector3 lastPosition = m_Polygon.controlPoints[m_Polygon.controlPoints.Count - 1];
                Vector3 currentPosition = m_CurrentPosition;

                UpdateDashedLine(m_DrawingLineMesh, lastPosition, currentPosition);
                m_DrawingLineMaterial.SetFloat("_LineDistance", 1f);
                return true;
            }
            return false;
        }

        void HandleKeyEvent(Event evt)
        {
            KeyCode key = evt.keyCode;

            switch (key)
            {
                case KeyCode.Space:
                case KeyCode.Return:
                {
                    if (m_Polygon.polyEditMode == PolyShape.PolyEditMode.Path)
                    {
                        if (SceneCameraIsAlignedWithPolyUp())
                            SetPolyEditMode(PolyShape.PolyEditMode.Edit);
                        else
                            SetPolyEditMode(PolyShape.PolyEditMode.Height);

                        evt.Use();
                    }
                    else if (m_Polygon.polyEditMode == PolyShape.PolyEditMode.Height)
                    {
                        SetPolyEditMode(PolyShape.PolyEditMode.Edit);
                        evt.Use();
                    }
                    else if (m_Polygon.polyEditMode == PolyShape.PolyEditMode.Edit)
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
                        UndoUtility.RecordObject(m_Polygon, "Delete Selected Points");
                        m_Polygon.m_Points.RemoveAt(m_SelectedIndex);
                        m_SelectedIndex = -1;
                        RebuildPolyShapeMesh(m_Polygon);
                        evt.Use();
                    }
                    break;
                }

                case KeyCode.Escape:
                {
                    m_Polygon.m_Points.Clear();
                    SetPolyEditMode(PolyShape.PolyEditMode.None);
                    evt.Use();
                    break;
                }

            }
        }

        /**
         *  Is the scene camera looking directly at the up vector of the current polygon?
         *  Prevents a situation where the height tool is rendered useless by coplanar
         *  ray tracking.
         */
        bool SceneCameraIsAlignedWithPolyUp()
        {
            float dot = Vector3.Dot(SceneView.lastActiveSceneView.camera.transform.forward, m_Polygon.transform.up);
            return Mathf.Abs(Mathf.Abs(dot) - 1f) < .01f;
        }

        void OnSelectModeChanged(SelectMode selectMode)
        {
            // User changed select mode manually, remove InputTool flag
            if (m_Polygon != null
                && m_Polygon.polyEditMode != PolyShape.PolyEditMode.None
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
            RebuildPolyShapeMesh(m_Polygon);
        }

        void UndoRedoPerformed()
        {
            ClearLineRenderers();
            InitLineRenderers();

            if (m_Polygon.polyEditMode != PolyShape.PolyEditMode.None)
                RebuildPolyShapeMesh(m_Polygon);
        }
    }
}

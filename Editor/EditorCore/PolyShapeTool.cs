using System;
using System.Linq;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

using Math = UnityEngine.ProBuilder.Math;
using Plane = UnityEngine.Plane;
using UObject = UnityEngine.Object;
using ToolManager = UnityEditor.EditorTools.ToolManager;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Represents the [PolyShape tool](../manual/polyshape.html) button on the [ProBuilder toolbar](../manual/toolbar.html) in the Editor.
    /// </summary>
    [EditorTool("Create PolyShape", toolPriority = 1001)]
    [Icon("Packages/com.unity.probuilder/Content/Icons/Toolbar/CreatePolyShape.png")]
    public class DrawPolyShapeTool : PolyShapeTool
    {
        GameObject m_LastPolyShape = null;

        [MenuItem(EditorToolbarMenuItem.k_MenuPrefix + "Editors/Create PolyShape", false, PreferenceKeys.menuEditor + 1)]
        static void MenuPerform_NewShape()
        {
            ToolManager.SetActiveTool<DrawPolyShapeTool>();
            ProBuilderAnalytics.SendActionEvent("New PolyShape", nameof(DrawPolyShapeTool));
        }

        PolyShape m_PolyShape = null;

        static GUIContent s_IconContent;

        /// <inheritdoc />
        public override GUIContent toolbarIcon
        {
            get
            {
                if (s_IconContent == null)
                    s_IconContent = new GUIContent()
                    {
                        image = IconUtility.GetIcon("Toolbar/CreatePolyShape"),
                        text = "Create PolyShape",
                        tooltip = "Create PolyShape"
                    };
                return s_IconContent;
            }
        }

        /// <inheritdoc/>
        public override void OnActivated()
        {
            m_LastPolyShape = null;
            m_IsExiting = false;
            MeshSelection.SetSelection((GameObject)null);
            ToolManager.SetActiveContext<GameObjectToolContext>();
            base.OnActivated();
        }

        /// <inheritdoc/>
        public override void OnWillBeDeactivated()
        {
            base.OnWillBeDeactivated();
            m_PolyShape = null;
            if (m_LastPolyShape)
                Selection.activeObject = m_LastPolyShape;
        }

        /// <inheritdoc/>
        public override void OnToolGUI(EditorWindow window)
        {
            if (m_PolyShape == null || polygon == null)
            {
                if(m_PolyShape)
                    m_LastPolyShape = m_PolyShape.gameObject;

                if (!TryCreatePolyShape())
                {
                    ToolManager.RestorePreviousTool();
                    return;
                }
            }

            var evt = Event.current;
            if (polygon?.polyEditMode == PolyShape.PolyEditMode.Height)
            {
                if (evt.type == EventType.MouseUp && evt.button == 0 && !EditorHandleUtility.IsAppendModifier(evt.modifiers))
                {
                    evt.Use();
                    if (!TryFinalizeShape())
                        return;
                }
            }

            base.OnToolGUI(window);
        }

        internal override void HandleKeyEvent(Event evt)
        {
            KeyCode key = evt.keyCode;

            switch (key)
            {
                case KeyCode.Space:
                case KeyCode.Return:
                {
                    if (polygon.polyEditMode == PolyShape.PolyEditMode.Height
                        || polygon.polyEditMode == PolyShape.PolyEditMode.Edit)
                    {
                        TryFinalizeShape();
                        LeaveTool();
                        evt.Use();
                    }
                    break;
                }

                case KeyCode.Escape:
                {
                    if (polygon != null && polygon.polyEditMode == PolyShape.PolyEditMode.Height)
                    {
                        TryFinalizeShape();
                    }
                    else
                    {
                        EditorApplication.delayCall += () =>
                        {
                            Undo.RevertAllDownToGroup(m_UndoGroup - 1); // cancel everything we were doing this group
                        };
                    }
                    LeaveTool();
                    evt.Use();
                    break;
                }
            }

            if (evt.type != EventType.Used)
                base.HandleKeyEvent(evt);
        }

        bool TryFinalizeShape()
        {
            m_LastPolyShape = polygon.gameObject;

            SetShapeHeight();
            polygon.gameObject.hideFlags = HideFlags.None;
            UndoUtility.RegisterCreatedObjectUndo(polygon.gameObject, "Finalize Draw PolyShape");
            Undo.CollapseUndoOperations(m_UndoGroup);
            polygon = null;
            return false;
        }

        /// <inheritdoc/>
        protected override void OnObjectSelectionChanged()
        {
            if(Selection.activeObject != null && polygon == null)
            {
                ToolManager.RestorePreviousTool();
                return;
            }

            if((Selection.activeObject == null && polygon == null) ||
               (Selection.activeObject is GameObject go && go == polygon.gameObject))
                return;

            if (polygon != null && polygon.polyEditMode == PolyShape.PolyEditMode.Height)
            {
                TryFinalizeShape();
            }
            else if (polygon != null && polygon.polyEditMode == PolyShape.PolyEditMode.Path)
            {
                // NOTE 2025-02-19 At this point the selection has already changed and we want to
                // cancel out of this tool. Unity does not provide a beforeSelectionChanged event
                // which would be proper time to remove and clean up the undo entries from the
                // undo stack. The selection change is already added to the undo stack so we cannot
                // revert it now. This is the next best thing.
                Undo.ClearUndo(polygon.gameObject);
                Undo.ClearUndo(polygon);
                Undo.SetCurrentGroupName(Undo.GetCurrentGroupName());
            }
            LeaveTool();
        }

        bool TryCreatePolyShape()
        {
            MeshSelection.objectSelectionChanged -= OnObjectSelectionChanged;
            var newPolyshape = CanCreateNewPolyShape();
            if (newPolyshape && !m_IsExiting)
            {
                Undo.SetCurrentGroupName("PolyShape Tool");
                //Finalize previous operation in term of undo
                m_UndoGroup = Undo.GetCurrentGroup();

                GameObject go = new GameObject("PolyShape");
                go.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;

                UndoUtility.RegisterCreatedObjectUndo(go, "Create Poly Shape");
                Undo.RegisterCompleteObjectUndo(go, "Adding components");
                m_PolyShape = Undo.AddComponent<PolyShape>(go);
                ProBuilderMesh pb = Undo.AddComponent<ProBuilderMesh>(go);
                pb.CreateShapeFromPolygon(m_PolyShape.m_Points, m_PolyShape.extrude, m_PolyShape.flipNormals);
                EditorUtility.InitObject(pb);

                // Special case - we don't want to reset the grid pivot because we rely on it to set the active plane for
                // interaction, regardless of whether snapping is enabled or not.
                if (ProGridsInterface.SnapEnabled() || ProGridsInterface.GridVisible())
                {
                    Vector3 pivot;
                    if (ProGridsInterface.GetPivot(out pivot))
                        go.transform.position = pivot;
                }

                m_PolyShape.polyEditMode = PolyShape.PolyEditMode.Path;
                UpdateTarget(m_PolyShape);
            }

            MeshSelection.objectSelectionChanged+= OnObjectSelectionChanged;
            return newPolyshape;
        }

        static bool CanCreateNewPolyShape()
        {
            //If inspector is locked we cannot create new PolyShape.
            //First created inspector seems to hold a specific semantic where
            //if not unlocked no matter how many inspectors are present they will
            //not allow the creation of new PolyShape.
            var inspWindows = InspectorWindow.GetInspectors();

            if (inspWindows.Any(x => x.isLocked))
            {
                if (UnityEditor.EditorUtility.DisplayDialog(
                        L10n.Tr("Inspector Locked"),
                        L10n.Tr("To create a new Poly Shape you need access to all Inspectors, which are currently locked. Do you wish to unlock all Inspectors?"),
                        L10n.Tr("Unlock"),
                        L10n.Tr("Cancel")))
                {
                    foreach (var insp in inspWindows)
                        insp.isLocked = false;
                }
                else
                {
                    return false;
                }
            }

            return true;
        }
    }

    /// <summary>
    /// Represents the [PolyShape tool](../manual/polyshape.html) button on the [ProBuilder toolbar](../manual/toolbar.html) in the Editor.
    /// </summary>
    [Icon("Packages/com.unity.probuilder/Content/Icons/Toolbar/CreatePolyShape.png")]
    [EditorTool("Edit PolyShape", typeof(PolyShape))]
    public class PolyShapeTool : EditorTool
    {
        [MenuItem("Tools/ProBuilder/Edit/Edit PolyShape", true, PreferenceKeys.menuEditor + 10)]
        static bool ValidateEditShapeTool()
        {
            foreach (var go in Selection.gameObjects)
                if (go.TryGetComponent<PolyShape>(out _)) return true;

            return false;
        }

        [MenuItem("Tools/ProBuilder/Edit/Edit PolyShape", false, PreferenceKeys.menuEditor + 10)]
        static void ActivateEditShapeTool()
        {
            ToolManager.SetActiveTool<PolyShapeTool>();
            ProBuilderAnalytics.SendActionEvent("Edit PolyShape", nameof(PolyShapeTool));
        }


        static readonly Color k_HandleColor = new Color(.8f, .8f, .8f, 1f);
        static readonly Color k_HandleColorGreen = new Color(.01f, .9f, .3f, 1f);
        static readonly Color k_HandleSelectedColor = new Color(.01f, .8f, .98f, 1f);

        static readonly Color k_LineColor = new Color(0f, 55f / 255f, 1f, 1f);
        static readonly Color k_InvalidLineColor = Color.red;
        static readonly Color k_DrawingLineColor = new Color(0.01f, .9f, 0.3f, 1f);

        Color m_CurrentLineColor = k_LineColor;

        const float k_HandleSize = .05f;

        GUIContent m_OverlayTitle;

        Plane m_Plane = new Plane(Vector3.up, Vector3.zero);

        Plane plane
        {
            set
            {
                m_Plane = value;
            }
            get
            {
                if (polygon.m_Points.Count >= 3 &&
                    m_Plane.distance.Equals(0) &&
                    m_Plane.normal.Equals(Vector3.up))
                {
                    Transform trs = polygon.transform;
                    m_Plane = new Plane(trs.TransformPoint(polygon.m_Points[0]),
                        trs.TransformPoint(polygon.m_Points[1]),
                        trs.TransformPoint(polygon.m_Points[2]));
                }
                else
                    m_Plane = EditorHandleUtility.GetPlaneFromSceneView();

                return m_Plane;
            }
        }

        internal int m_UndoGroup;

        int m_ControlId;
        int m_ControlId0;
        int m_SelectedIndex = -2;
        bool m_IsModifyingVertices = false;
        bool m_NextMouseUpAdvancesMode = false;
        internal bool m_IsExiting = false;

        bool m_PlacingPoint = false;
        float m_DistanceFromHeightHandle;

        MouseCursor m_MouseCursor;

        static float s_HeightMouseOffset;
        // should the height change handles be visible?
        bool m_DrawHeightHandles = true;

        Vector3 m_CurrentPosition = Vector3.positiveInfinity;

        PolyShape m_Polygon;

        /// <summary>
        /// Gets or sets the custom polygon shape.
        /// </summary>
        public PolyShape polygon
        {
            set
            {
                m_Polygon = value;
                if(m_Polygon != null)
                {
                    PolyShape.PolyEditMode mode = m_Polygon.polyEditMode;
                    m_Polygon.polyEditMode = PolyShape.PolyEditMode.None;
                    SetPolyEditMode(mode);
                }
            }

            get
            {
                return m_Polygon;
            }
        }

        static GUIContent s_IconContent;

        /// <inheritdoc />
        public override GUIContent toolbarIcon
        {
            get
            {
                if (s_IconContent == null)
                    s_IconContent = new GUIContent()
                    {
                        image = IconUtility.GetIcon("Toolbar/CreatePolyShape"),
                        text = "Edit PolyShape",
                        tooltip = "Edit PolyShape"
                    };
                return s_IconContent;
            }
        }

        void OnEnable()
        {
            m_OverlayTitle = new GUIContent("PolyShape Settings");
        }

        void OnDisable(){}

        /// <inheritdoc/>
        public override void OnActivated()
        {
            SetPolyEditMode(PolyShape.PolyEditMode.None);

            Undo.SetCurrentGroupName("PolyShape Tool");
            m_UndoGroup = Undo.GetCurrentGroup();

            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;
            MeshSelection.objectSelectionChanged += OnObjectSelectionChanged;
            ToolManager.activeContextChanged += OnActiveContextChanged;
            Undo.undoRedoPerformed += UndoRedoPerformed;

            m_Target = null;
            UpdateTarget();
        }

        /// <inheritdoc/>
        public override void OnWillBeDeactivated()
        {
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
            MeshSelection.objectSelectionChanged -= OnObjectSelectionChanged;
            ToolManager.activeContextChanged -= OnActiveContextChanged;
            Undo.undoRedoPerformed -= UndoRedoPerformed;
            if(polygon != null && polygon.polyEditMode != PolyShape.PolyEditMode.None)
                SetPolyEditMode(PolyShape.PolyEditMode.None);

            Undo.CollapseUndoOperations(m_UndoGroup);
            polygon = null;
        }

        internal void UpdateTarget(PolyShape shape = null)
        {
            if(shape != null)
                m_Target = shape;

            if(target is PolyShape ps)
            {
                polygon = ps;
                SetPolyEditMode(PolyShape.PolyEditMode.Edit);
            }
            else if(target is GameObject go)
            {
                if(go.transform.TryGetComponent<PolyShape>(out var psComp))
                {
                    polygon = psComp;
                    SetPolyEditMode(PolyShape.PolyEditMode.Edit);
                }
            }
        }

        /// <summary>
        /// Exits the tool and restores previous persistent tool.
        /// </summary>
        protected void LeaveTool()
        {
            m_IsExiting = true;
            //Quit Polygon edit mode and deactivate the tool
            SetPolyEditMode(PolyShape.PolyEditMode.None);
            polygon = null;
            ToolManager.RestorePreviousPersistentTool();
        }

        /// <summary>
        /// Implements the PolyShapeTool in the Unity Editor. This method is called when the PolyShapeTool is activated.
        /// </summary>
        /// <param name="window">The window calling the tool (SceneView).</param>
        public override void OnToolGUI(EditorWindow window)
        {
            Event evt = Event.current;

            // todo refactor overlays to use `Overlay` class
#pragma warning disable 618
            SceneViewOverlay.Window( m_OverlayTitle, OnOverlayGUI, 0, SceneViewOverlay.WindowDisplayOption.OneWindowPerTitle );
#pragma warning restore 618

            if (polygon == null)
                return;

            if (polygon.polyEditMode == PolyShape.PolyEditMode.None)
                return;

            // used when finishing a loop by clicking the first created point
            if (m_NextMouseUpAdvancesMode && evt.type == EventType.MouseUp)
            {
                evt.Use();
                m_NextMouseUpAdvancesMode = false;

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

            //The user can press a key to exit editing mode,
            //leading to null polygon at this point
            if (polygon == null)
                return;

            if (EditorHandleUtility.SceneViewInUse(evt))
                 return;

            m_ControlId = GUIUtility.GetControlID(FocusType.Passive);

            if(evt.type == EventType.Layout)
                HandleUtility.AddDefaultControl(m_ControlId);

            if(polygon.polyEditMode == PolyShape.PolyEditMode.Path && HandleUtility.nearestControl == m_ControlId && !m_PlacingPoint)
                m_MouseCursor = MouseCursor.ArrowPlus;
            else if((GUIUtility.hotControl != 0) || m_PlacingPoint)
                m_MouseCursor = MouseCursor.MoveArrow;
            else
                m_MouseCursor = MouseCursor.Arrow;

            if(evt.type == EventType.MouseMove)
                SceneView.RepaintAll();

            DoPointPlacement();
            DoExistingPointsGUI();
            if(evt.type == EventType.Repaint)
            {
                DoExistingLinesGUI();
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
            var currentPolygon = polygon;
            if(currentPolygon == null)
                return;

            EditorGUILayout.BeginVertical(GUILayout.Width(300));

            switch (currentPolygon.polyEditMode)
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
                    if(GUILayout.Button("Quit Editing", UI.EditorGUIUtility.GetActiveStyle("Button")))
                        LeaveTool();
                    EditorGUILayout.HelpBox("Move Poly Shape points to update the shape." +
                        "\nClick on an edge to add new points, press 'Backspace' to remove selected point." +
                        "\nPress 'Enter' or 'Space' to Finalize", MessageType.Info);
                    break;
                }
            }

            EditorGUI.BeginChangeCheck();

            float extrude = currentPolygon.extrude;
            extrude = EditorGUILayout.FloatField("Extrusion", extrude);

            bool flipNormals = currentPolygon.flipNormals;
            flipNormals = EditorGUILayout.Toggle("Flip Normals", flipNormals);

            if (EditorGUI.EndChangeCheck())
            {
                if (currentPolygon.polyEditMode == PolyShape.PolyEditMode.None)
                {
                    if (ProBuilderEditor.instance != null)
                        ProBuilderEditor.instance.ClearElementSelection();

                    UndoUtility.RecordObject(currentPolygon, "Change Polygon Shape Settings");
                    UndoUtility.RecordObject(currentPolygon.mesh, "Change Polygon Shape Settings");
                }
                else
                {
                    UndoUtility.RecordObject(currentPolygon, "Change Polygon Shape Settings");
                }

                currentPolygon.extrude = extrude;
                currentPolygon.flipNormals = flipNormals;

                RebuildPolyShapeMesh(currentPolygon);
            }
            EditorGUILayout.EndVertical();
        }

        internal void SetShapeHeight()
        {
            Undo.RegisterCompleteObjectUndo(polygon, "Set Height");

            RebuildPolyShapeMesh(polygon);
            SetPolyEditMode(PolyShape.PolyEditMode.None);
        }

        internal void SetPolyEditMode(PolyShape.PolyEditMode mode)
        {
            if(polygon == null)
                return;

            PolyShape.PolyEditMode old = polygon.polyEditMode;
            if (mode != old)
            {
                GUIUtility.hotControl = 0;

                // Entering edit mode after the shape has been finalized once before, which means
                // possibly reverting manual changes.  Store undo state so that if this was
                // not intentional user can revert.
                if (old == PolyShape.PolyEditMode.None && polygon.m_Points.Count > 2)
                {
                    if (ProBuilderEditor.instance != null)
                        ProBuilderEditor.instance.ClearElementSelection();

                    Undo.RegisterCompleteObjectUndo(polygon, "Edit Polygon Shape");
                    Undo.RegisterCompleteObjectUndo(polygon.mesh, "Edit Polygon Shape mesh");
                }

                polygon.polyEditMode = mode;

                // If coming from Path -> Height set the mouse / origin offset
                if (old == PolyShape.PolyEditMode.Path && mode == PolyShape.PolyEditMode.Height && Event.current != null)
                {
                    Vector3 up = polygon.transform.up;
                    Vector3 origin = polygon.transform.TransformPoint(Math.Average(polygon.m_Points));

                    if (SceneCameraIsAlignedWithPolyUp())
                    {
                        s_HeightMouseOffset = polygon.extrude;
                        SetShapeHeight();
                        polygon.gameObject.hideFlags = HideFlags.None;
                        Undo.CollapseUndoOperations(m_UndoGroup);
                        polygon = null;
                        return;
                    }
                    else
                    {
                        Ray r = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        Vector3 p = Math.GetNearestPointRayRay(origin, up, r.origin, r.direction);
                        float extrude = Vector3.Distance(origin, p) * Mathf.Sign(Vector3.Dot(p - origin, up));
                        s_HeightMouseOffset = polygon.extrude - EditorSnapping.MoveSnap(extrude);
                    }
                }
                else if(old == PolyShape.PolyEditMode.Path && mode == PolyShape.PolyEditMode.None)
                {
                    var go = polygon.gameObject;
                    EditorApplication.delayCall += () =>
                    {
                        DestroyImmediate(go);
                    };
                    return;
                }

                RebuildPolyShapeMesh(polygon);

                //Dirty the polygon for serialization (fix for transition between prefab and scene mode)
                if(polygon != null)
                    UnityEditor.EditorUtility.SetDirty(polygon);

            }
        }

        /// <summary>
        /// Redraws the PolyShape mesh in the Unity Editor.
        /// </summary>
        /// <param name="vertexCountChanged">This parameter is ignored.</param>
        public void RebuildPolyShapeMesh(bool vertexCountChanged = false)
        {
            // If Undo is called immediately after creation this situation can occur
            if (polygon == null)
                return;

            if (polygon.polyEditMode != PolyShape.PolyEditMode.Path)
            {
                // PBLD-137 - continously refresh the material, otherwise if Resident Drawer is on, new faces are drawn without material applied until there's some other trigger:
                // renderer enable/disable, material re-apply, etc.
                if (polygon.polyEditMode == PolyShape.PolyEditMode.Height && polygon.m_Points != null &&
                    polygon.m_Points.Count >= 3 && polygon.mesh.renderer.sharedMaterial == null)
                {
                    Undo.RegisterCompleteObjectUndo(polygon.mesh.renderer, "Adding Material");
                    polygon.mesh.renderer.sharedMaterial = EditorMaterialUtility.GetUserMaterial();
                }

                var result = polygon.CreateShapeFromPolygon();
                if(result.status == ActionResult.Status.Failure)
                {
                    m_CurrentLineColor = k_InvalidLineColor;
                    // hide the handle to change the height of the invalid mesh
                    m_DrawHeightHandles = false;

                    // skip height edit mode if the mesh is invalid
                    if(polygon.polyEditMode == PolyShape.PolyEditMode.Height)
                        SetPolyEditMode(PolyShape.PolyEditMode.Edit);
                }
                else
                {
                    // make sure everything set to normal if polygon creation succeeded
                    m_CurrentLineColor = k_LineColor;
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
            var trs = polygon.transform;

            if (polygon.isOnGrid)
            {
                Vector3 snapMask = ProBuilderSnapping.GetSnappingMaskBasedOnNormalVector(plane.normal);
                return trs.InverseTransformPoint(ProBuilderSnapping.Snap(point, Vector3.Scale(EditorSnapping.activeMoveSnapValue, snapMask)));
            }

            return trs.InverseTransformPoint(point);
        }

        Vector3 GetPointFromHit(Vector3 hit)
        {
            // this monstrosity exists so that grid and incremental snap work when possible, and
            // incremental is enabled when grid is not available.
            var point = m_Polygon.isOnGrid
                ? EditorSnapping.MoveSnap(hit)
                : EditorSnapping.snapMode == SnapMode.Relative
                    ? ProBuilderSnapping.Snap(hit, EditorSnapping.incrementalSnapMoveValue)
                    : hit;

            return point;
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

                    if (plane.Raycast(ray, out hitDistance))
                    {
                        evt.Use();
                        polygon.m_Points[m_SelectedIndex] = GetPointInLocalSpace(ray.GetPoint(hitDistance));
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
                    SceneView.RepaintAll();
                }
            }
            else if (polygon.polyEditMode == PolyShape.PolyEditMode.Path)
            {
                if (eventType == EventType.MouseDown && evt.button == 0 && HandleUtility.nearestControl == m_ControlId)
                {
                    if (polygon.m_Points.Count < 1)
                        SetupInputPlane(evt.mousePosition);

                    float hitDistance = Mathf.Infinity;

                    Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);

                    if (plane.Raycast(ray, out hitDistance))
                    {
                        Undo.RegisterCompleteObjectUndo(polygon, "Add Polygon Shape Point");

                        Vector3 hit = ray.GetPoint(hitDistance);

                        if (polygon.m_Points.Count < 1)
                        {
                            Undo.RegisterCompleteObjectUndo(polygon.transform, "Set position");
                            polygon.transform.position = GetPointFromHit(hit);

                            Vector3 cameraFacingPlaneNormal = plane.normal;
                            if (Vector3.Dot(cameraFacingPlaneNormal, SceneView.lastActiveSceneView.camera.transform.forward) > 0f)
                                cameraFacingPlaneNormal *= -1;

                            polygon.transform.rotation = Quaternion.LookRotation(cameraFacingPlaneNormal) * Quaternion.Euler(new Vector3(90f, 0f, 0f));
                        }

                        Vector3 point = GetPointInLocalSpace(hit);

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
                else
                {
                    float hitDistance = Mathf.Infinity;
                    Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);

                    if(plane.Raycast(ray, out hitDistance))
                    {
                        Vector3 hit = ray.GetPoint(hitDistance);
                        m_CurrentPosition = GetPointInLocalSpace(hit);
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
                    Vector2 mouse = evt.mousePosition;
                    Ray ray = HandleUtility.GUIPointToWorldRay(mouse);
                    float hitDistance = Mathf.Infinity;
                    if(plane.Raycast(ray, out hitDistance))
                    {
                        Vector3 hit = ray.GetPoint(hitDistance);
                        Vector3 point = GetPointInLocalSpace(hit);

                        int polyCount = polygon.m_Points.Count;

                        float distToLineInGUI;
                        int index;
                        Vector3 pInGUI = EditorHandleUtility.ClosestPointToPolyLine(polygon.m_Points, out index, out distToLineInGUI, true, polygon.transform);

                        Vector3 aToPoint = point - polygon.m_Points[index - 1];
                        Vector3 aToB = polygon.m_Points[index % polyCount] - polygon.m_Points[index - 1];

                        float ratio = Vector3.Dot(aToPoint, aToB.normalized) / aToB.magnitude;
                        Vector3 wp =  Vector3.Lerp(polygon.m_Points[index - 1], polygon.m_Points[index % polyCount], ratio);
                        wp = polygon.transform.TransformPoint(wp);

                        Vector2 aInGUI = HandleUtility.WorldToGUIPoint(polygon.transform.TransformPoint(polygon.m_Points[index - 1]));
                        Vector2 bInGUI = HandleUtility.WorldToGUIPoint(polygon.transform.TransformPoint(polygon.m_Points[index % polyCount]));
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

                            if (evt.type == EventType.MouseDown && evt.button == 0 && HandleUtility.nearestControl == m_ControlId)
                            {
                                evt.Use();

                                UndoUtility.RecordObject(polygon, "Insert Point");
                                polygon.m_Points.Insert(index, point);
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
            plane = EditorHandleUtility.FindBestPlane(mousePosition);

            var planeNormal = plane.normal;
            var planeCenter = plane.normal * -plane.distance;

            // if hit point on plane is cardinal axis and on grid, snap to grid.
            if (Math.IsCardinalAxis(planeNormal))
            {
                const float epsilon = .00001f;
                bool offGrid = false;
                Vector3 snapVal = EditorSnapping.activeMoveSnapValue;
                Vector3 center = Vector3.Scale(ProBuilderSnapping.GetSnappingMaskBasedOnNormalVector(planeNormal), planeCenter);
                for (int i = 0; i < 3; i++)
                    offGrid |= Mathf.Abs(snapVal[i] % center[i]) > epsilon;
                polygon.isOnGrid = !offGrid;
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

            Vector3 up = trs.up;
            Vector3 right = trs.right;
            Vector3 forward = trs.forward;
            Vector3 center = Vector3.zero;

            Event evt = Event.current;

            bool used = evt.type == EventType.Used;

            if (!used &&
                (evt.type == EventType.MouseDown && evt.button == 0 &&
                 !EditorHandleUtility.IsAppendModifier(evt.modifiers)))
            {
                m_SelectedIndex = -1;
            }

            if(evt.type == EventType.Repaint && polygon.polyEditMode == PolyShape.PolyEditMode.Path)
            {
                if (HandleUtility.nearestControl == m_ControlId)
                {
                    Vector3 currentPos = polygon.transform.TransformPoint(m_CurrentPosition);
                    Handles.color = k_HandleColor;
                    Handles.DotHandleCap(-1, currentPos, Quaternion.identity, HandleUtility.GetHandleSize(currentPos) * k_HandleSize, evt.type);
                    Handles.color = Color.white;
                }
            }

            if (polygon.polyEditMode == PolyShape.PolyEditMode.Height)
            {
                if (!used && evt.type == EventType.MouseUp && evt.button == 0 && !EditorHandleUtility.IsAppendModifier(evt.modifiers))
                {
                    evt.Use();
                    UndoUtility.RecordObject(polygon, "Set Height");
                    SetPolyEditMode(PolyShape.PolyEditMode.Edit);
                    return;
                }

                bool sceneInUse = EditorHandleUtility.SceneViewInUse(evt);
                Ray r = HandleUtility.GUIPointToWorldRay(evt.mousePosition);

                Vector3 origin = polygon.transform.TransformPoint(Math.Average(polygon.m_Points));
                float extrude = polygon.extrude;

                if (evt.type == EventType.MouseMove && !sceneInUse)
                {
                    Vector3 p = Math.GetNearestPointRayRay(origin, up, r.origin, r.direction);
                    extrude = EditorSnapping.MoveSnap(s_HeightMouseOffset + Vector3.Distance(origin, p) * Mathf.Sign(Vector3.Dot(p - origin, up)));
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

                    EditorGUI.BeginChangeCheck();

                    int id = GUIUtility.GetControlID(FocusType.Passive);
                    Handles.color = ii == m_SelectedIndex || HandleUtility.nearestControl == id ? k_HandleSelectedColor : k_HandleColor;
                    point = Handles.Slider2D(id, point, up, right, forward, size, Handles.DotHandleCap, Vector2.zero, true);

                    if (ii == 0)
                        m_ControlId0 = id;

                    if (EditorGUI.EndChangeCheck())
                    {
                        UndoUtility.RecordObject(polygon, "Move Polygon Shape Point");
                        polygon.m_Points[ii] = GetPointInLocalSpace(point);
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
                        UndoUtility.RecordObject(polygon, "Set Polygon Shape Height");
                        polygon.extrude = EditorSnapping.MoveSnap(Vector3.Distance(extrude, center) * Mathf.Sign(Vector3.Dot(up, extrude - center)));
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
            DrawPolyLine();
            DrawGuideLine();
        }

        void DrawPolyLine()
        {
            if (m_Polygon.m_Points.Count < 2)
                return;

            int count = m_Polygon.m_Points.Count;
            int vc = polygon.polyEditMode == PolyShape.PolyEditMode.Path ? count : count + 1;
            Vector3[] verticesPositions = new Vector3[vc];

            for(int i = 0; i < vc; i++)
            {
                verticesPositions[i] = m_Polygon.transform.TransformPoint(m_Polygon.m_Points[i % count]);
            }

            Handles.color = m_CurrentLineColor;
            Handles.DrawPolyLine(verticesPositions);
            Handles.color = Color.white;
        }

        /// <summary>
        /// Draw a helper line between the last point of the cut and the current position of the mouse cursor
        /// </summary>
        /// <returns>true if the line can be traced (the position of the cursor must be valid and the cut have one point minimum)</returns>
        void DrawGuideLine()
        {
            if(m_CurrentPosition.Equals(Vector3.positiveInfinity)
               || m_PlacingPoint
               || polygon.polyEditMode != PolyShape.PolyEditMode.Path)
                return;

            if((polygon.controlPoints.Count > 0 && HandleUtility.nearestControl == m_ControlId) // the cursor is not on any other knot
               || (polygon.controlPoints.Count > 2 && HandleUtility.nearestControl == m_ControlId0)) // or the cursor is hovering the first knot and we can close the shape
            {
                var closestPoint = (polygon.controlPoints.Count > 2 && HandleUtility.nearestControl == m_ControlId0) ? polygon.controlPoints[0] : m_CurrentPosition;
                Handles.color = k_DrawingLineColor;
                Handles.DrawDottedLine(m_Polygon.transform.TransformPoint(polygon.controlPoints[polygon.controlPoints.Count - 1]),
                    m_Polygon.transform.TransformPoint(closestPoint), 5f);
                Handles.color = Color.white;
            }
        }

        internal virtual void HandleKeyEvent(Event evt)
        {
            KeyCode key = evt.keyCode;

            switch (key)
            {
                case KeyCode.Space:
                case KeyCode.Return:
                {
                    if (polygon?.polyEditMode == PolyShape.PolyEditMode.Path)
                    {
                        if (polygon.m_Points.Count > 2)
                            SetPolyEditMode(PolyShape.PolyEditMode.Height);
                        evt.Use();
                    }
                    else if (polygon?.polyEditMode == PolyShape.PolyEditMode.Height
                            || polygon?.polyEditMode == PolyShape.PolyEditMode.Edit)
                    {
                        LeaveTool();
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
                    LeaveTool();
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
            float dot = Vector3.Dot(SceneView.lastActiveSceneView.camera.transform.forward, polygon.transform.up);
            return Mathf.Abs(Mathf.Abs(dot) - 1f) < .01f;
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
            if (polygon != null && polygon.polyEditMode == PolyShape.PolyEditMode.Height)
            {
                //resetting state if undo is performed while in height edit mode
                polygon.polyEditMode =  PolyShape.PolyEditMode.Path;
                polygon.extrude = 0f;
                polygon.mesh.Clear();
                DestroyImmediate(polygon.mesh.mesh, true);
            }

            if (polygon != null && polygon.polyEditMode != PolyShape.PolyEditMode.None)
                RebuildPolyShapeMesh(polygon);
        }

        void OnActiveContextChanged()
        {
            ToolManager.RestorePreviousPersistentTool();
        }

        void OnSelectModeChanged(SelectMode selectMode)
        {
            if(MeshSelection.activeMesh)
            {
                PolyShape shape = MeshSelection.activeMesh.GetComponent<PolyShape>();
                if(shape != null && shape != polygon)
                    LeaveTool();
            }
        }

        /// <summary>
        /// Called when object selection changes.
        /// </summary>
        protected virtual void OnObjectSelectionChanged()
        {
            if(polygon == null)
                return;

            if(MeshSelection.activeMesh)
            {
                PolyShape shape = MeshSelection.activeMesh.GetComponent<PolyShape>();
                if(shape == null)
                    LeaveTool();
                else if(shape != polygon)
                    UpdateTarget(shape);
            }
        }
    }
}

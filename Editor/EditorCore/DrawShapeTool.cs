using System;
using UnityEditor.EditorTools;
using UnityEditor.SettingsManagement;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using Math = UnityEngine.ProBuilder.Math;
using UObject = UnityEngine.Object;
#if UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.ToolManager;
#else
using ToolManager = UnityEditor.EditorTools.EditorTools;
#endif

namespace UnityEditor.ProBuilder
{
    class DrawShapeTool : EditorTool
    {
        internal const int k_MinOverlayWidth = 250;

        ShapeState m_CurrentState;

        internal ProBuilderShape m_LastShapeCreated = null;

        internal ProBuilderShape m_ProBuilderShape;
        internal bool m_IsShapeInit;

        Editor m_ShapeEditor;

        bool m_HandleSelectionChanges = false;

        internal bool handleSelectionChange
        {
            set
            {
                if(m_HandleSelectionChanges == value)
                    return;

                m_HandleSelectionChanges = value;
                if(value)
                    MeshSelection.objectSelectionChanged += OnSelectionChanged;
                else
                    MeshSelection.objectSelectionChanged -= OnSelectionChanged;

            }
        }

        // plane of interaction
        internal UnityEngine.Plane m_Plane;
        internal Vector3 m_PlaneForward, m_PlaneRight;
        internal Quaternion m_PlaneRotation;
        internal Vector3 m_BB_Origin, m_BB_OppositeCorner, m_BB_HeightCorner;

        // Shape's duplicate
        internal GameObject m_DuplicateGO = null;
        Material m_ShapePreviewMaterial;
        static readonly Color k_PreviewColor = new Color(.5f, .9f, 1f, .56f);

        //Shape's properties
        internal bool m_IsOnGrid;

        internal Bounds m_Bounds;
        static readonly Color k_BoundsColor = new Color(.2f, .4f, .8f, 1f);

        static readonly GUIContent k_ShapeTitle = new GUIContent("Create Shape");

        [UserSetting]
        internal static Pref<int> s_ActiveShapeIndex = new Pref<int>("ShapeBuilder.ActiveShapeIndex", 0);
        public static Pref<bool> s_SettingsEnabled = new Pref<bool>("ShapeComponent.SettingsEnabled", false, SettingsScope.Project);

        [UserSetting]
        internal static Pref<int> s_LastPivotLocation = new Pref<int>("ShapeBuilder.LastPivotLocation", (int)PivotLocation.FirstCorner);
        [UserSetting]
        internal static Pref<Vector3> s_LastPivotPosition = new Pref<Vector3>("ShapeBuilder.LastPivotPosition", Vector3.zero);
        [UserSetting]
        internal static Pref<Vector3> s_LastSize = new Pref<Vector3>("ShapeBuilder.LastSize", Vector3.one);
        [UserSetting]
        internal static Pref<Quaternion> s_LastRotation = new Pref<Quaternion>("ShapeBuilder.LastRotation", Quaternion.identity);

        int m_ControlID;

        internal float minSnapSize
        {
            get
            {
                if (m_IsOnGrid)
                {
                    return Mathf.Min(EditorSnapping.activeMoveSnapValue.x,
                        Mathf.Min(EditorSnapping.activeMoveSnapValue.y,
                            EditorSnapping.activeMoveSnapValue.z));
                }

                return Mathf.Min(EditorSnapping.incrementalSnapMoveValue.x,
                    Mathf.Min(EditorSnapping.incrementalSnapMoveValue.y,
                        EditorSnapping.incrementalSnapMoveValue.z));
            }
        }

        const float k_MinBoundLength = 0.001f; // 1mm

        // ideally this would be owned by the state machine
        public int controlID => m_ControlID;

        //Styling
        static class Styles
        {
            public static GUIStyle command = "command";
        }
        GUIStyle m_BoldCenteredStyle = null;

        //EditorTools
        GUIContent m_IconContent;
        public override GUIContent toolbarIcon
        {
            get { return m_IconContent; }
        }

        public static Type activeShapeType
        {
            get { return s_ActiveShapeIndex < 0 ? typeof(Cube) : EditorShapeUtility.availableShapeTypes[s_ActiveShapeIndex]; }
        }

#if UNITY_2021_1_OR_NEWER
        public override bool gridSnapEnabled => true;
#endif

        internal ProBuilderShape currentShapeInOverlay
        {
            get
            {
                if(m_CurrentState is ShapeState_InitShape  && m_LastShapeCreated != null)
                    return m_LastShapeCreated;

                if(m_CurrentState is ShapeState_DrawBaseShape && m_DuplicateGO != null)
                    return m_DuplicateGO.GetComponent<ProBuilderShape>();

                if(m_ProBuilderShape == null)
                {
                    m_ProBuilderShape = new GameObject("Shape", typeof(ProBuilderShape)).GetComponent<ProBuilderShape>();
                    m_ProBuilderShape.gameObject.hideFlags = HideFlags.HideAndDontSave;
                    m_ProBuilderShape.hideFlags = HideFlags.None;
                    m_ProBuilderShape.SetShape(EditorShapeUtility.CreateShape(activeShapeType),EditorUtility.newShapePivotLocation);
                    m_ProBuilderShape.pivotLocation = (PivotLocation)s_LastPivotLocation.value;
                    m_ProBuilderShape.pivotLocalPosition = s_LastPivotPosition.value;
                    m_ProBuilderShape.size = s_LastSize.value;
                    m_ProBuilderShape.rotation = s_LastRotation.value;
                }
                return m_ProBuilderShape;
            }
        }

        void OnEnable()
        {
            m_CurrentState = InitStateMachine();

            m_IconContent = new GUIContent()
            {
                image = IconUtility.GetIcon("Toolbar/Panel_Shapes"),
                text = "Shape Settings",
                tooltip = "Shape Settings"
            };

            Undo.undoRedoPerformed += HandleUndoRedoPerformed;
            ToolManager.activeToolChanged += OnActiveToolChanged;
            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;
            handleSelectionChange = true;


            m_ShapePreviewMaterial = new Material(BuiltinMaterials.defaultMaterial.shader);
            m_ShapePreviewMaterial.hideFlags = HideFlags.HideAndDontSave;

            if (m_ShapePreviewMaterial.HasProperty("_MainTex"))
                m_ShapePreviewMaterial.mainTexture = (Texture2D)Resources.Load("Textures/GridBox_Default");

            if (m_ShapePreviewMaterial.HasProperty("_Color"))
                m_ShapePreviewMaterial.SetColor("_Color", k_PreviewColor);
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed -= HandleUndoRedoPerformed;
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
            ToolManager.activeToolChanged -= OnActiveToolChanged;
            handleSelectionChange = false;

            if(m_ShapeEditor != null)
                DestroyImmediate(m_ShapeEditor);
            if(m_ProBuilderShape != null && !( m_CurrentState is ShapeState_InitShape ))
                ShapeState.ResetState();
        }

        void OnDestroy()
        {
            if(m_ShapePreviewMaterial)
                DestroyImmediate(m_ShapePreviewMaterial);
        }

        void OnSelectModeChanged(SelectMode _) => DestroyImmediate(this);

        void OnActiveToolChanged()
        {
            if(ToolManager.IsActiveTool(this))
                SetBounds(currentShapeInOverlay.size);
        }

        void HandleUndoRedoPerformed()
        {
            if(ToolManager.IsActiveTool(this))
                m_CurrentState = ShapeState.ResetState();
        }

        void OnSelectionChanged()
        {
            if(ToolManager.IsActiveTool(this))
            {
                if(Selection.activeGameObject != null)
                {
                    m_CurrentState = ShapeState.ResetState();
                    ToolManager.RestorePreviousTool();
                }
            }
        }

        /// <summary>
        /// Init the state machine associated to the tool.
        /// All states are linked together and initialized.
        /// </summary>
        /// <returns>
        /// Returns the current state of the StateMachine,
        /// this state machine will self-handle during its lifetime.
        /// </returns>
        ShapeState InitStateMachine()
        {
            ShapeState.tool = this;
            ShapeState initState = new ShapeState_InitShape();
            ShapeState drawBaseState = new ShapeState_DrawBaseShape();
            ShapeState drawHeightState = new ShapeState_DrawHeightShape();
            ShapeState.s_defaultState = initState;
            initState.m_nextState = drawBaseState;
            drawBaseState.m_nextState = drawHeightState;
            drawHeightState.m_nextState = initState;

            return ShapeState.StartStateMachine();
        }

        internal static void SaveShapeParams(ProBuilderShape proBuilderShape)
        {
            s_LastPivotLocation.value = (int)proBuilderShape.pivotLocation;
            s_LastPivotPosition.value = proBuilderShape.pivotLocalPosition;
            s_LastSize.value = proBuilderShape.size;
            s_LastRotation.value = proBuilderShape.rotation;

            EditorShapeUtility.SaveParams(proBuilderShape.shape);
        }

        internal static void ApplyPrefsSettings(ProBuilderShape proBuilderShape)
        {
            proBuilderShape.pivotLocation = (PivotLocation)s_LastPivotLocation.value;
            proBuilderShape.pivotLocalPosition = s_LastPivotPosition.value;
            proBuilderShape.size = s_LastSize.value;
            proBuilderShape.rotation = s_LastRotation.value;
        }

        // Transform the point according to the snapping settings
        public Vector3 GetPoint(Vector3 point, bool useIncrementSnap = false)
        {
            if(useIncrementSnap)
                return ProBuilderSnapping.Snap(point, EditorSnapping.incrementalSnapMoveValue);

            if (m_IsOnGrid)
                return ProBuilderSnapping.Snap(point, EditorSnapping.activeMoveSnapValue);

            return point;
        }

        internal void SetBounds(Vector3 size)
        {
            //Keep orientation created using mouse drag
            var dragDirection = m_BB_OppositeCorner - m_BB_Origin;
            float x = dragDirection.x < 0 ? -size.x : size.x;
            float z = dragDirection.z < 0 ? -size.z : size.z;

            m_BB_OppositeCorner = m_BB_Origin + new Vector3(x, 0, z);
            m_BB_HeightCorner = m_BB_Origin + size;
        }

        internal void DoDuplicateShapePreviewHandle(Vector3 position)
        {
            var evt = Event.current;

            if(evt.type != EventType.Repaint)
                return;

            bool previewShortcutActive = evt.shift && !(evt.control || evt.command);

            if (HandleUtility.nearestControl != m_ControlID || !previewShortcutActive)
            {
                DestroyImmediate(m_DuplicateGO);
                return;
            }

            var pivotLocation = (PivotLocation)s_LastPivotLocation.value;
            var size = currentShapeInOverlay.size;

            m_Bounds.size = size;
            Vector3 cornerPosition;

            switch (pivotLocation)
            {
                case PivotLocation.FirstCorner:
                    cornerPosition = GetPoint(position);
                    m_PlaneRotation = Quaternion.LookRotation(m_PlaneForward, m_Plane.normal);
                    m_Bounds.center = cornerPosition + m_PlaneRotation * size / 2f;

                    m_BB_Origin = cornerPosition;
                    m_BB_HeightCorner = m_Bounds.center + m_PlaneRotation * (size / 2f);
                    m_BB_OppositeCorner = m_BB_HeightCorner - m_PlaneRotation * new Vector3(0, size.y, 0);
                    break;

                case PivotLocation.Center:
                default:
                    position = GetPoint(position);
                    cornerPosition = position - size / 2f;
                    cornerPosition.y = position.y;
                    m_Bounds.center = cornerPosition + new Vector3(size.x / 2f, 0, size.z / 2f) + (size.y / 2f) * m_Plane.normal;
                    m_PlaneRotation = Quaternion.LookRotation(m_PlaneForward, m_Plane.normal);

                    m_BB_Origin = m_Bounds.center - m_PlaneRotation * (size / 2f);
                    m_BB_HeightCorner = m_Bounds.center + m_PlaneRotation * (size / 2f);
                    m_BB_OppositeCorner = m_BB_HeightCorner - m_PlaneRotation * new Vector3(0, size.y, 0);
                    break;
            }

            if (m_DuplicateGO == null)
            {
                var instantiated = ShapeFactory.Instantiate(activeShapeType, ((PivotLocation)s_LastPivotLocation.value));
                var shape = instantiated.GetComponent<ProBuilderShape>();
                m_DuplicateGO = shape.gameObject;
                m_DuplicateGO.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
                ApplyPrefsSettings(shape);
                shape.GetComponent<MeshRenderer>().sharedMaterial = m_ShapePreviewMaterial;

                EditorShapeUtility.CopyLastParams(shape.shape, shape.shape.GetType());
                shape.Rebuild(m_Bounds, m_PlaneRotation, m_BB_Origin);
                ProBuilderEditor.Refresh(false);
            }

            var pivot = GetPoint(position);
            if (pivotLocation == PivotLocation.Center)
                pivot += .5f * size.y * m_Plane.normal;

            m_DuplicateGO.transform.SetPositionAndRotation(pivot, Quaternion.LookRotation(m_PlaneForward, m_Plane.normal));

            DrawBoundingBox(false);
        }

        /// <summary>
        /// Recalculates the bounding box for this mesh's shape.
        /// </summary>
        /// <seealso cref="UnityEngine.ProBuilderMesh(Refresh)" />
        void RecalculateBounds()
        {
            var forward = HandleUtility.PointOnLineParameter(m_BB_OppositeCorner, m_BB_Origin, m_PlaneForward);
            var right = HandleUtility.PointOnLineParameter(m_BB_OppositeCorner, m_BB_Origin, m_PlaneRight);

            var localHeight = Quaternion.Inverse(m_PlaneRotation) * (m_BB_HeightCorner - m_BB_OppositeCorner);
            var height = localHeight.y;

            m_Bounds.size = forward * Vector3.forward + right * Vector3.right + height * Vector3.up;
            m_Bounds.center = m_BB_Origin + 0.5f * ( m_BB_OppositeCorner - m_BB_Origin ) + 0.5f * (m_BB_HeightCorner - m_BB_OppositeCorner);

            //Prevent Z-fighting with the drawing surface
            if(Mathf.Abs(m_Bounds.center.y) < 0.0001f)
                m_Bounds.center = m_Bounds.center + 0.0001f * Vector3.up;

            m_PlaneRotation = Quaternion.LookRotation(m_PlaneForward,m_Plane.normal);
        }

        internal void RebuildShape()
        {
            RecalculateBounds();

            if(m_Bounds.size.sqrMagnitude <= Mathf.Min(.01f , minSnapSize*minSnapSize)
               || Mathf.Abs(m_Bounds.extents.x) < k_MinBoundLength
               || Mathf.Abs(m_Bounds.extents.z) < k_MinBoundLength)
            {
                if(m_ProBuilderShape != null
                   && m_ProBuilderShape.mesh.vertexCount > 0)
                {
                    m_ProBuilderShape.size = Vector3.zero;
                    m_ProBuilderShape.mesh.Clear();
                    m_ProBuilderShape.mesh.Rebuild();
                    m_ProBuilderShape.Rebuild(new Bounds(m_BB_Origin, Vector3.zero), m_PlaneRotation, m_BB_Origin);
                    ProBuilderEditor.Refresh(true);
                }
                return;
            }

            if (!m_IsShapeInit)
            {
                var shapeComponent = currentShapeInOverlay;
                EditorShapeUtility.CopyLastParams(shapeComponent.shape, shapeComponent.shape.GetType());
                shapeComponent.gameObject.hideFlags = HideFlags.HideInHierarchy;
                shapeComponent.mesh.renderer.sharedMaterial = EditorMaterialUtility.GetUserMaterial();
                shapeComponent.rotation = Quaternion.identity;
                shapeComponent.gameObject.name = EditorShapeUtility.GetName(shapeComponent.shape);
                m_IsShapeInit = true;
            }

            m_ProBuilderShape.Rebuild(m_Bounds, m_PlaneRotation, m_BB_Origin);
            ProBuilderEditor.Refresh(false);

            SceneView.RepaintAll();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            // todo refactor overlays to use `Overlay` class
#pragma warning disable 618
            SceneViewOverlay.Window(k_ShapeTitle, OnOverlayGUI, 0, SceneViewOverlay.WindowDisplayOption.OneWindowPerTitle);
#pragma warning restore 618

            var evt = Event.current;

            if (EditorHandleUtility.SceneViewInUse(evt))
                return;

            m_ControlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(m_ControlID);

            if(GUIUtility.hotControl == 0)
                EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.ArrowPlus);

            m_CurrentState = m_CurrentState.DoState(evt);
        }

        internal void DrawBoundingBox(bool drawCorners = true)
        {
            using (new Handles.DrawingScope(k_BoundsColor, Matrix4x4.TRS(m_Bounds.center, m_PlaneRotation.normalized, Vector3.one)))
            {
                Handles.DrawWireCube(Vector3.zero, m_Bounds.size);
            }

            if(!drawCorners)
                return;

            using (new Handles.DrawingScope(Color.white))
            {
                Handles.DotHandleCap(-1, m_BB_Origin, Quaternion.identity, HandleUtility.GetHandleSize(m_BB_Origin) * 0.05f, EventType.Repaint);
                Handles.DotHandleCap(-1, m_BB_OppositeCorner, Quaternion.identity, HandleUtility.GetHandleSize(m_BB_OppositeCorner) * 0.05f, EventType.Repaint);
            }
            using (new Handles.DrawingScope(EditorHandleDrawing.vertexSelectedColor))
            {
                Handles.DotHandleCap(-1, m_BB_HeightCorner, Quaternion.identity, HandleUtility.GetHandleSize(m_BB_HeightCorner) * 0.05f, EventType.Repaint);
            }
        }

        void OnOverlayGUI(UObject overlayTarget, SceneView view)
        {
            DrawShapeGUI();

#if !UNITY_2021_1_OR_NEWER
            var snapDisabled = Tools.pivotRotation != PivotRotation.Global;
            using(new EditorGUI.DisabledScope(snapDisabled))
            {
                if(snapDisabled)
                    EditorGUILayout.Toggle("Snapping (only Global)", false);
                else
                    EditorSnapSettings.gridSnapEnabled = EditorGUILayout.Toggle("Grid Snapping", EditorSnapSettings.gridSnapEnabled);
            }
#endif

            string foldoutName = "Shape Properties (New Shape)";
            if(currentShapeInOverlay == m_LastShapeCreated)
                foldoutName = "Shape Properties (" + m_LastShapeCreated.name + ")";

            Editor.CreateCachedEditor(currentShapeInOverlay, typeof(ProBuilderShapeEditor), ref m_ShapeEditor);

            // 21.2 introduces Scene View Overlays. There's no need for additional styling, but we do need to force
            // the width to accomodate for IMGUI not laying out nicely.
#if UNITY_2021_2_OR_NEWER
            GUILayout.BeginVertical(GUILayout.MinWidth(k_MinOverlayWidth));
            ((ProBuilderShapeEditor)m_ShapeEditor).m_ShapePropertyLabel.text = foldoutName;
            ((ProBuilderShapeEditor)m_ShapeEditor).DrawShapeParametersGUI(this);
            GUILayout.EndVertical();
#else
            using (new EditorGUILayout.VerticalScope(new GUIStyle(EditorStyles.frameBox)))
            {
                ((ProBuilderShapeEditor)m_ShapeEditor).m_ShapePropertyLabel.text = foldoutName;
                ((ProBuilderShapeEditor)m_ShapeEditor).DrawShapeParametersGUI(this);
            }
#endif
        }

        void ResetPrefs()
        {
            var type = EditorShapeUtility.availableShapeTypes[s_ActiveShapeIndex];
            if(currentShapeInOverlay == m_LastShapeCreated)
                m_LastShapeCreated = null;

            UndoUtility.RegisterCompleteObjectUndo(currentShapeInOverlay, "Change Shape");
            currentShapeInOverlay.SetShape(EditorShapeUtility.CreateShape(type), currentShapeInOverlay.pivotLocation);
            SetBounds(currentShapeInOverlay.size);

            ProBuilderEditor.Refresh();
        }

        void DrawShapeGUI()
        {
            if(m_BoldCenteredStyle == null)
                m_BoldCenteredStyle = new GUIStyle("BoldLabel") { alignment = TextAnchor.MiddleCenter };

            EditorGUILayout.LabelField(EditorShapeUtility.shapeTypes[s_ActiveShapeIndex.value], m_BoldCenteredStyle, GUILayout.ExpandWidth(true));

            if(EditorShapeUtility.s_ResetUserPrefs.value)
                ResetPrefs();

            var shape = currentShapeInOverlay.shape;

            int groupCount = EditorShapeUtility.shapeTypesGUI.Count;
            for(int i = 0; i < groupCount; i++)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                int index = GUILayout.Toolbar(s_ActiveShapeIndex.value - + i * EditorShapeUtility.MaxContentPerGroup, EditorShapeUtility.shapeTypesGUI[i], Styles.command);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                if (EditorGUI.EndChangeCheck())
                {
                    s_ActiveShapeIndex.value = index + i * EditorShapeUtility.MaxContentPerGroup;

                    var type = EditorShapeUtility.availableShapeTypes[s_ActiveShapeIndex];
                    if(shape.GetType() != type)
                    {
                        if(currentShapeInOverlay == m_LastShapeCreated)
                            m_LastShapeCreated = null;

                        UndoUtility.RegisterCompleteObjectUndo(currentShapeInOverlay, "Change Shape");
                        currentShapeInOverlay.SetShape(EditorShapeUtility.CreateShape(type), currentShapeInOverlay.pivotLocation);
                        SetBounds(currentShapeInOverlay.size);

                        ProBuilderEditor.Refresh();
                    }
                }
            }
        }
    }
}

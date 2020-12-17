using System;
using System.Linq;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;
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
        ShapeState m_CurrentState;

        internal ShapeComponent m_LastShapeCreated = null;
        internal static Quaternion s_LastShapeRotation = Quaternion.identity;

        internal ShapeComponent m_ShapeComponent;
        internal bool m_IsShapeInit;

        Editor m_ShapeEditor;

        // plane of interaction
        internal UnityEngine.Plane m_Plane;
        internal Vector3 m_PlaneForward;
        internal Vector3 m_PlaneRight;
        internal Quaternion m_PlaneRotation;
        internal Vector3 m_BB_Origin, m_BB_OppositeCorner, m_BB_HeightCorner;

        internal bool m_IsOnGrid;

        internal Bounds m_Bounds;
        internal static readonly Color k_BoundsColor = new Color(.2f, .4f, .8f, 1f);

        readonly GUIContent k_ShapeTitle = new GUIContent("Draw Shape");
        GUIContent m_SnapAngleContent;

        [Range(1,90)]
        [SerializeField]
        protected int m_snapAngle = 15;

        public float snapAngle => (float)m_snapAngle;

        internal static Pref<int> s_ActiveShapeIndex = new Pref<int>("ShapeBuilder.ActiveShapeIndex", 0);
        internal static Pref<Vector3> s_Size = new Pref<Vector3>("ShapeBuilder.Size", Vector3.zero);
        internal static Pref<bool> s_SettingsEnabled = new Pref<bool>("ShapeBuilder.SettingsEnabled", false);

        int m_ControlID;
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

        ShapeComponent currentShapeInOverlay
        {
            get
            {
                if(( m_CurrentState is ShapeState_InitShape ) && m_LastShapeCreated != null)
                    return m_LastShapeCreated;

                if(m_ShapeComponent == null)
                {
                    m_ShapeComponent = new GameObject("Shape", typeof(ShapeComponent)).GetComponent<ShapeComponent>();
                    m_ShapeComponent.gameObject.hideFlags = HideFlags.HideAndDontSave;
                    m_ShapeComponent.hideFlags = HideFlags.None;
                    m_ShapeComponent.SetShape(EditorShapeUtility.CreateShape(activeShapeType), EditorUtility.newShapePivotLocation);
                }
                return m_ShapeComponent;
            }
        }

        static DrawShapeTool()
        {
        }

        void OnEnable()
        {
            m_CurrentState = InitStateMachine();

            m_IconContent = new GUIContent()
            {
                image = IconUtility.GetIcon("Tools/ShapeTool/Arch"),
                text = "Draw Shape Tool",
                tooltip = "Draw Shape Tool"
            };

            m_SnapAngleContent = new GUIContent("Rotation Snap", L10n.Tr("Defines an angle in [1,90] to snap rotation."));

            Undo.undoRedoPerformed += HandleUndoRedoPerformed;
            MeshSelection.objectSelectionChanged += OnSelectionChanged;
            ToolManager.activeToolChanged += OnActiveToolChanged;
        }

        void OnDestroy()
        {
            MeshSelection.objectSelectionChanged -= OnSelectionChanged;
        }

        void OnActiveToolChanged()
        {
            if(ToolManager.IsActiveTool(this))
            {
                var type = EditorShapeUtility.availableShapeTypes[s_ActiveShapeIndex];
                currentShapeInOverlay.SetShape(EditorShapeUtility.CreateShape(type),EditorUtility.newShapePivotLocation);
                SetBounds(currentShapeInOverlay.Size);
            }
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
                if(m_ShapeComponent != null && MeshSelection.activeMesh != m_ShapeComponent.mesh)
                    m_CurrentState = ShapeState.ResetState();
            }
        }

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

        void OnDisable()
        {
            if(m_ShapeEditor != null)
                DestroyImmediate(m_ShapeEditor);
            if (m_ShapeComponent != null && m_ShapeComponent.gameObject.hideFlags == HideFlags.HideAndDontSave)
                DestroyImmediate(m_ShapeComponent.gameObject);
        }

        // Returns a local space point,
        public Vector3 GetPoint(Vector3 point)
        {
            if (m_IsOnGrid)
            {
                Vector3 snapMask = ProBuilderSnapping.GetSnappingMaskBasedOnNormalVector(m_Plane.normal);
                return ProBuilderSnapping.Snap(point, Vector3.Scale(EditorSnapping.activeMoveSnapValue, snapMask));
            }
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

            s_Size.value = size;
        }

        internal void SetBoundsOrigin(Vector3 position)
        {
            Vector3 size = s_Size.value;
            m_Bounds.size = size;
            var cornerPosition = position - size / 2f;
            cornerPosition.y = position.y;
            cornerPosition = GetPoint(cornerPosition);
            m_Bounds.center = cornerPosition + new Vector3(size.x/2f,0, size.z/2f) + (size.y / 2f) * m_Plane.normal;
            m_PlaneRotation = Quaternion.LookRotation(m_PlaneForward,m_Plane.normal);
        }

        void RecalculateBounds()
        {
            var forward = HandleUtility.PointOnLineParameter(m_BB_OppositeCorner, m_BB_Origin, m_PlaneForward);
            var right = HandleUtility.PointOnLineParameter(m_BB_OppositeCorner, m_BB_Origin, m_PlaneRight);

            var heightDirection = m_BB_HeightCorner - m_BB_OppositeCorner;
            if(Mathf.Sign(Vector3.Dot(m_Plane.normal, heightDirection)) < 0)
                m_Plane.Flip();
            var height = heightDirection.magnitude;

            m_Bounds.size = forward * Vector3.forward + right * Vector3.right + height * Vector3.up;
            m_Bounds.center = m_BB_Origin + 0.5f * ( m_BB_OppositeCorner - m_BB_Origin ) + m_Plane.normal * (height * .5f);

            //Prevent Z-fighting with the drawing surface
            if(Mathf.Abs(m_Bounds.center.y) < 0.0001f)
                m_Bounds.center = m_Bounds.center + 0.0001f * Vector3.up;

            m_PlaneRotation = Quaternion.LookRotation(m_PlaneForward,m_Plane.normal);
        }

        internal void RebuildShape()
        {
            RecalculateBounds();

            if(m_Bounds.size.sqrMagnitude < .01f
               || Mathf.Abs(m_Bounds.extents.x) < 0.001f
               || Mathf.Abs(m_Bounds.extents.z) < 0.001f)
            {
                if(m_ShapeComponent.mesh.vertexCount > 0)
                {
                    m_ShapeComponent.mesh.Clear();
                    m_ShapeComponent.mesh.Rebuild();
                    ProBuilderEditor.Refresh(true);
                }
                return;
            }

            if (!m_IsShapeInit)
            {
                m_ShapeComponent.shape = EditorShapeUtility.GetLastParams(m_ShapeComponent.shape.GetType());
                m_ShapeComponent.gameObject.hideFlags = HideFlags.None;
                UndoUtility.RegisterCreatedObjectUndo(m_ShapeComponent.gameObject, "Draw Shape");
            }
;
            m_ShapeComponent.Rebuild(m_Bounds, m_PlaneRotation, EditorUtility.newShapePivotLocation);
            ProBuilderEditor.Refresh(false);

            if (!m_IsShapeInit)
            {
                EditorUtility.InitObject(m_ShapeComponent.mesh);
                m_IsShapeInit = true;
            }

            SceneView.RepaintAll();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            SceneViewOverlay.Window(k_ShapeTitle, OnOverlayGUI, 0, SceneViewOverlay.WindowDisplayOption.OneWindowPerTitle);

            var evt = Event.current;

            if (EditorHandleUtility.SceneViewInUse(evt))
                return;

            m_ControlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(m_ControlID);

            m_CurrentState = m_CurrentState.DoState(evt);
        }

        internal void DrawBoundingBox()
        {
            using (new Handles.DrawingScope(k_BoundsColor, Matrix4x4.TRS(m_Bounds.center, m_PlaneRotation.normalized, Vector3.one)))
            {
                Handles.DrawWireCube(Vector3.zero, m_Bounds.size);
            }
        }

        void OnOverlayGUI(UObject overlayTarget, SceneView view)
        {
            EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.ArrowPlus);
            EditorGUILayout.HelpBox(L10n.Tr("Hold Shift to preview shape. Click to create. Drag & drop for custom creation."), MessageType.Info);

            DrawShapeGUI();

            EditorSnapSettings.gridSnapEnabled = EditorGUILayout.Toggle("Snapping", EditorSnapSettings.gridSnapEnabled);
            m_snapAngle = EditorGUILayout.IntSlider(m_SnapAngleContent, m_snapAngle, 1, 90);

            string foldoutName = "New Shape Settings";
            if(currentShapeInOverlay != null &&  currentShapeInOverlay == m_LastShapeCreated)
                foldoutName = "Settings (" + m_LastShapeCreated.name + ")";

            Editor.CreateCachedEditor(currentShapeInOverlay, typeof(ShapeComponentEditor), ref m_ShapeEditor);

            GUIStyle style = new GUIStyle(EditorStyles.frameBox);

            using(new EditorGUILayout.VerticalScope(style))
            {
                s_SettingsEnabled.value = EditorGUILayout.Foldout(s_SettingsEnabled.value, foldoutName);
                if(s_SettingsEnabled)
                {
                    EditorGUI.indentLevel++;
                    ( (ShapeComponentEditor) m_ShapeEditor ).DrawShapeParametersGUI(this);
                    EditorGUI.indentLevel--;
                }
            }
        }

        void DrawShapeGUI()
        {
            if(m_BoldCenteredStyle == null)
                m_BoldCenteredStyle = new GUIStyle("BoldLabel") { alignment = TextAnchor.MiddleCenter };

            EditorGUILayout.LabelField(EditorShapeUtility.shapeTypes[s_ActiveShapeIndex.value], m_BoldCenteredStyle, GUILayout.ExpandWidth(true));

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
                        currentShapeInOverlay.SetShape(EditorShapeUtility.CreateShape(type), EditorUtility.newShapePivotLocation);
                        SetBounds(currentShapeInOverlay.Size);

                        ProBuilderEditor.Refresh();
                    }
                }
            }
        }
    }
}

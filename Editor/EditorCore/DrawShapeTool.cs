using System;
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
    internal class DrawShapeTool : EditorTool
    {
        ShapeState m_CurrentState;

        internal ShapeComponent m_Shape;
        internal bool m_IsShapeInit;
        internal Vector3 m_ShapeForward;

        Editor m_ShapeEditor;

        // plane of interaction
        internal UnityEngine.Plane m_Plane;
        internal Vector3 m_PlaneForward;
        internal Vector3 m_PlaneRight;
        internal Vector3 m_BB_Origin, m_BB_OppositeCorner, m_BB_HeightCorner;

        internal bool m_IsOnGrid;

        internal Quaternion m_Rotation;
        internal Bounds m_Bounds;
        readonly Color k_BoundsColor = new Color(.2f, .4f, .8f, 1f);

        readonly GUIContent k_ShapeTitle = new GUIContent("Draw Shape");

        internal static TypeCache.TypeCollection s_AvailableShapeTypes;
        internal static Pref<int> s_ActiveShapeIndex = new Pref<int>("ShapeBuilder.ActiveShapeIndex", 0);
        internal static Pref<Vector3> s_Size = new Pref<Vector3>("ShapeBuilder.Size", Vector3.zero);

        GUIContent m_IconContent;
        public override GUIContent toolbarIcon
        {
            get { return m_IconContent; }
        }

        public static Type activeShapeType
        {
            get { return s_ActiveShapeIndex < 0 ? typeof(Cube) : s_AvailableShapeTypes[s_ActiveShapeIndex]; }
        }

        static DrawShapeTool()
        {
            s_AvailableShapeTypes = TypeCache.GetTypesDerivedFrom<Shape>();
        }

        void OnEnable()
        {
            m_CurrentState = InitStateMachine();

            m_IconContent = new GUIContent()
            {
                image = EditorGUIUtility.LoadIconRequired("CustomTool"),
                text = "Draw Shape Tool",
                tooltip = "Draw Shape Tool"
            };

            Undo.undoRedoPerformed += HandleUndoRedoPerformed;
            MeshSelection.objectSelectionChanged += OnSelectionChanged;
        }

        void OnDestroy()
        {
            MeshSelection.objectSelectionChanged -= OnSelectionChanged;
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
                if(MeshSelection.activeMesh != m_Shape.mesh)
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
            if (m_Shape.gameObject.hideFlags == HideFlags.HideAndDontSave)
                DestroyImmediate(m_Shape.gameObject);
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
            m_Rotation = Quaternion.LookRotation(m_PlaneForward,m_Plane.normal);

            var dragDirection = m_BB_OppositeCorner - m_BB_Origin;
            float dragDotForward = Vector3.Dot(dragDirection, m_PlaneForward);
            float dragDotRight = Vector3.Dot(dragDirection, m_PlaneRight);
            if(dragDotForward < 0 && dragDotRight > 0 )
                m_ShapeForward = -Vector3.forward;
            else if(dragDotForward > 0 && dragDotRight < 0)
                m_ShapeForward = Vector3.forward;
            else if(dragDotForward < 0 && dragDotRight < 0 )
                m_ShapeForward = -Vector3.right;
            else if(dragDotForward > 0 && dragDotRight > 0)
                m_ShapeForward = Vector3.right;
        }

        internal void SetBoundsOrigin(Vector3 position)
        {
            Vector3 size = s_Size.value;
            m_Bounds.size = size;
            var cornerPosition = position - size / 2f;
            cornerPosition.y = position.y;
            cornerPosition = GetPoint(cornerPosition);
            m_Bounds.center = cornerPosition + new Vector3(size.x/2f,0, size.z/2f) + (size.y / 2f) * m_Plane.normal;
            m_Rotation = Quaternion.LookRotation(m_PlaneForward,m_Plane.normal);
        }

        internal void RebuildShape()
        {
            RecalculateBounds();

            if (m_Bounds.size.sqrMagnitude < .01f
                || Mathf.Abs(m_Bounds.extents.x) == 0
                || Mathf.Abs(m_Bounds.extents.z) == 0)
                return;

            if (!m_IsShapeInit)
            {
                m_Shape.shape = EditorShapeUtility.GetLastParams(m_Shape.shape.GetType());
                m_Shape.gameObject.hideFlags = HideFlags.None;
                UndoUtility.RegisterCreatedObjectUndo(m_Shape.gameObject, "Draw Shape");
            }

            m_Shape.shape.Forward = m_ShapeForward;
            m_Shape.Rebuild(m_Bounds, m_Rotation);
            m_Shape.mesh.SetPivot(PivotLocation.Center);
            ProBuilderEditor.Refresh(false);

            if (!m_IsShapeInit)
            {
                EditorUtility.InitObject(m_Shape.mesh);
                m_IsShapeInit = true;
            }

            SceneView.RepaintAll();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            SceneViewOverlay.Window(k_ShapeTitle, OnActiveToolGUI, 0, SceneViewOverlay.WindowDisplayOption.OneWindowPerTitle);

            var evt = Event.current;

            if (EditorHandleUtility.SceneViewInUse(evt))
                return;

            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);

            m_CurrentState = m_CurrentState.DoState(evt);
        }

        internal void DrawBoundingBox()
        {
            using (new Handles.DrawingScope(k_BoundsColor, Matrix4x4.TRS(m_Bounds.center, m_Rotation.normalized, Vector3.one)))
            {
                Handles.DrawWireCube(Vector3.zero, m_Bounds.size);
            }
        }

        void OnActiveToolGUI(UObject overlayTarget, SceneView view)
        {
            EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.ArrowPlus);
            EditorGUILayout.HelpBox(L10n.Tr("Hold and drag to create a new shape while controlling its size. Click to duplicate the last created shape."), MessageType.Info);

            EditorSnapSettings.gridSnapEnabled = EditorGUILayout.Toggle("Use Grid Snapping", EditorSnapSettings.gridSnapEnabled);

            if (m_Shape == null)
                return;

            Editor.CreateCachedEditor(m_Shape, typeof(ShapeComponentEditor), ref m_ShapeEditor);
            ((ShapeComponentEditor)m_ShapeEditor).DrawShapeGUI(this);
        }
    }
}

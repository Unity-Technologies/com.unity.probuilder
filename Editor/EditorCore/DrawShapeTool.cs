using System;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Math = UnityEngine.ProBuilder.Math;
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

        Editor m_ShapeEditor;

        // plane of interaction
        internal Plane m_Plane;
        internal Vector3 m_Forward;
        internal Vector3 m_Right;
        internal Vector3 m_Origin, m_OppositeCorner, m_HeightCorner;

        Quaternion m_Rotation;
        Bounds m_Bounds;

        readonly GUIContent k_ShapeTitle = new GUIContent("Draw Shape");

        internal static TypeCache.TypeCollection s_AvailableShapeTypes;
        internal static Pref<int> s_ActiveShapeIndex = new Pref<int>("ShapeBuilder.ActiveShapeIndex", 0);
        internal static Pref<Vector3> s_Size = new Pref<Vector3>("ShapeBuilder.Size", Vector3.one * 100);

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

        void RecalculateBounds()
        {
            var forward = HandleUtility.PointOnLineParameter(m_OppositeCorner, m_Origin, m_Forward);
            var right = HandleUtility.PointOnLineParameter(m_OppositeCorner, m_Origin, m_Right);

            var direction = m_HeightCorner - m_OppositeCorner;
            var height = direction.magnitude * Mathf.Sign(Vector3.Dot(m_Plane.normal, direction));

            m_Bounds.center = ((m_OppositeCorner + m_Origin) * .5f) + m_Plane.normal * (height * .5f);
            m_Bounds.size = new Vector3(forward, height, right);
            m_Rotation = Quaternion.identity;
        }

        internal void RebuildShape()
        {
            RecalculateBounds();

            if (m_Bounds.size.sqrMagnitude < .01f)
                return;

            if (!m_IsShapeInit)
            {
                m_Shape.shape = EditorShapeUtility.GetLastParams(m_Shape.shape.GetType());
                m_Shape.gameObject.hideFlags = HideFlags.None;
                UndoUtility.RegisterCreatedObjectUndo(m_Shape.gameObject, "Draw Shape");
            }

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
            using (new Handles.DrawingScope(new Color(.2f, .4f, .8f, 1f), Matrix4x4.TRS(m_Bounds.center, m_Rotation.normalized, Vector3.one)))
            {
                Handles.DrawWireCube(Vector3.zero, m_Bounds.size);
            }
        }

        void OnActiveToolGUI(UObject overlayTarget, SceneView view)
        {
            EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.ArrowPlus);
            EditorGUILayout.HelpBox(L10n.Tr("Click to create the shape. Hold and drag to create the shape while controlling its size."), MessageType.Info);

            if (m_Shape == null)
                return;

            Editor.CreateCachedEditor(m_Shape, typeof(ShapeComponentEditor), ref m_ShapeEditor);
            m_ShapeEditor.OnInspectorGUI();
        }
    }
}

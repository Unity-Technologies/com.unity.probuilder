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
    class DrawShapeTool : EditorTool
    {
        enum InputState
        {
            SelectPlane,
            SetWidthAndDepth,
            SetHeight
        }

        [SerializeField]
        InputState m_InputState;

        ShapeComponent m_Shape;
        SerializedObject m_Object;

        ShapeComponent m_ShapeComponent;
        Editor m_ShapeEditor;

        // plane of interaction
        Plane m_Plane;
        Vector3 m_Forward;
        Vector3 m_Right;
        Vector3 m_Origin, m_OppositeCorner, m_HeightCorner;

        Quaternion m_Rotation;
        Bounds m_Bounds;

        bool m_IsDragging;
        bool m_IsInit;
        GUIContent m_ShapeTitle;

        static TypeCache.TypeCollection s_AvailableShapeTypes;
        static Pref<int> s_ActiveShapeIndex = new Pref<int>("ShapeBuilder.ActiveShapeIndex", 0);
        static Pref<Vector3> s_Size = new Pref<Vector3>("ShapeBuilder.Size", Vector3.one * 100);

        static Type activeShapeType
        {
            get { return s_ActiveShapeIndex < 0 ? typeof(Cube) : s_AvailableShapeTypes[s_ActiveShapeIndex]; }
        }

        static DrawShapeTool()
        {
            s_AvailableShapeTypes = TypeCache.GetTypesDerivedFrom<Shape>();
        }

        void OnEnable()
        {
            InitNewShape();
            ToolManager.activeToolChanged += ActiveToolChanged;
            m_ShapeTitle = new GUIContent("Draw Shape");
        }

        void OnDisable()
        {
            if(m_ShapeEditor != null)
                DestroyImmediate(m_ShapeEditor);
            if (m_Shape.gameObject.hideFlags == HideFlags.HideAndDontSave)
                DestroyImmediate(m_Shape.gameObject);
            ToolManager.activeToolChanged -= ActiveToolChanged;
        }

        void InitNewShape()
        {
            m_IsInit = false;
            m_Shape = new GameObject("Shape", typeof(ShapeComponent)).GetComponent<ShapeComponent>();
            m_Shape.gameObject.hideFlags = HideFlags.HideAndDontSave;
            m_Shape.hideFlags = HideFlags.None;
            m_Shape.SetShape(EditorShapeUtility.CreateShape(activeShapeType));
            m_Object = new SerializedObject(m_Shape);
        }

        void ActiveToolChanged()
        {
            if (ToolManager.IsActiveTool(this))
                m_InputState = InputState.SelectPlane;
        }

        void AdvanceInputState()
        {
            if (m_InputState == InputState.SelectPlane)
                m_InputState = InputState.SetWidthAndDepth;
            else if (m_InputState == InputState.SetWidthAndDepth)
                m_InputState = InputState.SetHeight;
            else
                FinishShape();

            SceneView.RepaintAll();
        }

        void RebuildShape()
        {
            RecalculateBounds();

            if (m_Bounds.size.sqrMagnitude < .01f)
                return;

            if (!m_IsInit)
            {
                m_Shape.shape = EditorShapeUtility.GetLastParams(m_Shape.shape.GetType());
                m_Shape.gameObject.hideFlags = HideFlags.None;
                UndoUtility.RegisterCreatedObjectUndo(m_Shape.gameObject, "Draw Shape");
            }

            m_Shape.Rebuild(m_Bounds, m_Rotation);
            m_Shape.mesh.SetPivot(PivotLocation.Center);
            ProBuilderEditor.Refresh(false);

            if (!m_IsInit)
            {
                EditorUtility.InitObject(m_Shape.mesh);
                m_IsInit = true;
            }
        }

        void FinishShape()
        {
            s_Size.value = m_Shape.size;
            s_ActiveShapeIndex.value = s_AvailableShapeTypes.IndexOf(m_Shape.shape.GetType());
            m_Shape = null;
            InitNewShape();
            m_InputState = InputState.SelectPlane;
        }

        void CancelShape()
        {
            if (m_Shape != null)
                DestroyImmediate(m_Shape.gameObject);
            InitNewShape();
            m_InputState = InputState.SelectPlane;
        }

        public override void OnToolGUI(EditorWindow window)
        {
            SceneViewOverlay.Window(m_ShapeTitle, OnActiveToolGUI, 0, SceneViewOverlay.WindowDisplayOption.OneWindowPerTitle);

            if (m_InputState != InputState.SelectPlane && m_IsDragging)
                DrawBoundingBox();

            var evt = Event.current;

            if (EditorHandleUtility.SceneViewInUse(evt))
                return;

            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            HandleUtility.AddDefaultControl(controlID);

            switch (m_InputState)
            {
                case InputState.SelectPlane:
                    SelectPlane(evt);
                    break;
                case InputState.SetWidthAndDepth:
                    SetWidthAndDepth(evt);
                    break;
                case InputState.SetHeight:
                    SetHeight(evt);
                    break;
            }
        }

        void SelectPlane(Event evt)
        {
            if (evt.type == EventType.MouseDown)
            {
                m_IsDragging = false;
                var res = EditorHandleUtility.FindBestPlaneAndBitangent(evt.mousePosition);

                Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                float hit;

                if (res.item1.Raycast(ray, out hit))
                {
                    m_Plane = res.item1;
                    m_Forward = res.item2;
                    m_Right = Vector3.Cross(m_Plane.normal, m_Forward);
                    m_Origin = ray.GetPoint(hit);
                    m_HeightCorner = m_Origin;
                    m_OppositeCorner = m_Origin;
                    AdvanceInputState();
                }
            }
        }

        /// <summary>
        /// Calculates the rotation angles to give a shape depending on the orientation we started drawing it
        /// </summary>
        /// <param name="diff">Difference between point A and point B</param>
        /// <returns></returns>
        Quaternion ToRotationAngles(Vector3 diff)
        {
            Vector3 angles = Vector3.zero;
            if (diff.y < 0)
            {
                angles.z = -180f;
            }
            if (diff.z < 0)
            {
                angles.y = -180f;
            }
            return Quaternion.Euler(angles);
        }

        void SetWidthAndDepth(Event evt)
        {
            switch (evt.type)
            {
                case EventType.MouseDrag:
                    {
                        m_IsDragging = true;
                        Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                        float distance;

                        if (m_Plane.Raycast(ray, out distance))
                        {
                            m_OppositeCorner = ray.GetPoint(distance);
                            m_HeightCorner = m_OppositeCorner;
                            RebuildShape();
                            SceneView.RepaintAll();
                        }
                        break;
                    }

                case EventType.MouseUp:
                    {
                        if (!m_IsDragging)
                        {
                            m_IsDragging = true;
                            Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                            float distance;

                            if (m_Plane.Raycast(ray, out distance))
                            {
                                var pos = ray.GetPoint(distance);
                                CancelShape();
                                var shape = CreateLastShape();
                                shape.transform.position = pos;
                            }
                        }
                        else if (Vector3.Distance(m_OppositeCorner, m_Origin) < .1f)
                            CancelShape();
                        else
                            AdvanceInputState();
                        break;

                    }
            }
        }

        public static ProBuilderMesh CreateLastShape()
        {
            var type = activeShapeType;
            var shape = ShapeGenerator.CreateShape(type).GetComponent<ShapeComponent>();
            shape.shape = EditorShapeUtility.GetLastParams(shape.shape.GetType());
            UndoUtility.RegisterCreatedObjectUndo(shape.gameObject, "Create Shape");

            Bounds bounds = new Bounds(Vector3.zero, s_Size);
            shape.Rebuild(bounds, Quaternion.identity);
            ProBuilderEditor.Refresh(false);

            var res = shape.GetComponent<ProBuilderMesh>();
            EditorUtility.InitObject(res);
            return res;
        }

        void SetHeight(Event evt)
        {
            switch (evt.type)
            {
                case EventType.MouseMove:
                case EventType.MouseDrag:
                    {
                        Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                        m_HeightCorner = Math.GetNearestPointRayRay(m_OppositeCorner, m_Plane.normal, ray.origin, ray.direction);

                        var diff = m_HeightCorner - m_Origin;
                        if (m_Shape != null)
                            m_Shape.SetRotation(ToRotationAngles(diff));
                        RebuildShape();

                        SceneView.RepaintAll();
                        break;
                    }

                case EventType.MouseUp:
                    {
                        RebuildShape();
                        AdvanceInputState();
                        break;
                    }
            }
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

        void DrawBoundingBox()
        {
            using (new Handles.DrawingScope(new Color(.2f, .4f, .8f, 1f), Matrix4x4.TRS(m_Bounds.center, m_Rotation, Vector3.one)))
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

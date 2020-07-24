using System;
using System.Linq;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using Math = UnityEngine.ProBuilder.Math;
using UObject = UnityEngine.Object;

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

        // plane of interaction
        Plane m_Plane;
        Vector3 m_Forward;
        Vector3 m_Right;
        Vector3 m_Origin, m_OppositeCorner, m_HeightCorner;

        Quaternion m_Rotation;
        Bounds m_Bounds;

        bool m_IsDragging;

        GUIContent m_ShapeTitle;

        static TypeCache.TypeCollection m_AvailableShapeTypes;
        string[] m_ShapeTypesPopupContent;

        [SerializeField]
        static int m_ActiveShapeIndex;

        static Vector3 m_Size;

        static ScriptableShape m_ShapeData;
        SerializedObject m_Object;

        static Type activeShapeType {
            get { return m_ActiveShapeIndex < 0 ? typeof(Cube) : m_AvailableShapeTypes[m_ActiveShapeIndex]; }
        }

        static DrawShapeTool()
        {
            m_AvailableShapeTypes = TypeCache.GetTypesDerivedFrom<Shape>();
        }

        void OnEnable()
        {
            m_ActiveShapeIndex = EditorPrefs.GetInt("ShapeBuilder.ActiveShapeIndex");
            m_ShapeData = ScriptableObject.CreateInstance<ScriptableShape>();
            m_ShapeData.m_shape = CreateShape(activeShapeType);
            m_Object = new SerializedObject(m_ShapeData);

            EditorTools.EditorTools.activeToolChanged += ActiveToolChanged;
            m_ShapeTitle = new GUIContent("Draw Shape");
            m_ShapeTypesPopupContent = m_AvailableShapeTypes.Select(x => x.Name).ToArray();
        }

        void OnDisable()
        {
            DestroyImmediate(m_ShapeData);
            EditorTools.EditorTools.activeToolChanged -= ActiveToolChanged;
        }

        Shape CreateShape(Type type)
        {
            var shape = Activator.CreateInstance(type) as Shape;
            ShapeParameters.SetToLastParams(ref m_ShapeData.m_shape);
            return shape;
        }

        void ActiveToolChanged()
        {
            if (EditorTools.EditorTools.IsActiveTool(this))
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

        void SetActiveShapeType(Type type)
        {
            if (!typeof(Shape).IsAssignableFrom(type))
                throw new ArgumentException("type must inherit UnityEngine.ProBuilder.Shape", "type");

            m_ActiveShapeIndex = m_AvailableShapeTypes.IndexOf(type);
            EditorPrefs.SetInt("ShapeBuilder.ActiveShapeIndex", m_ActiveShapeIndex);

            if (m_ActiveShapeIndex < 0)
                throw new Exception("type must inherit UnityEngine.ProBuilder.Shape");

            if (m_Shape != null)
            {
                DestroyImmediate(m_Shape.gameObject);

                if (m_InputState != InputState.SelectPlane)
                    RebuildShape();
            }
        }

        void RebuildShape()
        {
            RecalculateBounds();

            if (m_Bounds.size.sqrMagnitude < .01f)
                return;

            // The sphere doesn't like being built before its height is set
            if (m_ShapeData.m_shape is Sphere && System.Math.Abs(m_Bounds.size.y) < 0.01f)
                return;

            bool init = false;
            if (m_Shape == null)
            {
                init = true;
                ShapeParameters.SetToLastParams(ref m_ShapeData.m_shape);
                m_Shape = ShapeGenerator.CreateShape(m_ShapeData.m_shape).GetComponent<ShapeComponent>();
                UndoUtility.RegisterCreatedObjectUndo(m_Shape.gameObject, "Draw Shape");
            }

            m_Shape.Rebuild(m_Bounds, m_Rotation);
            m_Shape.mesh.SetPivot(PivotLocation.Center);
            ProBuilderEditor.Refresh(false);

            if (init)
                EditorUtility.InitObject(m_Shape.mesh, false);
        }

        void FinishShape()
        {
            m_Shape = null;
            m_InputState = InputState.SelectPlane;
        }

        void CancelShape()
        {
            if (m_Shape != null)
                DestroyImmediate(m_Shape.gameObject);
            m_InputState = InputState.SelectPlane;
        }

        public override void OnToolGUI(EditorWindow window)
        {
            SceneViewOverlay.Window(m_ShapeTitle, OnActiveToolGUI, 0, SceneViewOverlay.WindowDisplayOption.OneWindowPerTitle);

            if (m_InputState != InputState.SelectPlane)
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
        Vector3 ToRotationAngles(Vector3 diff)
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
            return angles;
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
                            var diff = m_OppositeCorner - m_Origin;
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
                                var shape = CreateLastShape(Vector3.one * distance / 5f);
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

        public static ProBuilderMesh CreateLastShape(Vector3 defaultSize)
        {
            if (m_Size == Vector3.zero)
                m_Size = defaultSize;
            var type = activeShapeType;
            var shape = ShapeGenerator.CreateShape(type).GetComponent<ShapeComponent>();
            UndoUtility.RegisterCreatedObjectUndo(shape.gameObject, "Create Shape");

            Bounds bounds = new Bounds(Vector3.zero, m_Size);
            shape.Rebuild(bounds, Quaternion.identity);
            shape.mesh.SetPivot(PivotLocation.Center);
            ProBuilderEditor.Refresh(false);

            var res = shape.GetComponent<ProBuilderMesh>();
            EditorUtility.InitObject(res, false);
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
            var fo = HandleUtility.PointOnLineParameter(m_OppositeCorner, m_Origin, m_Forward);
            var ri = HandleUtility.PointOnLineParameter(m_OppositeCorner, m_Origin, m_Right);

            var direction = m_HeightCorner - m_OppositeCorner;
            var height = direction.magnitude * Mathf.Sign(Vector3.Dot(m_Plane.normal, direction));

            m_Bounds.center = ((m_OppositeCorner + m_Origin) * .5f) + m_Plane.normal * (height * .5f);
            m_Bounds.size = new Vector3(fo, height, ri);
            m_Rotation = Quaternion.identity;
        }

        void DrawBoundingBox()
        {
            using (new Handles.DrawingScope(new Color(.2f, .4f, .8f, 1f)))
            {
                EditorHandleUtility.PushMatrix();
                Handles.matrix = Matrix4x4.TRS(m_Bounds.center, m_Rotation, Vector3.one);
                Handles.DrawWireCube(Vector3.zero, m_Bounds.size);
                EditorHandleUtility.PopMatrix();
            }
        }

        void OnActiveToolGUI(UObject target, SceneView view)
        {
            if (Selection.activeGameObject != null && Selection.activeGameObject.GetComponent<ShapeComponent>() != null && m_Shape == null)
            {
                DrawShapeGUI(false);
            }
            else
            {
                DrawShapeGUI(true);
            }
            // Use differents arrows when dragging (resize etc)
            EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.ArrowPlus);
            var rect = EditorGUILayout.GetControlRect(false, 45);
            EditorGUI.HelpBox(rect, "Click to create the shape. Hold and drag to create the shape while controlling its size.", MessageType.Info);
        }

        void DrawShapeGUI(bool isNew)
        {
            m_Object.Update();
            EditorGUI.BeginChangeCheck();

            var shapeComp = isNew ? m_Shape : Selection.activeTransform?.gameObject.GetComponent<ShapeComponent>();
            var shape = shapeComp.m_Shape;
            var serObject = isNew ? m_Object : new SerializedObject(shapeComp);
            var shapeProperty = serObject.FindProperty("m_shape");

            if (isNew)
            {
                m_ActiveShapeIndex = EditorGUILayout.Popup(m_ActiveShapeIndex, m_ShapeTypesPopupContent);
            }
            else
            {
                m_ActiveShapeIndex = m_AvailableShapeTypes.IndexOf(shape.GetType());
                m_ActiveShapeIndex = EditorGUILayout.Popup(m_ActiveShapeIndex, m_ShapeTypesPopupContent);
            }
            EditorPrefs.SetInt("ShapeBuilder.ActiveShapeIndex", m_ActiveShapeIndex);

            if (EditorGUI.EndChangeCheck())
            {
                var type = m_AvailableShapeTypes[m_ActiveShapeIndex];
                SetActiveShapeType(type);
                var temp = CreateShape(type);

                if (isNew)
                {
                    m_ShapeData.m_shape = temp;
                }
                else
                {
                    shapeComp.SetShape(temp);
                }
                UndoUtility.RegisterCompleteObjectUndo(m_ShapeData, "Change Shape");
            }

            if (isNew)
            {
                m_Size = EditorGUILayout.Vector3Field("Size", m_Size);
            }
            else
            {
                shapeComp.size = EditorGUILayout.Vector3Field("Size", shapeComp.size);
                m_Size = shapeComp.size;
            }


            EditorGUILayout.PropertyField(shapeProperty, true);
            if (serObject.ApplyModifiedProperties())
            {
                ShapeParameters.SaveParams(shapeComp.m_Shape);
                if (shapeComp != null)
                {
                    shapeComp.Rebuild();
                    ProBuilderEditor.Refresh(false);
                }
            }
            m_ShapeData.m_shape = shape;
        }
    }
}
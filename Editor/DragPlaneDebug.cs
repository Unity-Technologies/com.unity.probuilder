using System;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace SlideyDebuggey
{

    class DragPlaneDebug : EditorWindow
    {
        [MenuItem("Window/Drag Plane Debug")]
        static void inti()
        {
            GetWindow<DragPlaneDebug>();
        }

        void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            m_ConstraintRotation = Quaternion.identity;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        [SerializeField]
        Camera m_Camera;
        [SerializeField]
        Quaternion m_ConstraintRotation = Quaternion.identity;
        [SerializeField]
        bool m_UseSceneCamera = true;

        Vector3 m_HandlePosition;
        static bool s_ModifyRotation;

        [ClutchShortcut("Modify Plane Rotation", typeof(SceneView), KeyCode.C)]
        static void ModifyPlaneRotation(ShortcutArguments args)
        {
            s_ModifyRotation = args.stage == ShortcutStage.Begin;
            SceneView.RepaintAll();
        }

        [Shortcut("Toggle Projection Method", typeof(SceneView), KeyCode.B)]
        static void ToggleProjection()
        {
            HandleUtility2.projectionMethod = InternalUtility.NextEnumValue(HandleUtility2.projectionMethod);
            SceneView.RepaintAll();
        }

        void OnSceneGUI(SceneView view)
        {
            m_Camera = m_UseSceneCamera ? view.camera : Camera.main;

            if (m_Camera == null)
                return;

            if (s_ModifyRotation)
                m_ConstraintRotation = Handles.RotationHandle(m_ConstraintRotation, m_HandlePosition);

            var constraintDir = m_ConstraintRotation * Vector3.forward;

            Handles.BeginGUI();
            GUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(256));
            var s_ConstraintDir = m_ConstraintRotation * Vector3.forward;
            HandleUtility2.projectionMethod = (ProjectionMethod)EditorGUILayout.EnumPopup(HandleUtility2.projectionMethod);
            EditorGUILayout.Vector3Field("Constraint Direction", s_ConstraintDir);
            var labelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 200;
            HandleUtility2.s_VisualizeHitPlane = EditorGUILayout.Toggle("Visualize Intersection Plane", HandleUtility2.s_VisualizeHitPlane);
            HandleUtility2.s_VisualizeHitTesting = EditorGUILayout.Toggle("Visualize Mouse Intersection", HandleUtility2.s_VisualizeHitTesting);
            EditorGUIUtility.labelWidth = labelWidth;
            GUILayout.Space(8);
            if (GUILayout.Button("Reset Constraint"))
            {
                m_HandlePosition = Vector3.zero;
                m_ConstraintRotation = Quaternion.identity;
            }

            GUILayout.EndVertical();
            Handles.EndGUI();

            if (!s_ModifyRotation && Event.current.type == EventType.Repaint)
                HandleUtility2.CalcPositionWithConstraint(m_Camera, Event.current.mousePosition, m_HandlePosition, constraintDir);

            m_HandlePosition = PositionHandle.DoPositionHandle(m_HandlePosition, m_ConstraintRotation);

            Handles.DrawLine(m_HandlePosition - s_ConstraintDir * 1000f, m_HandlePosition + s_ConstraintDir * 1000f);
            Handles.color = Color.white;
            EditorMeshHandles.DrawGizmo(Vector3.zero, m_ConstraintRotation);

            if (Event.current.type == EventType.MouseMove)
                view.Repaint();
        }
    }
}

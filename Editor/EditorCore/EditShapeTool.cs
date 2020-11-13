using System;
using System.Linq;
using UnityEngine;
using UnityEditor.EditorTools;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;
using Math = UnityEngine.ProBuilder.Math;
using Object = UnityEngine.Object;

namespace UnityEditor.ProBuilder
{
    [EditorTool("Edit Shape", typeof(ShapeComponent))]
    sealed class EditShapeTool: BoxManipulationTool
    {
        Editor m_ShapeEditor;

        void OnEnable()
        {
            InitTool();
            m_OverlayTitle = new GUIContent("Edit Shape Tool");
            m_BoundsHandleColor = new Color(.2f, .4f, .8f, 1f);
        }

        void OnDisable()
        {
            if(m_ShapeEditor != null)
                DestroyImmediate(m_ShapeEditor);
        }

        public override void OnToolGUI(EditorWindow window)
        {
            base.OnToolGUI(window);

            foreach(var obj in targets)
            {
                var shape = obj as ShapeComponent;

                if (shape != null && !shape.edited)
                {
                    if(m_BoundsHandleActive && GUIUtility.hotControl == k_HotControlNone)
                        EndBoundsEditing();

                    if(Mathf.Approximately(shape.transform.lossyScale.sqrMagnitude, 0f))
                        return;

                    DoManipulationGUI(shape);
                }
            }
        }

        protected override void OnOverlayGUI(Object obj, SceneView view)
        {
            Editor.CreateCachedEditor(targets.ToArray(), typeof(ShapeComponentEditor), ref m_ShapeEditor);
            ( (ShapeComponentEditor) m_ShapeEditor ).DrawShapeGUI(null);
            ( (ShapeComponentEditor) m_ShapeEditor ).DrawShapeParametersGUI(null);

            EditorSnapSettings.gridSnapEnabled = EditorGUILayout.Toggle("Snap To Grid", EditorSnapSettings.gridSnapEnabled);

            m_snapAngle = EditorGUILayout.IntSlider(m_SnapAngleContent, m_snapAngle, 1, 90);
        }

        protected override void DoManipulationGUI(Object toolTarget)
        {
            ShapeComponent shapeComponent = toolTarget as ShapeComponent;
            if(shapeComponent == null)
                return;

            var matrix = IsEditing
                ? m_ActiveBoundsState.positionAndRotationMatrix
                : Matrix4x4.TRS(shapeComponent.transform.position, shapeComponent.transform.rotation, Vector3.one);

            using (new Handles.DrawingScope(matrix))
            {
                m_BoundsHandle.SetColor(m_BoundsHandleColor);

                EditorShapeUtility.CopyColliderPropertiesToHandle(
                    shapeComponent.transform, shapeComponent.mesh.mesh.bounds,
                    m_BoundsHandle, IsEditing, m_ActiveBoundsState);

                EditorGUI.BeginChangeCheck();

                m_BoundsHandle.DrawHandle();

                if (EditorGUI.EndChangeCheck())
                {
                    BeginBoundsEditing(shapeComponent.mesh);
                    UndoUtility.RegisterCompleteObjectUndo(shapeComponent, "Scale Shape");
                    EditorShapeUtility.CopyHandlePropertiesToCollider(m_BoundsHandle, m_ActiveBoundsState);
                    ApplyProperties(shapeComponent, m_ActiveBoundsState);
                }

                DoRotateHandlesGUI(toolTarget, shapeComponent.mesh, shapeComponent.meshFilterBounds);
            }
        }

        protected override void UpdateTargetRotation(Object toolTarget, Quaternion rotation)
        {
            var shapeComponent = toolTarget as ShapeComponent;
            if(shapeComponent == null)
                return;

            UndoUtility.RegisterCompleteObjectUndo(shapeComponent, "Rotate Shape");
            shapeComponent.RotateInsideBounds(rotation);
            ProBuilderEditor.Refresh();
        }

        public static void ApplyProperties(ShapeComponent shape, EditorShapeUtility.BoundsState activeBoundsState)
        {
            var bounds = new Bounds();
            var trs = shape.transform;

            bounds.center = Handles.matrix.MultiplyPoint3x4(activeBoundsState.boundsHandleValue.center);
            bounds.size = Math.Abs(Vector3.Scale(activeBoundsState.boundsHandleValue.size, Math.InvertScaleVector(trs.lossyScale)));

            shape.Rebuild(bounds, shape.transform.rotation);
            shape.mesh.SetPivot(shape.transform.position);
            ProBuilderEditor.Refresh(false);
        }

    }
}

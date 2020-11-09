using UnityEngine;
using UnityEditor.EditorTools;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;
using Math = UnityEngine.ProBuilder.Math;

namespace UnityEditor.ProBuilder
{
    [EditorTool("Edit Shape", typeof(ShapeComponent))]
    sealed class EditShapeTool: BoxManipulationTool
    {
        void OnEnable()
        {
            InitTool();
            m_OverlayTitle = new GUIContent("Edit Shape Tool");
            m_BoundsHandleColor = new Color(.2f, .4f, .8f, 1f);
        }

        public override void OnToolGUI(EditorWindow window)
        {
            base.OnToolGUI(window);

            foreach (var obj in targets)
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

        protected override void OnOverlayGUI(Object target, SceneView view)
        {
            foreach(var obj in targets)
            {
                var shapeComponent = obj as ShapeComponent;
                if(shapeComponent.edited)
                {
                    EditorGUILayout.HelpBox(
                        L10n.Tr(
                            "You have manually modified one or more of the selected Shapes. Revert manual changes to use the tool."),
                        MessageType.Info);
                    break;
                }
            }

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

                CopyColliderPropertiesToHandle(shapeComponent.transform, shapeComponent.mesh.mesh.bounds);

                EditorGUI.BeginChangeCheck();

                m_BoundsHandle.DrawHandle();

                if (EditorGUI.EndChangeCheck())
                {
                    BeginBoundsEditing(shapeComponent.mesh);
                    UndoUtility.RegisterCompleteObjectUndo(shapeComponent, "Scale Shape");
                    CopyHandlePropertiesToCollider(shapeComponent);
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

        void CopyHandlePropertiesToCollider(ShapeComponent shape)
        {
            Vector3 snappedHandleSize = ProBuilderSnapping.Snap(m_BoundsHandle.size, EditorSnapping.activeMoveSnapValue);
            //Find the scaling direction
            Vector3 centerDiffSign = ( m_BoundsHandle.center - m_ActiveBoundsState.boundsHandleValue.center ).normalized;
            Vector3 sizeDiffSign = ( m_BoundsHandle.size - m_ActiveBoundsState.boundsHandleValue.size ).normalized;
            Vector3 globalSign = Vector3.Scale(centerDiffSign,sizeDiffSign);
            //Set the center to the right position
            Vector3 center = m_ActiveBoundsState.boundsHandleValue.center + Vector3.Scale((snappedHandleSize - m_ActiveBoundsState.boundsHandleValue.size)/2f,globalSign);
            //Set new Bounding box value
            m_ActiveBoundsState.boundsHandleValue = new Bounds(center, snappedHandleSize);

            var bounds = new Bounds();
            var trs = shape.transform;

            bounds.center = Handles.matrix.MultiplyPoint3x4(m_ActiveBoundsState.boundsHandleValue.center);
            bounds.size = Math.Abs(Vector3.Scale(m_ActiveBoundsState.boundsHandleValue.size, Math.InvertScaleVector(trs.lossyScale)));

            shape.Rebuild(bounds, shape.transform.rotation);
            shape.mesh.SetPivot(shape.transform.position);
            ProBuilderEditor.Refresh(false);
        }

    }
}

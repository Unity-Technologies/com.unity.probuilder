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
        const int k_HotControlNone = 0;

        bool IsEditing => m_BoundsHandleActive;

        void OnEnable()
        {
            InitTool();
            m_OverlayTitle = new GUIContent("Edit Shape Tool");
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
                m_BoundsHandle.SetColor(Handles.s_PreselectionColor);

                CopyColliderPropertiesToHandle(shapeComponent);

                EditorGUI.BeginChangeCheck();

                m_BoundsHandle.DrawHandle();

                if (EditorGUI.EndChangeCheck())
                {
                    BeginBoundsEditing(shapeComponent.mesh);
                    UndoUtility.RegisterCompleteObjectUndo(shapeComponent, "Scale Shape");
                    CopyHandlePropertiesToCollider(shapeComponent);
                }

                DoRotateHandlesGUI(shapeComponent, shapeComponent.mesh, shapeComponent.meshFilterBounds);
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


        static Vector3 TransformColliderCenterToHandleSpace(Matrix4x4 localToWorldMatrix, Vector3 colliderCenter)
        {
            return Handles.inverseMatrix * (localToWorldMatrix * colliderCenter);
        }

        void CopyColliderPropertiesToHandle(ShapeComponent shape)
        {
            // when editing a shape, we don't bother doing the conversion from handle space bounds to model for the
            // active handle
            if (IsEditing)
            {
                m_BoundsHandle.center = m_ActiveBoundsState.boundsHandleValue.center;
                m_BoundsHandle.size = m_ActiveBoundsState.boundsHandleValue.size;
                return;
            }

            var bounds = shape.mesh.mesh.bounds;
            var trs = shape.transform.localToWorldMatrix;
            var lossyScale = shape.transform.lossyScale;

            m_BoundsHandle.center = TransformColliderCenterToHandleSpace(trs, bounds.center);
            m_BoundsHandle.size = Vector3.Scale(bounds.size, lossyScale);
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

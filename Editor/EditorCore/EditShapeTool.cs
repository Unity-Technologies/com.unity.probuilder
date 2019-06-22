using System;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEngine.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using Math = UnityEngine.ProBuilder.Math;

namespace UnityEditor.ProBuilder
{
    public abstract class EditShapeTool<T> : EditorTool where T : Shape
    {
        const int k_HotControlNone = 0;
        BoxBoundsHandle m_BoundsHandle;
        bool m_BoundsHandleActive;

        // Don't recalculate the active bounds during an edit operation, it causes the handles to drift
        ShapeState m_ActiveShapeState;

        protected struct ShapeState
        {
            public Shape shape;
            public Matrix4x4 localToWorldMatrix;
            public Matrix4x4 positionAndRotationMatrix;
            public Bounds boundsHandleValue;
            // bounds in world space position, with size
            public Bounds originalBounds;
            // rotation in world space
            public Quaternion originalRotation;
        }

        public override GUIContent toolbarIcon
        {
            get { return PrimitiveBoundsHandle.editModeButton; }
        }

        bool IsEditing(Shape shape)
        {
            return m_BoundsHandleActive && shape == m_ActiveShapeState.shape;
        }

        void OnEnable()
        {
            m_BoundsHandle = new BoxBoundsHandle();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            foreach (var obj in targets)
            {
                var shape = obj as T;

                if (shape != null)
                {
                    if (m_BoundsHandleActive && GUIUtility.hotControl == k_HotControlNone)
                        EndBoundsEditing();

                    if (Mathf.Approximately(shape.transform.lossyScale.sqrMagnitude, 0f))
                        return;

                    EditorGUI.BeginChangeCheck();

                    if(IsEditing(shape))
                        DoShapeGUI(shape, m_ActiveShapeState.localToWorldMatrix, m_ActiveShapeState.originalBounds);
                    else
                        DoShapeGUI(shape, shape.transform.localToWorldMatrix, shape.meshFilterBounds);

                    if(EditorGUI.EndChangeCheck())
                        BeginBoundsEditing(shape);
                }
            }
        }

        protected virtual void DoShapeGUI(T shape, Matrix4x4 localToWorldMatrix, Bounds bounds)
        {
            var matrix = IsEditing(shape)
                ? m_ActiveShapeState.positionAndRotationMatrix
                : Matrix4x4.TRS(shape.transform.position, shape.transform.rotation, Vector3.one);

            using (new Handles.DrawingScope(matrix))
            {
                m_BoundsHandle.SetColor(Handles.s_ColliderHandleColor);

                CopyColliderPropertiesToHandle(shape);

                EditorGUI.BeginChangeCheck();

                m_BoundsHandle.DrawHandle();

                if (EditorGUI.EndChangeCheck())
                {
                    BeginBoundsEditing(shape);
                    CopyHandlePropertiesToCollider(shape);
                }
            }
        }

        void BeginBoundsEditing(Shape shape)
        {
            if (m_BoundsHandleActive)
                return;

            UndoUtility.RecordComponents<ProBuilderMesh, Transform>(
                new[] { shape },
                string.Format("Modify {0}", ObjectNames.NicifyVariableName(target.GetType().Name)));

            m_BoundsHandleActive = true;

            var localBounds = shape.mesh.mesh.bounds;

            m_ActiveShapeState = new ShapeState()
            {
                shape = shape,
                localToWorldMatrix = shape.transform.localToWorldMatrix,
                positionAndRotationMatrix = Matrix4x4.TRS(shape.transform.position, shape.transform.rotation, Vector3.one),
                boundsHandleValue = localBounds,
                originalBounds = new Bounds(shape.transform.TransformPoint(localBounds.center), shape.size),
                originalRotation = shape.transform.rotation
            };
        }

        void EndBoundsEditing()
        {
            m_BoundsHandleActive = false;
        }

        static Vector3 TransformColliderCenterToHandleSpace(Matrix4x4 localToWorldMatrix, Vector3 colliderCenter)
        {
            return Handles.inverseMatrix * (localToWorldMatrix * colliderCenter);
        }

        static Vector3 InvertScaleVector(Vector3 scaleVector)
        {
            for (int axis = 0; axis < 3; ++axis)
                scaleVector[axis] = scaleVector[axis] == 0f ? 0f : 1f / scaleVector[axis];
            return scaleVector;
        }

        void CopyColliderPropertiesToHandle(Shape shape)
        {
            // when editing a shape, we don't bother doing the conversion from handle space bounds to model for the
            // active handle
            if (IsEditing(shape))
            {
                m_BoundsHandle.center = m_ActiveShapeState.boundsHandleValue.center;
                m_BoundsHandle.size = m_ActiveShapeState.boundsHandleValue.size;
                return;
            }

            var bounds = shape.mesh.mesh.bounds;
            var trs = shape.transform.localToWorldMatrix;
            var lossyScale = shape.transform.lossyScale;

            m_BoundsHandle.center = TransformColliderCenterToHandleSpace(trs, bounds.center);
            m_BoundsHandle.size = Vector3.Scale(bounds.size, lossyScale);
        }

        void CopyHandlePropertiesToCollider(Shape shape)
        {
            m_ActiveShapeState.boundsHandleValue = new Bounds(m_BoundsHandle.center, m_BoundsHandle.size);

            var bounds = new Bounds();

            var trs = shape.transform;

            bounds.center = Handles.matrix.MultiplyPoint3x4(m_BoundsHandle.center);
            bounds.size = Math.Abs(Vector3.Scale(m_BoundsHandle.size, InvertScaleVector(trs.lossyScale)));

            shape.Rebuild(bounds, shape.transform.rotation);
            shape.mesh.SetPivot(EditorUtility.newShapePivotLocation);
            ProBuilderEditor.Refresh(false);
        }

        protected void RebuildShape(Shape shape, Bounds bounds, Quaternion rotation)
        {
            shape.Rebuild(bounds, rotation);
            shape.mesh.SetPivot(EditorUtility.newShapePivotLocation);
            ProBuilderEditor.Refresh();
        }
    }
}

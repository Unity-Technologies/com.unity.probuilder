using System;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEngine.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using Math = UnityEngine.ProBuilder.Math;

namespace UnityEditor.ProBuilder
{
    [EditorTool("Edit Shape", typeof(ShapeComponent))]
    public sealed class EditShapeTool: EditorTool
    {
        const int k_HotControlNone = 0;
        BoxBoundsHandle m_BoundsHandle;
        bool m_BoundsHandleActive;

        // Don't recalculate the active bounds during an edit operation, it causes the handles to drift
        ShapeState m_ActiveShapeState;

        struct ShapeState
        {
            public ShapeComponent shape;
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

        bool IsEditing => m_BoundsHandleActive;

        void OnEnable()
        {
            m_BoundsHandle = new BoxBoundsHandle();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            foreach (var obj in targets)
            {
                var shape = obj as ShapeComponent;

                if (shape != null)
                {
                    if (m_BoundsHandleActive && GUIUtility.hotControl == k_HotControlNone)
                        EndBoundsEditing();

                    if (Mathf.Approximately(shape.transform.lossyScale.sqrMagnitude, 0f))
                        return;

                    DoShapeGUI(shape);
                }
            }
        }

        void DoShapeGUI(ShapeComponent shape)
        {
            var matrix = IsEditing
                ? m_ActiveShapeState.positionAndRotationMatrix
                : Matrix4x4.TRS(shape.transform.position, shape.transform.rotation, Vector3.one);

            using (new Handles.DrawingScope(matrix))
            {
                CopyColliderPropertiesToHandle(shape);

                if(!IsEditing)
                {
                    EditorGUI.BeginChangeCheck();

                    Quaternion rot = Handles.RotationHandle(shape.rotation, Vector3.zero);

                    if(EditorGUI.EndChangeCheck())
                    {
                        shape.SetRotation(rot);
                        ProBuilderEditor.Refresh();
                    }
                }

                m_BoundsHandle.SetColor(Handles.s_ColliderHandleColor);

                EditorGUI.BeginChangeCheck();

                m_BoundsHandle.DrawHandle();

                if (EditorGUI.EndChangeCheck())
                {
                    BeginBoundsEditing(shape);
                    CopyHandlePropertiesToCollider(shape);
                }

            }
        }

        void BeginBoundsEditing(ShapeComponent shape)
        {
            if (m_BoundsHandleActive)
                return;

            m_BoundsHandleActive = true;

            UndoUtility.RecordComponents<ProBuilderMesh, Transform>(
                new[] { shape },
                string.Format("Modify {0}", ObjectNames.NicifyVariableName(target.GetType().Name)));

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

        void CopyColliderPropertiesToHandle(ShapeComponent shape)
        {
            // when editing a shape, we don't bother doing the conversion from handle space bounds to model for the
            // active handle
            if (IsEditing)
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

        void CopyHandlePropertiesToCollider(ShapeComponent shape)
        {
            m_ActiveShapeState.boundsHandleValue = new Bounds(m_BoundsHandle.center, m_BoundsHandle.size);

            var bounds = new Bounds();

            var trs = shape.transform;

            bounds.center = Handles.matrix.MultiplyPoint3x4(m_BoundsHandle.center);
            bounds.size = Math.Abs(Vector3.Scale(m_BoundsHandle.size, Math.InvertScaleVector(trs.lossyScale)));

            shape.Rebuild(bounds, shape.transform.rotation);
            shape.mesh.SetPivot(shape.transform.position);
            ProBuilderEditor.Refresh(false);
        }

    }
}

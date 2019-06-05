using System;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEngine.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder
{
    // todo Don't recalculate bounds during an edit operation, it causes the handles to drift
    [EditorTool("Edit Shape", typeof(Shape))]
    public class EditShapeTool : EditorTool
    {
        BoxBoundsHandle m_BoundsHandle;
        bool m_EditingBounds;

        public override GUIContent toolbarIcon
        {
            get { return PrimitiveBoundsHandle.editModeButton; }
        }

        void OnEnable()
        {
            m_BoundsHandle = new BoxBoundsHandle();
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        void OnDisable()
        {
            Undo.undoRedoPerformed += UndoRedoPerformed;
        }

        void UndoRedoPerformed()
        {
            foreach (var shape in targets)
                (shape as Shape).Rebuild();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            foreach (var shape in targets)
            {
                var cast = shape as Shape;

                if (cast != null)
                    DoShapeGUI(cast);
            }
        }

        protected virtual void DoShapeGUI(Shape shape)
        {
            if (Mathf.Approximately(shape.transform.lossyScale.sqrMagnitude, 0f))
                return;

            using (new Handles.DrawingScope(Matrix4x4.TRS(shape.transform.position, shape.transform.rotation, Vector3.one)))
            {
                CopyColliderPropertiesToHandle(shape);

                m_BoundsHandle.SetColor(Handles.s_ColliderHandleColor);

                EditorGUI.BeginChangeCheck();
                
                m_BoundsHandle.DrawHandle();

                if (EditorGUI.EndChangeCheck())
                {
                    UndoUtility.RecordObject(
                        shape,
                        string.Format("Modify {0}", ObjectNames.NicifyVariableName(target.GetType().Name)));
                    CopyHandlePropertiesToCollider(shape);
                }
            }
        }

        static Vector3 TransformColliderCenterToHandleSpace(Transform colliderTransform, Vector3 colliderCenter)
        {
            return Handles.inverseMatrix * (colliderTransform.localToWorldMatrix * colliderCenter);
        }

        static Vector3 InvertScaleVector(Vector3 scaleVector)
        {
            for (int axis = 0; axis < 3; ++axis)
                scaleVector[axis] = scaleVector[axis] == 0f ? 0f : 1f / scaleVector[axis];
            return scaleVector;
        }

        void CopyColliderPropertiesToHandle(Shape shape)
        {
            var bounds = shape.mesh.mesh.bounds;
            var trs = shape.transform;
            m_BoundsHandle.center = TransformColliderCenterToHandleSpace(trs, bounds.center);
            m_BoundsHandle.size = Vector3.Scale(bounds.size, trs.lossyScale);
        }

        void CopyHandlePropertiesToCollider(Shape shape)
        {
            var bounds = new Bounds();

            var trs = shape.transform;
            bounds.center = Handles.matrix.MultiplyPoint3x4(m_BoundsHandle.center);
            Vector3 size = Vector3.Scale(m_BoundsHandle.size, InvertScaleVector(trs.lossyScale));
            size = new Vector3(Mathf.Abs(size.x), Mathf.Abs(size.y), Mathf.Abs(size.z));
            bounds.size = size;

            shape.Rebuild(bounds, shape.transform.rotation);
            shape.mesh.SetPivot(EditorUtility.newShapePivotLocation);
            ProBuilderEditor.Refresh(false);
        }
    }
}

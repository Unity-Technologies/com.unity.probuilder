using System;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEngine.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using Math = UnityEngine.ProBuilder.Math;
using UnityEditor.ProBuilder.Actions;

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

        bool IsEditing(ShapeComponent shape)
        {
            return m_BoundsHandleActive;
        }

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

        void DoShapeGUI(ShapeComponent shape, Matrix4x4 localToWorldMatrix, Bounds bounds)
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
                //update on rotate !

                if (Camera.current != null)
                {
                    //for(int i = 0; i < 3; i++)
                    //{
                    //    var x = 
                    //    for(int j = 0; j < 2; j++)
                    //    {

                    //    }
                    //}

                    //Vector3 worldTangent = Handles.matrix.MultiplyVector(Vector3.up);
                    //Vector3 worldBinormal = Handles.matrix.MultiplyVector(Vector3.forward);
                    //Vector3 worldDir = Vector3.Cross(worldTangent, worldBinormal).normalized;

                    // adjust color if handle is back facing
                    //float cosV;

                    //if (Camera.current.orthographic)
                    //    cosV = Vector3.Dot(-Camera.current.transform.forward, worldDir);
                    //else
                    //    cosV = Vector3.Dot((Camera.current.transform.position - Handles.matrix.MultiplyPoint(m_BoundsHandle.center + new Vector3(bounds.extents.x, 0, 0))).normalized, worldDir);

                    //Debug.Log(cosV);

                    var x = Mathf.CeilToInt(Vector3.Dot(Camera.current.transform.forward, shape.transform.right));
                    var y = Mathf.CeilToInt(Vector3.Dot(Camera.current.transform.forward, shape.transform.up));
                    var z = Mathf.CeilToInt(Vector3.Dot(Camera.current.transform.forward, shape.transform.forward));

                    var angle = 180f;
                    var radius = 1.5f;

                    var pos = m_BoundsHandle.center + new Vector3(bounds.extents.x, 0, 0) * (x >= 1 ? -1f : 1f);
                    var rot = Quaternion.LookRotation(shape.transform.right * (x >= 1 ? 1f : -1f), shape.transform.up);

                    if (RotateBoundsHandle(pos, rot, angle, radius, Handles.xAxisColor))
                    {
                        MirrorObjects.Mirror(shape.GetComponent<ProBuilderMesh>(), new Vector3(-1f, 1f, 1f), false);
                    }

                    pos = m_BoundsHandle.center + new Vector3(0, bounds.extents.y, 0) * (y >= 1 ? -1f : 1f);
                    rot = Quaternion.LookRotation(shape.transform.right * (y >= 1 ? 1f : -1f), shape.transform.forward);

                    if (RotateBoundsHandle(pos, rot, angle, radius, Handles.yAxisColor))
                    {
                        MirrorObjects.Mirror(shape.GetComponent<ProBuilderMesh>(), new Vector3(1f, -1f, 1f), false);
                    }

                    pos = m_BoundsHandle.center + new Vector3(0, 0, bounds.extents.z) * (z >= 1 ? -1f : 1f);
                    rot = Quaternion.LookRotation(shape.transform.up * (z >= 1 ? -1f : 1f), shape.transform.forward);

                    if (RotateBoundsHandle(pos, rot, angle, radius, Handles.zAxisColor))
                    {
                        MirrorObjects.Mirror(shape.GetComponent<ProBuilderMesh>(), new Vector3(1f, 1f, -1f), false);
                    }
                }
            }
        }

        bool RotateBoundsHandle(Vector3 position, Quaternion rotation, float angle, float size, Color handleColor)
        {
            Event evt = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            switch (evt.GetTypeForControl(controlID))
            {
                case EventType.Layout:
                    HandleUtility.AddControl(controlID, HandleUtility.DistanceToArc(position, rotation * Vector3.forward, rotation * Vector3.up, angle, size));
                    break;
                case EventType.Repaint:
                    using (new Handles.DrawingScope(HandleUtility.nearestControl == controlID ? Handles.preselectionColor : handleColor))
                    {
                        Handles.DrawWireArc(position, rotation * Vector3.forward, rotation * Vector3.up, angle, size);
                        break;
                    }
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == controlID && (evt.button == 0 || evt.button == 2))
                    {
                        GUIUtility.hotControl = controlID; // Grab mouse focus
                        evt.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID && (evt.button == 0 || evt.button == 2))
                    {
                        GUIUtility.hotControl = 0;
                        evt.Use();

                        if (HandleUtility.nearestControl == controlID)
                            return true;
                    }
                    break;
                case EventType.MouseMove:

                  //  if (HandleUtility.nearestControl == controlID)
                        HandleUtility.Repaint();
                    break;
            }
            return false;
        }

        void BeginBoundsEditing(ShapeComponent shape)
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

        void CopyColliderPropertiesToHandle(ShapeComponent shape)
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

        void CopyHandlePropertiesToCollider(ShapeComponent shape)
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

        protected void RebuildShape(ShapeComponent shape, Bounds bounds, Quaternion rotation)
        {
            shape.Rebuild(bounds, rotation);
            shape.mesh.SetPivot(EditorUtility.newShapePivotLocation);
            ProBuilderEditor.Refresh();
        }
    }
}

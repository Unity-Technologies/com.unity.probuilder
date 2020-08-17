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

        Vector2 s_StartMousePosition;
        Vector3 s_StartPosition;
        Quaternion s_LastRotation;
        int s_CurrentId = -1;
        bool s_IsMouseDown;

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

        sealed class FaceData
        {
            public Vector3 CenterPosition;
            public Vector3 Normal;
            public EdgeData[] Edges;

            public FaceData()
            {
                Edges = new EdgeData[4];
            }

            public void SetData(Vector3 centerPosition, Vector3 normal)
            {
                CenterPosition = centerPosition;
                Normal = normal;
            }
        }

        struct EdgeData
        {
            public Vector3 PointA;
            public Vector3 PointB;

            public EdgeData(Vector3 pointA, Vector3 pointB)
            {
                PointA = pointA;
                PointB = pointB;
            }
        }

        public override GUIContent toolbarIcon
        {
            get { return PrimitiveBoundsHandle.editModeButton; }
        }

        bool IsEditing()
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

                    if(IsEditing(a))
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
            var matrix = IsEditing()
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

                DoRotateHandlesGUI(shape, m_ActiveShapeState.boundsHandleValue);
            }
        }

        FaceData[] m_Faces = new FaceData[6];


        FaceData[] GetFaces(Vector3 extents)
        {
            for (int i = 0; i < m_Faces.Length; i++)
            {
                m_Faces[i] = new FaceData();
            }

            Vector3 xAxis = Vector3.right;
            Vector3 yAxis = Vector3.up;
            Vector3 zAxis = Vector3.forward;

            // +X
            var pos = m_BoundsHandle.center - new Vector3(extents.x, 0, 0);
            m_Faces[0].SetData(pos, xAxis);
            m_Faces[0].Edges[0] = new EdgeData(new Vector3(-extents.x, extents.y, extents.z), new Vector3(-extents.x, -extents.y, extents.z));
            m_Faces[0].Edges[1] = new EdgeData(new Vector3(-extents.x, extents.y, extents.z), new Vector3(-extents.x, extents.y, -extents.z));
            m_Faces[0].Edges[2] = new EdgeData(new Vector3(-extents.x, -extents.y, -extents.z), new Vector3(-extents.x, -extents.y, extents.z));
            m_Faces[0].Edges[3] = new EdgeData(new Vector3(-extents.x, -extents.y, -extents.z), new Vector3(-extents.x, extents.y, -extents.z));

            // -X
            pos = m_BoundsHandle.center + new Vector3(extents.x, 0, 0);
            m_Faces[1].SetData(pos, -xAxis);
            m_Faces[1].Edges[0] = new EdgeData(new Vector3(extents.x, extents.y, extents.z), new Vector3(extents.x, -extents.y, extents.z));
            m_Faces[1].Edges[1] = new EdgeData(new Vector3(extents.x, extents.y, extents.z), new Vector3(extents.x, extents.y, -extents.z));
            m_Faces[1].Edges[2] = new EdgeData(new Vector3(extents.x, -extents.y, -extents.z), new Vector3(extents.x, -extents.y, extents.z));
            m_Faces[1].Edges[3] = new EdgeData(new Vector3(extents.x, -extents.y, -extents.z), new Vector3(extents.x, extents.y, -extents.z));

            // +Y
            pos = m_BoundsHandle.center - new Vector3(0, extents.y, 0);
            m_Faces[2].SetData(pos, yAxis);
            m_Faces[2].Edges[0] = new EdgeData(new Vector3(extents.x, -extents.y, extents.z), new Vector3(-extents.x, -extents.y, extents.z));
            m_Faces[2].Edges[1] = new EdgeData(new Vector3(extents.x, -extents.y, extents.z), new Vector3(extents.x, -extents.y, -extents.z));
            m_Faces[2].Edges[2] = new EdgeData(new Vector3(-extents.x, -extents.y, -extents.z), new Vector3(-extents.x, -extents.y, extents.z));
            m_Faces[2].Edges[3] = new EdgeData(new Vector3(-extents.x, -extents.y, -extents.z), new Vector3(extents.x, -extents.y, -extents.z));

            // -Y
            pos = m_BoundsHandle.center + new Vector3(0, extents.y, 0);
            m_Faces[3].SetData(pos, -yAxis);
            m_Faces[3].Edges[0] = new EdgeData(new Vector3(extents.x, extents.y, extents.z), new Vector3(-extents.x, extents.y, extents.z));
            m_Faces[3].Edges[1] = new EdgeData(new Vector3(extents.x, extents.y, extents.z), new Vector3(extents.x, extents.y, -extents.z));
            m_Faces[3].Edges[2] = new EdgeData(new Vector3(-extents.x, extents.y, -extents.z), new Vector3(-extents.x, extents.y, extents.z));
            m_Faces[3].Edges[3] = new EdgeData(new Vector3(-extents.x, extents.y, -extents.z), new Vector3(extents.x, extents.y, -extents.z));

            // +Z
            pos = m_BoundsHandle.center - new Vector3(0, 0, extents.z);
            m_Faces[4].SetData(pos, zAxis);
            m_Faces[4].Edges[0] = new EdgeData(new Vector3(extents.x, extents.y, -extents.z), new Vector3(-extents.x, extents.y, -extents.z));
            m_Faces[4].Edges[1] = new EdgeData(new Vector3(extents.x, extents.y, -extents.z), new Vector3(extents.x, -extents.y, -extents.z));
            m_Faces[4].Edges[2] = new EdgeData(new Vector3(-extents.x, -extents.y, -extents.z), new Vector3(-extents.x, extents.y, -extents.z));
            m_Faces[4].Edges[3] = new EdgeData(new Vector3(-extents.x, -extents.y, -extents.z), new Vector3(extents.x, -extents.y, -extents.z));

            // -Z
            pos = m_BoundsHandle.center + new Vector3(0, 0, extents.z);
            m_Faces[5].SetData(pos, -zAxis);
            m_Faces[5].Edges[0] = new EdgeData(new Vector3(extents.x, extents.y, extents.z), new Vector3(-extents.x, extents.y, extents.z));
            m_Faces[5].Edges[1] = new EdgeData(new Vector3(extents.x, extents.y, extents.z), new Vector3(extents.x, -extents.y, extents.z));
            m_Faces[5].Edges[2] = new EdgeData(new Vector3(-extents.x, -extents.y, extents.z), new Vector3(-extents.x, extents.y, extents.z));
            m_Faces[5].Edges[3] = new EdgeData(new Vector3(-extents.x, -extents.y, extents.z), new Vector3(extents.x, -extents.y, extents.z));

            return m_Faces;
        }

        void DoRotateHandlesGUI(ShapeComponent shape, Bounds bounds)
        {
            var matrix = shape.gameObject.transform.localToWorldMatrix;
            var extents = bounds.extents;
            bool hasRotated = false;

            using (new Handles.DrawingScope(matrix))
            {
                var faces = GetFaces(extents);
                foreach(var face in faces)
                {
                    if (IsFaceVisible(face))
                    {
                        foreach (var edge in face.Edges)
                        {
                            var rot = RotateBoundsHandle(edge.PointA, edge.PointB, edge.PointA - edge.PointB);
                            shape.Rotate(rot);
                            hasRotated = hasRotated || rot != Quaternion.identity;
                        }
                    }
                }
                if (hasRotated)
                    ProBuilderEditor.Refresh();
            }
        }

        bool IsFaceVisible(FaceData face)
        {
            Vector3 worldDir = Handles.matrix.MultiplyVector(face.Normal).normalized;

            float cosV;

            if (Camera.current.orthographic)
                cosV = Vector3.Dot(-Camera.current.transform.forward, worldDir);
            else
                cosV = Vector3.Dot((Camera.current.transform.position - Handles.matrix.MultiplyPoint(face.CenterPosition)).normalized, worldDir);

            return cosV < 0;
        }

        

        Quaternion RotateBoundsHandle(Vector3 pointA, Vector3 pointB, Vector3 axis)
        {
            Event evt = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            Quaternion rotation = Quaternion.identity;
            switch (evt.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == controlID && (evt.button == 0 || evt.button == 2))
                    {
                        s_CurrentId = controlID;
                        s_LastRotation = Quaternion.identity;
                        s_StartMousePosition = Event.current.mousePosition;
                        s_StartPosition = HandleUtility.ClosestPointToPolyLine(pointA, pointB);
                        s_IsMouseDown = true;
                        GUIUtility.hotControl = controlID;
                        evt.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID && (evt.button == 0 || evt.button == 2))
                    {
                        GUIUtility.hotControl = 0;
                        evt.Use();
                        s_IsMouseDown = false;
                        s_CurrentId = -1;
                    }
                    break;
                case EventType.MouseMove:
                    if (HandleUtility.nearestControl == controlID)
                        HandleUtility.Repaint();
                    break;
                case EventType.Layout:
                    HandleUtility.AddControl(controlID, HandleUtility.DistanceToLine(pointA, pointB));
                    break;
                case EventType.Repaint:
                    bool isSelected = (HandleUtility.nearestControl == controlID && s_CurrentId == -1) || s_CurrentId == controlID;
                    if (isSelected)
                    {
                        EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.RotateArrow);
                    }
                    using (new Handles.DrawingScope(isSelected ? Color.white : Color.green))
                    {
                        Handles.DrawAAPolyLine(isSelected ? 10f : 3f, pointA, pointB);
                        break;
                    }
                case EventType.MouseDrag:
                    if (s_IsMouseDown && s_CurrentId == controlID)
                    {
                        Vector3 direction = Vector3.Cross(Vector3.Cross(pointA, pointB).normalized, axis).normalized;
                        var rotDist = HandleUtility.CalcLineTranslation(s_StartMousePosition, Event.current.mousePosition, s_StartPosition, direction);
                        rotDist = Handles.SnapValue(rotDist, 90f);
                        var rot = Quaternion.AngleAxis(rotDist * -1, axis);
                        rotation = s_LastRotation * Quaternion.Inverse(rot);
                        s_LastRotation = rot;
                    }
                        break;
            }
            return rotation;
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
            if (IsEditing())
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
            shape.mesh.SetPivot(shape.transform.position);
            ProBuilderEditor.Refresh(false);
        }

        void RebuildShape(ShapeComponent shape, Bounds bounds, Quaternion rotation)
        {
            shape.Rebuild(bounds, rotation);
            shape.mesh.SetPivot(shape.transform.position);
            ProBuilderEditor.Refresh();
        }
    }
}

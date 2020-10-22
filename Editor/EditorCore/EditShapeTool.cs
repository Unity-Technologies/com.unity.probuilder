// using System.Collections.Generic;
// using UnityEngine;
// using UnityEditor.EditorTools;
// using UnityEditor.IMGUI.Controls;
// using UnityEngine.ProBuilder;
// using UnityEngine.ProBuilder.MeshOperations;
// using UnityEngine.ProBuilder.Shapes;
// using Math = UnityEngine.ProBuilder.Math;
//
// namespace UnityEditor.ProBuilder
// {
//     [EditorTool("Edit Shape", typeof(ShapeComponent))]
//     public sealed class EditShapeTool: EditorTool
//     {
//         const int k_HotControlNone = 0;
//         BoxBoundsHandle m_BoundsHandle;
//         bool m_BoundsHandleActive;
//
//         Vector2 m_StartMousePosition;
//         Vector3 m_StartPosition;
//         Quaternion m_LastRotation;
//         int m_CurrentId = -1;
//         bool m_IsMouseDown;
//
//         // Don't recalculate the active bounds during an edit operation, it causes the handles to drift
//         ShapeState m_ActiveShapeState;
//
//         struct ShapeState
//         {
//             public Matrix4x4 positionAndRotationMatrix;
//             public Bounds boundsHandleValue;
//         }
//
//         FaceData[] m_Faces = new FaceData[6];
//
//         sealed class FaceData
//         {
//             public Vector3 CenterPosition;
//             public Vector3 Normal;
//             public EdgeData[] Edges;
//
//             public bool IsVisible
//             {
//                 get
//                 {
//                     Vector3 worldDir = Handles.matrix.MultiplyVector(Normal).normalized;
//
//                     Vector3 cameraDir;
//                     if (Camera.current.orthographic)
//                         cameraDir = -Camera.current.transform.forward;
//                     else
//                         cameraDir = (Camera.current.transform.position - Handles.matrix.MultiplyPoint(CenterPosition)).normalized;
//
//                     return Vector3.Dot(cameraDir, worldDir) < 0;
//                 }
//             }
//
//             public FaceData()
//             {
//                 Edges = new EdgeData[4];
//             }
//
//             public void SetData(Vector3 centerPosition, Vector3 normal)
//             {
//                 CenterPosition = centerPosition;
//                 Normal = normal;
//             }
//         }
//
//         struct EdgeData
//         {
//             public Vector3 PointA;
//             public Vector3 PointB;
//
//             public EdgeData(Vector3 pointA, Vector3 pointB)
//             {
//                 PointA = pointA;
//                 PointB = pointB;
//             }
//         }
//
//         //hashset to avoid drawing twice the same edge
//         HashSet<EdgeData> edgesToDraw = new HashSet<EdgeData>(new EdgeDataComparer());
//
//         //Comparer for the edgesToDraw hashset
//         class EdgeDataComparer : IEqualityComparer<EdgeData>
//         {
//             public bool Equals(EdgeData edge1, EdgeData edge2)
//             {
//                 bool result = edge1.PointA == edge2.PointA && edge1.PointB == edge2.PointB;
//                 result |= edge1.PointA == edge2.PointB && edge1.PointB == edge2.PointA;
//                 return result;
//             }
//
//             //Don't wan't to compare hashcode, only using equals
//             public int GetHashCode(EdgeData edge) {return 0;}
//         }
//
//         public override GUIContent toolbarIcon
//         {
//             get { return PrimitiveBoundsHandle.editModeButton; }
//         }
//
//         bool IsEditing => m_BoundsHandleActive;
//
//         void OnEnable()
//         {
//             m_BoundsHandle = new BoxBoundsHandle();
//             for (int i = 0; i < m_Faces.Length; i++)
//             {
//                 m_Faces[i] = new FaceData();
//             }
//         }
//
//         public override void OnToolGUI(EditorWindow window)
//         {
//             foreach (var obj in targets)
//             {
//                 var shape = obj as ShapeComponent;
//
//                 if (shape != null)
//                 {
//                     if (m_BoundsHandleActive && GUIUtility.hotControl == k_HotControlNone)
//                         EndBoundsEditing();
//
//                     if (Mathf.Approximately(shape.transform.lossyScale.sqrMagnitude, 0f))
//                         return;
//
//                     DoShapeGUI(shape);
//                 }
//             }
//         }
//
//         void DoShapeGUI(ShapeComponent shape)
//         {
//             var matrix = IsEditing
//                 ? m_ActiveShapeState.positionAndRotationMatrix
//                 : Matrix4x4.TRS(shape.transform.position, shape.transform.rotation, Vector3.one);
//
//             using (new Handles.DrawingScope(matrix))
//             {
//                 m_BoundsHandle.SetColor(Handles.s_ColliderHandleColor);
//
//                 CopyColliderPropertiesToHandle(shape);
//
//                 EditorGUI.BeginChangeCheck();
//
//                 m_BoundsHandle.DrawHandle();
//
//                 if (EditorGUI.EndChangeCheck())
//                 {
//                     BeginBoundsEditing(shape);
//                     CopyHandlePropertiesToCollider(shape);
//                 }
//
//                 DoRotateHandlesGUI(shape, shape.meshFilterBounds);
//             }
//         }
//
//
//         void UpdateFaces(Vector3 extents)
//         {
//             // -X
//             var pos = m_BoundsHandle.center - new Vector3(extents.x, 0, 0);
//             m_Faces[0].SetData(pos, Vector3.right);
//             m_Faces[0].Edges[0] = new EdgeData(new Vector3(-extents.x, extents.y, extents.z), new Vector3(-extents.x, -extents.y, extents.z));
//             m_Faces[0].Edges[1] = new EdgeData(new Vector3(-extents.x, extents.y, extents.z), new Vector3(-extents.x, extents.y, -extents.z));
//             m_Faces[0].Edges[2] = new EdgeData(new Vector3(-extents.x, -extents.y, -extents.z), new Vector3(-extents.x, -extents.y, extents.z));
//             m_Faces[0].Edges[3] = new EdgeData(new Vector3(-extents.x, -extents.y, -extents.z), new Vector3(-extents.x, extents.y, -extents.z));
//
//             // +X
//             pos = m_BoundsHandle.center + new Vector3(extents.x, 0, 0);
//             m_Faces[1].SetData(pos, -Vector3.right);
//             m_Faces[1].Edges[0] = new EdgeData(new Vector3(extents.x, extents.y, extents.z), new Vector3(extents.x, -extents.y, extents.z));
//             m_Faces[1].Edges[1] = new EdgeData(new Vector3(extents.x, extents.y, extents.z), new Vector3(extents.x, extents.y, -extents.z));
//             m_Faces[1].Edges[2] = new EdgeData(new Vector3(extents.x, -extents.y, -extents.z), new Vector3(extents.x, -extents.y, extents.z));
//             m_Faces[1].Edges[3] = new EdgeData(new Vector3(extents.x, -extents.y, -extents.z), new Vector3(extents.x, extents.y, -extents.z));
//
//             // -Y
//             pos = m_BoundsHandle.center - new Vector3(0, extents.y, 0);
//             m_Faces[2].SetData(pos, Vector3.up);
//             m_Faces[2].Edges[0] = new EdgeData(new Vector3(extents.x, -extents.y, extents.z), new Vector3(-extents.x, -extents.y, extents.z));
//             m_Faces[2].Edges[1] = new EdgeData(new Vector3(extents.x, -extents.y, extents.z), new Vector3(extents.x, -extents.y, -extents.z));
//             m_Faces[2].Edges[2] = new EdgeData(new Vector3(-extents.x, -extents.y, -extents.z), new Vector3(-extents.x, -extents.y, extents.z));
//             m_Faces[2].Edges[3] = new EdgeData(new Vector3(-extents.x, -extents.y, -extents.z), new Vector3(extents.x, -extents.y, -extents.z));
//
//             // +Y
//             pos = m_BoundsHandle.center + new Vector3(0, extents.y, 0);
//             m_Faces[3].SetData(pos, -Vector3.up);
//             m_Faces[3].Edges[0] = new EdgeData(new Vector3(extents.x, extents.y, extents.z), new Vector3(-extents.x, extents.y, extents.z));
//             m_Faces[3].Edges[1] = new EdgeData(new Vector3(extents.x, extents.y, extents.z), new Vector3(extents.x, extents.y, -extents.z));
//             m_Faces[3].Edges[2] = new EdgeData(new Vector3(-extents.x, extents.y, -extents.z), new Vector3(-extents.x, extents.y, extents.z));
//             m_Faces[3].Edges[3] = new EdgeData(new Vector3(-extents.x, extents.y, -extents.z), new Vector3(extents.x, extents.y, -extents.z));
//
//             // -Z
//             pos = m_BoundsHandle.center - new Vector3(0, 0, extents.z);
//             m_Faces[4].SetData(pos, Vector3.forward);
//             m_Faces[4].Edges[0] = new EdgeData(new Vector3(extents.x, extents.y, -extents.z), new Vector3(-extents.x, extents.y, -extents.z));
//             m_Faces[4].Edges[1] = new EdgeData(new Vector3(extents.x, extents.y, -extents.z), new Vector3(extents.x, -extents.y, -extents.z));
//             m_Faces[4].Edges[2] = new EdgeData(new Vector3(-extents.x, -extents.y, -extents.z), new Vector3(-extents.x, extents.y, -extents.z));
//             m_Faces[4].Edges[3] = new EdgeData(new Vector3(-extents.x, -extents.y, -extents.z), new Vector3(extents.x, -extents.y, -extents.z));
//
//             // +Z
//             pos = m_BoundsHandle.center + new Vector3(0, 0, extents.z);
//             m_Faces[5].SetData(pos, -Vector3.forward);
//             m_Faces[5].Edges[0] = new EdgeData(new Vector3(extents.x, extents.y, extents.z), new Vector3(-extents.x, extents.y, extents.z));
//             m_Faces[5].Edges[1] = new EdgeData(new Vector3(extents.x, extents.y, extents.z), new Vector3(extents.x, -extents.y, extents.z));
//             m_Faces[5].Edges[2] = new EdgeData(new Vector3(-extents.x, -extents.y, extents.z), new Vector3(-extents.x, extents.y, extents.z));
//             m_Faces[5].Edges[3] = new EdgeData(new Vector3(-extents.x, -extents.y, extents.z), new Vector3(extents.x, -extents.y, extents.z));
//         }
//
//         void DoRotateHandlesGUI(ShapeComponent shape, Bounds bounds)
//         {
//             var matrix = shape.gameObject.transform.localToWorldMatrix;
//             bool hasRotated = false;
//
//             edgesToDraw.Clear();
//             UpdateFaces(bounds.extents);
//             using (new Handles.DrawingScope(matrix))
//             {
//                 foreach(var face in m_Faces)
//                 {
//                     if (face.IsVisible)
//                     {
//                         foreach (var edge in face.Edges)
//                             edgesToDraw.Add(edge);
//                     }
//                 }
//
//                 foreach(var edgeData in edgesToDraw)
//                 {
//                     var rot = RotateEdgeHandle(edgeData);
//                     shape.Rotate(rot);
//                     hasRotated |= (rot != Quaternion.identity);
//                 }
//
//                 if (hasRotated)
//                     ProBuilderEditor.Refresh();
//             }
//         }
//
//         Quaternion RotateEdgeHandle(EdgeData edge)
//         {
//             Event evt = Event.current;
//             int controlID = GUIUtility.GetControlID(FocusType.Passive);
//             Quaternion rotation = Quaternion.identity;
//             switch (evt.GetTypeForControl(controlID))
//             {
//                 case EventType.MouseDown:
//                     if (HandleUtility.nearestControl == controlID && (evt.button == 0 || evt.button == 2))
//                     {
//                         m_CurrentId = controlID;
//                         m_LastRotation = Quaternion.identity;
//                         m_StartMousePosition = Event.current.mousePosition;
//                         m_StartPosition = HandleUtility.ClosestPointToPolyLine(edge.PointA, edge.PointB);
//                         m_IsMouseDown = true;
//                         GUIUtility.hotControl = controlID;
//                         evt.Use();
//                     }
//                     break;
//                 case EventType.MouseUp:
//                     if (GUIUtility.hotControl == controlID && (evt.button == 0 || evt.button == 2))
//                     {
//                         GUIUtility.hotControl = 0;
//                         evt.Use();
//                         m_IsMouseDown = false;
//                         m_CurrentId = -1;
//                     }
//                     break;
//                 case EventType.MouseMove:
//                     HandleUtility.Repaint();
//                     break;
//                 case EventType.Layout:
//                     HandleUtility.AddControl(controlID, HandleUtility.DistanceToLine(edge.PointA, edge.PointB));
//                     break;
//                 case EventType.Repaint:
//                     bool isSelected = (HandleUtility.nearestControl == controlID && m_CurrentId == -1) || m_CurrentId == controlID;
//                     if (isSelected)
//                         EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.RotateArrow);
//                     using (new Handles.DrawingScope(isSelected ? Color.white : Color.green))
//                     {
//                         Handles.DrawAAPolyLine(isSelected ? 10f : 3f, edge.PointA, edge.PointB);
//                     }
//                     break;
//                 case EventType.MouseDrag:
//                     if (m_IsMouseDown && m_CurrentId == controlID)
//                     {
//                         Vector3 axis = edge.PointA - edge.PointB;
//                         //Get a direction orthogonal to both direction to camera and edge direction
//                         Vector3 direction = Vector3.Cross(-Camera.current.transform.forward, axis).normalized;
//                         var rotDist = HandleUtility.CalcLineTranslation(m_StartMousePosition, Event.current.mousePosition, m_StartPosition, direction);
//                         rotDist = Handles.SnapValue(rotDist, 90f);
//                         var rot = Quaternion.AngleAxis(rotDist * -1, axis);
//                         rotation = m_LastRotation * Quaternion.Inverse(rot);
//                         m_LastRotation = rot;
//                     }
//                     break;
//             }
//             return rotation;
//         }
//
//         void BeginBoundsEditing(ShapeComponent shape)
//         {
//             if (m_BoundsHandleActive)
//                 return;
//
//             UndoUtility.RecordComponents<ProBuilderMesh, Transform>(
//                 new[] { shape },
//                 string.Format("Modify {0}", ObjectNames.NicifyVariableName(target.GetType().Name)));
//
//             m_BoundsHandleActive = true;
//             var localBounds = shape.mesh.mesh.bounds;
//             m_ActiveShapeState = new ShapeState()
//             {
//                 positionAndRotationMatrix = Matrix4x4.TRS(shape.transform.position, shape.transform.rotation, Vector3.one),
//                 boundsHandleValue = localBounds,
//             };
//         }
//
//         void EndBoundsEditing()
//         {
//             m_BoundsHandleActive = false;
//         }
//
//         static Vector3 TransformColliderCenterToHandleSpace(Matrix4x4 localToWorldMatrix, Vector3 colliderCenter)
//         {
//             return Handles.inverseMatrix * (localToWorldMatrix * colliderCenter);
//         }
//
//         void CopyColliderPropertiesToHandle(ShapeComponent shape)
//         {
//             // when editing a shape, we don't bother doing the conversion from handle space bounds to model for the
//             // active handle
//             if (IsEditing)
//             {
//                 m_BoundsHandle.center = m_ActiveShapeState.boundsHandleValue.center;
//                 m_BoundsHandle.size = m_ActiveShapeState.boundsHandleValue.size;
//                 return;
//             }
//
//             var bounds = shape.mesh.mesh.bounds;
//             var trs = shape.transform.localToWorldMatrix;
//             var lossyScale = shape.transform.lossyScale;
//
//             m_BoundsHandle.center = TransformColliderCenterToHandleSpace(trs, bounds.center);
//             m_BoundsHandle.size = Vector3.Scale(bounds.size, lossyScale);
//         }
//
//         void CopyHandlePropertiesToCollider(ShapeComponent shape)
//         {
//             m_ActiveShapeState.boundsHandleValue = new Bounds(m_BoundsHandle.center, m_BoundsHandle.size);
//
//             var bounds = new Bounds();
//
//             var trs = shape.transform;
//
//             bounds.center = Handles.matrix.MultiplyPoint3x4(m_BoundsHandle.center);
//             bounds.size = Math.Abs(Vector3.Scale(m_BoundsHandle.size, Math.InvertScaleVector(trs.lossyScale)));
//
//             shape.Rebuild(bounds, shape.transform.rotation);
//             shape.mesh.SetPivot(shape.transform.position);
//             ProBuilderEditor.Refresh(false);
//         }
//
//     }
// }


using System;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEngine.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;
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

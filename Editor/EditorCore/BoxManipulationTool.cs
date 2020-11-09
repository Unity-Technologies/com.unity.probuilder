using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    abstract class BoxManipulationTool : EditorTool
    {
        protected const int k_HotControlNone = 0;

        protected BoxBoundsHandle m_BoundsHandle;
        protected bool m_BoundsHandleActive;
        protected Color m_BoundsHandleColor = Handles.s_SelectedColor;

        Vector2 m_StartMousePosition;
        Vector3 m_StartPosition;
        Quaternion m_LastRotation;
        int m_CurrentId = -1;
        bool m_IsMouseDown;

        [Range(1,90)]
        [SerializeField]
        protected int m_snapAngle = 15;

        protected GUIContent m_OverlayTitle;
        protected GUIContent m_SnapAngleContent;

        protected FaceData[] m_Faces;

        protected sealed class FaceData
        {
            public Vector3 CenterPosition;
            public Vector3 Normal;
            public EdgeData[] Edges;

            public bool IsVisible
            {
                get
                {
                    Vector3 worldDir = Handles.matrix.MultiplyVector(Normal).normalized;

                    Vector3 cameraDir;
                    if (Camera.current.orthographic)
                        cameraDir = -Camera.current.transform.forward;
                    else
                        cameraDir = (Camera.current.transform.position - Handles.matrix.MultiplyPoint(CenterPosition)).normalized;

                    return Vector3.Dot(cameraDir, worldDir) < 0;
                }
            }

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

        protected struct EdgeData
        {
            public Vector3 PointA;
            public Vector3 PointB;

            public Vector3 Center
            {
                get => ( (PointA + PointB) / 2.0f );
            }

            public EdgeData(Vector3 pointA, Vector3 pointB)
            {
                PointA = pointA;
                PointB = pointB;
            }
        }

        protected struct BoundsState
        {
            public Matrix4x4 positionAndRotationMatrix;
            public Bounds boundsHandleValue;
        }

        // Don't recalculate the active bounds during an edit operation, it causes the handles to drift
        protected BoundsState m_ActiveBoundsState;

        protected bool IsEditing => m_BoundsHandleActive;


        //hashset to avoid drawing twice the same edge
        protected HashSet<EdgeData> edgesToDraw = new HashSet<EdgeData>(new EdgeDataComparer());

        //Comparer for the edgesToDraw hashset
        class EdgeDataComparer : IEqualityComparer<EdgeData>
        {
            public bool Equals(EdgeData edge1, EdgeData edge2)
            {
                bool result = edge1.PointA == edge2.PointA && edge1.PointB == edge2.PointB;
                result |= edge1.PointA == edge2.PointB && edge1.PointB == edge2.PointA;
                return result;
            }

            //Don't wan't to compare hashcode, only using equals
            public int GetHashCode(EdgeData edge) {return 0;}
        }

        Dictionary<EdgeData, SimpleTuple<EdgeData, EdgeData>> m_EdgeDataToNeighborsEdges;

        public override GUIContent toolbarIcon
        {
            get { return PrimitiveBoundsHandle.editModeButton; }
        }

        protected void InitTool()
        {
            m_BoundsHandle = new BoxBoundsHandle();
            m_Faces = new FaceData[6];
            for (int i = 0; i < m_Faces.Length; i++)
            {
                m_Faces[i] = new FaceData();
            }
            m_SnapAngleContent = new GUIContent("Snap Angle", L10n.Tr("Defines an angle in [1,90] to snap rotation."));
        }

        public override void OnToolGUI(EditorWindow window)
        {
            SceneViewOverlay.Window( m_OverlayTitle, OnOverlayGUI, 0, SceneViewOverlay.WindowDisplayOption.OneWindowPerTitle );
        }

        protected abstract void DoManipulationGUI(Object toolTarget);

        protected abstract void UpdateTargetRotation(Object toolTarget, Quaternion rotation);

        protected abstract void OnOverlayGUI(Object target, SceneView view);

        protected void BeginBoundsEditing(ProBuilderMesh mesh)
        {
            if (m_BoundsHandleActive)
                return;

            UndoUtility.RecordComponents<ProBuilderMesh, Transform>(
                new[] { mesh },
                string.Format("Modify {0}", ObjectNames.NicifyVariableName(target.GetType().Name)));

            m_BoundsHandleActive = true;
            Bounds localBounds = mesh.mesh.bounds;
            m_ActiveBoundsState = new BoundsState()
            {
                positionAndRotationMatrix = Matrix4x4.TRS(mesh.transform.position, mesh.transform.rotation, Vector3.one),
                boundsHandleValue = localBounds,
            };
        }

        protected void EndBoundsEditing()
        {
            m_BoundsHandleActive = false;
        }

        protected void DoRotateHandlesGUI(Object toolTarget, ProBuilderMesh mesh, Bounds bounds)
        {
            var matrix = mesh.transform.localToWorldMatrix;

            edgesToDraw.Clear();
            UpdateFaces(bounds);
            using (new Handles.DrawingScope(matrix))
            {
                foreach(var face in m_Faces)
                {
                    if (face.IsVisible)
                    {
                        foreach (var edge in face.Edges)
                            edgesToDraw.Add(edge);
                    }
                }

                foreach(var edgeData in edgesToDraw)
                {
                    Quaternion rot;
                    if(RotateEdgeHandle(edgeData, out rot))
                        UpdateTargetRotation(toolTarget, rot);
                }
            }
        }

        protected bool RotateEdgeHandle(EdgeData edge, out Quaternion rotation)
        {
            Event evt = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            bool hasRotated = false;
            rotation = Quaternion.identity;
            switch (evt.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == controlID && (evt.button == 0 || evt.button == 2))
                    {
                        m_CurrentId = controlID;
                        m_LastRotation = Quaternion.identity;
                        m_StartMousePosition = Event.current.mousePosition;
                        m_StartPosition = HandleUtility.ClosestPointToPolyLine(edge.PointA, edge.PointB);
                        m_IsMouseDown = true;
                        GUIUtility.hotControl = controlID;
                        evt.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID && (evt.button == 0 || evt.button == 2))
                    {
                        GUIUtility.hotControl = 0;
                        evt.Use();
                        m_IsMouseDown = false;
                        m_CurrentId = -1;
                    }
                    break;
                case EventType.MouseMove:
                    HandleUtility.Repaint();
                    break;
                case EventType.Layout:
                    HandleUtility.AddControl(controlID, HandleUtility.DistanceToLine(edge.PointA, edge.PointB));
                    break;
                case EventType.Repaint:
                    bool isSelected = (HandleUtility.nearestControl == controlID && m_CurrentId == -1) || m_CurrentId == controlID;
                    Color color = edge.Center.x == 0 ? Handles.s_XAxisColor : ( edge.Center.y == 0 ? Handles.s_YAxisColor : Handles.s_ZAxisColor );
                    if(isSelected)
                    {
                        EditorGUIUtility.AddCursorRect(new Rect(0, 0, Screen.width, Screen.height), MouseCursor.RotateArrow);
                        //Draw Arc
                        Vector3 edgeToPrevious = m_EdgeDataToNeighborsEdges[edge].item1.Center - edge.Center;
                        Vector3 edgeToNext = m_EdgeDataToNeighborsEdges[edge].item2.Center - edge.Center;
                        Vector3 normal = Vector3.Cross(edgeToNext,edgeToPrevious).normalized;
                        using(new Handles.DrawingScope(color))
                        {
                             Handles.DrawWireArc(Vector3.zero,
                                 normal,
                                 m_EdgeDataToNeighborsEdges[edge].item1.Center,
                                 180f,
                                 edge.Center.magnitude);
                        }
                    }

                    using (new Handles.DrawingScope(isSelected ? Color.white : m_BoundsHandleColor))
                    {
                        Handles.DrawAAPolyLine(isSelected ? 10f : 3f, edge.PointA, edge.PointB);
                    }
                    break;
                case EventType.MouseDrag:
                    if (m_IsMouseDown && m_CurrentId == controlID)
                    {
                        Vector3 axis = edge.PointA - edge.PointB;
                        Vector3 axisToPrevious = (m_EdgeDataToNeighborsEdges[edge].item1.Center - edge.Center);
                        Vector3 axisToNext =  (m_EdgeDataToNeighborsEdges[edge].item2.Center - edge.Center);

                        var rotDistToPrevious = HandleUtility.CalcLineTranslation(m_StartMousePosition, Event.current.mousePosition, m_StartPosition, axisToPrevious);
                        var rotDistToNext = HandleUtility.CalcLineTranslation(m_StartMousePosition, Event.current.mousePosition, m_StartPosition, axisToNext);

                        float mainRot = rotDistToNext;
                        if(Mathf.Abs(rotDistToPrevious) > Mathf.Abs(rotDistToNext))
                            mainRot = -rotDistToPrevious;

                        mainRot = ( (int) ( mainRot * (90f / (float)m_snapAngle) )) * (float)m_snapAngle;
                        var rot = Quaternion.AngleAxis(mainRot, axis);

                        rotation = m_LastRotation * Quaternion.Inverse(rot);
                        m_LastRotation = rot;

                        hasRotated = true;
                    }
                    break;
            }
            return hasRotated;
        }

        protected void UpdateFaces(Bounds bounds)
        {
            Vector3 extents = bounds.extents;

            EdgeData edgeX1 = new EdgeData(new Vector3(extents.x, extents.y, extents.z),
                                new Vector3(-extents.x, extents.y, extents.z));
            EdgeData edgeX2 = new EdgeData(new Vector3(extents.x, -extents.y, extents.z),
                                new Vector3(-extents.x, -extents.y, extents.z));
            EdgeData edgeX3 = new EdgeData(new Vector3(extents.x, extents.y, -extents.z),
                                new Vector3(-extents.x, extents.y, -extents.z));
            EdgeData edgeX4 = new EdgeData(new Vector3(extents.x, -extents.y, -extents.z),
                                new Vector3(-extents.x, -extents.y, -extents.z));

            EdgeData edgeY1 = new EdgeData(new Vector3(extents.x, extents.y, extents.z),
                                new Vector3(extents.x, -extents.y, extents.z) );
            EdgeData edgeY2 = new EdgeData(new Vector3(-extents.x, extents.y, extents.z),
                                new Vector3(-extents.x, -extents.y, extents.z));
            EdgeData edgeY3 = new EdgeData(new Vector3(extents.x, extents.y, -extents.z),
                                new Vector3(extents.x, -extents.y, -extents.z));
            EdgeData edgeY4 = new EdgeData(new Vector3(-extents.x, extents.y, -extents.z),
                                new Vector3(-extents.x, -extents.y, -extents.z));

            EdgeData edgeZ1 = new EdgeData(new Vector3(extents.x, extents.y, extents.z),
                                new Vector3(extents.x, extents.y, -extents.z));
            EdgeData edgeZ2 = new EdgeData(new Vector3(-extents.x, extents.y, extents.z),
                                new Vector3(-extents.x, extents.y, -extents.z));
            EdgeData edgeZ3 = new EdgeData(new Vector3(extents.x, -extents.y, extents.z),
                                new Vector3(extents.x, -extents.y, -extents.z));
            EdgeData edgeZ4 = new EdgeData(new Vector3(-extents.x, -extents.y, extents.z),
                                new Vector3(-extents.x, -extents.y, -extents.z));

            // -X
            var pos = m_BoundsHandle.center - new Vector3(extents.x, 0, 0);
            m_Faces[0].SetData(pos, Vector3.right);
            m_Faces[0].Edges[0] = edgeY2;
            m_Faces[0].Edges[1] = edgeZ2;
            m_Faces[0].Edges[2] = edgeZ4;
            m_Faces[0].Edges[3] = edgeY4;

            // +X
            pos = m_BoundsHandle.center + new Vector3(extents.x, 0, 0);
            m_Faces[1].SetData(pos, -Vector3.right);
            m_Faces[1].Edges[0] = edgeY1;
            m_Faces[1].Edges[1] = edgeZ1;
            m_Faces[1].Edges[2] = edgeZ3;
            m_Faces[1].Edges[3] = edgeY3;

            // -Y
            pos = m_BoundsHandle.center - new Vector3(0, extents.y, 0);
            m_Faces[2].SetData(pos, Vector3.up);
            m_Faces[2].Edges[0] = edgeX2;
            m_Faces[2].Edges[1] = edgeZ3;
            m_Faces[2].Edges[2] = edgeZ4;
            m_Faces[2].Edges[3] = edgeX4;

            // +Y
            pos = m_BoundsHandle.center + new Vector3(0, extents.y, 0);
            m_Faces[3].SetData(pos, -Vector3.up);
            m_Faces[3].Edges[0] = edgeX1;
            m_Faces[3].Edges[1] = edgeZ1;
            m_Faces[3].Edges[2] = edgeZ2;
            m_Faces[3].Edges[3] = edgeX3;

            // -Z
            pos = m_BoundsHandle.center - new Vector3(0, 0, extents.z);
            m_Faces[4].SetData(pos, Vector3.forward);
            m_Faces[4].Edges[0] = edgeX3;
            m_Faces[4].Edges[1] = edgeY3;
            m_Faces[4].Edges[2] = edgeY4;
            m_Faces[4].Edges[3] = edgeX4;

            // +Z
            pos = m_BoundsHandle.center + new Vector3(0, 0, extents.z);
            m_Faces[5].SetData(pos, -Vector3.forward);
            m_Faces[5].Edges[0] = edgeX1;
            m_Faces[5].Edges[1] = edgeY1;
            m_Faces[5].Edges[2] = edgeY2;
            m_Faces[5].Edges[3] = edgeX2;

            if(m_EdgeDataToNeighborsEdges == null)
            {
                m_EdgeDataToNeighborsEdges = new Dictionary<EdgeData, SimpleTuple<EdgeData, EdgeData>>();
                m_EdgeDataToNeighborsEdges.Add(edgeX1, new SimpleTuple<EdgeData, EdgeData>(edgeX2, edgeX3));
                m_EdgeDataToNeighborsEdges.Add(edgeX2, new SimpleTuple<EdgeData, EdgeData>(edgeX4, edgeX1));
                m_EdgeDataToNeighborsEdges.Add(edgeX3, new SimpleTuple<EdgeData, EdgeData>(edgeX1, edgeX4));
                m_EdgeDataToNeighborsEdges.Add(edgeX4, new SimpleTuple<EdgeData, EdgeData>(edgeX3, edgeX2));

                m_EdgeDataToNeighborsEdges.Add(edgeY1, new SimpleTuple<EdgeData, EdgeData>(edgeY3, edgeY2));
                m_EdgeDataToNeighborsEdges.Add(edgeY2, new SimpleTuple<EdgeData, EdgeData>(edgeY1, edgeY4));
                m_EdgeDataToNeighborsEdges.Add(edgeY3, new SimpleTuple<EdgeData, EdgeData>(edgeY4, edgeY1));
                m_EdgeDataToNeighborsEdges.Add(edgeY4, new SimpleTuple<EdgeData, EdgeData>(edgeY2, edgeY3));

                m_EdgeDataToNeighborsEdges.Add(edgeZ1, new SimpleTuple<EdgeData, EdgeData>(edgeZ2, edgeZ3));
                m_EdgeDataToNeighborsEdges.Add(edgeZ2, new SimpleTuple<EdgeData, EdgeData>(edgeZ4, edgeZ1));
                m_EdgeDataToNeighborsEdges.Add(edgeZ3, new SimpleTuple<EdgeData, EdgeData>(edgeZ1, edgeZ4));
                m_EdgeDataToNeighborsEdges.Add(edgeZ4, new SimpleTuple<EdgeData, EdgeData>(edgeZ3, edgeZ2));
            }
            else
            {
                m_EdgeDataToNeighborsEdges[edgeX1]= new SimpleTuple<EdgeData, EdgeData>(edgeX2, edgeX3);
                m_EdgeDataToNeighborsEdges[edgeX2]= new SimpleTuple<EdgeData, EdgeData>(edgeX4, edgeX1);
                m_EdgeDataToNeighborsEdges[edgeX3]= new SimpleTuple<EdgeData, EdgeData>(edgeX1, edgeX4);
                m_EdgeDataToNeighborsEdges[edgeX4]= new SimpleTuple<EdgeData, EdgeData>(edgeX3, edgeX2);

                m_EdgeDataToNeighborsEdges[edgeY1]= new SimpleTuple<EdgeData, EdgeData>(edgeY3, edgeY2);
                m_EdgeDataToNeighborsEdges[edgeY2]= new SimpleTuple<EdgeData, EdgeData>(edgeY1, edgeY4);
                m_EdgeDataToNeighborsEdges[edgeY3]= new SimpleTuple<EdgeData, EdgeData>(edgeY4, edgeY1);
                m_EdgeDataToNeighborsEdges[edgeY4]= new SimpleTuple<EdgeData, EdgeData>(edgeY2, edgeY3);

                m_EdgeDataToNeighborsEdges[edgeZ1]= new SimpleTuple<EdgeData, EdgeData>(edgeZ2, edgeZ3);
                m_EdgeDataToNeighborsEdges[edgeZ2]= new SimpleTuple<EdgeData, EdgeData>(edgeZ4, edgeZ1);
                m_EdgeDataToNeighborsEdges[edgeZ3]= new SimpleTuple<EdgeData, EdgeData>(edgeZ1, edgeZ4);
                m_EdgeDataToNeighborsEdges[edgeZ4]= new SimpleTuple<EdgeData, EdgeData>(edgeZ3, edgeZ2);
            }
        }

        protected void CopyColliderPropertiesToHandle(Transform transform, Bounds bounds)
        {
            // when editing a shape, we don't bother doing the conversion from handle space bounds to model for the
            // active handle
            if (IsEditing)
            {
                m_BoundsHandle.center = m_ActiveBoundsState.boundsHandleValue.center;
                m_BoundsHandle.size = m_ActiveBoundsState.boundsHandleValue.size;
                return;
            }

            var localToWorld = transform.localToWorldMatrix;
            var lossyScale = transform.lossyScale;

            m_BoundsHandle.center = Handles.inverseMatrix * (localToWorld * bounds.center);
            m_BoundsHandle.size = Vector3.Scale(bounds.size, lossyScale);
        }

    }
}

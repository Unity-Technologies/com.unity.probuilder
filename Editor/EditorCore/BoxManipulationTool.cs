using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEngine.ProBuilder;

using FaceData = UnityEditor.ProBuilder.EditorShapeUtility.FaceData;
using EdgeData = UnityEditor.ProBuilder.EditorShapeUtility.EdgeData;
using BoundsState = UnityEditor.ProBuilder.EditorShapeUtility.BoundsState;

namespace UnityEditor.ProBuilder
{
    abstract class BoxManipulationTool : EditorTool
    {
        protected const int k_HotControlNone = 0;

        protected BoxBoundsHandle m_BoundsHandle;
        protected bool m_BoundsHandleActive;
        protected Color m_BoundsHandleColor;

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

        // Don't recalculate the active bounds during an edit operation, it causes the handles to drift
        protected BoundsState m_ActiveBoundsState;
        protected bool IsEditing => m_BoundsHandleActive;

        //hashset to avoid drawing twice the same edge
        protected HashSet<EdgeData> m_EdgesToDraw = new HashSet<EdgeData>(new EditorShapeUtility.EdgeDataComparer());

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
            m_SnapAngleContent = new GUIContent("Rotation Snap", L10n.Tr("Defines an angle in [1,90] to snap rotation."));
            m_EdgeDataToNeighborsEdges = new Dictionary<EdgeData, SimpleTuple<EdgeData, EdgeData>>();
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

            m_EdgesToDraw.Clear();
            EditorShapeUtility.UpdateFaces(bounds, Vector3.zero, m_Faces, m_EdgeDataToNeighborsEdges);
            using (new Handles.DrawingScope(matrix))
            {
                foreach(var face in m_Faces)
                {
                    if (face.IsVisible)
                    {
                        foreach (var edge in face.Edges)
                            m_EdgesToDraw.Add(edge);
                    }
                }

                foreach(var edgeData in m_EdgesToDraw)
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
    }
}

using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
#if UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.ToolManager;
#else
using ToolManager = UnityEditor.EditorTools.EditorTools;
#endif

namespace UnityEditor.ProBuilder
{
    internal class ShapeState_InitShape : ShapeState
    {
        //NOTE: All class attributes are used for handle display
        bool m_BoundsHandleActive = false;
        EditorShapeUtility.BoundsState m_ActiveBoundsState;
        BoxBoundsHandle m_BoundsHandle;
        //hashset to avoid drawing twice the same edge
        HashSet<EditorShapeUtility.EdgeData> m_EdgesToDraw = new HashSet<EditorShapeUtility.EdgeData>(new EditorShapeUtility.EdgeDataComparer());
        EditorShapeUtility.FaceData[] m_Faces;
        Dictionary<EditorShapeUtility.EdgeData, SimpleTuple<EditorShapeUtility.EdgeData, EditorShapeUtility.EdgeData>> m_EdgeDataToNeighborsEdges;

        //Handle Manipulation
        Vector2 m_StartMousePosition;
        Vector3 m_StartPosition;
        Quaternion m_LastRotation;
        int m_CurrentId = -1;
        bool m_IsMouseDown;

        protected override void InitState()
        {
            tool.m_IsShapeInit = false;

            //Init edition tool
            m_BoundsHandle = new BoxBoundsHandle();

            m_Faces = new EditorShapeUtility.FaceData[6];
            for (int i = 0; i < m_Faces.Length; i++)
            {
                m_Faces[i] = new EditorShapeUtility.FaceData();
            }
            m_EdgeDataToNeighborsEdges = new Dictionary<EditorShapeUtility.EdgeData, SimpleTuple<EditorShapeUtility.EdgeData, EditorShapeUtility.EdgeData>>();
        }

        public override ShapeState DoState(Event evt)
        {
            if(evt.type == EventType.KeyDown)
            {
                switch(evt.keyCode)
                {
                    case KeyCode.Escape:
                        ToolManager.RestorePreviousTool();
                        break;
                }
            }

            if(tool.m_LastShapeCreated != null)
            {
                DoEditingGUI(tool.m_LastShapeCreated);
            }

            if(GUIUtility.hotControl != 0)
                return this;

            if (evt.isMouse)
            {
                var res = EditorHandleUtility.FindBestPlaneAndBitangent(evt.mousePosition);

                Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                float hit;

                if (res.item1.Raycast(ray, out hit))
                {
                    //Plane init
                    tool.m_Plane = res.item1;
                    tool.m_PlaneForward = res.item2;
                    tool.m_PlaneRight = Vector3.Cross(tool.m_Plane.normal, tool.m_PlaneForward);

                    var planeNormal = tool.m_Plane.normal;
                    var planeCenter = tool.m_Plane.normal * -tool.m_Plane.distance;
                    // if hit point on plane is cardinal axis and on grid, snap to grid.
                    if (Math.IsCardinalAxis(planeNormal))
                    {
                        const float epsilon = .00001f;
                        bool offGrid = false;
                        Vector3 snapVal = EditorSnapping.activeMoveSnapValue;
                        Vector3 center = Vector3.Scale(ProBuilderSnapping.GetSnappingMaskBasedOnNormalVector(planeNormal), planeCenter);
                        for (int i = 0; i < 3; i++)
                            offGrid |= Mathf.Abs(snapVal[i] % center[i]) > epsilon;
                        tool.m_IsOnGrid = !offGrid;
                    }
                    else
                    {
                        tool.m_IsOnGrid = false;
                    }

                    //Click has been done => Define a plane for the tool
                    if(evt.type == EventType.MouseDown)
                    {
                        //BB init
                        tool.m_BB_Origin = tool.GetPoint(ray.GetPoint(hit));
                        tool.m_BB_HeightCorner = tool.m_BB_Origin;
                        tool.m_BB_OppositeCorner = tool.m_BB_Origin;

                        return NextState();
                    }
                    else
                    {
                        tool.SetBoundsOrigin(ray.GetPoint(hit));
                    }
                }
            }
            tool.DrawBoundingBox();

            return this;
        }

        void DoEditingGUI(ShapeComponent shapeComponent)
        {
            if(m_BoundsHandleActive && GUIUtility.hotControl == 0)
                m_BoundsHandleActive = false;

            var matrix = m_BoundsHandleActive
                ? m_ActiveBoundsState.positionAndRotationMatrix
                : Matrix4x4.TRS(shapeComponent.transform.position, shapeComponent.transform.rotation, Vector3.one);

            using (new Handles.DrawingScope(matrix))
            {
                m_BoundsHandle.SetColor(DrawShapeTool.k_BoundsColor);

                EditorShapeUtility.CopyColliderPropertiesToHandle(
                    shapeComponent.transform, shapeComponent.mesh.mesh.bounds,
                    m_BoundsHandle, m_BoundsHandleActive, m_ActiveBoundsState);

                EditorGUI.BeginChangeCheck();

                m_BoundsHandle.DrawHandle();

                if (EditorGUI.EndChangeCheck())
                {
                    if(!m_BoundsHandleActive)
                        BeginBoundsEditing(shapeComponent.mesh);
                    UndoUtility.RegisterCompleteObjectUndo(shapeComponent, "Scale Shape");
                    EditorShapeUtility.CopyHandlePropertiesToCollider(m_BoundsHandle, m_ActiveBoundsState);
                    EditShapeTool.ApplyProperties(shapeComponent, m_ActiveBoundsState);
                    DrawShapeTool.s_Size.value = m_BoundsHandle.size;
                }

                DoRotateHandlesGUI(shapeComponent, shapeComponent.mesh, shapeComponent.meshFilterBounds);
            }
        }

        void BeginBoundsEditing(ProBuilderMesh mesh)
        {
            UndoUtility.RecordComponents<ProBuilderMesh, Transform>(
                new[] { mesh },
                string.Format("Modify {0}", ObjectNames.NicifyVariableName(mesh.gameObject.GetType().Name)));

            m_BoundsHandleActive = true;
            Bounds localBounds = mesh.mesh.bounds;
            m_ActiveBoundsState = new EditorShapeUtility.BoundsState()
            {
                positionAndRotationMatrix = Matrix4x4.TRS(mesh.transform.position, mesh.transform.rotation, Vector3.one),
                boundsHandleValue = localBounds,
            };
        }

        void DoRotateHandlesGUI(ShapeComponent shapeComponent, ProBuilderMesh mesh, Bounds bounds)
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
                    {
                        UndoUtility.RegisterCompleteObjectUndo(shapeComponent, "Rotate Shape");
                        shapeComponent.RotateInsideBounds(rot);
                        DrawShapeTool.s_LastShapeRotation = shapeComponent.rotation;
                        ProBuilderEditor.Refresh();
                    }
                }
            }
        }

        bool RotateEdgeHandle(EditorShapeUtility.EdgeData edge, out Quaternion rotation)
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

                    using (new Handles.DrawingScope(isSelected ? Color.white : DrawShapeTool.k_BoundsColor))
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

                        mainRot = ( (int) ( mainRot * (90f / tool.snapAngle) )) * tool.snapAngle;
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

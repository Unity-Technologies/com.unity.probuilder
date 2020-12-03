using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using Math = UnityEngine.ProBuilder.Math;
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
        EditorShapeUtility.FaceData[] m_Faces;

        //Handle Manipulation
        int m_CurrentId = -1;

        //Orientation Handle Manipulation
        bool m_isManipulatingOrientation = false;
        Quaternion m_ShapeRotation = Quaternion.identity;
        Vector3 m_CurrentHandlePosition = Vector3.zero;
        EditorShapeUtility.FaceData m_CurrentTargetedFace = null;

        //Size Handle Manipulation
        bool m_isManipulatingSize = false;
        Vector2 m_MouseStartPosition;
        Vector2 m_StartPosition;
        float m_SizeOffset;
        Vector3 m_OriginalSize;
        Vector3 m_OriginalCenter;
        bool m_InitSizeInteraction = true;

        protected override void InitState()
        {
            tool.m_IsShapeInit = false;

            m_Faces = new EditorShapeUtility.FaceData[6];
            for (int i = 0; i < m_Faces.Length; i++)
                m_Faces[i] = new EditorShapeUtility.FaceData();
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
                DoEditingGUI(tool.m_LastShapeCreated);

            if(GUIUtility.hotControl != 0)
                return this;

            if(evt.isMouse)
            {
                var res = EditorHandleUtility.FindBestPlaneAndBitangent(evt.mousePosition);

                Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                float hit;

                if(res.item1.Raycast(ray, out hit))
                {
                    //Plane init
                    tool.m_Plane = res.item1;
                    tool.m_PlaneForward = res.item2;
                    tool.m_PlaneRight = Vector3.Cross(tool.m_Plane.normal, tool.m_PlaneForward);

                    var planeNormal = tool.m_Plane.normal;
                    var planeCenter = tool.m_Plane.normal * -tool.m_Plane.distance;
                    // if hit point on plane is cardinal axis and on grid, snap to grid.
                    if(Math.IsCardinalAxis(planeNormal))
                    {
                        const float epsilon = .00001f;
                        bool offGrid = false;
                        Vector3 snapVal = EditorSnapping.activeMoveSnapValue;
                        Vector3 center =
                            Vector3.Scale(ProBuilderSnapping.GetSnappingMaskBasedOnNormalVector(planeNormal),
                                planeCenter);
                        for(int i = 0; i < 3; i++)
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

                if(evt.shift)
                    tool.DrawBoundingBox();
            }

            return this;
        }

        void DoEditingGUI(ShapeComponent shapeComponent)
        {
            var matrix = Matrix4x4.TRS(shapeComponent.transform.position, shapeComponent.transform.rotation, Vector3.one);

            using (new Handles.DrawingScope(matrix))
            {
                DoOrientationHandlesGUI(shapeComponent, shapeComponent.mesh, shapeComponent.editionBounds);
                DoSizeHandlesGUI(shapeComponent, shapeComponent.mesh, shapeComponent.editionBounds);
            }
        }

        void DoSizeHandlesGUI(ShapeComponent shapeComponent, ProBuilderMesh mesh, Bounds bounds)
        {
            var matrix = mesh.transform.localToWorldMatrix;

            EditorShapeUtility.UpdateFaces(bounds, Vector3.zero, m_Faces);

            using (new Handles.DrawingScope(matrix))
            {
                int faceCount = m_Faces.Length;

                if(Event.current.type == EventType.Repaint)
                    m_isManipulatingSize = false;

                for(int i = 0; i < faceCount; i++)
                {
                    if(Event.current.type == EventType.Repaint)
                    {
                        Color color = DrawShapeTool.k_BoundsColor;
                        color.a *= m_Faces[i].IsVisible ? 1f : 0.5f;

                        using(new Handles.DrawingScope(color))
                        {
                            int pointsCount = m_Faces[i].Points.Length;
                            for(int k = 0; k < pointsCount; k++)
                                Handles.DrawLine(m_Faces[i].Points[k], m_Faces[i].Points[( k + 1 ) % pointsCount]);
                        }
                    }

                    if(DoFaceSizeHandle(m_Faces[i]))
                    {
                        if(!m_InitSizeInteraction)
                        {
                            m_InitSizeInteraction = true;
                            m_OriginalSize = shapeComponent.size;
                            m_OriginalCenter = shapeComponent.transform.position;
                        }

                        float modifier = 1f;
                        if(Event.current.alt)
                            modifier = 2f;

                        var sizeOffset = ProBuilderSnapping.Snap(modifier * m_SizeOffset * Math.Abs(m_Faces[i].Normal), EditorSnapping.activeMoveSnapValue);
                        var center = Event.current.alt ? Vector3.zero : Mathf.Sign(m_SizeOffset)*(sizeOffset.magnitude / 2f) * m_Faces[i].Normal;

                        EditShapeTool.ApplyProperties(shapeComponent, m_OriginalCenter + center, m_OriginalSize + sizeOffset);
                    }
                }
            }
        }

        bool DoFaceSizeHandle(EditorShapeUtility.FaceData face)
        {
            Event evt = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            float handleSize = HandleUtility.GetHandleSize(face.CenterPosition) * 0.75f;

            Vector3 startPos = face.CenterPosition + 0.25f * handleSize * face.Normal;
            Vector3 endPos = startPos + handleSize * face.Normal;

            bool isSelected = (HandleUtility.nearestControl == controlID && m_CurrentId == -1) || m_CurrentId == controlID;

            if(evt.type == EventType.Repaint)
                m_isManipulatingSize |= isSelected;

            if(m_isManipulatingOrientation)
                return false;

            switch(evt.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == controlID && (evt.button == 0 || evt.button == 2))
                    {
                        m_CurrentId = controlID;
                        GUIUtility.hotControl = controlID;
                        m_MouseStartPosition = evt.mousePosition;
                        m_InitSizeInteraction = false;
                        evt.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID && (evt.button == 0 || evt.button == 2))
                    {
                        GUIUtility.hotControl = 0;
                        evt.Use();
                        m_CurrentId = -1;
                    }
                    break;
                case EventType.Layout:
                    HandleUtility.AddControl(controlID, HandleUtility.DistanceToLine(startPos, endPos));
                    break;
                case EventType.Repaint:
                    Color color = isSelected ? EditorHandleDrawing.edgeSelectedColor : face.m_Color;
                    color.a *= face.IsVisible ? 1f : 0.25f;
                    using(new Handles.DrawingScope(color))
                        Handles.ArrowHandleCap(controlID, startPos , Quaternion.LookRotation(face.Normal), handleSize, EventType.Repaint);

                    if(isSelected)
                    {
                        color = DrawShapeTool.k_BoundsColor;
                        color.a *= 0.25f;

                        using(new Handles.DrawingScope(color))
                            Handles.DrawAAConvexPolygon(face.Points);
                    }

                    break;
                case EventType.MouseMove:

                    break;
                case EventType.MouseDrag:
                    if((HandleUtility.nearestControl == controlID && m_CurrentId == -1) || m_CurrentId == controlID)
                    {
                        m_SizeOffset = HandleUtility.CalcLineTranslation(m_MouseStartPosition, Event.current.mousePosition, m_StartPosition, face.Normal);
                        return true;
                    }

                    break;
            }
            return false;
        }

        void DoOrientationHandlesGUI(ShapeComponent shapeComponent, ProBuilderMesh mesh, Bounds bounds)
        {
            var matrix = mesh.transform.localToWorldMatrix;

            EditorShapeUtility.UpdateFaces(bounds, Vector3.zero, m_Faces);

            using (new Handles.DrawingScope(matrix))
            {
                DoCentralHandle();

                if(DoOrientationHandle())
                {
                    UndoUtility.RegisterCompleteObjectUndo(shapeComponent, "Rotate Shape");
                    shapeComponent.RotateInsideBounds(m_ShapeRotation);
                    DrawShapeTool.s_LastShapeRotation = shapeComponent.rotation;
                    ProBuilderEditor.Refresh();
                }
            }
        }

        void DoCentralHandle()
        {
            if(Event.current.type == EventType.Repaint)
            {
                if(m_isManipulatingSize)
                    return;

                int faceCount = m_Faces.Length;
                for(int i = 0; i < faceCount; i++)
                {
                    if(m_Faces[i].IsVisible)
                    {
                        float handleSize = HandleUtility.GetHandleSize(Vector3.zero) * 0.1f;

                        Color color = DrawShapeTool.k_BoundsColor;
                        color.a *= ( m_CurrentTargetedFace == null || m_CurrentTargetedFace == m_Faces[i] )
                            ? 1f
                            : 0.5f;

                        using(new Handles.DrawingScope(color))
                        {
                            int pointsCount = m_Faces[i].Points.Length;
                            for(int k = 0; k < pointsCount; k++)
                                Handles.DrawLine(m_Faces[i].Points[k], m_Faces[i].Points[( k + 1 ) % pointsCount]);

                            Handles.DrawLine(Vector3.zero, m_Faces[i].CenterPosition);
                            Handles.SphereHandleCap(-1, m_Faces[i].CenterPosition, Quaternion.identity, handleSize, EventType.Repaint);
                        }

                        if(m_CurrentTargetedFace != null)
                        {
                            handleSize = HandleUtility.GetHandleSize(Vector3.zero) * 0.5f;
                            for(int j = i + 1; j < faceCount; j++)
                            {
                                if(m_Faces[j].IsVisible)
                                {
                                    var normal = Vector3.Cross(m_Faces[i].Normal, m_Faces[j].Normal);
                                    var angle = Vector3.SignedAngle(m_Faces[i].Normal, m_Faces[j].Normal, normal);

                                    color = Color.blue;
                                    if(normal == Vector3.up || normal == Vector3.down)
                                        color = Color.green;
                                    else if(normal == Vector3.right || normal == Vector3.left)
                                        color = Color.red;

                                    using(new Handles.DrawingScope(color))
                                    {
                                        Handles.DrawWireArc(Vector3.zero, normal, m_Faces[i].Normal, angle, handleSize);
                                    }
                                }
                            }
                        }
                    }
                }

            }
        }

        bool DoOrientationHandle()
        {
            Event evt = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            bool hasRotated = false;

            float handleSize = HandleUtility.GetHandleSize(m_CurrentHandlePosition) * 0.1f;

            bool isSelected = (HandleUtility.nearestControl == controlID && m_CurrentId == -1) || m_CurrentId == controlID;
            m_isManipulatingOrientation = isSelected;

            if(m_isManipulatingSize)
                return false;

            switch(evt.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == controlID && (evt.button == 0 || evt.button == 2))
                    {
                        m_CurrentId = controlID;
                        m_CurrentTargetedFace = null;
                        m_CurrentHandlePosition = Vector3.zero;
                        GUIUtility.hotControl = controlID;

                        m_CurrentTargetedFace = null;
                        foreach(var boundsFace in m_Faces)
                        {
                            if(boundsFace.IsVisible && EditorShapeUtility.PointerIsInFace(boundsFace))
                            {
                                UnityEngine.Plane p = new UnityEngine.Plane(boundsFace.Normal,  Handles.matrix.MultiplyPoint(boundsFace.CenterPosition));

                                Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                                float dist;
                                if(p.Raycast(ray, out dist))
                                {
                                    m_CurrentHandlePosition = Handles.inverseMatrix.MultiplyPoint(ray.GetPoint(dist));
                                    m_CurrentTargetedFace = boundsFace;
                                }
                            }
                        }

                        evt.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID && (evt.button == 0 || evt.button == 2))
                    {
                        GUIUtility.hotControl = 0;
                        evt.Use();
                        m_CurrentId = -1;
                        m_CurrentTargetedFace = null;
                        m_CurrentHandlePosition = Vector3.zero;
                    }
                    break;
                case EventType.Layout:
                    foreach(var face in m_Faces)
                        HandleUtility.AddControl(controlID, HandleUtility.DistanceToCircle(face.CenterPosition, handleSize / 2.0f));
                    break;
                case EventType.Repaint:
                    if(isSelected)
                    {
                        using(new Handles.DrawingScope(EditorHandleDrawing.edgeSelectedColor))
                        {
                            Handles.DrawLine(Vector3.zero, m_CurrentHandlePosition);
                            Handles.SphereHandleCap(controlID, m_CurrentHandlePosition, Quaternion.identity, handleSize, EventType.Repaint);
                        }

                        if(isSelected && m_CurrentTargetedFace != null)
                        {
                            Color color = DrawShapeTool.k_BoundsColor;
                            color.a *= 0.25f;

                            using(new Handles.DrawingScope(color))
                                Handles.DrawAAConvexPolygon(m_CurrentTargetedFace.Points);
                        }

                    }
                    break;
                case EventType.MouseMove:
                case EventType.MouseDrag:
                    bool hit = false;
                    if((HandleUtility.nearestControl == controlID && m_CurrentId == -1) || m_CurrentId == controlID)
                    {
                        var previousFace = m_CurrentTargetedFace;
                        m_CurrentTargetedFace = null;
                        foreach(var boundsFace in m_Faces)
                        {
                            if(boundsFace.IsVisible && EditorShapeUtility.PointerIsInFace(boundsFace))
                            {
                                UnityEngine.Plane p = new UnityEngine.Plane(boundsFace.Normal,  Handles.matrix.MultiplyPoint(boundsFace.CenterPosition));

                                Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                                float dist;
                                if(p.Raycast(ray, out dist))
                                {
                                    m_CurrentHandlePosition = m_CurrentId == controlID ? Handles.inverseMatrix.MultiplyPoint(ray.GetPoint(dist)) : boundsFace.CenterPosition;
                                    m_CurrentTargetedFace = boundsFace;
                                    hit = true;
                                }
                            }
                        }

                        if(m_CurrentTargetedFace != null && previousFace != null && m_CurrentTargetedFace != previousFace)
                        {
                            Vector3 rotationAxis = Vector3.Cross(previousFace.Normal, m_CurrentTargetedFace.Normal);
                            m_ShapeRotation = Quaternion.AngleAxis(Vector3.SignedAngle(previousFace.Normal, m_CurrentTargetedFace.Normal,rotationAxis),rotationAxis);
                            hasRotated = true;
                        }
                    }
                    if(!hit)
                        m_CurrentTargetedFace = null;

                    break;
            }
            return hasRotated;
        }

    }
}

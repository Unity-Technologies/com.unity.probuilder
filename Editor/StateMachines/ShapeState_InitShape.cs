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
        bool m_BoundsHandleActive = false;
        EditorShapeUtility.BoundsState m_ActiveBoundsState;
        BoxBoundsHandle m_BoundsHandle;
        EditorShapeUtility.FaceData[] m_Faces;

        //Handle Manipulation
        Quaternion m_ShapeRotation = Quaternion.identity;
        bool m_IsMouseOver = false;
        int m_CurrentId = -1;
        Vector3 m_CurrentHandlePosition = Vector3.zero;
        EditorShapeUtility.FaceData m_CurrentTargetedFace = null;

        protected override void InitState()
        {
            tool.m_IsShapeInit = false;

            //Init edition tool
            m_BoundsHandle = new BoxBoundsHandle();

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
            {
                DoEditingGUI(tool.m_LastShapeCreated);
            }

            if(GUIUtility.hotControl != 0)
                return this;

            if(!m_IsMouseOver)
            {
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
                }

                tool.DrawBoundingBox();
            }

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
                    shapeComponent.transform, shapeComponent.editionBounds,
                    m_BoundsHandle, m_BoundsHandleActive, m_ActiveBoundsState);

                if(Event.current.shift)
                {
                    DoOrientationHandlesGUI(shapeComponent, shapeComponent.mesh, shapeComponent.editionBounds);
                }
                else
                {
                    EditorGUI.BeginChangeCheck();

                    if(m_CurrentId == -1)
                        m_BoundsHandle.DrawHandle();

                    if(EditorGUI.EndChangeCheck())
                    {
                        if(!m_BoundsHandleActive)
                            BeginBoundsEditing(shapeComponent);
                        UndoUtility.RegisterCompleteObjectUndo(shapeComponent, "Scale Shape");
                        EditorShapeUtility.CopyHandlePropertiesToCollider(m_BoundsHandle, m_ActiveBoundsState);
                        EditShapeTool.ApplyProperties(shapeComponent, m_ActiveBoundsState);
                        DrawShapeTool.s_Size.value = m_BoundsHandle.size;
                    }
                }

            }
        }

        void BeginBoundsEditing(ShapeComponent shape)
        {
            UndoUtility.RecordComponents<ShapeComponent, ProBuilderMesh, Transform>(
                new[] { shape },
                string.Format("Modify {0}", ObjectNames.NicifyVariableName(shape.mesh.gameObject.GetType().Name)));

            m_BoundsHandleActive = true;
            Bounds localBounds = shape.editionBounds;
            m_ActiveBoundsState = new EditorShapeUtility.BoundsState()
            {
                positionAndRotationMatrix = Matrix4x4.TRS(shape.transform.position, shape.transform.rotation, Vector3.one),
                boundsHandleValue = localBounds,
            };
        }

        void DoOrientationHandlesGUI(ShapeComponent shapeComponent, ProBuilderMesh mesh, Bounds bounds)
        {
            var matrix = mesh.transform.localToWorldMatrix;

            EditorShapeUtility.UpdateFaces(bounds, Vector3.zero, m_Faces);
            using (new Handles.DrawingScope(matrix))
            {
                foreach(var face in m_Faces)
                    DoFaceGUI(face);

                DoCenterHandle();

                if(DoOrientationHandle())
                {
                    UndoUtility.RegisterCompleteObjectUndo(shapeComponent, "Rotate Shape");
                    shapeComponent.RotateInsideBounds(m_ShapeRotation);
                    DrawShapeTool.s_LastShapeRotation = shapeComponent.rotation;
                    ProBuilderEditor.Refresh();
                }
            }
        }

        void DoCenterHandle()
        {
            if(Event.current.type == EventType.Repaint)
            {
                int faceCount = m_Faces.Length;
                for(int i = 0; i < faceCount; i++)
                {
                    if(m_Faces[i].IsVisible)
                    {
                        float handleSize = HandleUtility.GetHandleSize(Vector3.zero) * 0.1f;

                        Color color = DrawShapeTool.k_BoundsColor;
                        color.a *= ( m_CurrentTargetedFace == null || m_CurrentTargetedFace == m_Faces[i] )
                            ? 1f
                            : 0.25f;
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
                    bool isSelected = (HandleUtility.nearestControl == controlID && m_CurrentId == -1) || m_CurrentId == controlID;
                    m_IsMouseOver |= isSelected;
                    if(isSelected)
                    {
                        using(new Handles.DrawingScope(EditorHandleDrawing.edgeSelectedColor))
                        {
                            Handles.DrawLine(Vector3.zero, m_CurrentHandlePosition);
                            Handles.SphereHandleCap(controlID, m_CurrentHandlePosition, Quaternion.identity, handleSize, EventType.Repaint);
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

                        if(m_CurrentTargetedFace != null && m_CurrentTargetedFace != previousFace)
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

        // bool DoOrientationHandle(EditorShapeUtility.FaceData face)
        // {
        //     Event evt = Event.current;
        //     int controlID = GUIUtility.GetControlID(FocusType.Passive);
        //     bool hasRotated = false;
        //
        //     float handleSize = HandleUtility.GetHandleSize(face.CenterPosition) * 0.25f;
        //     if(face.IsVisible)
        //     {
        //         switch(evt.GetTypeForControl(controlID))
        //         {
        //             case EventType.MouseDown:
        //                 if (HandleUtility.nearestControl == controlID && (evt.button == 0 || evt.button == 2))
        //                 {
        //                     m_CurrentId = controlID;
        //                     m_CurrentTargetedFace = null;
        //                     m_CurrentHandlePosition = Vector3.zero;
        //                     GUIUtility.hotControl = controlID;
        //                     evt.Use();
        //                 }
        //                 break;
        //             case EventType.MouseUp:
        //                 if (GUIUtility.hotControl == controlID && (evt.button == 0 || evt.button == 2))
        //                 {
        //                     GUIUtility.hotControl = 0;
        //                     evt.Use();
        //                     m_CurrentId = -1;
        //
        //                     if(m_CurrentTargetedFace != null && m_CurrentTargetedFace != face)
        //                     {
        //                         Vector3 rotationAxis = Vector3.Cross(face.Normal, m_CurrentTargetedFace.Normal);
        //                         m_ShapeRotation = Quaternion.AngleAxis(Vector3.SignedAngle(face.Normal, m_CurrentTargetedFace.Normal,rotationAxis),rotationAxis);
        //                         hasRotated = true;
        //                     }
        //
        //                     m_CurrentTargetedFace = null;
        //                     m_CurrentHandlePosition = Vector3.zero;
        //                 }
        //                 break;
        //             case EventType.MouseMove:
        //                 HandleUtility.Repaint();
        //                 break;
        //             case EventType.Layout:
        //                 HandleUtility.AddControl(controlID, HandleUtility.DistanceToDisc(face.CenterPosition, face.Normal, handleSize / 2f) * 0.5f);
        //                 break;
        //             case EventType.Repaint:
        //                 bool isSelected = (HandleUtility.nearestControl == controlID && m_CurrentId == -1) || m_CurrentId == controlID;
        //                 m_IsMouseOver |= isSelected;
        //
        //                 using(new Handles.DrawingScope(DrawShapeTool.k_BoundsColor))
        //                 {
        //                     int pointsCount = face.Points.Length;
        //                     for(int i = 0; i < pointsCount; i++)
        //                         Handles.DrawLine(face.Points[i], face.Points[( i + 1 ) % pointsCount]);
        //                 }
        //
        //                 var faceColor = m_CurrentTargetedFace == face ? EditorHandleDrawing.faceSelectedColor : DrawShapeTool.k_BoundsColor;
        //                 using(new Handles.DrawingScope(faceColor * new Color(1f, 1f, 1f, 0.2f)))
        //                     Handles.DrawAAConvexPolygon(m_CurrentTargetedFace.Points);
        //
        //                 if(m_CurrentId == -1 || m_CurrentId == controlID)
        //                 {
        //                     using(new Handles.DrawingScope(isSelected
        //                         ? EditorHandleDrawing.edgeSelectedColor
        //                         : face.m_Color))
        //                     {
        //                         var position = isSelected && m_CurrentHandlePosition != Vector3.zero ? m_CurrentHandlePosition : face.CenterPosition;
        //                         var normal = isSelected && m_CurrentTargetedFace != null ? m_CurrentTargetedFace.Normal : face.Normal;
        //                         Handles.DrawLine(position, position + normal * handleSize);
        //                         Handles.CircleHandleCap(controlID, position, Quaternion.LookRotation(normal), handleSize, EventType.Repaint);
        //
        //                         if(isSelected)
        //                         {
        //                             using(new Handles.DrawingScope(face.m_Color * new Color(1f, 1f, 1f, 0.25f)))
        //                                 Handles.DrawSolidDisc(position, normal, handleSize);
        //
        //                             if(m_CurrentTargetedFace != null)
        //                                 using(new Handles.DrawingScope(m_CurrentTargetedFace.m_Color * new Color(1f, 1f, 1f, 0.1f)))
        //                                     Handles.DrawAAConvexPolygon(m_CurrentTargetedFace.Points);
        //                         }
        //                     }
        //                 }
        //                 break;
        //             case EventType.MouseDrag:
        //                 if(m_CurrentId == controlID)
        //                 {
        //                     m_CurrentTargetedFace = null;
        //                     foreach(var boundsFace in m_Faces)
        //                     {
        //                         if(boundsFace.IsVisible && EditorShapeUtility.PointerIsInFace(boundsFace))
        //                         {
        //                             UnityEngine.Plane p = new UnityEngine.Plane(boundsFace.Normal,  Handles.matrix.MultiplyPoint(boundsFace.CenterPosition));
        //
        //                             Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
        //                             float dist;
        //                             if(p.Raycast(ray, out dist))
        //                             {
        //                                 m_CurrentHandlePosition = Handles.inverseMatrix.MultiplyPoint(ray.GetPoint(dist));
        //                                 m_CurrentTargetedFace = boundsFace;
        //                             }
        //                         }
        //                     }
        //                 }
        //                 break;
        //         }
        //     }
        //
        //     return hasRotated;
        // }

    }
}

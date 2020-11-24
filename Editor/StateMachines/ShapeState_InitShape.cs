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
        //hashset to avoid drawing twice the same edge
        HashSet<EditorShapeUtility.EdgeData> m_EdgesToDraw = new HashSet<EditorShapeUtility.EdgeData>(new EditorShapeUtility.EdgeDataComparer());
        EditorShapeUtility.FaceData[] m_Faces;
        Dictionary<EditorShapeUtility.EdgeData, SimpleTuple<EditorShapeUtility.EdgeData, EditorShapeUtility.EdgeData>> m_EdgeDataToNeighborsEdges;

        //Handle Manipulation
        Vector2 m_StartMousePosition;
        Vector3 m_StartPosition;
        Vector3 m_CurrentHandlePos;
        Quaternion m_LastRotation;
        Quaternion m_ShapeRotation = Quaternion.identity;
        int m_CurrentId = -1;
        bool m_IsMouseDown;
        bool m_IsMouseOver = false;
        int m_hotControl;

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

                DoRotateHandlesGUI(shapeComponent, shapeComponent.mesh, shapeComponent.editionBounds);

                EditorGUI.BeginChangeCheck();

                if(m_hotControl == 0)
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

        void DoRotateHandlesGUI(ShapeComponent shapeComponent, ProBuilderMesh mesh, Bounds bounds)
        {
            var matrix = mesh.transform.localToWorldMatrix;

            m_EdgesToDraw.Clear();
            EditorShapeUtility.UpdateFaces(bounds, Vector3.zero, m_Faces, m_EdgeDataToNeighborsEdges);
            using (new Handles.DrawingScope(matrix))
            {
                m_IsMouseOver = false;
                foreach(var face in m_Faces)
                {
                    if(FaceOrientationHandle(face))
                    {
                        shapeComponent.RotateInsideBounds(m_ShapeRotation);
                    }
                }

                // foreach(var edgeData in m_EdgesToDraw)
                // {
                //     Quaternion rot;
                //     if(RotateEdgeHandle(edgeData, out rot))
                //     {
                //         UndoUtility.RegisterCompleteObjectUndo(shapeComponent, "Rotate Shape");
                //         shapeComponent.RotateInsideBounds(rot);
                //         DrawShapeTool.s_LastShapeRotation = shapeComponent.rotation;
                //         ProBuilderEditor.Refresh();
                //     }
                // }
            }
        }

        bool FaceOrientationHandle(EditorShapeUtility.FaceData face)
        {
            if(face.IsVisible)
            {
                bool mouseUp = false;
                if(Event.current.type == EventType.MouseUp)
                    mouseUp = true;

                int controlID = GUIUtility.GetControlID(FocusType.Passive);
                if(m_hotControl == 0 || m_hotControl == controlID)
                {
                    Transform camTransform = Camera.current.transform;
                    Vector3 camUpToHandle = Handles.matrix.MultiplyVector(camTransform.up);
                    Vector3 camRightToHandle = Handles.matrix.MultiplyVector(camTransform.right);

                    float handleSize = HandleUtility.GetHandleSize(face.PlacementPosition) * 0.1f;
                    var pos = face.PlacementPosition + handleSize * camUpToHandle;

                    if(m_hotControl == 0)
                        m_CurrentHandlePos = face.PlacementPosition - 1.5f * handleSize * camRightToHandle ;

                    float distToMouse = Vector2.Distance(HandleUtility.WorldToGUIPoint(m_CurrentHandlePos),
                        Event.current.mousePosition);
                    float distMax = Vector2.Distance(HandleUtility.WorldToGUIPoint(m_CurrentHandlePos),
                        HandleUtility.WorldToGUIPoint(m_CurrentHandlePos + Vector3.one * handleSize));

                    m_IsMouseOver |= distToMouse < distMax;

                    using(new Handles.DrawingScope(face.m_Color))
                    {
                        if(m_hotControl == 0)
                            Handles.Label(pos, face.m_Label, face.m_Style);
                        else
                            Handles.Label(pos, "Move To");

                        EditorGUI.BeginChangeCheck();
                        m_CurrentHandlePos = Handles.FreeMoveHandle(controlID, m_CurrentHandlePos, Quaternion.identity,
                            handleSize, Vector3.zero, Handles.CircleHandleCap);

                        if(EditorGUI.EndChangeCheck())
                            m_hotControl = EditorGUIUtility.hotControl;
                    }

                    if(m_hotControl == controlID)
                    {
                        Handles.DrawLine(face.PlacementPosition, m_CurrentHandlePos);
                        m_ShapeRotation = Quaternion.identity;

                        UnityEngine.Plane p = new UnityEngine.Plane(Handles.inverseMatrix.MultiplyVector(camTransform.forward), face.PlacementPosition);
                        var camPosition = Handles.inverseMatrix.MultiplyPoint(camTransform.position);

                        var hoverDistance = Single.PositiveInfinity;
                        foreach(var otherFace in m_Faces)
                        {
                            if(otherFace != face)
                            {
                                // //Solution 1 : display direction around the current position -> less distance
                                //  var angle = Vector3.SignedAngle(face.Normal, otherFace.Normal, Vector3.up);
                                //  var sinAngle = Mathf.Sin(angle * Mathf.Deg2Rad);
                                //  var currentDir =   Mathf.Abs(sinAngle) > 0.1f ?
                                //                  -Mathf.Sign(sinAngle) * camRightToHandle.normalized :
                                //                  -0.5f * camUpToHandle.normalized;
                                //
                                //  var currentPos = pos + 10f * handleSize * currentDir;
                                //  var dist = Vector3.Distance(currentPos, m_CurrentHandlePos);
                                // bool isHovered = dist < 5f * handleSize * currentDir.magnitude;

                                //Solution 2 position coherent to faces places
                                var currentPos = otherFace.PlacementPosition;
                                Ray ray = new Ray(camPosition, currentPos - camPosition);
                                currentPos = p.ClosestPointOnPlane(currentPos);

                                if(!Camera.current.orthographic)
                                {
                                    float hit;
                                    if(p.Raycast(ray, out hit))
                                        currentPos = ray.GetPoint(hit);
                                }

                                var dist = Vector3.Distance(currentPos, m_CurrentHandlePos);
                                bool isHovered = dist < 5f * handleSize && dist < hoverDistance;

                                if(isHovered)
                                {
                                    hoverDistance = dist;
                                    if(Mathf.Abs(Vector3.Dot(face.Normal, Vector3.up)) < Mathf.Epsilon &&
                                       Mathf.Abs(Vector3.Dot(otherFace.Normal, Vector3.up)) < Mathf.Epsilon)
                                    {
                                        m_ShapeRotation = Quaternion.AngleAxis(
                                            Vector3.SignedAngle(face.Normal, otherFace.Normal, Vector3.up), Vector3.up);
                                    }
                                    else
                                    {
                                        if(Mathf.Abs(Vector3.Dot(face.Normal, otherFace.Normal)) < Mathf.Epsilon)
                                        {
                                            Vector3 rotationAxis = Vector3.Cross(face.Normal, otherFace.Normal);

                                            m_ShapeRotation = Quaternion.AngleAxis(
                                                Vector3.SignedAngle(face.Normal, otherFace.Normal, rotationAxis), rotationAxis);
                                        }
                                        else // both normals are on the Y axis, rotate around X axis
                                        {
                                            m_ShapeRotation = Quaternion.AngleAxis(
                                                Vector3.SignedAngle(face.Normal, otherFace.Normal, Vector3.right), Vector3.right);
                                        }
                                    }
                                }
                            }
                        }

                        foreach(var otherFace in m_Faces)
                        {
                            if(otherFace != face)
                            {
                                // //Solution 1 : display direction around the current position -> less distance
                                // var angle = Vector3.SignedAngle(face.Normal, otherFace.Normal, Vector3.up);
                                // var sinAngle = Mathf.Sin(angle * Mathf.Deg2Rad);
                                // var currentDir =   Mathf.Abs(sinAngle) > 0.1f ?
                                //                 -Mathf.Sign(sinAngle) * camRightToHandle.normalized :
                                //                 -0.5f * camUpToHandle.normalized;
                                //
                                // var currentPos = pos + 10f * handleSize * currentDir;
                                // var dist = Vector3.Distance(currentPos, m_CurrentHandlePos);
                                // bool isHovered = dist < 5f * handleSize * currentDir.magnitude;

                                //Solution 2 position coherent to faces places
                                var currentPos = otherFace.PlacementPosition;
                                Ray ray = new Ray(camPosition, currentPos - camPosition);
                                currentPos = p.ClosestPointOnPlane(currentPos);

                                if(!Camera.current.orthographic)
                                {
                                    float hit;
                                    if(p.Raycast(ray, out hit))
                                        currentPos = ray.GetPoint(hit);
                                }

                                var dist = Vector3.Distance(currentPos, m_CurrentHandlePos);
                                bool isHovered = Math.Approx(dist,hoverDistance);

                                Handles.Label(currentPos, otherFace.m_Label,isHovered ? otherFace.m_Style : GUI.skin.label);
                            }
                        }
                    }
                }

                if (m_hotControl != 0 && (mouseUp || Event.current.type == EventType.MouseLeaveWindow))
                {
                    m_hotControl = 0;
                    return m_ShapeRotation != Quaternion.identity;
                }
            }

            return false;
        }

    }
}

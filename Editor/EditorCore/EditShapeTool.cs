using System.Collections.Generic;
using UnityEngine;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;
using Math = UnityEngine.ProBuilder.Math;

#if UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.ToolManager;
#else
using ToolManager = UnityEditor.EditorTools.EditorTools;
#endif

namespace UnityEditor.ProBuilder
{
    [EditorTool("Edit Shape", typeof(ShapeComponent))]
    public sealed class EditShapeTool: EditorTool
    {
        const int k_HotControlNone = 0;
        BoxBoundsHandle m_BoundsHandle;
        bool m_BoundsHandleActive;

        Vector2 m_StartMousePosition;
        Vector3 m_StartPosition;
        Quaternion m_LastRotation;
        int m_CurrentId = -1;
        bool m_IsMouseDown;

        // Don't recalculate the active bounds during an edit operation, it causes the handles to drift
        ShapeState m_ActiveShapeState;

        bool m_AskingForReset = false;
        const string k_dialogTitle = "Warning : Shape modified";
        const string k_dialogText = "The current shape has been manually edited, by editing it you will loose all modifications.";


        [Range(1,90)]
        [SerializeField]
        int m_snapAngle = 15;

        GUIContent m_OverlayTitle;
        GUIContent m_SnapAngleContent;

        struct ShapeState
        {
            public Matrix4x4 positionAndRotationMatrix;
            public Bounds boundsHandleValue;
        }

        FaceData[] m_Faces = new FaceData[6];

        sealed class FaceData
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

        struct EdgeData
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

        //hashset to avoid drawing twice the same edge
        HashSet<EdgeData> edgesToDraw = new HashSet<EdgeData>(new EdgeDataComparer());

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

        bool IsEditing => m_BoundsHandleActive;

        void OnEnable()
        {
            m_BoundsHandle = new BoxBoundsHandle();
            for (int i = 0; i < m_Faces.Length; i++)
            {
                m_Faces[i] = new FaceData();
            }
            m_OverlayTitle = new GUIContent("Poly Shape Tool");
            m_SnapAngleContent = new GUIContent("Snap Angle", L10n.Tr("Defines an angle in [1,90] to snap rotation."));

            ToolManager.activeToolChanged += ValidateCurrentTargets;
            MeshSelection.objectSelectionChanged += ValidateCurrentTargets;
        }

        void ValidateCurrentTargets()
        {
            if(ToolManager.IsActiveTool(this))
            {
                foreach(var obj in targets)
                {
                    var shapeComponent = obj as ShapeComponent;
                    if(shapeComponent.edited)
                    {
                        UndoUtility.RegisterCompleteObjectUndo(shapeComponent, "Change Shape");
                        shapeComponent.SetShape(EditorShapeUtility.CreateCustomShapeFromMesh(shapeComponent));
                        shapeComponent.edited = false;
                        ProBuilderEditor.Refresh(false);
                    }
                }
            }
        }

        public override void OnToolGUI(EditorWindow window)
        {
            SceneViewOverlay.Window( m_OverlayTitle, OnOverlayGUI, 0, SceneViewOverlay.WindowDisplayOption.OneWindowPerTitle );

            foreach (var obj in targets)
            {
                var shape = obj as ShapeComponent;

                if (shape != null)
                {
                    if(m_BoundsHandleActive && GUIUtility.hotControl == k_HotControlNone)
                        EndBoundsEditing();

                    if(Mathf.Approximately(shape.transform.lossyScale.sqrMagnitude, 0f))
                        return;

                    DoShapeGUI(shape);
                }
            }

        }

        void OnOverlayGUI(Object target, SceneView view)
        {
            m_snapAngle = EditorGUILayout.IntSlider(m_SnapAngleContent, m_snapAngle, 1, 90);
        }

        // void DisplayShapeResetDialog(ShapeComponent shape)
        // {
        //     if(UnityEditor.EditorUtility.DisplayDialog(
        //         k_dialogTitle, k_dialogText,
        //         "Continue", "Cancel"))
        //     {
        //         shape.edited = false;
        //         shape.Rebuild();
        //     }
        //     else
        //     {
        //         ToolManager.RestorePreviousTool();
        //     }
        //     m_AskingForReset = false;
        // }

        void DoShapeGUI(ShapeComponent shapeComponent)
        {
            // if(shape.edited)
            // {
            //     if(!m_AskingForReset)
            //     {
            //         m_AskingForReset = true;
            //         EditorApplication.delayCall += () => DisplayShapeResetDialog(shape);
            //     }
            //     return;
            // }

            var matrix = IsEditing
                ? m_ActiveShapeState.positionAndRotationMatrix
                : Matrix4x4.TRS(shapeComponent.transform.position, shapeComponent.transform.rotation, Vector3.one);

            using (new Handles.DrawingScope(matrix))
            {
                m_BoundsHandle.SetColor(Handles.s_PreselectionColor);

                CopyColliderPropertiesToHandle(shapeComponent);

                EditorGUI.BeginChangeCheck();

                m_BoundsHandle.DrawHandle();

                if (EditorGUI.EndChangeCheck())
                {
                    BeginBoundsEditing(shapeComponent);
                    UndoUtility.RegisterCompleteObjectUndo(shapeComponent, "Scale Shape");
                    CopyHandlePropertiesToCollider(shapeComponent);
                }

                DoRotateHandlesGUI(shapeComponent, shapeComponent.meshFilterBounds);
            }
        }

        void UpdateFaces(Vector3 extents)
        {
            EdgeData edgeX1 = new EdgeData(new Vector3(extents.x, extents.y, extents.z), new Vector3(-extents.x, extents.y, extents.z));
            EdgeData edgeX2 = new EdgeData(new Vector3(extents.x, -extents.y, extents.z), new Vector3(-extents.x, -extents.y, extents.z));
            EdgeData edgeX3 = new EdgeData(new Vector3(extents.x, extents.y, -extents.z), new Vector3(-extents.x, extents.y, -extents.z));
            EdgeData edgeX4 = new EdgeData(new Vector3(extents.x, -extents.y, -extents.z), new Vector3(-extents.x, -extents.y, -extents.z));

            EdgeData edgeY1 = new EdgeData(new Vector3(extents.x, extents.y, extents.z), new Vector3(extents.x, -extents.y, extents.z));
            EdgeData edgeY2 = new EdgeData(new Vector3(-extents.x, extents.y, extents.z), new Vector3(-extents.x, -extents.y, extents.z));
            EdgeData edgeY3 = new EdgeData(new Vector3(extents.x, extents.y, -extents.z), new Vector3(extents.x, -extents.y, -extents.z));
            EdgeData edgeY4 = new EdgeData(new Vector3(-extents.x, extents.y, -extents.z), new Vector3(-extents.x, -extents.y, -extents.z));

            EdgeData edgeZ1 = new EdgeData(new Vector3(extents.x, extents.y, extents.z), new Vector3(extents.x, extents.y, -extents.z));
            EdgeData edgeZ2 = new EdgeData(new Vector3(-extents.x, extents.y, extents.z), new Vector3(-extents.x, extents.y, -extents.z));
            EdgeData edgeZ3 = new EdgeData(new Vector3(extents.x, -extents.y, extents.z), new Vector3(extents.x, -extents.y, -extents.z));
            EdgeData edgeZ4 = new EdgeData(new Vector3(-extents.x, -extents.y, extents.z), new Vector3(-extents.x, -extents.y, -extents.z));

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

        void DoRotateHandlesGUI(ShapeComponent shapeComponent, Bounds bounds)
        {
            var matrix = shapeComponent.gameObject.transform.localToWorldMatrix;
            bool hasRotated = false;

            edgesToDraw.Clear();
            UpdateFaces(bounds.extents);
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
                    {
                        UndoUtility.RegisterCompleteObjectUndo(shapeComponent, "Rotate Shape");
                        shapeComponent.Rotate(rot);
                        hasRotated = true;
                    }
                }

                if (hasRotated)
                    ProBuilderEditor.Refresh();
            }
        }

        bool RotateEdgeHandle(EdgeData edge, out Quaternion rotation)
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

                    using (new Handles.DrawingScope(isSelected ? Color.white : Handles.s_PreselectionColor))
                    {
                        Handles.DrawAAPolyLine(isSelected ? 10f : 3f, edge.PointA, edge.PointB);
                    }
                    break;
                case EventType.MouseDrag:
                    if (m_IsMouseDown && m_CurrentId == controlID)
                    {
                        Vector3 axis = edge.PointA - edge.PointB;
                        Vector3 axisToPrevious = Handles.matrix * (m_EdgeDataToNeighborsEdges[edge].item1.Center - edge.Center);
                        Vector3 axisToNext =  Handles.matrix * (m_EdgeDataToNeighborsEdges[edge].item2.Center - edge.Center);

                        //Get a direction orthogonal to both direction to camera and edge direction
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
                positionAndRotationMatrix = Matrix4x4.TRS(shape.transform.position, shape.transform.rotation, Vector3.one),
                boundsHandleValue = localBounds,
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
            Vector3 snappedHandleSize = ProBuilderSnapping.Snap(m_BoundsHandle.size, EditorSnapping.activeMoveSnapValue);
            //Find the scaling direction
            Vector3 centerDiffSign = ( m_BoundsHandle.center - m_ActiveShapeState.boundsHandleValue.center ).normalized;
            Vector3 sizeDiffSign = ( m_BoundsHandle.size - m_ActiveShapeState.boundsHandleValue.size ).normalized;
            Vector3 globalSign = Vector3.Scale(centerDiffSign,sizeDiffSign);
            //Set the center to the right position
            Vector3 center = m_ActiveShapeState.boundsHandleValue.center + Vector3.Scale((snappedHandleSize - m_ActiveShapeState.boundsHandleValue.size)/2f,globalSign);
            //Set new Bounding box value
            m_ActiveShapeState.boundsHandleValue = new Bounds(center, snappedHandleSize);

            var bounds = new Bounds();
            var trs = shape.transform;

            bounds.center = Handles.matrix.MultiplyPoint3x4(m_ActiveShapeState.boundsHandleValue.center);
            bounds.size = Math.Abs(Vector3.Scale(m_ActiveShapeState.boundsHandleValue.size, Math.InvertScaleVector(trs.lossyScale)));

            shape.Rebuild(bounds, shape.transform.rotation);
            shape.mesh.SetPivot(shape.transform.position);
            ProBuilderEditor.Refresh(false);
        }

    }
}

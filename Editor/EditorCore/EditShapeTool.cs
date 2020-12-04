using System;
using System.Linq;
using UnityEngine;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;
using Math = UnityEngine.ProBuilder.Math;
using Object = UnityEngine.Object;

using FaceData = UnityEditor.ProBuilder.EditorShapeUtility.FaceData;

namespace UnityEditor.ProBuilder
{
    [EditorTool("Edit Shape", typeof(ShapeComponent))]
    sealed class EditShapeTool : EditorTool
    {
        Editor m_ShapeEditor;

        static readonly Color k_BoundsHandleColor = new Color(.2f, .4f, .8f, 1f);

        GUIContent m_SnapAngleContent;

        GUIContent m_OverlayTitle;

        static FaceData[] s_Faces;

        public static FaceData[] Faces
        {
            get
            {
                if(s_Faces == null)
                {
                    s_Faces = new FaceData[6];
                    for(int i = 0; i < s_Faces.Length; i++)
                        s_Faces[i] = new FaceData();
                }
                return s_Faces;
            }
        }

        static bool s_UpdateDrawShapeTool = false;

        //Handle Manipulation
        static int s_CurrentId = -1;
        static ShapeComponent s_CurrentShape = null;

        //Manage orientation and size interop
        static bool s_IsManipulatingSize = false;
        static bool s_IsManipulatingOrientation = false;

        //Size Handle management
        static bool s_InitSizeInteraction = true;
        static Vector3 s_OriginalSize;
        static Vector3 s_OriginalCenter;
        static Vector2 s_MouseStartPosition;
        static float s_SizeDelta;

        //Orientation Handle Manipulation
        static Quaternion s_ShapeRotation = Quaternion.identity;
        static Vector3 s_CurrentHandlePosition = Vector3.zero;
        static FaceData s_CurrentTargetedFace = null;

        public override GUIContent toolbarIcon
        {
            get { return PrimitiveBoundsHandle.editModeButton; }
        }

        void OnEnable()
        {
            InitTool();
            m_OverlayTitle = new GUIContent("Edit Shape Tool");
        }

        protected void InitTool()
        {
            m_SnapAngleContent = new GUIContent("Rotation Snap", L10n.Tr("Defines an angle in [1,90] to snap rotation."));
        }

        void OnDisable()
        {
            if(m_ShapeEditor != null)
                DestroyImmediate(m_ShapeEditor);
        }

        public override void OnToolGUI(EditorWindow window)
        {
            SceneViewOverlay.Window( m_OverlayTitle, OnOverlayGUI, 0, SceneViewOverlay.WindowDisplayOption.OneWindowPerTitle );

            foreach(var obj in targets)
            {
                var shape = obj as ShapeComponent;

                if (shape != null && !shape.edited)
                {
                    DoEditingGUI(shape);
                }
            }
        }

        void OnOverlayGUI(Object obj, SceneView view)
        {
            Editor.CreateCachedEditor(targets.ToArray(), typeof(ShapeComponentEditor), ref m_ShapeEditor);
            ( (ShapeComponentEditor) m_ShapeEditor ).DrawShapeGUI(null);
            ( (ShapeComponentEditor) m_ShapeEditor ).DrawShapeParametersGUI(null);

            EditorSnapSettings.gridSnapEnabled = EditorGUILayout.Toggle("Snap To Grid", EditorSnapSettings.gridSnapEnabled);
        }

        internal static void DoEditingGUI(ShapeComponent shapeComponent, bool updateDrawShapeTool = false)
        {
            if(shapeComponent == null || (s_CurrentShape != null && shapeComponent != s_CurrentShape) )
                return;

            s_UpdateDrawShapeTool = updateDrawShapeTool;

            var matrix = Matrix4x4.TRS(shapeComponent.transform.position, shapeComponent.transform.rotation, Vector3.one);

            using (new Handles.DrawingScope(matrix))
            {
                EditorShapeUtility.UpdateFaces(shapeComponent.editionBounds, Vector3.zero, Faces);
                DoOrientationHandlesGUI(shapeComponent, shapeComponent.mesh, shapeComponent.editionBounds);
                DoSizeHandlesGUI(shapeComponent, shapeComponent.mesh, shapeComponent.editionBounds);
            }
        }

        static void DoSizeHandlesGUI(ShapeComponent shapeComponent, ProBuilderMesh mesh, Bounds bounds)
        {
            var matrix = mesh.transform.localToWorldMatrix;
            using (new Handles.DrawingScope(matrix))
            {
                int faceCount = s_Faces.Length;

                if(Event.current.type == EventType.Repaint)
                    s_IsManipulatingSize = false;

                for(int i = 0; i < faceCount; i++)
                {
                    if(Event.current.type == EventType.Repaint)
                    {
                        Color color = k_BoundsHandleColor;
                        color.a *= Faces[i].IsVisible ? 1f : 0.5f;

                        using(new Handles.DrawingScope(color))
                        {
                            int pointsCount = Faces[i].Points.Length;
                            for(int k = 0; k < pointsCount; k++)
                                Handles.DrawLine(Faces[i].Points[k], Faces[i].Points[( k + 1 ) % pointsCount]);
                        }
                    }

                    if(DoFaceSizeHandle(shapeComponent, Faces[i]))
                    {
                        if(!s_InitSizeInteraction)
                        {
                            s_InitSizeInteraction = true;
                            s_OriginalSize = shapeComponent.size;
                            s_OriginalCenter = shapeComponent.transform.position;
                        }

                        float modifier = 1f;
                        if(Event.current.alt)
                            modifier = 2f;

                        var sizeOffset = ProBuilderSnapping.Snap(modifier * s_SizeDelta * Math.Abs(s_Faces[i].Normal), EditorSnapping.activeMoveSnapValue);
                        var center = Event.current.alt ? Vector3.zero : Mathf.Sign(s_SizeDelta)*(sizeOffset.magnitude / 2f) * s_Faces[i].Normal;

                        ApplyProperties(shapeComponent, s_OriginalCenter + center, s_OriginalSize + sizeOffset);
                    }
                }
            }
        }

        static bool DoFaceSizeHandle(ShapeComponent shapeComponent, FaceData face)
        {
            Event evt = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            float handleSize = HandleUtility.GetHandleSize(face.CenterPosition) * 0.75f;

            Vector3 startPos = face.CenterPosition + 0.25f * handleSize * face.Normal;
            Vector3 endPos = startPos + handleSize * face.Normal;

            bool isSelected = (HandleUtility.nearestControl == controlID && s_CurrentId == -1) || s_CurrentId == controlID;

            if(evt.type == EventType.Repaint)
                s_IsManipulatingSize |= isSelected;

            if(s_IsManipulatingOrientation)
                return false;

            if(s_IsManipulatingSize)
                s_CurrentShape = shapeComponent;
            else if(s_CurrentShape == shapeComponent)
                s_CurrentShape = null;

            switch(evt.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == controlID && (evt.button == 0 || evt.button == 2))
                    {
                        s_CurrentId = controlID;
                        GUIUtility.hotControl = controlID;
                        s_MouseStartPosition = evt.mousePosition;
                        s_InitSizeInteraction = false;
                        evt.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID && (evt.button == 0 || evt.button == 2))
                    {
                        GUIUtility.hotControl = 0;
                        evt.Use();
                        s_CurrentId = -1;
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
                        color = k_BoundsHandleColor;
                        color.a *= 0.25f;

                        using(new Handles.DrawingScope(color))
                            Handles.DrawAAConvexPolygon(face.Points);
                    }

                    break;
                case EventType.MouseMove:

                    break;
                case EventType.MouseDrag:
                    if((HandleUtility.nearestControl == controlID && s_CurrentId == -1) || s_CurrentId == controlID)
                    {
                        s_SizeDelta = HandleUtility.CalcLineTranslation(s_MouseStartPosition, Event.current.mousePosition, face.CenterPosition, face.Normal);
                        return true;
                    }

                    break;
            }
            return false;
        }

        static void DoOrientationHandlesGUI(ShapeComponent shapeComponent, ProBuilderMesh mesh, Bounds bounds)
        {
            var matrix = mesh.transform.localToWorldMatrix;

            EditorShapeUtility.UpdateFaces(bounds, Vector3.zero, Faces);

            using (new Handles.DrawingScope(matrix))
            {
                DoCentralHandle();

                if(DoOrientationHandle(shapeComponent))
                {
                    UndoUtility.RegisterCompleteObjectUndo(shapeComponent, "Rotate Shape");
                    shapeComponent.RotateInsideBounds(s_ShapeRotation);

                    //Only Updating Draw shape tool when using this tool
                    if(s_UpdateDrawShapeTool)
                        DrawShapeTool.s_LastShapeRotation = shapeComponent.rotation;

                    ProBuilderEditor.Refresh();
                }
            }
        }

        static void DoCentralHandle()
        {
            if(Event.current.type == EventType.Repaint)
            {
                if(s_IsManipulatingSize)
                    return;

                int faceCount = Faces.Length;
                for(int i = 0; i < faceCount; i++)
                {
                    if(Faces[i].IsVisible)
                    {
                        float handleSize = HandleUtility.GetHandleSize(Vector3.zero) * 0.1f;

                        Color color = k_BoundsHandleColor;
                        color.a *= ( s_CurrentTargetedFace == null || s_CurrentTargetedFace == Faces[i] )
                            ? 1f
                            : 0.5f;

                        using(new Handles.DrawingScope(color))
                        {
                            int pointsCount = Faces[i].Points.Length;
                            for(int k = 0; k < pointsCount; k++)
                                Handles.DrawLine(Faces[i].Points[k], Faces[i].Points[( k + 1 ) % pointsCount]);

                            Handles.DrawLine(Vector3.zero, Faces[i].CenterPosition);
                            Handles.SphereHandleCap(-1, Faces[i].CenterPosition, Quaternion.identity, handleSize, EventType.Repaint);
                        }

                        if(s_CurrentTargetedFace != null)
                        {
                            handleSize = HandleUtility.GetHandleSize(Vector3.zero) * 0.5f;
                            for(int j = i + 1; j < faceCount; j++)
                            {
                                if(Faces[j].IsVisible)
                                {
                                    var normal = Vector3.Cross(Faces[i].Normal, Faces[j].Normal);
                                    var angle = Vector3.SignedAngle(Faces[i].Normal, Faces[j].Normal, normal);

                                    color = Color.blue;
                                    if(normal == Vector3.up || normal == Vector3.down)
                                        color = Color.green;
                                    else if(normal == Vector3.right || normal == Vector3.left)
                                        color = Color.red;

                                    using(new Handles.DrawingScope(color))
                                    {
                                        Handles.DrawWireArc(Vector3.zero, normal, Faces[i].Normal, angle, handleSize);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        static bool DoOrientationHandle(ShapeComponent shapeComponent)
        {
            Event evt = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            bool hasRotated = false;

            float handleSize = HandleUtility.GetHandleSize(s_CurrentHandlePosition) * 0.1f;

            bool isSelected = (HandleUtility.nearestControl == controlID && s_CurrentId == -1) || s_CurrentId == controlID;

            if(evt.type == EventType.Repaint)
                s_IsManipulatingOrientation = isSelected;

            if(s_IsManipulatingSize)
                return false;

            if(s_IsManipulatingOrientation)
                s_CurrentShape = shapeComponent;
            else if(s_CurrentShape == shapeComponent)
                s_CurrentShape = null;


            switch(evt.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == controlID && (evt.button == 0 || evt.button == 2))
                    {
                        s_CurrentId = controlID;
                        s_CurrentTargetedFace = null;
                        s_CurrentHandlePosition = Vector3.zero;
                        GUIUtility.hotControl = controlID;

                        s_CurrentTargetedFace = null;
                        foreach(var boundsFace in Faces)
                        {
                            if(boundsFace.IsVisible && EditorShapeUtility.PointerIsInFace(boundsFace))
                            {
                                UnityEngine.Plane p = new UnityEngine.Plane(boundsFace.Normal,  Handles.matrix.MultiplyPoint(boundsFace.CenterPosition));

                                Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                                float dist;
                                if(p.Raycast(ray, out dist))
                                {
                                    s_CurrentHandlePosition = Handles.inverseMatrix.MultiplyPoint(ray.GetPoint(dist));
                                    s_CurrentTargetedFace = boundsFace;
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
                        s_CurrentId = -1;
                        s_CurrentTargetedFace = null;
                        s_CurrentHandlePosition = Vector3.zero;
                    }
                    break;
                case EventType.Layout:
                    foreach(var face in Faces)
                        HandleUtility.AddControl(controlID, HandleUtility.DistanceToCircle(face.CenterPosition, handleSize / 2.0f));
                    break;
                case EventType.Repaint:
                    if(isSelected)
                    {
                        using(new Handles.DrawingScope(EditorHandleDrawing.edgeSelectedColor))
                        {
                            Handles.DrawLine(Vector3.zero, s_CurrentHandlePosition);
                            Handles.SphereHandleCap(controlID, s_CurrentHandlePosition, Quaternion.identity, handleSize, EventType.Repaint);
                        }

                        if(isSelected && s_CurrentTargetedFace != null)
                        {
                            Color color = k_BoundsHandleColor;
                            color.a *= 0.25f;

                            using(new Handles.DrawingScope(color))
                                Handles.DrawAAConvexPolygon(s_CurrentTargetedFace.Points);
                        }

                    }
                    break;
                case EventType.MouseMove:
                case EventType.MouseDrag:
                    bool hit = false;
                    if((HandleUtility.nearestControl == controlID && s_CurrentId == -1) || s_CurrentId == controlID)
                    {
                        var previousFace = s_CurrentTargetedFace;
                        s_CurrentTargetedFace = null;
                        foreach(var boundsFace in s_Faces)
                        {
                            if(boundsFace.IsVisible && EditorShapeUtility.PointerIsInFace(boundsFace))
                            {
                                UnityEngine.Plane p = new UnityEngine.Plane(boundsFace.Normal,  Handles.matrix.MultiplyPoint(boundsFace.CenterPosition));

                                Ray ray = HandleUtility.GUIPointToWorldRay(evt.mousePosition);
                                float dist;
                                if(p.Raycast(ray, out dist))
                                {
                                    s_CurrentHandlePosition = s_CurrentId == controlID ? Handles.inverseMatrix.MultiplyPoint(ray.GetPoint(dist)) : boundsFace.CenterPosition;
                                    s_CurrentTargetedFace = boundsFace;
                                    hit = true;
                                }
                            }
                        }

                        if(s_CurrentTargetedFace != null && previousFace != null && s_CurrentTargetedFace != previousFace)
                        {
                            Vector3 rotationAxis = Vector3.Cross(previousFace.Normal, s_CurrentTargetedFace.Normal);
                            s_ShapeRotation = Quaternion.AngleAxis(Vector3.SignedAngle(previousFace.Normal, s_CurrentTargetedFace.Normal,rotationAxis),rotationAxis);
                            hasRotated = true;
                        }
                    }
                    if(!hit)
                        s_CurrentTargetedFace = null;

                    break;
            }
            return hasRotated;
        }

        public static void ApplyProperties(ShapeComponent shape, Vector3 centerOffset, Vector3 size)
        {
            var trs = shape.transform;

            var bounds = new Bounds();
            bounds.center = centerOffset;
            bounds.size = size;

            shape.Rebuild(bounds, trs.rotation);
            shape.mesh.SetPivot(trs.position);

            ProBuilderEditor.Refresh(false);
        }

    }
}

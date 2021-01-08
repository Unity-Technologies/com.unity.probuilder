using System.Linq;
using UnityEngine;
using UnityEditor.EditorTools;
using UnityEditor.IMGUI.Controls;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.Shapes;
using Math = UnityEngine.ProBuilder.Math;
using Object = UnityEngine.Object;

using FaceData = UnityEditor.ProBuilder.EditorShapeUtility.FaceData;
using Plane = UnityEngine.ProBuilder.Shapes.Plane;
using Sprite = UnityEngine.ProBuilder.Shapes.Sprite;

#if !UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.EditorTools;
#else
using ToolManager = UnityEditor.EditorTools.ToolManager;
#endif

namespace UnityEditor.ProBuilder
{
    [EditorTool("Edit Shape", typeof(ShapeComponent))]
    sealed class EditShapeTool : EditorTool
    {
        Editor m_ShapeEditor;

        static readonly Color k_BoundsHandleColor = new Color(.2f, .4f, .8f, 1f);

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
        static FaceData s_CurrentFace = null;
        static int[] s_OrientationControlIDs = new int[4];
        static int[] s_FaceControlIDs = new int[6];

        //Size Handle management
        static bool s_InitSizeInteraction = true;
        static Vector3 s_OriginalSize;
        static Vector3 s_OriginalCenter;
        static Vector2 s_MouseStartPosition;
        static float s_SizeDelta;

        //Orientation Handle Manipulation
        static float s_CurrentAngle = 0;
        static int s_CurrentArrowHovered = -1;
        static Quaternion s_ShapeRotation = Quaternion.identity;
        static Vector3[][] s_ArrowsLines = new Vector3[4][];

        public override GUIContent toolbarIcon
        {
            get { return PrimitiveBoundsHandle.editModeButton; }
        }

        void OnEnable()
        {
            m_OverlayTitle = new GUIContent("Edit Shape Tool");
            for(int i = 0; i < s_ArrowsLines.Length; i++)
                s_ArrowsLines[i] = new Vector3[3];

#if !UNITY_2020_2_OR_NEWER
            ToolManager.activeToolChanging += ActiveToolChanging;
#endif
            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;

        }

        void OnDisable()
        {
#if !UNITY_2020_2_OR_NEWER
            ToolManager.activeToolChanging -= ActiveToolChanging;
#endif
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
            if(m_ShapeEditor != null)
                DestroyImmediate(m_ShapeEditor);
        }

#if !UNITY_2020_2_OR_NEWER
        public void ActiveToolChanging()
        {
            if(ToolManager.IsActiveTool(this))
                EditorApplication.delayCall += () => ProBuilderEditor.selectMode = SelectMode.Object;
        }
#else
        public override void OnActivated()
        {
            base.OnActivated();
            EditorApplication.delayCall += () => ProBuilderEditor.selectMode = SelectMode.Object;
        }

        public override void OnWillBeDeactivated()
        {
            base.OnWillBeDeactivated();
            EditorApplication.delayCall += () => ProBuilderEditor.ResetToLastSelectMode();
        }
#endif

        public void OnSelectModeChanged(SelectMode selectMode)
        {
            if(ToolManager.IsActiveTool(this) && selectMode != SelectMode.Object)
                ToolManager.RestorePreviousTool();
        }

        public override void OnToolGUI(EditorWindow window)
        {
            SceneViewOverlay.Window( m_OverlayTitle, OnOverlayGUI, 0, SceneViewOverlay.WindowDisplayOption.OneWindowPerTitle );

            foreach(var obj in targets)
            {
                var shape = obj as ShapeComponent;

                if (shape != null && !shape.edited)
                    DoEditingGUI(shape);
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
            if(shapeComponent == null)
                return;

            s_UpdateDrawShapeTool = updateDrawShapeTool;

            var scale = shapeComponent.transform.lossyScale;
            var position = shapeComponent.transform.position
                           + Vector3.Scale(shapeComponent.transform.TransformDirection(shapeComponent.shape.shapeBox.center),scale);
            var matrix = Matrix4x4.TRS(position, shapeComponent.transform.rotation, Vector3.one);

            using (new Handles.DrawingScope(matrix))
            {
                EditorShapeUtility.UpdateFaces(shapeComponent.editionBounds, scale, Faces);

                for(int i = 0; i <4; ++i)
                    s_OrientationControlIDs[i] = GUIUtility.GetControlID(FocusType.Passive);
                for(int i = 0; i <Faces.Length; ++i)
                    s_FaceControlIDs[i] = GUIUtility.GetControlID(FocusType.Passive);

                var absSize = Math.Abs(shapeComponent.editionBounds.size);
                if(absSize.x > Mathf.Epsilon && absSize.y > Mathf.Epsilon && absSize.z > Mathf.Epsilon )
                    DoOrientationHandlesGUI(shapeComponent, shapeComponent.mesh, shapeComponent.editionBounds);

                DoSizeHandlesGUI(shapeComponent);
            }
        }

        static void DoSizeHandlesGUI(ShapeComponent shapeComponent)
        {
            int faceCount = s_Faces.Length;

            var drawFacesHandle = GUIUtility.hotControl == 0 || s_FaceControlIDs.Contains(GUIUtility.hotControl);

            var evt = Event.current;

            var is2D = shapeComponent.shape is Plane || shapeComponent.shape is Sprite;
            for(int i = 0; i < faceCount; i++)
            {
                var face = Faces[i];
                if(is2D && !face.IsValid)
                    continue;

                if(Event.current.type == EventType.Repaint)
                {
                    Color color = k_BoundsHandleColor;
                    color.a *= face.IsVisible ? 1f : 0.5f;
                    using(new Handles.DrawingScope(color))
                    {
                        int pointsCount = face.Points.Length;
                        for(int k = 0; k < pointsCount; k++)
                            Handles.DrawLine(face.Points[k], face.Points[( k + 1 ) % pointsCount]);
                    }
                }

                bool drawFaceOnRepaint = evt.type == EventType.Repaint ? s_CurrentFace == face : true;

                if( (drawFacesHandle || drawFaceOnRepaint) && DoFaceSizeHandle(face, s_FaceControlIDs[i]))
                {
                    var scale = shapeComponent.transform.lossyScale;
                    if(!s_InitSizeInteraction)
                    {
                        s_InitSizeInteraction = true;
                        s_OriginalSize = shapeComponent.size;
                        s_OriginalCenter = shapeComponent.transform.position
                                            + Vector3.Scale(shapeComponent.transform.TransformDirection(shapeComponent.shape.shapeBox.center),scale);
                    }

                    float modifier = 1f;
                    if(Event.current.alt)
                        modifier = 2f;

                    var shapeSizeSigns = Math.Sign(shapeComponent.size);
                    var scaleSigns = Math.Sign(scale);
                    var scaleInverse = new Vector3(1f/scale.x, 1f/scale.y, 1f/scale.z);
                    var delta = Vector3.Scale(Vector3.Scale(s_SizeDelta * Math.Abs(s_Faces[i].Normal), scaleInverse) , Vector3.Scale(scale,shapeSizeSigns));
                    var sizeOffset = ProBuilderSnapping.Snap(modifier * delta, EditorSnapping.activeMoveSnapValue);

                    var faceNormal = shapeComponent.transform.TransformVector(s_Faces[i].Normal);
                    var center = Event.current.alt ? Vector3.zero : Vector3.Scale(Mathf.Sign(s_SizeDelta)*(sizeOffset.magnitude / 2f) * faceNormal , scaleSigns);

                    ApplyProperties(shapeComponent, s_OriginalCenter + center, s_OriginalSize + sizeOffset);
                }
            }
        }

        static bool DoFaceSizeHandle(FaceData face, int controlID)
        {
            if( s_OrientationControlIDs.Contains(HandleUtility.nearestControl) && !EditorShapeUtility.PointerIsInFace(face) )
                return false;

            Event evt = Event.current;
            float handleSize = HandleUtility.GetHandleSize(face.CenterPosition) * 0.05f;

            bool isSelected = (HandleUtility.nearestControl == controlID && s_CurrentId == -1) || s_CurrentId == controlID;

            switch(evt.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == controlID && evt.button == 0)
                    {
                        s_CurrentId = controlID;
                        GUIUtility.hotControl = controlID;
                        s_MouseStartPosition = evt.mousePosition;
                        s_InitSizeInteraction = false;
                        evt.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID && evt.button == 0)
                    {
                        GUIUtility.hotControl = 0;
                        evt.Use();
                        s_CurrentId = -1;
                    }
                    break;
                case EventType.Layout:
                    HandleUtility.AddControl(controlID, HandleUtility.DistanceToLine(face.CenterPosition, face.CenterPosition) / 2f);
                    break;
                case EventType.Repaint:
                    Color color = isSelected ? EditorHandleDrawing.edgeSelectedColor : k_BoundsHandleColor;
                    color.a *= face.IsVisible ? 1f : 0.25f;
                    using(new Handles.DrawingScope(color))
                        Handles.DotHandleCap(controlID, face.CenterPosition , Quaternion.identity, handleSize, EventType.Repaint);
                    break;
                case EventType.MouseDrag:
                    if(s_CurrentId == controlID)
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
            var evt = Event.current;
            if( GUIUtility.hotControl != 0
                && !s_OrientationControlIDs.Contains(GUIUtility.hotControl)
                || s_FaceControlIDs.Contains(HandleUtility.nearestControl))
                return;

            s_CurrentFace = null;
            foreach(var f in Faces)
            {
                if(f.IsVisible && EditorShapeUtility.PointerIsInFace(f))
                {
                    if(s_OrientationControlIDs.Contains(HandleUtility.nearestControl))
                        s_CurrentFace = f;
                    if(DoOrientationHandle(f))
                    {
                        UndoUtility.RecordComponents<Transform, ProBuilderMesh, ShapeComponent>(shapeComponent.GetComponents(typeof(Component)),"Rotate Shape");
                        shapeComponent.RotateInsideBounds(s_ShapeRotation);

                        //Only Updating Draw shape tool when using this tool
                        if(s_UpdateDrawShapeTool)
                            DrawShapeTool.s_LastShapeRotation = shapeComponent.rotation;

                        ProBuilderEditor.Refresh();
                    }
                }
            }

        }

        static bool DoOrientationHandle(FaceData face)
        {
            Event evt = Event.current;
            bool hasRotated = false;

            float handleSize = HandleUtility.GetHandleSize(Vector3.zero);

            switch(evt.type)
            {
                case EventType.MouseDown:
                    if ( s_OrientationControlIDs.Contains(HandleUtility.nearestControl) && evt.button == 0 )
                    {
                        s_CurrentId = HandleUtility.nearestControl;
                        GUIUtility.hotControl = s_CurrentId;
                        evt.Use();
                    }
                   break;
                case EventType.MouseUp:
                    if (s_OrientationControlIDs.Contains(HandleUtility.nearestControl) && evt.button == 0 )
                    {
                        GUIUtility.hotControl = 0;
                        evt.Use();
                        if(s_CurrentId == HandleUtility.nearestControl)
                        {
                            //Execute rotation
                            Vector3 targetedNormal = Vector3.zero;
                            for(int i = 0; i < s_OrientationControlIDs.Length; i++)
                            {
                                if(s_OrientationControlIDs[i] == s_CurrentId)
                                {
                                    targetedNormal = (s_ArrowsLines[i][1] - face.CenterPosition).normalized;
                                    break;
                                }
                            }

                            Vector3 rotationAxis = Vector3.Cross(face.Normal, targetedNormal);
                            var angle = Vector3.SignedAngle(face.Normal, targetedNormal, rotationAxis);
                            s_ShapeRotation = Quaternion.AngleAxis(angle, rotationAxis);
                            s_CurrentAngle = (s_CurrentAngle + angle) % 360;

                            hasRotated = true;
                        }
                        s_CurrentId = -1;
                    }
                    break;
                    case EventType.Layout:
                        for(int i = 0; i < 4; i++)
                        {
                            float dist1 = HandleUtility.DistanceToLine(s_ArrowsLines[i][0], s_ArrowsLines[i][1]);
                            float dist2 = HandleUtility.DistanceToLine(s_ArrowsLines[i][1], s_ArrowsLines[i][2]);
                            HandleUtility.AddControl(s_OrientationControlIDs[i], Mathf.Min(dist1,dist2) / 2f);
                        }
                        break;
                   case EventType.Repaint:
                        if(s_CurrentArrowHovered != HandleUtility.nearestControl)
                           s_CurrentAngle = 0f;

                       int pointsCount = face.Points.Length;
                       s_CurrentArrowHovered = -1;
                       for(int i = 0; i < pointsCount; i++)
                       {
                           var sideDirection = ( face.Points[( i + 1 ) % pointsCount] - face.Points[i] ).normalized;
                           var arrowDirection = Vector3.Cross(face.Normal.normalized, sideDirection).normalized;
                           var top = face.CenterPosition + 0.5f * handleSize * arrowDirection;
                           var A = (  0.5f * handleSize * arrowDirection ).magnitude;
                           var a = 0.4f * Mathf.Sqrt(2f * A * A);
                           var h = 0.5f * Mathf.Sqrt(2f * a * a);
                           s_ArrowsLines[i][0] = top - ( h * arrowDirection + h * sideDirection );
                           s_ArrowsLines[i][1] = top;
                           s_ArrowsLines[i][2] = top - ( h * arrowDirection - h * sideDirection );

                           bool selected = HandleUtility.nearestControl == s_OrientationControlIDs[i];

                           if(selected)
                               EditorGUIUtility.AddCursorRect(new Rect(0,0,Screen.width, Screen.height), MouseCursor.RotateArrow);

                           Color color = selected
                               ? EditorHandleDrawing.edgeSelectedColor
                               : k_BoundsHandleColor;
                           color.a = 1.0f;

                           var targetedNormal = (s_ArrowsLines[i][1] - face.CenterPosition).normalized;
                           Vector3 rotationAxis = Vector3.Cross(face.Normal, targetedNormal);

                           var middlePoint = ( face.Points[( i + 1 ) % pointsCount] + face.Points[i] ) / 2f;
                           float length = ( middlePoint - face.CenterPosition ).magnitude;

                           using(new Handles.DrawingScope(color))
                           {
                               Handles.DrawAAPolyLine(5f, s_ArrowsLines[i]);
                               if(selected)
                               {
                                   s_CurrentArrowHovered = HandleUtility.nearestControl;

                                   var result = Quaternion.AngleAxis(s_CurrentAngle, rotationAxis) * face.CenterPosition;
                                   if(Mathf.Abs(Vector3.Dot(result, arrowDirection)) > 0.01f)
                                       result = length * result.normalized;

                                   Handles.DrawAAPolyLine(5f, new Vector3[]{Vector3.zero, result});

                                   var globalRadius = 0.5f * handleSize;
                                   Handles.DrawSolidArc(Vector3.zero, rotationAxis, face.Normal, 360f,0.15f * globalRadius);
                                   Handles.DrawLine(0.75f * globalRadius * arrowDirection.normalized, globalRadius * arrowDirection.normalized);
                                   Handles.DrawLine(-0.75f * globalRadius * arrowDirection.normalized, -globalRadius * arrowDirection.normalized);
                                   Handles.DrawLine(-0.75f * globalRadius * face.Normal, -globalRadius * face.Normal);
                                   Handles.DrawLine(Vector3.zero, globalRadius * face.Normal);
                                   Handles.DrawWireArc(Vector3.zero, rotationAxis, face.Normal, 360f, globalRadius);

                                   color.a = 0.25f;
                                   using(new Handles.DrawingScope(color))
                                       Handles.DrawSolidArc(Vector3.zero, rotationAxis, face.Normal, s_CurrentAngle,globalRadius);
                               }
                           }
                       }
                        break;
                case EventType.MouseDrag:
                    if(s_OrientationControlIDs.Contains(s_CurrentId) && HandleUtility.nearestControl != s_CurrentId)
                    {
                        GUIUtility.hotControl = 0;
                        s_CurrentId = -1;
                    }
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

            UndoUtility.RecordComponents<Transform, ProBuilderMesh, ShapeComponent>(shape.GetComponents(typeof(Component)),"Resize Shape");
            shape.Rebuild(bounds, trs.rotation);

            ProBuilderEditor.Refresh(false);
        }

    }
}

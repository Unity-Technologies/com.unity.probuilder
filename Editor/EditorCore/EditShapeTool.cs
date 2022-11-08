using System.Linq;
using UnityEngine;
using UnityEditor.EditorTools;
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
    [EditorTool("Edit Shape", typeof(ProBuilderShape))]
    sealed class EditShapeTool : EditorTool
    {
        Editor m_ShapeEditor;

        static readonly Color k_BoundsHandleColor = new Color(.2f, .4f, .8f, 1f);

        GUIContent m_OverlayTitle;

        static FaceData[] s_Faces;

        public static FaceData[] faces
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

        //Handle Manipulation
        static int s_CurrentId = -1;
        static readonly int[] k_OrientationControlIDs = new int[4];
        static int[] s_FaceControlIDs = new int[6];

        //Size Handle management
        static Vector2 s_StartMousePosition;
        static Vector3 s_StartSize;
        static Vector3 s_StartPositionLocal;
        static Vector3 s_StartPositionGlobal;
        static Vector3 s_StartScale;
        static Vector3 s_StartScaleInverse;
        static Vector3 s_StartCenter;
        static Vector3 s_Direction;
        static bool s_SizeManipulationInit;
        static float s_SizeDelta;

        static float s_DefaultMidpointHandleSize = 0.03f;
        static float s_DefaultMidpointSquareSize = 0.15f;

        //Orientation Handle Manipulation
        static float s_CurrentAngle = 0;
        static int s_CurrentArrowHovered = -1;
        static Quaternion s_ShapeRotation = Quaternion.identity;
        static Vector3[][] s_ArrowsLines = new Vector3[4][];

#if UNITY_2021_1_OR_NEWER
        public override bool gridSnapEnabled => true;
#endif

        static GUIContent s_IconContent;
        public override GUIContent toolbarIcon
        {
            get
            {
                if(s_IconContent == null)
                    s_IconContent = new GUIContent()
                    {
                        image = IconUtility.GetIcon("Tools/EditShape"),
                        text = "Edit Shape",
                        tooltip = "Edit Shape"
                    };
                return s_IconContent;
            }
        }

        void OnEnable()
        {
            m_OverlayTitle = new GUIContent("Edit Shape");
            for(int i = 0; i < s_ArrowsLines.Length; i++)
                s_ArrowsLines[i] = new Vector3[3];

            m_ShapeEditor = Editor.CreateEditor(targets.ToArray(), typeof(ProBuilderShapeEditor));
            EditorApplication.playModeStateChanged += PlaymodeStateChanged ;

#if !UNITY_2020_2_OR_NEWER
            ToolManager.activeToolChanging += ActiveToolChanging;
            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;
#endif
        }

        void OnDisable()
        {
#if !UNITY_2020_2_OR_NEWER
            ToolManager.activeToolChanging -= ActiveToolChanging;
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
#endif
            EditorApplication.playModeStateChanged -= PlaymodeStateChanged ;

            if(m_ShapeEditor != null)
                DestroyImmediate(m_ShapeEditor);
        }

        void PlaymodeStateChanged(PlayModeStateChange stateChange)
        {
            if(stateChange == PlayModeStateChange.ExitingEditMode
               || stateChange == PlayModeStateChange.ExitingPlayMode)
                return;

            if(m_ShapeEditor != null)
                DestroyImmediate(m_ShapeEditor);

            m_ShapeEditor = Editor.CreateEditor(targets.ToArray(), typeof(ProBuilderShapeEditor));
        }

#if !UNITY_2020_2_OR_NEWER
        void ActiveToolChanging()
        {
            if(ToolManager.IsActiveTool(this))
                EditorApplication.delayCall += () => ChangeToObjectMode();
        }

        void ChangeToObjectMode()
        {
            if(ToolManager.IsActiveTool(this))
                ProBuilderEditor.selectMode = SelectMode.Object;
        }

#else
        public override void OnActivated()
        {
            base.OnActivated();
            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;
            EditorApplication.delayCall += () => ProBuilderEditor.selectMode = SelectMode.Object;
        }

        public override void OnWillBeDeactivated()
        {
            base.OnWillBeDeactivated();
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
            EditorApplication.delayCall += () => ResetToLastSelectMode();
        }

        public void ResetToLastSelectMode()
        {
            if(ProBuilderToolManager.activeTool != Tool.Custom && ProBuilderToolManager.IsAnyProBuilderContextActive())
                ProBuilderEditor.ResetToLastSelectMode();
        }
#endif

        void OnSelectModeChanged(SelectMode selectMode)
        {
            if(ToolManager.IsActiveTool(this) && selectMode != SelectMode.Object)
                ToolManager.RestorePreviousTool();
        }

        public override void OnToolGUI(EditorWindow window)
        {
// todo refactor overlays to use `Overlay` class
#pragma warning disable 618
            SceneViewOverlay.Window( m_OverlayTitle, OnOverlayGUI, 0, SceneViewOverlay.WindowDisplayOption.OneWindowPerTitle );
#pragma warning restore 618

            if(Event.current.type == EventType.MouseMove)
            {
                SceneView.RepaintAll();
                return;
            }

            foreach(var obj in targets)
            {
                var shape = obj as ProBuilderShape;

                if (shape != null && shape.isEditable)
                    DoEditingHandles(shape);
            }
        }

        void OnOverlayGUI(Object obj, SceneView view)
        {
#if !UNITY_2021_1_OR_NEWER
            var snapDisabled = Tools.pivotRotation != PivotRotation.Global;
            using(new EditorGUI.DisabledScope(snapDisabled))
            {
                if(snapDisabled)
                    EditorGUILayout.Toggle("Snapping (only Global)", false);
                else
                    EditorSnapSettings.gridSnapEnabled = EditorGUILayout.Toggle("Grid Snapping", EditorSnapSettings.gridSnapEnabled);
            }
#endif

#if UNITY_2021_2_OR_NEWER
            GUILayout.BeginVertical(GUILayout.MinWidth(DrawShapeTool.k_MinOverlayWidth));
            ( (ProBuilderShapeEditor) m_ShapeEditor ).DrawShapeGUI(null);
            ( (ProBuilderShapeEditor) m_ShapeEditor ).DrawShapeParametersGUI(null);
            GUILayout.EndVertical();
#else
            using(new EditorGUILayout.VerticalScope(new GUIStyle(EditorStyles.frameBox)))
            {
                ( (ProBuilderShapeEditor) m_ShapeEditor ).DrawShapeGUI(null);
                ( (ProBuilderShapeEditor) m_ShapeEditor ).DrawShapeParametersGUI(null);
            }
#endif
        }

        /// <summary>
        /// The Editing handles are used to manipulate and resize ProBuilderShapes
        /// These handles are used in 2 tools : EditShapeTool and DrawShapeTool. In this second tool,
        /// these handles allow to modified the last created shape.
        /// </summary>
        /// <param name="proBuilderShape">The Shape on which to attach the handles</param>
        /// <param name="updatePrefs">Parameter used to update the DrawShapeTool when needed</param>
        internal static void DoEditingHandles(ProBuilderShape proBuilderShape, bool updatePrefs = false)
        {
            if(proBuilderShape == null)
                return;

            var scale = proBuilderShape.transform.lossyScale;
            var position = proBuilderShape.transform.position
                           + Vector3.Scale(proBuilderShape.transform.TransformDirection(proBuilderShape.shapeBox.center),scale);
            var matrix = Matrix4x4.TRS(position, proBuilderShape.transform.rotation, Vector3.one);

            using (new Handles.DrawingScope(matrix))
            {
                EditorShapeUtility.UpdateFaces(proBuilderShape.editionBounds, scale, faces);

                for(int i = 0; i <4; ++i)
                    k_OrientationControlIDs[i] = GUIUtility.GetControlID(FocusType.Passive);
                for(int i = 0; i <faces.Length; ++i)
                    s_FaceControlIDs[i] = GUIUtility.GetControlID(FocusType.Passive);

                var absSize = Math.Abs(proBuilderShape.editionBounds.size);
                if(absSize.x > Mathf.Epsilon && absSize.y > Mathf.Epsilon && absSize.z > Mathf.Epsilon )
                    DoOrientationHandles(proBuilderShape, updatePrefs);

                DoSizeHandles(proBuilderShape, updatePrefs);
            }
        }

        static void DoSizeHandles(ProBuilderShape proBuilderShape, bool updatePrefs)
        {
            int faceCount = s_Faces.Length;

            var evt = Event.current;

            var is2D = proBuilderShape.shape is Plane || proBuilderShape.shape is Sprite;
            for(int i = 0; i < faceCount; i++)
            {
                var face = faces[i];
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

                if( DoFaceSizeHandle(face, s_FaceControlIDs[i]) )
                {

                    if(!s_SizeManipulationInit)
                    {
                        var offset = proBuilderShape.transform.TransformVector(proBuilderShape.shapeBox.center);
                        s_StartCenter = proBuilderShape.transform.position + offset;
                        s_StartScale = proBuilderShape.transform.lossyScale;
                        s_StartScaleInverse = new Vector3(1f / Mathf.Abs(s_StartScale.x), 1f/Mathf.Abs(s_StartScale.y), 1f/Mathf.Abs(s_StartScale.z));
                        s_StartPositionLocal = face.CenterPosition + Vector3.Scale(offset, s_StartScale);
                        s_StartPositionGlobal = proBuilderShape.transform.TransformPoint(s_StartPositionLocal);
                        s_StartSize = proBuilderShape.size;
                        s_SizeManipulationInit = true;
                        s_Direction = Vector3.Scale(face.Normal, Math.Sign(s_StartSize));
                    }

                    var targetSize = s_StartSize;
                    //Should we expand on the 2 sides?
                    var modifier = evt.alt ? 2f : 1f;
                    var delta = modifier * ( s_SizeDelta * s_Faces[i].Normal );

                    if(Math.IsCardinalAxis(proBuilderShape.transform.up)
                       && EditorSnapSettings.gridSnapEnabled
                       && !EditorSnapSettings.incrementalSnapActive
                       && !evt.alt)
                    {
                        var facePosition = s_StartPositionGlobal + Vector3.Scale(delta,s_StartScaleInverse);
                        var snapValue = Vector3.Scale(Vector3.Scale(EditorSnapping.activeMoveSnapValue, s_StartScaleInverse), Math.Abs(face.Normal));

                        facePosition = ProBuilderSnapping.Snap(facePosition, snapValue);
                        targetSize += Vector3.Scale((facePosition - s_StartPositionGlobal), s_Direction);
                    }
                    else
                    {
                        var snapValue = EditorSnapSettings.incrementalSnapActive
                            ? Vector3.Scale(Vector3.Scale(EditorSnapping.activeMoveSnapValue, s_StartScaleInverse), Math.Abs(face.Normal))
                            : Vector3.zero;

                        //Move regarding the face tangent direction
                        delta.Scale(s_Direction);
                        //scale by the object scale factor
                        delta.Scale(s_StartScaleInverse);

                        targetSize = ProBuilderSnapping.Snap(targetSize + delta, snapValue);
                    }

                    var center = Vector3.zero;
                    if(!evt.alt)
                    {
                        center = Vector3.Scale((targetSize - s_StartSize) / 2f, s_Direction);
                        center = Vector3.Scale(center, Math.Sign(s_StartScale));
                        center = proBuilderShape.transform.TransformVector(center);
                    }
                    ApplyProperties(proBuilderShape, s_StartCenter + center, targetSize);

                    if(updatePrefs)
                        DrawShapeTool.SaveShapeParams(proBuilderShape);
                }
            }
        }

        static bool DoFaceSizeHandle(FaceData face, int controlID)
        {
            if( k_OrientationControlIDs.Contains(HandleUtility.nearestControl) && !EditorShapeUtility.PointerIsInFace(face) )
                return false;

            Event evt = Event.current;
            float handleSize = HandleUtility.GetHandleSize(face.CenterPosition) * s_DefaultMidpointHandleSize;
            bool isSelected = (HandleUtility.nearestControl == controlID && s_CurrentId == -1) || s_CurrentId == controlID;

            switch(evt.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    if (HandleUtility.nearestControl == controlID && evt.button == 0)
                    {
                        s_CurrentId = controlID;
                        GUIUtility.hotControl = controlID;
                        s_StartMousePosition = evt.mousePosition;
                        s_SizeManipulationInit = false;
                        evt.Use();
                        SceneView.RepaintAll();
                    }
                    break;
                case EventType.MouseUp:
                    if (GUIUtility.hotControl == controlID && evt.button == 0)
                    {
                        GUIUtility.hotControl = 0;
                        evt.Use();
                        s_CurrentId = -1;
                        s_SizeManipulationInit = false;
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
                        s_SizeDelta = HandleUtility.CalcLineTranslation(s_StartMousePosition, evt.mousePosition, s_StartPositionLocal, face.Normal);
                        return true;
                    }
                    break;
            }
            return false;
        }

        static void DoOrientationHandles(ProBuilderShape proBuilderShape, bool updatePrefs)
        {
            if( GUIUtility.hotControl != 0 && !k_OrientationControlIDs.Contains(GUIUtility.hotControl) )
                return;

            foreach(var f in faces)
            {
                if(f.IsVisible && EditorShapeUtility.PointerIsInFace(f))
                {
                    if(DoOrientationHandle(f, proBuilderShape))
                    {
                        UndoUtility.RecordComponents<Transform, ProBuilderMesh, ProBuilderShape>(proBuilderShape.GetComponents(typeof(Component)),"Rotate Shape");
                        proBuilderShape.RotateInsideBounds(s_ShapeRotation);

                        ProBuilderEditor.Refresh();

                        if(updatePrefs)
                            DrawShapeTool.SaveShapeParams(proBuilderShape);
                    }
                }
            }

        }

        static bool DoOrientationHandle(FaceData face, ProBuilderShape proBuilderShape)
        {
            Event evt = Event.current;
            bool hasRotated = false;

            switch(evt.type)
            {
                case EventType.MouseDown:
                    if ( k_OrientationControlIDs.Contains(HandleUtility.nearestControl) && evt.button == 0 )
                    {
                        s_CurrentId = HandleUtility.nearestControl;
                        GUIUtility.hotControl = s_CurrentId;
                        evt.Use();
                    }
                   break;
                case EventType.MouseUp:
                    if (k_OrientationControlIDs.Contains(HandleUtility.nearestControl) && evt.button == 0 )
                    {
                        GUIUtility.hotControl = 0;
                        evt.Use();
                        if(s_CurrentId == HandleUtility.nearestControl)
                        {
                            //Execute rotation
                            Vector3 targetedNormal = Vector3.zero;
                            for(int i = 0; i < k_OrientationControlIDs.Length; i++)
                            {
                                if(k_OrientationControlIDs[i] == s_CurrentId)
                                {
                                    targetedNormal = (s_ArrowsLines[i][1] - face.CenterPosition).normalized;
                                    break;
                                }
                            }

                            var currentNormal = face.Normal;
                            currentNormal.Scale(Math.Sign(proBuilderShape.size));
                            targetedNormal.Scale(Math.Sign(proBuilderShape.size));
                            Vector3 rotationAxis = Vector3.Cross(currentNormal,targetedNormal);
                            var angle = Vector3.SignedAngle(currentNormal, targetedNormal, rotationAxis);
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
                            var rectPos = 0.8f * s_ArrowsLines[i][1] + 0.2f * face.CenterPosition;
                            float dist = HandleUtility.DistanceToRectangle( rectPos,
                                Quaternion.LookRotation(face.Normal),
                                HandleUtility.GetHandleSize(face.CenterPosition) * s_DefaultMidpointSquareSize/2f);
                            HandleUtility.AddControl(k_OrientationControlIDs[i], dist);
                        }
                        break;
                   case EventType.Repaint:
                        if(s_CurrentArrowHovered != HandleUtility.nearestControl)
                           s_CurrentAngle = 0f;

                       int pointsCount = face.Points.Length;
                       s_CurrentArrowHovered = -1;
                       for(int i = 0; i < pointsCount; i++)
                       {
                           var rectHandleSize = HandleUtility.GetHandleSize(face.CenterPosition) * s_DefaultMidpointSquareSize;

                           var sideDirection = ( face.Points[( i + 1 ) % pointsCount] - face.Points[i] ).normalized;
                           var arrowDirection = Vector3.Cross(face.Normal.normalized, sideDirection).normalized;

                           var topDirection = 2.5f * rectHandleSize * arrowDirection;
                           var top = face.CenterPosition + topDirection;
                           var A = topDirection.magnitude;
                           var a = 0.33f * Mathf.Sqrt(2f * A * A);
                           var h = 0.5f * Mathf.Sqrt(2f * a * a);
                           s_ArrowsLines[i][0] = top - ( h * arrowDirection + h * sideDirection );
                           s_ArrowsLines[i][1] = top;
                           s_ArrowsLines[i][2] = top - ( h * arrowDirection - h * sideDirection );

                           bool selected = HandleUtility.nearestControl == k_OrientationControlIDs[i];

                           Color color = selected
                               ? EditorHandleDrawing.edgeSelectedColor
                               : k_BoundsHandleColor;
                           color.a = 1.0f;

                           using(new Handles.DrawingScope(color))
                           {
                               Handles.DrawAAPolyLine(5f, s_ArrowsLines[i]);
                               if(selected)
                               {
                                   EditorGUIUtility.AddCursorRect(new Rect(0,0,Screen.width, Screen.height), MouseCursor.RotateArrow);
                                   s_CurrentArrowHovered = HandleUtility.nearestControl;
                                   Handles.DrawAAPolyLine(3f,
                                       new Vector3[]
                                       {
                                           Vector3.Scale(proBuilderShape.rotation * Vector3.up, proBuilderShape.size / 2f),
                                           Vector3.zero,
                                           Vector3.Scale(proBuilderShape.rotation * Vector3.forward, proBuilderShape.size / 2f)
                                       });
                               }
                           }
                       }
                        break;
                case EventType.MouseDrag:
                    if(k_OrientationControlIDs.Contains(s_CurrentId) && HandleUtility.nearestControl != s_CurrentId)
                    {
                        GUIUtility.hotControl = 0;
                        s_CurrentId = -1;
                    }
                    break;
             }
             return hasRotated;
        }

        static void ApplyProperties(ProBuilderShape proBuilderShape, Vector3 newCenterPosition, Vector3 newSize)
        {
            var bounds = new Bounds();
            bounds.center = newCenterPosition;
            bounds.size = newSize;

            UndoUtility.RecordComponents<Transform, ProBuilderMesh, ProBuilderShape>(proBuilderShape.GetComponents(typeof(Component)),"Resize Shape");
            proBuilderShape.UpdateBounds(bounds);

            ProBuilderEditor.Refresh(false);
        }

    }
}

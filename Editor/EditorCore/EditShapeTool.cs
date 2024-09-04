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
using ToolManager = UnityEditor.EditorTools.ToolManager;

namespace UnityEditor.ProBuilder
{
    [EditorTool("Edit Shape", typeof(ProBuilderShape))]
    sealed class EditShapeTool : EditorTool
    {
        [MenuItem("Tools/ProBuilder/Edit/Edit Shape", true, PreferenceKeys.menuEditor + 10)]
        static bool ValidateEditShapeTool()
        {
            foreach (var go in Selection.gameObjects)
                if (go.TryGetComponent<ProBuilderShape>(out _)) return true;

            return false;
        }

        [MenuItem("Tools/ProBuilder/Edit/Edit Shape", false, PreferenceKeys.menuEditor + 10)]
        static void ActivateEditShapeTool()
        {
            ToolManager.SetActiveTool<EditShapeTool>();
            ProBuilderAnalytics.SendActionEvent("Edit Shape Tool", nameof(EditShapeTool));
        }

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
        static Vector3[][] s_ArrowsLines;

        static Vector3[][] arrowsLines
        {
            get
            {
                if(s_ArrowsLines == null)
                {
                    s_ArrowsLines = new Vector3[4][];
                    for (int i = 0; i < s_ArrowsLines.Length; i++)
                        s_ArrowsLines[i] = new Vector3[3];
                }

                return s_ArrowsLines;
            }
        }

        public override bool gridSnapEnabled => true;

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
                        tooltip = "Edit ProBuilder Shape"
                    };
                return s_IconContent;
            }
        }

        void OnEnable()
        {
            m_OverlayTitle = new GUIContent("Shape Settings");
            m_ShapeEditor = Editor.CreateEditor(targets.ToArray(), typeof(ProBuilderShapeEditor));
        }

        void OnDisable()
        {
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

        public override void OnActivated()
        {
            base.OnActivated();
            EditorApplication.playModeStateChanged += PlaymodeStateChanged ;
            ProBuilderEditor.selectModeChanged += OnSelectModeChanged;
            ToolManager.activeContextChanged += OnActiveContextChanged;
        }

        public override void OnWillBeDeactivated()
        {
            base.OnWillBeDeactivated();
            ToolManager.activeContextChanged -= OnActiveContextChanged;
            ProBuilderEditor.selectModeChanged -= OnSelectModeChanged;
            EditorApplication.playModeStateChanged -= PlaymodeStateChanged ;
        }

        void OnSelectModeChanged(SelectMode selectMode)
        {
            if(ToolManager.IsActiveTool(this))
                ToolManager.RestorePreviousTool();
        }

        void OnActiveContextChanged()
        {
            ToolManager.RestorePreviousPersistentTool();
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
            GUILayout.BeginVertical(GUILayout.MinWidth(DrawShapeTool.k_MinOverlayWidth));
            ( (ProBuilderShapeEditor) m_ShapeEditor ).DrawShapeGUI();
            ( (ProBuilderShapeEditor) m_ShapeEditor ).DrawShapeParametersGUI(null);
            GUILayout.EndVertical();
        }

        /// <summary>
        /// The Editing handles are used to manipulate and resize ProBuilderShapes
        /// These handles are used in 2 tools : EditShapeTool and DrawShapeTool. In this second tool,
        /// these handles allow to modified the last created shape.
        /// </summary>
        /// <param name="proBuilderShape">The Shape on which to attach the handles</param>
        internal static void DoEditingHandles(ProBuilderShape proBuilderShape, DrawShapeTool tool = null)
        {
            if(proBuilderShape == null)
                return;

            var matrix = Matrix4x4.TRS(proBuilderShape.shapeWorldCenter, proBuilderShape.transform.rotation, Vector3.one);

            using (new Handles.DrawingScope(matrix))
            {
                EditorShapeUtility.UpdateFaces(proBuilderShape.editionBounds, proBuilderShape.transform.lossyScale, faces);

                for (int i = 0; i < 4; ++i)
                    k_OrientationControlIDs[i] = GUIUtility.GetControlID(FocusType.Passive);
                for (int i = 0; i < faces.Length; ++i)
                    s_FaceControlIDs[i] = GUIUtility.GetControlID(FocusType.Passive);

                if (!(proBuilderShape.shape is Cube))
                {
                    var absSize = Math.Abs(proBuilderShape.editionBounds.size);
                    if (absSize.x > Mathf.Epsilon && absSize.y > Mathf.Epsilon && absSize.z > Mathf.Epsilon)
                        DoOrientationHandles(proBuilderShape, tool);
                }

                DoSizeHandles(proBuilderShape, tool);
            }
        }

        static void DoSizeHandles(ProBuilderShape proBuilderShape, DrawShapeTool tool = null)
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

                if( DoFaceSizeHandle(proBuilderShape.transform, face, s_FaceControlIDs[i]) )
                {

                    if(!s_SizeManipulationInit)
                    {
                        s_StartCenter = proBuilderShape.shapeWorldCenter;
                        s_StartScale = proBuilderShape.transform.lossyScale;
                        s_StartScaleInverse = new Vector3(1f / Mathf.Abs(s_StartScale.x), 1f/Mathf.Abs(s_StartScale.y), 1f/Mathf.Abs(s_StartScale.z));
                        s_StartPositionLocal = proBuilderShape.shapeLocalBounds.center + face.CenterPosition;
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

                        targetSize += ProBuilderSnapping.Snap(delta, snapValue);
                    }

                    var center = Vector3.zero;
                    if(!evt.alt)
                    {
                        center = Vector3.Scale((targetSize - s_StartSize) / 2f, s_Direction);
                        center = Vector3.Scale(center, Math.Sign(s_StartScale));
                        center = proBuilderShape.transform.TransformVector(center);
                    }
                    ApplyProperties(proBuilderShape, s_StartCenter + center, targetSize);

                    if(tool != null)
                        tool.SaveShapeParams(proBuilderShape);
                }
            }
        }

        static bool DoFaceSizeHandle(Transform trs, FaceData face, int controlID)
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
                        using(new Handles.DrawingScope(Matrix4x4.identity))
                            s_SizeDelta = HandleUtility.CalcLineTranslation(s_StartMousePosition, evt.mousePosition, s_StartPositionGlobal, trs.TransformDirection(face.Normal).normalized);
                        return true;
                    }
                    break;
            }
            return false;
        }

        static void DoOrientationHandles(ProBuilderShape proBuilderShape, DrawShapeTool tool)
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

                        if(tool != null)
                            tool.SaveShapeParams(proBuilderShape);
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
                                    targetedNormal = (arrowsLines[i][1] - face.CenterPosition).normalized;
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
                            var rectPos = 0.8f * arrowsLines[i][1] + 0.2f * face.CenterPosition;
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
                           arrowsLines[i][0] = top - ( h * arrowDirection + h * sideDirection );
                           arrowsLines[i][1] = top;
                           arrowsLines[i][2] = top - ( h * arrowDirection - h * sideDirection );

                           bool selected = HandleUtility.nearestControl == k_OrientationControlIDs[i];

                           Color color = selected
                               ? EditorHandleDrawing.edgeSelectedColor
                               : k_BoundsHandleColor;
                           color.a = 1.0f;

                           using(new Handles.DrawingScope(color))
                           {
                               Handles.DrawAAPolyLine(5f, arrowsLines[i]);
                               if(selected)
                               {
                                   EditorGUIUtility.AddCursorRect(new Rect(0,0,Screen.width, Screen.height), MouseCursor.RotateArrow);
                                   s_CurrentArrowHovered = HandleUtility.nearestControl;
                                   Handles.DrawAAPolyLine(3f,
                                       new Vector3[]
                                       {
                                           Vector3.Scale(proBuilderShape.shapeRotation * Vector3.up, proBuilderShape.size / 2f),
                                           Vector3.zero,
                                           Vector3.Scale(proBuilderShape.shapeRotation * Vector3.forward, proBuilderShape.size / 2f)
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

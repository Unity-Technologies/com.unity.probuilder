using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering;
using ColorUtility = UnityEngine.ProBuilder.ColorUtility;
using Math = UnityEngine.ProBuilder.Math;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class OffsetElements : MenuAction
    {
        internal enum CoordinateSpace
        {
            World,
            Local,
            Element,
            Handle
        }

        static readonly TooltipContent s_TooltipFace = new TooltipContent ( "Offset Faces", "Move the selected elements by a set amount." );
        static readonly TooltipContent s_TooltipEdge = new TooltipContent ( "Offset Edges", "Move the selected elements by a set amount." );
        static readonly TooltipContent s_TooltipVert = new TooltipContent ( "Offset Vertices", "Move the selected elements by a set amount." );

        internal static Pref<Vector3> s_Translation = new Pref<Vector3>("MoveElements.s_Translation", Vector3.up);
        internal static Pref<CoordinateSpace> s_CoordinateSpace = new Pref<CoordinateSpace>("MoveElements.s_CoordinateSpace", CoordinateSpace.World);

        public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Offset", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get
            {
                if(ProBuilderEditor.selectMode == SelectMode.Face)
                    return s_TooltipFace;
                if(ProBuilderEditor.selectMode == SelectMode.Edge)
                    return s_TooltipEdge;
                return s_TooltipVert;
            }
        }

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Face | SelectMode.Edge | SelectMode.Vertex; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedVertexCount > 0; }
        }

        protected override MenuActionState optionsMenuState
        {
            get { return MenuActionState.VisibleAndEnabled; }
        }

        protected override void DoAlternateAction()
        {
            ConfigurableWindow.GetWindow<MoveElementsSettings>(true, "Offset Settings", true);
        }

        public override ActionResult DoAction()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordSelection("Offset Elements(s)");

            var handleRotation = MeshSelection.GetHandleRotation();

            foreach (var group in MeshSelection.elementSelection)
            {
                var mesh = group.mesh;
                var positions = mesh.positionsInternal;
                var offset = s_Translation.value;

                switch (s_CoordinateSpace.value)
                {
                    case CoordinateSpace.World:
                    case CoordinateSpace.Handle:
                    {
                        var pre = mesh.transform.localToWorldMatrix;
                        var post = mesh.transform.worldToLocalMatrix;

                        if (s_CoordinateSpace.value == CoordinateSpace.Handle)
                            offset = handleRotation * offset;

                        foreach (var index in mesh.selectedCoincidentVertices)
                        {
                            var p = pre.MultiplyPoint3x4(positions[index]);
                            p += offset;
                            positions[index] = post.MultiplyPoint3x4(p);
                        }
                        break;
                    }

                    case CoordinateSpace.Local:
                    {
                        foreach (var index in mesh.selectedCoincidentVertices)
                            positions[index] += offset;
                        break;
                    }

                    case CoordinateSpace.Element:
                    {
                        foreach (var elements in group.elementGroups)
                        {
                            var rotation = Quaternion.Inverse(mesh.transform.rotation) * elements.rotation;
                            var o = rotation * offset;
                            foreach (var index in elements.indices)
                                positions[index] += o;
                        }
                        break;
                    }
                }

                mesh.Rebuild();
                mesh.Optimize();
                ProBuilderEditor.Refresh();
            }

            if(ProBuilderEditor.selectMode.ContainsFlag(SelectMode.Edge | SelectMode.TextureEdge))
                return new ActionResult(ActionResult.Status.Success, "Move " + MeshSelection.selectedEdgeCount + (MeshSelection.selectedEdgeCount > 1 ? " Edges" : " Edge"));
            if(ProBuilderEditor.selectMode.ContainsFlag(SelectMode.Face | SelectMode.TextureFace))
                return new ActionResult(ActionResult.Status.Success, "Move " + MeshSelection.selectedFaceCount + (MeshSelection.selectedFaceCount > 1 ? " Faces" : " Face"));
            return new ActionResult(ActionResult.Status.Success, "Move " + MeshSelection.selectedVertexCount + (MeshSelection.selectedVertexCount > 1 ? " Vertices" : " Vertex"));
        }
    }

    class MoveElementsSettings : ConfigurableWindow
    {
        void OnEnable()
        {
            titleContent.text = L10n.Tr("Offset Element Settings");
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui += OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate += OnSceneGUI;
#endif
        }

        void OnDisable()
        {
#if UNITY_2019_1_OR_NEWER
            SceneView.duringSceneGui -= OnSceneGUI;
#else
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
#endif
        }

        void OnGUI()
        {
            DoContextMenu();

            EditorGUI.BeginChangeCheck();

            var dist = OffsetElements.s_Translation.value;
            var coord = OffsetElements.s_CoordinateSpace.value;

            EditorGUI.BeginChangeCheck();

            coord = (OffsetElements.CoordinateSpace) EditorGUILayout.EnumPopup("Coordinate Space", coord);
            dist = EditorGUILayout.Vector3Field("Translate", dist);

            if (EditorGUI.EndChangeCheck())
            {
                OffsetElements.s_Translation.SetValue(dist, true);
                OffsetElements.s_CoordinateSpace.SetValue(coord);
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(L10n.Tr("Offset Selection")))
            {
                var instance = EditorToolbarLoader.GetInstance<OffsetElements>();
                EditorUtility.ShowNotification(instance.DoAction().notification);
            }

            if (EditorGUI.EndChangeCheck())
                SceneView.RepaintAll();
        }

        static List<Vector3> s_Points = new List<Vector3>();

        void OnSceneGUI(SceneView view)
        {
            s_Points.Clear();

            var coord = OffsetElements.s_CoordinateSpace.value;
            var offset = OffsetElements.s_Translation.value;
            var handleRotation = MeshSelection.GetHandleRotation();
            var camera = view.camera.transform.forward * -.01f;

            foreach (var selection in MeshSelection.elementSelection)
            {
                var mesh = selection.mesh;

                if (coord == OffsetElements.CoordinateSpace.Element)
                {
                    foreach (var elements in selection.elementGroups)
                    {
                        s_Points.Add(elements.position + camera);
                        s_Points.Add(elements.rotation * offset);
                    }
                }
                else
                {
                    var preview = offset;

                    if (coord == OffsetElements.CoordinateSpace.Handle)
                        preview = handleRotation * offset;
                    else if (coord == OffsetElements.CoordinateSpace.Local)
                        preview = mesh.transform.TransformDirection(offset);

                    foreach (var elements in selection.elementGroups)
                    {
                        s_Points.Add(elements.position + camera);
                        s_Points.Add(preview);
                    }
                }
            }

            using (var lines = new EditorMeshHandles.LineDrawingScope(ColorUtility.GetColor(offset)))
            {
                for (int i = 0; i < s_Points.Count; i += 2)
                    lines.DrawLine(s_Points[i], s_Points[i] + s_Points[i + 1]);
            }

            using (var points = new EditorMeshHandles.PointDrawingScope(Color.gray))
            {
                for (int i = 0; i < s_Points.Count; i += 2)
                    points.Draw(s_Points[i]);
            }

            using(var points = new EditorMeshHandles.PointDrawingScope(ColorUtility.GetColor(offset)))
            {
                for (int i = 0; i < s_Points.Count; i += 2)
                    points.Draw(s_Points[i] + s_Points[i+1]);
            }
        }
    }
}

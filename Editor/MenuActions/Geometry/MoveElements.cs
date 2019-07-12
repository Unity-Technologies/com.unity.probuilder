using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class MoveElements : MenuAction
    {
        enum CoordinateSpace
        {
            World,
            Local,
            Element,
            Handle
        }

        static readonly TooltipContent s_TooltipFace = new TooltipContent ( "Move Faces", "Move the selected elements by a set amount." );
        static readonly TooltipContent s_TooltipEdge = new TooltipContent ( "Move Edges", "Move the selected elements by a set amount." );
        static readonly TooltipContent s_TooltipVert = new TooltipContent ( "Move Vertices", "Move the selected elements by a set amount." );

        static Pref<Vector3> s_MoveDistance = new Pref<Vector3>("MoveElements.s_MoveDistance", Vector3.up);
        static Pref<CoordinateSpace> s_CoordinateSpace = new Pref<CoordinateSpace>("MoveElements.s_CoordinateSpace", CoordinateSpace.World);

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

        protected override void OnSettingsGUI()
        {
            GUILayout.Label("Move Settings", EditorStyles.boldLabel);

            var dist = s_MoveDistance.value;
            var coord = s_CoordinateSpace.value;

            EditorGUI.BeginChangeCheck();

            coord = (CoordinateSpace) EditorGUILayout.EnumPopup("Space", coord);
            dist = EditorGUILayout.Vector3Field("Move", dist);

            if (EditorGUI.EndChangeCheck())
            {
                s_MoveDistance.SetValue(dist, true);
                s_CoordinateSpace.SetValue(coord);
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Move Selection"))
                EditorUtility.ShowNotification(DoAction().notification);
        }

        public override ActionResult DoAction()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordSelection("Move Elements(s)");

            foreach (var group in MeshSelection.elementSelection)
            {
                var mesh = group.mesh;
                var positions = mesh.positionsInternal;
                var offset = s_MoveDistance.value;

                switch (s_CoordinateSpace.value)
                {
                    case CoordinateSpace.World:
                    case CoordinateSpace.Handle:
                    {
                        var pre = mesh.transform.localToWorldMatrix;
                        var post = mesh.transform.worldToLocalMatrix;

                        if (s_CoordinateSpace.value == CoordinateSpace.Handle)
                            offset = MeshSelection.GetHandleRotation() * offset;

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
}

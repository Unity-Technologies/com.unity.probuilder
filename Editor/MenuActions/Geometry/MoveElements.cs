using UnityEngine;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class MoveElements : MenuAction
    {
        enum CoordinateSpace
        {
            Local,
            World
        }

        static readonly TooltipContent s_TooltipFace = new TooltipContent ( "Move Faces", "Move the selected elements by a set amount." );
        static readonly TooltipContent s_TooltipEdge = new TooltipContent ( "Move Edges", "Move the selected elements by a set amount." );
        static readonly TooltipContent s_TooltipVert = new TooltipContent ( "Move Vertices", "Move the selected elements by a set amount." );

        static Pref<Vector3> s_MoveDistance = new Pref<Vector3>("MoveElements.s_MoveDistance", Vector3.up);
        static Pref<CoordinateSpace> s_CoordinateSpace = new Pref<CoordinateSpace>("MoveElements.s_CoordinateSpace", CoordinateSpace.World);

        public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
        
        public override Texture2D icon
        {
            get { return null; }
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

            foreach (var mesh in MeshSelection.topInternal)
            {
                var positions = mesh.positionsInternal;

                var offset = s_CoordinateSpace.value == CoordinateSpace.World
                    ? mesh.transform.InverseTransformDirection(s_MoveDistance.value)
                    : s_MoveDistance.value;

                foreach (var i in mesh.selectedCoincidentVertices)
                    positions[i] += offset;

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

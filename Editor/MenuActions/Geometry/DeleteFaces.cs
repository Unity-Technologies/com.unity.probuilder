using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    [MenuActionShortcut(typeof(SceneView), KeyCode.Backspace)]
    sealed class DeleteFaces : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Face_Delete", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Delete Faces",
                @"Delete all selected faces.",
                keyCommandDelete
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Face; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedFaceCount > 0; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordSelection("Delete Face");

            int count = 0;

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
            {
                if (pb.selectedFaceCount == pb.facesInternal.Length)
                {
                    Debug.LogWarning("Attempting to delete all faces on this mesh...  I'm afraid I can't let you do that.");
                    continue;
                }

                Undo.RecordObject(pb.renderer, "Update Renderer");

                pb.DeleteFaces(pb.selectedFacesInternal);
                count += pb.selectedFaceCount;

                pb.ToMesh();
                pb.Refresh();
                pb.Optimize();
            }

            MeshSelection.ClearElementSelection();
            ProBuilderEditor.Refresh();

            if (count > 0)
                return new ActionResult(ActionResult.Status.Success, "Delete " + count + " Faces");

            return new ActionResult(ActionResult.Status.Failure, "No Faces Selected");
        }
    }
}

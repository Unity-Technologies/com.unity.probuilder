using UnityEngine;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class MergeFaces : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Face_Merge", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Merge Faces",
                @"Tells ProBuilder to treat the selected faces as if they were a single face.  Be careful not to use this with unconnected faces!"
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

            UndoUtility.RecordSelection("Merge Faces");

            int success = 0;

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
            {
                if (pb.selectedFaceCount > 1)
                {
                    success += pb.selectedFaceCount;

                    Face face = MergeElements.Merge(pb, pb.selectedFacesInternal);

                    pb.ToMesh();
                    pb.Refresh();
                    pb.Optimize();

                    pb.SetSelectedFaces(new Face[] { face });
                }
            }

            ProBuilderEditor.Refresh();

            if (success > 0)
                return new ActionResult(ActionResult.Status.Success, "Merged " + success + " Faces");

            return new ActionResult(ActionResult.Status.Failure, "Merge Faces\nNo Faces Selected");
        }
    }
}

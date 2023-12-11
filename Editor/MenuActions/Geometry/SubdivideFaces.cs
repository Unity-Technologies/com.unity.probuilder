using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class SubdivideFaces : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override string iconPath => "Toolbar/Face_Subdivide";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        protected internal override bool hasFileMenuEntry
        {
            get { return false; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Subdivide Faces",
                @"Inserts a new vertex at the center of each selected face and creates a new edge from the center of each perimeter edge to the center vertex.",
                keyCommandAlt, 'S'
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

            int success = 0;
            UndoUtility.RecordSelection("Subdivide Faces");

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
            {
                Face[] faces = pb.Subdivide(pb.selectedFacesInternal);

                pb.ToMesh();

                if (faces != null)
                {
                    success += pb.selectedFacesInternal.Length;
                    pb.SetSelectedFaces(faces);

                    pb.Refresh();
                    pb.Optimize();
                }
            }

            if (success > 0)
            {
                ProBuilderEditor.Refresh();

                return new ActionResult(ActionResult.Status.Success, "Subdivide " + success + ((success > 1) ? " faces" : " face"));
            }
            else
            {
                Debug.LogWarning("Subdivide faces failed - did you not have any faces selected?");
                return new ActionResult(ActionResult.Status.Failure, "Subdivide Faces\nNo faces selected");
            }
        }
    }
}

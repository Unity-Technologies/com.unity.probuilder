using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ConformFaceNormals : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
        public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Face_ConformNormals", IconSkin.Pro); } }
        public override TooltipContent tooltip { get { return s_TooltipContent; } }
        public override string menuTitle { get { return "Conform Normals"; } }

        static readonly TooltipContent s_TooltipContent = new TooltipContent
            (
                "Conform Face Normals",
                @"Orients all selected faces to face the same direction."
            );

        public override SelectMode validSelectModes
        {
            get { return SelectMode.Face; }
        }

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedFaceCountObjectMax > 1; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            var selection = MeshSelection.topInternal;

            UndoUtility.RecordSelection("Conform " + (MeshSelection.selectedFaceCount > 0 ? "Face" : "Object") + " Normals.");

            ActionResult res = ActionResult.NoSelection;

            foreach (ProBuilderMesh pb in selection)
            {
                var faces = pb.GetSelectedFaces();

                if (faces == null)
                    continue;

                res = UnityEngine.ProBuilder.MeshOperations.SurfaceTopology.ConformNormals(pb, faces);

                pb.ToMesh();
                pb.Refresh();
                pb.Optimize();
            }

            ProBuilderEditor.Refresh();

            return res;
        }
    }
}

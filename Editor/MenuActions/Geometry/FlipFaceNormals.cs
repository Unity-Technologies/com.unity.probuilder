using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class FlipFaceNormals : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Geometry; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Face_FlipNormals", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Flip Face Normals",
                @"Reverses the direction of all faces in selection.",
                keyCommandAlt, 'N'
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

            UndoUtility.RecordSelection("Flip Face Normals");

            int c = 0;
            int faceCount = MeshSelection.selectedFaceCount;

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
            {
                if (pb.selectedFaceCount < 1 && faceCount < 1)
                {
                    foreach (var face in pb.facesInternal)
                        face.Reverse();

                    c += pb.facesInternal.Length;
                }
                else
                {
                    foreach (var face in pb.GetSelectedFaces())
                        face.Reverse();

                    c += pb.selectedFaceCount;
                }


                pb.ToMesh();
                pb.Refresh();
                pb.Optimize();
            }

            if (c > 0)
                return new ActionResult(ActionResult.Status.Success, "Flip " + c + (c > 1 ? " Face Normals" : " Face Normal"));

            return new ActionResult(ActionResult.Status.Canceled, "Flip Normals\nNo Faces Selected");
        }
    }
}

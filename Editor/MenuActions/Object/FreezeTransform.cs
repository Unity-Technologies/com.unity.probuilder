using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEngine;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class FreezeTransform : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Object; }
        }
        public override string iconPath => "Toolbar/Pivot_FreezeTransform";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Freeze Transform",
                @"Set the pivot point to world coordinates (0,0,0) and clear all Transform values while keeping the mesh in place."
            );

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedObjectCount > 0; }
        }

        public bool ShouldFlipFaces(Vector3 scale)
        {
            var globalSign = Mathf.Sign(scale.x) * Mathf.Sign(scale.y) * Mathf.Sign(scale.z);
            return globalSign < 0;
        }

        protected override ActionResult PerformActionImplementation()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordMeshAndTransformSelection("Freeze Transforms");

            var selection = MeshSelection.topInternal;
            Vector3[][] positions = new Vector3[selection.Count][];

            for (int i = 0, c = selection.Count; i < c; i++)
                positions[i] = selection[i].VerticesInWorldSpace();

            for (int i = 0, c = selection.Count; i < c; i++)
            {
                ProBuilderMesh pb = selection[i];
                bool flipFaces = ShouldFlipFaces(pb.transform.localScale);

                pb.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                pb.transform.localScale = Vector3.one;

                foreach(Face face in pb.facesInternal)
                {
                    face.manualUV = true;
                    if(flipFaces)
                        face.Reverse();
                }

                pb.positions = positions[i];

                pb.ToMesh();
                pb.Refresh();
                pb.Optimize();
            }

            ProBuilderEditor.Refresh();
            SceneView.RepaintAll();

            return new ActionResult(ActionResult.Status.Success, "Freeze Transforms");
        }
    }
}

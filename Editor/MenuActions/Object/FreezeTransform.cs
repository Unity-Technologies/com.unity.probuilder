using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class FreezeTransform : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Object; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Pivot_Reset", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Freeze Transform",
                @"Set the pivot point to world/parent coordinates (0,0,0) and clear all Transform values while keeping the mesh in place."
            );

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedObjectCount > 0; }
        }

        public override ActionResult DoAction()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordMeshAndTransformSelection("Freeze Transforms");

            var selection = MeshSelection.topInternal;
                

            for (int i = 0, c = selection.Count; i < c; i++)
            {                
                ProBuilderMesh pb = selection[i];
                TransformUtility.UnparentChildren(pb.transform);
                bool inParentSpace = (pb.transform.parent != null);
                Vector3[] positions = pb.VerticesInParentSpace();

                pb.transform.position = (inParentSpace  ? pb.transform.parent.position : Vector3.zero);
                pb.transform.rotation = (inParentSpace ? pb.transform.parent.rotation : Quaternion.identity);
                pb.transform.localScale = (inParentSpace ? pb.transform.parent.localScale : Vector3.one);

                foreach (Face face in pb.facesInternal)
                    face.manualUV = true;

                pb.positions = positions;

                pb.ToMesh();
                pb.Refresh();
                pb.Optimize();
                TransformUtility.ReparentChildren(pb.transform);
            }

            ProBuilderEditor.Refresh();

            SceneView.RepaintAll();

            return new ActionResult(ActionResult.Status.Success, "Freeze Transforms");
        }
    }
}

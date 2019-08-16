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
                @"Set the pivot point to world/parent coordinates (0,0,0) and clear Transform values while keeping the mesh in place."
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
            Vector3[][] positions = new Vector3[selection.Count][];
            

            for (int i = 0, c = selection.Count; i < c; i++)
                positions[i] = selection[i].VerticesInParentSpace();

            //Note need to sort from children to parent.
            for (int i = 0, c = selection.Count; i < c; i++)
            {                
                ProBuilderMesh pb = selection[i];
                bool inParentSpace = (pb.transform.parent != null);

                pb.transform.position = (inParentSpace  ? pb.transform.parent.position : Vector3.zero);
                pb.transform.rotation = (inParentSpace ? pb.transform.parent.rotation : Quaternion.identity);
                pb.transform.localScale = (inParentSpace ? pb.transform.parent.localScale : Vector3.one);

                foreach (Face face in pb.facesInternal)
                    face.manualUV = true;

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

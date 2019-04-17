using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class MergeObjects : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Object; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Object_Merge", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Merge Objects",
                @"Merges all selected ProBuilder objects to a single mesh."
            );

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedObjectCount > 1; }
        }

        public override ActionResult DoAction()
        {
            if (MeshSelection.selectedObjectCount < 2)
                return new ActionResult(ActionResult.Status.Canceled, "Must Select 2+ Objects");

            var selected = MeshSelection.top.ToArray();
            List<ProBuilderMesh> res = CombineMeshes.Combine(MeshSelection.topInternal);

            if (res != null)
            {
                foreach (var mesh in res)
                {
                    mesh.Optimize();
                    mesh.gameObject.name = Selection.activeGameObject.name + "-Merged";
                    UndoUtility.RegisterCreatedObjectUndo(mesh.gameObject, "Merge Objects");
                    Selection.objects = res.Select(x => x.gameObject).ToArray();
                }

                // Delete donor objects
                for (int i = 0; i < selected.Length; i++)
                {
                    if (selected[i] != null)
                        UndoUtility.DestroyImmediate(selected[i].gameObject);
                }
            }

            ProBuilderEditor.Refresh();

            return new ActionResult(ActionResult.Status.Success, "Merged Objects");
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.ProBuilder.Shapes;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class MergeObjects : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Object; }
        }
        public override string iconPath => "Toolbar/Object_Merge";
        public override Texture2D icon => IconUtility.GetIcon(iconPath);

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
            get { return base.enabled && MeshSelection.selectedObjectCount > 1 && MeshSelection.activeMesh != null; }
        }

        protected override ActionResult PerformActionImplementation()
        {
            if (MeshSelection.selectedObjectCount < 2)
                return new ActionResult(ActionResult.Status.Canceled, "Must Select 2+ Objects");

            DoMergeObjectsAction();
            return new ActionResult(ActionResult.Status.Success, "Merged Objects");
        }

        internal List<ProBuilderMesh> DoMergeObjectsAction()
        {
            var selected = MeshSelection.top.ToArray();
            ProBuilderMesh currentMesh = MeshSelection.activeMesh;
            UndoUtility.RecordObject(currentMesh, "Merge Objects");
            List<ProBuilderMesh> res = CombineMeshes.Combine(MeshSelection.topInternal, currentMesh);

            if (res != null)
            {
                foreach (var mesh in res)
                {
                    mesh.Optimize();
                    if (mesh != currentMesh)
                    {
                        mesh.gameObject.name = Selection.activeGameObject.name + "-Merged";
                        UndoUtility.RegisterCreatedObjectUndo(mesh.gameObject, "Merge Objects");
                        
                        Selection.objects = res.Select(x => x.gameObject).ToArray();
                    }
                    
                    // Remove PolyShape and ProBuilderShape components if any are present post-merge
                    var polyShapeComp = mesh.gameObject.GetComponent<PolyShape>();
                    if (polyShapeComp != null )
                        UndoUtility.DestroyImmediate(polyShapeComp);
                    
                    var proBuilderShape = mesh.gameObject.GetComponent<ProBuilderShape>();
                    if (proBuilderShape != null )
                        UndoUtility.DestroyImmediate(proBuilderShape);
                }

                // Delete donor objects if they are not part of the result
                for (int i = 0; i < selected.Length; i++)
                {
                    if (selected[i] != null && res.Contains(selected[i]) == false)
                        UndoUtility.DestroyImmediate(selected[i].gameObject);
                }
            }

            return res;
        }
    }
}


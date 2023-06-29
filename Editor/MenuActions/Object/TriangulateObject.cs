using UnityEngine.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class TriangulateObject : MenuAction
    {
        public override ToolbarGroup group { get { return ToolbarGroup.Object; } }
        public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Object_Triangulate", IconSkin.Pro); } }
        public override TooltipContent tooltip { get { return s_Tooltip; } }
        public override string menuTitle { get { return "Triangulate"; } }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Triangulate Objects",
                @"Removes all quads and n-gons on the mesh and inserts triangles instead.  Use this and a hard smoothing group to achieve a low-poly facetized look."
            );

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedObjectCount > 0; }
        }

#if UNITY_2023_2_OR_NEWER
        [MenuItem("CONTEXT/ProBuilderMesh/Triangulate", true)]
        static bool ValidateTriangulateObjectAction()
        {
            return MeshSelection.selectedObjectCount > 0;
        }

        // This boolean allows to call the action only once in case of multi-selection as PB actions
        // are called on the entire selection and not per element.
        static bool s_ActionAlreadyTriggered = false;
        [MenuItem("CONTEXT/ProBuilderMesh/Triangulate")]
        static void TriangulateObjectAction(MenuCommand command)
        {
            if (!s_ActionAlreadyTriggered)
            {
                s_ActionAlreadyTriggered = true;
                //Once again, delayCall is necessary to prevent multiple call in case of multi-selection
                EditorApplication.delayCall += () =>
                {
                    EditorToolbarLoader.GetInstance<TriangulateObject>().PerformAction();
                    s_ActionAlreadyTriggered = false;
                };
            }
        }
#endif

        protected override ActionResult PerformActionImplementation()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordSelection("Triangulate Objects");

            foreach (var mesh in MeshSelection.topInternal)
            {
                mesh.ToMesh();
                mesh.ToTriangles(mesh.facesInternal);
                mesh.Refresh();
                mesh.Optimize();
                mesh.ClearSelection();
            }

            ProBuilderEditor.Refresh();

            var c = MeshSelection.selectedObjectCount;
            return new ActionResult(ActionResult.Status.Success, "Triangulate " + c + (c > 1 ? " Objects" : " Object"));
        }
    }
}

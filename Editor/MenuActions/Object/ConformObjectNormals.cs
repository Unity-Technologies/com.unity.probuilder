using UnityEngine.ProBuilder;
using UnityEngine;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class ConformObjectNormals : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Object; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Object_ConformNormals", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        public override string menuTitle
        {
            get { return "Conform Normals"; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Conform Object Normals",
                @"Check the object for faces that are flipped in the opposite direction of most other faces, then reverses any dissenters."
            );

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedObjectCount > 0; }
        }

#if UNITY_2023_2_OR_NEWER
        [MenuItem("CONTEXT/ProBuilderMesh/Conform Object Normals", true)]
        static bool ValidateConformObjectNormalsAction()
        {
            return MeshSelection.selectedObjectCount > 0;
        }

        // This boolean allows to call the action only once in case of multi-selection as PB actions
        // are called on the entire selection and not per element.
        static bool s_ActionAlreadyTriggered = false;
        [MenuItem("CONTEXT/ProBuilderMesh/Conform Object Normals")]
        static void ConformObjectNormalsAction(MenuCommand command)
        {
            if (!s_ActionAlreadyTriggered)
            {
                s_ActionAlreadyTriggered = true;
                //Once again, delayCall is necessary to prevent multiple call in case of multi-selection
                EditorApplication.delayCall += () =>
                {
                    EditorToolbarLoader.GetInstance<ConformObjectNormals>().PerformAction();
                    s_ActionAlreadyTriggered = false;
                };
            }
        }
#endif

        protected override ActionResult PerformActionImplementation()
        {
            UndoUtility.RecordSelection("Conform Object Normals");

            ActionResult res = ActionResult.NoSelection;

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
            {
                res = UnityEngine.ProBuilder.MeshOperations.SurfaceTopology.ConformNormals(pb, pb.faces);

                pb.ToMesh();
                pb.Refresh();
                pb.Optimize();
            }

            ProBuilderEditor.Refresh();
            return res;
        }
    }
}

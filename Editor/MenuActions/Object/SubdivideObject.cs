using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class SubdivideObject : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Object; }
        }

        public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Object_Subdivide"); } }
        public override Texture2D icon2x { get { return IconUtility.GetLargeIcon("Toolbar/Object_Subdivide"); } }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Subdivide Object",
                "Increase the number of edges and vertices on this object by creating 4 new quads in every face."
            );

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedObjectCount > 0; }
        }

#if UNITY_2023_2_OR_NEWER
        [MenuItem("CONTEXT/ProBuilderMesh/Subdivide Object", true)]
        static bool ValidateSubdivideObjectAction()
        {
            return MeshSelection.selectedObjectCount > 0;
        }

        // This boolean allows to call the action only once in case of multi-selection as PB actions
        // are called on the entire selection and not per element.
        static bool s_ActionAlreadyTriggered = false;
        [MenuItem("CONTEXT/ProBuilderMesh/Subdivide Object", false, 15)]
        static void SubdivideObjectAction(MenuCommand command)
        {
            if (!s_ActionAlreadyTriggered)
            {
                s_ActionAlreadyTriggered = true;
                //Once again, delayCall is necessary to prevent multiple call in case of multi-selection
                EditorApplication.delayCall += () =>
                {
                    EditorToolbarLoader.GetInstance<SubdivideObject>().PerformAction();
                    s_ActionAlreadyTriggered = false;
                };
            }
        }
#endif

        protected override ActionResult PerformActionImplementation()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            UndoUtility.RecordSelection("Subdivide Selection");

            int success = 0;

            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
            {
                pb.ToMesh();

                if (pb.Subdivide())
                    success++;
                else
                    Debug.LogError($"Subidivision of [{pb.name}] failed, complex concave objects are not supported");

                pb.Refresh();
                pb.Optimize();

                pb.SetSelectedVertices(new int[0]);
            }

            ProBuilderEditor.Refresh();
            return new ActionResult(ActionResult.Status.Success, "Subdivide " + success + " Objects");
        }
    }
}

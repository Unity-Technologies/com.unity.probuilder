using UnityEngine.ProBuilder;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class CenterPivot : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Object; }
        }

        public override Texture2D icon
        {
            get { return IconUtility.GetIcon("Toolbar/Pivot_CenterOnObject", IconSkin.Pro); }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Center Pivot",
                @"Set the pivot point of this object to the center of its bounds."
            );

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedObjectCount > 0; }
        }

#if UNITY_2023_2_OR_NEWER
        [MenuItem("CONTEXT/ProBuilderMesh/Center Pivot", true)]
        static bool ValidateCenterPivotAction()
        {
            return MeshSelection.selectedObjectCount > 0;
        }

        // This boolean allows to call the action only once in case of multi-selection as PB actions
        // are called on the entire selection and not per element.
        static bool s_ActionAlreadyTriggered = false;
        [MenuItem("CONTEXT/ProBuilderMesh/Center Pivot")]
        static void CenterPivotAction(MenuCommand command)
        {
            if (!s_ActionAlreadyTriggered)
            {
                s_ActionAlreadyTriggered = true;
                //Once again, delayCall is necessary to prevent multiple call in case of multi-selection
                EditorApplication.delayCall += () =>
                {
                    EditorToolbarLoader.GetInstance<CenterPivot>().PerformAction();
                    s_ActionAlreadyTriggered = false;
                };
            }
        }
#endif

        protected override ActionResult PerformActionImplementation()
        {
            if (MeshSelection.selectedObjectCount < 1)
                return ActionResult.NoSelection;

            Object[] objects = new Object[MeshSelection.selectedObjectCount * 2];

            for (int i = 0, c = MeshSelection.selectedObjectCount; i < c; i++)
            {
                objects[i] = MeshSelection.topInternal[i];
                objects[i + c] = MeshSelection.topInternal[i].transform;
            }

            UndoUtility.RegisterCompleteObjectUndo(objects, "Center Pivot");

            foreach (var mesh in MeshSelection.topInternal)
            {
                TransformUtility.UnparentChildren(mesh.transform);
                mesh.CenterPivot(null);
                mesh.Optimize();
                TransformUtility.ReparentChildren(mesh.transform);
            }

            ProBuilderEditor.Refresh();

            return new ActionResult(ActionResult.Status.Success, "Center Pivot");
        }
    }
}

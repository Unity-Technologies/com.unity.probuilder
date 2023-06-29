using UnityEngine.ProBuilder;
using UnityEngine;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class SetCollider : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Entity; }
        }

        public override Texture2D icon
        {
            get { return null; }
        }

        public override TooltipContent tooltip
        {
            get { return s_Tooltip; }
        }

        static readonly TooltipContent s_Tooltip = new TooltipContent
            (
                "Set Collider",
                "Apply the Collider material and adds a mesh collider (if no collider is present). The MeshRenderer will be automatically turned off on entering play mode."
            );

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedObjectCount > 0; }
        }

#if UNITY_2023_2_OR_NEWER
        [MenuItem("CONTEXT/ProBuilderMesh/Set Collider", true)]
        static bool ValidateSetColliderAction()
        {
            return MeshSelection.selectedObjectCount > 0;
        }

        // This boolean allows to call the action only once in case of multi-selection as PB actions
        // are called on the entire selection and not per element.
        static bool s_ActionAlreadyTriggered = false;
        [MenuItem("CONTEXT/ProBuilderMesh/Set Collider")]
        static void SetColliderAction(MenuCommand command)
        {
            if (!s_ActionAlreadyTriggered)
            {
                s_ActionAlreadyTriggered = true;
                //Once again, delayCall is necessary to prevent multiple call in case of multi-selection
                EditorApplication.delayCall += () =>
                {
                    EditorToolbarLoader.GetInstance<SetCollider>().PerformAction();
                    s_ActionAlreadyTriggered = false;
                };
            }
        }
#endif

        protected override ActionResult PerformActionImplementation()
        {
            foreach (ProBuilderMesh pb in MeshSelection.topInternal)
            {
                var existing = pb.GetComponents<EntityBehaviour>();

                // For now just nuke any existing entity types (since there are only two). In the future we should be
                // smarter about conflicting entity types.
                for (int i = 0, c = existing.Length; i < c; i++)
                    Undo.DestroyObjectImmediate(existing[i]);

                if (pb.TryGetComponent<Entity>(out var entity))
                    Undo.DestroyObjectImmediate(entity);

                if (!pb.GetComponent<Collider>())
                    Undo.AddComponent<MeshCollider>(pb.gameObject);

                if (!pb.GetComponent<Renderer>())
                    Undo.AddComponent<MeshRenderer>(pb.gameObject);

                UndoUtility.RegisterCompleteObjectUndo(pb, "Set Collider");

                Undo.AddComponent<ColliderBehaviour>(pb.gameObject).Initialize();
            }

            int selectionCount = MeshSelection.selectedObjectCount;

            if (selectionCount < 1)
                return new ActionResult(ActionResult.Status.NoChange, "Set Collider\nNo objects selected");

            return new ActionResult(ActionResult.Status.Success, "Set Collider\nSet " + selectionCount + " Objects");
        }
    }
}

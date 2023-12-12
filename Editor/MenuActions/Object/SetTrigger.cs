using System;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;

namespace UnityEditor.ProBuilder.Actions
{
    sealed class SetTrigger : MenuAction
    {
        public override ToolbarGroup group
        {
            get { return ToolbarGroup.Entity; }
        }

        public override string iconPath => string.Empty;
        public override Texture2D icon => null;

        public override TooltipContent tooltip
        {
            get { return _tooltip; }
        }

        static readonly TooltipContent _tooltip = new TooltipContent
            (
                "Set Trigger",
                "Apply the Trigger material and adds a collider marked as a trigger. The MeshRenderer will be automatically turned off on entering play mode."
            );

        public override bool enabled
        {
            get { return base.enabled && MeshSelection.selectedObjectCount > 0; }
        }

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

                UndoUtility.RegisterCompleteObjectUndo(pb, "Set Trigger");

                Undo.AddComponent<TriggerBehaviour>(pb.gameObject).Initialize();
            }

            int selectionCount = MeshSelection.selectedObjectCount;
            if (selectionCount < 1)
                return new ActionResult(ActionResult.Status.NoChange, "Set Trigger\nNo objects selected");

            return new ActionResult(ActionResult.Status.Success, "Set Trigger\nSet " + selectionCount + " Objects");
        }
    }
}

using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;

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
			get { return _tooltip; }
		}

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Set Collider",
			"Apply the Collider material and adds a mesh collider (if no collider is present). The MeshRenderer will be automatically turned off on entering play mode."
		);

		public override bool enabled
		{
			get { return ProBuilderEditor.instance != null && MeshSelection.TopInternal().Length > 0; }
		}

		public override ActionResult DoAction()
		{
			foreach (ProBuilderMesh pb in MeshSelection.Top())
			{
				var existing = pb.GetComponents<EntityBehaviour>();

				// For now just nuke any existing entity types (since there are only two). In the future we should be
				// smarter about conflicting entity types.
				for (int i = 0, c = existing.Length; i < c; i++)
					Undo.DestroyObjectImmediate(existing[i]);

				var entity = pb.GetComponent<Entity>();

				if (entity != null)
					Undo.DestroyObjectImmediate(entity);

				if (!pb.GetComponent<Collider>())
					Undo.AddComponent<MeshCollider>(pb.gameObject);

				if (!pb.GetComponent<Renderer>())
					Undo.AddComponent<MeshRenderer>(pb.gameObject);

				UndoUtility.RegisterCompleteObjectUndo(pb, "Set Collider");

				Undo.AddComponent<ColliderBehaviour>(pb.gameObject).Initialize();
			}

			int selectionCount = MeshSelection.TopInternal().Length;

			if (selectionCount < 1)
				return new ActionResult(ActionResult.Status.NoChange, "Set Collider\nNo objects selected");

			return new ActionResult(ActionResult.Status.Success, "Set Collider\nSet " + selectionCount + " Objects");
		}
	}
}

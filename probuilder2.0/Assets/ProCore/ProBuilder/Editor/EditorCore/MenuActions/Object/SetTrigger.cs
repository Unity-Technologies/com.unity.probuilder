using System;
using ProBuilder.Core;
using ProBuilder.EditorCore;
using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;

namespace ProBuilder.Actions
{
	class SetTrigger : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Entity; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return true; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Set Trigger",
			"Apply the Trigger material and adds a collider marked as a trigger. The MeshRenderer will be automatically turned off on entering play mode."
		);

		public override bool IsEnabled()
		{
			return pb_Editor.instance != null && selection != null && selection.Length > 0;
		}

		public override pb_ActionResult DoAction()
		{
			foreach (pb_Object pb in pb_Selection.All())
			{
				var existing = pb.GetComponents<pb_EntityBehaviour>();

				// For now just nuke any existing entity types (since there are only two). In the future we should be
				// smarter about conflicting entity types.
				for (int i = 0, c = existing.Length; i < c; i++)
					Undo.DestroyObjectImmediate(existing[i]);

				var entity = pb.GetComponent<pb_Entity>();

				if (entity != null)
					Undo.DestroyObjectImmediate(entity);

				if (!pb.GetComponent<Collider>())
					Undo.AddComponent<MeshCollider>(pb.gameObject);

				if (!pb.GetComponent<Renderer>())
					Undo.AddComponent<MeshRenderer>(pb.gameObject);

				pb_Undo.RegisterCompleteObjectUndo(pb, "Set Trigger");

				Undo.AddComponent<pb_TriggerBehaviour>(pb.gameObject).Initialize();
			}

			int selectionCount = pb_Selection.All().Length;

			if(selectionCount < 1)
				return new pb_ActionResult(Status.NoChange, "Set Trigger\nNo objects selected");

			return new pb_ActionResult(Status.Success, "Set Trigger\nSet " + selectionCount + " Objects");
		}
	}
}

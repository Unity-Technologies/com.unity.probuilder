// This script demonstrates how to create a new action that can be accessed from the ProBuilder toolbar.
// A new menu item is registered under "Geometry" actions called "Make Double-Sided".

using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine.ProBuilder;
using UnityEngine.ProBuilder.MeshOperations;

namespace ProBuilder.ExampleActions
{
	/// <summary>
	/// This is the actual action that will be executed.
	/// </summary>
	[ProBuilderMenuAction]
	public class MakeFacesDoubleSided : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return null; } }
		public override TooltipContent tooltip { get { return k_Tooltip; } }

		/// <summary>
		/// What to show in the hover tooltip window.
		/// TooltipContent is similar to GUIContent, with the exception that it also includes an optional params[]
		/// char list in the constructor to define shortcut keys (ex, CMD_CONTROL, K).
		/// </summary>
		static readonly TooltipContent k_Tooltip = new TooltipContent
		(
			"Set Double-Sided",
			"Adds another face to the back of the selected faces."
		);

		/// <summary>
		/// Determines if the action should be enabled or grayed out.
		/// </summary>
		/// <returns></returns>
		public override bool enabled
		{
			get { return MeshSelection.selectedFaceCount > 0; }
		}

		/// <summary>
		/// This action is applicable in Face selection modes.
		/// </summary>
		public override SelectMode validSelectModes
		{
			get { return SelectMode.Face | SelectMode.TextureFace; }
		}

		/// <summary>
		/// Return a pb_ActionResult indicating the success/failure of action.
		/// </summary>
		/// <returns></returns>
		protected override ActionResult PerformActionImplementation()
		{
			var selection = MeshSelection.top.ToArray();
			Undo.RecordObjects(selection, "Make Double-Sided Faces");

			foreach(var mesh in selection)
			{
				AppendElements.DuplicateAndFlip(mesh, mesh.GetSelectedFaces());

				mesh.ToMesh();
				mesh.Refresh();
				mesh.Optimize();
			}

			// Rebuild the pb_Editor caches
			ProBuilderEditor.Refresh();

			return new ActionResult(ActionResult.Status.Success, "Make Faces Double-Sided");
		}
	}
}

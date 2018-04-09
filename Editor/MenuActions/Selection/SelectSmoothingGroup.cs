using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;
using System.Collections.Generic;
using System.Linq;
using ProBuilder.Core;
using ProBuilder.EditorCore;

namespace ProBuilder.Actions
{
	class SelectSmoothingGroup : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Selection; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Selection_SelectBySmoothingGroup", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return m_Tooltip; } }

		private static readonly pb_TooltipContent m_Tooltip = new pb_TooltipContent
		(
			"Select by Smooth",
			"Selects all faces matching the selected smoothing groups."
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					pb_Editor.instance.editLevel != EditLevel.Top &&
					selection != null &&
					selection.Length > 0 &&
					selection.Any(x => x.SelectedFaceCount > 0);
		}

		public override bool IsHidden()
		{
			return true;
		}

		public override MenuActionState AltState()
		{
			if(	IsEnabled() &&
				pb_Editor.instance.editLevel == EditLevel.Geometry &&
				pb_Editor.instance.selectionMode == SelectMode.Face)
				return MenuActionState.VisibleAndEnabled;

			return MenuActionState.Visible;
		}

		public override pb_ActionResult DoAction()
		{
			pb_Undo.RecordSelection(selection, "Select Faces with Smoothing Group");

			HashSet<int> selectedSmoothGroups = new HashSet<int>(selection.SelectMany(x => x.SelectedFaces.Select(y => y.smoothingGroup)));

			List<GameObject> newSelection = new List<GameObject>();

			foreach(pb_Object pb in selection)
			{
				IEnumerable<pb_Face> matches = pb.faces.Where(x => selectedSmoothGroups.Contains(x.smoothingGroup));

				if(matches.Count() > 0)
				{
					newSelection.Add(pb.gameObject);
					pb.SetSelectedFaces(matches);
				}
			}

			Selection.objects = newSelection.ToArray();

			pb_Editor.Refresh();

			return new pb_ActionResult(Status.Success, "Select Faces with Smoothing Group");
		}
	}
}



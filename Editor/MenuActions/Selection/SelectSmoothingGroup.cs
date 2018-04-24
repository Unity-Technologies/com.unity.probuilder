using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Collections.Generic;
using System.Linq;
using ProBuilder.Core;
using UnityEditor.ProBuilder;

namespace ProBuilder.Actions
{
	class SelectSmoothingGroup : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Selection; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Selection_SelectBySmoothingGroup", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return m_Tooltip; } }

		private static readonly TooltipContent m_Tooltip = new TooltipContent
		(
			"Select by Smooth",
			"Selects all faces matching the selected smoothing groups."
		);

		public override bool IsEnabled()
		{
			return 	ProBuilderEditor.instance != null &&
					ProBuilderEditor.instance.editLevel != EditLevel.Top &&
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
				ProBuilderEditor.instance.editLevel == EditLevel.Geometry &&
				ProBuilderEditor.instance.selectionMode == SelectMode.Face)
				return MenuActionState.VisibleAndEnabled;

			return MenuActionState.Visible;
		}

		public override pb_ActionResult DoAction()
		{
			UndoUtility.RecordSelection(selection, "Select Faces with Smoothing Group");

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

			ProBuilderEditor.Refresh();

			return new pb_ActionResult(Status.Success, "Select Faces with Smoothing Group");
		}
	}
}



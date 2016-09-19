using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder2.Actions
{
	public class SelectMaterial : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Selection; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Selection_SelectByMaterial"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Select by Material",
			"Selects all faces matching the selected materials."
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
			return 	editLevel != EditLevel.Geometry;
		}

		public override pb_ActionResult DoAction()
		{
			pbUndo.RecordSelection(selection, "Select Faces with Material");

			HashSet<Material> sel = new HashSet<Material>(selection.SelectMany(x => x.SelectedFaces.Select(y => y.material).Where( z => z != null)));
			List<GameObject> newSelection = new List<GameObject>();

			foreach(pb_Object pb in Object.FindObjectsOfType<pb_Object>())
			{
				IEnumerable<pb_Face> matches = pb.faces.Where(x => sel.Contains(x.material));

				if(matches.Count() > 0)
				{
					newSelection.Add(pb.gameObject);
					pb.SetSelectedFaces(matches);
				}
			}

			Selection.objects = newSelection.ToArray();

			pb_Editor.Refresh();

			return new pb_ActionResult(Status.Success, "Select Faces with Material");
		}
	}
}



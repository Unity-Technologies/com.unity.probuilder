using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using ProBuilder.Core;
using UnityEditor.ProBuilder;
using ProBuilder.MeshOperations;

namespace ProBuilder.Actions
{
	class TriangulateFaces : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Face_Triangulate", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Triangulate Faces",
			"Break all selected faces down to triangles."
		);

		public override bool IsEnabled()
		{
			return 	ProBuilderEditor.instance != null &&
					editLevel == EditLevel.Geometry &&
					selection != null &&
					selection.Length > 0 &&
					selection.Sum(x => x.SelectedFaceCount) > 0;
		}

		public override bool IsHidden()
		{
			return 	editLevel != EditLevel.Geometry ||
					(PreferencesInternal.GetBool(pb_Constant.pbElementSelectIsHamFisted) && selectionMode != SelectMode.Face);
		}

		public override pb_ActionResult DoAction()
		{
			pb_ActionResult res = pb_ActionResult.NoSelection;

			UndoUtility.RecordSelection(selection, "Triangulate Faces");

			foreach(pb_Object pb in selection)
			{
				pb_Face[] triangulatedFaces = null;
				pb.ToMesh();
				res = pb.ToTriangles(pb.SelectedFaces, out triangulatedFaces);
				pb.Refresh();
				pb.Optimize();
				pb.SetSelectedFaces(triangulatedFaces);
			}

			ProBuilderEditor.Refresh();

			return res;
		}
	}
}

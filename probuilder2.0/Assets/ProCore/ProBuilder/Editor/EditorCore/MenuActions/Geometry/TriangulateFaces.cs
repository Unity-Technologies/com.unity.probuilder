using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;
using System.Linq;
using ProBuilder.Core;
using ProBuilder.EditorCore;
using ProBuilder.MeshOperations;

namespace ProBuilder.Actions
{
	class TriangulateFaces : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Face_Triangulate", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Triangulate Faces",
			"Break all selected faces down to triangles."
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					editLevel == EditLevel.Geometry &&
					selection != null &&
					selection.Length > 0 &&
					selection.Sum(x => x.SelectedFaceCount) > 0;
		}

		public override bool IsHidden()
		{
			return 	editLevel != EditLevel.Geometry ||
					(pb_PreferencesInternal.GetBool(pb_Constant.pbElementSelectIsHamFisted) && selectionMode != SelectMode.Face);
		}

		public override pb_ActionResult DoAction()
		{
			pb_ActionResult res = pb_ActionResult.NoSelection;

			pb_Undo.RecordSelection(selection, "Triangulate Faces");

			foreach(pb_Object pb in selection)
			{
				pb_Face[] triangulatedFaces = null;
				pb.ToMesh();
				res = pb.ToTriangles(pb.SelectedFaces, out triangulatedFaces);
				pb.Refresh();
				pb.Optimize();
				pb.SetSelectedFaces(triangulatedFaces);
			}

			pb_Editor.Refresh();

			return res;
		}
	}
}

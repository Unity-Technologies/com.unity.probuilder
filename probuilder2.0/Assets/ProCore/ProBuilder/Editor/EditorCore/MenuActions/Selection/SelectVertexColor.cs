using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;
using System.Collections.Generic;
using System.Linq;

namespace ProBuilder2.Actions
{
	public class SelectVertexColor : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Selection; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Select Faces with Vertex Colors",
			"Selects all faces matching the selected vertex colors."
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					pb_Editor.instance.editLevel != EditLevel.Top &&
					selectionMode == SelectMode.Face &&
					selection != null &&
					selection.Length > 0;
		}

		public override bool IsHidden()
		{
			return true;
		}

		public override pb_ActionResult DoAction()
		{
			pbUndo.RecordSelection(selection, "Select Faces with Vertex Colors");
			
			HashSet<Color32> colors = new HashSet<Color32>();			

			foreach(pb_Object pb in selection)
			{
				Color[] mesh_colors = pb.colors;

				if(mesh_colors == null || mesh_colors.Length != pb.vertexCount)
					continue;

				foreach(int i in pb.SelectedTriangles)
					colors.Add(mesh_colors[i]);
			}

			List<GameObject> newSelection = new List<GameObject>();

			foreach(pb_Object pb in Object.FindObjectsOfType<pb_Object>())
			{
				Color[] mesh_colors = pb.colors;

				if(mesh_colors == null || mesh_colors.Length != pb.vertexCount)
					continue;

				List<pb_Face> matches = new List<pb_Face>();
				pb_Face[] faces = pb.faces;

				for(int i = 0; i < faces.Length; i++)
				{
					int[] tris = faces[i].distinctIndices;

					for(int n = 0; n < tris.Length; n++)
					{
						if( colors.Contains((Color32)mesh_colors[tris[n]]) )
						{
							matches.Add(faces[i]);
							break;
						}
					}
				}

				if(matches.Count > 0)
				{
					newSelection.Add(pb.gameObject);
					pb.SetSelectedFaces(matches);
				}
			}

			Selection.objects = newSelection.ToArray();
			
			pb_Editor.Refresh();

			return new pb_ActionResult(Status.Success, "Select Faces with Vertex Colors");
		}
	}
}



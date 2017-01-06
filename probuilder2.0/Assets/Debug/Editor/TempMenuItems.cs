using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;
using System.Linq;
using System.Text;
using System;
using System.Reflection;

public class TempMenuItems : EditorWindow
{
	[MenuItem("Tools/Temp Menu Item &d")]
	static void MenuInit()
	{
		pb_Object[] selection = Selection.transforms.GetComponents<pb_Object>();

		pbUndo.RecordObjects(selection, "Extrude Faces (Experimental)");

		foreach(pb_Object pb in selection)
		{
			pb.Extrude(pb.SelectedFaces, ExtrudeMethod.VertexNormal, .25f);
			pb.ToMesh();
			pb.Refresh();
			pb.Optimize();

			// HashSet<pb_Face> used = new HashSet<pb_Face>();
			// List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb, pb.SelectedFaces, true);
			// List<List<pb_Face>> groups = new List<List<pb_Face>>();

			// foreach(pb_WingedEdge wing in wings)
			// {
			// 	if(used.Add(wing.face))
			// 	{
			// 		HashSet<pb_Face> group = new HashSet<pb_Face>() { wing.face };

			// 		pb_GrowShrink.Flood(wing, group);

			// 		foreach(pb_Face f in group)
			// 			used.Add(f);

			// 		groups.Add(group.ToList());
			// 	}
			// }

			// pb.SetSelectedEdges( groups.SelectMany(x => pbMeshUtils.GetPerimeterEdges(pb, x)) );
		}

		pb_Editor.Refresh(true);
	}

}

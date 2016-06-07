using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;
using System.Linq;
using System.Text;
using System.Reflection;

using Parabox.Debug;

public class TempMenuItems : EditorWindow
{
	[MenuItem("Tools/Temp Menu Item &d")]
	static void MenuInit()
	{
		// GameObject go = GameObject.Find("bevel_test");
		// if(go != null)
		// 	GameObject.DestroyImmediate(go);
		// pb_Object pb = pb_ShapeGenerator.CubeGenerator(Vector3.one);
		// go = pb.gameObject;


		// pb_Edge edge = pb.faces[0].edges[0];

		// pb.ToMesh();
		// pb_Bevel.BevelEdge(pb, edge, .2f);

		// pb.Refresh();
		// pb.Optimize();

		foreach(pb_Object pb in Selection.transforms.GetComponents<pb_Object>())
		{
			profiler.Begin("gen winged edge");

			List<pb_WingedEdge> wingedEdges = pb_WingedEdge.GenerateWingedEdges(pb);

			List<pb_Edge> selection = new List<pb_Edge>();
			
			foreach(pb_Edge e in pb.SelectedEdges)
			{
				pb_WingedEdge wing = wingedEdges.FirstOrDefault(x => x.edge.local.Equals(e));
				pb_WingedEdge opp = wing.opposite;

				int loopbreaker = 0;
				while(opp != null && opp != wing && loopbreaker++ < 500)
				{
					selection.Add(opp.edge.local);
					opp = opp.next.next.opposite;
				}
			}
			profiler.End();

			pb.SetSelectedEdges(selection);
			pb_Editor.instance.UpdateSelection();
		}
	}
}

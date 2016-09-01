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
		pb_Object[] selection = Selection.transforms.GetComponents<pb_Object>();

		foreach(pb_Object pb in selection)
		{
			List<pb_WingedEdge> wings = pb_WingedEdge.GetWingedEdges(pb);
			pb_Edge edge = pb.SelectedEdges.FirstOrDefault();
			if(edge == null) continue;
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();
			pb_Edge common = new pb_Edge(lookup[edge.x], lookup[edge.y]);
			pb_WingedEdge wing = wings.FirstOrDefault(x => x.edge.common.Equals(common));
			if(wing == null) continue;

			List<pb_WingedEdge> spokes = pbMeshUtils.GetSpokes(wing, wing.edge.common.x, true);
			Debug.Log("spokes: " + spokes.Count);
			if(spokes == null)
			{
				pb.SetSelectedEdges(new List<pb_Edge>());
				continue;
			}
			IEnumerable<pb_EdgeLookup> el = spokes.Select(x => x.edge);
			el = el.Distinct();
			pb.SetSelectedEdges( el.Select(x => x.local) );
		}

		pb_Editor.Refresh();
	}
}

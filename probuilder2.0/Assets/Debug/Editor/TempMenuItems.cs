using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using System.Linq;
using System.Text;
using System.Reflection;

// using TMesh = TriangleNet.Mesh;
// using UMesh = UnityEngine.Mesh;
// using TriangleNet;
// using TriangleNet.Data;
// using TriangleNet.Geometry;

using Parabox.Debug;

public class TempMenuItems : EditorWindow
{
	static pb_Profiler profiler = new pb_Profiler("sort indices");

	[MenuItem("Tools/Temp Menu Item &d")]
	static void MenuInit()
	{
		foreach(pb_Object pb in Selection.transforms.GetComponents<pb_Object>())
		{
			pb_Vertex[] vertices = pb_Vertex.GetVertices(pb.msh);
			IEnumerable<pb_Tuple<pb_Vertex, int>> indexed = vertices.Select((x,i)=>new pb_Tuple<pb_Vertex, int>(x, i));
			IEnumerable<IGrouping<pb_Vertex, int>> common = indexed.GroupBy( x => x.Item1, x => x.Item2 );
			Debug.Log(pb.sharedIndices.Count() + " -> " + common.Count());
		}
	}
}

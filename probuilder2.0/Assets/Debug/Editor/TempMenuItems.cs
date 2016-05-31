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
	[MenuItem("Tools/Temp Menu Item &d")]
	static void MenuInit()
	{
		foreach(pb_Object pb in Selection.transforms.GetComponents<pb_Object>())
		{
			pb_Vertex[] vertices = pb_Vertex.GetVertices(pb.msh);
			IEnumerable<pb_Tuple<pb_Vertex, int>> indexed = vertices.Select((x,i)=>new pb_Tuple<pb_Vertex, int>(x, i));
			List<IGrouping<pb_Vertex, int>> common = indexed.GroupBy( x => x.Item1, x => x.Item2 ).ToList();

			Dictionary<int, int> lookup = new Dictionary<int, int>();

			for(int i = 0; i < common.Count; i++)	
				foreach(int n in common[i])
					if(!lookup.ContainsKey(n))
						lookup.Add(n, i);

			pb_Vertex[] condensed = common.Select(x => x.Key).ToArray();
			int[] tris = pb.msh.triangles;

			for(int i = 0; i < tris.Length; i++)
				tris[i] = lookup[tris[i]];

			// Mesh m = new Mesh();
			// pb_Vertex.SetMesh(m, condensed);
			// m.triangles = tris;

			// GameObject go = GameObject.Instantiate(pb.gameObject);
			// GameObject.DestroyImmediate(go.GetComponent<pb_Object>());
			// GameObject.DestroyImmediate(go.GetComponent<pb_Entity>());
			// go.GetComponent<MeshFilter>().sharedMesh = m;
		}
	}
}

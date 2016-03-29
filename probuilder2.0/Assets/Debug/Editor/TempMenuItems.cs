using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using System.Linq;

using TMesh = TriangleNet.Mesh;
using UMesh = UnityEngine.Mesh;
using TriangleNet;
using TriangleNet.Data;
using TriangleNet.Geometry;

public class TempMenuItems : EditorWindow
{
	[MenuItem("Tools/Temp Menu Item &d")]
	static void MenuInit()
	{
		const int VERTEX_COUNT = 128;
		
		Vector2[] v = new Vector2[VERTEX_COUNT];

		for(int i = 0; i < v.Length; i++)
			v[i] = new Vector2( Random.Range(-10f, 10f), Random.Range(-10f, 10f) );

		List<Vector2> v2 = pb_Math.SortCounterClockwise(v);

		List<int> indices = pb_Triangulation.Triangulate(v2);
	
		UMesh m = new UMesh();
		m.vertices = v2.Select(x=>(Vector3)x).ToArray();
		m.uv = v2.ToArray();
		m.triangles = indices.ToArray();
		m.normals = pbUtil.Fill<Vector3>(-Vector3.forward, VERTEX_COUNT);

		GameObject go = new GameObject();
		go.AddComponent<MeshFilter>().sharedMesh = m;
		go.AddComponent<MeshRenderer>().sharedMaterial = pb_Constant.DefaultMaterial;
	}

}

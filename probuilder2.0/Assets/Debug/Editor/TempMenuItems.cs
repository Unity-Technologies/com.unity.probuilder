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
	static pb_Object _pb = null;
	static pb_Object pbi
	{
		get
		{
			if(_pb == null)
			{
				GameObject go = GameObject.Find("_test_subd");
				_pb = go == null ? null : go.GetComponent<pb_Object>();
				if(_pb == null)
				{
					_pb = pb_ShapeGenerator.CubeGenerator(Vector3.one * 4f);
					_pb.Subdivide();
					_pb.Subdivide();
					_pb.Subdivide();
					_pb.Subdivide();
				}
			}
			return _pb;
		}
	}

	[MenuItem("Tools/Temp Menu Item &d")]
	static void MenuInit()
	{
		pb_Object pb = pbi;

		int ITERATIONS = 10;

		profiler.Begin("old");
		for(int i = 0; i < ITERATIONS; ++i)
		{
			profiler.Begin("ToMesh");
			pb.ToMesh();
			profiler.End();
			profiler.Begin("Refresh");
			pb.Refresh();
			profiler.End();

			profiler.Begin("CollapseSharedVertices Old");
			pb_MeshUtility.CollapseSharedVertices(pb);
			profiler.End();
		}
		profiler.End();

		profiler.Begin("new");
		for(int i = 0; i < ITERATIONS; ++i)
		{
			profiler.Begin("ToMesh");
			pb.ToMesh();
			profiler.End();
			profiler.Begin("Refresh");
			pb.Refresh();
			profiler.End();

			profiler.Begin("CollapseSharedVertices New");
			pb_MeshUtility.CollapseSharedVertices(pb.msh);
			profiler.End();
		}
		profiler.End();
	}
}

#pragma warning disable 0219

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;
using ProBuilder2.Math;

using Parabox.Debug;

public class ProfileLooper : Editor
{
	const int REPEAT = 10;
	static pb_Profiler profiler;

	[MenuItem("Tools/Debug/" + pb_Constant.PRODUCT_NAME + "/Run Profiler Loop %&P")]
	public static void RunLoop()
	{
		profiler = new pb_Profiler("Test Profiler");

		const string METHOD_A = "Get Perimeter Edges (Old)";
		const string METHOD_B = "Get Perimeter Edges (New)";

		foreach(pb_Object pb in Selection.transforms.GetComponents<pb_Object>())
		{		
			Dictionary<int, int> lookup = pb.sharedIndices.ToDictionary();

			for(int i = 0; i < REPEAT; i++)	
			{
				profiler.BeginSample(METHOD_A);
				{
					IEnumerable<pb_Edge> e = pbMeshUtils.GetPerimeterEdges(pb, lookup, pb.faces);
				}
				profiler.EndSample();

				profiler.BeginSample(METHOD_B);
				{
				}
				profiler.EndSample();
			}
		}

		Debug.Log( profiler.ToString() );
	}
}

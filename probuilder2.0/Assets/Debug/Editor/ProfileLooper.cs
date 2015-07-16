#if PB_DEBUG
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.Math;

using Parabox.Debug;

public class ProfileLooper : Editor
{
	const int REPEAT = 10;
	static pb_Profiler profiler;

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Debug/Run Loop x1000")]
	public static void RunLoop()
	{
		profiler = new pb_Profiler("Test Profiler");

		pb_Object pb = pb_ShapeGenerator.CylinderGenerator(32, 15f, 20f, 32);
		
		profiler.BeginSample("GetUniversalEdges");
		for(int i = 0; i < REPEAT; i++)
		{
			profiler.BeginSample("IndexOf");
			MethodA(pb);
			profiler.EndSample();
		}

		for(int i = 0; i < REPEAT; i++)
		{
			profiler.BeginSample("Sorted IndexOf");			
			MethodB(pb);
			profiler.EndSample();
		}
		profiler.EndSample();

		GameObject go = pb.gameObject;
		DestroyImmediate(pb);
		DestroyImmediate(go);

		Debug.Log( profiler.ToString() );
	}

	public static void MethodA(pb_Object pb)
	{
		pb_Edge[] edges = pb_Edge.AllEdges(pb.faces);
		int len = edges.Length;

		pb_IntArray[] sharedIndices = pb.sharedIndices;

		pb_Edge[] uniEdges = new pb_Edge[len];
		for(int i = 0; i < len; i++)
			uniEdges[i] = new pb_Edge(sharedIndices.IndexOf(edges[i].x), sharedIndices.IndexOf(edges[i].y));
	}

	public static void MethodB(pb_Object pb)
	{

	}
}
#endif
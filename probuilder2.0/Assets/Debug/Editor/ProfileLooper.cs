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

		List<Vector4> uvs = pbUtil.Fill<Vector4>(Vector4.zero, 6000);
		
		profiler.BeginSample("Vec 2");
		for(int i = 0; i < REPEAT; i++)
		{
			profiler.BeginSample("Alloc Vector2");
			MethodA(uvs);
			profiler.EndSample();
		}

		for(int i = 0; i < REPEAT; i++)
		{
			profiler.BeginSample("Assign Vec2 Component");			
			MethodB(uvs);
			profiler.EndSample();
		}
		profiler.EndSample();

		Debug.Log( profiler.ToString() );
	}

	public static void MethodA(List<Vector4> uvs)
	{
		Vector2 x = Vector2.zero;

		for(int i = 0; i < uvs.Count; i++)
		{
			x = uvs[i];
			if(Vector2.Distance(x, Vector2.one) > 2f) Debug.Log("doesn't matter");
		}
	}

	public static void MethodB(List<Vector4> uvs)
	{
		Vector2 x = Vector2.zero;

		for(int i = 0; i < uvs.Count; i++)
		{
			x.x = uvs[i].x;
			x.y = uvs[i].y;
			if(Vector2.Distance(x, Vector2.one) > 2f) Debug.Log("doesn't matter");
		}
	}
}
#endif

using UE = UnityEngine;
using System.Collections;
using System.Diagnostics;
using System.Threading;
using UnityEditor;
using Parabox.Debug;

/**
 * Used to verify that pb_Profiler is generally accurate.
 */
public class TestProfiler : Editor
{
	const int SLEEP = 300;
	const int ITERATIONS = 25;
	const int MAX_SUM_DELTA = 20;	// If the profiler is off by more than x ms, something's wrong.

	[MenuItem("Window/pb_Profiler Test")]
	public static void Init()
	{
		Stopwatch stopwatch = new Stopwatch();

		stopwatch.Start();

			for(int i = 0; i < ITERATIONS; i ++)
			{
				Thread.Sleep(SLEEP);
			}
		stopwatch.Stop();

		pb_Profiler profiler = new pb_Profiler();
			profiler.BeginSample("Sleep");
				for(int i = 0; i < ITERATIONS; i++)
				{
					profiler.BeginSample("Thread.Sleep");
					Thread.Sleep(SLEEP);
					profiler.EndSample();
				}
			profiler.EndSample();

		profiler.EndSample();

		long s_sum = stopwatch.ElapsedMilliseconds;
		long p_sum = profiler.GetRootSample().sum;

		if( System.Math.Abs(p_sum - s_sum) > MAX_SUM_DELTA )
			UE.Debug.LogError("Stopwatch: " + s_sum + "\nProfiler: " + s_sum);
		else
			UE.Debug.Log("Stopwatch: " + s_sum + "\nProfiler: " + s_sum);
	}

}

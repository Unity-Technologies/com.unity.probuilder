// #define DO_THE_DEBUG_DANCE
// #define PROFILER_EXISTS

using Parabox.Debug;
using System.Diagnostics;

public static class profiler
{
#if !PROFILER_EXISTS
	class pb_Profiler
	{
		public pb_Profiler(string name) {}
		public void BeginSample(string str, int offset = 0) {}
		public void EndSample() {}
	}
#endif

	static pb_Profiler _profiler = new pb_Profiler("Global");

	[Conditional("DO_THE_DEBUG_DANCE")]
	public static void Begin(string str)
	{
		_profiler.BeginSample(str, 1);
	}

	[Conditional("DO_THE_DEBUG_DANCE")]
	public static void End()
	{
		_profiler.EndSample();
	}

	[Conditional("DO_THE_DEBUG_DANCE")]
	public static void BeginSample(string str)
	{
		_profiler.BeginSample(str, 1);
	}

	[Conditional("DO_THE_DEBUG_DANCE")]
	public static void EndSample()
	{
		_profiler.EndSample();
	}

	[Conditional("DO_THE_DEBUG_DANCE")]
	public static void Print()
	{
		UnityEngine.Debug.Log( _profiler.ToString() );
	}
}

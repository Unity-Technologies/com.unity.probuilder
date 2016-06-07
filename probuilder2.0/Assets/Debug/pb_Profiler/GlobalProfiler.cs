using Parabox.Debug;
using System.Diagnostics;

public static class profiler
{
#if DEBUG
	static pb_Profiler _profiler = new pb_Profiler("Global");
#endif

	[Conditional("DEBUG")]
	public static void Begin(string str)
	{
		_profiler.BeginSample(str);
	}

	[Conditional("DEBUG")]
	public static void End()
	{
		_profiler.EndSample();
	}

	[Conditional("DEBUG")]
	public static void Print()
	{
		UnityEngine.Debug.Log( _profiler.ToString() );
	}
}

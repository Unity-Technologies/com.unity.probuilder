using Parabox.Debug;
using System.Diagnostics;

public static class profiler
{
	static pb_Profiler _profiler = new pb_Profiler("Global");

	[Conditional("DEBUG")]
	public static void Begin(string str)
	{
		_profiler.BeginSample(str, 1);
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

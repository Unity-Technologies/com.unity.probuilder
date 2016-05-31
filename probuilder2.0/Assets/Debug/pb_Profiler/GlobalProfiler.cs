using Parabox.Debug;

public static class profiler
{
	static pb_Profiler _profiler = new pb_Profiler("Global");

	public static void Begin(string str)
	{
		_profiler.BeginSample(str);
	}

	public static void End()
	{
		_profiler.EndSample();
	}

	public static void Print()
	{
		UnityEngine.Debug.Log( _profiler.ToString() );
	}
}

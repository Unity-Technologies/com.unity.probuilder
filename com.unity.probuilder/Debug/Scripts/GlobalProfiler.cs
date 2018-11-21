//#define DO_THE_DEBUG_DANCE
//#define PROFILER_EXISTS

#if PROFILER_EXISTS
using Parabox.Debug;
#endif

#pragma warning disable 618

#if !PROFILER_EXISTS
class pb_Profiler
{
    public pb_Profiler(string name) {}
    public void BeginSample(string str, int offset = 0) {}
    public void EndSample() {}
}
#endif

public static class profiler
{
#if !DO_THE_DEBUG_DANCE
    [System.Obsolete("Profiler code exists in non-debug build!")]
#endif
    static pb_Profiler s_Profiler = new pb_Profiler("Global");

#if !DO_THE_DEBUG_DANCE
    [System.Obsolete("Profiler code exists in non-debug build!")]
#endif
    public static void Begin(string str)
    {
        s_Profiler.BeginSample(str, 1);
    }

#if !DO_THE_DEBUG_DANCE
    [System.Obsolete("Profiler code exists in non-debug build!")]
#endif
    public static void End()
    {
        s_Profiler.EndSample();
    }

#if !DO_THE_DEBUG_DANCE
    [System.Obsolete("Profiler code exists in non-debug build!")]
#endif
    public static void BeginSample(string str)
    {
        s_Profiler.BeginSample(str, 1);
    }

#if !DO_THE_DEBUG_DANCE
    [System.Obsolete("Profiler code exists in non-debug build!")]
#endif
    public static void EndSample()
    {
        s_Profiler.EndSample();
    }

#if !DO_THE_DEBUG_DANCE
    [System.Obsolete("Profiler code exists in non-debug build!")]
#endif
    public static void Print()
    {
        UnityEngine.Debug.Log(s_Profiler.ToString());
    }
}
#pragma warning restore 618

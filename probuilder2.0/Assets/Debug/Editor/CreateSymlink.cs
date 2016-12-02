using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Diagnostics;

public class CreateSymlink : Editor
{
	[MenuItem("Assets/Create/Folder Symlink Here")]
	static void DoTheSymlink()
	{
		// mklink /J pb_Profiler D:\github\pb_Profiler\Assets\pb_Profiler

#if UNITY_EDITOR_WIN
		Process.Start("CMD.exe", "echo %cd%");
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
		Debug.Log("ln -s {0} {1}");
#endif
	}
}

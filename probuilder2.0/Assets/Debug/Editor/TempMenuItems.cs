using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;
using System.Diagnostics;

public class TempMenuItems : EditorWindow
{
	[MenuItem("Tools/Temp Menu Item &d")]
	static void MenuInit()
	{
		SaveMeshTemplate(null);
	}

	public static void SaveMeshTemplate(Mesh mesh)
	{
		StackTrace trace = new StackTrace(1, true);
		for (int i = 0; i < trace.FrameCount; i++)
		{
			StackFrame first = trace.GetFrame(i);
			UnityEngine.Debug.Log(first.GetFileName() + ": " + first.GetMethod());
		}
	}
}

using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using Math = UnityEngine.ProBuilder.Math;
using Object = System.Object;
using UObject = UnityEngine.Object;
using UnityEngine.ProBuilder.AssetIdRemapUtility;

class TempMenuItems : EditorWindow
{
	[MenuItem("Tools/Temp Menu Item &d", false, 1000)]
	static void MenuInit()
	{
		foreach(var mesh in MeshSelection.Top())
			foreach(var face in mesh.faces)
				face.InvalidateCache();
	}

	public static void SaveMeshTemplate(Mesh mesh)
	{
//		StackTrace trace = new StackTrace(1, true);
//		for (int i = 0; i < trace.FrameCount; i++)
//		{
//			StackFrame first = trace.GetFrame(i);
//			UnityEngine.Debug.Log(first.GetFileName() + ": " + first.GetMethod());
//		}
	}
}

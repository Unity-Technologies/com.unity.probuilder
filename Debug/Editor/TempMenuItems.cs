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

class TempMenuItems : EditorWindow
{
	[MenuItem("Tools/Temp Menu Item &d")]
	static void MenuInit()
	{
		var mesh = Selection.transforms.GetComponents<ProBuilderMesh>().FirstOrDefault();
		var tangents = mesh.faces.Select(x => Math.NormalTangentBitangent(mesh, x).tangent).ToArray();
		var meshTangents = mesh.mesh.tangents;
		Debug.Log("mesh tangents: \n" + meshTangents.ToString("\n") + "\n\n---\ncalculated tangents:\n" + tangents.ToString("\n"));
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

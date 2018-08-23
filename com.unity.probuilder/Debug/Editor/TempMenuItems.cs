using System;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder;
using UnityEngine.Assertions;
using UnityEngine.ProBuilder;
using UObject = UnityEngine.Object;
using UnityEngine.ProBuilder.AssetIdRemapUtility;
using UnityEngine.ProBuilder.MeshOperations;

class TempMenuItems : EditorWindow
{
	[MenuItem("Tools/Temp Menu Item &d", false, 1000)]
	static void MenuInit()
	{
		var shapes = Enum.GetValues(typeof(ShapeType)) as ShapeType[];
		float x = -10f;
		ProBuilderMesh[] primitives = new ProBuilderMesh[shapes.Length];
		for (int i = 0, c = shapes.Length; i < c; i++)
		{
			primitives[i] = ShapeGenerator.CreateShape(shapes[i]);
			primitives[i].GetComponent<MeshFilter>().sharedMesh.name = shapes[i].ToString();

			primitives[i].transform.position = new Vector3(x, 0f, 0f);
			x += primitives[i].mesh.bounds.size.x + .5f;
		}
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

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
		var mesh = ShapeGenerator.CreateShape(ShapeType.Cube);
		Debug.Log("original -> shared vertexes:\n" + mesh.sharedVertexes.ToString("\n"));
		Debug.Log("original -> shared vertexes lookup:\n" + string.Join("\n", mesh.sharedVertexLookup.OrderBy(x=>x.Key).Select(x=>x.Key +", " +x.Value)) );
		Debug.Log("original -> has lookup cache: " + mesh.hasSharedLookupCache + "\n" + JsonUtility.ToJson(mesh, true));

		UnityEditor.Undo.RegisterCompleteObjectUndo(new [] { mesh }, "Merge Vertexes");

		mesh.MergeVertexes(new int[] { 0, 1 }, true);

		mesh.ToMesh();
		mesh.Refresh();
		Debug.Log("collapsed -> shared vertexes:\n" + mesh.sharedVertexes.ToString("\n"));
		Debug.Log("collapsed -> shared vertexes lookup:\n" + string.Join("\n", mesh.sharedVertexLookup.OrderBy(x=>x.Key).Select(x=>x.Key +", " +x.Value)) );
		Debug.Log("collapsed -> has lookup cache: " + mesh.hasSharedLookupCache + "\n" + JsonUtility.ToJson(mesh, true));

		Undo.PerformUndo();

		mesh.InvalidateCaches();

		mesh.ToMesh();
		mesh.Refresh();

		Debug.Log("undo -> shared vertexes:\n" + mesh.sharedVertexes.ToString("\n"));
		Debug.Log("undo -> shared vertexes lookup:\n" + string.Join("\n", mesh.sharedVertexLookup.OrderBy(x=>x.Key).Select(x=>x.Key +", " +x.Value)) );
		Debug.Log("undo -> has lookup cache: " + mesh.hasSharedLookupCache + "\n" + JsonUtility.ToJson(mesh, true));
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

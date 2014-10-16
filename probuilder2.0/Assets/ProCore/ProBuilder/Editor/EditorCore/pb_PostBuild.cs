using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditor.Callbacks;

public class pb_PostBuild : Editor
{
	// todo - This works in 4.2 +

	// [PostProcessScene]
	// public static void OnPostprocessScene()
	// {
		// // Debug.Log("OnPostprocessScene called - " + EditorApplication.currentScene);

		// System.Reflection.MemberInfo info = typeof(BuildPipeline);
		// // Debug.Log("info: " + info);
		// object[] attributes = info.GetCustomAttributes(true);
		// for (int i = 0; i < attributes.Length; i ++)
		// {
		// 	Debug.Log(attributes[i]);
		// }

		// pb_Object[] pbs = (pb_Object[])GameObject.FindObjectsOfType(typeof(pb_Object));
		// foreach(pb_Object pb in pbs)
		// {
		// 	// Debug.Log("Hiding :  " + pb.id);
		// 	pb.HideNodraw();
		// 	EditorUtility.SetDirty(pb as Object);
		// 	// ProBuilder.CreatePrimitive(ProBuilder.Shape.Cube).transform.position = new Vector3(10f, 16f, 12f);

		// }
	// }
}

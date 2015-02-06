using UnityEngine;
using UnityEditor;
using System.Collections;

public class ShowHideFlags : Editor
{
	[MenuItem("Tools/Show HideFalgs")]
	static void Indot()
	{
		foreach(Transform t in Selection.transforms)
			Debug.Log(t.gameObject.hideFlags);
	}
}

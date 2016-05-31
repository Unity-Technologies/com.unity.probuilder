using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;

public class MeshInfo : Editor
{
	[MenuItem("Tools/Debug/ProBuilder/Print Mesh Info")]
	static void PrintMeshInfo()
	{
		foreach(MeshFilter mf in Selection.transforms.GetComponents<MeshFilter>())
			if(mf.sharedMesh != null)	
				Debug.Log(pb_MeshUtility.Print(mf.sharedMesh));
	}
}

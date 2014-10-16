using UnityEngine;
using UnityEditor;
using System.Collections;

/**
 * Sets all pb_Object internal caches to match the mesh values (vertices, uvs).
 */
public class RebuildVertexCaches : Editor
{
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Repair/Rebuild Element Caches")]
	public static void Rebuild()
	{
		foreach(pb_Object pb in FindObjectsOfType(typeof(pb_Object)))
		{
			Mesh m = pb.msh;
			if(m == null) continue;

			pb.SetVertices(m.vertices);
			pb.SetUV(m.uv);
		}
	}
}

using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;

public class FreezeTransforms : Editor
{

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/Freeze Transforms", false, pb_Constant.MENU_ACTIONS + 1)]
	public static void MenuFreezeTransforms()
	{
		pb_Object[] selection = pbUtil.GetComponents<pb_Object>(Selection.transforms);

		pbUndo.RecordObjects(selection, "Freeze Transforms");

		foreach(pb_Object pb in selection)
		{
			Vector3[] v = pb.VerticesInWorldSpace();

			pb.transform.position = Vector3.zero;
			pb.transform.localRotation = Quaternion.identity;
			pb.transform.localScale = Vector3.one;

			pb.SetVertices(v);
			pb.ToMesh();
			pb.Refresh();
		}
	}
}

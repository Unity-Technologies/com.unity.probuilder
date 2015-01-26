using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;

/**
 * Set the pivot point of a pb_Object mesh to 0,0,0 while retaining current world space.
 */
public class pb_FreezeTransform : Editor
{

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Freeze Transforms", false, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_OBJECT)]
	public static bool MenuVerifyFreezeTransforms()
	{
		return Selection.transforms.GetComponents<pb_Object>().Length > 0;
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Freeze Transforms", false, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_OBJECT)]
	public static void MenuFreezeTransforms()
	{
		pb_Object[] selection = pbUtil.GetComponents<pb_Object>(Selection.transforms);

		pbUndo.RecordObjects(Selection.transforms, "Freeze Transforms");
		pbUndo.RecordObjects(selection, "Freeze Transforms");

		foreach(pb_Object pb in selection)
		{
			pb.ToMesh();

			Vector3[] v = pb.VerticesInWorldSpace();

			pb.transform.position = Vector3.zero;
			pb.transform.localRotation = Quaternion.identity;
			pb.transform.localScale = Vector3.one;

			foreach(pb_Face face in pb.faces)
			{
				face.manualUV = true;
			}

			pb.SetVertices(v);

			pb.Refresh();
			pb.GenerateUV2();
		}
	}
}

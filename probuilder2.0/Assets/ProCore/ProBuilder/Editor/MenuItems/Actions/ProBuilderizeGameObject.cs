using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;

namespace ProBuilder2.Actions
{
public class ProBuilderizeMesh : Editor
{
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/ProBuilderize Selection", true, pb_Constant.MENU_ACTIONS + 1)]
	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/ProBuilderize Selection (Preserve Faces)", true, pb_Constant.MENU_ACTIONS + 2)]
	public static bool VerifyProBuilderize()
	{
		return Selection.transforms.Length - pbUtil.GetComponents<pb_Object>(Selection.transforms).Length > 0;
	}	

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/ProBuilderize Selection", false, pb_Constant.MENU_ACTIONS + 1)]
	public static void MenuProBuilderizeTris()
	{
		ProBuilderizeObjects(false);
	}

	[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Actions/ProBuilderize Selection (Preserve Faces)", false, pb_Constant.MENU_ACTIONS + 2)]
	public static void MenuProBuilderizeQuads()
	{
		ProBuilderizeObjects(true);
	}

	public static void ProBuilderizeObjects(bool preserveFaces)
	{
		foreach(Transform t in Selection.transforms)
		{
			if(t.GetComponent<MeshFilter>())
			{
				pb_Object pb = ProBuilderize(t, preserveFaces);

			}
		}
	}

	public static pb_Object ProBuilderize(Transform t, bool preserveFaces)
	{

		pb_Object pb = pbMeshOps.CreatePbObjectWithTransform(t, preserveFaces);

		pb_Editor_Utility.SetEntityType(EntityType.Detail, pb.gameObject);
		
#if UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5	
		t.gameObject.active = false;
#else
		t.gameObject.SetActive(false);
#endif

		return pb;
	}
}
}
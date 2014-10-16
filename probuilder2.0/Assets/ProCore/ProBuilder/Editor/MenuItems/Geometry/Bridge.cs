#if !PROTOTYPE

#if UNITY_4_3 || UNITY_4_3_0 || UNITY_4_3_1 || UNITY_4_3_2 || UNITY_4_3_3 || UNITY_4_3_4 || UNITY_4_3_5
#define UNITY_4_3
#elif UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
#define UNITY_4
#elif UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#define UNITY_3
#endif

using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.MeshOperations;
using ProBuilder2.Common;

/**
 * This probably doesn't need to exist anymore...
 */
namespace ProBuilder2.Actions
{
	public class Bridge : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Bridge Edges &b", true, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_EDGE)]
		public static bool VerifyBridgeEdges()
		{
			return pb_Editor.instance != null && pb_Editor.instance.selectedEdgeCount == 2;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Bridge Edges &b", false, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_EDGE)]
		public static void BridgeEdges()
		{
			pb_Menu_Commands.MenuBridgeEdges(pbUtil.GetComponents<pb_Object>(Selection.transforms));
		}
	}
}
#endif
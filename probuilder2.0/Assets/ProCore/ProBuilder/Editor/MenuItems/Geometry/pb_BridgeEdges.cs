#if !PROTOTYPE

using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.MeshOperations;
using ProBuilder2.Common;

/**
 * Bridge edges.
 */
namespace ProBuilder2.Actions
{
	public class pb_BridgeEdges : Editor
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
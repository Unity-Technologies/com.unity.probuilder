using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.MeshOperations;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

namespace ProBuilder2.Actions
{
	/**
	 * Menu interface for flip edges actions.
	 */
	public class pb_FlipEdge : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Flip Face Edge", true,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_FACE + 2)]
		public static bool VerifyFlipEdges()
		{
			return pb_Editor.instance != null && pb_Editor.instance.selectedFaceCount > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Flip Face Edge", false,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_FACE + 2)]
		public static void FlipEdges()
		{
			pb_Menu_Commands.MenuFlipEdges(pbUtil.GetComponents<pb_Object>(Selection.transforms));

			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
		}
	}
}

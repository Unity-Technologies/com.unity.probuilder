using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.MeshOperations;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

namespace ProBuilder2.Actions
{
	/**
	 * Menu interface for deleting and detaching faces.
	 */
	public class pb_DetachDeleteFace : Editor
	{
#if !PROTOTYPE

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Detach Face Selection", true, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_FACE + 4)]
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Delete Face (Backspace)", true, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_FACE + 5)]
		public static bool VerifyFaceAction()
		{
			return pb_Editor.instance != null && pb_Editor.instance.selectedFaceCount > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Detach Face Selection", false, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_FACE + 4)]
		public static void MenuDetachFace()
		{
			pb_Menu_Commands.MenuDetachFaces(pbUtil.GetComponents<pb_Object>(Selection.transforms));
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Delete Face (Backspace)", false, pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_FACE + 5)]
		public static void MenuDeleteFace()
		{
			pb_Menu_Commands.MenuDeleteFace(pbUtil.GetComponents<pb_Object>(Selection.transforms));
		}
#endif
	}
}

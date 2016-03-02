using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;

namespace ProBuilder2.Actions
{
	/**
	 * Menu interface for 'Conform Normals' action.
	 */
	public class pb_ConformNormals : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Conform Normals", true,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_FACE + 2)]
		public static bool MenuVerifyConformNormals()
		{
			return pb_Editor.instance != null && pb_Editor.instance.selection.Length > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Conform Normals", false,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_FACE + 2)]
		public static void MenuConformNormals()
		{
			pb_Object[] selection = pbUtil.GetComponents<pb_Object>(Selection.transforms);

			if(pb_Editor.instance != null)
			{
				if(pb_Editor.instance.selectedFaceCount > 0)
					pb_Menu_Commands.MenuConformNormals(selection);
				else
					pb_Menu_Commands.MenuConformObjectNormals(selection);
			}
		}
	}
}

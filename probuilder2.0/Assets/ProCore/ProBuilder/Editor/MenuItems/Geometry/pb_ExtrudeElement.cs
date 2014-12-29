using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;

namespace ProBuilder2.Actions
{
	public class pb_ExtrudeElement : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Extrude with Translation %e", true,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_FACE + 1)]
		public static bool VerifyExtrudeFace()
		{
			return pb_Editor.instance != null && (pb_Editor.instance.selectedEdgeCount > 0 || pb_Editor.instance.selectedFaceCount > 0);
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Geometry/Extrude with Translation %e", false,  pb_Constant.MENU_GEOMETRY + pb_Constant.MENU_GEOMETRY_FACE + 1)]
		public static void Extrude()
		{
			pb_Menu_Commands.MenuExtrude(pbUtil.GetComponents<pb_Object>(Selection.transforms));
			// PerformExtrusion( pb_Preferences_Internal.GetFloat(pb_Constant.pbExtrudeDistance) );
			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
		}
	}
}

#if UNITY_4_3 || UNITY_4_3_0 || UNITY_4_3_1
#define UNITY_4_3
#elif UNITY_4_0 || UNITY_4_0_1 || UNITY_4_1 || UNITY_4_2
#define UNITY_4
#elif UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#define UNITY_3
#endif

using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Common;
using ProBuilder2.MeshOperations;

namespace ProBuilder2.Actions
{
	public class ExtrudeFace : Editor
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

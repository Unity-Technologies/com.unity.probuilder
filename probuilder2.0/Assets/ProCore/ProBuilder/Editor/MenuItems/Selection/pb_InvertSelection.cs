// Thanks to forum member @Igmon for this feature suggestion:
// http://www.sixbysevenstudio.com/forum/viewtopic.php?f=14&t=2374&p=4351#p4351

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

namespace ProBuilder2.Actions
{
	/**
	 * Menu interface for inverting the current element selection.
	 */
	public class pb_InvertSelection : Editor
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Invert Face Selection %#i", true, pb_Constant.MENU_SELECTION + 0)]
		public static bool VerifySelectionAction()
		{
			return pb_Editor.instance && pb_Editor.instance.selectedVertexCount > 0;
		}

		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Selection/Invert Face Selection %#i", false, pb_Constant.MENU_SELECTION + 0)]
		public static void InvertFaceSelection()
		{
			pb_Menu_Commands.MenuInvertSelection( pbUtil.GetComponents<pb_Object>(Selection.transforms) );
			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
		}
	}
}

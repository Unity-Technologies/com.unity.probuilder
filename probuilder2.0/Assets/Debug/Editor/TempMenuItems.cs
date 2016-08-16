using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;
using System.Linq;
using System.Text;
using System.Reflection;

using Parabox.Debug;

public class TempMenuItems : EditorWindow
{
	[MenuItem("Tools/Temp Menu Item &d")]
	static void MenuInit()
	{
		// pb_Object[] selection = Selection.transforms.GetComponents<pb_Object>();

		// foreach(pb_Object pb in selection)
		// {
		// 	// pb_Menu_Commands.MenuGrowSelection(selection);
			
			// MenuGrowSelection(selection);
		// }

		pb_Editor.Refresh();
	}
}

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

public class TempMenuItems : EditorWindow
{
	[MenuItem("Tools/Temp Menu Item &d")]
	static void MenuInit()
	{
//		Debug.Log(PackageImporter.IsPreUpmProBuilderInProject().ToString());
	}
}

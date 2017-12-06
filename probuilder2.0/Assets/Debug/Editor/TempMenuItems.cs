using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Linq;

using ProBuilder.AssetUtility;

public class TempMenuItems : EditorWindow
{
	[MenuItem("Tools/Temp Menu Item &d")]
	static void MenuInit()
	{
		Debug.Log(PackageImporter.IsPreUpmProBuilderInProject().ToString());
	}
}

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System;
using System.Reflection;
using ProBuilder.Core;
using ProBuilder.MeshOperations;
using ProBuilder.EditorCore;

public class TempMenuItems : EditorWindow
{
	[MenuItem("Tools/Temp Menu Item &d")]
	static void MenuInit()
	{
		Debug.Log(EditorPrefs.GetFloat("MoveSnapX"));
	}
}
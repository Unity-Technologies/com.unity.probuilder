using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.MeshOperations;
using System.Linq;
using System.Text;
using System;
using System.Reflection;

using Parabox.Debug;

public class TempMenuItems : EditorWindow
{
	[MenuItem("Tools/Temp Menu Item &d")]
	static void MenuInit()
	{
		// EditorWindow.GetWindow<TempMenuItems>();

		pb_Object[] selection = Selection.transforms.GetComponents<pb_Object>();

		profiler.Begin("depth");

		Dictionary<Color32, pb_Tuple<pb_Object, pb_Face>> map;
		Texture2D tex = pb_HandleUtility.RenderSelectionPickerTexture(SceneView.lastActiveSceneView.camera, selection, out map);
		
		profiler.End();

		pb_EditorUtility.SaveTexture(tex, "Assets/test.png");

		GameObject.DestroyImmediate(tex);
	}

}

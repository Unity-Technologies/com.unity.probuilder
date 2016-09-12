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
		EditorWindow.GetWindow<TempMenuItems>();

		// pb_Object[] selection = Selection.transforms.GetComponents<pb_Object>();

		// profiler.Begin("depth");

		// Dictionary<Color32, pb_Tuple<pb_Object, pb_Face>> map;
		// Texture2D tex = pb_HandleUtility.RenderSelectionPickerTexture(SceneView.lastActiveSceneView.camera, selection, out map);
		
		// profiler.End();

		// pb_EditorUtility.SaveTexture(tex, "Assets/test.png");

		// GameObject.DestroyImmediate(tex);
	}

	Color32 color = new Color32(255, 0, 0, 255);

	void OnGUI()
	{
		color = EditorGUILayout.ColorField("color", color);

		int c = color.r << 16 | color.g << 8 | color.b;

		GUILayout.Label("color : " + color);
		GUILayout.Label("packed: " + c);

		Color32 u = new Color32(
			(byte) ((c >> 16) & 0xFF),
			(byte) ((c >>  8) & 0xFF),
			(byte) ((c      ) & 0xFF),
			(byte) (255)
			);
		GUILayout.Label("unpacked: " + u);
	}

}

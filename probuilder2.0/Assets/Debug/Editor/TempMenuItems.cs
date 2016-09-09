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
		Texture2D tex = pb_Handle_Utility.RenderSelectionPickerTexture(SceneView.lastActiveSceneView.camera, selection, out map);
		
		profiler.End();

		pb_EditorUtility.SaveTexture(tex, "Assets/test.png");

		GameObject.DestroyImmediate(tex);
	}

	// Color32 color = new Color32(255, 0, 0, 1);
	// GUIStyle labelStyle = null;

	// void OnGUI()
	// {
	// 	if(labelStyle == null)
	// 	{
	// 		Font font = Resources.Load<Font>("monkey");
	// 		labelStyle = new GUIStyle(EditorStyles.label);
	// 		labelStyle.font = font;
	// 	}

	// 	color = EditorGUILayout.ColorField("color", color);

	// 	GUILayout.Label( string.Format("rgba 32:  {0:x}, {1:x}, {2:x}, {3:x}", color.r, color.g, color.b, color.a), labelStyle);

	// 	uint hash = DecodeRGBA(color);
	// 	Color32 encoded = EncodeRGBA(hash);

	// 	GUILayout.Label( string.Format("uint   :  {0:x}", hash), labelStyle);
	// 	GUILayout.Label( string.Format("rgba 32:  {0:x}, {1:x}, {2:x}, {3:x}", encoded.r, encoded.g, encoded.b, encoded.a), labelStyle);
	// 	uint hash2 = DecodeRGBA(encoded);
	// 	GUILayout.Label( string.Format("back   :  {0:x}", hash2), labelStyle);
	// }

}

using UnityEngine;
using System.Collections;
using UnityEditor;
using ProBuilder2.Common;

public class colorwheel : EditorWindow {

	[MenuItem("Tools/COLOR WHEEL")]
	public static void intdtas()
	{
		EditorWindow.GetWindow<colorwheel>().Show();
	}

	Color col = new Color(0f, .7f, 1f, 1f);

	void OnGUI()
	{
		col = EditorGUILayout.ColorField("Color", col);

		int[] icol = new int[]
		{
			(int)(col.r * 255f),
			(int)(col.g * 255f),
			(int)(col.b * 255f),
			(int)(col.a * 255f)
		};
		
		pb_HsvColor hsv = pb_HsvColor.FromRGB(col);
		pb_XYZ_Color xyz = pb_XYZ_Color.FromRGB(col);
		pb_CIE_Lab_Color lab = pb_CIE_Lab_Color.FromXYZ(xyz);

		GUILayout.Label(string.Format("Color (RGB): ({0}, {1}, {2}, {3})", icol[0], icol[1], icol[2], icol[3]));
		GUILayout.Label("Color (HSV): " + hsv.ToString());
		GUILayout.Label("Color (XYZ): " + xyz.ToString());
		GUILayout.Label("Color (LAB): " + lab.ToString());
		GUILayout.Label("Name: " + pb_ColorUtil.GetColorName(col));
	}

}

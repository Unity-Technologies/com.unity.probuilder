using UnityEngine;
using UnityEditor;
using System.Collections;

/**
 * Generic GUI utility methods used in ProBuilder windows.
 */
namespace ProBuilder2.GUI
{
	public class pb_GUI_Utility
	{
#region Private

		private static GUIStyle _splitStyle;
		private static GUIStyle SplitStyle
		{
			get
			{
				if(_splitStyle == null)
				{
					_splitStyle = new GUIStyle();
					_splitStyle.normal.background = EditorGUIUtility.whiteTexture;
					_splitStyle.margin = new RectOffset(6,6,0,0);
				}
				return _splitStyle;
			}
		}
#endregion

		/**
		 * Draws a horizontal line and inserts a GUILayout.Space(2).
		 * @param lines How many lines to draw. Typically 1 or 2 suffice.
		 */
		public static void DrawSeparator(int lines, Color color)
		{
			Color old = UnityEngine.GUI.backgroundColor;
			UnityEngine.GUI.backgroundColor = color;
			DrawSeparator(lines);
			UnityEngine.GUI.backgroundColor = old;
		}
		 
		public static void DrawSeparator(int lines)
		{
			GUILayout.Box("", SplitStyle, GUILayout.MaxHeight(2));
			
			for(int i = 1; i < lines; i++)
			{
				GUILayout.Space(2);
				GUILayout.Box("", SplitStyle, GUILayout.MaxHeight(2));
			}
		}

		/**
		 * Draw a solid color block at rect.
		 */
		public static void DrawSolidColor(Rect rect, Color col)
		{
			Color old = UnityEngine.GUI.backgroundColor;
			UnityEngine.GUI.backgroundColor = col;

			UnityEngine.GUI.Box(rect, "", SplitStyle);

			UnityEngine.GUI.backgroundColor = old;
		}

		const int FieldBoxWidth = 64;

		public static float FloatFieldConstrained(GUIContent content, float value, int width)
		{
			GUILayout.BeginHorizontal();
				GUILayout.Label(content, GUILayout.MaxWidth(width-FieldBoxWidth));
				value = EditorGUILayout.FloatField("", value, GUILayout.MaxWidth( FieldBoxWidth-4 ));
			GUILayout.EndHorizontal();

			return value;
		}

		public static int IntFieldConstrained(GUIContent content, int value, int width)
		{
			GUILayout.BeginHorizontal();
				GUILayout.Label(content, GUILayout.MaxWidth(width-FieldBoxWidth));
				value = EditorGUILayout.IntField("", value, GUILayout.MaxWidth( FieldBoxWidth-4 ));
			GUILayout.EndHorizontal();

			return value;
		}
	}
}
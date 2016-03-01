using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.EditorCommon;

/**
 * Generic GUI utility methods used in ProBuilder windows.
 */
namespace ProBuilder2.Interface
{
	public class pb_GUI_Utility
	{
#region Private

		static readonly Color TOOL_SETTINGS_COLOR = EditorGUIUtility.isProSkin ? Color.green : new Color(.2f, .2f, .2f, .2f);

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

		static GUIContent _guiContent = null;

		public static GUIContent TempGUIContent(string label, Texture2D icon = null)
		{
			if(_guiContent == null)
				_guiContent = new GUIContent();

			_guiContent.text = label;
			_guiContent.image = icon;

			return _guiContent;
		}

		private static Stack<bool> guiEnabled = new Stack<bool>();
		private static Stack<Color> guiColor = new Stack<Color>();

		public static void PushGUIEnabled(bool enabled)
		{
			guiEnabled.Push(GUI.enabled);
			GUI.enabled = enabled;
		}

		public static void PopGUIEnabled()
		{
			GUI.enabled = guiEnabled.Pop();
		}

		public static void PushGUIContentColor(Color color)
		{
			guiColor.Push(GUI.color);
			GUI.contentColor = color;
		}

		public static void PopGUIContentColor()
		{
			GUI.contentColor = guiColor.Pop();
		}

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

		static GUIContent slider_guicontent = new GUIContent("", "");
		public static float FreeSlider(string content, float value, float min, float max)
		{
			slider_guicontent.text = content;
			return FreeSlider(slider_guicontent, value, min, max);
		}

		/**
		 * Similar to EditorGUILayoutUtility.Slider, except this allows for values outside of the min/max bounds via the float field.
		 */
		public static float FreeSlider(GUIContent content, float value, float min, float max)
		{
			const float PAD = 4f;
			const float SLIDER_HEIGHT = 16f;
			const float MIN_LABEL_WIDTH = 0f;
			const float MAX_LABEL_WIDTH = 128f;
			const float MIN_FIELD_WIDTH = 48f;

			GUILayoutUtility.GetRect(Screen.width, 18);

			Rect previousRect = GUILayoutUtility.GetLastRect();
			float y = previousRect.y;

			float labelWidth = content != null ? Mathf.Max(MIN_LABEL_WIDTH, Mathf.Min(GUI.skin.label.CalcSize(content).x + PAD, MAX_LABEL_WIDTH)) : 0f;
			float remaining = (Screen.width - (PAD * 2f)) - labelWidth;
			float sliderWidth = remaining - (MIN_FIELD_WIDTH + PAD);
			float floatWidth = MIN_FIELD_WIDTH;

			Rect labelRect = new Rect(PAD, y + 2f, labelWidth, SLIDER_HEIGHT);
			Rect sliderRect = new Rect(labelRect.x + labelWidth, y + 1f, sliderWidth, SLIDER_HEIGHT);
			Rect floatRect = new Rect(sliderRect.x + sliderRect.width + PAD, y + 1f, floatWidth, SLIDER_HEIGHT);

			if(content != null)
				GUI.Label(labelRect, content);

			EditorGUI.BeginChangeCheck();

				int controlID = GUIUtility.GetControlID(FocusType.Native, sliderRect);
				float tmp = value;
				tmp = GUI.Slider(sliderRect, tmp, 0f, min, max, GUI.skin.horizontalSlider, (!EditorGUI.showMixedValue) ? GUI.skin.horizontalSliderThumb : "SliderMixed", true, controlID);

			if(EditorGUI.EndChangeCheck())
				value = Event.current.control ? 1f * Mathf.Round(tmp / 1f) : tmp;

			value = EditorGUI.FloatField(floatRect, value);

			return value;
		}
	
		public static bool ToolSettingsGUI(	string text,
											string description,
											bool showSettings,
											System.Func<pb_Object[], pb_ActionResult> action,
											System.Action<int> gui,
											int guiWidth,
											int guiHeight,
											pb_Object[] selection)
		{
			return ToolSettingsGUI(text, description, showSettings, action, gui, true, guiWidth, guiHeight , selection);
		}

		public static bool ToolSettingsGUI(	string text,
											string description,
											bool showSettings,
											System.Func<pb_Object[], pb_ActionResult> action,
											System.Action<int> gui,
											bool enabled,
											int guiWidth,
											int guiHeight,
											pb_Object[] selection)
		{
			if(enabled)
			{
				GUILayout.BeginHorizontal();

				if(GUILayout.Button(new GUIContent(text, description), EditorStyles.miniButtonLeft, GUILayout.MaxWidth(guiWidth-24-6)))
					action(selection);

				if(GUILayout.Button( showSettings ? "-" : "+", EditorStyles.miniButtonRight, GUILayout.MaxWidth(24)))
					showSettings = !showSettings;
				GUILayout.EndHorizontal();

				if(showSettings)
				{
					UnityEngine.GUI.backgroundColor = TOOL_SETTINGS_COLOR;
					Rect al = GUILayoutUtility.GetLastRect();
					UnityEngine.GUI.Box( new Rect(al.x, al.y + al.height + 2, al.width, guiHeight), "");
					UnityEngine.GUI.backgroundColor = Color.white;

					gui(guiWidth);
					GUILayout.Space(4);
				}
			}
			else
			{
				if(GUILayout.Button(new GUIContent(text, description), EditorStyles.miniButton))
					action(selection);
			}

			return showSettings;
		}

		/**
		 * Similar to EditorGUILayout.Slider, except that this won't return clamped values
		 * unless the user modifies the value.
		 */
		public static float Slider(GUIContent content, float value, float min, float max)
		{
			float tmp = value;

			EditorGUI.BeginChangeCheck();

			tmp = EditorGUILayout.Slider(content, value, min, max);

			if( EditorGUI.EndChangeCheck() )
				return tmp;
			else
				return value;
		}
	}

}

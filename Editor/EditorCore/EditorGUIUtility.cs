using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.UI
{
    /// <summary>
    /// Generic GUI utility methods used in ProBuilder windows.
    /// </summary>
    static class EditorGUIUtility
    {
        internal static class Styles
        {
            public static GUIStyle command = "command";
            public static GUIContent[] selectModeIcons;

            static Texture2D s_ObjectIcon;
            public static Texture2D ObjectIcon => s_ObjectIcon ??= IconUtility.GetIcon("Modes/Mode_Object");

            static Texture2D s_VertexIcon;
            public static Texture2D VertexIcon => s_VertexIcon ??= IconUtility.GetIcon("Modes/Mode_Vertex");

            static Texture2D s_EdgeIcon;
            public static Texture2D EdgeIcon => s_EdgeIcon ??= IconUtility.GetIcon("Modes/Mode_Edge");

            static Texture2D s_FaceIcon;
            public static Texture2D FaceIcon => s_FaceIcon ??= IconUtility.GetIcon("Modes/Mode_Face");

            public static void Init()
            {
                selectModeIcons = new GUIContent[]
                {
                    ObjectIcon != null
                    ? new GUIContent(s_ObjectIcon, "Object Selection")
                    : new GUIContent("OBJ", "Object Selection"),
                    VertexIcon != null
                    ? new GUIContent(s_VertexIcon, "Vertex Selection")
                    : new GUIContent("VRT", "Vertex Selection"),
                    EdgeIcon != null
                    ? new GUIContent(s_EdgeIcon, "Edge Selection")
                    : new GUIContent("EDG", "Edge Selection"),
                    FaceIcon != null
                    ? new GUIContent(s_FaceIcon, "Face Selection")
                    : new GUIContent("FCE", "Face Selection"),
                };
            }
        }

        static readonly Color TOOL_SETTINGS_COLOR = UnityEditor.EditorGUIUtility.isProSkin
            ? Color.green
            : new Color(.2f, .2f, .2f, .2f);

        static GUIStyle _splitStyle;
        static GUIStyle SplitStyle
        {
            get
            {
                if (_splitStyle == null)
                {
                    _splitStyle = new GUIStyle();
                    _splitStyle.normal.background = UnityEditor.EditorGUIUtility.whiteTexture;
                    _splitStyle.margin = new RectOffset(6, 6, 0, 0);
                }
                return _splitStyle;
            }
        }

        static GUIStyle _centeredGreyMiniLabel;
        public static GUIStyle CenteredGreyMiniLabel
        {
            get
            {
                if (_centeredGreyMiniLabel == null)
                {
                    _centeredGreyMiniLabel = new GUIStyle(UnityEditor.EditorStyles.label);
                    _centeredGreyMiniLabel.normal.textColor = Color.gray;
                    _centeredGreyMiniLabel.alignment = TextAnchor.MiddleCenter;
                }
                return _centeredGreyMiniLabel;
            }
        }

        static GUIStyle _solidBackgroundStyle;
        public static GUIStyle solidBackgroundStyle
        {
            get
            {
                if (_solidBackgroundStyle == null)
                {
                    _solidBackgroundStyle = new GUIStyle();
                    _solidBackgroundStyle.normal.background = UnityEditor.EditorGUIUtility.whiteTexture;
                }
                return _solidBackgroundStyle;
            }
        }

        static GUIStyle _buttonNoBackgroundSmallMarginStyle = null;
        public static GUIStyle ButtonNoBackgroundSmallMarginStyle
        {
            get
            {
                if (_buttonNoBackgroundSmallMarginStyle == null)
                {
                    _buttonNoBackgroundSmallMarginStyle = new GUIStyle();
                    _buttonNoBackgroundSmallMarginStyle.margin = new RectOffset(0, 0, 0, 0);
                    _buttonNoBackgroundSmallMarginStyle.alignment = TextAnchor.MiddleCenter;
                    _buttonNoBackgroundSmallMarginStyle.padding = new RectOffset(2, 2, 2, 2);
                }
                return _buttonNoBackgroundSmallMarginStyle;
            }
        }

        static GUIContent _guiContent = null;

        public static GUIContent TempContent(string label, string tooltip = null, Texture2D icon = null)
        {
            if (_guiContent == null)
                _guiContent = new GUIContent();

            _guiContent.text = label;
            _guiContent.tooltip = tooltip;
            _guiContent.image = icon;

            return _guiContent;
        }

        static Stack<bool> s_GuiEnabled = new Stack<bool>();
        static Stack<Color> s_ContentColor = new Stack<Color>();
        static Stack<Color> s_BackgroundColor = new Stack<Color>();

        public static void PushGUIEnabled(bool enabled)
        {
            s_GuiEnabled.Push(GUI.enabled);
            GUI.enabled = enabled;
        }

        public static void PopGUIEnabled()
        {
            GUI.enabled = s_GuiEnabled.Pop();
        }

        public static void PushGUIContentColor(Color color)
        {
            s_ContentColor.Push(GUI.color);
            GUI.contentColor = color;
        }

        public static void PopGUIContentColor()
        {
            GUI.contentColor = s_ContentColor.Pop();
        }

        public static void PushBackgroundColor(Color color)
        {
            s_BackgroundColor.Push(GUI.backgroundColor);
            GUI.backgroundColor = color;
        }

        public static void PopBackgroundColor()
        {
            GUI.backgroundColor = s_BackgroundColor.Pop();
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

            for (int i = 1; i < lines; i++)
            {
                GUILayout.Space(2);
                GUILayout.Box("", SplitStyle, GUILayout.MaxHeight(2));
            }
        }

        static Dictionary<GUIStyle, GUIStyle> onStyles = new Dictionary<GUIStyle, GUIStyle>();

        public static GUIStyle GetOnStyle(GUIStyle style)
        {
            GUIStyle on;

            if (onStyles.TryGetValue(style, out on))
                return on;

            on = new GUIStyle(style);
            on.normal.textColor = on.onNormal.textColor;
            on.normal.background = on.onNormal.background;
            onStyles.Add(style, on);
            return on;
        }

        static Dictionary<GUIStyle, GUIStyle> activeStyles = new Dictionary<GUIStyle, GUIStyle>();

        public static GUIStyle GetActiveStyle(GUIStyle style)
        {
            GUIStyle activeStyle;

            if (activeStyles.TryGetValue(style, out activeStyle))
                return activeStyle;

            activeStyle = new GUIStyle(style);
            activeStyle.normal.textColor = activeStyle.active.textColor;
            activeStyle.normal.background = activeStyle.active.background;
            activeStyles.Add(style, activeStyle);
            return activeStyle;
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
            GUILayout.Label(content, GUILayout.MaxWidth(width - FieldBoxWidth));
            value = UnityEditor.EditorGUILayout.FloatField("", value, GUILayout.MaxWidth(FieldBoxWidth - 4));
            GUILayout.EndHorizontal();

            return value;
        }

        public static int IntFieldConstrained(GUIContent content, int value, int width)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(content, GUILayout.MaxWidth(width - FieldBoxWidth));
            value = UnityEditor.EditorGUILayout.IntField("", value, GUILayout.MaxWidth(FieldBoxWidth - 4));
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
            float pixelsPerPoint = 1f;

            pixelsPerPoint = UnityEditor.EditorGUIUtility.pixelsPerPoint;

            float PAD = 8f / pixelsPerPoint;
            float sliderHeight = UnityEditor.EditorGUIUtility.singleLineHeight;
            const float MIN_LABEL_WIDTH = 0f;
            const float MAX_LABEL_WIDTH = 128f;
            const float MIN_FIELD_WIDTH = 48f;

            Rect rect = EditorGUILayout.GetSliderRect(true);
            float y = rect.y;

            float labelWidth = content != null ? Mathf.Max(MIN_LABEL_WIDTH, Mathf.Min(GUI.skin.label.CalcSize(content).x + PAD, MAX_LABEL_WIDTH)) : 0f;
            float remaining = ((Screen.width / pixelsPerPoint) - (PAD * 2f)) - labelWidth;
            float sliderWidth = remaining - (MIN_FIELD_WIDTH + PAD);
            float floatWidth = MIN_FIELD_WIDTH;

            Rect labelRect = new Rect(PAD, y + 2f, labelWidth, sliderHeight);
            Rect sliderRect = new Rect(labelRect.x + labelWidth, y + 1f, sliderWidth, sliderHeight);
            Rect floatRect = new Rect(sliderRect.x + sliderRect.width + PAD, y + 1f, floatWidth, sliderHeight);

            if (content != null)
                GUI.Label(labelRect, content);

            EditorGUI.BeginChangeCheck();

            int controlID = GUIUtility.GetControlID(FocusType.Passive, sliderRect);
            float tmp = value;
            tmp = GUI.Slider(sliderRect, tmp, 0f, min, max, GUI.skin.horizontalSlider, (!EditorGUI.showMixedValue) ? GUI.skin.horizontalSliderThumb : "SliderMixed", true, controlID);

            if (EditorGUI.EndChangeCheck())
                value = Event.current.control ? 1f * Mathf.Round(tmp / 1f) : tmp;

            value = EditorGUI.FloatField(floatRect, value);

            return value;
        }

        public static int FreeSliderWithRange(string content, int value, int min, int max, ref int uiMin, ref int uiMax, ref bool expanded)
        {
            slider_guicontent.text = content;
            return FreeSliderWithRange(slider_guicontent, value, min, max, ref uiMin, ref uiMax, ref expanded);
        }

        /**
         * Similar to EditorGUILayoutUtility.Slider, except this allows for values outside of the uiMin/uiMax bounds via the int field.
         * Contrary to the FreeSlider however it has a hard range defined with min and max.
         */
        public static int FreeSliderWithRange(GUIContent content, int value, int min, int max, ref int uiMin, ref int uiMax, ref bool expanded)
        {
            float pixelsPerPoint = 1f;

            pixelsPerPoint = UnityEditor.EditorGUIUtility.pixelsPerPoint;

            float PAD = 6f / pixelsPerPoint;
            const float SLIDER_HEIGHT = 16f;
            const float MIN_LABEL_WIDTH = 0f;
            const float MAX_LABEL_WIDTH = 128f;
            const float MIN_FIELD_WIDTH = 48f;

            GUILayoutUtility.GetRect(UnityEditor.EditorGUIUtility.currentViewWidth / pixelsPerPoint, 18);

            Rect previousRect = GUILayoutUtility.GetLastRect();
            float y = previousRect.y;

            float labelWidth = content != null ? Mathf.Max(MIN_LABEL_WIDTH, Mathf.Min(GUI.skin.label.CalcSize(content).x + PAD, MAX_LABEL_WIDTH)) : 0f;
            float remaining = ((Screen.width / pixelsPerPoint) - (PAD * 4f)) - labelWidth;
            float sliderWidth = remaining - (MIN_FIELD_WIDTH + PAD);
            float intWidth = MIN_FIELD_WIDTH;
            float indentOffset = EditorGUI.indentLevel * 15f;

            Rect labelRect = new Rect(PAD, y + 2f, labelWidth, SLIDER_HEIGHT);
            Rect sliderRect = new Rect(labelRect.x + labelWidth + 2*PAD, y + 1f, sliderWidth, SLIDER_HEIGHT);
            Rect intRect = new Rect(sliderRect.x + sliderRect.width + PAD, y + 1f, intWidth, SLIDER_HEIGHT);

            Rect totalRect = GUILayoutUtility.GetRect(1, UnityEditor.EditorGUIUtility.singleLineHeight);
            Rect foldoutRect = new Rect(labelRect.xMax - PAD, labelRect.y, 15, totalRect.height);

            if (content != null)
                GUI.Label(labelRect, content);

            EditorGUI.BeginChangeCheck();

            int controlID = GUIUtility.GetControlID(FocusType.Passive, sliderRect);
            float tmp = value;
            float tmpUIMin = uiMin;
            float tmpUIMax = uiMax;
            tmp = GUI.Slider(sliderRect, tmp, 0f, tmpUIMin, tmpUIMax, GUI.skin.horizontalSlider, (!EditorGUI.showMixedValue) ? GUI.skin.horizontalSliderThumb : "SliderMixed", true, controlID);

            if (EditorGUI.EndChangeCheck())
                value = (int) (Event.current.control ? 1 * Mathf.Round(tmp / 1f) :  tmp);

            value = EditorGUI.DelayedIntField(intRect, value);

            if (value > uiMax)
                uiMax = value;

            if (value < uiMin)
                uiMin = value;

            expanded = EditorGUI.Foldout(foldoutRect, expanded, GUIContent.none);
            if (expanded)
            {
                Rect rangeLabelRect = new Rect(sliderRect.x, sliderRect.yMax, sliderRect.width / 2, intRect.height);
                Rect minRect = new Rect(intRect.x - (intRect.width + indentOffset + PAD), sliderRect.yMax + 2f, intRect.width, intRect.height);
                Rect maxRect = new Rect(intRect.x, sliderRect.yMax + 2f, intRect.width, intRect.height);

                EditorGUI.PrefixLabel(rangeLabelRect, new GUIContent("Range:"));
                uiMin = UnityEditor.EditorGUI.DelayedIntField(minRect, uiMin);
                uiMin = UnityEngine.ProBuilder.Math.Clamp(uiMin, min, uiMax);
                if (value < uiMin)
                    value = uiMin;
                uiMax = UnityEditor.EditorGUI.DelayedIntField(maxRect, uiMax);
                uiMax = UnityEngine.ProBuilder.Math.Clamp(uiMax, uiMin + 1, max);
                if (value > uiMax)
                    value = uiMax;
            }

            return UnityEngine.ProBuilder.Math.Clamp(value, min, max);
        }

        public static bool ToolSettingsGUI(string text,
            string description,
            bool showSettings,
            Func<ProBuilderMesh[], ActionResult> action,
            Action gui,
            ProBuilderMesh[] selection)
        {
            return ToolSettingsGUI(text, description, showSettings, action, gui, true, selection);
        }

        public static bool ToolSettingsGUI(string text,
            string description,
            bool showSettings,
            Func<ProBuilderMesh[], ActionResult> action,
            Action gui,
            bool enabled,
            ProBuilderMesh[] selection)
        {
            if (enabled)
            {
                GUILayout.BeginHorizontal();

                if (GUILayout.Button(new GUIContent(text, description), UnityEditor.EditorStyles.miniButtonLeft))
                    action(selection);

                if (GUILayout.Button(showSettings ? "-" : "+", UnityEditor.EditorStyles.miniButtonRight, GUILayout.MaxWidth(24)))
                    showSettings = !showSettings;
                GUILayout.EndHorizontal();

                if (showSettings)
                {
                    GUILayout.BeginVertical(EditorStyles.sceneTextBox);
                    gui();
                    GUILayout.EndVertical();
                    GUILayout.Space(4);
                }
            }
            else
            {
                if (GUILayout.Button(new GUIContent(text, description), UnityEditor.EditorStyles.miniButton))
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

            tmp = UnityEditor.EditorGUILayout.Slider(content, value, min, max);

            if (EditorGUI.EndChangeCheck())
                return tmp;
            else
                return value;
        }

        static Rect sceneLabelRect = new Rect(0f, 0f, 0f, 0f);
        static Color SceneLabelBackgroundColor = new Color(.12f, .12f, .12f, 1f);

        static GUIStyle sceneBoldLabel
        {
            get
            {
                if (_sceneBoldLabel == null)
                {
                    _sceneBoldLabel = new GUIStyle(UnityEditor.EditorStyles.boldLabel);
                    _sceneBoldLabel.normal.textColor = Color.white;
                }
                return _sceneBoldLabel;
            }
        }

        static GUIStyle _sceneBoldLabel = null;

        /**
         *  Draw a label in the scene view with a solid color background.
         */
        public static void SceneLabel(string text, Vector2 position, bool center = true)
        {
            GUIContent gc = TempContent(text);

            float width = UnityEditor.EditorStyles.boldLabel.CalcSize(gc).x;
            float height = UnityEditor.EditorStyles.label.CalcHeight(gc, width) + 4;

            sceneLabelRect.x = center ? position.x - width * .5f : position.x;
            sceneLabelRect.y = center ? position.y - height * .5f : position.y - height;
            sceneLabelRect.width = width;
            sceneLabelRect.height = height;

            DrawSolidColor(sceneLabelRect, SceneLabelBackgroundColor);

            GUI.Label(sceneLabelRect, gc, sceneBoldLabel);
        }

        public static SelectMode DoElementModeToolbar(Rect rect, SelectMode mode)
        {
            Styles.Init();

            EditorGUI.BeginChangeCheck();

            var textureMode = mode.ContainsFlag(SelectMode.TextureVertex | SelectMode.TextureEdge | SelectMode.TextureFace);

            int currentSelectionMode = -1;

            switch (mode)
            {
                case SelectMode.Vertex:
                case SelectMode.TextureVertex:
                    currentSelectionMode = 1;
                    break;
                case SelectMode.Edge:
                case SelectMode.TextureEdge:
                    currentSelectionMode = 2;
                    break;
                case SelectMode.Face:
                case SelectMode.TextureFace:
                    currentSelectionMode = 3;
                    break;
                default:
                    currentSelectionMode = -1;
                    break;
            }

            currentSelectionMode = GUI.Toolbar(rect, currentSelectionMode, Styles.selectModeIcons, Styles.command);

            if (EditorGUI.EndChangeCheck())
            {
                mode = currentSelectionMode switch
                {
                    1 => textureMode ? SelectMode.TextureVertex : SelectMode.Vertex,
                    2 => textureMode ? SelectMode.TextureEdge : SelectMode.Edge,
                    3 => textureMode ? SelectMode.TextureFace : SelectMode.Face,
                    _ => mode
                };
            }

            return mode;
        }
    }
}

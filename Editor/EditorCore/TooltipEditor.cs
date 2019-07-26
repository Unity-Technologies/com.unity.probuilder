using UnityEngine;
using UnityEditor;
using System.Reflection;
using System;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
    /// <inheritdoc />
    /// <summary>
    /// Tooltip window implementation.
    /// </summary>
    sealed class TooltipEditor : EditorWindow
    {
#if !UNITY_2019_1_OR_NEWER
        static TooltipEditor()
        {
            s_ShowModeEnum = ReflectionUtility.GetType("UnityEditor.ShowMode");
        
            s_ShowPopupWithModeMethod = typeof(EditorWindow).GetMethod(
                "ShowPopupWithMode",
                BindingFlags.NonPublic | BindingFlags.Instance);
        }

        static readonly Type s_ShowModeEnum;
        static readonly MethodInfo s_ShowPopupWithModeMethod;
#endif

        static readonly Color BasicBackgroundColor = new Color(.87f, .87f, .87f, 1f);
        const int k_PositionPadding = 4;

        static TooltipEditor s_Instance;
        static Rect s_WindowRect = new Rect(0, 0, 0, 0);

        static GUIStyle s_ProOnlyStyle = null;
        static GUIStyle proOnlyStyle
        {
            get
            {
                if (s_ProOnlyStyle == null)
                {
                    s_ProOnlyStyle = new GUIStyle(EditorStyles.largeLabel);
                    Color c = s_ProOnlyStyle.normal.textColor;
                    c.a = .20f;
                    s_ProOnlyStyle.normal.textColor = c;
                    s_ProOnlyStyle.fontStyle = FontStyle.Bold;
                    s_ProOnlyStyle.alignment = TextAnchor.UpperRight;
                    s_ProOnlyStyle.fontSize += 22;
                    s_ProOnlyStyle.padding.top += 1;
                    s_ProOnlyStyle.padding.right += 4;
                }
                return s_ProOnlyStyle;
            }
        }

        // much like highlander, there can only be one
        public static TooltipEditor instance()
        {
            if (s_Instance == null)
            {
                s_Instance = ScriptableObject.CreateInstance<TooltipEditor>();
                s_Instance.minSize = Vector2.zero;
                s_Instance.maxSize = Vector2.zero;
                s_Instance.hideFlags = HideFlags.HideAndDontSave;
#if UNITY_2019_1_OR_NEWER
                s_Instance.ShowTooltip();
#else
                if (s_ShowPopupWithModeMethod != null && s_ShowModeEnum != null)
                    s_ShowPopupWithModeMethod.Invoke(s_Instance, new [] { Enum.ToObject(s_ShowModeEnum, 1), false});
                else
                    s_Instance.ShowPopup();
#endif

                object parent = ReflectionUtility.GetValue(s_Instance, s_Instance.GetType(), "m_Parent");
                object window = ReflectionUtility.GetValue(parent, parent.GetType(), "window");
                ReflectionUtility.SetValue(parent, "mouseRayInvisible", true);
                ReflectionUtility.SetValue(window, "m_DontSaveToLayout", true);
            }

            return s_Instance;
        }

        // unlike highlander, this will hide
        public static void Hide()
        {
            TooltipEditor[] windows = Resources.FindObjectsOfTypeAll<TooltipEditor>();

            for (int i = 0; i < windows.Length; i++)
            {
                windows[i].Close();
                GameObject.DestroyImmediate(windows[i]);
                windows[i] = null;
            }
        }

        public static void Show(Rect rect, TooltipContent content)
        {
            instance().ShowInternal(rect, content);
        }

        public void ShowInternal(Rect rect, TooltipContent content)
        {
            this.content = content;

            Vector2 size = content.CalcSize();

            Vector2 p = new Vector2(rect.x + rect.width + k_PositionPadding, rect.y);
            // if(p.x > Screen.width) p.x = rect.x - POSITION_PADDING - size.x;

            this.minSize = size;
            this.maxSize = size;

            this.position = new Rect(
                    p.x,
                    p.y,
                    size.x,
                    size.y);

            s_WindowRect = new Rect(0, 0, size.x, size.y);
        }

        public TooltipContent content = null;

        void OnGUI()
        {
            if (!EditorGUIUtility.isProSkin)
            {
                GUI.backgroundColor = BasicBackgroundColor;
                GUI.Box(s_WindowRect, "");
                GUI.backgroundColor = Color.white;
            }

            if (content == null)
                return;

            content.Draw();
        }
    }
}

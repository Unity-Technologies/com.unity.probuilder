#if !UNITY_2019_1_OR_NEWER
using System;
using System.Reflection;
#endif
using UnityEngine;

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

        // much like highlander, there can only be one
        public static TooltipEditor instance()
        {
            if (s_Instance == null)
            {
                s_Instance = CreateInstance<TooltipEditor>();
                s_Instance.minSize = Vector2.zero;
                s_Instance.maxSize = Vector2.zero;
                s_Instance.hideFlags = HideFlags.HideAndDontSave;
#if UNITY_2019_1_OR_NEWER
                s_Instance.ShowTooltip();
                s_Instance.m_Parent.window.SetAlpha(1f);
#else
                if (s_ShowPopupWithModeMethod != null && s_ShowModeEnum != null)
                    s_ShowPopupWithModeMethod.Invoke(s_Instance, new [] { Enum.ToObject(s_ShowModeEnum, 1), false});
                else
                    s_Instance.ShowPopup();
#endif
            }

            return s_Instance;
        }

        public static void Hide()
        {
            var all = Resources.FindObjectsOfTypeAll<TooltipEditor>();

            for (int i = 0, c = all.Length; i < c; i++)
            {
                if (s_Instance != null)
                    s_Instance.Close();
            }
        }

        public static bool IsFocused()
        {
            return s_Instance != null && mouseOverWindow == s_Instance;
        }

        public static void Show(Rect rect, TooltipContent content)
        {
            instance().ShowInternal(rect, content);
        }

        public void ShowInternal(Rect rect, TooltipContent content)
        {
            this.content = content;
            Vector2 size = content.CalcSize();

            var dpiRatio = Screen.dpi / 96f;
            var screenWidth = Screen.currentResolution.width / dpiRatio;

            Vector2 p = new Vector2(rect.x + rect.width + k_PositionPadding, rect.y);
             if((p.x + size.x) > screenWidth)
                p.x = rect.x - k_PositionPadding - size.x;

            minSize = size;
            maxSize = size;
            var newPosition = new Rect(
                p.x,
                p.y,
                size.x,
                size.y);

            if (position != newPosition)
            {
                position = newPosition;
                s_WindowRect = new Rect(0, 0, size.x, size.y);
            }
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

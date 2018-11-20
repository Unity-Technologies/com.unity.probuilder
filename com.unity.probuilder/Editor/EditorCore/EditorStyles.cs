using System;
using UnityEngine;

namespace UnityEditor.ProBuilder.UI
{
    /// <summary>
    /// Collection of commonly used styles in the editor.
    /// </summary>
    static class EditorStyles
    {
        const string k_FontAsap = "About/Font/Asap-Regular.otf";

        const string k_ButtonBackgroundNormal = "Toolbar/Background/RoundedRect_Normal";
        const string k_ButtonBackgroundHover = "Toolbar/Background/RoundedRect_Hover";
        const string k_ButtonBackgroundPressed = "Toolbar/Background/RoundedRect_Pressed";
        const string k_SettingsBackgroundNormal = "Toolbar/RoundedBorder";
        const string k_SceneTextBoxBackgroundNormal = "Scene/TextBackground";

        static readonly Color k_TextColorWhiteNormal = new Color(0.7f, 0.7f, 0.7f, 1f);
        static readonly Color k_TextColorWhiteHover = new Color(0.7f, 0.7f, 0.7f, 1f);
        static readonly Color k_TextColorWhiteActive = new Color(0.5f, 0.5f, 0.5f, 1f);

        static bool s_Initialized;
        static GUIStyle s_ButtonStyle;
        static GUIStyle s_ToolbarHelpIcon;
        static GUIStyle s_SettingsGroup;
        static GUIStyle s_RowStyle;
        static GUIStyle s_HeaderLabel;
        static GUIStyle s_SceneTextBox;

        public static GUIStyle buttonStyle { get { Init(); return s_ButtonStyle; } }
        public static GUIStyle toolbarHelpIcon { get { Init(); return s_ToolbarHelpIcon; } }
        public static GUIStyle settingsGroup { get { Init(); return s_SettingsGroup; } }
        public static GUIStyle rowStyle { get { Init(); return s_RowStyle; } }
        public static GUIStyle headerLabel { get { Init(); return s_HeaderLabel; } }
        public static GUIStyle sceneTextBox { get { Init(); return s_SceneTextBox; } }

        static void Init()
        {
            if (s_Initialized)
                return;

            s_Initialized = true;

            s_ButtonStyle = new GUIStyle()
            {
                normal = new GUIStyleState()
                {
                    background = IconUtility.GetIcon(k_ButtonBackgroundNormal),
                    textColor = UnityEditor.EditorGUIUtility.isProSkin ? k_TextColorWhiteNormal : Color.black
                },
                hover = new GUIStyleState()
                {
                    background = IconUtility.GetIcon(k_ButtonBackgroundHover),
                    textColor = UnityEditor.EditorGUIUtility.isProSkin ? k_TextColorWhiteHover : Color.black,
                },
                active = new GUIStyleState()
                {
                    background = IconUtility.GetIcon(k_ButtonBackgroundPressed),
                    textColor = UnityEditor.EditorGUIUtility.isProSkin ? k_TextColorWhiteActive : Color.black,
                },
                alignment = ProBuilderEditor.s_IsIconGui ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft,
                border = new RectOffset(3, 3, 3, 3),
                stretchWidth = true,
                stretchHeight = false,
                margin = new RectOffset(4, 4, 4, 4),
                padding = new RectOffset(4, 4, 4, 4)
            };

            s_ToolbarHelpIcon = new GUIStyle()
            {
                margin = new RectOffset(0, 0, 0, 0),
                padding = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter,
                fixedWidth = 18,
                fixedHeight = 18
            };

            s_SettingsGroup = new GUIStyle()
            {
                normal = new GUIStyleState()
                {
                    background = IconUtility.GetIcon(k_SettingsBackgroundNormal)
                },
                hover = new GUIStyleState()
                {
                    background = IconUtility.GetIcon(k_SettingsBackgroundNormal)
                },
                active = new GUIStyleState()
                {
                    background = IconUtility.GetIcon(k_SettingsBackgroundNormal)
                },
                border = new RectOffset(3, 3, 3, 3),
                stretchWidth = true,
                stretchHeight = false,
                margin = new RectOffset(4, 4, 4, 4),
                padding = new RectOffset(4, 4, 4, 6)
            };

            s_RowStyle = new GUIStyle()
            {
                normal = new GUIStyleState() { background = UnityEditor.EditorGUIUtility.whiteTexture },
                stretchWidth = true,
                stretchHeight = false,
                margin = new RectOffset(4, 4, 4, 4),
                padding = new RectOffset(4, 4, 4, 4)
            };

            s_HeaderLabel = new GUIStyle(UnityEditor.EditorStyles.boldLabel)
            {
                alignment = TextAnchor.LowerLeft,
                fontSize = 18,
                stretchWidth = true,
                stretchHeight = false
            };

            Font asap = FileUtility.LoadInternalAsset<Font>(k_FontAsap);
            if (asap != null)
                s_HeaderLabel.font = asap;

            s_SceneTextBox = new GUIStyle(GUI.skin.box)
            {
                wordWrap = false,
                richText = true,
                stretchWidth = false,
                stretchHeight = false,
                border = new RectOffset(2, 2, 2, 2),
                padding = new RectOffset(4, 4, 4, 4),
                alignment = TextAnchor.UpperLeft,
                normal = new GUIStyleState()
                {
                    textColor = k_TextColorWhiteNormal,
                    background = IconUtility.GetIcon(k_SceneTextBoxBackgroundNormal)
                }
            };
        }
    }
}

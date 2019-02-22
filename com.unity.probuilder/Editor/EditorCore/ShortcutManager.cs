#if UNITY_2019_1_OR_NEWER
#define SHORTCUT_MANAGER
#endif

#if !SHORTCUT_MANAGER

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEditor.SettingsManagement;

namespace UnityEditor.ProBuilder
{
    static class ShortcutManager
    {
        const int k_ShortcutLineHeight = 20;

        static Shortcut[] s_Shortcuts;
        static int s_SelectedIndex = 0;
        static Vector2 s_ShortcutScroll;

        [UserSettingBlock("Shortcuts")]
        static void ShortcutSettings(string searchContext)
        {
            if (!string.IsNullOrEmpty(searchContext))
                return;

            s_Shortcuts = ProBuilderEditor.s_Shortcuts;

            if (s_Shortcuts == null || s_Shortcuts.Length < 1)
                ProBuilderEditor.s_Shortcuts.SetValue(s_Shortcuts = Shortcut.DefaultShortcuts().ToArray(), true);

            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.Width(EditorGUIUtility.labelWidth));
            ShortcutSelectPanel();
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            ShortcutEditPanel();
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();

            SettingsGUILayout.DoResetContextMenuForLastRect(ProBuilderEditor.s_Shortcuts);
        }

        static void ShortcutSelectPanel()
        {
            GUILayout.Space(4);
            GUI.contentColor = Color.white;

            GUIStyle labelStyle = GUIStyle.none;

            if (EditorGUIUtility.isProSkin)
                labelStyle.normal.textColor = new Color(1f, 1f, 1f, .8f);

            labelStyle.alignment = TextAnchor.MiddleLeft;
            labelStyle.contentOffset = new Vector2(4f, 0f);

            s_ShortcutScroll = EditorGUILayout.BeginScrollView(s_ShortcutScroll, false, true, GUILayout.MinHeight(150));

            for (int n = 1; n < s_Shortcuts.Length; n++)
            {
                if (n == s_SelectedIndex)
                {
                    GUI.backgroundColor = new Color(0.23f, .49f, .89f, 1f);
                    labelStyle.normal.background = EditorGUIUtility.whiteTexture;
                    Color oc = labelStyle.normal.textColor;
                    labelStyle.normal.textColor = Color.white;
                    GUILayout.Box(s_Shortcuts[n].action, labelStyle, GUILayout.MinHeight(k_ShortcutLineHeight),
                        GUILayout.MaxHeight(k_ShortcutLineHeight));
                    labelStyle.normal.background = null;
                    labelStyle.normal.textColor = oc;
                    GUI.backgroundColor = Color.white;
                }
                else
                {
                    if (GUILayout.Button(s_Shortcuts[n].action, labelStyle, GUILayout.MinHeight(k_ShortcutLineHeight),
                            GUILayout.MaxHeight(k_ShortcutLineHeight)))
                    {
                        s_SelectedIndex = n;
                    }
                }
            }

            EditorGUILayout.EndScrollView();
        }

        static void ShortcutEditPanel()
        {
            s_SelectedIndex = Math.Min(Math.Max(0, s_SelectedIndex), s_Shortcuts.Length - 1);

            GUILayout.Label("Key", EditorStyles.boldLabel);
            KeyCode key = s_Shortcuts[s_SelectedIndex].key;
            key = (KeyCode)EditorGUILayout.EnumPopup(key, GUILayout.MaxWidth(128));
            s_Shortcuts[s_SelectedIndex].key = key;

            GUILayout.Label("Modifiers", EditorStyles.boldLabel);

            // EnumMaskField returns a bit-mask where the flags correspond to the indexes of the enum, not the enum values,
            // so this isn't technically correct.
            EventModifiers em = (EventModifiers)s_Shortcuts[s_SelectedIndex].eventModifiers;
            s_Shortcuts[s_SelectedIndex].eventModifiers = (EventModifiers)EditorGUILayout.EnumFlagsField(em, GUILayout.MaxWidth(128));

            GUILayout.Label("Description", EditorStyles.boldLabel);

            GUILayout.Label(s_Shortcuts[s_SelectedIndex].description, EditorStyles.wordWrappedLabel);
        }
    }
}

#endif

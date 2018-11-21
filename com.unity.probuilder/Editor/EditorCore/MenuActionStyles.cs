// #define GENERATE_DESATURATED_ICONS

using UnityEngine.ProBuilder;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Styles used in MenuAction.
    /// </summary>
    static class MenuActionStyles
    {
        internal static readonly Color TEXT_COLOR_WHITE_NORMAL = new Color(0.82f, 0.82f, 0.82f, 1f);
        internal static readonly Color TEXT_COLOR_WHITE_HOVER = new Color(0.7f, 0.7f, 0.7f, 1f);
        internal static readonly Color TEXT_COLOR_WHITE_ACTIVE = new Color(0.5f, 0.5f, 0.5f, 1f);

        static GUIStyle s_ButtonStyleVertical = null;
        static GUIStyle s_ButtonStyleHorizontal = null;
        static GUIStyle s_RowStyleVertical = null;
        static GUIStyle s_RowStyleHorizontal = null;
        static GUIStyle s_AltButtonStyle = null;

        /// <summary>
        /// Reset static GUIStyle objects so that they will be re-initialized the next time used.
        /// </summary>
        internal static void ResetStyles()
        {
            s_ButtonStyleVertical = null;
            s_ButtonStyleHorizontal = null;
            s_RowStyleVertical = null;
            s_AltButtonStyle = null;
        }

        /// <summary>
        /// Vertical icon button.
        /// </summary>
        internal static GUIStyle buttonStyleVertical
        {
            get
            {
                if (s_ButtonStyleVertical == null)
                {
                    s_ButtonStyleVertical = new GUIStyle();
                    s_ButtonStyleVertical.normal.background = IconUtility.GetIcon("Toolbar/Button_Normal", IconSkin.Pro);
                    s_ButtonStyleVertical.normal.textColor = EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_NORMAL : Color.black;
                    s_ButtonStyleVertical.hover.background = IconUtility.GetIcon("Toolbar/Button_Hover", IconSkin.Pro);
                    s_ButtonStyleVertical.hover.textColor = EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_HOVER : Color.black;
                    s_ButtonStyleVertical.active.background = IconUtility.GetIcon("Toolbar/Button_Pressed", IconSkin.Pro);
                    s_ButtonStyleVertical.active.textColor = EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_ACTIVE : Color.black;
                    s_ButtonStyleVertical.alignment = ProBuilderEditor.s_IsIconGui ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;
                    s_ButtonStyleVertical.border = new RectOffset(4, 0, 0, 0);
                    s_ButtonStyleVertical.stretchWidth = true;
                    s_ButtonStyleVertical.stretchHeight = false;
                    s_ButtonStyleVertical.margin = new RectOffset(4, 5, 4, 4);
                    s_ButtonStyleVertical.padding = new RectOffset(8, 0, 2, 2);
                }
                return s_ButtonStyleVertical;
            }
        }

        internal static GUIStyle buttonStyleHorizontal
        {
            get
            {
                if (s_ButtonStyleHorizontal == null)
                {
                    s_ButtonStyleHorizontal = new GUIStyle();

                    s_ButtonStyleHorizontal.normal.textColor    = EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_NORMAL : Color.black;
                    s_ButtonStyleHorizontal.normal.background   = IconUtility.GetIcon("Toolbar/Button_Normal_Horizontal", IconSkin.Pro);
                    s_ButtonStyleHorizontal.hover.background    = IconUtility.GetIcon("Toolbar/Button_Hover_Horizontal", IconSkin.Pro);
                    s_ButtonStyleHorizontal.hover.textColor         = EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_HOVER : Color.black;
                    s_ButtonStyleHorizontal.active.background   = IconUtility.GetIcon("Toolbar/Button_Pressed_Horizontal", IconSkin.Pro);
                    s_ButtonStyleHorizontal.active.textColor    = EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_ACTIVE : Color.black;
                    s_ButtonStyleHorizontal.alignment           = TextAnchor.MiddleCenter;
                    s_ButtonStyleHorizontal.border              = new RectOffset(0, 0, 4, 0);
                    s_ButtonStyleHorizontal.stretchWidth        = true;
                    s_ButtonStyleHorizontal.stretchHeight       = false;
                    s_ButtonStyleHorizontal.margin              = new RectOffset(4, 4, 4, 5);
                    s_ButtonStyleHorizontal.padding                 = new RectOffset(2, 2, 8, 0);
                }
                return s_ButtonStyleHorizontal;
            }
        }

        internal static GUIStyle rowStyleVertical
        {
            get
            {
                if (s_RowStyleVertical == null)
                {
                    s_RowStyleVertical = new GUIStyle();
                    s_RowStyleVertical.alignment = TextAnchor.MiddleLeft;
                    s_RowStyleVertical.stretchWidth = true;
                    s_RowStyleVertical.stretchHeight = false;
                    s_RowStyleVertical.margin = new RectOffset(0, 0, 0, 0);
                    s_RowStyleVertical.padding = new RectOffset(0, 0, 0, 0);
                }
                return s_RowStyleVertical;
            }
        }

        internal static GUIStyle rowStyleHorizontal
        {
            get
            {
                if (s_RowStyleHorizontal == null)
                {
                    s_RowStyleHorizontal = new GUIStyle();
                    s_RowStyleHorizontal.alignment = TextAnchor.MiddleCenter;
                    s_RowStyleHorizontal.stretchWidth = true;
                    s_RowStyleHorizontal.stretchHeight = false;
                    s_RowStyleHorizontal.margin = new RectOffset(0, 0, 0, 0);
                    s_RowStyleHorizontal.padding = new RectOffset(0, 0, 0, 0);
                }
                return s_RowStyleHorizontal;
            }
        }

        internal static GUIStyle altButtonStyle
        {
            get
            {
                if (s_AltButtonStyle == null)
                {
                    s_AltButtonStyle = new GUIStyle();

                    s_AltButtonStyle.normal.background  = IconUtility.GetIcon("Toolbar/AltButton_Normal", IconSkin.Pro);
                    s_AltButtonStyle.normal.textColor   = EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_NORMAL : Color.black;
                    s_AltButtonStyle.hover.background   = IconUtility.GetIcon("Toolbar/AltButton_Hover", IconSkin.Pro);
                    s_AltButtonStyle.hover.textColor    = EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_HOVER : Color.black;
                    s_AltButtonStyle.active.background  = IconUtility.GetIcon("Toolbar/AltButton_Pressed", IconSkin.Pro);
                    s_AltButtonStyle.active.textColor   = EditorGUIUtility.isProSkin ? TEXT_COLOR_WHITE_ACTIVE : Color.black;
                    s_AltButtonStyle.alignment          = TextAnchor.MiddleCenter;
                    s_AltButtonStyle.border                 = new RectOffset(1, 1, 1, 1);
                    s_AltButtonStyle.stretchWidth       = false;
                    s_AltButtonStyle.stretchHeight      = true;
                    s_AltButtonStyle.margin                 = new RectOffset(4, 4, 4, 4);
                    s_AltButtonStyle.padding            = new RectOffset(2, 2, 1, 3);
                }
                return s_AltButtonStyle;
            }
        }
    }
}

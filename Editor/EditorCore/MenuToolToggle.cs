using System;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.ProBuilder;

#if !UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.EditorTools;
#else
using ToolManager = UnityEditor.EditorTools.ToolManager;
#endif

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Base class from which any action that is represented in the ProBuilder toolbar inherits.
    /// </summary>
    public abstract class MenuToolToggle: MenuAction
    {
        protected EditorTool m_Tool;

        public EditorTool Tool
        {
            get => m_Tool;
        }

        protected MenuToolToggle()
        {
            iconMode = ProBuilderEditor.s_IsIconGui;
        }

        /// <summary>
        /// Perform whatever action this menu item is supposed to do when starting. You are responsible for implementing Undo.
        /// </summary>
        /// <returns>A new ActionResult with a summary of the state of the action's success.</returns>

        internal ActionResult StartActivation()
        {
            if(onPerformAction != null)
                onPerformAction(this);
            return PerformActionImplementation();
        }

        /// <summary>
        /// Perform whatever action this menu item is supposed to do when ending. You are responsible for implementing Undo.
        /// </summary>
        /// <returns>A new ActionResult with a summary of the state of the action's success.</returns>
        internal abstract ActionResult EndActivation();

        /// <summary>
        /// Draw a menu button.  Returns true if the button is active and settings are enabled, false if settings are not enabled.
        /// </summary>
        /// <param name="isHorizontal"></param>
        /// <param name="showOptions"></param>
        /// <param name="optionsRect"></param>
        /// <param name="layoutOptions"></param>
        /// <returns></returns>
        internal override bool DoButton(bool isHorizontal, bool showOptions, ref Rect optionsRect, params GUILayoutOption[] layoutOptions)
        {
            bool wasEnabled = GUI.enabled;
            bool buttonEnabled = (menuActionState & MenuActionState.Enabled) == MenuActionState.Enabled;

            bool isActiveTool = m_Tool != null && ToolManager.IsActiveTool(m_Tool);

            GUI.enabled = buttonEnabled;

            GUI.backgroundColor = Color.white;

            if (iconMode)
            {
                GUIStyle style = ToolbarGroupUtility.GetStyle(group, isHorizontal);

                Texture2D normalTex = style.normal.background;
                Texture2D hoverTex = style.hover.background;
                if( isActiveTool )
                {
                    style.normal.background = hoverTex;
                    style.hover.background = normalTex;
                }

                bool isToggled = GUILayout.Toggle( isActiveTool, buttonEnabled || !disabledIcon ? icon : disabledIcon, style, layoutOptions);
                if(isToggled != isActiveTool)
                {
                    if (showOptions && (optionsMenuState & MenuActionState.VisibleAndEnabled) == MenuActionState.VisibleAndEnabled)
                    {
                        DoAlternateAction();
                    }
                    else
                    {
                        var result = isActiveTool ? EndActivation() : StartActivation();
                        EditorUtility.ShowNotification(result.notification);
                        ProBuilderAnalytics.SendActionEvent(this, ProBuilderAnalytics.TriggerType.ProBuilderUI);
                    }
                }

                style.normal.background = normalTex;
                style.hover.background = hoverTex;

                if ((optionsMenuState & MenuActionState.VisibleAndEnabled) == MenuActionState.VisibleAndEnabled)
                {
                    Rect r = GUILayoutUtility.GetLastRect();
                    r.x = r.x + r.width - 16;
                    r.y += 0;
                    r.width = 14;
                    r.height = 14;
                    GUI.Label(r, IconUtility.GetIcon("Toolbar/Options", IconSkin.Pro), GUIStyle.none);
                    optionsRect = r;
                    GUI.enabled = wasEnabled;
                    return buttonEnabled;
                }
                else
                {
                    GUI.enabled = wasEnabled;
                    return false;
                }
            }
            else
            {
                // in text mode always use the vertical layout.
                isHorizontal = false;
                GUILayout.BeginHorizontal(MenuActionStyles.rowStyleVertical, layoutOptions);
                GUI.backgroundColor = ToolbarGroupUtility.GetColor(group);

                GUIStyle style = MenuActionStyles.buttonStyleVertical;
                RectOffset border = new RectOffset(style.border.left,style.border.right,style.border.top,style.border.bottom);
                if( isActiveTool )
                {
                    style.border = new RectOffset( 0, 4, 0, 0 );
                }

                bool isToggled = GUILayout.Toggle( isActiveTool, menuTitle, style);
                if (isToggled != isActiveTool)
                {
                    var result = isActiveTool ? EndActivation() : StartActivation();
                    EditorUtility.ShowNotification(result.notification);
                    ProBuilderAnalytics.SendActionEvent(this, ProBuilderAnalytics.TriggerType.ProBuilderUI);
                }

                MenuActionState altState = optionsMenuState;

                if ((altState & MenuActionState.Visible) == MenuActionState.Visible)
                {
                    GUI.enabled = GUI.enabled && (altState & MenuActionState.Enabled) == MenuActionState.Enabled;

                    if (DoAltButton(GUILayout.MaxWidth(21), GUILayout.MaxHeight(16)))
                        DoAlternateAction();
                }

                style.border = border;
                GUILayout.EndHorizontal();

                GUI.backgroundColor = Color.white;

                GUI.enabled = wasEnabled;

                return false;
            }
        }
    }
}

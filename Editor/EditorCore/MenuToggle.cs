
#if UNITY_2019_1_OR_NEWER
#define SHORTCUT_MANAGER
#endif

using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.ProBuilder;
using Debug = UnityEngine.Debug;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Base class from which any action that is represented in the ProBuilder toolbar inherits.
    /// </summary>
    public abstract class MenuToggle: MenuAction
    {
        public enum MenuToggleState
        {
            Inactive,
            Active,
        };

        MenuToggleState m_CurrentState;

        protected delegate void StartEndCallBack();


        protected MenuToggle()
        {
            iconMode = ProBuilderEditor.s_IsIconGui;
            m_CurrentState = MenuToggleState.Inactive;

            EditorApplication.update += OnUpdate;
        }

        /// <summary>
        /// Not used for MenuToggle.
        /// </summary>
        /// <returns>A new ActionResult with a summary of the state of the action's success.</returns>
        public override ActionResult DoAction() {return ActionResult.Success;}

        protected void OnStart()
        {
            m_CurrentState = MenuToggleState.Active;
        }

        protected void OnEnd()
        {
            m_CurrentState = MenuToggleState.Inactive;
        }

        /// <summary>
        /// Perform whatever action this menu item is supposed to do when starting. You are responsible for implementing Undo.
        /// </summary>
        /// <returns>A new ActionResult with a summary of the state of the action's success.</returns>
        protected abstract ActionResult StartActivation(StartEndCallBack callback);

        /// <summary>
        /// Call the Update for the current tool.
        /// </summary>
        public void OnUpdate()
        {
            if(m_CurrentState == MenuToggleState.Active)
                UpdateAction();
        }

        // /// <summary>
        // /// Perform whatever action this menu item is supposed to do during its update. You are responsible for implementing Undo.
        // /// </summary>
        protected virtual void UpdateAction(){}

        /// <summary>
        /// Perform whatever action this menu item is supposed to do when ending. You are responsible for implementing Undo.
        /// </summary>
        /// <returns>A new ActionResult with a summary of the state of the action's success.</returns>
        protected abstract ActionResult EndActivation(StartEndCallBack callback);

        /// <summary>
        /// Draw a menu button.  Returns true if the button is active and settings are enabled, false if settings are not enabled.
        /// </summary>
        /// <param name="isHorizontal"></param>
        /// <param name="showOptions"></param>
        /// <param name="optionsRect"></param>
        /// <param name="layoutOptions"></param>
        /// <returns></returns>
        public override bool DoButton(bool isHorizontal, bool showOptions, ref Rect optionsRect, params GUILayoutOption[] layoutOptions)
        {
            bool wasEnabled = GUI.enabled;
            bool buttonEnabled = (menuActionState & MenuActionState.Enabled) == MenuActionState.Enabled;

            GUI.enabled = buttonEnabled;

            GUI.backgroundColor = Color.white;

            if (iconMode)
            {
                if (GUILayout.Button(buttonEnabled || !disabledIcon ? icon : disabledIcon, ToolbarGroupUtility.GetStyle(group, isHorizontal), layoutOptions))
                {
                    if (showOptions && (optionsMenuState & MenuActionState.VisibleAndEnabled) == MenuActionState.VisibleAndEnabled)
                    {
                        DoAlternateAction();
                    }
                    else
                    {
                        ActionResult result = DoAction();
                        EditorUtility.ShowNotification(result.notification);
                    }
                }

                bool isToggled = GUILayout.Toggle( m_CurrentState == MenuToggleState.Active, menuTitle, style);
                if (isToggled != (m_CurrentState == MenuToggleState.Active))
                {
                    m_CurrentState = isToggled ? MenuToggleState.Active : MenuToggleState.Inactive;
                    ActionResult result = (m_CurrentState == MenuToggleState.Active) ? StartActivation(OnStart) : EndActivation(OnEnd);
                    EditorUtility.ShowNotification(result.notification);
                }


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
                if( m_CurrentState == MenuToggleState.Active )
                {
                    style.border = new RectOffset( 0, 4, 0, 0 );
                }

                bool isToggled = GUILayout.Toggle( m_CurrentState == MenuToggleState.Active, menuTitle, style);
                if (isToggled != (m_CurrentState == MenuToggleState.Active))
                {
                    m_CurrentState = isToggled ? MenuToggleState.Active : MenuToggleState.Inactive;
                    ActionResult result = (m_CurrentState == MenuToggleState.Active) ? StartActivation(OnStart) : EndActivation(OnEnd);
                    EditorUtility.ShowNotification(result.notification);
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

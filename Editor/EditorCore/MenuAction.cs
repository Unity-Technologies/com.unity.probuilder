// #define GENERATE_DESATURATED_ICONS

using System;
using UnityEngine;
using UnityEngine.ProBuilder;
#if UNITY_2020_2_OR_NEWER
using ToolManager = UnityEditor.EditorTools.ToolManager;
#else
using ToolManager = UnityEditor.EditorTools.EditorTools;
#endif

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Base class from which any action that is represented in the ProBuilder toolbar inherits.
    /// </summary>
    public abstract class MenuAction
    {
        /// <summary>
        /// A flag indicating the state of a menu action. This determines whether the menu item is visible, and if visible, enabled.
        /// </summary>
        [System.Flags]
        public enum MenuActionState
        {
            /// <summary>
            /// The button is not visible in the toolbar.
            /// </summary>
            Hidden = 0x0,
            /// <summary>
            /// The button is visible in the toolbar.
            /// </summary>
            Visible = 0x1,
            /// <summary>
            /// The button (and by proxy, the action it performs) are valid given the current selection.
            /// </summary>
            Enabled = 0x2,
            /// <summary>
            /// Button and action are both visible in the toolbar and valid given the current selection.
            /// </summary>
            VisibleAndEnabled = 0x3
        };

        /// <value>
        /// Path to the ProBuilder menu category.
        /// </value>
        /// <remarks>
        /// Use this where you wish to add a top level menu item.
        /// </remarks>
        internal const string probuilderMenuPath = "Tools/ProBuilder/";

        /// <value>
        /// The unicode character for the control key symbol on Windows, or command key on macOS.
        /// </value>
        internal const char keyCommandSuper = PreferenceKeys.CMD_SUPER;

        /// <value>
        /// The unicode character for the shift key symbol.
        /// </value>
        internal const char keyCommandShift = PreferenceKeys.CMD_SHIFT;

        /// <value>
        /// The unicode character for the option key symbol on macOS.
        /// </value>
        /// <seealso cref="keyCommandAlt"/>
        internal const char keyCommandOption = PreferenceKeys.CMD_OPTION;

        /// <value>
        /// The unicode character for the alt key symbol on Windows.
        /// </value>
        internal const char keyCommandAlt = PreferenceKeys.CMD_ALT;

        /// <value>
        /// The unicode character for the delete key symbol.
        /// </value>
        internal const char keyCommandDelete = PreferenceKeys.CMD_DELETE;

        static readonly GUIContent AltButtonContent = new GUIContent("+", "");

        static readonly Vector2 AltButtonSize = new Vector2(21, 0);

        Vector2 m_LastCalculatedSize = Vector2.zero;

        public static Action<MenuAction> onPerformAction;

        protected MenuAction()
        {
            iconMode = ProBuilderEditor.s_IsIconGui;
        }

        /// <summary>
        /// Compare two menu items precedence by their category and priority modifier.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        internal static int CompareActionsByGroupAndPriority(MenuAction left, MenuAction right)
        {
            if (left == null)
            {
                if (right == null)
                    return 0;
                else
                    return -1;
            }
            else
            {
                if (right == null)
                {
                    return 1;
                }
                else
                {
                    int l = (int)left.group, r = (int)right.group;

                    if (l < r)
                        return -1;
                    else if (l > r)
                        return 1;
                    else
                    {
                        int lp = left.toolbarPriority < 0 ? int.MaxValue : left.toolbarPriority,
                            rp = right.toolbarPriority < 0 ? int.MaxValue : right.toolbarPriority;

                        return lp.CompareTo(rp);
                    }
                }
            }
        }

        Texture2D m_DesaturatedIcon = null;

        /// <summary>
        /// By default this function will look for an image named `${icon}_disabled`. If your disabled icon is somewhere else override this function.
        /// </summary>
        protected virtual Texture2D disabledIcon
        {
            get
            {
                if (m_DesaturatedIcon == null)
                {
                    if (icon == null)
                        return null;

                    m_DesaturatedIcon = IconUtility.GetIcon(string.Format("Toolbar/{0}_disabled", icon.name));

#if GENERATE_DESATURATED_ICONS
                    if (!m_DesaturatedIcon)
                        m_DesaturatedIcon = ProBuilder2.EditorCommon.DebugUtilities.pb_GenerateDesaturatedImage.CreateDesaturedImage(icon);
#endif
                }

                return m_DesaturatedIcon;
            }
        }

        /// <value>
        /// What category this action belongs in.
        /// </value>
        public abstract ToolbarGroup group { get; }

        /// <value>
        /// Optional value influences where in the toolbar this menu item will be placed.
        /// <remarks>
        /// 0 is first, 1 is second, -1 is no preference.
        /// </remarks>
        /// </value>
        public virtual int toolbarPriority { get { return -1; } }

        /// <value>
        /// The icon to be displayed for this action.
        /// </value>
        /// <remarks>
        /// Not used when toolbar is in text mode.
        /// </remarks>
        public abstract Texture2D icon { get; }

        /// <value>
        /// The contents to display for this menu action's tooltip.
        /// </value>
        public abstract TooltipContent tooltip { get; }

        /// <value>
        /// Optional override for the action title displayed in the toolbar button.
        /// </value>
        /// <remarks>
        /// If unimplemented the tooltip title is used.
        /// </remarks>
        public virtual string menuTitle { get { return tooltip.title; } }

        /// <value>
        /// True if this class should have an entry built into the hardware menu. This is not implemented for custom actions.
        /// </value>
        protected virtual bool hasFileMenuEntry { get { return true; } }

        /// <summary>
        /// Is the current mode and selection valid for this action?
        /// </summary>
        /// <value>A flag indicating both the visibility and enabled state for an action.</value>
        public MenuActionState menuActionState
        {
            get
            {
                if (hidden)
                    return MenuActionState.Hidden;
                if (enabled)
                    return MenuActionState.VisibleAndEnabled;
                return MenuActionState.Visible;
            }
        }

        /// <summary>
        /// In which SelectMode states is this action applicable. Drives the `virtual bool hidden { get; }` property unless overridden.
        /// </summary>
        public virtual SelectMode validSelectModes
        {
            get { return SelectMode.Any; }
        }

        /// <summary>
        /// A check for whether or not the action is valid given the current selection.
        /// </summary>
        /// <seealso cref="hidden"/>
        /// <value>True if this action is valid with current selection and mode.</value>
        public virtual bool enabled
        {
            get
            {
                var b1 = ProBuilderEditor.instance != null;
                var b2 = ProBuilderEditor.selectMode.ContainsFlag(validSelectModes);
                var b3 = !ProBuilderEditor.selectMode.ContainsFlag(SelectMode.InputTool);
                return b1
                       && b2
                       && b3;
            }
        }

        /// <summary>
        /// Is this action visible in the ProBuilder toolbar?
        /// </summary>
        /// <remarks>This returns false by default.</remarks>
        /// <seealso cref="enabled"/>
        /// <value>True if this action should be shown in the toolbar with the current mode and settings, false otherwise.</value>
        public virtual bool hidden
        {
            get { return !ProBuilderEditor.selectMode.ContainsFlag(validSelectModes); }
        }

        /// <summary>
        /// Get a flag indicating the visibility and enabled state of an extra options menu modifier for this action.
        /// </summary>
        /// <value>A flag specifying whether an options icon should be displayed for this action button. If your action implements some etra options, you must also implement OnSettingsGUI.</value>
        protected virtual MenuActionState optionsMenuState
        {
            get { return MenuActionState.Hidden; }
        }

        /// <summary>
        /// Perform whatever action this menu item is supposed to do.
        /// Implementation should be coded in PerformActionImplementation.
        /// Perform action should be called to trigger the onPerformAction event.
        /// </summary>
        /// <returns>A new ActionResult with a summary of the state of the action's success.</returns>
        public ActionResult PerformAction()
        {
            if(onPerformAction != null)
                onPerformAction(this);
            return PerformActionImplementation();
        }

        /// <summary>
        /// Perform whatever action this menu item is supposed to do. This method should never been call directly
        /// but though PerformAction.
        /// </summary>
        /// <returns>A new ActionResult with a summary of the state of the action's success.</returns>
        protected abstract ActionResult PerformActionImplementation();

        /// <summary>
        /// Perform whatever action this menu item is supposed to do. You are responsible for implementing Undo.
        /// </summary>
        /// <returns>A new ActionResult with a summary of the state of the action's success.</returns>
        const string obsoleteDoActionMsg = "DoAction() has been replaced by PerformAction(), the implementation of the action should inherits from PerformActionImplementation(). (UnityUpgradable) -> PerformAction()";
        [Obsolete(obsoleteDoActionMsg, false)]
        public ActionResult DoAction() => PerformAction();


        protected virtual void DoAlternateAction()
        {
            MenuOption.Show(OnSettingsGUI, OnSettingsEnable, OnSettingsDisable);
        }

        /// <summary>
        /// Implement the extra settings GUI for your action in this method.
        /// </summary>
        protected virtual void OnSettingsGUI() {}

        /// <summary>
        /// Called when the settings window is opened.
        /// </summary>
        protected virtual void OnSettingsEnable() {}

        /// <summary>
        /// Called when the settings window is closed.
        /// </summary>
        protected virtual void OnSettingsDisable() {}

        protected bool iconMode { get; set; }

        /// <summary>
        /// Draw a menu button.  Returns true if the button is active and settings are enabled, false if settings are not enabled.
        /// </summary>
        /// <param name="isHorizontal"></param>
        /// <param name="showOptions"></param>
        /// <param name="optionsRect"></param>
        /// <param name="layoutOptions"></param>
        /// <returns></returns>
        internal virtual bool DoButton(bool isHorizontal, bool showOptions, ref Rect optionsRect, params GUILayoutOption[] layoutOptions)
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
                        ActionResult result = PerformAction();
                        EditorUtility.ShowNotification(result.notification);
                        ProBuilderAnalytics.SendActionEvent(this, ProBuilderAnalytics.TriggerType.ProBuilderUI);
                    }
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

                if (GUILayout.Button(menuTitle, MenuActionStyles.buttonStyleVertical))
                {
                    ActionResult res = PerformAction();
                    EditorUtility.ShowNotification(res.notification);
                    ProBuilderAnalytics.SendActionEvent(this, ProBuilderAnalytics.TriggerType.ProBuilderUI);
                }
                MenuActionState altState = optionsMenuState;

                if ((altState & MenuActionState.Visible) == MenuActionState.Visible)
                {
                    GUI.enabled = GUI.enabled && (altState & MenuActionState.Enabled) == MenuActionState.Enabled;

                    if (DoAltButton(GUILayout.MaxWidth(21), GUILayout.MaxHeight(16)))
                        DoAlternateAction();
                }
                GUILayout.EndHorizontal();

                GUI.backgroundColor = Color.white;

                GUI.enabled = wasEnabled;

                return false;
            }
        }

        protected bool DoAltButton(params GUILayoutOption[] options)
        {
            return GUILayout.Button(AltButtonContent, MenuActionStyles.altButtonStyle, options);
        }

        /// <summary>
        /// Get the rendered width of this GUI item.
        /// </summary>
        /// <param name="isHorizontal"></param>
        /// <returns></returns>
        internal Vector2 GetSize(bool isHorizontal)
        {
            if (iconMode)
            {
                m_LastCalculatedSize = ToolbarGroupUtility.GetStyle(ToolbarGroup.Object, isHorizontal).CalcSize(UI.EditorGUIUtility.TempContent(null, null, icon));
            }
            else
            {
                // in text mode always use the vertical layout.
                isHorizontal = false;
                m_LastCalculatedSize = MenuActionStyles.buttonStyleVertical.CalcSize(UI.EditorGUIUtility.TempContent(menuTitle)) + AltButtonSize;
            }
            return m_LastCalculatedSize;
        }
    }
}

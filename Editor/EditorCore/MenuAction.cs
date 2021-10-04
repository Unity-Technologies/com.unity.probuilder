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
    /// Base class for any action that appears on the ProBuilder toolbar.
    /// </summary>
    public abstract class MenuAction
    {
        /// <summary>
        /// Determines whether the menu item is visible, and if visible, whether it is enabled.
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

        /// <summary>
        /// Path to the ProBuilder menu category.
        /// </summary>
        /// <remarks>
        /// Use this where you wish to add a top level menu item.
        /// </remarks>
        internal const string probuilderMenuPath = "Tools/ProBuilder/";

        /// <summary>
        /// The unicode character for the control key symbol on Windows, or command key on macOS.
        /// </summary>
        internal const char keyCommandSuper = PreferenceKeys.CMD_SUPER;

        /// <summary>
        /// The unicode character for the shift key symbol.
        /// </summary>
        internal const char keyCommandShift = PreferenceKeys.CMD_SHIFT;

        /// <summary>
        /// The unicode character for the option key symbol on macOS.
        /// </summary>
        /// <seealso cref="keyCommandAlt"/>
        internal const char keyCommandOption = PreferenceKeys.CMD_OPTION;

        /// <summary>
        /// The unicode character for the alt key symbol on Windows.
        /// </summary>
        internal const char keyCommandAlt = PreferenceKeys.CMD_ALT;

        /// <summary>
        /// The unicode character for the delete key symbol.
        /// </summary>
        internal const char keyCommandDelete = PreferenceKeys.CMD_DELETE;

        static readonly GUIContent AltButtonContent = new GUIContent("+", "");

        static readonly Vector2 AltButtonSize = new Vector2(21, 0);

        Vector2 m_LastCalculatedSize = Vector2.zero;

        /// <summary>
        /// Invoked when the user selects an action to perform from the toolbar.
        /// </summary>
        public static Action<MenuAction> onPerformAction;

        /// <summary>
        /// Creates a new button on the [ProBuilder toolbar](../manual/toolbar.html) in the Editor.
        /// </summary>
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
        /// Gets the icon to use when the action button on the toolbar is disabled. By default, this function looks for an image named
        /// `${icon}_disabled`. Override this function if your disabled icon does not follow that naming convention or location.
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

        /// <summary>
        /// Gets the category assigned to this action.
        /// </summary>
        public abstract ToolbarGroup group { get; }

        /// <summary>
        /// Gets the value to optionally influence where this menu item appears in the toolbar.
        /// The default value is -1 (no preference).
        /// </summary>
        /// <remarks>
        /// 0 is first, 1 is second, -1 is no preference.
        /// </remarks>
        public virtual int toolbarPriority { get { return -1; } }

        /// <summary>
        /// Gets the icon to display on the toolbar for this action.
        /// </summary>
        /// <remarks>
        /// This property is not used when the [Toolbar display mode](../manual/toolbar.html#toolbar-display-modes) is set to text.
        /// </remarks>
        public abstract Texture2D icon { get; }

        /// <summary>
        /// Gets the contents of the tooltip to display for this menu action.
        /// </summary>
        public abstract TooltipContent tooltip { get; }

        /// <summary>
        /// Gets the override title for this action to display on the toolbar button.
        /// </summary>
        /// <remarks>
        /// If you don't implement this property, toolbar button displays the tooltip title.
        /// </remarks>
        public virtual string menuTitle { get { return tooltip.title; } }

        /// <summary>
        /// Gets whether this class should have an entry built into the hardware menu. This is not implemented for custom actions.
        /// </summary>
        protected virtual bool hasFileMenuEntry { get { return true; } }

        /// <summary>
        /// Gets a flag that indicates both the visibility and enabled state of an action
        /// to determine whether the current mode and selection is valid for it.
        /// </summary>
        /// <returns>.</returns>
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
        /// Gets the SelectMode states where this action applies. This drives the <see cref="hidden"/> property unless you override it.
        /// </summary>
        public virtual SelectMode validSelectModes
        {
            get { return SelectMode.Any; }
        }

        /// <summary>
        /// Gets whether or not the action is valid given the current selection.
        ///
        /// True if this action is valid with the current selection and mode.
        /// </summary>
        /// <seealso cref="hidden"/>
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
        /// Gets whether this action is visible in the ProBuilder toolbar.
        ///
        /// True if this action appears in the toolbar with the current mode and settings; false otherwise.
        /// </summary>
        /// <remarks>This returns false by default.</remarks>
        /// <seealso cref="enabled"/>
        public virtual bool hidden
        {
            get { return !ProBuilderEditor.selectMode.ContainsFlag(validSelectModes); }
        }

        /// <summary>
        /// Gets a flag that indicates whether the action implements extra options. If it does, it must also
        /// implement <see cref="OnSettingsGUI"/> so that an options indicator appears for this action button.
        /// </summary>
        protected virtual MenuActionState optionsMenuState
        {
            get { return MenuActionState.Hidden; }
        }

        /// <summary>
        /// Performs the action for this menu item. Use <see cref="PerformActionImplementation"/> to implement the action.
        /// Calling this method triggers the <see cref="onPerformAction"/> event.
        /// </summary>
        /// <remarks>
        /// Any new action classes that derive from this base class must also use the `PerformAction` method to register
        /// the new action in the Undo call stack.
        /// </remarks>
        /// <returns>A new ActionResult with a summary of the state of the action's success.</returns>
        public ActionResult PerformAction()
        {
            if(onPerformAction != null)
                onPerformAction(this);
            return PerformActionImplementation();
        }

        /// <summary>
        /// Performs the action for this menu item. Use this method to implement the action and then
        /// use <see cref="PerformAction"/> to call it.
        /// </summary>
        /// <returns>A new ActionResult with a summary of the state of the action's success.</returns>
        protected abstract ActionResult PerformActionImplementation();

        const string obsoleteDoActionMsg = "DoAction() has been replaced by PerformAction(), the implementation of the action should inherit from PerformActionImplementation(). (UnityUpgradable) -> PerformAction()";
        /// <summary>
        /// Performs whatever action this menu item is supposed to do.
        /// </summary>
        /// <returns>A new ActionResult with a summary of the state of the action's success.</returns>
        [Obsolete(obsoleteDoActionMsg, false)]
        public ActionResult DoAction() => PerformAction();


        /// <summary>
        /// Performs the action for this menu item when in Text mode.
        /// </summary>
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

        /// <summary>
        /// Gets or sets whether the [Toolbar display mode](../manual/toolbar.html#toolbar-display-modes)
        /// is set to Icon mode (true) or Text mode (false).
        /// </summary>
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

        /// <summary>
        /// Draws the menu item for this action in Text mode.
        /// </summary>
        /// <param name="options">Optional array of layout options for this menu item.</param>
        /// <returns>True if successful; false otherwise.</returns>
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

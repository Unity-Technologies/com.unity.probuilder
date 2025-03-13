// #define GENERATE_DESATURATED_ICONS

using System;
using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEngine.UIElements;

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
        [Flags]
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

        /// <summary>
        /// Invoked when the user selects an action to perform from the toolbar.
        /// </summary>
        public static Action<MenuAction> onPerformAction;

        /// <summary>
        /// Creates a new button on the [ProBuilder toolbar](../manual/toolbar.html) in the Editor.
        /// </summary>
        protected MenuAction(){}

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

                return -1;
            }

            if (right == null)
                return 1;

            int l = (int)left.group, r = (int)right.group;

            if (l < r)
                return -1;
            if (l > r)
                return 1;

            int lp = left.toolbarPriority < 0 ? int.MaxValue : left.toolbarPriority,
                rp = right.toolbarPriority < 0 ? int.MaxValue : right.toolbarPriority;

            return lp.CompareTo(rp);
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
        /// Gets the icon to display in the Context Menu for this action.
        /// </summary>
        public abstract Texture2D icon { get; }

        /// <summary>
        /// Gets the local path of the icon to display in the Context Menu for this action.
        /// </summary>
        public abstract string iconPath { get; }

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
        protected internal virtual bool hasFileMenuEntry { get { return true; } }

        /// <summary>
        /// Gets a flag that indicates both the visibility and enabled state of an action
        /// to determine whether the current mode and selection is valid for it.
        /// </summary>
        /// <value>Flag indicating current state of the MenuItem.</value>
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
            {;
                var b1 = ProBuilderEditor.selectMode.ContainsFlag(validSelectModes);
                var b2 = !ProBuilderEditor.selectMode.ContainsFlag(SelectMode.InputTool);
                return b1 && b2;
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

        internal bool optionsVisible => optionsMenuState != MenuActionState.Hidden;
        internal bool optionsEnabled => optionsMenuState == MenuActionState.VisibleAndEnabled;

        /// <summary>
        /// Adds a checkmark status to the MenuItem generated in the GenerateMenuItems.cs script.
        /// </summary>
        /// <returns></returns>
        internal virtual bool IsMenuItemChecked() => false;

        /// <summary>
        /// Override the action automatically generated in the GenerateMenuItems.cs script.
        /// </summary>
        /// <returns></returns>
        internal virtual string GetMenuItemOverride() => "";

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
            var res = PerformActionImplementation();
            ContentsChanged();
            return res;
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
        /// Performs the action for this menu item when in Text mode.
        /// </summary>
        public void PerformAltAction() => DoAlternateAction();

        /// <summary>
        /// Replaces OnSettingsGUI for 2023.2 and newer to display settings in a SceneView overlay.
        /// Creates a custom settings window for this action. Populate a root visual element in that method with
        /// the settings content.
        /// </summary>
        /// <returns>A VisualElement containing settings content.</returns>
        public virtual VisualElement CreateSettingsContent()
        {
            return null;
        }

        /// <summary>
        /// If extra handles or gizmos are needed during the action execution in the scene, implement them here.
        /// </summary>
        /// <param name="sceneView">SceneView for which the DoSceneGUI method is called.</param>
        public virtual void DoSceneGUI(SceneView sceneView) {}

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
        /// Draws the menu item for this action in Text mode.
        /// </summary>
        /// <param name="options">Optional array of layout options for this menu item.</param>
        /// <returns>True if successful; false otherwise.</returns>
        [Obsolete]
        protected bool DoAltButton(params GUILayoutOption[] options)
        {
            return false;
        }

        /// <summary>
        /// Raised when MenuAction contents change.
        /// </summary>
        public event Action changed;

        /// <summary>
        /// Called during PerformAction.
        /// Calling this method triggers the <see cref="changed"/> event.
        /// </summary>
        protected void ContentsChanged() => changed?.Invoke();

        /// <summary>
        /// Override to register <see cref="ContentsChanged"/> to event callbacks.
        /// </summary>
        public virtual void RegisterChangedCallbacks() { }

        /// <summary>
        /// Override to unregister <see cref="ContentsChanged"/> from event callbacks.
        /// </summary>
        public virtual void UnregisterChangedCallbacks() { }
    }
}

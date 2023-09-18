using UnityEditor.ProBuilder.UI;
using UnityEditor.SettingsManagement;
using UnityEngine;

namespace UnityEditor.ProBuilder
{
    sealed class ProBuilderWindow : EditorWindow, IHasCustomMenu
    {
        static Pref<bool> s_WindowIsFloating = new Pref<bool>("UnityEngine.ProBuilder.ProBuilderEditor-isUtilityWindow",
            false, SettingsScope.Project);

        [UserSetting("Toolbar", "Icon GUI", "Toggles the ProBuilder window interface between text and icon versions.")]
        static Pref<bool> s_IsIconGui = new Pref<bool>("editor.toolbarIconGUI", false);

        // if the ratio is 1/2 height/width then switch to horizontal mode
        bool horizontalMode => position.height / position.width < .5;

        public static void MenuOpenWindow()
        {
            GetWindow<ProBuilderWindow>(s_WindowIsFloating, "ProBuilder", true);
        }

        public void SetIconMode(bool iconMode)
        {
            s_IsIconGui.SetValue(iconMode);
            CreateGUI();
        }

        void CreateGUI()
        {
            rootVisualElement.Clear();
            rootVisualElement.Add(new ProBuilderToolbar(s_IsIconGui, horizontalMode));
        }

        /// <summary>
        /// Builds the context menu for the ProBuilder toolbar. This menu allows the user to toggle between
        /// text and button mode, and to change whether the toolbar is floating or dockable.
        /// </summary>
        /// <param name="menu">The context menu</param>
        public void AddItemsToMenu(GenericMenu menu)
        {
            bool floating = s_WindowIsFloating;

            menu.AddItem(new GUIContent("Window/Open as Floating Window", ""), floating, () => SetIsUtilityWindow(true));
            menu.AddItem(new GUIContent("Window/Open as Dockable Window", ""), !floating, () => SetIsUtilityWindow(false));
            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Use Icon Mode", ""), s_IsIconGui, () => { SetIconMode(true); });
            menu.AddItem(new GUIContent("Use Text Mode", ""), !s_IsIconGui, () => { SetIconMode(false); });
        }

        void SetIsUtilityWindow(bool isUtilityWindow)
        {
            s_WindowIsFloating.value = isUtilityWindow;
            var windowTitle = titleContent;
            Close();
            var res = GetWindow(GetType(), isUtilityWindow);
            res.titleContent = windowTitle;
        }
    }
}

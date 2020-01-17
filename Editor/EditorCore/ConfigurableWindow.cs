using UnityEngine;

namespace UnityEditor.ProBuilder
{
    abstract class ConfigurableWindow : EditorWindow, IHasCustomMenu
    {
        protected virtual bool defaultIsUtilityWindow
        {
            get { return false; }
        }

        string utilityWindowKey
        {
            get { return GetType().ToString() + "-isUtilityWindow"; }
        }

        protected static bool IsUtilityWindow<T>(bool defaultIsUtility = false) where T : ConfigurableWindow
        {
            return ProBuilderSettings.Get<bool>(typeof(T).ToString() + "-isUtilityWindow", SettingsScope.Project, defaultIsUtility);
        }

        public static new T GetWindow<T>(string title, bool focus = true) where T : ConfigurableWindow
        {
            return EditorWindow.GetWindow<T>(IsUtilityWindow<T>(), title, focus);
        }

        /// <summary>
        /// Get or create an instance of EditorWindow. Note that `utility` may be overridden by user set preference.
        /// </summary>
        public static new T GetWindow<T>(bool utility, string title, bool focus) where T : ConfigurableWindow
        {
            return EditorWindow.GetWindow<T>(IsUtilityWindow<T>(utility), title, focus);
        }

        public virtual void AddItemsToMenu(GenericMenu menu)
        {
            bool floating = ProBuilderSettings.Get<bool>(utilityWindowKey, SettingsScope.Project, false);

            if (menu.GetItemCount() > 1)
                menu.AddSeparator("");

            menu.AddItem(new GUIContent("Open as Floating Window", ""), floating, () => SetIsUtilityWindow(true));
            menu.AddItem(new GUIContent("Open as Dockable Window", ""), !floating, () => SetIsUtilityWindow(false));

            menu.AddSeparator("");
        }

        protected void DoContextMenu()
        {
            var e = Event.current;

            if (e.type == EventType.ContextClick)
            {
                var menu = new GenericMenu();
                AddItemsToMenu(menu);
                menu.ShowAsContext();
            }
        }

        void SetIsUtilityWindow(bool isUtilityWindow)
        {
            ProBuilderSettings.Set<bool>(utilityWindowKey, isUtilityWindow, SettingsScope.Project);
            ProBuilderSettings.Save();
            var title = titleContent;
            Close();
            var res = GetWindow(GetType(), isUtilityWindow);
            res.titleContent = title;
        }
    }
}

using UnityEngine;
using UnityEngine.ProBuilder;
using UnityEditor.SettingsManagement;

namespace UnityEditor.ProBuilder
{
    abstract class ConfigurableWindow : EditorWindow, IHasCustomMenu
    {
        string utilityWindowKey
        {
            get { return GetType().ToString() + "-isUtilityWindow"; }
        }

        protected static bool IsUtilityWindow<T>() where T : ConfigurableWindow
        {
            return ProBuilderSettings.Get<bool>(typeof(T).ToString() + "-isUtilityWindow", SettingsScope.Project, false);
        }

        public static new T GetWindow<T>(string title, bool focus = true) where T : ConfigurableWindow
        {
            return GetWindow<T>(IsUtilityWindow<T>(), title, focus);
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

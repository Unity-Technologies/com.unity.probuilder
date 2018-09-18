using UnityEngine;
using UnityEngine.ProBuilder;

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
			return Settings.Get<bool>(typeof(T).ToString() + "-isUtilityWindow", Settings.Scope.Project, false);
		}

		public static new T GetWindow<T>(string title, bool focus = true) where T : ConfigurableWindow
		{
			return GetWindow<T>(IsUtilityWindow<T>(), title, focus);
		}

		public virtual void AddItemsToMenu(GenericMenu menu)
		{
			bool floating = Settings.Get<bool>(utilityWindowKey, Settings.Scope.Project, false);
			menu.AddItem(new GUIContent("Window/Open as Floating Window", ""), floating, () => SetIsUtilityWindow(true) );
			menu.AddItem(new GUIContent("Window/Open as Dockable Window", ""), !floating, () => SetIsUtilityWindow(false) );
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
			Settings.Set<bool>(utilityWindowKey, isUtilityWindow, Settings.Scope.Project);
			var title = titleContent;
			Close();
			var res = GetWindow(GetType(), isUtilityWindow);
			res.titleContent = title;
		}
	}
}

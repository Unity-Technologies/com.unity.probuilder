using System;
using System.Linq;
using UnityEngine;

namespace UnityEditor.SettingsManagement
{
	public static class SettingsGUILayout
	{
		public static float SearchableSlider(GUIContent content, float value, float min, float max, string searchContext)
		{
			if (!MatchSearchGroups(searchContext, content.text))
				return value;
			return UnityEditor.EditorGUILayout.Slider(content, value, min, max);
		}

		public static float SearchableSlider(string content, float value, float min, float max, string searchContext)
		{
			if (!MatchSearchGroups(searchContext, content))
				return value;
			return UnityEditor.EditorGUILayout.Slider(content, value, min, max);
		}

		internal static float SearchableFloatField(GUIContent title, float value, string searchContext)
		{
			if(!MatchSearchGroups(searchContext, title.text))
				return value;
			return UnityEditor.EditorGUILayout.FloatField(title, value);
		}

		internal static float SearchableFloatField(string title, float value, string searchContext)
		{
			if(!MatchSearchGroups(searchContext, title))
				return value;
			return UnityEditor.EditorGUILayout.FloatField(title, value);
		}

		internal static int SearchableIntField(GUIContent title, int value, string searchContext)
		{
			if(!MatchSearchGroups(searchContext, title.text))
				return value;
			return UnityEditor.EditorGUILayout.IntField(title, value);
		}

		internal static int SearchableIntField(string title, int value, string searchContext)
		{
			if(!MatchSearchGroups(searchContext, title))
				return value;
			return UnityEditor.EditorGUILayout.IntField(title, value);
		}

		internal static bool SearchableToggle(GUIContent title, bool value, string searchContext)
		{
			if(!MatchSearchGroups(searchContext, title.text))
				return value;
			return UnityEditor.EditorGUILayout.Toggle(title, value);
		}

		internal static bool SearchableToggle(string title, bool value, string searchContext)
		{
			if(!MatchSearchGroups(searchContext, title))
				return value;
			return UnityEditor.EditorGUILayout.Toggle(title, value);
		}

		internal static string SearchableTextField(GUIContent title, string value, string searchContext)
		{
			if(!MatchSearchGroups(searchContext, title.text))
				return value;
			return UnityEditor.EditorGUILayout.TextField(title, value);
		}

		internal static string SearchableTextField(string title, string value, string searchContext)
		{
			if(!MatchSearchGroups(searchContext, title))
				return value;
			return UnityEditor.EditorGUILayout.TextField(title, value);
		}

		internal static Color SearchableColorField(GUIContent title, Color value, string searchContext)
		{
			if(!MatchSearchGroups(searchContext, title.text))
				return value;
			return UnityEditor.EditorGUILayout.ColorField(title, value);
		}

		internal static Color SearchableColorField(string title, Color value, string searchContext)
		{
			if (!MatchSearchGroups(searchContext, title))
				return value;
			return UnityEditor.EditorGUILayout.ColorField(title, value);
		}

		internal static bool MatchSearchGroups(string searchContext, string content)
		{
			if (searchContext == null)
				return true;
			var ctx = searchContext.Trim();
			if (string.IsNullOrEmpty(ctx))
				return true;
			var split = searchContext.Split(' ');
			return split.Any(x => !string.IsNullOrEmpty(x) && content.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) > -1);
		}

		internal static bool DebugModeFilter(IUserSetting pref)
		{
			if (!EditorPrefs.GetBool("DeveloperMode", false))
				return true;

			if (pref.scope == SettingScope.Project && UserSettingsProvider.showProjectSettings)
				return true;

			if (pref.scope == SettingScope.User && UserSettingsProvider.showUserSettings)
				return true;

			return false;
		}

		public static float SettingsSlider(GUIContent content, UserSetting<float> value, float min, float max, string searchContext)
		{
			if (!DebugModeFilter(value) || !MatchSearchGroups(searchContext, content.text))
				return value;
			var res = UnityEditor.EditorGUILayout.Slider(content, value, min, max);
			DoResetContextMenuForLastRect(value);
			return res;
		}

		public static float SettingsSlider(string content, UserSetting<float> value, float min, float max, string searchContext)
		{
			if (!DebugModeFilter(value) || !MatchSearchGroups(searchContext, content))
				return value;
			var res = UnityEditor.EditorGUILayout.Slider(content, value, min, max);
			DoResetContextMenuForLastRect(value);
			return res;
		}

		public static float SettingsFloatField(GUIContent title, UserSetting<float> value, string searchContext)
		{
			if(!DebugModeFilter(value) || !MatchSearchGroups(searchContext, title.text))
				return value;
			var res = UnityEditor.EditorGUILayout.FloatField(title, value);
			DoResetContextMenuForLastRect(value);
			return res;
		}

		public static float SettingsFloatField(string title, UserSetting<float> value, string searchContext)
		{
			if(!DebugModeFilter(value) || !MatchSearchGroups(searchContext, title))
				return value;
			var res = UnityEditor.EditorGUILayout.FloatField(title, value);
			DoResetContextMenuForLastRect(value);
			return res;
		}

		public static int SettingsIntField(GUIContent title, UserSetting<int> value, string searchContext)
		{
			if(!DebugModeFilter(value) || !MatchSearchGroups(searchContext, title.text))
				return value;
			var res = UnityEditor.EditorGUILayout.IntField(title, value);
			DoResetContextMenuForLastRect(value);
			return res;
		}

		public static int SettingsIntField(string title, UserSetting<int> value, string searchContext)
		{
			if(!DebugModeFilter(value) || !MatchSearchGroups(searchContext, title))
				return value;
			var res = UnityEditor.EditorGUILayout.IntField(title, value);
			DoResetContextMenuForLastRect(value);
			return res;
		}

		public static bool SettingsToggle(GUIContent title, UserSetting<bool> value, string searchContext)
		{
			if(!DebugModeFilter(value) || !MatchSearchGroups(searchContext, title.text))
				return value;
			var res = UnityEditor.EditorGUILayout.Toggle(title, value);
			DoResetContextMenuForLastRect(value);
			return res;
		}

		public static bool SettingsToggle(string title, UserSetting<bool> value, string searchContext)
		{
			if(!DebugModeFilter(value) || !MatchSearchGroups(searchContext, title))
				return value;
			var res = UnityEditor.EditorGUILayout.Toggle(title, value);
			DoResetContextMenuForLastRect(value);
			return res;
		}

		public static string SettingsTextField(GUIContent title, UserSetting<string> value, string searchContext)
		{
			if(!DebugModeFilter(value) || !MatchSearchGroups(searchContext, title.text))
				return value;
			var res = UnityEditor.EditorGUILayout.TextField(title, value);
			DoResetContextMenuForLastRect(value);
			return res;
		}

		public static string SettingsTextField(string title, UserSetting<string> value, string searchContext)
		{
			if(!DebugModeFilter(value) || !MatchSearchGroups(searchContext, title))
				return value;
			var res = UnityEditor.EditorGUILayout.TextField(title, value);
			DoResetContextMenuForLastRect(value);
			return res;
		}

		public static Color SettingsColorField(GUIContent title, UserSetting<Color> value, string searchContext)
		{
			if(!DebugModeFilter(value) || !MatchSearchGroups(searchContext, title.text))
				return value;
			var res = UnityEditor.EditorGUILayout.ColorField(title, value);
			DoResetContextMenuForLastRect(value);
			return res;
		}

		public static Color SettingsColorField(string title, UserSetting<Color> value, string searchContext)
		{
			if (!DebugModeFilter(value) || !MatchSearchGroups(searchContext, title))
				return value;
			var res = UnityEditor.EditorGUILayout.ColorField(title, value);
			DoResetContextMenuForLastRect(value);
			return res;
		}

		public static void DoResetContextMenuForLastRect(IUserSetting pref)
		{
			DoResetContextMenu(GUILayoutUtility.GetLastRect(), pref);
		}

		public static void DoResetContextMenu(Rect rect, IUserSetting pref)
		{
			var evt = Event.current;

			if (evt.type == EventType.ContextClick && rect.Contains(evt.mousePosition))
			{
				var menu = new GenericMenu();
				menu.AddItem(new GUIContent("Reset [" + pref.scope +"] " + pref.key), false, () =>
				{
					pref.Reset(true);
				});
				menu.ShowAsContext();
			}
		}
	}
}

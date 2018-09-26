using UnityEditor.Settings;
using UnitySettings = UnityEditor.Settings.Settings;

namespace UnityEditor.ProBuilder
{
	public class Pref<T> : UserSetting<T>
	{
		public Pref(string key, T value, SettingScope scope = SettingScope.Project)
		: base(ProBuilderSettings.instance, key, value, scope)
		{}

		public Pref(UnitySettings settings, string key, T value, SettingScope scope = SettingScope.Project)
			: base(settings, key, value, scope) { }
	}
}

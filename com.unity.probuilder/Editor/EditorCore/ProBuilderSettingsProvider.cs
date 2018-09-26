using UnityEditor;
using UnityEditor.Settings;
using UnitySettings = UnityEditor.Settings.Settings;

namespace UnityEditor.ProBuilder
{
	static class ProBuilderSettingsProvider
	{
		[SettingsProvider]
		static SettingsProvider CreateSettingsProvider()
		{
			var provider = new UserSettingsProvider("Preferences/ProBuilder",
				ProBuilderSettings.instance,
				new [] { typeof(ProBuilderSettingsProvider).Assembly });

			provider.afterSettingsSaved += () =>
			{
				if (ProBuilderEditor.instance != null)
					ProBuilderEditor.instance.OnEnable();
			};

			return provider;
		}
	}
}

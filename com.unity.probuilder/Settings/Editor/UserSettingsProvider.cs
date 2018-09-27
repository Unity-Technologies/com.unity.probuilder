#if UNITY_2018_3_OR_NEWER
#define SETTINGS_PROVIDER_ENABLED
#endif

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace UnityEditor.SettingsManagement
{
#if SETTINGS_PROVIDER_ENABLED
	public sealed class UserSettingsProvider : SettingsProvider
#else
	public sealed class UserSettingsProvider
#endif
	{
		const string k_UserSettingsProviderSettingsPath = "ProjectSettings/UserSettingsProviderSettings.json";
		const string k_SettingsGearIcon = "Packages/com.unity.probuilder/Settings/Content/Options.png";
#if SETTINGS_PROVIDER_ENABLED
		const int k_LabelWidth = 240;
#else
		const int k_LabelWidth = 180;
#endif

		List<string> m_Categories;
		Dictionary<string, List<PrefEntry>> m_Settings;
		Dictionary<string, List<MethodInfo>> m_SettingBlocks;
#if !SETTINGS_PROVIDER_ENABLED
		HashSet<string> keywords = new HashSet<string>();
#endif
		static readonly string[] s_SearchContext = new string[1];
		Assembly[] m_Assemblies;
		static Settings s_Settings;
		Settings m_SettingsInstance;
		public event Action afterSettingsSaved;

		static Settings userSettingsProviderSettings
		{
			get
			{
				if(s_Settings == null)
					s_Settings = new Settings(k_UserSettingsProviderSettingsPath);
				return s_Settings;
			}
		}

		public Settings settingsInstance
		{
			get { return m_SettingsInstance; }
		}

		internal static UserSetting<bool> showHiddenSettings = new UserSetting<bool>(userSettingsProviderSettings, "settings.showHidden", false, SettingScope.User);
		internal static UserSetting<bool> showUnregisteredSettings = new UserSetting<bool>(userSettingsProviderSettings, "settings.showUnregistered", false, SettingScope.User);
		internal static UserSetting<bool> listByKey = new UserSetting<bool>(userSettingsProviderSettings, "settings.listByKey", false, SettingScope.User);
		internal static UserSetting<bool> showUserSettings = new UserSetting<bool>(userSettingsProviderSettings, "settings.showUserSettings", false, SettingScope.User);
		internal static UserSetting<bool> showProjectSettings = new UserSetting<bool>(userSettingsProviderSettings, "settings.showProjectSettings", false, SettingScope.User);

		static class Styles
		{
			static bool s_Initialized;

			public static GUIStyle settingsArea;
			public static GUIStyle settingsGizmo;

			public static void Init()
			{
				if (s_Initialized)
					return;

				s_Initialized = true;

				settingsArea = new GUIStyle()
				{
					margin = new RectOffset(6, 6, 0, 0)
				};

				settingsGizmo = new GUIStyle()
				{
					normal = new GUIStyleState()
					{
						background = AssetDatabase.LoadAssetAtPath<Texture2D>(k_SettingsGearIcon)
					},
					fixedWidth = 14,
					fixedHeight = 14,
					padding = new RectOffset(0,0,0,0),
					margin = new RectOffset(4,4,4,4),
					imagePosition = ImagePosition.ImageOnly
				};
			}
		}

#if SETTINGS_PROVIDER_ENABLED
		public UserSettingsProvider(string path, Settings settings, Assembly[] assemblies, SettingsScopes scopes = SettingsScopes.Any)
			: base(path, scopes)
#else
		public UserSettingsProvider(Settings settings, Assembly[] assemblies)
#endif
		{
			if(settings == null)
				throw new ArgumentNullException("settings");

			if(assemblies == null)
				throw new ArgumentNullException("assemblies");

			m_SettingsInstance = settings;
			m_Assemblies = assemblies;
			m_SettingsInstance.afterSettingsSaved += OnAfterSettingsSaved;

#if !SETTINGS_PROVIDER_ENABLED
			SearchForUserSettingAttributes();
#endif
		}

		~UserSettingsProvider()
		{
			m_SettingsInstance.afterSettingsSaved -= OnAfterSettingsSaved;
		}

#if SETTINGS_PROVIDER_ENABLED
		public override void OnActivate(string searchContext, VisualElement rootElement)
		{
			SearchForUserSettingAttributes();
		}
#endif

		void OnAfterSettingsSaved()
		{
			if (afterSettingsSaved != null)
				afterSettingsSaved();
		}

		struct PrefEntry
		{
			GUIContent m_Content;
			IUserSetting m_Pref;

			public GUIContent content
			{
				get { return m_Content; }
			}

			public IUserSetting pref
			{
				get { return m_Pref; }
			}

			public PrefEntry(GUIContent content, IUserSetting pref)
			{
				m_Content = content;
				m_Pref = pref;
			}
		}

		void SearchForUserSettingAttributes()
		{
			keywords.Clear();

			if(m_Settings != null)
				m_Settings.Clear();
			else
				m_Settings = new Dictionary<string, List<PrefEntry>>();

			if(m_SettingBlocks != null)
				m_SettingBlocks.Clear();
			else
				m_SettingBlocks = new Dictionary<string, List<MethodInfo>>();

			var types = m_Assemblies.SelectMany(x => x.GetTypes());

			// collect instance fields/methods too, but only so we can throw a warning that they're invalid.
			var fields = types.SelectMany(x =>
				x.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
					.Where(prop => Attribute.IsDefined(prop, typeof(UserSettingAttribute))));

			var methods = types.SelectMany(x => x.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
				.Where(y => Attribute.IsDefined(y, typeof(UserSettingBlockAttribute))));

			foreach (var field in fields)
			{
				if (!field.IsStatic)
				{
					Debug.LogWarning("Cannot create setting entries for instance fields. Skipping \"" + field.Name + "\".");
					continue;
				}

				var attrib = (UserSettingAttribute) Attribute.GetCustomAttribute(field, typeof(UserSettingAttribute));

				if (!attrib.visibleInSettingsProvider)
					continue;

				var pref = (IUserSetting)field.GetValue(null);

				if (pref == null)
				{
					Debug.LogWarning("[UserSettingAttribute] is only valid for types inheriting Pref<T>. Skipping \"" + field.Name + "\"");
					continue;
				}

				var category = string.IsNullOrEmpty(attrib.category) ? "Uncategorized" : attrib.category;
				var content = listByKey ? new GUIContent(pref.key) : attrib.title;

				List<PrefEntry> settings;

				if (m_Settings.TryGetValue(category, out settings))
					settings.Add(new PrefEntry(content, pref));
				else
					m_Settings.Add(category, new List<PrefEntry>() { new PrefEntry(content, pref) });
			}

			foreach (var method in methods)
			{
				var attrib = (UserSettingBlockAttribute) Attribute.GetCustomAttribute(method, typeof(UserSettingBlockAttribute));
				var category = string.IsNullOrEmpty(attrib.category) ? "Uncategorized" : attrib.category;
				List<MethodInfo> blocks;

				var parameters = method.GetParameters();

				if (!method.IsStatic || parameters.Length < 1 || parameters[0].ParameterType != typeof(string))
				{
					Debug.LogWarning("[UserSettingBlockAttribute] is only valid for static functions with a single string parameter. Ex, `static void MySettings(string searchContext)`. Skipping \"" + method.Name + "\"");
					continue;
				}

				if (m_SettingBlocks.TryGetValue(category, out blocks))
					blocks.Add(method);
				else
					m_SettingBlocks.Add(category, new List<MethodInfo>() { method });

				if (attrib.keywords != null)
				{
					foreach (var word in attrib.keywords)
						keywords.Add(word);
				}
			}

			if (showHiddenSettings)
			{
				var unlisted = new List<PrefEntry>();
				m_Settings.Add("Unlisted", unlisted);
				foreach (var pref in UserSettings.FindUserSettings(m_Assemblies, SettingVisibility.Unlisted | SettingVisibility.Hidden))
					unlisted.Add(new PrefEntry( new GUIContent(pref.key), pref ));
			}

			if (showUnregisteredSettings)
			{
				var unregistered = new List<PrefEntry>();
				m_Settings.Add("Unregistered", unregistered);
				foreach (var pref in UserSettings.FindUserSettings(m_Assemblies, SettingVisibility.Unregistered))
					unregistered.Add(new PrefEntry( new GUIContent(pref.key), pref ));
			}

			foreach (var cat in m_Settings)
			{
				foreach (var entry in cat.Value)
				{
					var content = entry.content;

					if(content != null && !string.IsNullOrEmpty(content.text))
					{
						foreach (var word in content.text.Split(' '))
							keywords.Add(word);
					}
				}
			}

			m_Categories = m_Settings.Keys.Union(m_SettingBlocks.Keys).ToList();
			m_Categories.Sort();
		}

#if SETTINGS_PROVIDER_ENABLED
		public override void OnTitleBarGUI()
		{
			Styles.Init();

			if (GUILayout.Button(GUIContent.none, Styles.settingsGizmo))
				DoContextMenu();
		}
#endif

		void DoContextMenu()
		{
			var menu = new GenericMenu();

			if (EditorPrefs.GetBool("DeveloperMode", false))
			{
				menu.AddItem(new GUIContent("Refresh"), false, SearchForUserSettingAttributes);

				menu.AddSeparator("");

				menu.AddItem(new GUIContent("List Settings By Key"), listByKey, () =>
				{
					listByKey.SetValue(!listByKey, true);
					SearchForUserSettingAttributes();
				});

				menu.AddSeparator("");

				menu.AddItem(new GUIContent("Show User Settings"), showUserSettings, () =>
				{
					showUserSettings.SetValue(!showUserSettings, true);
					SearchForUserSettingAttributes();
				});

				menu.AddItem(new GUIContent("Show Project Settings"), showProjectSettings, () =>
				{
					showProjectSettings.SetValue(!showProjectSettings, true);
					SearchForUserSettingAttributes();
				});

				menu.AddSeparator("");

				menu.AddItem(new GUIContent("Show Unlisted Settings"), showHiddenSettings, () =>
				{
					showHiddenSettings.SetValue(!showHiddenSettings, true);
					SearchForUserSettingAttributes();
				});

				menu.AddItem(new GUIContent("Show Unregistered Settings"), showUnregisteredSettings, () =>
				{
					showUnregisteredSettings.SetValue(!showUnregisteredSettings, true);
					SearchForUserSettingAttributes();
				});

				menu.AddSeparator("");

				menu.AddItem(new GUIContent("Open Project Settings File"), false, () =>
				{
					var path = Path.GetFullPath(settingsInstance.settingsPath);
					System.Diagnostics.Process.Start(path);
				});

				menu.AddItem(new GUIContent("Print All Settings"), false, () =>
				{
					Debug.Log(UserSettings.GetSettingsString(m_Assemblies));
				});

				menu.AddSeparator("");
			}

			menu.AddItem(new GUIContent("Reset All"), false, () =>
			{
				if (!UnityEditor.EditorUtility.DisplayDialog("Reset All Settings", "Reset all ProBuilder settings? This is not undo-able.", "Reset", "Cancel"))
					return;

				// Do not reset SettingVisibility.Unregistered
				foreach (var pref in UserSettings.FindUserSettings(m_Assemblies, SettingVisibility.Visible | SettingVisibility.Hidden | SettingVisibility.Unlisted))
					pref.Reset();

				settingsInstance.Save();
			});

			menu.ShowAsContext();
		}

#if SETTINGS_PROVIDER_ENABLED
		public override void OnGUI(string searchContext)
#else
		public void OnGUI(string searchContext)
#endif
		{
			Styles.Init();

#if !SETTINGS_PROVIDER_ENABLED
			var evt = Event.current;
			if(evt.type == EventType.ContextClick)
				DoContextMenu();
#endif

			EditorGUIUtility.labelWidth = k_LabelWidth;

			EditorGUI.BeginChangeCheck();
			GUILayout.BeginVertical(Styles.settingsArea);

			var hasSearchContext = !string.IsNullOrEmpty(searchContext);
			s_SearchContext[0] = searchContext;
			if (hasSearchContext)
			{
				// todo - Improve search comparison
				var searchKeywords = searchContext.Split(' ');

				foreach (var settingField in m_Settings)
				foreach (var setting in settingField.Value)
					if (searchKeywords.Any(x => !string.IsNullOrEmpty(x) && setting.content.text.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) > -1))
						DoPreferenceField(setting.content, setting.pref);

				foreach (var settingsBlock in m_SettingBlocks)
				foreach (var block in settingsBlock.Value)
					block.Invoke(null, s_SearchContext);
			}
			else
			{
				foreach (var key in m_Categories)
				{
					GUILayout.Label(key, EditorStyles.boldLabel);

					List<PrefEntry> settings;

					if (m_Settings.TryGetValue(key, out settings))
						foreach (var setting in settings)
							DoPreferenceField(setting.content, setting.pref);

					List<MethodInfo> blocks;

					if (m_SettingBlocks.TryGetValue(key, out blocks))
						foreach (var block in blocks)
							block.Invoke(null, s_SearchContext);

					GUILayout.Space(8);
				}
			}

			EditorGUIUtility.labelWidth = 0;

			GUILayout.EndVertical();

			if (EditorGUI.EndChangeCheck())
			{
				settingsInstance.Save();
			}
		}

		void DoPreferenceField(GUIContent title, IUserSetting pref)
		{
			if (EditorPrefs.GetBool("DeveloperMode", false))
			{
				if (pref.scope == SettingScope.Project && !showProjectSettings)
					return;
				if (pref.scope == SettingScope.User && !showUserSettings)
					return;
			}

			if (pref is UserSetting<float>)
			{
				var cast = (UserSetting<float>)pref;
				cast.value = EditorGUILayout.FloatField(title, cast.value);
			}
			else if (pref is UserSetting<int>)
			{
				var cast = (UserSetting<int>)pref;
				cast.value = EditorGUILayout.IntField(title, cast.value);
			}
			else if (pref is UserSetting<bool>)
			{
				var cast = (UserSetting<bool>)pref;
				cast.value = EditorGUILayout.Toggle(title, cast.value);
			}
			else if (pref is UserSetting<string>)
			{
				var cast = (UserSetting<string>)pref;
				cast.value = EditorGUILayout.TextField(title, cast.value);
			}
			else if (pref is UserSetting<Color>)
			{
				var cast = (UserSetting<Color>)pref;
				cast.value = EditorGUILayout.ColorField(title, cast.value);
			}
			else if (typeof(Enum).IsAssignableFrom(pref.type))
			{
				Enum val = (Enum)pref.GetValue();
				EditorGUI.BeginChangeCheck();
				if (Attribute.IsDefined(pref.type, typeof(FlagsAttribute)))
					val = EditorGUILayout.EnumFlagsField(title, val);
				else
					val = EditorGUILayout.EnumPopup(title, val);
				if (EditorGUI.EndChangeCheck())
					pref.SetValue(val);
			}
			else if (typeof(UnityEngine.Object).IsAssignableFrom(pref.type))
			{
				var obj = (UnityEngine.Object)pref.GetValue();
				EditorGUI.BeginChangeCheck();
				obj = EditorGUILayout.ObjectField(title, obj, pref.type, false);
				if (EditorGUI.EndChangeCheck())
					pref.SetValue(obj);
			}
			else
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(title, GUILayout.Width(EditorGUIUtility.labelWidth - EditorStyles.label.margin.right * 2));
				var obj = pref.GetValue();
				GUILayout.Label(obj == null ? "null" : pref.GetValue().ToString());
				GUILayout.EndHorizontal();
			}

			SettingsGUILayout.DoResetContextMenuForLastRect(pref);
		}
	}
}
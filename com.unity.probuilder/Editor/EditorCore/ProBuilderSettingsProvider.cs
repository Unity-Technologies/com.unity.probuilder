#if UNITY_2018_3_OR_NEWER
#define SETTINGS_PROVIDER_ENABLED
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
#if SETTINGS_PROVIDER_ENABLED
	sealed class ProBuilderSettingsProvider : SettingsProvider
#else
	sealed class ProBuilderSettingsProvider
#endif
	{
#if SETTINGS_PROVIDER_ENABLED
		List<string> m_Categories;
		Dictionary<string, List<SimpleTuple<GUIContent, IPref>>> m_Settings;
		Dictionary<string, List<MethodInfo>> m_SettingBlocks;
		static readonly string[] s_SearchContext = new string[1];
		static Pref<bool> s_ShowHiddenSettings = new Pref<bool>("settings.showHidden", false, Settings.Scope.User);
		static Pref<bool> s_ShowUnregisteredSettings = new Pref<bool>("settings.showUnregistered", false, Settings.Scope.User);
#else
		static List<string> m_Categories;
		static Dictionary<string, List<SimpleTuple<GUIContent, IPref>>> m_Settings;
		static Dictionary<string, List<MethodInfo>> m_SettingBlocks;
		static readonly string[] s_SearchContext = new string[1];
		static HashSet<string> keywords = new HashSet<string>();
		static bool s_Initialized;
#endif

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
						background = IconUtility.GetIcon("Toolbar/Options", IconSkin.Pro)
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
		[SettingsProvider]
		static SettingsProvider CreateSettingsProvider()
		{
			return new ProBuilderSettingsProvider("Preferences/ProBuilder");
		}

		public ProBuilderSettingsProvider(string path, SettingsScopes scopes = SettingsScopes.Any)
			: base(path, scopes)
		{
		}

		public override void OnActivate(string searchContext, VisualElement rootElement)
		{
			SearchForUserSettingAttributes();
		}
#else

		static void Init()
		{
			if (s_Initialized)
				return;
			s_Initialized = true;
			SearchForUserSettingAttributes();
		}
#endif

#if SETTINGS_PROVIDER_ENABLED
		void SearchForUserSettingAttributes()
#else
		static void SearchForUserSettingAttributes()
#endif
		{
			keywords.Clear();

			if(m_Settings != null)
				m_Settings.Clear();
			else
				m_Settings = new Dictionary<string, List<SimpleTuple<GUIContent, IPref>>>();

			if(m_SettingBlocks != null)
				m_SettingBlocks.Clear();
			else
				m_SettingBlocks = new Dictionary<string, List<MethodInfo>>();

			var types = typeof(ProBuilderSettingsProvider).Assembly.GetTypes();

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
					Log.Warning("Cannot create setting entries for instance fields. Skipping \"" + field.Name + "\".");
					continue;
				}

				var attrib = (UserSettingAttribute) Attribute.GetCustomAttribute(field, typeof(UserSettingAttribute));

				if (!attrib.visibleInSettingsProvider)
					continue;

				var pref = (IPref)field.GetValue(null);

				if (pref == null)
				{
					Log.Warning("[UserSettingAttribute] is only valid for types inheriting Pref<T>. Skipping \"" + field.Name + "\"");
					continue;
				}

				var category = string.IsNullOrEmpty(attrib.category) ? "Uncategorized" : attrib.category;

				List<SimpleTuple<GUIContent, IPref>> settings;

				if (m_Settings.TryGetValue(category, out settings))
					settings.Add(new SimpleTuple<GUIContent, IPref>(attrib.title, pref));
				else
					m_Settings.Add(category, new List<SimpleTuple<GUIContent, IPref>>() { new SimpleTuple<GUIContent, IPref>(attrib.title, pref) });

				if(attrib.title != null && !string.IsNullOrEmpty(attrib.title.text))
				{
					foreach (var word in attrib.title.text.Split(' '))
						keywords.Add(word);
				}
			}

			foreach (var method in methods)
			{
				var attrib = (UserSettingBlockAttribute) Attribute.GetCustomAttribute(method, typeof(UserSettingBlockAttribute));
				var category = string.IsNullOrEmpty(attrib.category) ? "Uncategorized" : attrib.category;
				List<MethodInfo> blocks;

				var parameters = method.GetParameters();

				if (!method.IsStatic || parameters.Length < 1 || parameters[0].ParameterType != typeof(string))
				{
					Log.Warning("[UserSettingBlockAttribute] is only valid for static functions with a single string parameter. Ex, `static void MySettings(string searchContext)`. Skipping \"" + method.Name + "\"");
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

			if (s_ShowHiddenSettings)
			{
				var unlisted = new List<SimpleTuple<GUIContent, IPref>>();
				m_Settings.Add("Unlisted", unlisted);
				foreach (var pref in UserSettings.FindUserSettings(SettingVisibility.Unlisted | SettingVisibility.Hidden))
					unlisted.Add(new SimpleTuple<GUIContent, IPref>( new GUIContent(pref.key), pref ));
			}

			if (s_ShowUnregisteredSettings)
			{
				var unregistered = new List<SimpleTuple<GUIContent, IPref>>();
				m_Settings.Add("Unregistered", unregistered);
				foreach (var pref in UserSettings.FindUserSettings(SettingVisibility.Unregistered))
					unregistered.Add(new SimpleTuple<GUIContent, IPref>( new GUIContent(pref.key), pref ));
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

				menu.AddItem(new GUIContent("Show Unlisted Settings"), s_ShowHiddenSettings, () =>
				{
					s_ShowHiddenSettings.SetValue(!s_ShowHiddenSettings, true);
					SearchForUserSettingAttributes();
				});

				menu.AddItem(new GUIContent("Show Unregistered Settings"), s_ShowUnregisteredSettings, () =>
				{
					s_ShowUnregisteredSettings.SetValue(!s_ShowUnregisteredSettings, true);
					SearchForUserSettingAttributes();
				});

				menu.AddSeparator("");
			}

			menu.AddItem(new GUIContent("Reset All"), false, () =>
			{
				if (!UnityEditor.EditorUtility.DisplayDialog("Reset All Settings", "Reset all ProBuilder settings? This is not undo-able.", "Reset", "Cancel"))
					return;
				foreach (var pref in UserSettings.FindUserSettings(SettingVisibility.Visible | SettingVisibility.Unlisted))
					pref.Reset();
				Settings.Save();
			});

			menu.ShowAsContext();
		}

#if SETTINGS_PROVIDER_ENABLED
		public override void OnGUI(string searchContext)
#else
		[PreferenceItem("ProBuilder")]
		static void OnGUI()
#endif
		{
			Styles.Init();

#if SETTINGS_PROVIDER_ENABLED
			var hasSearchContext = !string.IsNullOrEmpty(searchContext);
			s_SearchContext[0] = searchContext;
#else
			Init();
			const string searchContext = "";
			const bool hasSearchContext = false;
#endif

			EditorGUI.BeginChangeCheck();
			EditorGUIUtility.labelWidth = 240;
			GUILayout.BeginVertical(Styles.settingsArea);

			if (hasSearchContext)
			{
				// todo - Improve search comparison
				var searchKeywords = searchContext.Split(' ');

				foreach (var settingField in m_Settings)
				foreach (var setting in settingField.Value)
					if (searchKeywords.Any(x => !string.IsNullOrEmpty(x) && setting.item1.text.IndexOf(x, StringComparison.InvariantCultureIgnoreCase) > -1))
						DoPreferenceField(setting.item1, setting.item2);

				foreach (var settingsBlock in m_SettingBlocks)
				foreach (var block in settingsBlock.Value)
					block.Invoke(null, s_SearchContext);
			}
			else
			{
				foreach (var key in m_Categories)
				{
					GUILayout.Label(key, EditorStyles.boldLabel);

					List<SimpleTuple<GUIContent, IPref>> settings;

					if (m_Settings.TryGetValue(key, out settings))
						foreach (var setting in settings)
							DoPreferenceField(setting.item1, setting.item2);

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
				Settings.Save();

				if (ProBuilderEditor.instance != null)
					ProBuilderEditor.instance.OnEnable();
			}
		}

		internal static void DoPreferenceField(GUIContent title, IPref pref)
		{
			if (pref is Pref<float>)
			{
				var cast = (Pref<float>)pref;
				cast.value = EditorGUILayout.FloatField(title, cast.value);
			}
			else if (pref is Pref<int>)
			{
				var cast = (Pref<int>)pref;
				cast.value = EditorGUILayout.IntField(title, cast.value);
			}
			else if (pref is Pref<bool>)
			{
				var cast = (Pref<bool>)pref;
				cast.value = EditorGUILayout.Toggle(title, cast.value);
			}
			else if (pref is Pref<string>)
			{
				var cast = (Pref<string>)pref;
				cast.value = EditorGUILayout.TextField(title, cast.value);
			}
			else if (pref is Pref<Color>)
			{
				var cast = (Pref<Color>)pref;
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

			UI.EditorGUILayout.DoResetContextMenuForLastRect(pref);
		}
	}
}
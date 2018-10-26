using System;
using System.Linq;
using UnityEngine;
using System.Text.RegularExpressions;
using UnityEngine.ProBuilder.AssetIdRemapUtility;
using UnityEngine.ProBuilder;
using Version = UnityEngine.ProBuilder.Version;
using UnityEditor.SettingsManagement;

namespace UnityEditor.ProBuilder
{
	[InitializeOnLoad]
	static class AboutWindowSetup
	{
		static AboutWindowSetup()
		{
			EditorApplication.delayCall += AboutWindow.ValidateVersion;
		}
	}

	sealed class AboutWindow : EditorWindow
	{
		static readonly GUIContent k_LearnContent = new GUIContent("Learn ProBuilder", "Documentation");
		static readonly GUIContent k_ForumLinkContent = new GUIContent("Support Forum", "ProCore Support Forum");
		static readonly GUIContent k_BannerContent = new GUIContent("", "ProBuilder Quick-Start Video Tutorials");
		static readonly GUIContent k_ApiExamplesContent = new GUIContent("API Examples");

		const string k_VideoUrl = @"http://bit.ly/pbstarter";
		const string k_LearnUrl = @"http://procore3d.com/docs/probuilder";
		const string k_SupportUrl = @"http://www.procore3d.com/forum/";
		const string k_ApiExamplesLink = @"https://github.com/Unity-Technologies/ProBuilder-API-Examples";

		const float k_BannerWidth = 480f;
		const float k_BannerHeight = 270f;

		static Pref<SemVer> s_StoredVersionInfo = new Pref<SemVer>("about.identifier", new SemVer(), SettingsScope.Project);

		const string k_AboutPrefFormat = "M.m.p-T.b";

		internal const string k_FontRegular = "Asap-Regular.otf";
		internal const string k_FontMedium = "Asap-Medium.otf";

		// Use less contrast-y white and black font colors for better readability
		public static readonly Color k_FontWhite = HexToColor(0xCECECE);
		public static readonly Color k_FontBlack = HexToColor(0x545454);
		public static readonly Color k_FontBlueNormal = HexToColor(0x00AAEF);
		public static readonly Color k_FontBlueHover = HexToColor(0x008BEF);

		const string k_ProductName = PreferenceKeys.pluginTitle;
		string m_ChangeLogRichText = "";
		SemVer m_ChangeLogVersionInfo;

		static class Styles
		{
			public static GUIStyle bannerStyle;
			public static GUIStyle header1Style;
			public static GUIStyle linkStyle;
			public static GUIStyle separatorStyle;
			public static GUIStyle changelogStyle;
			public static GUIStyle changelogTextStyle;

			static bool s_IsInitialized;

			public static void Init()
			{
				if (s_IsInitialized)
					return;

				s_IsInitialized = true;

				bannerStyle = new GUIStyle()
				{
					// RectOffset(left, right, top, bottom)
					margin = new RectOffset(12, 12, 12, 12),
					normal = new GUIStyleState() {
						background = FileUtility.LoadInternalAsset<Texture2D>("Content/About/Images/Banner_Normal.png")
					},
					hover = new GUIStyleState() {
						background = FileUtility.LoadInternalAsset<Texture2D>("Content/About/Images/Banner_Hover.png")
					},
				};

				header1Style = new GUIStyle()
				{
					margin = new RectOffset(10, 10, 10, 10),
					alignment = TextAnchor.MiddleCenter,
					fontSize = 24,
					// fontStyle = FontStyle.Bold,
					font = FileUtility.LoadInternalAsset<Font>("Content/Font/" + k_FontMedium),
					normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? k_FontWhite : k_FontBlack }
				};

				linkStyle = new GUIStyle()
				{
					margin = new RectOffset(10, 10, 10, 10),
					alignment = TextAnchor.MiddleCenter,
					fontSize = 16,
					font = FileUtility.LoadInternalAsset<Font>("Content/Font/" + k_FontRegular),
					normal = new GUIStyleState() {
						textColor = k_FontBlueNormal,
						background = FileUtility.LoadInternalAsset<Texture2D>(
							string.Format("Content/About/Images/ScrollBackground_{0}.png", EditorGUIUtility.isProSkin ? "Pro" : "Light"))
					},
					hover = new GUIStyleState() {
						textColor = k_FontBlueHover,
						background = FileUtility.LoadInternalAsset<Texture2D>(
							string.Format("Content/About/Images/ScrollBackground_{0}.png", EditorGUIUtility.isProSkin ? "Pro" : "Light"))
					}
				};

				separatorStyle = new GUIStyle()
				{
					margin = new RectOffset(10, 10, 10, 10),
					alignment = TextAnchor.MiddleCenter,
					fontSize = 16,
					font = FileUtility.LoadInternalAsset<Font>("Content/Font/" + k_FontRegular),
					normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? k_FontWhite : k_FontBlack }
				};

				changelogStyle = new GUIStyle()
				{
					margin = new RectOffset(10, 10, 10, 10),
					font = FileUtility.LoadInternalAsset<Font>("Content/Font/" + k_FontRegular),
					richText = true,
					normal = new GUIStyleState() { background = FileUtility.LoadInternalAsset<Texture2D>(
						string.Format("Content/About/Images/ScrollBackground_{0}.png",
							EditorGUIUtility.isProSkin ? "Pro" : "Light"))
					}
				};

				changelogTextStyle = new GUIStyle()
				{
					margin = new RectOffset(10, 10, 10, 10),
					font = FileUtility.LoadInternalAsset<Font>("Content/Font/" + k_FontRegular),
					fontSize = 14,
					normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? k_FontWhite : k_FontBlack },
					richText = true,
					wordWrap = true
				};
			}
		}

		Vector2 scroll = Vector2.zero;

		internal static void ValidateVersion()
		{
			var currentVersion = Version.currentInfo;
			var oldVersion = (SemVer) s_StoredVersionInfo;

			bool isNewVersion = currentVersion != oldVersion;

			if (isNewVersion)
			{
				PreferencesUpdater.CheckEditorPrefsVersion();
				s_StoredVersionInfo.SetValue(currentVersion, true);
			}

			bool assetStoreInstallFound = isNewVersion && PackageImporter.IsPreProBuilder4InProject();
			bool deprecatedGuidsFound = isNewVersion && PackageImporter.DoesProjectContainDeprecatedGUIDs();

			const string k_AssetStoreUpgradeTitle = "Old ProBuilder Install Found in Assets";
			const string k_AssetStoreUpgradeDialog = "The Asset Store version of ProBuilder is incompatible with Package Manager. Would you like to convert your project to the Package Manager version of ProBuilder?";
			const string k_DeprecatedGuidsTitle = "Broken ProBuilder References Found in Project";
			const string k_DeprecatedGuidsDialog = "ProBuilder has found some mesh components that are missing references. To keep these models editable by ProBuilder, they need to be repaired. Would you like to perform the repair action now?";

			if (isNewVersion && (assetStoreInstallFound || deprecatedGuidsFound))
				if (UnityEditor.EditorUtility.DisplayDialog(assetStoreInstallFound ? k_AssetStoreUpgradeTitle : k_DeprecatedGuidsTitle,
					assetStoreInstallFound ? k_AssetStoreUpgradeDialog : k_DeprecatedGuidsDialog +
					"\n\nIf you choose \"No\" this dialog may be accessed again at any time through the \"Tools/ProBuilder/Repair/Convert to Package Manager\" menu item.",
					"Yes", "No"))
					EditorApplication.delayCall += AssetIdRemapEditor.OpenConversionEditor;
		}

		public static void Init()
		{
			GetWindow<AboutWindow>(true, k_ProductName, true);
		}

		static Color HexToColor(uint x)
		{
			return new Color( 	((x >> 16) & 0xFF) / 255f,
								((x >> 8) & 0xFF) / 255f,
								(x & 0xFF) / 255f,
								1f);
		}

		void OnEnable()
		{
			Styles.Init();

			Texture2D banner = Styles.bannerStyle.normal.background;

			if(banner == null)
			{
				Log.Warning("Could not load About window resources");
				EditorApplication.delayCall += Close;
			}
			else
			{
				Styles.bannerStyle.fixedWidth = k_BannerWidth;
				Styles.bannerStyle.fixedHeight = k_BannerHeight;

				wantsMouseMove = true;
				minSize = new Vector2(k_BannerWidth + 24, k_BannerHeight * 2.5f);
				maxSize = new Vector2(k_BannerWidth + 24, k_BannerHeight * 2.5f);
			}

			TextAsset changeText = FileUtility.LoadInternalAsset<TextAsset>("CHANGELOG.md");
			string raw = changeText != null ? changeText.text : "";

			if (!string.IsNullOrEmpty(raw))
			{
				Changelog logs = new Changelog(changeText.text);
				ChangelogEntry first = logs.entries.First();

				if (first != null)
				{
					m_ChangeLogVersionInfo = first.versionInfo;
					m_ChangeLogRichText = GetRichTextReleaseNotes(first.releaseNotes);
				}
			}
			else
			{
				Log.Error(FileUtility.GetProBuilderInstallDirectory() + "CHANGELOG.md not found!");
			}
		}

		void OnGUI()
		{
			if (Styles.bannerStyle.normal.background == null)
			{
				GUILayout.Label("Could Not Load About Window", EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				return;
			}

			var evt = Event.current;

			Vector2 mousePosition = evt.mousePosition;

			if( GUILayout.Button(k_BannerContent, Styles.bannerStyle) )
				Application.OpenURL(k_VideoUrl);

			if(GUILayoutUtility.GetLastRect().Contains(mousePosition))
				Repaint();

			GUILayout.BeginVertical(Styles.changelogStyle);

			GUILayout.Label(k_ProductName + " " + m_ChangeLogVersionInfo.ToString("M.m.p"), Styles.header1Style);

			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				if(GUILayout.Button(k_LearnContent, Styles.linkStyle))
					Application.OpenURL(k_LearnUrl);

				GUILayout.Label("|", Styles.separatorStyle);

				if(GUILayout.Button(k_ForumLinkContent, Styles.linkStyle))
					Application.OpenURL(k_SupportUrl);

				GUILayout.Label("|", Styles.separatorStyle);

				if(GUILayout.Button(k_ApiExamplesContent, Styles.linkStyle))
					Application.OpenURL(k_ApiExamplesLink);
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			if(GUILayoutUtility.GetLastRect().Contains(mousePosition))
				Repaint();

			GUILayout.EndVertical();

			// always bold the first line (cause it's the version info stuff)
			scroll = EditorGUILayout.BeginScrollView(scroll, Styles.changelogStyle);
			GUILayout.Label(m_ChangeLogRichText, Styles.changelogTextStyle);
			EditorGUILayout.EndScrollView();

			GUILayout.BeginHorizontal();
			GUILayout.Label(Version.currentInfo.ToString());

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("licenses", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
				GetWindow<LicenseEditor>(true, "ProBuilder 3rd Party Licenses", true);

			GUILayout.EndHorizontal();
		}

		static string GetRichTextReleaseNotes(string text)
		{
			string rich = text;
			rich = Regex.Replace(rich, "^-", "\u2022", RegexOptions.Multiline);
			rich = Regex.Replace(rich, @"(?<=^###\\s).*", "<size=16><b>${0}</b></size>", RegexOptions.Multiline);
			rich = Regex.Replace(rich, @"^###\ ", "", RegexOptions.Multiline);
			return rich;
		}
	}
}

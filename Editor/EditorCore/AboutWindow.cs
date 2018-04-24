using UnityEngine;
using UnityEditor;
using System.Text;
using System.Text.RegularExpressions;
using ProBuilder.AssetUtility;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder
{
	/// <summary>
	/// Used to pop up the window on import.
	/// </summary>
	[InitializeOnLoad]
	static class AboutWindowSetup
	{
		static AboutWindowSetup()
		{
			EditorApplication.delayCall += AboutWindow.ValidateVersion;
		}
	}

	class AboutWindow : EditorWindow
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

		const string k_AboutWindowVersionPref = "ProBuilder_AboutWindowIdentifier";
		const string k_AboutPrefFormat = "M.m.p-T.b";

		internal const string k_FontRegular = "Asap-Regular.otf";
		internal const string k_FontMedium = "Asap-Medium.otf";

		// Use less contast-y white and black font colors for better readabililty
		public static readonly Color k_FontWhite = HexToColor(0xCECECE);
		public static readonly Color k_FontBlack = HexToColor(0x545454);
		public static readonly Color k_FontBlueNormal = HexToColor(0x00AAEF);
		public static readonly Color k_FontBlueHover = HexToColor(0x008BEF);

		const string k_ProductName = pb_Constant.PRODUCT_NAME;
		pb_VersionInfo m_ChangeLogVersionInfo;
		string m_ChangeLogRichText = "";

		internal static GUIStyle bannerStyle,
								header1Style,
								versionInfoStyle,
								linkStyle,
								separatorStyle,
								changelogStyle,
								changelogTextStyle;

		Vector2 scroll = Vector2.zero;

		internal static void ValidateVersion()
		{
			string currentVersionString = pb_Version.Current.ToString(k_AboutPrefFormat);
			bool isNewVersion = PreferencesInternal.GetString(k_AboutWindowVersionPref).Equals(currentVersionString);
			PreferencesInternal.SetString(k_AboutWindowVersionPref, currentVersionString, PreferenceLocation.Global);

			if (isNewVersion && PackageImporter.IsPreUpmProBuilderInProject())
				if (UnityEditor.EditorUtility.DisplayDialog("Conflicting ProBuilder Install in Project",
					"The Asset Store version of ProBuilder is incompatible with Package Manager. Would you like to convert your project to the Package Manager version of ProBuilder?\n\nIf you choose \"No\" this dialog may be accessed again at any time through the \"Tools/ProBuilder/Repair/Convert to Package Manager\" menu item.",
					"Yes", "No"))
					EditorApplication.delayCall += AssetIdRemapUtility.OpenConversionEditor;
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

		internal static void InitGuiStyles()
		{
			bannerStyle = new GUIStyle()
			{
				// RectOffset(left, right, top, bottom)
				margin = new RectOffset(12, 12, 12, 12),
				normal = new GUIStyleState() {
					background = FileUtil.LoadInternalAsset<Texture2D>("Content/About/Images/Banner_Normal.png")
				},
				hover = new GUIStyleState() {
					background = FileUtil.LoadInternalAsset<Texture2D>("Content/About/Images/Banner_Hover.png")
				},
			};

			header1Style = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				alignment = TextAnchor.MiddleCenter,
				fontSize = 24,
				// fontStyle = FontStyle.Bold,
				font = FileUtil.LoadInternalAsset<Font>("Content/Font/" + k_FontMedium),
				normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? k_FontWhite : k_FontBlack }
			};

			versionInfoStyle = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				fontSize = 14,
				font = FileUtil.LoadInternalAsset<Font>("Content/Font/" + k_FontRegular),
				normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? k_FontWhite : k_FontBlack }
			};

			linkStyle = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				alignment = TextAnchor.MiddleCenter,
				fontSize = 16,
				font = FileUtil.LoadInternalAsset<Font>("Content/Font/" + k_FontRegular),
				normal = new GUIStyleState() {
					textColor = k_FontBlueNormal,
					background = FileUtil.LoadInternalAsset<Texture2D>(
						string.Format("Content/About/Images/ScrollBackground_{0}.png", EditorGUIUtility.isProSkin ? "Pro" : "Light"))
				},
				hover = new GUIStyleState() {
					textColor = k_FontBlueHover,
					background = FileUtil.LoadInternalAsset<Texture2D>(
						string.Format("Content/About/Images/ScrollBackground_{0}.png", EditorGUIUtility.isProSkin ? "Pro" : "Light"))
				}
			};

			separatorStyle = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				alignment = TextAnchor.MiddleCenter,
				fontSize = 16,
				font = FileUtil.LoadInternalAsset<Font>("Content/Font/" + k_FontRegular),
				normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? k_FontWhite : k_FontBlack }
			};

			changelogStyle = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				font = FileUtil.LoadInternalAsset<Font>("Content/Font/" + k_FontRegular),
				richText = true,
				normal = new GUIStyleState() { background = FileUtil.LoadInternalAsset<Texture2D>(
					string.Format("Content/About/Images/ScrollBackground_{0}.png",
						EditorGUIUtility.isProSkin ? "Pro" : "Light"))
				}
			};

			changelogTextStyle = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				font = FileUtil.LoadInternalAsset<Font>("Content/Font/" + k_FontRegular),
				fontSize = 14,
				normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? k_FontWhite : k_FontBlack },
				richText = true,
				wordWrap = true
			};
		}

		void OnEnable()
		{
			InitGuiStyles();

			Texture2D banner = bannerStyle.normal.background;

			if(banner == null)
			{
				pb_Log.Warning("Could not load About window resources");
				EditorApplication.delayCall += Close;
			}
			else
			{
				bannerStyle.fixedWidth = k_BannerWidth;
				bannerStyle.fixedHeight = k_BannerHeight;

				wantsMouseMove = true;
				minSize = new Vector2(k_BannerWidth + 24, k_BannerHeight * 2.5f);
				maxSize = new Vector2(k_BannerWidth + 24, k_BannerHeight * 2.5f);
			}

			string changes = System.IO.File.ReadAllText(FileUtil.GetProBuilderInstallDirectory() + "CHANGELOG.md");

			if (!string.IsNullOrEmpty(changes))
			{
				FormatChangelog(changes, out m_ChangeLogVersionInfo, out m_ChangeLogRichText);

#if !(DEBUG || DEVELOPMENT || PB_DEBUG)
				if(!pb_Version.Current.Equals(m_ChangeLogVersionInfo))
					pb_Log.Info("Changelog version does not match internal version. {0} != {1}",
						m_ChangeLogVersionInfo.ToString(k_AboutPrefFormat),
						pb_Version.Current.ToString(k_AboutPrefFormat));
#endif
			}
			else
			{
				pb_Log.Error(FileUtil.GetProBuilderInstallDirectory() + "CHANGELOG.md not found!");
			}
		}

		void OnGUI()
		{
			if (bannerStyle.normal.background == null)
			{
				GUILayout.Label("Could Not Load About Window", EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				return;
			}

			var evt = Event.current;

			Vector2 mousePosition = evt.mousePosition;

			if( GUILayout.Button(k_BannerContent, bannerStyle) )
				Application.OpenURL(k_VideoUrl);

			if(GUILayoutUtility.GetLastRect().Contains(mousePosition))
				Repaint();

			GUILayout.BeginVertical(changelogStyle);

			GUILayout.Label(k_ProductName, header1Style);

			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();

				if(GUILayout.Button(k_LearnContent, linkStyle))
					Application.OpenURL(k_LearnUrl);

				GUILayout.Label("|", separatorStyle);

				if(GUILayout.Button(k_ForumLinkContent, linkStyle))
					Application.OpenURL(k_SupportUrl);

				GUILayout.Label("|", separatorStyle);

				if(GUILayout.Button(k_ApiExamplesContent, linkStyle))
					Application.OpenURL(k_ApiExamplesLink);
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			if(GUILayoutUtility.GetLastRect().Contains(mousePosition))
				Repaint();

			GUILayout.EndVertical();

			// always bold the first line (cause it's the version info stuff)
			scroll = EditorGUILayout.BeginScrollView(scroll, changelogStyle);
			GUILayout.Label(string.Format("Version: {0}", m_ChangeLogVersionInfo != null
				? m_ChangeLogVersionInfo.ToString("M.m.p")
				: "Changelog Not Loaded"), versionInfoStyle);

			GUILayout.Label("\n" + m_ChangeLogRichText, changelogTextStyle);
			EditorGUILayout.EndScrollView();

			GUILayout.BeginHorizontal();
			GUILayout.Label(pb_Version.Current.ToString());

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("licenses", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
				GetWindow<LicenseEditor>(true, "ProBuilder 3rd Party Licenses", true);

			GUILayout.EndHorizontal();
		}

		/// <summary>
		/// Extracts and formats the latest changelog entry into rich text.  Also grabs the version.
		/// </summary>
		/// <param name="raw"></param>
		/// <param name="version"></param>
		/// <param name="formattedChangelog"></param>
		/// <returns></returns>
		public static bool FormatChangelog(string raw, out pb_VersionInfo version, out string formattedChangelog)
		{
			bool success = true;

			// get first version entry
			string[] split = Regex.Split(raw, "(?mi)^#\\s", RegexOptions.Multiline);
			string firstChangelogEntryLine = split.Length > 1 ? split[1] : "";
			// get the version info
			Match versionMatch = Regex.Match(firstChangelogEntryLine, @"(?<=^ProBuilder\s).[0-9]*\.[0-9]*\.[0-9]*[A-Z|a-z|\-]*\.[0-9]*");
			success = pb_VersionInfo.TryGetVersionInfo(versionMatch.Success ? versionMatch.Value : firstChangelogEntryLine.Split('\n')[0], out version);

			try
			{
				StringBuilder sb = new StringBuilder();
				string[] newLineSplit = firstChangelogEntryLine.Trim().Split('\n');
				for(int i = 2; i < newLineSplit.Length; i++)
					sb.AppendLine(newLineSplit[i]);

				formattedChangelog = sb.ToString();
				formattedChangelog = Regex.Replace(formattedChangelog, "^-", "\u2022", RegexOptions.Multiline);
				formattedChangelog = Regex.Replace(formattedChangelog, @"(?<=^##\\s).*", "<size=16><b>${0}</b></size>", RegexOptions.Multiline);
				formattedChangelog = Regex.Replace(formattedChangelog, @"^##\ ", "", RegexOptions.Multiline);
			}
			catch
			{
				formattedChangelog = "";
				success = false;
			}

			return success;
		}
	}
}

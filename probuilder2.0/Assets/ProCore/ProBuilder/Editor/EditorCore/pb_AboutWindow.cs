using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using ProBuilder.AssetUtility;
using ProBuilder.Core;

namespace ProBuilder.EditorCore
{
	[InitializeOnLoad]
	static class pb_AboutWindowSetup
	{
		static pb_AboutWindowSetup()
		{
			EditorApplication.delayCall += pb_AboutWindow.ValidateVersion;
		}
	}

	class pb_AboutWindow : EditorWindow
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
		const string k_AboutPrefFormat = "M.m.p-t.b";

		internal const string k_FontRegular = "Asap-Regular.otf";
		internal const string k_FontMedium = "Asap-Medium.otf";

		// Use less contast-y white and black font colors for better readabililty
		public static readonly Color k_FontWhite = HexToColor(0xCECECE);
		public static readonly Color k_FontBlack = HexToColor(0x545454);
		public static readonly Color k_FontBlueNormal = HexToColor(0x00AAEF);
		public static readonly Color k_FontBlueHover = HexToColor(0x008BEF);

		string m_ProductName = pb_Constant.PRODUCT_NAME;
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
			bool isNewVersion = !pb_PreferencesInternal.GetString(k_AboutWindowVersionPref).Equals(currentVersionString);
			pb_PreferencesInternal.SetString(k_AboutWindowVersionPref, currentVersionString, pb_PreferenceLocation.Global);

			if (isNewVersion && PackageImporter.IsPreUpmProBuilderInProject())
				if (EditorUtility.DisplayDialog("Conflicting ProBuilder Install in Project",
					"The Asset Store version of ProBuilder is incompatible with Package Manager. Would you like to convert your project to the Package Manager version of ProBuilder?\n\nIf you choose \"No\" this dialog may be accessed again at any time through the \"Tools/ProBuilder/Repair/Convert to Package Manager\" menu item.",
					"Yes", "No"))
					EditorApplication.delayCall += AssetIdRemapUtility.OpenConversionEditor;
		}

		public static void Init ()
		{
			GetWindow<pb_AboutWindow>(true, pb_Constant.PRODUCT_NAME, true);
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
					background = pb_FileUtil.LoadInternalAsset<Texture2D>("About/Images/Banner_Normal.png")
				},
				hover = new GUIStyleState() {
					background = pb_FileUtil.LoadInternalAsset<Texture2D>("About/Images/Banner_Hover.png")
				},
			};

			header1Style = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				alignment = TextAnchor.MiddleCenter,
				fontSize = 24,
				// fontStyle = FontStyle.Bold,
				font = pb_FileUtil.LoadInternalAsset<Font>("About/Font/" + k_FontMedium),
				normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? k_FontWhite : k_FontBlack }
			};

			versionInfoStyle = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				fontSize = 14,
				font = pb_FileUtil.LoadInternalAsset<Font>("About/Font/" + k_FontRegular),
				normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? k_FontWhite : k_FontBlack }
			};

			linkStyle = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				alignment = TextAnchor.MiddleCenter,
				fontSize = 16,
				font = pb_FileUtil.LoadInternalAsset<Font>("About/Font/" + k_FontRegular),
				normal = new GUIStyleState() {
					textColor = k_FontBlueNormal,
					background = pb_FileUtil.LoadInternalAsset<Texture2D>(
						string.Format("About/Images/ScrollBackground_{0}.png", EditorGUIUtility.isProSkin ? "Pro" : "Light"))
				},
				hover = new GUIStyleState() {
					textColor = k_FontBlueHover,
					background = pb_FileUtil.LoadInternalAsset<Texture2D>(
						string.Format("About/Images/ScrollBackground_{0}.png", EditorGUIUtility.isProSkin ? "Pro" : "Light"))
				}
			};

			separatorStyle = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				alignment = TextAnchor.MiddleCenter,
				fontSize = 16,
				font = pb_FileUtil.LoadInternalAsset<Font>("About/Font/" + k_FontRegular),
				normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? k_FontWhite : k_FontBlack }
			};

			changelogStyle = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				font = pb_FileUtil.LoadInternalAsset<Font>("About/Font/" + k_FontRegular),
				richText = true,
				normal = new GUIStyleState() { background = pb_FileUtil.LoadInternalAsset<Texture2D>(
					string.Format("About/Images/ScrollBackground_{0}.png",
						EditorGUIUtility.isProSkin ? "Pro" : "Light"))
				}
			};

			changelogTextStyle = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				font = pb_FileUtil.LoadInternalAsset<Font>("About/Font/" + k_FontRegular),
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

				if(!m_ProductName.Contains("Basic"))
					m_ProductName = "ProBuilder Advanced";
			}

			TextAsset changeText = pb_FileUtil.LoadInternalAsset<TextAsset>("About/changelog.txt");

			string raw = changeText != null ? changeText.text : "";

			if (!string.IsNullOrEmpty(raw))
			{
				pb_VersionUtil.FormatChangelog(raw, out m_ChangeLogVersionInfo, out m_ChangeLogRichText);
#if !(DEBUG || DEVELOPMENT || PB_DEBUG)
				if(!pb_Version.Current.Equals(m_ChangeLogVersionInfo))
					pb_Log.Info("Changelog version does not match internal version. {0} != {1}",
						m_ChangeLogVersionInfo.ToString(k_AboutPrefFormat),
						pb_Version.Current.ToString(k_AboutPrefFormat));
#endif
			}
		}

		void OnGUI()
		{
			if (bannerStyle.normal.background == null)
			{
				GUILayout.Label("Could Not Load About Window", EditorStyles.centeredGreyMiniLabel, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
				return;
			}

			Vector2 mousePosition = Event.current.mousePosition;

			if( GUILayout.Button(k_BannerContent, bannerStyle) )
				Application.OpenURL(k_VideoUrl);

			if(GUILayoutUtility.GetLastRect().Contains(mousePosition))
				Repaint();

			GUILayout.BeginVertical(changelogStyle);

			GUILayout.Label(m_ProductName, header1Style);

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
			GUILayout.Label(string.Format("Version: {0}", m_ChangeLogVersionInfo.ToString("M.m.p")), versionInfoStyle);
			GUILayout.Label("\n" + m_ChangeLogRichText, changelogTextStyle);
			EditorGUILayout.EndScrollView();

			GUILayout.BeginHorizontal();
			GUILayout.Label(pb_Version.Current.ToString());
			if (GUILayout.Button("licenses", EditorStyles.miniButton, GUILayout.ExpandWidth(false)))
				GetWindow<pb_LicenseWindow>(true, "ProBuilder 3rd Party Licenses", true);
			GUILayout.EndHorizontal();
		}

		/// <summary>
		/// Draw a horizontal line across the screen and update the guilayout.
		/// </summary>
		void HorizontalLine()
		{
			Rect r = GUILayoutUtility.GetLastRect();
			Color og = GUI.backgroundColor;
			GUI.backgroundColor = Color.black;
			GUI.Box(new Rect(0f, r.y + r.height + 2, Screen.width, 2f), "");
			GUI.backgroundColor = og;

			GUILayout.Space(6);
		}
	}
}

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace ProBuilder2.EditorCommon
{
	/**
	 * INSTRUCTIONS
	 *
	 *  - Only modify properties in the USER SETTINGS region.
	 *	- All content is loaded from external files (pc_AboutEntry_YourProduct.  Use the templates!
	 */

	/**
	 * Used to pop up the window on import.
	 */
	[InitializeOnLoad]
	static class pb_AboutWindowSetup
	{
		static pb_AboutWindowSetup()
		{
			pb_AboutWindow.Init(false);
		}
	}

	/**
	 * Changelog.txt file should follow this format:
	 *
	 *	| -- Product Name 2.1.0 -
	 *	|
	 *	| # Features
	 *	| 	- All kinds of awesome stuff
	 *	| 	- New flux capacitor design achieves time travel at lower velocities.
	 *	| 	- Dark matter reactor recalibrated.
	 *	|
	 *	| # Bug Fixes
	 *	| 	- No longer explodes when spacebar is pressed.
	 *	| 	- Fix rolling issue in Rickmeter.
	 *	|
	 *	| # Changes
	 *	| 	- Changed Blue to Red.
	 *	| 	- Enter key now causes explosions.
	 *
	 * This path is relative to the PRODUCT_ROOT path.
	 *
	 * Note that your changelog may contain multiple entries.  Only the top-most
	 * entry will be displayed.
	 */
	public class pb_AboutWindow : EditorWindow
	{
		// Modify these constants to customize about screen.
	 	const string PACKAGE_NAME = "ProBuilder";

		// Path to the root folder
		internal const string ABOUT_ROOT = "Assets/ProCore/" + PACKAGE_NAME + "/About";

	#if SHOW_PRODUCT_THUMBS

		/**
		 * Contains data for use in Advertisement shelf.
		 */
		private class AdvertisementThumb
		{
			public Texture2D image;
			public string url;
			public string about;
			public GUIContent guiContent;

			public AdvertisementThumb(string imagePath, string url, string about)
			{
				guiContent = new GUIContent("", about);
				this.image = LoadAssetAtPath<Texture2D>(imagePath);

				guiContent.image = this.image;
				this.url = url;
				this.about = about;
			}
		}

		/**
		 * Advertisement thumb constructor is:
		 * new AdvertisementThumb( PathToAdImage : string, URLToPurchase : string, ProductDescription : string )
		 * Provide as many or few (or none) as desired.
		 *
		 * Notes - The http:// part is required.  Partial URLs do not work on Mac.
		 */
		private AdvertisementThumb[] advertisements = new AdvertisementThumb[] {
			new AdvertisementThumb( ABOUT_ROOT + "/Images/ProBuilder_AssetStore_Icon_96px.png", "http://www.protoolsforunity3d.com/probuilder/", "Build and Texture Geometry In-Editor"),
			new AdvertisementThumb( ABOUT_ROOT + "/Images/ProGrids_AssetStore_Icon_96px.png", "http://www.protoolsforunity3d.com/progrids/", "True Grids and Grid-Snapping"),
			new AdvertisementThumb( ABOUT_ROOT + "/Images/ProGroups_AssetStore_Icon_96px.png", "http://www.protoolsforunity3d.com/progroups/", "Hide, Freeze, Group, & Organize"),
			new AdvertisementThumb( ABOUT_ROOT + "/Images/Prototype_AssetStore_Icon_96px.png", "http://www.protoolsforunity3d.com/prototype/", "Design and Build With Zero Lag"),
			new AdvertisementThumb( ABOUT_ROOT + "/Images/QuickBrush_AssetStore_Icon_96px.png", "http://www.protoolsforunity3d.com/quickbrush/", "Quickly Add Detail Geometry"),
			new AdvertisementThumb( ABOUT_ROOT + "/Images/QuickDecals_AssetStore_Icon_96px.png", "http://www.protoolsforunity3d.com/quickdecals/", "Add Dirt, Splatters, Posters, etc"),
			new AdvertisementThumb( ABOUT_ROOT + "/Images/QuickEdit_AssetStore_Icon_96px.png", "http://www.protoolsforunity3d.com/quickedit/", "Edit Imported Meshes!"),
		};

		GUIStyle advertisementStyle;
		const int AD_HEIGHT = 96;
		Vector2 adScroll = Vector2.zero;
	#endif

		GUIContent gc_Learn = new GUIContent("Learn ProBuilder", "Documentation");
		GUIContent gc_Forum = new GUIContent("Support Forum", "ProCore Support Forum");
		GUIContent gc_Contact = new GUIContent("Contact Us", "Send us an email!");
		GUIContent gc_Banner = new GUIContent("", "ProBuilder Quick-Start Video Tutorials");

		private const string VIDEO_URL = @"http://bit.ly/pbstarter";
		private const string LEARN_URL = @"http://procore3d.com/docs/probuilder";
		private const string SUPPORT_URL = @"http://www.procore3d.com/forum/";
		// private const string CONTACT_EMAIL = @"mailto:contact@procore3d.com?subject=Hey%20ProBuilder%20People!";
		private const string CONTACT_EMAIL = @"http://www.procore3d.com/about/";

		internal const string FONT_REGULAR = "Asap-Regular.otf";
		internal const string FONT_MEDIUM = "Asap-Medium.otf";

		// Use less contast-y white and black font colors for better readabililty
		public static readonly Color font_white = HexToColor(0xCECECE);
		public static readonly Color font_black = HexToColor(0x545454);
		public static readonly Color font_blue_normal = HexToColor(0x00AAEF);
		public static readonly Color font_blue_hover = HexToColor(0x008BEF);// HexToColor(0x008BEF);

		private string AboutEntryPath = "";
		private string ProductVersion = "";
		private string ChangelogPath = "";
		private string changelog = "";
		private string productName = pb_Constant.PRODUCT_NAME;

		private static Color HexToColor(uint x)
		{
			return new Color( 	((x >> 16) & 0xFF) / 255f,
								((x >> 8) & 0xFF) / 255f,
								(x & 0xFF) / 255f,
								1f);
		}

		internal static GUIStyle bannerStyle,
								header1Style,
								versionInfoStyle,
								linkStyle,
								separatorStyle,
								changelogStyle,
								changelogTextStyle;

		Vector2 scroll = Vector2.zero;

		/**
		 * Return true if Init took place, false if not.
		 */
		public static bool Init (bool fromMenu)
		{
			string identifier = "", version = "", aboutEntryPath = "";
			bool gotVersion = false, gotIdentifier = false;
			string[] matches = Directory.GetFiles("../", "pc_AboutEntry_ProBuilder.txt", SearchOption.AllDirectories);

			for(int i = 0; i < matches.Length && !(gotVersion && gotIdentifier); i++)
			{
				if( GetField(matches[i], "version: ", out version) )
					gotVersion = true;

				if( GetField(matches[i], "identifier: ", out identifier) )
					gotIdentifier = true;

				aboutEntryPath = matches[i];
			}

			if(!gotIdentifier || !gotVersion)
			{
				Debug.LogWarning("Couldn't find pb_AboutEntry_ProBuilder.txt");
				return false;
			}

			if(fromMenu || EditorPrefs.GetString(identifier) != version)
			{
				string tname;
				pb_AboutWindow win;

				if(GetField(aboutEntryPath, "name: ", out tname))
					win = (pb_AboutWindow)EditorWindow.GetWindow(typeof(pb_AboutWindow), true, tname, true);
				else
					win = (pb_AboutWindow)EditorWindow.GetWindow<pb_AboutWindow>(true, "About", true);

				win.SetAboutEntryPath(aboutEntryPath);
				win.ShowUtility();

				EditorPrefs.SetString(identifier, version);

				return true;
			}
			else
			{
				return false;
			}
		}

		public static void InitGuiStyles()
		{
			bannerStyle = new GUIStyle()
			{
				// RectOffset(left, right, top, bottom)
				margin = new RectOffset(12, 12, 12, 12),
				normal = new GUIStyleState() {
					background = LoadAssetAtPath<Texture2D>(string.Format("{0}/Images/Banner_Normal.png", ABOUT_ROOT))
				},
				hover = new GUIStyleState() {
					background = LoadAssetAtPath<Texture2D>(string.Format("{0}/Images/Banner_Hover.png", ABOUT_ROOT))
				},
			};

			header1Style = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				alignment = TextAnchor.MiddleCenter,
				fontSize = 24,
				// fontStyle = FontStyle.Bold,
				font = LoadAssetAtPath<Font>(string.Format("{0}/Font/{1}", ABOUT_ROOT, FONT_MEDIUM)),
				normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? font_white : font_black }
			};

			versionInfoStyle = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				fontSize = 14,
				font = LoadAssetAtPath<Font>(string.Format("{0}/Font/{1}", ABOUT_ROOT, FONT_REGULAR)),
				normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? font_white : font_black }
			};

			linkStyle = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				alignment = TextAnchor.MiddleCenter,
				fontSize = 16,
				font = LoadAssetAtPath<Font>(string.Format("{0}/Font/{1}", ABOUT_ROOT, FONT_REGULAR)),
				normal = new GUIStyleState() {
					textColor = font_blue_normal,
					background = LoadAssetAtPath<Texture2D>(
						string.Format("{0}/Images/ScrollBackground_{1}.png",
							ABOUT_ROOT,
							EditorGUIUtility.isProSkin ? "Pro" : "Light"))
				},
				hover = new GUIStyleState() {
					textColor = font_blue_hover,
					background = LoadAssetAtPath<Texture2D>(
						string.Format("{0}/Images/ScrollBackground_{1}.png",
							ABOUT_ROOT,
							EditorGUIUtility.isProSkin ? "Pro" : "Light"))
				}
			};

			separatorStyle = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				alignment = TextAnchor.MiddleCenter,
				fontSize = 16,
				font = LoadAssetAtPath<Font>(string.Format("{0}/Font/{1}", ABOUT_ROOT, FONT_REGULAR)),
				normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? font_white : font_black }
			};

			changelogStyle = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				font = LoadAssetAtPath<Font>(string.Format("{0}/Font/{1}", ABOUT_ROOT, FONT_REGULAR)),
				richText = true,
				normal = new GUIStyleState() { background = LoadAssetAtPath<Texture2D>(
					string.Format("{0}/Images/ScrollBackground_{1}.png",
						ABOUT_ROOT,
						EditorGUIUtility.isProSkin ? "Pro" : "Light"))
				}
			};

			changelogTextStyle = new GUIStyle()
			{
				margin = new RectOffset(10, 10, 10, 10),
				font = LoadAssetAtPath<Font>(string.Format("{0}/Font/{1}", ABOUT_ROOT, FONT_REGULAR)),
				fontSize = 14,
				normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? font_white : font_black },
				richText = true,
				wordWrap = true
			};
		}

		public void OnEnable()
		{
			InitGuiStyles();
			Texture2D banner = bannerStyle.normal.background;
			bannerStyle.fixedWidth = banner.width;
			bannerStyle.fixedHeight = banner.height;

			this.wantsMouseMove = true;

			this.minSize = new Vector2(banner.width + 24, banner.height * 2.5f);
			this.maxSize = new Vector2(banner.width + 24, banner.height * 2.5f);

			if(!productName.Contains("Basic"))
				productName = "ProBuilder Advanced";
		}

		public void SetAboutEntryPath(string path)
		{
			AboutEntryPath = path;
			PopulateDataFields(AboutEntryPath);
		}

		internal static T LoadAssetAtPath<T>(string InPath) where T : UnityEngine.Object
		{
			return (T) AssetDatabase.LoadAssetAtPath(InPath, typeof(T));
		}

		void OnGUI()
		{
			Vector2 mousePosition = Event.current.mousePosition;

			if( GUILayout.Button(gc_Banner, bannerStyle) )
				Application.OpenURL(VIDEO_URL);

			if(GUILayoutUtility.GetLastRect().Contains(mousePosition))
				Repaint();

			GUILayout.BeginVertical(changelogStyle);

			GUILayout.Label(productName, header1Style);

			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();

				if(GUILayout.Button(gc_Learn, linkStyle))
					Application.OpenURL(LEARN_URL);

				GUILayout.Label("|", separatorStyle);

				if(GUILayout.Button(gc_Forum, linkStyle))
					Application.OpenURL(SUPPORT_URL);

				GUILayout.Label("|", separatorStyle);

				if(GUILayout.Button(gc_Contact, linkStyle))
					Application.OpenURL(CONTACT_EMAIL);

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			if(GUILayoutUtility.GetLastRect().Contains(mousePosition))
				Repaint();

			GUILayout.EndVertical();

			// always bold the first line (cause it's the version info stuff)
			scroll = EditorGUILayout.BeginScrollView(scroll, changelogStyle);
			GUILayout.Label(string.Format("Version: {0}", ProductVersion), versionInfoStyle);
			GUILayout.Label("\n" + changelog, changelogTextStyle);
			EditorGUILayout.EndScrollView();

	#if SHOW_PRODUCT_THUMBS

			HorizontalLine();

			GUILayout.Label("More ProCore Products", EditorStyles.boldLabel);

			int pad = advertisements.Length * AD_HEIGHT > Screen.width ? 22 : 6;
			adScroll = EditorGUILayout.BeginScrollView(adScroll, false, false, GUILayout.MinHeight(AD_HEIGHT + pad), GUILayout.MaxHeight(AD_HEIGHT + pad));
			GUILayout.BeginHorizontal();

	    	advertisementStyle = advertisementStyle ?? new GUIStyle(GUI.skin.button);
	    	advertisementStyle.normal.background = null;

			foreach(AdvertisementThumb ad in advertisements)
			{
				if(ad.url.ToLower().Contains(ProductName.ToLower()))
					continue;

				if(GUILayout.Button(ad.guiContent, advertisementStyle,
					GUILayout.MinWidth(AD_HEIGHT), GUILayout.MaxWidth(AD_HEIGHT),
					GUILayout.MinHeight(AD_HEIGHT), GUILayout.MaxHeight(AD_HEIGHT)))
				{
					Application.OpenURL(ad.url);
				}
			}
			GUILayout.EndHorizontal();
			EditorGUILayout.EndScrollView();
	#endif
		}

		/**
		 * Draw a horizontal line across the screen and update the guilayout.
		 */
		void HorizontalLine()
		{
			Rect r = GUILayoutUtility.GetLastRect();
			Color og = GUI.backgroundColor;
			GUI.backgroundColor = Color.black;
			GUI.Box(new Rect(0f, r.y + r.height + 2, Screen.width, 2f), "");
			GUI.backgroundColor = og;

			GUILayout.Space(6);
		}

		/**
		 * Open VersionInfo and Changelog and pull out text to populate vars for OnGUI to display.
		 */
		void PopulateDataFields(string entryPath)
		{
			if(!File.Exists(entryPath))
				return;

			string versionInfo = File.ReadAllText(entryPath);

			ProductVersion = "";
			ChangelogPath = "";

			if(!string.IsNullOrEmpty(versionInfo))
			{
				string[] txt = versionInfo.Split('\n');

				foreach(string line in txt)
				{
					if(line.StartsWith("version:"))
						ProductVersion = line.Replace("version: ", "").Trim();
					else
					if(line.StartsWith("changelog:"))
						ChangelogPath = line.Replace("changelog: ", "").Trim();
				}
			}

			// Get first entry in changelog.txt
			TextAsset changelogText = LoadAssetAtPath<TextAsset>( ChangelogPath );

			if(changelogText)
			{
				pb_VersionInfo vi;

				if(FormatChangelog(changelogText.text, out vi, out changelog))
					ProductVersion = vi.text;
			}
		}

		public static bool FormatChangelog(string raw, out pb_VersionInfo version, out string formatted_changes)
		{
			bool success = true;

			// get first version entry
			string[] split = Regex.Split(raw, "(?mi)^#\\s", RegexOptions.Multiline);

			// get the version info
			try
			{
				Match versionMatch = Regex.Match(split[1], @"(?<=^ProBuilder\s).[0-9]*\.[0-9]*\.[0-9]*[a-z][0-9]*");
				version = pb_VersionInfo.FromString(versionMatch.Success ? versionMatch.Value : split[1].Split('\n')[0]);
			}
			catch
			{
				version = pb_VersionInfo.FromString("not found");
				success = false;
			}

			try
			{
				StringBuilder sb = new StringBuilder();
				string[] newLineSplit = split[1].Trim().Split('\n');
				for(int i = 2; i < newLineSplit.Length; i++)
					sb.AppendLine(newLineSplit[i]);

				formatted_changes = sb.ToString();
				formatted_changes = Regex.Replace(formatted_changes, "^-", "\u2022", RegexOptions.Multiline);
				formatted_changes = Regex.Replace(formatted_changes, @"(?<=^##\\s).*", "<size=16><b>${0}</b></size>", RegexOptions.Multiline);
				formatted_changes = Regex.Replace(formatted_changes, @"^##\ ", "", RegexOptions.Multiline);
			}
			catch
			{
				formatted_changes = "";
				success = false;
			}

			return success;
		}

		private static bool GetField(string path, string field, out string value)
		{
			value = "";

	        if (!File.Exists(path))
	        	return false;

			foreach(string str in File.ReadAllLines(path))
			{
				if(str.Contains(field))
				{
					value = str.Replace(field, "").Trim();
					return true;
				}
			}

			return false;
		}
	}
}

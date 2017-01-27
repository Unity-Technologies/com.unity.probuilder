using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

/**
 * INSTRUCTIONS
 *
 *  - Only modify properties in the USER SETTINGS region.
 *	- All content is loaded from external files (pc_AboutEntry_YourProduct.  Use the templates!
 */

/**
 * Used to pop up the window on import.
 */
public class pb_AboutWindowSetup : AssetPostprocessor
{
	static void OnPostprocessAllAssets (
		string[] importedAssets,
		string[] deletedAssets,
		string[] movedAssets,
		string[] movedFromAssetPaths)
	{

		string[] entries = System.Array.FindAll(importedAssets, name => name.Contains("pc_AboutEntry") && !name.EndsWith(".meta"));
		
		foreach(string str in entries)
			if( pb_AboutWindow.Init(str, false) )
				break;
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
	const string ABOUT_ROOT = "Assets/ProCore/" + PACKAGE_NAME + "/About";
	
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

	GUIContent gc_Learn = new GUIContent("Learn ProBuilder", "Video Tutorials");
	GUIContent gc_Forum = new GUIContent("Support Forum", "ProCore Support Forum");
	GUIContent gc_Contact = new GUIContent("Contact Us", "Send us an email!");

	// Use less contast-y white and black font colors for better readabililty
	private static readonly Color font_white = new Color(0.8235f, 0.8235f, 0.8235f, 1f);
	private static readonly Color font_black = new Color(0.1765f, 0.1765f, 0.1765f, 1f);
	private static readonly Color font_blue = new Color(0.03f, 0.62f, 0.93f, 1f);

	private string AboutEntryPath = "";
	private string ProductName = "";
	private string ProductVersion = "";
	private string ChangelogPath = "";
	private string BannerPath = ABOUT_ROOT + "/Images/Banner.png";
	private Texture2D banner;
	private string changelog = "";
	private string productName = pb_Constant.PRODUCT_NAME;

	private GUIStyle bannerStyle = new GUIStyle()
	{
		// RectOffset(left, right, top, bottom)
		margin = new RectOffset(12, 12, 14, 12)
	};

	private GUIStyle header1Style = new GUIStyle()
	{
		margin = new RectOffset(10, 10, 10, 10),
		alignment = TextAnchor.MiddleCenter,
		fontSize = 24,
		normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? font_white : font_black }
	};

	private GUIStyle versionInfoStyle = new GUIStyle()
	{
		margin = new RectOffset(10, 10, 10, 10),
		alignment = TextAnchor.MiddleLeft,
		fontSize = 14,
		fontStyle = FontStyle.Bold,
		normal = new GUIStyleState() { textColor = EditorGUIUtility.isProSkin ? font_white : font_black }
	};

	private GUIStyle linkStyle = new GUIStyle()
	{
		margin = new RectOffset(10, 10, 10, 10),
		alignment = TextAnchor.MiddleCenter,
		fontSize = 14,
		normal = new GUIStyleState() { textColor = font_blue }
	};

	private GUIStyle changelogStyle = new GUIStyle()
	{
		margin = new RectOffset(10, 10, 10, 10),
		normal = new GUIStyleState() { background = LoadAssetAtPath<Texture2D>(string.Format("{0}/Images/ScrollBackground_Pro.png", ABOUT_ROOT)) }
	};

	Vector2 scroll = Vector2.zero;

	/**
	 * Return true if Init took place, false if not.
	 */
	public static bool Init (string aboutEntryPath, bool fromMenu)
	{
		string identifier, version;

		if( !GetField(aboutEntryPath, "version: ", out version) || !GetField(aboutEntryPath, "identifier: ", out identifier))
			return false;

		if(fromMenu || EditorPrefs.GetString(identifier) != version)
		{
			string tname;	
			pb_AboutWindow win;

			if(GetField(aboutEntryPath, "name: ", out tname))
				win = (pb_AboutWindow)EditorWindow.GetWindow(typeof(pb_AboutWindow), true, tname, true);
			else
				win = (pb_AboutWindow)EditorWindow.GetWindow(typeof(pb_AboutWindow));

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

	public void OnEnable()
	{
		banner = LoadAssetAtPath<Texture2D>(BannerPath);

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

	static T LoadAssetAtPath<T>(string InPath) where T : UnityEngine.Object
	{
		return (T) AssetDatabase.LoadAssetAtPath(InPath, typeof(T));
	}

	void OnGUI()
	{
		if( GUILayout.Button(banner, bannerStyle) )
			Debug.Log("@todo");

		GUILayout.Label(productName, header1Style);

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

			if(GUILayout.Button(gc_Learn, linkStyle))
				Debug.Log("@todo");

			if(GUILayout.Button(gc_Forum, linkStyle))
				Debug.Log("@todo");

			if(GUILayout.Button(gc_Contact, linkStyle))
				Debug.Log("@todo");

		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		// always bold the first line (cause it's the version info stuff)
		scroll = EditorGUILayout.BeginScrollView(scroll, changelogStyle);
		GUILayout.Label(string.Format("Version: {0}", ProductVersion), versionInfoStyle);
		GUILayout.Label("\n" + changelog);
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
		/* Get data from VersionInfo.txt */
		TextAsset versionInfo = LoadAssetAtPath<TextAsset>( entryPath );
		
		ProductName = "";
		ProductVersion = "";
		ChangelogPath = "";

		if(versionInfo != null)
		{
			string[] txt = versionInfo.text.Split('\n');
			foreach(string cheese in txt)
			{
				if(cheese.StartsWith("name:")) 
					ProductName = cheese.Replace("name: ", "").Trim();
				else 
				if(cheese.StartsWith("version:"))
					ProductVersion = cheese.Replace("version: ", "").Trim();
				else 
				if(cheese.StartsWith("changelog:"))
					ChangelogPath = cheese.Replace("changelog: ", "").Trim();
			}
		}

		// Get first entry in changelog.txt
		TextAsset changelogText = LoadAssetAtPath<TextAsset>( ChangelogPath );

		if(changelogText)
		{
			string[] split = Regex.Split(changelogText.text, "(?mi)^#\\s", RegexOptions.Multiline);
			StringBuilder sb = new StringBuilder();
			string[] newLineSplit = split[1].Trim().Split('\n');
			for(int i = 2; i < newLineSplit.Length; i++)
				sb.AppendLine(newLineSplit[i]);
			
			changelog = sb.ToString();
		}
	}

	private static bool GetField(string path, string field, out string value)
	{
		TextAsset entry = LoadAssetAtPath<TextAsset>(path);

		value = "";

		if(!entry) return false;

		foreach(string str in entry.text.Split('\n'))
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

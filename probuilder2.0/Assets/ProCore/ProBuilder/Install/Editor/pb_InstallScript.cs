/**
 * Defines the minimum available Unity version.
 */

#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_4_6_1 || UNITY_4_7
#define UNITY_4_3
#endif

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.IO;

// class pb_InstallPostProcessor : AssetPostprocessor 
// {

// #if !PROTOTYPE
// 	const string PACKNAME = "ProBuilder";
// #else
// 	const string PACKNAME = "Prototype";
// #endif

// 	static void OnPostprocessAllAssets (
// 		string[] importedAssets,
// 		string[] deletedAssets,
// 		string[] movedAssets,
// 		string[] movedFromAssetPaths)
// 	{
// 		Debug.Log("pb_InstallPostProcessor: OnPostprocessAllAssets");
// 		if( System.Array.Exists(importedAssets, element => element.Contains(".unitypackage") && element.Contains(PACKNAME)) )
// 		{
// 			pb_InstallScript.AttemptAutoInstall();
// 			Debug.Log("pb_InstallPostProcessor: Found Packs");
// 		}
// 	}
// }

[InitializeOnLoad]
class pb_InstallHook : Editor
{
	static pb_InstallHook()
	{
		pb_InstallScript.AttemptAutoInstall();
	}
}

public class pb_InstallScript : EditorWindow
{
#if !PROTOTYPE
	const string PACKNAME = "ProBuilder";
#else
	const string PACKNAME = "Prototype";
#endif

	/**
	 * Release (use Dll) or Source (no Dll).
	 */
	private enum InstallType
	{
		Release,
		#if !PROTOTYPE
		Source
		#endif
	}

	static bool probuilderExists = false;
	static bool needsOrganized = false;

	/**
	 * Attempt to install package without user interaction.
	 */
	public static void AttemptAutoInstall()
	{
		CloseProBuilderWindow();
		EditorApplication.delayCall += InstallProBuilder;
	}

	[MenuItem("Tools/" + PACKNAME + "/Install or Update", false, 0)]
	public static void MenuInitGraphics()
	{
		string path;
		probuilderExists = FindFile("ProBuilderCore.dll", out path) || FindFile("pb_Object.cs", out path);

		EditorWindow.GetWindow<pb_InstallScript>(true, PACKNAME + (probuilderExists ? " Update" : " Install"), true).Show();
	}

	private static void InstallProBuilder()
	{
		string pbcore_path;
		probuilderExists = FindFile("ProBuilderCore.dll", out pbcore_path) || FindFile("pb_Object.cs", out pbcore_path);
	
		string pbeditor_path;
		if( !FindFile("ProBuilderEditor.dll", out pbeditor_path) )
			FindFile("pb_Editor.cs", out pbeditor_path);

		needsOrganized = probuilderExists && (
			!pbcore_path.Replace("\\", "/").Contains("Assets/ProCore/" + PACKNAME + "/Classes") ||
			!pbeditor_path.Replace("\\", "/").Contains("Assets/ProCore/" + PACKNAME + "/Editor") );

		/* See if ProBuilder already exists, and if so, if it's in the correct directory */
		if(probuilderExists)
		{
			if(!needsOrganized || UpdateOrganizePromptContinue())
			{
				// cast int to enum cause if Prototype, the Source option doesn't
				ImportLatestPack( (InstallType)(pbcore_path.Contains(".dll") ? (InstallType)0 : (InstallType)1) );

				return;
			}
		}

		#if PROTOTYPE
		if(!probuilderExists)
			ImportLatestPack(InstallType.Release);
		else
			EditorWindow.GetWindow<pb_InstallScript>(true, PACKNAME + " Install", true).Show();
		#else
		// If this isn't an upgrade, or the upgrade failed for whatever reason, show the dialogue
		EditorWindow.GetWindow<pb_InstallScript>(true, PACKNAME + " Install", true).Show();
		#endif	
	}

	/**
	 * Asks the user to continue upgrading in the event that they have moved the PB directory.
	 */
	static bool UpdateOrganizePromptContinue()
	{
		return EditorUtility.DisplayDialog("Continue Installation?", "Install script has detected that the " + PACKNAME + " folder has been moved from the \"Assets/ProCore/\" path.  This means the upgrade process will not be able to preserve script references, and you may introduce duplicate namespace errors.\n\nYou can exit the install process and move " + PACKNAME + " back to the ProCore directory, or continue anyway.", "Continue Install", "Cancel");
	}

	GUIStyle headerStyle = new GUIStyle();
	InstallType install = InstallType.Release;

	void OnGUI()
	{
		Color oldBGC = GUI.backgroundColor;

		if(EditorGUIUtility.isProSkin)
			headerStyle.normal.textColor = new Color(1f, 1f, 1f, .8f);
		else
			headerStyle.normal.textColor = Color.black;

		headerStyle.alignment = TextAnchor.MiddleCenter;
		headerStyle.fontSize = 14;
		headerStyle.fontStyle = FontStyle.Bold;

		GUILayout.Label(PACKNAME + " Install Tool", headerStyle);

		GUILayout.Space(10);

		GUI.color = Color.white;
		
		GUILayout.BeginHorizontal();

			GUILayout.BeginVertical();
				int installTypeEnumWidth = (int)Mathf.Max(Screen.width/2f - 8, 96);

				GUILayout.Label("Install Type");
				install = (InstallType)EditorGUILayout.EnumPopup(install, GUILayout.MaxWidth(installTypeEnumWidth));
			GUILayout.EndVertical();

			if(needsOrganized)	
				GUI.enabled = false;

			GUI.backgroundColor = Color.green;
			if(GUILayout.Button("Install", GUILayout.MinWidth(Mathf.Min(Screen.width - installTypeEnumWidth - 8, 96)), GUILayout.MinHeight(32)))
			{
				if(probuilderExists)
					if(!EditorUtility.DisplayDialog("Install " + PACKNAME + " Update", "Install " + PACKNAME + "\n\nWarning!  Back up your project!",
						"Run Update", "Cancel"))
						return;
				
				ImportLatestPack( install );
				
				this.Close();
			}
			
			GUI.enabled = true;

			GUI.backgroundColor = Color.white;

		GUILayout.EndHorizontal();

		TextAnchor ta = GUI.skin.box.alignment;
		GUI.skin.box.alignment = TextAnchor.MiddleLeft;
		Color oldBoxTC = GUI.skin.box.normal.textColor;

		if(EditorGUIUtility.isProSkin)
			GUI.skin.box.normal.textColor = new Color(1f, 1f, 1f, .65f);

		GUILayout.Space(4);

		if(needsOrganized)
		{
			EditorGUILayout.HelpBox("Install Script has detected ProBuilder exists in this project, but is not in the \"Assets/ProCore/\" folder.\n\nTo upgrade your project without losing your work, please manually move the ProBuilder folder to \"Assets/ProCore/\".", MessageType.Warning);
		}

		switch(install)
		{
			case InstallType.Release:
				GUILayout.Box("Release is the standard installation type.  It provides pre-compiled " + PACKNAME + " libraries instead of source code, meaning Unity doesn't need to compile extra files.\n\n*Note that all " + PACKNAME + " `Actions` and example scene files are still provided as source code.");
				break;
			#if !PROTOTYPE
			case InstallType.Source:
				GUILayout.Box(PACKNAME + " will be installed with full source code.  Note that you will need to remove any previous "+ PACKNAME + " \"Release\" installations prior to installing a Source version.  This is not recommended for users upgrading from a prior installation, as you *will* lose all prior-built " + PACKNAME + " objects.");
				break;
			#endif
		}

		GUILayout.Space(4);
		GUI.skin.box.alignment = ta;
		GUI.skin.box.normal.textColor = oldBoxTC;

		GUI.backgroundColor = oldBGC;
	}

	/**
	 * Can't reference ProBuilder classes, so do some hacky workaround
	 */
	private static void CloseProBuilderWindow()
	{
		IEnumerable<EditorWindow> ew = (Resources.FindObjectsOfTypeAll(typeof(EditorWindow)) as EditorWindow[]).Where(x => x.GetType().ToString().Contains("pb_Editor"));

		foreach(EditorWindow win in ew)
			win.Close();
	}

	/**
	 * Always install the latest.  
	 */
	private static void ImportLatestPack(InstallType i)
	{
		string[] packs = GetProBuilderPacks(i == InstallType.Release ? "-unity" : "-source");
		int tmp;
		int high = GetHighestVersion(packs, out tmp);

		LoadPack( packs[high] );//foundPacks[selectedPack]);

		/* nuke this installer and any other stuff */
		PostInstallCleanup();
	}

	private static void PostInstallCleanup()
	{
		DeleteFile("pb_InstallScript.cs");
	}

	private static void LoadPack(string pb_path)
	{
		#if !UNITY_STANDALONE_OSX && !UNITY_IPHONE
		pb_path = pb_path.Replace("\\", "/");
		#endif		

		if(pb_path == "") return;
		
		AssetDatabase.ImportPackage(pb_path, false);
	}

#region FILE UTILITY

	/**
	 * Accepts just the file name and searchs all directories for it.
	 */
	private static void DeleteFile(string str)
	{
		string[] files = System.IO.Directory.GetFiles("Assets/", str, System.IO.SearchOption.AllDirectories);

		foreach(string file in files)
		{
			// try to be a little more specific than just chekcng the file name.
			if( InProCorePath(file) )
				AssetDatabase.DeleteAsset(file);
		}
	}

	/**
	 * Find a file or directory with partial path (usually file name).
	 */
	private static bool FindFile(string fileName, out string path)
	{
		if( fileName.Contains(".") )
		{
			string[] allFiles = Directory.GetFiles("Assets/", "*.*", SearchOption.AllDirectories);
			string[] matches = System.Array.FindAll(allFiles, name => name.Replace("\\", "/").Contains(fileName));

			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			foreach(string str in allFiles)
				sb.AppendLine(str);

			if(matches.Length > 0)
			{
				path = matches[0];
				return true;
			}
			else
			{
				path = "";
				return false;
			}
		}
		else
		{
			string[] allDir = Directory.GetDirectories("Assets/", "*", SearchOption.AllDirectories);
			string[] matches = System.Array.FindAll(allDir, element => element.Replace("\\", "/").Contains(fileName));
			path = matches.Length > 0 ? matches[0] : "";
			return matches.Length > 0;
		}
	}
	/**
	 * Assumes packages have already been filtered
	 */
	private static int GetHighestVersion(string[] packs, out int revision)//, string match)
	{
		// sort out non ProBuilder2v- packages
		int highestVersion = 0;
		int index = 0;
		
		for(int i = 0; i < packs.Length; i++)
		{
			string pattern = @"[v0-9]{4,6}";
			MatchCollection matches = Regex.Matches(packs[i], pattern, RegexOptions.IgnorePatternWhitespace);

			revision = -1;
			foreach(Match m in matches)
				revision = int.Parse(m.ToString().Replace("v", ""));
			
			if(revision < 1)
				continue;

			if(revision > highestVersion) {
				highestVersion = revision;
				index = i;
			}
		}
		revision = highestVersion;
		return index;
	}


	static bool InProCorePath(string file)
	{
		return (file.Contains("6by7") || file.Contains("ProCore")) && (file.Contains("ProBuilder") || file.Contains("Prototype"));
	}

	private static string[] GetProBuilderPacks(string match)
	{
		string[] allFiles = Directory.GetFiles("Assets/", "*.*", SearchOption.AllDirectories);
		
		#if PROTOTYPE
		string pack = "Prototype-v";
		#else
		string pack = "ProBuilder2-v";
		#endif

		#if UNITY_5
		string[] allPackages = System.Array.FindAll(allFiles, name =>
			name.EndsWith(".unitypackage") &&
			name.Contains(pack) &&
			name.Contains(match) &&
			!name.Contains("unity5")
			);
		#else
		string[] allPackages = System.Array.FindAll(allFiles, name =>
			name.EndsWith(".unitypackage") &&
			name.Contains(pack) &&
			name.Contains(match) &&
			!name.Contains("unity4")
			);
		#endif

		return allPackages;
	}
#endregion
}

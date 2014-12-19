#if UNITY_4_3 || UNITY_4_5 || UNITY_4_6 || UNITY_5 || UNITY_5_0
#define UNITY_4_3
#define UNITY_4
#elif UNITY_3_0 || UNITY_3_0_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5
#define UNITY_3
#endif

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Linq;
using System.IO;

class QuickStartPostProcessor2 : AssetPostprocessor 
{

#if !PROTOTYPE
	const string PACKNAME = "ProBuilder";
#else
	const string PACKNAME = "Prototype";
#endif

	static void OnPostprocessAllAssets (
		string[] importedAssets,
		string[] deletedAssets,
		string[] movedAssets,
		string[] movedFromAssetPaths)
	{
		if( System.Array.Exists(importedAssets, element => element.Contains(".unitypackage") && element.Contains(PACKNAME)) )
		{
			QuickStart2.AttemptAutoInstall();
		}
	}
}

public class QuickStart2 : EditorWindow
{
#if !PROTOTYPE
	const string PACKNAME = "ProBuilder";
#else
	const string PACKNAME = "Prototype";
#endif

	private enum InstallType
	{
		Release,
		#if !PROTOTYPE
		Source
		#endif
	}

	static string[] FILES_TO_DELETE = new string[]
	{
		"VertexColorInterface.cs",
		"pb_Upgrade_Utility.cs",
		"MirrorTool.cs",
		"pbVersionBridge",
		"pb_About.cs",
		"SetProjectionAxis.cs"
	};

	static bool probuilderExists = false;
	static bool needsOrganized = false;

	public static void AttemptAutoInstall()
	{
		CloseProBuilderWindow();

		CreateProCoreDirectories();
		EditorApplication.delayCall += InstallProBuilder;
	}

	[MenuItem("Tools/" + PACKNAME + "/Install or Update", false, 0)]
	public static void MenuInitGraphics()
	{
		EditorWindow.GetWindow<QuickStart2>(true, PACKNAME + " Install", true).Show();
	}

	private static void InstallProBuilder()
	{
		RemoveOldInstallScript();

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
			if(needsOrganized)
			{
				MoveOldFiles();
			}

			if(!needsOrganized)
			{
				// success!  do the upgrade
				foreach(string str in FILES_TO_DELETE)
					DeleteFile(str);

				// 2.3 Needs to show users a message, so if upgrading show a window
				EditorPrefs.SetBool("pbShowUpgradeDialog", true);

				ImportPack( (InstallType)(pbcore_path.Contains(".dll") ? (InstallType)0 : (InstallType)1) );

				return;
			}
		}

		#if PROTOTYPE
		if(!probuilderExists)
			ImportPack(InstallType.Release);
		else
			EditorWindow.GetWindow<QuickStart2>(true, PACKNAME + " Install", true).Show();
		#else
		// If this isn't an upgrade, or the upgrade failed for whatever reason, show the dialogue
		EditorWindow.GetWindow<QuickStart2>(true, PACKNAME + " Install", true).Show();
		#endif	
	}

	static void RemoveOldInstallScript()
	{
		string[] allFiles = System.IO.Directory.GetFiles("Assets/", "QuickStart.cs.*", System.IO.SearchOption.AllDirectories);
		string[] matches = System.Array.FindAll(allFiles, name => InProCorePath(name) && !name.Contains(".meta") );

		/* close the old window */
		EditorWindow[] edWins = (Resources.FindObjectsOfTypeAll(typeof(EditorWindow)) as EditorWindow[]);
		EditorWindow[] ew = (EditorWindow[])System.Array.FindAll(edWins, x => x.GetType().ToString() == "QuickStart");
		if(ew != null)
			foreach(EditorWindow e in ew)
				e.Close();


		for(int i = 0; i < matches.Length; i++)
		{
			// Debug.Log("matches: " + matches[i]);
			AssetDatabase.DeleteAsset(matches[i]);
		}
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
			
				foreach(string str in FILES_TO_DELETE)
					DeleteFile(str);
				
				ImportPack( install );
				
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
			EditorGUILayout.HelpBox("Install Script has detected ProBuilder exists in this project, but is not in the \"Assets/6by7/\" folder.\n\nTo upgrade your project without losing your work, please manually move the ProBuilder folder to \"Assets/6by7/\".\n\nClick the \"Continue\" button once you've moved ProBuilder's folders.", MessageType.Warning);

			if(GUILayout.Button("Continue"))
			{
				MoveOldFiles();
			}
		}
		else
		{
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
		}
		GUILayout.Space(4);
		GUI.skin.box.alignment = ta;
		GUI.skin.box.normal.textColor = oldBoxTC;

		GUI.backgroundColor = oldBGC;
	}

	private static void MoveOldFiles()
	{
		string[] move = new string[]
		{
			PACKNAME + "/API Examples",
			PACKNAME + "/Classes",
			PACKNAME + "/Credits.txt",
			PACKNAME + "/Editor",
			PACKNAME + "/Resources",
			PACKNAME + "/Shader",
			"Shared/Code/SixBySeven.dll",
			"Shared/Resources/GUI",
		};

		List<FileMove> FilesToMoveOnUpgrade = new List<FileMove>();

		foreach(string str in move)
		{
			string path;
			if(FindFile(str, out path))
				FilesToMoveOnUpgrade.Add(new FileMove(path, "Assets/ProCore/" + str));						
		}

		if(VerifyOrganizeFiles(FilesToMoveOnUpgrade))
		{
			// File movements check out - do the thang
			foreach(FileMove fm in FilesToMoveOnUpgrade)
				fm.Move();

			needsOrganized = false;
		}
		else
			needsOrganized = true;
	}

	private static bool CreateProCoreDirectories()
	{
		if(!Directory.Exists("Assets/ProCore"))
			AssetDatabase.CreateFolder("Assets", "ProCore");

		if(!Directory.Exists("Assets/ProCore/" + PACKNAME))
			AssetDatabase.CreateFolder("Assets/ProCore", PACKNAME);

		if(!Directory.Exists("Assets/ProCore/Shared"))
			AssetDatabase.CreateFolder("Assets/ProCore", "Shared");

		if(!Directory.Exists("Assets/ProCore/Shared/Resources"))
			AssetDatabase.CreateFolder("Assets/ProCore/Shared", "Resources");

		if(!Directory.Exists("Assets/ProCore/Shared/Code"))
			AssetDatabase.CreateFolder("Assets/ProCore/Shared", "Code");

		return true;
	}

	/**
	 * Can't reference ProBuilder classes, so do some hacky workaround
	 */
	static void CloseProBuilderWindow()
	{
		List<EditorWindow> ew = (Resources.FindObjectsOfTypeAll(typeof(EditorWindow)) as EditorWindow[]).Where(x => x.GetType().ToString().Contains("pb_Editor")).ToList();

		for(int i = 0; i < ew.Count; i++)
			ew[i].Close();
	}

	/**
	 * Always install the latest.  
	 */
	private static void ImportPack(InstallType i)
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
		/* delete 6by7/ProBuilder folder, only accounting for most basic case */
		string old_pb_path = "";
		if(FindFile("6by7/" + PACKNAME, out old_pb_path))
			DeleteDirectory(old_pb_path);

		string six_path;
		if(FindFile("6by7", out six_path))
		{
			string[] files = Directory.GetFiles(six_path);
			// string[] folders = Directory.GetDirectories(six_path);
			if(files.Length < 1)// && folders.Length < 1)
				DeleteDirectory(six_path);
		}

		DeleteFile("QuickStart2.cs");
	}

	public static void LoadPack(string pb_path)
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
	 * Attempts to Delete directory at path.  Will also delete all files in said directory.
	 */
	public static void DeleteDirectory(string path)
	{
		if(!Directory.Exists(path))
			return;

		string[] files = Directory.GetFiles(path);
		string[] dirs = Directory.GetDirectories(path);

		foreach (string file in files)
		{
			File.SetAttributes(file, FileAttributes.Normal);
			File.Delete(file);
		}

		foreach (string dir in dirs)
		{
			DeleteDirectory(dir);
		}

		Directory.Delete(path, false);

		if(File.Exists(path+".meta"))
			File.Delete(path+".meta");
	}

	struct FileMove
	{
		public string source;
		public string destination;

		public FileMove(string source, string destination)
		{
			this.source = source;
			this.destination = destination;
		}

		public bool Verify()
		{
			return AssetDatabase.ValidateMoveAsset(source, destination) == "";
		}

		public void Move()
		{
			AssetDatabase.MoveAsset(source, destination);
		}

		public override string ToString()
		{
			return source + " -> " + destination;// + "  Valid: " + Verify();
		}
	}

	/**
	 * Returns true if all movements are confirmmed.
	 */
	private static bool VerifyOrganizeFiles(List<FileMove> files)
	{
		foreach(FileMove fm in files)
		{
			if(!fm.Verify())
			{
				Debug.LogWarning("Failed verifying move: " + fm.ToString());
				return false;
			}
		}
		return true;
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

		#if UNITY_4_3
		string[] allPackages = System.Array.FindAll(allFiles, name =>
			name.EndsWith(".unitypackage") &&
			name.Contains(pack) &&
			name.Contains(match) &&
			!name.Contains("unity35")
			);
		#else
		string[] allPackages = System.Array.FindAll(allFiles, name =>
			name.EndsWith(".unitypackage") &&
			name.Contains(pack) &&
			name.Contains(match) &&
			!name.Contains("unity43")
			);
		#endif

		return allPackages;
	}
#endregion
}

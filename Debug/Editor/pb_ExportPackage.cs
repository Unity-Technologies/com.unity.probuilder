using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.ProBuilder;

/**
 * Export Unity packages for release - usually called from command line.
 */
public class pb_ExportPackage : Editor
{
	const string PROBUILDERCORE_UNITY5_DLL_GUID		= "0472bdc8d6d15384d98f22ee34302f9c";
	const string PROBUILDERMESHOPS_UNITY5_DLL_GUID	= "b617d7797480df7499f141d87e13ebc5";
	const string KDTREE_DLL_GUID					= "2785d4fdbcaa3504ea87f76f0f2cdce3";
	const string PROBUILDEREDITOR_UNITY5_DLL_GUID	= "4df21bd079886d84699ca7be1316c7a7";

	[MenuItem("Tools/Debug/Test Export")]
	static void DOIT()
	{
		Export("ProCore", Application.dataPath, "ProBuilder2", "-unity4");
	}

	static string CHANGELOG_PATH { get { return "Assets/ProCore/" + "ProBuilder" + "/About/changelog.txt"; } }
	const string DateTimeFormat = "MM-dd-yyyy";

	public static void ExportCommandLine()
	{
		string[] args = System.Environment.GetCommandLineArgs();

		string sourceDir = "";
		string outDir = "";
		string outName = "";
		string outSuffix = "";

		foreach(string str in args)
		{
			if(str.StartsWith("sourceDir:"))
				sourceDir = str.Replace("sourceDir:", "").Trim();

			if(str.StartsWith("outDir:"))
				outDir = str.Replace("outDir:", "").Trim();

			if(str.StartsWith("outName:"))
				outName = str.Replace("outName:", "").Trim();

			if(str.StartsWith("outSuffix:"))
				outSuffix = str.Replace("outSuffix:", "").Trim();
		}

		Debug.Log("Exporting\n" + "Source: " + sourceDir + "\nOut: " + outDir + "\nName: " + outName + "\nSuffix: "+  outSuffix);

		Export(sourceDir, outDir, outName, outSuffix);
	}

	/**
	 * Recursively export a package from SourcePath.  SourcePath is relative to Assets/ directory.
	 */
	private static void Export(string sourceDirectory, string outDirectory, string outName, string suffix)
	{
		// Read version number and revision number from changelog.txt
		string version = GetVersionNumber();

		// Populate the about entry information text file.
		WriteAboutEntry(CHANGELOG_PATH, version);

		string outDir = outDirectory.Replace("\\", "/");

		if(!outDir.EndsWith("/"))
			outDir += "/";

		AssetDatabase.ExportPackage(
			"Assets/" + sourceDirectory,
			string.Format("{0}{1}-v{2}{3}.unitypackage", outDir, outName, version, suffix),
			ExportPackageOptions.Recurse);
	}

	/**
	 * Populate the AboutWindowEntry with version and dating information.
	 * Ex:
	 *
	 *	name: ProBuilder
	 *	identifier: ProBuilder2_AboutWindowIdentifier
	 *	version: 2.2.5b0
	 *	revision: 2176
	 *	date: 04-18-2014
	 *	changelog: Assets/changelog.txt
	 */
	private static void WriteAboutEntry(string changelog, string version, bool inEditor = false)
	{
		string versionInfoText =
			"name: " + PreferenceKeys.pluginTitle + "\n" +
			"identifier: " + PreferenceKeys.pluginTitle + "_AboutWindowIdentifier\n" +
			"version: " + version + "\n" +
			"date: " + System.DateTime.Now.ToString(DateTimeFormat) + "\n" +
			"changelog: " + changelog;

		string version_entry_path = "Assets/ProCore/" + "ProBuilder" + "/About/pc_AboutEntry_ProBuilder.txt";

		if(File.Exists(version_entry_path))
			File.Delete(version_entry_path);

		using (FileStream fs = File.Create(version_entry_path))
		{
			Byte[] contents = new UTF8Encoding(true).GetBytes(versionInfoText);
			fs.Write(contents, 0, contents.Length);
		}

		AssetDatabase.Refresh();

#if !UNITY_4_7 && !UNITY_5_0
		if(inEditor)
		{
			TextAsset res = AssetDatabase.LoadAssetAtPath<TextAsset>(version_entry_path);
			EditorGUIUtility.PingObject(res);
		}
#endif
	}

	[MenuItem("Tools/Debug/ProBuilder/Rebuild About Entry", false, 800)]
	private static void MenuWriteAboutEntry()
	{
		WriteAboutEntry(CHANGELOG_PATH, GetVersionNumber(), true);
	}


	private static string GetVersionNumber()
	{
		string version_number = "";

		using(StreamReader sr = new StreamReader(CHANGELOG_PATH))
		{
			for(int i = 0; i < 32; i++)
			{
				string line = sr.ReadLine();

				Match m = Regex.Match(line, @"(?<=^#\sProBuilder\s).[0-9]*\.[0-9]*\.[0-9]*[a-z][0-9]*");

				if(m.Success)
				{
					version_number = m.Value;

					if(i > 1)
						Debug.LogWarning("First matching changelog header was not the first line.");
					break;
				}
			}
		}

		return version_number;
	}

	/**
	 *	Since DLLs are always built and never exist in the project, their GUIDs are unique across installs.
	 *	We don't want that - they need to be uniform to update correctly.  This finds DLLs in the project
	 *	and replaces their GUID with the stored ID.
	 *	@todo Find a better solution than hard-coding GUIDs :/
	 */
	[MenuItem("Tools/Find DLLs")]
	public static void OverrideDLLGUIDs()
	{
		string[] core_dll = Directory.GetFiles("Assets", "ProBuilderCore-Unity*.dll", SearchOption.AllDirectories);
		string[] mesh_dll = Directory.GetFiles("Assets", "ProBuilderMeshOps-Unity*.dll", SearchOption.AllDirectories);
		string[] editor_dll = Directory.GetFiles("Assets", "ProBuilderEditor-Unity*.dll", SearchOption.AllDirectories);

		/// purposefully don't return from function if files aren't found - batch export should
		/// fail if these aren't modified.

		if(core_dll == null || core_dll.Length < 1)
			Debug.LogError("Could not find ProBuilderCore DLL");

		if(mesh_dll == null || mesh_dll.Length < 1)
			Debug.LogError("Could not find ProBuilderMeshOps DLL");

		if(editor_dll == null || editor_dll.Length < 1)
			Debug.LogError("Could not find ProBuilderEditor DLL");

		SetGUID(core_dll[0], PROBUILDERCORE_UNITY5_DLL_GUID);
		SetGUID(mesh_dll[0], PROBUILDERMESHOPS_UNITY5_DLL_GUID);
		SetGUID(editor_dll[0], PROBUILDEREDITOR_UNITY5_DLL_GUID);
	}

	private static void SetGUID(string asset_path, string new_guid)
	{
#if UNITY_4_7
		string meta_path = asset_path + ".meta";
#else
		string meta_path = AssetDatabase.GetTextMetaFilePathFromAssetPath(asset_path);
#endif
		StringBuilder sb = new StringBuilder();
		Encoding encoding;

		FileInfo meta_file = new FileInfo(meta_path);
		bool hiddenMetaFiles = (meta_file.Attributes & FileAttributes.Hidden) != 0;

		if(hiddenMetaFiles)
			meta_file.Attributes &= ~FileAttributes.Hidden;

		using (StreamReader sr = new StreamReader(meta_path))
		{
			string line;
			encoding = sr.CurrentEncoding;

			while ((line = sr.ReadLine()) != null)
			{
				if(line.Contains("guid: "))
					sb.AppendLine("guid: " + new_guid);
				else
					sb.AppendLine(line);
			}
		}

		using (StreamWriter writer = new StreamWriter(meta_path, false, encoding))
		{
			writer.Write(sb.ToString());
		}

		if(hiddenMetaFiles)
			meta_file.Attributes |= FileAttributes.Hidden;
	}
}


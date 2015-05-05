using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using System.Linq;
using System.Text.RegularExpressions;

/**
 * Export Unity packages for release - usually called from command line.
 */
public class pb_ExportPackage : Editor
{
	[MenuItem("Tools/TEST EXPORT")]
	static void DOIT()
	{
		Export("ProCore", Application.dataPath, "ProBuilder2", "-unity4");
	}

	static string CHANGELOG_PATH { get { return "Assets/ProCore/" + pb_Constant.PRODUCT_NAME + "/About/changelog.txt"; } }
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
	private static void Export(string SourceDirectory, string OutDirectory, string OutName, string suffix)
	{	
		// Read version number and revision number from changelog.txt
		#if UNITY_5_1
		TextAsset changelog = (TextAsset)AssetDatabase.LoadAssetAtPath(CHANGELOG_PATH, typeof(TextAsset));
		#else
		TextAsset changelog = (TextAsset)Resources.LoadAssetAtPath(CHANGELOG_PATH, typeof(TextAsset));
		#endif

		Regex reg = new Regex(@"([0-9].[0-9].[0-9][a-z][0-9]*)", RegexOptions.None);
		Match first = reg.Match(changelog.text);
		string version = first.Success ? first.Value : "Failed parsing version number!";

		reg = new Regex(@"(\(r[0-9]{4}\))", RegexOptions.None);
		first = reg.Match(changelog.text);
		string revision = first.Success ? first.Value.Replace("(r", "").Replace(")", "") : "Failed parsing SVN revision!";

		// Populate the about entry information text file.
		WriteAboutEntry(CHANGELOG_PATH, version, revision);

		AssetDatabase.ExportPackage("Assets/" + SourceDirectory, (OutDirectory.EndsWith("/") ? OutDirectory : OutDirectory + "/") + OutName + "-v" + revision + suffix + ".unitypackage", ExportPackageOptions.Recurse);
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
	private static void WriteAboutEntry(string changelog, string version, string svnRevision)
	{
		string versionInfoText = 
			"name: " + pb_Constant.PRODUCT_NAME + "\n" + 
			"identifier: " + pb_Constant.PRODUCT_NAME + "_AboutWindowIdentifier\n" +
			"version: " + version + "\n" +
			"revision: " + svnRevision + "\n" +
			"date: " + System.DateTime.Now.ToString(DateTimeFormat) + "\n" +
			"changelog: " + changelog;

		string version_entry_path = "Assets/ProCore/" + pb_Constant.PRODUCT_NAME + "/About/pc_AboutEntry_ProBuilder.txt";

		if(File.Exists(version_entry_path))
			File.Delete(version_entry_path);
		
		using (FileStream fs = File.Create(version_entry_path))
		{
			Byte[] contents = new UTF8Encoding(true).GetBytes(versionInfoText);
			fs.Write(contents, 0, contents.Length);
		}

		AssetDatabase.Refresh();
	}
}


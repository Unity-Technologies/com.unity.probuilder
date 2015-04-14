using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;

/**
 * Export Unity packages for release - usually called from command line.
 */
public class pb_ExportPackage : Editor
{
	const string DateTimeFormat = "MM-dd-yyyy";

	/**
	 * Appended to exported package name if not None.
	 */
	public enum VersionMarking
	{
		SVN,
		DateTime,
		None
	}

#region Export Methods

	public static void ExportReleaseResources()
	{
		string[] arg = System.Environment.GetCommandLineArgs();
		string[] addlIgnore = new string[0];
		string define = "";

		foreach(string str in arg)
		{
			if(str.StartsWith("ignore:"))
				addlIgnore = str.Replace("ignore:", "").Trim().Split(';');

			if(str.StartsWith("define:"))
				define = str.Replace("define:", "").Trim();
		}
		
		string[] ignore = new string[3 + addlIgnore.Length];
		ignore[0] = "ClassesCore";
		ignore[1] = "EditorCore";
		ignore[2] = "ClassesEditing";

		for(int i = 3; i < ignore.Length; i++)
			ignore[i] = addlIgnore[i-3];

		Export("Assets/ProCore", "../../bin/temp", "ProBuilder2(Resources)", VersionMarking.None, ignore, define);
	}

#endregion

#region Export Util

	private static void Export(string SourcePath, string DestPath, string Name, VersionMarking Mark, string[] IgnorePattern, string Define)
	{

	}
#endregion

#region File

	/**
	 * Recursively collect exportable files using an ignore pattern.
	 */
	private static List<string> CollectFilesRecursive(string InPath, string[] InIgnorePattern)
	{
		List<string> CollectedAssetPaths = new List<string>();

		foreach(string file in Directory.GetFiles(InPath))
		{
			bool skip = false;
			for(int i = 0; i < InIgnorePattern.Length; i++)
			{
				if(file.Contains(InIgnorePattern[i]))	
				{
					skip = true;
					break;
				}
			}

			if(!skip)
			{
				CollectedAssetPaths.Add(file);
			}
		}

		
		foreach(string directory in Directory.GetDirectories(InPath))
		{
			bool skip = false;
			for(int i = 0; i < InIgnorePattern.Length; i++)
			{
				if(directory.Contains(InIgnorePattern[i]))	
				{
					skip = true;
					break;
				}
			}

			if(!skip)
			{
				CollectedAssetPaths.AddRange( CollectFilesRecursive(directory, InIgnorePattern) );
			}
		}

		return CollectedAssetPaths;
	}


	public static void AddDefine(string path, string define)
	{
		StringBuilder sb = new StringBuilder();

		Encoding encoding;
		
		sb.AppendLine( "#define " + define);
		
		using (StreamReader sr = new StreamReader(path))
		{
			string line;
			encoding = sr.CurrentEncoding;
			while ((line = sr.ReadLine()) != null)
			{
				sb.AppendLine(line);
			}
		}
		
		using (StreamWriter writer = new StreamWriter(path, false, encoding))
		{
			writer.Write(sb.ToString());
		}
	}
#endregion
}


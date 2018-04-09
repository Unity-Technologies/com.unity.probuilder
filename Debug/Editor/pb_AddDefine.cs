using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;

/**
 *	An editor script meant to be called from the command line to automatically add a #define to every
 *	C# file in a given folder.
 */
public class pb_AddDefine : Editor
{
	/**
	 *	Call from command line with args to prepend a define line to every file in a folder.
	 *	Args:
	 *		- define: The define to sign files with.
	 *		- ignore: File or folder names to ignore when collecting C# files to prepend.
	 *		- folder: The root folder to scan for C# files.  Relative to project root (ex, Assets/MyPlugin/).
	 */
	public static void PrependDefine()
	{
		string[] arg = System.Environment.GetCommandLineArgs();
		string define = "";
		string[] ignore = new string[0];
		string folder = "Assets/ProCore/ProBuilder";

		foreach(string str in arg)
		{
			if(str.StartsWith("define:"))
				define = str.Replace("define:", "").Trim();
			else if(str.StartsWith("ignore:"))
				ignore = str.Replace("ignore:", "").Trim().Split(';');
			else if(str.StartsWith("folder:"))
				folder = str.Replace("folder:", "").Trim();
		}

		// new string[] {".meta", "Debug", "AutomatedExport.cs"}
		foreach(string str in CollectFiles(folder, new HashSet<string>(ignore)))
		{
			if(str.EndsWith(".cs"))
				AddDefine(str, define);
		}
	}

	private static List<string> CollectFiles(string path, HashSet<string> ignore)
	{
		List<string> matches = new List<string>();

		foreach(string file in Directory.GetFiles(path))
		{
			if(ignore.Contains(Path.GetFileName(file)))
				continue;

			matches.Add(file);
		}
		
		foreach(string directory in Directory.GetDirectories(path))
		{
			string dirName = directory.Length > 0 ?
				directory.Substring( directory.LastIndexOf("\\") + 1 ) : "";

			if( !ignore.Contains(dirName))
			{
				matches.AddRange( CollectFiles(directory, ignore) );
			}
		}

		return matches;
	}

	private static void AddDefine(string path, string define)
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
}

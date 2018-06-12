using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

class CsProjectPostProcessor : AssetPostprocessor
{
	static readonly string[,] k_AddlProjects = new string[2,2]
	{
#if UNITY_EDITOR_WIN
		{ @"..\..\unity\tools\Projects\CSharp\UnityEngine.csproj", "414FBAF2-1014-415F-9B26-A8C2D1CB2201" },
		{ @"..\..\unity\tools\Projects\CSharp\UnityEditor.csproj", "8329E01A-E504-4500-8D04-059C9BD10068" },
#else
		{ @"../../unity/tools/Projects/CSharp/UnityEngine.csproj", "414FBAF2-1014-415F-9B26-A8C2D1CB2201" },
		{ @"../../unity/tools/Projects/CSharp/UnityEditor.csproj", "8329E01A-E504-4500-8D04-059C9BD10068" },
#endif
	};

	static void OnGeneratedCSProjectFiles()
	{
		foreach (var sln in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.sln"))
			AppendProjects(sln);
	}

	static void AppendProjects(string sln)
	{
		var sr = new StringReader(File.ReadAllText(sln));
		var sb = new StringBuilder();
		string slnGuid = "";

		while (sr.Peek() > -1)
		{
			var line = sr.ReadLine();

			if(string.IsNullOrEmpty(slnGuid) && line.StartsWith("Project(\""))
				slnGuid = line.Substring(10, 36);

			// end of projects
			if (line.Equals("Global"))
			{
				for(int i = 0, c = k_AddlProjects.Length; i < c; i++)
				{
					var proj = k_AddlProjects[i, 0];
					var name = Path.GetFileNameWithoutExtension(k_AddlProjects[i, 0]);
					var guid = Path.GetFileNameWithoutExtension(k_AddlProjects[i, 1]);

					sb.AppendLine(string.Format("Project(\"{{{0}}}\") = \"{1}\", \"{2}\", \"{{{3}}}\"", slnGuid, name, proj, guid));
					sb.AppendLine("EndProject");
				}
			}

			sb.AppendLine(line);

			if (line.Contains("GlobalSection(ProjectConfigurationPlatforms)"))
			{
				for(int i = 0, c = k_AddlProjects.Length; i < c; i++)
				{
					var guid = k_AddlProjects[i, 1];

					sb.AppendLine(string.Format("\t\t{{{0}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU", guid));
					sb.AppendLine(string.Format("\t\t{{{0}}}.Debug|Any CPU.Build.0 = Debug|Any CPU", guid));
					sb.AppendLine(string.Format("\t\t{{{0}}}.Release|Any CPU.ActiveCfg = Release|Any CPU", guid));
					sb.AppendLine(string.Format("\t\t{{{0}}}.Release|Any CPU.Build.0 = Release|Any CPU", guid));
				}
			}
		}

		sr.Dispose();

		File.WriteAllText(sln, sb.ToString());
	}
}

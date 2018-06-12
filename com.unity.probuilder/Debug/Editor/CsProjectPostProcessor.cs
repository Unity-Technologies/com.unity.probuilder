using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

class CsProjectPostProcessor : AssetPostprocessor
{
	static readonly string[] k_AddlProjects = new string[]
	{
#if UNITY_EDITOR_WIN
		@"..\..\unity\tools\Projects\CSharp\UnityEngine.csproj",
		@"..\..\unity\tools\Projects\CSharp\UnityEditor.csproj",
#else
		@"../../unity/tools/Projects/CSharp/UnityEngine.csproj",
		@"../../unity/tools/Projects/CSharp/UnityEditor.csproj",
#endif
	};

	static void OnGeneratedCSProjectFiles()
	{
		foreach (var sln in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.sln"))
			AppendProjects(sln);
	}

	static void AppendProjects(string sln)
	{
		Debug.Log("yo");

		var sr = new StringReader(File.ReadAllText(sln));
		var sb = new StringBuilder();
		string slnGuid = "";
		var prjGuid = new List<string>();

		while (sr.Peek() > -1)
		{
			var line = sr.ReadLine();

			if(string.IsNullOrEmpty(slnGuid) && line.StartsWith("Project(\""))
				slnGuid = line.Substring(10, 36);

			// end of projects
			if (line.Equals("Global"))
			{
				foreach (var prj in k_AddlProjects)
				{
					var name = Path.GetFileNameWithoutExtension(prj);
					var guid = System.Guid.NewGuid().ToString().ToUpper();
					sb.AppendLine(string.Format("Project(\"{{{0}}}\") = \"{1}\", \"{2}\", \"{{{3}}}\"", slnGuid, name, prj, guid));
					sb.AppendLine("EndProject");
					prjGuid.Add(guid);
				}
			}

			sb.AppendLine(line);

			if (line.Contains("GlobalSection(ProjectConfigurationPlatforms)"))
			{
				foreach (var g in prjGuid)
				{
					sb.AppendLine(string.Format("\t\t{{{0}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU", g));
					sb.AppendLine(string.Format("\t\t{{{0}}}.Debug|Any CPU.Build.0 = Debug|Any CPU", g));
					sb.AppendLine(string.Format("\t\t{{{0}}}.Release|Any CPU.ActiveCfg = Release|Any CPU", g));
					sb.AppendLine(string.Format("\t\t{{{0}}}.Release|Any CPU.Build.0 = Release|Any CPU", g));
				}
			}
		}

		sr.Dispose();

		File.WriteAllText(sln, sb.ToString());
	}
}

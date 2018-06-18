using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

class CsSolutionSettings : ScriptableSingleton<CsSolutionSettings>
{
	public string[] additionalProjects = new string[0];
}

static class CsSolutionSettingsEditor
{
	[PreferenceItem("C# Solution")]
	static void CsSolutionSettingsPrefs()
	{
		var settings = CsSolutionSettings.instance;


		GUILayout.Label("Additional C# Projects", EditorStyles.boldLabel);

		var prj = settings.additionalProjects;

		for (int i = 0, c = prj.Length; i < c; i++)
		{
			GUILayout.BeginHorizontal();
			GUILayout.TextField(prj[i]);
			if (GUILayout.Button("..."))
				prj[i] = EditorUtility.OpenFilePanelWithFilters("C# Project", "../", new string[] { "Project", "proj" });
			if (GUILayout.Button("Remove"))
			{
				ArrayUtility.RemoveAt(ref prj, i);
				settings.additionalProjects = prj;
				EditorUtility.SetDirty(settings);
				GUIUtility.ExitGUI();
			}

			GUILayout.EndHorizontal();
		}

		if (GUILayout.Button("Add"))
		{
			var add = EditorUtility.OpenFilePanelWithFilters("C# Project", "../", new string[] { "Project", "csproj" });

			if (!string.IsNullOrEmpty(add))
			{
				ArrayUtility.Add(ref prj, add);
				settings.additionalProjects = prj;
				EditorUtility.SetDirty(settings);
			}
		}
	}
}

class CsProjectPostProcessor : AssetPostprocessor
{
	static void OnGeneratedCSProjectFiles()
	{
		foreach (var sln in Directory.GetFiles(Directory.GetCurrentDirectory(), "*.sln"))
			AppendProjects(sln);
	}

	static void AppendProjects(string sln)
	{
		var settings = CsSolutionSettings.instance;

		var slnText = File.ReadAllText(sln);

		var addProjects = new Dictionary<string, string>();

		foreach (var prj in settings.additionalProjects)
		{
			if (File.Exists(prj) && !slnText.Contains(prj))
				addProjects.Add(prj, System.Guid.NewGuid().ToString().ToUpper());
		}

		if (!addProjects.Any())
			return;

		var sr = new StringReader(slnText);
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
				foreach(var kvp in addProjects)
				{
					var proj = kvp.Key;
					var name = Path.GetFileNameWithoutExtension(proj);
					var guid = kvp.Value;

					sb.AppendLine(string.Format("Project(\"{{{0}}}\") = \"{1}\", \"{2}\", \"{{{3}}}\"", slnGuid, name, proj, guid));
					sb.AppendLine("EndProject");
				}
			}

			sb.AppendLine(line);

			if (line.Contains("GlobalSection(ProjectConfigurationPlatforms)"))
			{
				foreach(var kvp in addProjects)
				{
					var guid = kvp.Value;

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

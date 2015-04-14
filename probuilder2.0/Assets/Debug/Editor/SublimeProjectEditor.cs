using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;
using System.Diagnostics;

public class SublimeProjectEditor : EditorWindow
{
	string prj_name = "";
	string prj_path = "";
	bool prj_exists = false;

	[MenuItem("Tools/Sublime Text 3/Open Sublime Text %&s")]
	public static void OpenSublime()
	{
		System.Diagnostics.Process.Start( @"C:\Program Files\Sublime Text 3\sublime_text.exe" );
	}

	[MenuItem("Tools/Sublime Text 3/Create Sublime-Project")]
	public static void CreateSublimePrj()
	{
		EditorWindow.GetWindow<SublimeProjectEditor>(true, "Sublime-Project Editor", true);
	}

	[MenuItem("Tools/Sublime Text 3/Open Sublime-Project")]
	public static void OpenSublimePrj()
	{
		string subprj = SublimeProjectExists();
		if(subprj != null)
		{
			#if !UNITY_STANDALONE_OSX
			System.Diagnostics.Process.Start( @"C:\Program Files\Sublime Text 3\sublime_text.exe", "--project " + subprj);
			#else
			System.Diagnostics.Process.Start( @"subl ", "--project " + subprj);
			#endif
		}
	}

	public void OnEnable()
	{
		string[] split = Application.dataPath.Split("/"[0]);
		prj_name = split[split.Length - 2];		
		prj_exists = SublimeProjectExists() != null; 
	}

	public void OnGUI()
	{
		GUILayout.Label("Unity Sublime-Project Editor", EditorStyles.boldLabel);

		GUILayout.Space(4);

		if(prj_exists)
		{
			EditorGUILayout.HelpBox("Sublime-Project Found!  Creating a new project will overwrite the old if the names clash.", MessageType.Warning);
		}
		
		prj_name = EditorGUILayout.TextField("Project Name", prj_name);

		GUILayout.Label("Project Path");
		prj_path = EditorGUILayout.TextField("Assets/", prj_path);

		if(GUILayout.Button("Create Project"))
		{
			if(CreateSublimeProject(prj_name, "Assets/" + prj_path))
			{
				if(EditorUtility.DisplayDialog("Sublime-Project Created!", "Sublime-Project successfully created.  Open now?", "Yes", "No"))
					OpenSublimePrj();
				else
					this.Close();
			}
		}
	}

	public static bool CreateSublimeProject(string prj_name, string code_path)
	{
		// if( !code_path.EndsWith("\\") && !code_path.EndsWith("/") )
		// 	code_path += "/";
		// string[] split = Application.dataPath.Split("/"[0]);
		// string open_project = split[split.Length - 2];

		/*begin prj text*/
		string project_contents = "{\n    \"folders\":\n    [\n        {\n            \"path\": \"" + code_path + "\",\n            \"file_exclude_patterns\":\n            [\n                \"*.unity\",\"*.guiskin\",\"*.dll\",\"*.meta\" ,\"*.prefab\" ,\"*.anim\", \"*.physicMaterial\", \"*.flare\" , \"*.cubemap\" , \"*.font\" , \"*.fontsettings\" , \"*.renderTexture\"\n            ],\n\n            \"folder_exclude_patterns\":\n            [\n                \"Resources\"\n            ]\n        }\n    ],\n    \"build_systems\":\n    [\n        {\n            \"name\": \"Unity Mono\",\n            \"cmd\": [\"C:\\\\Program Files (x86)\\\\Unity\\\\Editor\\\\Data\\\\Mono\\\\bin\\\\xbuild.bat\", \"${project_path}/${project_base_name}.sln\",\"/noconsolelogger\", \"/logger:${project_path}\\\\SublimeUnity\\\\SublimeLogger.dll\"],\n            \"file_regex\": \"([^(]*)\\\\((\\\\d*),(\\\\d*)\\\\)\\\\s*(.*)\"\n        }\n    ]\n}";

		// place in main project folder
		string path = Directory.GetParent(Application.dataPath).ToString();

		path += "/" + prj_name + ".sublime-project";

		if(File.Exists(path))
			File.Delete(path);

		TextWriter tw = new StreamWriter(path);
		tw.WriteLine(project_contents);
		tw.Close();

		return true;
	}

	public static string SublimeProjectExists()
	{
		string prj = Directory.GetParent(Application.dataPath).ToString();
		string[] allFiles = System.IO.Directory.GetFiles(prj, "*.*", System.IO.SearchOption.AllDirectories);
		string[] allSublimeProjects = System.Array.FindAll(allFiles, name => name.EndsWith(".sublime-project"));
		if(allSublimeProjects.Length < 1)
			return null;
		else
			return allSublimeProjects[0];
	}
}

/**
 *	Editor Wizard for easily managing global defines in Unity
 *	@khenkel
 */

using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

public class DefineManager : EditorWindow
{
	enum Compiler
	{
		CSharp,
		Editor,
		UnityScript,
		Boo
	}
	Compiler compiler = Compiler.Editor;

	// http://forum.unity3d.com/threads/93901-global-define/page2
	// Do not modify these paths
	const int COMPILER_COUNT = 4;
	const string CSHARP_PATH 		= "Assets/smcs.rsp";
	const string EDITOR_PATH 		= "Assets/gmcs.rsp";
	const string UNITYSCRIPT_PATH 	= "Assets/us.rsp";
	const string BOO_PATH 			= "Assets/boo.rsp";

	List<string> csDefines = new List<string>(); 
	List<string> booDefines = new List<string>(); 
	List<string> usDefines = new List<string>(); 
	List<string> editorDefines = new List<string>(); 

	[MenuItem("Window/Define Manager")]
	public static void OpenDefManager()
	{
		EditorWindow.GetWindow<DefineManager>(true, "Global Define Manager", true);
	}

	void OnEnable()
	{
		csDefines = ParseRspFile(CSHARP_PATH);
		usDefines = ParseRspFile(UNITYSCRIPT_PATH);
		booDefines = ParseRspFile(BOO_PATH);
		editorDefines = ParseRspFile(EDITOR_PATH);
	}

	List<string> defs;
	Vector2 scroll = Vector2.zero;
	void OnGUI()
	{
		Color oldColor = GUI.backgroundColor;

		GUILayout.BeginHorizontal();
		for(int i = 0; i < COMPILER_COUNT; i++)	
		{
			if(i == (int)compiler)
				GUI.backgroundColor = Color.gray;

			GUIStyle st;
			switch(i)
			{
				case 0:
					st = EditorStyles.miniButtonLeft;
					break;
				case COMPILER_COUNT-1:
					st = EditorStyles.miniButtonRight;
					break;
				default:
					st = EditorStyles.miniButtonMid;
					break;
			}

			if(GUILayout.Button( ((Compiler)i).ToString(), st))
				compiler = (Compiler)i;

			GUI.backgroundColor = oldColor;
		}
		GUILayout.EndHorizontal();
		
		switch(compiler)
		{
			case Compiler.CSharp:
				defs = csDefines;
				break;

			case Compiler.Editor:
				defs = editorDefines;
				break;

			case Compiler.UnityScript:
				defs = usDefines;
				break;

			case Compiler.Boo:
				defs = booDefines;
				break;
		}

		GUILayout.Label(compiler.ToString() + " User Defines");

		scroll = GUILayout.BeginScrollView(scroll);
		for(int i = 0; i < defs.Count; i++)
		{
			GUILayout.BeginHorizontal();
				
				defs[i] = EditorGUILayout.TextField(defs[i]);
				
				GUI.backgroundColor = Color.red;
				if(GUILayout.Button("x", GUIStyle.none, GUILayout.MaxWidth(18)))
					defs.RemoveAt(i);
				GUI.backgroundColor = oldColor;

			GUILayout.EndHorizontal();

		}
		
		GUILayout.Space(4);

		GUI.backgroundColor = Color.cyan;
		if(GUILayout.Button("Add"))	
			defs.Add("NEW_DEFINE");

		GUILayout.EndScrollView();


		GUILayout.BeginHorizontal();
			GUI.backgroundColor = Color.green;
			if( GUILayout.Button("Apply") )
			{
				SetDefines(compiler, defs);
				AssetDatabase.ImportAsset("Assets/Bugger/Editor/DefineManager.cs", ImportAssetOptions.ForceUpdate);
				OnEnable();
			}
		
			GUI.backgroundColor = Color.red;
			if(GUILayout.Button("Apply All", GUILayout.MaxWidth(64)))
				for(int i = 0; i < COMPILER_COUNT; i++)
				{
					SetDefines((Compiler)i, defs);
					AssetDatabase.ImportAsset("Assets/Bugger/Editor/DefineManager.cs", ImportAssetOptions.ForceUpdate);
					OnEnable();		
				}
		
		GUILayout.EndHorizontal();
		GUI.backgroundColor = oldColor;
	}

	void SetDefines(Compiler compiler, List<string> defs)
	{
		switch(compiler)
		{
			case Compiler.CSharp:
				WriteDefines(CSHARP_PATH, defs);
				break;

			case Compiler.UnityScript:
				WriteDefines(UNITYSCRIPT_PATH, defs);
				break;
			
			case Compiler.Boo:
				WriteDefines(BOO_PATH, defs);
				break;

			case Compiler.Editor:
				WriteDefines(EDITOR_PATH, defs);
				break;
		}
	}

	List<string> ParseRspFile(string path)
	{
		if(!File.Exists(path))
			return new List<string>();

		string[] lines = File.ReadAllLines(path);
		List<string> defs = new List<string>();

		foreach(string cheese in lines)
		{
			if(cheese.StartsWith("-define:"))
			{
				defs.AddRange( cheese.Replace("-define:", "").Split(';') );
			}
		}

		return defs;
	}

	void WriteDefines(string path, List<string> defs)
	{
		if(defs.Count < 1 && File.Exists(path))
		{
			File.Delete(path);
			File.Delete(path + ".meta");
			AssetDatabase.Refresh();
			return;
		}

		StringBuilder sb = new StringBuilder();
		sb.Append("-define:");
		
		for(int i = 0; i < defs.Count; i++)
		{
			sb.Append(defs[i]);
			if(i < defs.Count-1) sb.Append(";");
		}

		using (StreamWriter writer = new StreamWriter(path, false))
		{
			writer.Write(sb.ToString());
		}
	}
}

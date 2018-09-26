//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEditor;
//using UnityEngine.ProBuilder;
//using UnityEditor.ProBuilder;
//using UObject = UnityEngine.Object;
//using UnityEditor.Settings;
//
//class TempMenuItems : EditorWindow
//{
//	[SettingsKey("TempMenuItems.m_Scroll", SettingScope.Project)]
//	static Vector2 m_Scroll;
//
//	string m_Settings;
//
////	[MenuItem("Tools/Temp Menu Item &d", false, 1000)]
//	static void MenuInit()
//	{
//		GetWindow<TempMenuItems>();
//	}
//
//	[MenuItem("Tools/Recompile")]
//	static void Recompile()
//	{
//		if(ScriptingSymbolManager.ContainsDefine("PROBUILDER_RECOMPILE_FLAG"))
//			ScriptingSymbolManager.RemoveScriptingDefine("PROBUILDER_RECOMPILE_FLAG");
//		else
//			ScriptingSymbolManager.AddScriptingDefine("PROBUILDER_RECOMPILE_FLAG");
//	}
//
//	[MenuItem("Tools/Dump Settings &d")]
//	static void PrintAll()
//	{
//		Debug.Log(UserSettings.GetSettingsString(SettingScope.User));
//	}
//
//	void OnEnable()
//	{
//		var settings = UserSettings.FindUserSettings(SettingVisibility.All);
//		var sb = new System.Text.StringBuilder();
//
//		foreach (var pref in settings.OrderBy(x => x.type))
//		{
//			sb.AppendLine(string.Format("{0,-24}{1,-24}{2,-64}{3}", pref.type, pref.scope, pref.key, pref.GetValue().ToString()));
//		}
//
//		m_Settings = sb.ToString();
//	}
//
//
//	void OnGUI()
//	{
//		m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);
//
//		GUILayout.Label(m_Settings);
//
//		EditorGUILayout.EndScrollView();
//	}
//
//	public static void SaveMeshTemplate(Mesh mesh)
//	{
////		StackTrace trace = new StackTrace(1, true);
////		for (int i = 0; i < trace.FrameCount; i++)
////		{
////			StackFrame first = trace.GetFrame(i);
////			UnityEngine.Debug.Log(first.GetFileName() + ": " + first.GetMethod());
////		}
//	}
//}

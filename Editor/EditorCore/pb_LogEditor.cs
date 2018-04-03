using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ProBuilder.Core;
using ProBuilder.Interface;
using UnityEngine;
using UnityEditor;

namespace ProBuilder.EditorCore
{
	/// <summary>
	/// Initialize pb_Log preferences here because pb_Log does not have access to pb_PreferencesInternal.
	/// </summary>
	[InitializeOnLoad]
	static class pb_LogPreferences
	{
		static pb_LogPreferences()
		{
			SetLogPreferences();
		}

		internal static void SetLogPreferences()
		{
			EditorApplication.delayCall += () =>
			{
				pb_Log.SetLogLevel( (pb_LogLevel) pb_PreferencesInternal.GetInt("pb_Log::m_LogLevel", (int) pb_LogLevel.Default) );
				pb_Log.SetOutput( (pb_LogOutput) pb_PreferencesInternal.GetInt("pb_Log::m_Output", (int) pb_LogOutput.Console) );
				pb_Log.SetLogFile( pb_PreferencesInternal.GetString("pb_Log::m_LogFilePath", pb_Log.DEFAULT_LOG_PATH) );
			};
		}
	}

	/// <summary>
	/// Manage settings for pb_Log.
	/// </summary>
	class pb_LogEditor : EditorWindow
	{
		[MenuItem("Tools/" + pb_Constant.PRODUCT_NAME + "/Debug/Log Preferences")]
		private static void MenuInit()
		{
			EditorWindow.GetWindow<pb_LogEditor>(true, "Log Preferences", true);
		}

		private static GUIContent[] gc_output = new GUIContent[]
		{
			new GUIContent("Console"),
			new GUIContent("File")
		};

		private static GUIContent[] gc_level = new GUIContent[]
		{
			new GUIContent("Error"),
			new GUIContent("Warning"),
			new GUIContent("Info"),
			new GUIContent("Debug"),
			new GUIContent("All")
		};

		private pb_LogOutput m_Buffer;
		private pb_LogLevel m_LogLevel;
		private string m_LogFilePath;

		private void OnEnable()
		{
			m_Buffer = (pb_LogOutput) pb_PreferencesInternal.GetInt("pb_Log::m_Output", (int) pb_LogOutput.Console);
			m_LogLevel = (pb_LogLevel) pb_PreferencesInternal.GetInt("pb_Log:::m_LogLevel", (int) pb_LogLevel.All);
			m_LogFilePath = pb_PreferencesInternal.GetString("pb_Log::m_LogFilePath", pb_Log.DEFAULT_LOG_PATH);
		}

		private void OnGUI()
		{
			GUILayout.Label("Log Output", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();
			m_Buffer = (pb_LogOutput) pb_EditorGUILayout.FlagToolbar((int)m_Buffer, gc_output);
			if(EditorGUI.EndChangeCheck())
			{
				pb_PreferencesInternal.SetInt("pb_Log::m_Output", (int) m_Buffer);
				pb_LogPreferences.SetLogPreferences();
			}

			GUI.enabled = (m_Buffer & pb_LogOutput.File) > 0;

			GUILayout.BeginHorizontal();
			GUILayout.Label("Log Path");
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("...", EditorStyles.miniButton))
			{
				string dir = EditorUtility.OpenFolderPanel("ProBuilder Log Directory", "", "");

				if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
				{
					m_LogFilePath = string.Format("{0}/{1}", dir, pb_Log.DEFAULT_LOG_PATH);

					try
					{
						Uri directoryUri = new Uri(dir);
						Uri fileUri = new Uri(m_LogFilePath);
						string relativePath = directoryUri.MakeRelativeUri(fileUri).ToString();
						if (!string.IsNullOrEmpty(relativePath))
							m_LogFilePath = relativePath;
					}
					catch {}

					pb_PreferencesInternal.SetString("pb_Log::m_LogFilePath", m_LogFilePath);
					pb_Log.SetLogFile(m_LogFilePath);
				}
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal();
			GUI.enabled = false;
			EditorGUILayout.LabelField(m_LogFilePath);
			GUI.enabled = true;
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("open", EditorStyles.miniButton))
				EditorUtility.OpenWithDefaultApp(m_LogFilePath);
			GUILayout.EndHorizontal();

			GUILayout.Label("Chatty-ness", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();
			m_LogLevel = (pb_LogLevel) pb_EditorGUILayout.FlagToolbar((int)m_LogLevel, gc_level, false, true);
			if(EditorGUI.EndChangeCheck())
			{
				pb_PreferencesInternal.SetInt("pb_Log::m_LogLevel", (int) m_LogLevel);
				pb_Log.SetLogLevel(m_LogLevel);
			}

			GUILayout.FlexibleSpace();
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Clear Log File", EditorStyles.miniButton))
				pb_Log.ClearLogFile();
			GUILayout.EndHorizontal();
		}
	}
}
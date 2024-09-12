using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder.UI;
using UnityEngine;
using UnityEditor;

namespace UnityEditor.ProBuilder
{
    /// <summary>
    /// Initialize Log preferences here because Log does not have access to preference values.
    /// </summary>
    [InitializeOnLoad]
    static class LogPreferences
    {
        static LogPreferences()
        {
            SetLogPreferences();
        }

        internal static void SetLogPreferences()
        {
            EditorApplication.delayCall += () =>
                {
                    Log.SetLogLevel(LogEditor.s_LogLevel);
                    Log.SetOutput(LogEditor.s_LogOutput);
                    Log.SetLogFile(LogEditor.s_LogPath);
                };
        }
    }

    /// <summary>
    /// Manage settings for pb_Log.
    /// </summary>
    sealed class LogEditor : EditorWindow
    {
        internal static Pref<LogLevel> s_LogLevel = new Pref<LogLevel>("log.level", LogLevel.Default);
        internal static Pref<LogOutput> s_LogOutput = new Pref<LogOutput>("log.output", LogOutput.Console);
        internal static Pref<string> s_LogPath = new Pref<string>("log.path", Log.k_ProBuilderLogFileName);

        [MenuItem("Tools/" + PreferenceKeys.pluginTitle + "/Debug/Log Preferences")]
        static void MenuInit()
        {
            GetWindow<LogEditor>(true, "Log Preferences", true);
        }

        static GUIContent[] gc_output = new GUIContent[]
        {
            new GUIContent("Console"),
            new GUIContent("File")
        };

        static GUIContent[] gc_level = new GUIContent[]
        {
            new GUIContent("Error"),
            new GUIContent("Warning"),
            new GUIContent("Info"),
            new GUIContent("Debug"),
            new GUIContent("All")
        };

        void OnGUI()
        {
            GUILayout.Label("Log Output", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            s_LogOutput.value = (LogOutput)global::UnityEditor.ProBuilder.UI.EditorGUILayout.FlagToolbar((int)s_LogOutput.value, gc_output);
            if (EditorGUI.EndChangeCheck())
            {
                ProBuilderSettings.Save();
                LogPreferences.SetLogPreferences();
            }

            GUI.enabled = (s_LogOutput & LogOutput.File) > 0;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Log Path");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("...", EditorStyles.miniButton))
            {
                string dir = UnityEditor.EditorUtility.OpenFolderPanel("ProBuilder Log Directory", "", "");

                if (!string.IsNullOrEmpty(dir) && Directory.Exists(dir))
                {
                    s_LogPath.value = string.Format("{0}/{1}", dir, Log.k_ProBuilderLogFileName);

                    try
                    {
                        Uri directoryUri = new Uri(dir);
                        Uri fileUri = new Uri(s_LogPath);
                        string relativePath = directoryUri.MakeRelativeUri(fileUri).ToString();
                        if (!string.IsNullOrEmpty(relativePath))
                            s_LogPath.value = relativePath;
                    }
                    catch {}

                    ProBuilderSettings.Save();
                    Log.SetLogFile(s_LogPath);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            GUI.enabled = false;
            EditorGUILayout.LabelField(s_LogPath);
            GUI.enabled = true;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Open", EditorStyles.miniButton))
                UnityEditor.EditorUtility.OpenWithDefaultApp(s_LogPath);
            GUILayout.EndHorizontal();

            GUILayout.Label("Chatty-ness", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            s_LogLevel.value = (LogLevel)global::UnityEditor.ProBuilder.UI.EditorGUILayout.FlagToolbar((int)s_LogLevel.value, gc_level, false, true);
            if (EditorGUI.EndChangeCheck())
            {
                ProBuilderSettings.Save();
                Log.SetLogLevel(s_LogLevel);
            }

            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Clear Log File", EditorStyles.miniButton))
                Log.ClearLogFile();
            GUILayout.EndHorizontal();
        }
    }
}

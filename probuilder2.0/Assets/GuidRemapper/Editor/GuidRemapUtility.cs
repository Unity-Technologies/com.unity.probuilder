using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.TreeViewExamples;

namespace UnityEditor.GuidRemap
{
    public class GuidRemapUtility : EditorWindow
    {
        [MenuItem("Window/GUID Remap Utility")]
        static void Init()
        {
            GetWindow<GuidRemapUtility>(true, "GUID Remap Utility", true).Show();
        }

        TextAsset m_RemapJson;
        GuidRemapObject m_GuidMap;
        Vector2 m_Scroll;
        TreeView m_TreeView;

        List<string> m_Files = new List<string>();
        List<List<string>> m_From = new List<List<string>>();
        List<string> m_To = new List<string>();

        void OnEnable()
        {
            if (m_RemapJson == null)
                m_RemapJson = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/remap.json");

            ReloadJson();
        }

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            m_RemapJson = (TextAsset) EditorGUILayout.ObjectField("Remap File", m_RemapJson, typeof(TextAsset), false);
            if (EditorGUI.EndChangeCheck())
                ReloadJson();

            m_Scroll = GUILayout.BeginScrollView(m_Scroll);
            for (int i = 0, c = m_Files.Count; i < c; i++)
            {
                GUILayout.BeginHorizontal();

                GUILayout.Label(m_Files[i]);

                GUILayout.BeginVertical();
                foreach(var s in m_From[i])
                    GUILayout.Label(s);
                GUILayout.EndVertical();

                GUILayout.Label(m_To[i]);

                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
        }

        void ReloadJson()
        {
            m_GuidMap = new GuidRemapObject();
            JsonUtility.FromJsonOverwrite(m_RemapJson.text, m_GuidMap);

            foreach (var remap in m_GuidMap.map)
            {
                m_Files.Add(remap.file);
                m_From.Add(remap.from.ToList());
                m_To.Add(remap.to);
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine.ProBuilder;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace UnityEditor.ProBuilder
{
    [CustomEditor(typeof(PreferenceDictionary))]
    sealed class PreferenceDictionaryEditor : UnityEditor.Editor
    {
        PreferenceDictionary m_Preferences = null;
        Vector2 m_Scroll = Vector2.zero;
        SearchField m_Search;
        string m_SearchText;

        void OnEnable()
        {
            m_Preferences = target as PreferenceDictionary;
            m_Search = new SearchField();
            m_SearchText = "";
        }

        public override void OnInspectorGUI()
        {
            m_SearchText = m_Search.OnGUI(m_SearchText);

            GUILayout.Label("Key Value Pairs", EditorStyles.boldLabel);

            m_Scroll = EditorGUILayout.BeginScrollView(m_Scroll);

            DoPreferenceList<string, bool>(m_Preferences.GetBoolDictionary());
            DoPreferenceList<string, int>(m_Preferences.GetIntDictionary());
            DoPreferenceList<string, float>(m_Preferences.GetFloatDictionary());
            DoPreferenceList<string, string>(m_Preferences.GetStringDictionary());
            DoPreferenceList<string, Color>(m_Preferences.GetColorDictionary());
            DoPreferenceList<string, Material>(m_Preferences.GetMaterialDictionary());


            EditorGUILayout.EndScrollView();
        }

        void DoPreferenceList<T, K>(Dictionary<T, K> dictionary)
        {
            foreach (var kvp in dictionary)
            {
                var label = kvp.Key.ToString();
                var value = kvp.Value.ToString();

                if (string.IsNullOrEmpty(m_SearchText) || label.Contains(m_SearchText))
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(label, GUILayout.Width(180), GUILayout.ExpandWidth(false));
                    GUILayout.Label(value);
                    GUILayout.EndHorizontal();
                }
            }
        }
    }
}

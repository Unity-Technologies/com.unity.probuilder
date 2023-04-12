using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEditor.ProBuilder.Debug
{
    class ResourceExplorer : ConfigurableWindow
    {
        [MenuItem("Tools/Debug/Resource Explorer")]
        static void init() => GetWindow<ResourceExplorer>();

        [SerializeField]
        Mesh[] m_Loaded;

        [SerializeField]
        HashSet<Mesh> m_Ignore = new HashSet<Mesh>();

        HideFlags m_Flags;

        [SerializeField]
        bool m_IgnoreEmptyName = true;

        void OnEnable()
        {
            ScanResources();
        }

        void ScanResources()
        {
            m_Loaded = Resources.FindObjectsOfTypeAll<Mesh>();
        }

        void OnFocus()
        {
            ScanResources();
        }

        Vector2 m_Scroll;

        bool PassesFilter(Mesh mesh)
        {
            if (mesh == null || m_IgnoreEmptyName && string.IsNullOrEmpty(mesh.name))
                return false;

            if (m_Flags == HideFlags.None)
            {
                if (mesh.hideFlags != HideFlags.None)
                    return false;
            }
            else if ((int)(mesh.hideFlags & m_Flags) < 1)
                return false;

            return true;
        }

        void OnGUI()
        {
            GUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Button("refresh", EditorStyles.toolbarButton))
                ScanResources();
            if (GUILayout.Button("show hidden", EditorStyles.toolbarButton))
                m_Ignore.Clear();
            m_Flags = (HideFlags) EditorGUILayout.EnumFlagsField(m_Flags);
            GUILayout.Space(16);
            if (GUILayout.Button("destroy filtered", EditorStyles.toolbarButton))
            {
                for(int i = m_Loaded.Length-1; i > -1; --i)
                    if(PassesFilter(m_Loaded[i]))
                        DestroyImmediate(m_Loaded[i]);
                ScanResources();
            }
            GUILayout.FlexibleSpace();
            m_IgnoreEmptyName = EditorGUILayout.Toggle("ignore empty name", m_IgnoreEmptyName);

            GUILayout.EndHorizontal();
            m_Scroll = GUILayout.BeginScrollView(m_Scroll);

            for (int i = 0, c = m_Loaded.Length; i < c; ++i)
            {
                if (!PassesFilter(m_Loaded[i]))
                    continue;

                GUILayout.BeginHorizontal();
                if (!m_Ignore.Contains(m_Loaded[i]))
                {
                    if (GUILayout.Button("", EditorStyles.toggle, GUILayout.Width(64)))
                        m_Ignore.Add(m_Loaded[i]);
                    GUILayout.Label(m_Loaded[i].name);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }
    }
}

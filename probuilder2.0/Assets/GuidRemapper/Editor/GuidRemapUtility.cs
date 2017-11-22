using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace UnityEditor.GuidRemap
{
    public class SimpleTreeView : TreeView
    {
        public SimpleTreeView(TreeViewState state, MultiColumnHeader header) : base(state, header)
        {

        }

        protected override TreeViewItem BuildRoot()
        {
            SimpleTreeElement root = new SimpleTreeElement(0, -1, "Root", "");

            var all = new List<TreeViewItem>();
            all.Add(new SimpleTreeElement(1, 0, "First", "I'm first"));
            all.Add(new SimpleTreeElement(2, 0, "Second", "I'm not first"));
            SetupParentsAndChildrenFromDepths(root, all);
            return root;
        }

        protected override void RowGUI (RowGUIArgs args)
        {
            SimpleTreeElement item = args.item as SimpleTreeElement;

            for (int i = 0; i < args.GetNumVisibleColumns (); ++i)
            {
                CellGUI(args.GetCellRect(i), item, i, ref args);
            }
        }

        void CellGUI(Rect rect, SimpleTreeElement item, int visibileColum, ref RowGUIArgs args)
        {
            GUI.Label(rect, item.label);
        }
    }

    public class SimpleTreeElement : TreeViewItem
    {
        public string label;

        public SimpleTreeElement(int id, int depth, string displayName, string label) : base(id, depth, displayName)
        {
            this.label = label;
        }
    }

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

        [SerializeField] TreeViewState m_TreeViewState;
        [SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
        MultiColumnHeader m_MultiColumnHeader;
        TreeView m_TreeView;

        List<string> m_Files = new List<string>();
        List<List<string>> m_From = new List<List<string>>();
        List<string> m_To = new List<string>();

        void OnEnable()
        {
            if (m_RemapJson == null)
                m_RemapJson = AssetDatabase.LoadAssetAtPath<TextAsset>("Assets/remap.json");

            ReloadJson();

            // Check whether there is already a serialized view state (state
            // that survived assembly reloading)
            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState ();

            if(m_MultiColumnHeaderState == null)
                m_MultiColumnHeaderState = new MultiColumnHeaderState(new MultiColumnHeaderState.Column[]
                {
                    new MultiColumnHeaderState.Column() { headerContent = new GUIContent("farts", "smelly") },
                    new MultiColumnHeaderState.Column() { headerContent = new GUIContent("second", "smelly") },
                });

            m_MultiColumnHeader = new MultiColumnHeader(m_MultiColumnHeaderState);
            m_TreeView = new SimpleTreeView(m_TreeViewState, m_MultiColumnHeader);
            m_TreeView.Reload();
        }

        void OnGUI()
        {
            EditorGUI.BeginChangeCheck();
            m_RemapJson = (TextAsset) EditorGUILayout.ObjectField("Remap File", m_RemapJson, typeof(TextAsset), false);
            if (EditorGUI.EndChangeCheck())
                ReloadJson();

            m_TreeView.OnGUI(new Rect(0, 32, position.width, position.height - 32));

//            m_Scroll = GUILayout.BeginScrollView(m_Scroll);
//            for (int i = 0, c = m_Files.Count; i < c; i++)
//            {
//                GUILayout.BeginHorizontal();
//
//                GUILayout.Label(m_Files[i]);
//
//                GUILayout.BeginVertical();
//                foreach(var s in m_From[i])
//                    GUILayout.Label(s);
//                GUILayout.EndVertical();
//
//                GUILayout.Label(m_To[i]);
//
//                GUILayout.EndHorizontal();
//            }
//            GUILayout.EndScrollView();
        }

        void ReloadJson()
        {
            m_GuidMap = new GuidRemapObject();
            JsonUtility.FromJsonOverwrite(m_RemapJson.text, m_GuidMap);

            foreach (var remap in m_GuidMap.map)
            {

            }
        }
    }
}

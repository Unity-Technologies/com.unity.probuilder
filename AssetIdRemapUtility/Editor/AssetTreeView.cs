using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.VersionControl;
using UnityEngine;
using UObject = UnityEngine.Object;

namespace UnityEngine.ProBuilder.AssetIdRemapUtility
{
#pragma warning disable CS0618 // Type or member is obsolete
    sealed class AssetTreeItem : TreeViewItem
#pragma warning restore CS0618
    {
        string m_RelativePath;
        string m_FullPath;
        bool m_IsEnabled;
        bool m_IsDirectory;
        bool m_IsMixedState;

        public AssetTreeItem(int id, string fullPath, string relativePath) : base(id, 0)
        {
            m_IsDirectory = Directory.Exists(fullPath);
            m_FullPath = fullPath;
            m_RelativePath = relativePath;
            m_IsEnabled = true;
            displayName = m_FullPath.Replace("\\", "/").Replace(Application.dataPath, "Assets/");
        }

        public bool enabled
        {
            get { return m_IsEnabled; }
            set { m_IsEnabled = value; }
        }

        public bool isDirectory
        {
            get { return m_IsDirectory; }
            set { m_IsDirectory = value; }
        }

        public string fullPath
        {
            get { return m_FullPath; }
        }

        public string relativePath
        {
            get { return m_RelativePath; }
        }

        public bool isMixedState { get { return m_IsMixedState; } }

        public void SetEnabled(bool isEnabled)
        {
            enabled = isEnabled;

            if (children != null)
            {
                foreach (var child in children)
                {
                    AssetTreeItem asset = child as AssetTreeItem;

                    if (asset != null)
                        asset.SetEnabled(isEnabled);
                }
            }

            var upstream = parent;

            while (upstream != null)
            {
                var up = upstream as AssetTreeItem;

                if (up != null && up.children != null)
                {
                    AssetTreeItem firstChild = up.children.FirstOrDefault() as AssetTreeItem;

                    if (firstChild != null)
                    {
                        up.m_IsMixedState = up.children.Any(x =>
                            {
                                var y = x as AssetTreeItem;
                                return y.enabled != firstChild.enabled;
                            });

                        if (!up.m_IsMixedState)
                            up.enabled = firstChild.enabled;
                    }
                    else
                    {
                        up.m_IsMixedState = false;
                    }
                }

                upstream = upstream.parent;
            }
        }
    }

#pragma warning disable CS0618 // Type or member is obsolete
    class AssetTreeView : TreeView
    {
        string m_RootDirectory = null;
        GUIContent m_TempContent = new GUIContent();
        Rect m_ToggleRect = new Rect(0, 0, 0, 0);

        public string directoryRoot
        {
            get { return m_RootDirectory; }
            set { m_RootDirectory = value; }
        }

        public AssetTreeItem GetRoot()
        {
            return rootItem.hasChildren
                ? rootItem.children.First() as AssetTreeItem
                : null;
        }

        IEnumerable<Regex> m_DirectoryIgnorePatterns;
        IEnumerable<Regex> m_FileIgnorePatterns;

        public void SetDirectoryIgnorePatterns(string[] regexStrings)
        {
            m_DirectoryIgnorePatterns = regexStrings.Select(x => new Regex(x));
        }

        public void SetFileIgnorePatterns(string[] regexStrings)
        {
            m_FileIgnorePatterns = regexStrings.Select(x => new Regex(x));
        }

        public AssetTreeView(TreeViewState state, MultiColumnHeader header) : base(state, header)
        {
            showBorder = true;
            columnIndexForTreeFoldouts = 0;
            rowHeight = 18f;
        }
        protected override TreeViewItem BuildRoot()
        {
            AssetTreeItem root = new AssetTreeItem(0, Application.dataPath, "")
            {
                depth = -1,
                enabled = false
            };

            if (string.IsNullOrEmpty(m_RootDirectory) || !Directory.Exists(m_RootDirectory))
            {
                // if root has no children and you SetupDepthsFromParentsAndChildren nullrefs are thrown
                var list = new List<TreeViewItem>() {};
                SetupParentsAndChildrenFromDepths(root, list);
            }
            else
            {
                int nodeIndex = 0;
                PopulateAssetTree(m_RootDirectory, root, ref nodeIndex);
                SetupDepthsFromParentsAndChildren(root);
                ApplyEnabledFilters(root);
            }

            return root;
        }

        void PopulateAssetTree(string directory, AssetTreeItem parent, ref int nodeIdIndex)
        {
            string unixDirectory = directory.Replace("\\", "/");

            AssetTreeItem leaf = new AssetTreeItem(
                    nodeIdIndex++,
                    unixDirectory,
                    unixDirectory.Replace(parent.fullPath, "").Trim('/'))
            {
                enabled = true
            };

            parent.AddChild(leaf);

            foreach (string path in Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly))
            {
                if (path.StartsWith(".") || path.EndsWith(".meta"))
                    continue;

                string unixPath = path.Replace("\\", "/");

                leaf.AddChild(new AssetTreeItem(
                        nodeIdIndex++,
                        unixPath,
                        unixPath.Replace(unixDirectory, "").Trim('/')) { enabled = true });
            }

            foreach (string dir in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
                PopulateAssetTree(dir, leaf, ref nodeIdIndex);
        }

        public void ApplyEnabledFilters(TreeViewItem root)
        {
            AssetTreeItem node = root as AssetTreeItem;
            AssetTreeItem parent = root.parent as AssetTreeItem;

            if (node != null)
            {
                if (node.isDirectory)
                    node.SetEnabled(
                        (parent == null || parent.enabled) &&
                        (m_DirectoryIgnorePatterns == null || !m_DirectoryIgnorePatterns.Any(x => x.IsMatch(node.fullPath))));
                else
                    node.SetEnabled(
                        (parent == null || parent.enabled) &&
                        (m_FileIgnorePatterns == null || !m_FileIgnorePatterns.Any(x => x.IsMatch(node.fullPath))));
            }

            if (root.children != null)
                foreach (var child in root.children)
                    ApplyEnabledFilters(child);
        }

        protected override void AfterRowsGUI()
        {
            if (rootItem.hasChildren)
                return;

            if (Event.current.type == EventType.Repaint)
            {
                Rect r = new Rect(treeViewRect);
                // not sure why there is a two row pad to the treeViewRect
                r.y -= rowHeight * 2;
                GUI.Label(r, "No Existing ProBuilder Install Found", EditorStyles.centeredGreyMiniLabel);
            }
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            CenterRectUsingSingleLineHeight(ref args.rowRect);
            args.rowRect.xMin += GetContentIndent(args.item);

            AssetTreeItem item = args.item as AssetTreeItem;
            m_TempContent.text = item.relativePath;
            m_TempContent.tooltip = item.fullPath;
            m_ToggleRect.x = args.rowRect.xMin;
            m_ToggleRect.y = args.rowRect.yMin;
            m_ToggleRect.width = 16;
            m_ToggleRect.height = args.rowRect.height;
            args.rowRect.xMin += m_ToggleRect.width;

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = item.isMixedState;
            item.enabled = EditorGUI.Toggle(m_ToggleRect, "", item.enabled);
            EditorGUI.showMixedValue = false;

            if (EditorGUI.EndChangeCheck())
            {
                if (GetSelection().Any(x => FindItem(x, rootItem) == item))
                {
                    foreach (int id in GetSelection())
                    {
                        var sel = FindItem(id, rootItem) as AssetTreeItem;

                        if (sel != null)
                            sel.SetEnabled(item.enabled);
                    }
                }
                else
                {
                    item.SetEnabled(item.enabled);
                }
            }

            bool guiEnabled = GUI.enabled;
            GUI.enabled = item.enabled;
            GUI.Label(args.rowRect, m_TempContent);
            GUI.enabled = guiEnabled;
        }

        protected override void ContextClickedItem(int id)
        {
            var clicked = FindItem(id, rootItem) as AssetTreeItem;
            if (clicked != null)
            {
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("Show Asset"), false, () =>
                    {
                        EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UObject>(clicked.fullPath));
                    });
                menu.ShowAsContext();
            }
        }

        public List<AssetTreeItem> GetAssetList()
        {
            List<AssetTreeItem> assets = new List<AssetTreeItem>();
            GatherTreeItems(rootItem as AssetTreeItem, assets);
            return assets;
        }

        void GatherTreeItems(AssetTreeItem node, List<AssetTreeItem> list)
        {
            if (node == null)
                return;

            list.Add(node);

            if (node.children != null)
                foreach (var child in node.children)
                    if (child is AssetTreeItem)
                        GatherTreeItems(child as AssetTreeItem, list);
        }
    }
#pragma warning restore CS0618 // Type or member is obsolete
}

using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.Timeline;

namespace ProBuilder.AssetUtility
{
	public class AssetTreeItem : TreeViewItem
	{
		string m_RelativePath;
		string m_FullPath;
		string m_Name;
		bool m_IsEnabled;
		bool m_IsDirectory;

		public AssetTreeItem(int id, string fullPath, string relativePath) : base(id, 0)
		{
			m_IsDirectory = Directory.Exists(fullPath);
			m_FullPath = fullPath;
			m_RelativePath = relativePath;
			m_Name = m_IsDirectory ? new DirectoryInfo(fullPath).Name : Path.GetFileNameWithoutExtension(fullPath);
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
		}
	}

	public class AssetTreeView : TreeView
	{
		string m_RootDirectory = null;

		public string directory
		{
			get { return m_RootDirectory; }
			set { m_RootDirectory = value; }
		}

		public AssetTreeView(TreeViewState state, MultiColumnHeader header) : base(state, header)
		{
			showAlternatingRowBackgrounds = true;
			showBorder = true;
			columnIndexForTreeFoldouts = 0;
			rowHeight = 18f;
		}

		protected override TreeViewItem BuildRoot()
		{
			AssetTreeItem root = new AssetTreeItem(0, Application.dataPath, "") { depth = -1 };
			int nodeIndex = 0;
			PopulateAssetTree(m_RootDirectory, root, ref nodeIndex);
			SetupDepthsFromParentsAndChildren(root);
			return root;
		}

		void PopulateAssetTree(string directory, AssetTreeItem parent, ref int nodeIdIndex)
		{
			string unixDirectory = directory.Replace("\\", "/");

			AssetTreeItem leaf = new AssetTreeItem(
				nodeIdIndex++,
				unixDirectory,
				unixDirectory.Replace(parent.fullPath, "").Trim('/'));

			parent.AddChild(leaf);

			foreach (string path in Directory.GetFiles(directory, "*", SearchOption.TopDirectoryOnly))
			{
				if (!path.EndsWith(".meta"))
				{
					leaf.AddChild(new AssetTreeItem(
						nodeIdIndex++,
						path,
						path.Replace("\\", "/").Replace(unixDirectory, "").Trim('/')));
				}
			}

			foreach(string dir in Directory.GetDirectories(directory, "*", SearchOption.TopDirectoryOnly))
				PopulateAssetTree(dir, leaf, ref nodeIdIndex);
		}

		GUIContent m_TempContent = new GUIContent();
		Rect m_ToggleRect = new Rect(0,0,0,0);

		protected override void RowGUI (RowGUIArgs args)
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
			item.enabled = EditorGUI.Toggle(m_ToggleRect, "", item.enabled);
			if (EditorGUI.EndChangeCheck())
			{
				foreach (int id in GetSelection())
				{
					var sel = FindItem(id, rootItem) as AssetTreeItem;

					if(sel != null)
						sel.SetEnabled(item.enabled);
				}
			}

			bool guiEnabled = GUI.enabled;
			GUI.enabled = item.enabled;
			GUI.Label(args.rowRect, m_TempContent);
			GUI.enabled = guiEnabled;
		}
	}
}
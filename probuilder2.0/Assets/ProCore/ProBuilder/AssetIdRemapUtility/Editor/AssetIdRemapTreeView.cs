using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine.WSA;

namespace ProBuilder.AssetUtility
{
	class AssetIdRemapTreeView : TreeView
	{
		AssetIdRemapObject m_RemapObject = null;

		public AssetIdRemapObject remapObject
		{
			get { return m_RemapObject; }
			set { m_RemapObject = value; }
		}

		public AssetIdRemapTreeView(TreeViewState state, MultiColumnHeader header) : base(state, header)
		{
			rowHeight = 20f;
			showAlternatingRowBackgrounds = true;
			showBorder = true;
			extraSpaceBeforeIconAndLabel = 18f;
		}

		protected override TreeViewItem BuildRoot()
		{
			StringTupleTreeElement root = new StringTupleTreeElement(0, -1, "Root", "", "");

			var all = new List<TreeViewItem>();

			int index = 1;

			for (int i = 0, c = remapObject.map.Count; i < c; i++)
			{
				all.Add(new StringTupleTreeElement(index++, 0, "Remap Entry", remapObject[i].source.name, remapObject[i].destination.name));
				all.Add(new StringTupleTreeElement(index++, 1, "Local Path", remapObject[i].source.localPath, remapObject[i].destination.localPath));
				all.Add(new StringTupleTreeElement(index++, 1, "GUID", remapObject[i].source.guid, remapObject[i].destination.guid));
				all.Add(new StringTupleTreeElement(index++, 1, "File ID", remapObject[i].source.fileId, remapObject[i].destination.fileId));
				all.Add(new StringTupleTreeElement(index++, 1, "Type", remapObject[i].source.type, remapObject[i].destination.type));
			}

			SetupParentsAndChildrenFromDepths(root, all);
			return root;
		}

		protected override void RowGUI (RowGUIArgs args)
		{
			StringTupleTreeElement item = args.item as StringTupleTreeElement;

			for (int i = 0; i < args.GetNumVisibleColumns (); ++i)
			{
				CellGUI(args.GetCellRect(i), item, i, ref args);
			}
		}

		void CellGUI(Rect rect, StringTupleTreeElement item, int visibleColumn, ref RowGUIArgs args)
		{
			CenterRectUsingSingleLineHeight(ref rect);

			rect.x += foldoutWidth + 4;

			switch (visibleColumn)
			{
				case 0:
					GUI.Label(rect, item.item1);
					break;

				case 1:
					GUI.Label(rect, item.item2);
					break;
			}
		}
	}

	class StringTupleTreeElement : TreeViewItem
	{
		public string item1;
		public string item2;

		public StringTupleTreeElement(int id, int depth, string displayName, string key, string value) : base(id, depth, displayName)
		{
			this.item1 = key;
			this.item2 = value;
		}
	}
}

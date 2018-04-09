using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;
using System.Linq;
using ProBuilder.Core;
using ProBuilder.EditorCore;

namespace ProBuilder.Actions
{
	class SelectEdgeRing : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Selection; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Selection_Ring", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return m_Tooltip; } }
		public override int toolbarPriority { get { return 2; } }
		public override bool hasFileMenuEntry { get { return false; } }

		private static readonly pb_TooltipContent m_Tooltip = new pb_TooltipContent
		(
			"Select Edge Ring",
			"Selects a ring of edges.  Ringed edges are opposite the selected edge.\n\n<b>Shortcut</b>: Shift + Double-Click on Edge",
			CMD_ALT, 'R'
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					editLevel == EditLevel.Geometry &&
					selectionMode == SelectMode.Edge &&
					selection != null &&
					selection.Length > 0 &&
					selection.Sum(x => x.SelectedEdgeCount) > 0;
		}

		public override bool IsHidden()
		{
			return 	pb_Editor.instance == null ||
					editLevel != EditLevel.Geometry ||
					selectionMode != SelectMode.Edge;
		}

		public override pb_ActionResult DoAction()
		{
			return pb_MenuCommands.MenuRingSelection(selection);
		}
	}
}

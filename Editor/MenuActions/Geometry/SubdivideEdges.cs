using UnityEngine;
using UnityEditor;
using System.Linq;
using ProBuilder.Core;
using ProBuilder.EditorCore;
using ProBuilder.Interface;

namespace ProBuilder.Actions
{
	class SubdivideEdges : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Edge_Subdivide", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return true; } }
		public override bool hasFileMenuEntry { get { return false; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Subdivide Edges",
			"Appends evenly spaced new vertices to the selected edges.",
			CMD_ALT, 'S'
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					pb_Editor.instance.editLevel == EditLevel.Geometry &&
					pb_Editor.instance.selectionMode == SelectMode.Edge &&
					selection != null &&
					selection.Length > 0 &&
					selection.Any(x => x.SelectedEdgeCount > 0);
		}

		public override MenuActionState AltState()
		{
			return MenuActionState.VisibleAndEnabled;
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Subdivide Edge Settings", EditorStyles.boldLabel);

			int subdivisions = pb_PreferencesInternal.GetInt(pb_Constant.pbEdgeSubdivisions, 1);

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.HelpBox("How many vertices to insert on each selected edge.\n\nVertices will be equally spaced between one another and the boundaries of the edge.", MessageType.Info);

			subdivisions = (int) pb_EditorGUIUtility.FreeSlider("Subdivisions", subdivisions, 1, 32);

			if(EditorGUI.EndChangeCheck())
				pb_PreferencesInternal.SetInt(pb_Constant.pbEdgeSubdivisions, subdivisions);

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Subdivide Edges"))
				DoAction();
		}

		public override bool IsHidden()
		{
			return 	pb_Editor.instance == null ||
					pb_Editor.instance.editLevel != EditLevel.Geometry ||
					pb_Editor.instance.selectionMode != SelectMode.Edge;

		}

		public override pb_ActionResult DoAction()
		{
			return pb_MenuCommands.MenuSubdivideEdge(selection);
		}
	}
}

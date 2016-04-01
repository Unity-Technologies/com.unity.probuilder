using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;
using System.Linq;

namespace ProBuilder2.Actions
{
	public class BevelEdges : pb_MenuAction
	{
		public override pb_IconGroup group { get { return pb_IconGroup.Geometry; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Edge_Bevel"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Bevel Edges",
			@"Magic."
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
		
		// public override bool SettingsEnabled()
		// {
		// 	return true;
		// }

		// public override void OnSettingsGUI()
		// {
		// 	GUILayout.Label("Bevel Edge Settings", EditorStyles.boldLabel);
			
		// 	int subdivisions = EditorPrefs.GetInt(pb_Constant.pbEdgeSubdivisions, 1);

		// 	EditorGUI.BeginChangeCheck();

		// 	EditorGUILayout.HelpBox("How many vertices to insert on each selected edge.\n\nVertices will be equally spaced between one another and the boundaries of the edge.", MessageType.Info);

		// 	subdivisions = (int) pb_GUI_Utility.FreeSlider("Subdivisions", subdivisions, 1, 32);

		// 	if(EditorGUI.EndChangeCheck())
		// 		EditorPrefs.SetInt(pb_Constant.pbEdgeSubdivisions, subdivisions);

		// 	GUILayout.FlexibleSpace();

		// 	if(GUILayout.Button("Bevel Edges"))
		// 		DoAction();
		// }

		public override bool IsHidden()
		{
			return 	pb_Editor.instance == null ||
					pb_Editor.instance.editLevel != EditLevel.Geometry ||
					pb_Editor.instance.selectionMode != SelectMode.Edge;
					
		}

		public override pb_ActionResult DoAction()
		{
			return pb_Menu_Commands.MenuBevelEdge(selection);
		}
	}
}

using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;
using System.Linq;
using ProBuilder.Core;
using ProBuilder.EditorCore;

namespace ProBuilder.Actions
{
	class ExtrudeEdges : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Edge_Extrude", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool hasFileMenuEntry { get { return false; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Extrude Edges",
			@"Adds a new face extending from the currently selected edges.  Edges must have an open side to be extruded.",
			CMD_SUPER, 'E'
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

		public override bool IsHidden()
		{
			return 	pb_Editor.instance == null ||
					pb_Editor.instance.editLevel != EditLevel.Geometry ||
					pb_Editor.instance.selectionMode != SelectMode.Edge;

		}

		public override MenuActionState AltState()
		{
			return MenuActionState.VisibleAndEnabled;
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Extrude Settings", EditorStyles.boldLabel);

			EditorGUILayout.HelpBox("Extrude Amount determines how far an edge will be moved along it's normal when extruding.  This value can be negative.\n\nExtrude as Group determines whether or not adjacent faces stay attached to one another when extruding.", MessageType.Info);

			float extrudeAmount = pb_PreferencesInternal.HasKey(pb_Constant.pbExtrudeDistance) ? pb_PreferencesInternal.GetFloat(pb_Constant.pbExtrudeDistance) : .5f;
			bool extrudeAsGroup = pb_PreferencesInternal.GetBool(pb_Constant.pbExtrudeAsGroup);
			bool manifoldEdgeExtrusion = pb_PreferencesInternal.GetBool(pb_Constant.pbManifoldEdgeExtrusion);

			EditorGUI.BeginChangeCheck();

			extrudeAsGroup = EditorGUILayout.Toggle("As Group", extrudeAsGroup);
			manifoldEdgeExtrusion = EditorGUILayout.Toggle(new GUIContent("Manifold Edge Extrusion", "If false, only non-manifold (edges touching two faces) edges may be extruded.  If true, you may extrude any edge you like (for those who like to live dangerously)."), manifoldEdgeExtrusion);

			extrudeAmount = EditorGUILayout.FloatField("Distance", extrudeAmount);

			if(EditorGUI.EndChangeCheck())
			{
				pb_PreferencesInternal.SetFloat(pb_Constant.pbExtrudeDistance, extrudeAmount);
				pb_PreferencesInternal.SetBool(pb_Constant.pbExtrudeAsGroup, extrudeAsGroup);
				pb_PreferencesInternal.SetBool(pb_Constant.pbManifoldEdgeExtrusion, manifoldEdgeExtrusion);
			}

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Extrude Edges"))
				DoAction();
		}

		public override pb_ActionResult DoAction()
		{
			return pb_MenuCommands.MenuExtrude(selection, true);
		}
	}
}


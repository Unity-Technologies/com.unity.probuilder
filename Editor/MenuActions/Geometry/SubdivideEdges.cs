using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEditor.ProBuilder.UI;
using EditorGUILayout = UnityEditor.EditorGUILayout;
using EditorGUIUtility = UnityEditor.ProBuilder.UI.EditorGUIUtility;
using EditorStyles = UnityEditor.EditorStyles;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class SubdivideEdges : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Edge_Subdivide", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }
		protected override bool hasFileMenuEntry { get { return false; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Subdivide Edges",
			"Appends evenly spaced new vertices to the selected edges.",
			keyCommandAlt, 'S'
		);

		public override bool IsEnabled()
		{
			return ProBuilderEditor.instance != null &&
				ProBuilderEditor.instance.editLevel == EditLevel.Geometry &&
				ProBuilderEditor.instance.selectionMode == SelectMode.Edge &&
				MeshSelection.Top().Any(x => x.selectedEdgeCount > 0);
		}

		public override MenuActionState AltState()
		{
			return MenuActionState.VisibleAndEnabled;
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Subdivide Edge Settings", EditorStyles.boldLabel);

			int subdivisions = PreferencesInternal.GetInt(PreferenceKeys.pbEdgeSubdivisions, 1);

			EditorGUI.BeginChangeCheck();

			EditorGUILayout.HelpBox("How many vertices to insert on each selected edge.\n\nVertices will be equally spaced between one another and the boundaries of the edge.", MessageType.Info);

			subdivisions = (int)UI.EditorGUIUtility.FreeSlider("Subdivisions", subdivisions, 1, 32);

			if (EditorGUI.EndChangeCheck())
				PreferencesInternal.SetInt(PreferenceKeys.pbEdgeSubdivisions, subdivisions);

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Subdivide Edges"))
				DoAction();
		}

		public override bool IsHidden()
		{
			return ProBuilderEditor.instance == null ||
				ProBuilderEditor.instance.editLevel != EditLevel.Geometry ||
				ProBuilderEditor.instance.selectionMode != SelectMode.Edge;
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuSubdivideEdge(MeshSelection.Top());
		}
	}
}

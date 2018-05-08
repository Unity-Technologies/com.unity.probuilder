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
	sealed class BevelEdges : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Edge_Bevel", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Bevel",
			@"Smooth the selected edges by adding a slanted face connecting the two adjacent faces."
		);

		public override bool IsEnabled()
		{
			return 	ProBuilderEditor.instance != null &&
					ProBuilderEditor.instance.editLevel == EditLevel.Geometry &&
					MeshSelection.Top().Any(x => x.selectedEdgeCount > 0);
		}

		public override MenuActionState AltState()
		{
			return MenuActionState.VisibleAndEnabled;
		}

		public override bool IsHidden()
		{
			return 	ProBuilderEditor.instance == null ||
					editLevel != EditLevel.Geometry ||
					(selectionMode & (SelectMode.Face | SelectMode.Edge)) == 0;
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Bevel Edge Settings", EditorStyles.boldLabel);

			EditorGUILayout.HelpBox("Amount determines how much space the bevel takes up.  Bigger value means more bevel action.", MessageType.Info);

			float bevelAmount = PreferencesInternal.GetFloat(PreferenceKeys.pbBevelAmount);

			EditorGUI.BeginChangeCheck();

			bevelAmount = UI.EditorGUIUtility.FreeSlider("Distance", bevelAmount, .001f, .99f);
			if(bevelAmount < .001f) bevelAmount = .001f;

			if(EditorGUI.EndChangeCheck())
				PreferencesInternal.SetFloat(PreferenceKeys.pbBevelAmount, bevelAmount);

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Bevel Edges"))
				DoAction();
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuBevelEdges(MeshSelection.Top());
		}
	}
}

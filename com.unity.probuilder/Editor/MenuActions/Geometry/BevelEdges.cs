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

		public override bool enabled
		{
			get
			{
				return ProBuilderEditor.instance != null &&
					ProBuilderEditor.editLevel == EditLevel.Geometry &&
					MeshSelection.TopInternal().Any(x => x.selectedEdgeCount > 0);
			}
		}

		protected override MenuActionState optionsMenuState
		{
			get { return MenuActionState.VisibleAndEnabled; }
		}

		public override bool hidden
		{
			get
			{
				return ProBuilderEditor.instance == null ||
					editLevel != EditLevel.Geometry ||
					(componentMode & (ComponentMode.Face | ComponentMode.Edge)) == 0;
			}
		}

		protected override void OnSettingsGUI()
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
			return MenuCommands.MenuBevelEdges(MeshSelection.TopInternal());
		}
	}
}

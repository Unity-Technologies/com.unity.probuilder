using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using EditorGUILayout = UnityEditor.EditorGUILayout;
using EditorStyles = UnityEditor.EditorStyles;
using EditorUtility = UnityEditor.ProBuilder.EditorUtility;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class FillHole : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Edge_FillHole", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Fill Hole",
			@"Create a new face connecting all selected vertices."
		);

		public override bool IsEnabled()
		{
			return ProBuilderEditor.instance != null &&
				ProBuilderEditor.instance.editLevel == EditLevel.Geometry &&
				ProBuilderEditor.instance.selectionMode != SelectMode.Face &&
				MeshSelection.Top().Length > 0;
		}

		public override bool IsHidden()
		{
			return ProBuilderEditor.instance == null ||
				ProBuilderEditor.instance.editLevel != EditLevel.Geometry ||
				ProBuilderEditor.instance.selectionMode == SelectMode.Face;

		}

		public override MenuActionState AltState()
		{
			return MenuActionState.VisibleAndEnabled;
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Fill Hole Settings", EditorStyles.boldLabel);

			EditorGUILayout.HelpBox("Fill Hole can optionally fill entire holes (default) or just the selected vertices on the hole edges.\n\nIf no elements are selected, the entire object will be scanned for holes.", MessageType.Info);

			bool wholePath = PreferencesInternal.GetBool(PreferenceKeys.pbFillHoleSelectsEntirePath);

			EditorGUI.BeginChangeCheck();

			wholePath = EditorGUILayout.Toggle("Fill Entire Hole", wholePath);

			if(EditorGUI.EndChangeCheck())
				PreferencesInternal.SetBool(PreferenceKeys.pbFillHoleSelectsEntirePath, wholePath);

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Fill Hole"))
				EditorUtility.ShowNotification( DoAction().notification );
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuFillHole(MeshSelection.Top());
		}
	}
}



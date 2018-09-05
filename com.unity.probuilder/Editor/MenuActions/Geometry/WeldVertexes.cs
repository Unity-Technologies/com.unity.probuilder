using UnityEngine;
using UnityEditor;
using UnityEditor.ProBuilder.UI;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using EditorGUILayout = UnityEditor.EditorGUILayout;
using EditorStyles = UnityEditor.EditorStyles;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class WeldVertices : MenuAction
	{
		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Geometry; }
		}

		public override Texture2D icon
		{
			get { return IconUtility.GetIcon("Toolbar/Vert_Weld", IconSkin.Pro); }
		}

		public override TooltipContent tooltip
		{
			get { return _tooltip; }
		}

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Weld Vertices",
			@"Searches the current selection for vertices that are within the specified distance of on another and merges them into a single vertex.",
			keyCommandAlt, 'V'
		);

		public override bool enabled
		{
			get
			{
				return ProBuilderEditor.instance != null &&
					ProBuilderEditor.editLevel == EditLevel.Geometry &&
					ProBuilderEditor.componentMode == ComponentMode.Vertex &&
					MeshSelection.TopInternal().Any(x => x.selectedVertexCount > 1);
			}
		}

		public override bool hidden
		{
			get
			{
				return ProBuilderEditor.instance == null ||
					ProBuilderEditor.editLevel != EditLevel.Geometry ||
					ProBuilderEditor.componentMode != ComponentMode.Vertex;
			}
		}

		protected override MenuActionState optionsMenuState
		{
			get { return MenuActionState.VisibleAndEnabled; }
		}

		static readonly GUIContent gc_weldDistance = new GUIContent("Weld Distance", "The maximum distance between two vertices in order to be welded together.");
		const float k_MinWeldDistance = .00001f;

		protected override void OnSettingsGUI()
		{
			GUILayout.Label("Weld Settings", EditorStyles.boldLabel);

			EditorGUI.BeginChangeCheck();

			float weldDistance = PreferencesInternal.GetFloat(PreferenceKeys.pbWeldDistance);

			if (weldDistance <= k_MinWeldDistance)
				weldDistance = k_MinWeldDistance;

			weldDistance = EditorGUILayout.FloatField(gc_weldDistance, weldDistance);

			if (EditorGUI.EndChangeCheck())
			{
				if (weldDistance < k_MinWeldDistance)
					weldDistance = k_MinWeldDistance;
				PreferencesInternal.SetFloat(PreferenceKeys.pbWeldDistance, weldDistance);
			}

			GUILayout.FlexibleSpace();

			if (GUILayout.Button("Weld Vertices"))
				DoAction();
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuWeldVertices(MeshSelection.TopInternal());
		}
	}
}

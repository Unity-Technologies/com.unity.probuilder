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
	sealed class CollapseVertices : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Vert_Collapse", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Collapse Vertices",
			@"Merge all selected vertices into a single vertex, centered at the average of all selected points.",
			keyCommandAlt, 'C'
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

		protected override void OnSettingsGUI()
		{
			GUILayout.Label("Collapse Vertices Settings", EditorStyles.boldLabel);

			EditorGUILayout.HelpBox("Collapse To First setting decides where the collapsed vertex will be placed.\n\nIf True, the new vertex will be placed at the position of the first selected vertex.  If false, the new vertex is placed at the average position of all selected vertices.", MessageType.Info);

			bool collapseToFirst = PreferencesInternal.GetBool(PreferenceKeys.pbCollapseVertexToFirst);

			EditorGUI.BeginChangeCheck();

			collapseToFirst = EditorGUILayout.Toggle("Collapse To First", collapseToFirst);

			if(EditorGUI.EndChangeCheck())
				PreferencesInternal.SetBool(PreferenceKeys.pbCollapseVertexToFirst, collapseToFirst);

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Collapse Vertices"))
				DoAction();
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuCollapseVertices(MeshSelection.TopInternal());
		}
	}
}


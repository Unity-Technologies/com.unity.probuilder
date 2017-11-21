using UnityEngine;
using UnityEditor;
using ProBuilder.Interface;
using System.Linq;
using ProBuilder.Core;
using ProBuilder.EditorCore;

namespace ProBuilder.Actions
{
	class CollapseVertices : pb_MenuAction
	{
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Vert_Collapse", IconSkin.Pro); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return true; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Collapse Vertices",
			@"Merge all selected vertices into a single vertex, centered at the average of all selected points.",
			CMD_ALT, 'C'
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					pb_Editor.instance.editLevel == EditLevel.Geometry &&
					pb_Editor.instance.selectionMode == SelectMode.Vertex &&
					selection != null &&
					selection.Length > 0 &&
					selection.Any(x => x.SelectedTriangleCount > 1);
		}

		public override bool IsHidden()
		{
			return 	pb_Editor.instance == null ||
					pb_Editor.instance.editLevel != EditLevel.Geometry ||
					pb_Editor.instance.selectionMode != SelectMode.Vertex;

		}

		public override MenuActionState AltState()
		{
			return MenuActionState.VisibleAndEnabled;
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Collapse Vertices Settings", EditorStyles.boldLabel);

			EditorGUILayout.HelpBox("Collapse To First setting decides where the collapsed vertex will be placed.\n\nIf True, the new vertex will be placed at the position of the first selected vertex.  If false, the new vertex is placed at the average position of all selected vertices.", MessageType.Info);

			bool collapseToFirst = pb_PreferencesInternal.GetBool(pb_Constant.pbCollapseVertexToFirst);

			EditorGUI.BeginChangeCheck();

			collapseToFirst = EditorGUILayout.Toggle("Collapse To First", collapseToFirst);

			if(EditorGUI.EndChangeCheck())
				pb_PreferencesInternal.SetBool(pb_Constant.pbCollapseVertexToFirst, collapseToFirst);

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Collapse Vertices"))
				DoAction();
		}

		public override pb_ActionResult DoAction()
		{
			return pb_MenuCommands.MenuCollapseVertices(selection);
		}
	}
}


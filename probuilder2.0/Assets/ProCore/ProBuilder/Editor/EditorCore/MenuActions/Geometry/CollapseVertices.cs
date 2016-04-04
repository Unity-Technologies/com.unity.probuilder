using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;
using System.Linq;

namespace ProBuilder2.Actions
{
	public class CollapseVertices : pb_MenuAction
	{
		public override pb_IconGroup group { get { return pb_IconGroup.Geometry; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Vert_Collapse"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

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

		public override bool SettingsEnabled()
		{
			return true;
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Collapse Vertices Settings", EditorStyles.boldLabel);

			EditorGUILayout.HelpBox("Collapse To First setting decides where the collapsed vertex will be placed.\n\nIf True, the new vertex will be placed at the position of the first selected vertex.  If false, the new vertex is placed at the average position of all selected vertices.", MessageType.Info);
			
			bool collapseToFirst = pb_Preferences_Internal.GetBool(pb_Constant.pbCollapseVertexToFirst);

			EditorGUI.BeginChangeCheck();

			collapseToFirst = EditorGUILayout.Toggle("Collapse To First", collapseToFirst);

			if(EditorGUI.EndChangeCheck())
				EditorPrefs.SetBool(pb_Constant.pbCollapseVertexToFirst, collapseToFirst);

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Collapse Vertices"))
				DoAction();
		}

		public override bool IsHidden()
		{
			return 	pb_Editor.instance == null ||
					pb_Editor.instance.editLevel != EditLevel.Geometry ||
					pb_Editor.instance.selectionMode != SelectMode.Vertex;
					
		}

		public override pb_ActionResult DoAction()
		{
			return pb_Menu_Commands.MenuCollapseVertices(selection);
		}
	}
}


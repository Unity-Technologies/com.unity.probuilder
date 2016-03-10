using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;
using System.Linq;

namespace ProBuilder2.Actions
{
	public class ExtrudeFaces : pb_MenuAction
	{
		public override pb_IconGroup group { get { return pb_IconGroup.Geometry; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Face_Extrude"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Extrude Faces",
			@"Extrude selected faces, either as a group or individually.",
			CMD_SUPER, 'E'
		);

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					selection != null &&
					selection.Length > 0 &&
					selection.Sum(x => x.SelectedFaceCount) > 0;
		}

		public override bool SettingsEnabled()
		{
			return true;
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Extrude Settings", EditorStyles.boldLabel);
			
			float extrudeAmount = EditorPrefs.HasKey(pb_Constant.pbExtrudeDistance) ? EditorPrefs.GetFloat(pb_Constant.pbExtrudeDistance) : .5f;
			bool extrudeAsGroup = pb_Preferences_Internal.GetBool(pb_Constant.pbExtrudeAsGroup);

			EditorGUI.BeginChangeCheck();

			extrudeAsGroup = EditorGUILayout.Toggle("As Group", extrudeAsGroup);
			extrudeAmount = EditorGUILayout.FloatField("Distance", extrudeAmount);

			if(EditorGUI.EndChangeCheck())
			{
				EditorPrefs.SetFloat(pb_Constant.pbExtrudeDistance, extrudeAmount);
				EditorPrefs.SetBool(pb_Constant.pbExtrudeAsGroup, extrudeAsGroup);
			}
		}

		public override pb_ActionResult DoAction()
		{
			return pb_Menu_Commands.MenuExtrude(selection);
		}
	}
}


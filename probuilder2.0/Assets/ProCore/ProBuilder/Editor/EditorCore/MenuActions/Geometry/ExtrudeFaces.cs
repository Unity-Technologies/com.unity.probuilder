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
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Face_Extrude"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool hasFileMenuEntry { get { return false; } }
		[SerializeField] Texture2D[] icons = null;

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Extrude Faces",
			"Extrude selected faces, either as a group or individually.\n\nAlt + Click this button to show additional Extrude options.",
			CMD_SUPER, 'E'
		);

		public ExtrudeFaces()
		{
			icons = new Texture2D[3];

			icons[(int)ExtrudeMethod.IndividualFaces] = pb_IconUtility.GetIcon("Toolbar/ExtrudeFace_Individual");
			icons[(int)ExtrudeMethod.VertexNormal] = pb_IconUtility.GetIcon("Toolbar/ExtrudeFace_VertexNormals");
			icons[(int)ExtrudeMethod.FaceNormal] = pb_IconUtility.GetIcon("Toolbar/ExtrudeFace_FaceNormals");
		}

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null &&
					selection != null &&
					selection.Length > 0 &&
					selection.Sum(x => x.SelectedFaceCount) > 0;
		}

		public override bool IsHidden()
		{
			return 	editLevel != EditLevel.Geometry ||
					(pb_PreferencesInternal.GetBool(pb_Constant.pbElementSelectIsHamFisted) && selectionMode != SelectMode.Face);
		}

		public override MenuActionState AltState()
		{
			return MenuActionState.VisibleAndEnabled;
		}

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Extrude Settings", EditorStyles.boldLabel);

			EditorGUILayout.HelpBox("Extrude Amount determines how far a face will be moved along it's normal when extruding.  This value can be negative.\n\nYou may also choose to Extrude by Face Normal, Vertex Normal, or as Individual Faces.", MessageType.Info);

			float extrudeAmount = pb_PreferencesInternal.HasKey(pb_Constant.pbExtrudeDistance) ? pb_PreferencesInternal.GetFloat(pb_Constant.pbExtrudeDistance) : .5f;
			ExtrudeMethod method = pb_PreferencesInternal.GetEnum<ExtrudeMethod>(pb_Constant.pbExtrudeMethod);

			GUILayout.BeginHorizontal();
				GUILayout.FlexibleSpace();
					GUILayout.Label(icons[(int) method]);
				GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			EditorGUI.BeginChangeCheck();

			method = (ExtrudeMethod) EditorGUILayout.EnumPopup("Extrude By", method);
			extrudeAmount = EditorGUILayout.FloatField("Distance", extrudeAmount);

			if(EditorGUI.EndChangeCheck())
			{
				pb_PreferencesInternal.SetFloat(pb_Constant.pbExtrudeDistance, extrudeAmount);
				pb_PreferencesInternal.SetInt(pb_Constant.pbExtrudeMethod, (int) method);
			}

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Extrude Faces"))
				DoAction();
		}

		public override pb_ActionResult DoAction()
		{
			return pb_Menu_Commands.MenuExtrude(selection, false);
		}
	}
}


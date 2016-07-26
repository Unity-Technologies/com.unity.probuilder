using UnityEngine;
using UnityEditor;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;
using ProBuilder2.Interface;
using System.Linq;

namespace ProBuilder2.Actions
{
	public class DetachFaces : pb_MenuAction
	{
		public override pb_IconGroup group { get { return pb_IconGroup.Geometry; } }
		public override Texture2D icon { get { return pb_IconUtility.GetIcon("Toolbar/Face_Detach"); } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return true; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Detach Faces",
			"Creates a new object (or submesh) from the selected faces."
		);

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
					(pb_Preferences_Internal.GetBool(pb_Constant.pbElementSelectIsHamFisted) && selectionMode != SelectMode.Face);
		}

		public override MenuActionState AltState()
		{
			return MenuActionState.VisibleAndEnabled;
		}

		enum DetachSetting
		{
			GameObject,
			Submesh
		};

		public override void OnSettingsGUI()
		{
			GUILayout.Label("Detach Face Settings", EditorStyles.boldLabel);

			EditorGUILayout.HelpBox("Detach Faces can separate the selection into either a new GameObject or a submesh.", MessageType.Info);
			
			bool detachToNewObject = pb_Preferences_Internal.GetBool(pb_Constant.pbDetachToNewObject);
			DetachSetting setting = detachToNewObject ? DetachSetting.GameObject : DetachSetting.Submesh;

			EditorGUI.BeginChangeCheck();

			setting = (DetachSetting) EditorGUILayout.EnumPopup("Detach To", setting);

			if(EditorGUI.EndChangeCheck())
				EditorPrefs.SetBool(pb_Constant.pbDetachToNewObject, setting == DetachSetting.GameObject);

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Detach Selection"))
				pb_EditorUtility.ShowNotification( DoAction().notification );
		}


		public override pb_ActionResult DoAction()
		{
			return pb_Menu_Commands.MenuDetachFaces(selection);
		}
	}
}


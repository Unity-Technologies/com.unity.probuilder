using UnityEngine;
using UnityEditor;
using System.Linq;
using ProBuilder.Core;
using UnityEditor.ProBuilder;
using EditorUtility = UnityEditor.ProBuilder.EditorUtility;

namespace ProBuilder.Actions
{
	class DetachFaces : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Face_Detach", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }
		public override bool isProOnly { get { return true; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Detach Faces",
			"Creates a new object (or submesh) from the selected faces."
		);

		public override bool IsEnabled()
		{
			return 	ProBuilderEditor.instance != null &&
					selection != null &&
					selection.Length > 0 &&
					selection.Sum(x => x.SelectedFaceCount) > 0;
		}

		public override bool IsHidden()
		{
			return 	editLevel != EditLevel.Geometry ||
					(PreferencesInternal.GetBool(pb_Constant.pbElementSelectIsHamFisted) && selectionMode != SelectMode.Face);
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

			bool detachToNewObject = PreferencesInternal.GetBool(pb_Constant.pbDetachToNewObject);
			DetachSetting setting = detachToNewObject ? DetachSetting.GameObject : DetachSetting.Submesh;

			EditorGUI.BeginChangeCheck();

			setting = (DetachSetting) EditorGUILayout.EnumPopup("Detach To", setting);

			if(EditorGUI.EndChangeCheck())
				PreferencesInternal.SetBool(pb_Constant.pbDetachToNewObject, setting == DetachSetting.GameObject);

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Detach Selection"))
				EditorUtility.ShowNotification( DoAction().notification );
		}

		public override pb_ActionResult DoAction()
		{
			return MenuCommands.MenuDetachFaces(selection);
		}
	}
}


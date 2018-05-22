using UnityEngine;
using UnityEditor;
using System.Linq;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using EditorUtility = UnityEditor.ProBuilder.EditorUtility;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class DetachFaces : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Face_Detach", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return _tooltip; } }

		static readonly TooltipContent _tooltip = new TooltipContent
		(
			"Detach Faces",
			"Creates a new object (or submesh) from the selected faces."
		);

		public override bool IsEnabled()
		{
			return ProBuilderEditor.instance != null &&
				MeshSelection.Top().Sum(x => x.selectedFaceCount) > 0;
		}

		public override bool IsHidden()
		{
			return 	editLevel != EditLevel.Geometry ||
					(PreferencesInternal.GetBool(PreferenceKeys.pbElementSelectIsHamFisted) && selectionMode != SelectMode.Face);
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

			bool detachToNewObject = PreferencesInternal.GetBool(PreferenceKeys.pbDetachToNewObject);
			DetachSetting setting = detachToNewObject ? DetachSetting.GameObject : DetachSetting.Submesh;

			EditorGUI.BeginChangeCheck();

			setting = (DetachSetting) EditorGUILayout.EnumPopup("Detach To", setting);

			if(EditorGUI.EndChangeCheck())
				PreferencesInternal.SetBool(PreferenceKeys.pbDetachToNewObject, setting == DetachSetting.GameObject);

			GUILayout.FlexibleSpace();

			if(GUILayout.Button("Detach Selection"))
				EditorUtility.ShowNotification( DoAction().notification );
		}

		public override ActionResult DoAction()
		{
			return MenuCommands.MenuDetachFaces(MeshSelection.Top());
		}
	}
}


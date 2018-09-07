using UnityEngine;
using System.Linq;
using UnityEngine.ProBuilder;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class DetachFaces : MenuAction
	{
		public override ToolbarGroup group { get { return ToolbarGroup.Geometry; } }
		public override Texture2D icon { get { return IconUtility.GetIcon("Toolbar/Face_Detach", IconSkin.Pro); } }
		public override TooltipContent tooltip { get { return m_Tooltip; } }

		static readonly TooltipContent m_Tooltip = new TooltipContent
		(
			"Detach Faces",
			"Creates a new object (or submesh) from the selected faces."
		);

		public override bool enabled
		{
			get
			{
				return ProBuilderEditor.instance != null &&
					MeshSelection.TopInternal().Sum(x => x.selectedFaceCount) > 0;
			}
		}

		public override bool hidden
		{
			get
			{
				return editLevel != EditLevel.Geometry || componentMode != ComponentMode.Face;
			}
		}

		protected override MenuActionState optionsMenuState
		{
			get { return MenuActionState.VisibleAndEnabled; }
		}

		enum DetachSetting
		{
			GameObject,
			Submesh
		};

		protected override void OnSettingsGUI()
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
			return MenuCommands.MenuDetachFaces(MeshSelection.TopInternal());
		}
	}
}


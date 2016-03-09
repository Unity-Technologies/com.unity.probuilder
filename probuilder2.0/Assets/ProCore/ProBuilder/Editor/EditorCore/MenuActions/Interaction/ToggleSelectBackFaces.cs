using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Interface;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	[System.Serializable]
	public class ToggleSelectBackFaces : pb_MenuAction
	{
		public override pb_IconGroup group { get { return pb_IconGroup.Selection; } }
		public override Texture2D icon
		{
			get
			{
				return pb_Preferences_Internal.GetBool(pb_Constant.pbEnableBackfaceSelection) ? icons[1] : icons[0];
			}
		}
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Set Hiden Element Selection",
@"Setting Hidden Element Selection to <b>On</b> allows you to select faces that are either obscured by geometry or facing away from the scene camera (backfaces).

The default value is <b>On</b>.
"
		);

		private Texture2D[] icons;

		public ToggleSelectBackFaces()
		{
			icons = new Texture2D[]
			{
				pb_IconUtility.GetIcon("Selection_SelectHidden-Off"),
				pb_IconUtility.GetIcon("Selection_SelectHidden-On")
			};
		}

		public override pb_ActionResult DoAction()
		{
			bool isEnabled = pb_Preferences_Internal.GetBool(pb_Constant.pbEnableBackfaceSelection);
			pb_Editor.instance.SetSelectHiddenEnabled( !isEnabled );

			return new pb_ActionResult(Status.Success, "Set Hidden Element Selection\n" + (isEnabled ? "Off" : "On") );
		}

		public override bool IsEnabled()
		{
			return pb_Editor.instance != null;
		}
	}
}

using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder.Core;
using ProBuilder.EditorCore;
using ProBuilder.Interface;

namespace ProBuilder.Actions
{
	class ToggleSelectBackFaces : pb_MenuAction
	{
		bool isEnabled { get { return pb_PreferencesInternal.GetBool(pb_Constant.pbEnableBackfaceSelection); } }

		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Selection; } }
		public override Texture2D icon { get { return isEnabled ? icons[1] : icons[0]; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override int toolbarPriority { get { return 0; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Set Hidden Element Selection",
@"Setting Hidden Element Selection to <b>On</b> allows you to select faces that are either obscured by geometry or facing away from the scene camera (backfaces).

The default value is <b>On</b>.
");

		public override string menuTitle { get { return isEnabled ? "Select Hidden: On" : "Select Hidden: Off"; } }

		Texture2D[] icons;

		public ToggleSelectBackFaces()
		{
			icons = new Texture2D[]
			{
				pb_IconUtility.GetIcon("Toolbar/Selection_SelectHidden-Off", IconSkin.Pro),
				pb_IconUtility.GetIcon("Toolbar/Selection_SelectHidden-On", IconSkin.Pro)
			};
		}

		public override pb_ActionResult DoAction()
		{
			bool backFaceEnabled = pb_PreferencesInternal.GetBool(pb_Constant.pbEnableBackfaceSelection);
			pb_PreferencesInternal.SetBool(pb_Constant.pbEnableBackfaceSelection, !backFaceEnabled);
			pb_Editor.instance.LoadPrefs();
			return new pb_ActionResult(Status.Success, "Set Hidden Element Selection\n" + (!backFaceEnabled ? "On" : "Off") );
		}

		public override bool IsEnabled()
		{
			return pb_Editor.instance != null;
		}
	}
}

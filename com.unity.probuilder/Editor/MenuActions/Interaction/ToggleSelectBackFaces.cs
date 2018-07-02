using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class ToggleSelectBackFaces : MenuAction
	{
		bool isEnabled { get { return PreferencesInternal.GetBool(PreferenceKeys.pbEnableBackfaceSelection); } }

		public override ToolbarGroup group { get { return ToolbarGroup.Selection; } }
		public override Texture2D icon { get { return isEnabled ? icons[1] : icons[0]; } }
		public override TooltipContent tooltip { get { return _tooltip; } }
		public override int toolbarPriority { get { return 0; } }

		static readonly TooltipContent _tooltip = new TooltipContent
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
				IconUtility.GetIcon("Toolbar/Selection_SelectHidden-Off", IconSkin.Pro),
				IconUtility.GetIcon("Toolbar/Selection_SelectHidden-On", IconSkin.Pro)
			};
		}

		public override ActionResult DoAction()
		{
			bool backFaceEnabled = PreferencesInternal.GetBool(PreferenceKeys.pbEnableBackfaceSelection);
			PreferencesInternal.SetBool(PreferenceKeys.pbEnableBackfaceSelection, !backFaceEnabled);
			ProBuilderEditor.instance.LoadPrefs();
			return new ActionResult(ActionResult.Status.Success, "Set Hidden Element Selection\n" + (!backFaceEnabled ? "On" : "Off") );
		}

		public override bool enabled
		{
			get { return ProBuilderEditor.instance != null; }
		}
	}
}

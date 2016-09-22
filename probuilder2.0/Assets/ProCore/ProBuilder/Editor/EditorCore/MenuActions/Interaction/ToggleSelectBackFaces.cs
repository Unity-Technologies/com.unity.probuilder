using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Interface;
using ProBuilder2.Common;
using ProBuilder2.EditorCommon;

namespace ProBuilder2.Actions
{
	public class ToggleSelectBackFaces : pb_MenuAction
	{
		bool isEnabled { get { return pb_Preferences_Internal.GetBool(pb_Constant.pbEnableBackfaceSelection); } }

		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Selection; } }
		public override Texture2D icon { get { return null; } }
		public override pb_TooltipContent tooltip { get { return _tooltip; } }
		public override int toolbarPriority { get { return 1; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Select Back Faces",
@"Setting Backface Element Selection to <b>On</b> allows you to select faces that facing away from the scene camera.

The default value is <b>Off</b>.
");

		public override string menuTitle { get { return isEnabled ? "Select Backfaces: On" : "Select Backfaces: Off"; } }

		// private Texture2D[] icons;

		// public ToggleSelectBackFaces()
		// {
		// 	icons = new Texture2D[]
		// 	{
		// 		pb_IconUtility.GetIcon("Toolbar/Selection_SelectHidden-Off"),
		// 		pb_IconUtility.GetIcon("Toolbar/Selection_SelectHidden-On")
		// 	};
		// }

		public override pb_ActionResult DoAction()
		{
			bool isEnabled = pb_Preferences_Internal.GetBool(pb_Constant.pbEnableBackfaceSelection);
			pb_Editor.instance.SetSelectBackfacesEnabled( !isEnabled );

			return new pb_ActionResult(Status.Success, "Set Backface Selection\n" + (isEnabled ? "Off" : "On") );
		}

		public override bool IsEnabled()
		{
			return 	pb_Editor.instance != null;
		}

		public override bool IsHidden()
		{
			return 	pb_Editor.instance == null ||
					editLevel != EditLevel.Geometry ||
					selectionMode != SelectMode.Face;
		}
	}
}

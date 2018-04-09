using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder.Core;
using ProBuilder.EditorCore;
using ProBuilder.Interface;

namespace ProBuilder.Actions
{
	class ToggleHandleAlignment : pb_MenuAction
	{
		[SerializeField] int count = 0;
		[SerializeField] Texture2D[] icons = null;
		private int handleAlignment { get { return pb_Editor.instance == null ? 0 : (int)pb_Editor.instance.handleAlignment; } }
		public override pb_ToolbarGroup group { get { return pb_ToolbarGroup.Selection; } }
		public override Texture2D icon { get { return icons[handleAlignment]; } }
		public override int toolbarPriority { get { return 0; } }

		public override pb_TooltipContent tooltip
		{
			get
			{
				if(handleAlignment == (int) HandleAlignment.World)
					return _tooltip_world;
				if(handleAlignment == (int) HandleAlignment.Local)
					return _tooltip_local;
				else
					return _tooltip_plane;
			}
		}

		static readonly pb_TooltipContent _tooltip_world = new pb_TooltipContent(
			"Set Handle Alignment",
			"Toggles the coordinate space that the transform gizmo is rendered in.\n\nCurrent: World (handle is always the same)",
			'P');

		static readonly pb_TooltipContent _tooltip_local = new pb_TooltipContent(
			"Set Handle Alignment",
			"Toggles the coordinate space that the transform gizmo is rendered in.\n\nCurrent: Local (handle is relative to the GameObject selection)",
			'P');

		static readonly pb_TooltipContent _tooltip_plane = new pb_TooltipContent(
			"Set Handle Alignment",
			"Toggles the coordinate space that the transform gizmo is rendered in.\n\nCurrent: Plane (handle is relative to the element selection)",
			'P');

		public override string menuTitle { get { return "Handle: " + ((HandleAlignment)handleAlignment).ToString(); } }

		public ToggleHandleAlignment()
		{
			icons = new Texture2D[]
			{
				pb_IconUtility.GetIcon("Toolbar/HandleAlign_World", IconSkin.Pro),
				pb_IconUtility.GetIcon("Toolbar/HandleAlign_Local", IconSkin.Pro),
				pb_IconUtility.GetIcon("Toolbar/HandleAlign_Plane", IconSkin.Pro),
			};

			this.count = icons.Length;
		}

		public override pb_ActionResult DoAction()
		{
			int current = handleAlignment + 1;

			if(current >= count)
				current = 0;

			pb_Editor.instance.SetHandleAlignment( (HandleAlignment)current );
			pb_Editor.instance.LoadPrefs();
			return new pb_ActionResult(Status.Success, "Set Handle Alignment\n" + ((HandleAlignment)current).ToString());
		}

		public override bool IsEnabled()
		{
			return pb_Editor.instance != null;
		}
	}
}

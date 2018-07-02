using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class ToggleHandleAlignment : MenuAction
	{
		[SerializeField] int count = 0;
		[SerializeField] Texture2D[] icons = null;
		int handleAlignment { get { return ProBuilderEditor.instance == null ? 0 : (int)ProBuilderEditor.instance.handleAlignment; } }
		public override ToolbarGroup group { get { return ToolbarGroup.Selection; } }
		public override Texture2D icon { get { return icons[handleAlignment]; } }
		public override int toolbarPriority { get { return 0; } }

		public override TooltipContent tooltip
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

		static readonly TooltipContent _tooltip_world = new TooltipContent(
			"Set Handle Alignment",
			"Toggles the coordinate space that the transform gizmo is rendered in.\n\nCurrent: World (handle is always the same)",
			'P');

		static readonly TooltipContent _tooltip_local = new TooltipContent(
			"Set Handle Alignment",
			"Toggles the coordinate space that the transform gizmo is rendered in.\n\nCurrent: Local (handle is relative to the GameObject selection)",
			'P');

		static readonly TooltipContent _tooltip_plane = new TooltipContent(
			"Set Handle Alignment",
			"Toggles the coordinate space that the transform gizmo is rendered in.\n\nCurrent: Plane (handle is relative to the element selection)",
			'P');

		public override string menuTitle { get { return "Handle: " + ((HandleAlignment)handleAlignment).ToString(); } }

		public ToggleHandleAlignment()
		{
			icons = new Texture2D[]
			{
				IconUtility.GetIcon("Toolbar/HandleAlign_World", IconSkin.Pro),
				IconUtility.GetIcon("Toolbar/HandleAlign_Local", IconSkin.Pro),
				IconUtility.GetIcon("Toolbar/HandleAlign_Plane", IconSkin.Pro),
			};

			this.count = icons.Length;
		}

		public override ActionResult DoAction()
		{
			int current = handleAlignment + 1;

			if(current >= count)
				current = 0;

			ProBuilderEditor.instance.SetHandleAlignment( (HandleAlignment)current );
			ProBuilderEditor.instance.LoadPrefs();
			return new ActionResult(ActionResult.Status.Success, "Set Handle Alignment\n" + ((HandleAlignment)current).ToString());
		}

		public override bool enabled
		{
			get { return ProBuilderEditor.instance != null; }
		}
	}
}

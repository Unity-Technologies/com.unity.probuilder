using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEngine.ProBuilder;
using UnityEditor.ProBuilder;
using UnityEditor.ProBuilder.UI;

namespace UnityEditor.ProBuilder.Actions
{
	sealed class ToggleHandleOrientation : MenuAction
	{
		Texture2D[] m_Icons;

		HandleOrientation handleOrientation
		{
			get { return ProBuilderEditor.instance == null ? HandleOrientation.World : ProBuilderEditor.handleOrientation; }
			set { ProBuilderEditor.handleOrientation = value; }
		}

		public override ToolbarGroup group
		{
			get { return ToolbarGroup.Selection; }
		}

		public override Texture2D icon
		{
			get { return m_Icons[(int)handleOrientation]; }
		}

		public override int toolbarPriority
		{
			get { return 0; }
		}

		public override TooltipContent tooltip
		{
			get
			{
				if (handleOrientation == HandleOrientation.World)
					return s_TooltipWorld;
				if (handleOrientation == HandleOrientation.Local)
					return s_TooltipLocal;
				else
					return s_TooltipPlane;
			}
		}

		static readonly TooltipContent s_TooltipWorld = new TooltipContent(
			"Set Handle Alignment",
			"Toggles the coordinate space that the transform gizmo is rendered in.\n\nCurrent: World (handle is always the same)",
			'P');

		static readonly TooltipContent s_TooltipLocal = new TooltipContent(
			"Set Handle Alignment",
			"Toggles the coordinate space that the transform gizmo is rendered in.\n\nCurrent: Local (handle is relative to the GameObject selection)",
			'P');

		static readonly TooltipContent s_TooltipPlane = new TooltipContent(
			"Set Handle Alignment",
			"Toggles the coordinate space that the transform gizmo is rendered in.\n\nCurrent: Plane (handle is relative to the element selection)",
			'P');

		public override string menuTitle
		{
			get { return "Handle: " + ((HandleOrientation)handleOrientation).ToString(); }
		}

		public override SelectMode validSelectModes
		{
			get { return SelectMode.Vertex | SelectMode.Edge | SelectMode.Face; }
		}

		public override bool hidden
		{
			get { return false; }
		}

		public ToggleHandleOrientation()
		{
			m_Icons = new Texture2D[]
			{
				IconUtility.GetIcon("Toolbar/HandleAlign_World", IconSkin.Pro),
				IconUtility.GetIcon("Toolbar/HandleAlign_Local", IconSkin.Pro),
				IconUtility.GetIcon("Toolbar/HandleAlign_Plane", IconSkin.Pro),
			};
		}

		public override ActionResult DoAction()
		{
			int current = (int)handleOrientation + 1;

			if (current >= System.Enum.GetValues(typeof(HandleOrientation)).Length)
				current = 0;

			handleOrientation = (HandleOrientation)current;

			return new ActionResult(ActionResult.Status.Success, "Set Handle Alignment\n" + ((HandleOrientation)current).ToString());
		}

		public override bool enabled
		{
			get { return ProBuilderEditor.instance != null; }
		}
	}
}

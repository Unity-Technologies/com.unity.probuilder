using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Interface;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	[System.Serializable]
	public class ToggleHandleAlignment : pb_MenuAction
	{
		[SerializeField] int count = 0;
		[SerializeField] Texture2D[] icons = null;
		private int handleAlignment
		{ 
			get
			{
				return pb_Editor.instance == null ? 0 : (int)pb_Editor.instance.handleAlignment;
			}
		}

		public override pb_IconGroup group { get { return pb_IconGroup.Selection; } }
		public override Texture2D icon
		{
			get
			{
				return icons[handleAlignment];
			}
		}
		public override pb_TooltipContent tooltip { get { return _tooltip; } }

		static readonly pb_TooltipContent _tooltip = new pb_TooltipContent
		(
			"Set Handle Alignment",
@"Toggles the coordinate space that the transform gizmo is rendered in.

<b>Shortcut</b>: <i>P</i>"
		);

		public ToggleHandleAlignment()
		{
			icons = new Texture2D[]
			{
				pb_IconUtility.GetIcon("HandleAlign_World"),
				pb_IconUtility.GetIcon("HandleAlign_Local"),
				pb_IconUtility.GetIcon("HandleAlign_Plane"),
			};

			this.count = icons.Length;
		}

		public override pb_ActionResult DoAction()
		{
			int current = handleAlignment + 1;

			if(current >= count)
				current = 0;

			pb_Editor.instance.SetHandleAlignment( (HandleAlignment)current );

			return new pb_ActionResult(Status.Success, "Set Handle Alignment\n" + ((HandleAlignment)current).ToString());
		}

		public override bool IsEnabled()
		{
			return pb_Editor.instance != null;
		}
	}
}

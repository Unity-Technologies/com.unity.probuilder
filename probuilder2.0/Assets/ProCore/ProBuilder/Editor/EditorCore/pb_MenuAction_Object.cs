using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Interface;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	Menu Action shell for activating an object level action.
	 */
	public class pb_MenuAction_Object : pb_MenuAction
	{
		public System.Func<pb_Object[], pb_ActionResult> action;
		public System.Func<pb_Object[], bool> enabledFunc;

		public pb_MenuAction_Object(Texture2D icon,
									pb_TooltipContent tooltip,
									System.Func<pb_Object[], pb_ActionResult> action,
									System.Func<pb_Object[], bool> enabledOverride = null)
		{
			this.icon = icon;
			this.tooltip = tooltip;
			this.action = action;
			this.enabledFunc = enabledOverride;
		}

		public override bool IsEnabled()
		{
			pb_Object[] selection = pbUtil.GetComponents<pb_Object>(Selection.transforms);

			return 	pb_Editor.instance != null &&
					(enabledFunc != null ?
						enabledFunc(selection) :
						(
							selection != null &&
							selection.Length > 0
						));
		}

		public override pb_ActionResult DoAction()
		{
			return action(pbUtil.GetComponents<pb_Object>(Selection.transforms));
		}
	}
}

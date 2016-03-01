using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Interface;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	A simple wrapper for a menu action that just stores a delegate for 
	 *	a parameter-less action.
	 */
	public class pb_MenuAction_Simple : pb_MenuAction
	{
		public System.Func<pb_Object[], pb_ActionResult> action;

		public pb_MenuAction_Simple(Texture2D icon, string tooltip, System.Func<pb_Object[], pb_ActionResult> action)
		{
			this.icon = icon;
			this.tooltip = tooltip;
			this.action = action;
		}

		public override pb_ActionResult DoAction()
		{
			return action(new pb_Object[] { null });
		}
	}
}

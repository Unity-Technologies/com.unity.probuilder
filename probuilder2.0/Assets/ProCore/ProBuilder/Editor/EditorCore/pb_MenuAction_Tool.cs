using UnityEngine;
using UnityEditor;
using System.Collections;
using ProBuilder2.Interface;
using ProBuilder2.Common;

namespace ProBuilder2.EditorCommon
{
	/**
	 *	Menu Action shell for opening a tool window.
	 */
	public class pb_MenuAction_Tool : pb_MenuAction
	{
		public System.Action menuOpenDelegate;

		public pb_MenuAction_Tool(Texture2D icon, string tooltip, System.Action menuOpenDelegate)
		{
			this.icon = icon;
			this.tooltip = tooltip;
			this.menuOpenDelegate = menuOpenDelegate;
		}

		public override bool IsEnabled()
		{
			return true;
		}

		public override pb_ActionResult DoAction()
		{
			menuOpenDelegate();

			return pb_ActionResult.Success;
		}
	}
}

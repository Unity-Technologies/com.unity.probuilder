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
	public class pb_MenuAction_Element : pb_MenuAction
	{
		public System.Func<pb_Object[], pb_ActionResult> action;
		public System.Func<pb_Object[], bool> additionalCheck;
		public bool enforceElementMode = false;
		public SelectMode selectionMode = SelectMode.Face;

		public pb_MenuAction_Element(	Texture2D icon,
									string tooltip,
									System.Func<pb_Object[], pb_ActionResult> action,
									SelectMode selectionMode,
									bool enforceElementMode = false,
									System.Func<pb_Object[], bool> additionalCheck = null)
		{
			this.icon = icon;
			this.tooltip = tooltip;
			this.action = action;
			this.enforceElementMode = enforceElementMode;
			this.additionalCheck = additionalCheck;
		}

		public override bool IsEnabled()
		{
			pb_Object[] selection = pbUtil.GetComponents<pb_Object>(Selection.transforms);

			return 	pb_Editor.instance != null &&
					selection != null &&
					selection.Length > 0 &&
					pb_Editor.instance.editLevel == EditLevel.Geometry &&
					(!enforceElementMode || pb_Editor.instance.selectionMode == selectionMode) &&
					(additionalCheck == null || additionalCheck(selection));
		}

		public override pb_ActionResult DoAction()
		{
			return action(pbUtil.GetComponents<pb_Object>(Selection.transforms));
		}


	}
}

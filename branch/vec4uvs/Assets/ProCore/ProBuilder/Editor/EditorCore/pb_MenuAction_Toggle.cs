// using UnityEngine;
// using UnityEditor;
// using System.Collections;
// using ProBuilder2.Interface;

// namespace ProBuilder2.EditorCommon
// {
// 	/**
// 	 *	A toggle button action.
// 	 */
// 	[System.Serializable]
// 	public class pb_MenuAction_Toggle : pb_MenuAction
// 	{
// 		[SerializeField] int count = 0;
// 		[SerializeField] int current = 0;
// 		Texture2D[] icons = null;
// 		System.Action<int> action = null;

// 		public pb_MenuAction_Toggle(Texture2D[] icons, pb_TooltipContent tooltip, System.Action<int> action)
// 		{
// 			this.count = icons.Length;
// 			this.current = 0;
// 			this.icon = icons[0];
// 			this.icons = icons;
// 			this.tooltip = tooltip;
// 			this.action = action;
// 		}

// 		public override pb_ActionResult DoAction()
// 		{
// 			current++;

// 			if(current >= count)
// 				current = 0;

// 			icon = icons[current];

// 			action(current);

// 			return new pb_ActionResult(Status.Success, tooltip);
// 		}

// 		// Is this action valid based on the current selection and context?
// 		public override bool IsEnabled()
// 		{
// 			return true;
// 		}
// 	}
// }

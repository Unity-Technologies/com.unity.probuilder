using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using ProBuilder2.Common;
using ProBuilder2.Interface;

namespace ProBuilder2.EditorCommon
{
	public class pb_EditorToolbar_Mockup : EditorWindow
	{
		const int TOOLTIP_OFFSET = 4;

		[MenuItem("Tools/ProBuilder Window")]
		static void Init()
		{
			EditorWindow.GetWindow<pb_EditorToolbar_Mockup>(false, "ProBuilder", true);
		}

		pb_TooltipWindow tooltipWindow = null;
		List<pb_MenuAction> actions;

		void OnEnable()
		{
			actions = pb_EditorToolbarLoader.GetActions();
			this.wantsMouseMove = true;
			this.minSize = actions[0].GetSize() + new Vector2(6, 6);

			EditorApplication.update += this.Update;
		}

		void Update()
		{
		}

		Vector2 scroll = Vector2.zero;

		private void ShowTooltip(Rect rect, pb_MenuAction action, Vector2 scrollOffset)
		{
			Vector2 size = EditorStyles.boldLabel.CalcSize( pb_GUI_Utility.TempGUIContent(action.tooltip) );
			size += new Vector2(8,8);

			Rect tooltipRect = new Rect((this.position.x + rect.x + rect.width + TOOLTIP_OFFSET) - scrollOffset.x,
										(this.position.y + rect.y + TOOLTIP_OFFSET) - scrollOffset.y,
										size.x,
										size.y);

			if(tooltipWindow == null)
			{
				tooltipWindow = ScriptableObject.CreateInstance<pb_TooltipWindow>();
				tooltipWindow.ShowAsDropDown(tooltipRect, new Vector2(size.x, size.y));	
				tooltipWindow.SetTooltip(action.tooltip);
			}
		}

		void OnGUI()
		{
			Event e = Event.current;

			int max = (int)this.position.width - 24;
			int rows = System.Math.Max(max / (int)actions[0].GetSize().x, 1);

			int i = 1;

			scroll = GUILayout.BeginScrollView(scroll, false, false, GUIStyle.none, GUIStyle.none, GUIStyle.none);
			bool tooltipShown = false;

			GUILayout.BeginHorizontal();

			foreach(pb_MenuAction action in actions)
			{
				action.DoButton();

				Rect buttonRect = GUILayoutUtility.GetLastRect();

				if( e.shift &&
					e.type != EventType.Layout &&
					buttonRect.Contains(e.mousePosition) )
				{
					tooltipShown = true;
					ShowTooltip(buttonRect, action, scroll);
				}

				if(++i >= rows)
				{
					i = 1;
					GUILayout.EndHorizontal();
					GUILayout.BeginHorizontal();
				}
			}

			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();

			GUILayout.EndScrollView();

			if((e.type == EventType.Repaint || e.type == EventType.MouseMove) && !tooltipShown && tooltipWindow != null)
			{
				tooltipWindow.Close();
				tooltipWindow = null;
			}

			if( (EditorWindow.mouseOverWindow == this && e.delta.sqrMagnitude > .001f) || e.isMouse )
				Repaint();
		}
	}
}
